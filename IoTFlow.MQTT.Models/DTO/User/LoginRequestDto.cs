
using System.ComponentModel.DataAnnotations;

namespace IoTFlow.MQTT.Models.DTO.User
{
    public class LoginRequestDto
    {
        [Required]
        public string NameOrEmail { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
