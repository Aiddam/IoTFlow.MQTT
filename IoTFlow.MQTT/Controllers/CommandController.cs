using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Protocol;
using IoTFlow.MQTT.Interfaces.Utilities;
using IoTFlow.MQTT.Models.DTO;
using IoTFlow.MQTT.Utilities;

namespace IoTFlow.MQTT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly MqttServer _mqttServer;
        private readonly ITokenStore _tokenStore;

        public CommandController(MqttServer mqttServer, ITokenStore tokenStore)
        {
            _mqttServer = mqttServer;
            _tokenStore = tokenStore;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendCommand([FromBody] CommandModelDto model)
        {
            if (!_tokenStore.TryGetToken(model.DeviceGuid, out string token) || string.IsNullOrEmpty(token))
            {
                return BadRequest("The device is not authorised or is missing.");
            }

            var topic = $"device/{model.DeviceGuid}/command";

            string commandUrl = CommandUrlBuilder.BuildCommandUrl(model.Command, model.Parameters, model.CorrelationId);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(commandUrl)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            var injectedMessage = new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "HttpCommandSender"
            };

            await _mqttServer.InjectApplicationMessage(injectedMessage);
            return Ok($"Command sent to the device {model.DeviceGuid} via the topic {topic}");
        }
    }
}
