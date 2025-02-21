using Microsoft.AspNetCore.Mvc;
using MQTTnet;
using IotFlow.Controllers;
using MQTTnet.Server;
using MQTTnet.Protocol;

namespace IoTFlow.MQTT.Controllers
{
    public class MqttController : BaseController
    {
        private readonly MqttServer _mqttServer;

        // Інжекція сервісу MQTT-сервера
        public MqttController(MqttServer mqttServer)
        {
            _mqttServer = mqttServer;
        }
        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _mqttServer.GetClientsAsync();
            return Ok(clients);
        }

        // HTTP POST endpoint для публікації повідомлення в MQTT
        // Викликається, наприклад, через https://example.com/api/mqtt/publish
        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage([FromBody] PublishMessageModel model)
        {
            // Побудова MQTT повідомлення
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(model.Topic)
                .WithPayload(model.Payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)    // QoS 2 (точно один раз)
                .WithRetainFlag(false)
                .Build();

            // Створюємо об'єкт InjectedMqttApplicationMessage через конструктор
            var injectedMessage = new InjectedMqttApplicationMessage(message)
            {
                SenderClientId = "HttpPublisher"
            };

            // Публікація повідомлення через метод InjectApplicationMessage
            await _mqttServer.InjectApplicationMessage(injectedMessage);

            return Ok("Message published via MQTT");
        }
    }
    public class PublishMessageModel
    {
        public string Topic { get; set; }
        public string Payload { get; set; }
    }
}
