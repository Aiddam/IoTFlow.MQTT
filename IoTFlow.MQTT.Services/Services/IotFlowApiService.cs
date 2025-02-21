using IoTFlow.MQTT.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using IoTFlow.MQTT.Interfaces.Utilities;
using IoTFlow.MQTT.Models.DTO.User;
using IoTFlow.MQTT.Models.DTO.Device;
using System.Net.Http.Json;

namespace IoTFlow.MQTT.Services.Services
{
    public class IotFlowApiService : IIotFlowApiService<LoginRequestDto, UserDto, RefreshRequestDto>
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenStore _tokenStore;
        private readonly string _baseUrl;
        public IotFlowApiService(HttpClient httpClient, IConfiguration configuration, ITokenStore tokenStore)
        {
            _httpClient = httpClient;
            _tokenStore = tokenStore;
            _baseUrl = configuration["ApiBaseUrl"] ?? throw new Exception("ApiBaseUrl not specified in the configuration");
        }

        public async Task<UserDto> LoginUserAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}api/jwt-auth/login", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDto>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public Task<UserDto> RefreshUserAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> IsDeviceExist(string deviceGuid, CancellationToken cancellationToken = default)
        {
            _tokenStore.TryGetToken(deviceGuid, out var token);
            if (token == string.Empty || token == null)
            {
                throw new Exception("Invalid Guid");
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{_baseUrl}api/devices/{deviceGuid}/exist", cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<bool>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task SetDeviceMethodsAsync(string deviceGuid, string methods, CancellationToken cancellationToken = default)
        {
            _tokenStore.TryGetToken(deviceGuid, out var token);
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Invalid Guid");
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            List<MethodDto> methodsList;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var container = JsonSerializer.Deserialize<MethodsContainerDto>(methods, options);
                if (container == null || container.Methods == null)
                {
                    throw new Exception("Неправильно сформовані методи");
                }
                methodsList = container.Methods;
            }
            catch (Exception ex)
            {
                throw new Exception("Неправильно сформовані методи", ex);
            }

            string jsonContent = JsonSerializer.Serialize(methodsList);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}api/devices/{deviceGuid}/add-methods", content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> SetDeviceAlive(string deviceGuid, bool isAlive, CancellationToken cancellationToken = default)
        {
            _tokenStore.TryGetToken(deviceGuid, out var token);
            if (token == string.Empty || token == null)
            {
                throw new Exception("Invalid Guid");
            }
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var DeviceAliveRequest = new DeviceAliveRequest() { IsAlive = isAlive };
            string jsonContent = JsonSerializer.Serialize(DeviceAliveRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PutAsync($"{_baseUrl}api/devices/{deviceGuid}/set-device-alive", content, cancellationToken);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }
}
