using System.Net;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.Json.Serialization;
using MQTTnet.AspNetCore;
using IoTFlow.MQTT.Transformers;
using MQTTnet.Server;
using IoTFlow.MQTT.Interfaces.Services;
using IoTFlow.MQTT.Services.Services;
using IoTFlow.MQTT.Interfaces.Utilities;
using IoTFlow.MQTT.Utilities;
using IoTFlow.MQTT.Models.DTO.User;
using System.Text;
using System.Buffers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<IIotFlowApiService<LoginRequestDto, UserDto, RefreshRequestDto>, IotFlowApiService>();
builder.Services.AddSingleton<ITokenStore, TokenStore>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(1883, listenOptions =>
    {
        listenOptions.UseMqtt();
    });

    options.ListenAnyIP(5053);
});
builder.Services.AddCors(o =>
{
    o.AddPolicy("DevCorsPolicy", config =>
    {
        config.AllowAnyHeader().AllowAnyMethod().AllowCredentials();

        if (builder.Environment.IsDevelopment())
        {
            config.WithOrigins(builder.Configuration["Origins:Test"]!);
        }
        else
        {
            config.WithOrigins(builder.Configuration["ORIGIN_TEST"]!);
        }
    });
});

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(
        new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
}).AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMqttConnectionHandler();
builder.Services.AddConnections();

builder.Services.AddHostedMqttServer(options =>
{
    options.WithDefaultEndpoint();
    options.WithDefaultEndpointPort(1883);
    options.WithDefaultEndpointBoundIPAddress(IPAddress.Any);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseCors("DevCorsPolicy");
app.MapControllers();

app.UseMqttServer(server =>
{
    var apiService = app.Services.GetRequiredService<IIotFlowApiService<LoginRequestDto, UserDto, RefreshRequestDto>>();
    server.ValidatingConnectionAsync += async args =>
    {
        var tokenStore = app.Services.GetRequiredService<ITokenStore>();
        var username = args.UserName;
        var password = args.Password;
        Guid.TryParse(args.ClientId, out Guid deviceGuid);
        if (deviceGuid == Guid.Empty)
        {
            await server.DisconnectClientAsync(args.ClientId, new MqttServerClientDisconnectOptions());
        }
        var loginRequest = new LoginRequestDto
        {
            NameOrEmail = args.UserName,
            Password = args.Password
        };
        var loginResponse = await apiService.LoginUserAsync(loginRequest);
        if (loginResponse.Token == string.Empty)
        {
            await server.DisconnectClientAsync(args.ClientId, new MqttServerClientDisconnectOptions());
        }
        tokenStore.SetToken(args.ClientId, loginResponse.Token);
        var deviceResponse = await apiService.SetDeviceAlive(deviceGuid.ToString(), true);
        if (!deviceResponse)
        {
            await server.DisconnectClientAsync(args.ClientId, new MqttServerClientDisconnectOptions());
        }
    };
    server.ClientDisconnectedAsync += async args =>
    {
        Guid.TryParse(args.ClientId, out Guid deviceGuid);
        var tokenStore = app.Services.GetRequiredService<ITokenStore>();
        var deviceResponse = await apiService.SetDeviceAlive(deviceGuid.ToString(), false);
        tokenStore.RemoveToken(args.ClientId);
    };
    server.InterceptingPublishAsync += async args =>
    {
        var apiService = app.Services.GetRequiredService<IIotFlowApiService<LoginRequestDto, UserDto, RefreshRequestDto>>();
        if (args.ApplicationMessage.Topic.Contains("/method"))
        {
            string payload = args.ApplicationMessage.Payload.IsEmpty
                ? string.Empty
                : Encoding.UTF8.GetString(args.ApplicationMessage.Payload.ToArray());
            await apiService.SetDeviceMethodsAsync(args.ClientId, payload);
        }
    };
});

app.Run();