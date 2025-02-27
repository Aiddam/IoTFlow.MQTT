using IoTFlow.MQTT.Models.DTO.Device;
namespace IoTFlow.MQTT.Utilities
{
    public static class CommandUrlBuilder
    {
        public static string BuildCommandUrl(string methodName, IEnumerable<MethodParameterResponseDto>? parameters, Guid correlationId)
        {
            var correlationParam = $"correlationId={Uri.EscapeDataString(correlationId.ToString())}";
            if (parameters == null || !parameters.Any())
            {
                return $"{methodName}?{correlationParam}";
            }

            var queryString = string.Join("&", parameters.Select(p =>
                $"{Uri.EscapeDataString(p.ParameterName)}={Uri.EscapeDataString(p.Value)}"));

            return $"{methodName}?{queryString}&{correlationParam}";
        }
    }
}
