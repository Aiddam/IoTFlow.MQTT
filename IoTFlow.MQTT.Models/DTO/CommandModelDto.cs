
using IoTFlow.MQTT.Models.DTO.Device;

namespace IoTFlow.MQTT.Models.DTO
{
    public class CommandModelDto
    {
        public string Command { get; set; } = string.Empty;
        public string DeviceGuid { get; set; } = string.Empty;
        public ICollection <MethodParameterResponseDto>? Parameters { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
