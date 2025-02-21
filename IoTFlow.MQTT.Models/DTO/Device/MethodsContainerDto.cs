
using System.Text.Json.Serialization;

namespace IoTFlow.MQTT.Models.DTO.Device
{
    public class MethodsContainerDto
    {
        [JsonPropertyName("methods")]
        public List<MethodDto> Methods { get; set; } = new List<MethodDto>();
    }
}
