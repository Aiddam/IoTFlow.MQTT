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
    }
}
