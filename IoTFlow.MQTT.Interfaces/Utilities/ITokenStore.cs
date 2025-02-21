
namespace IoTFlow.MQTT.Interfaces.Utilities
{
    public interface ITokenStore
    {
        void SetToken(string clientId, string token);
        bool TryGetToken(string clientId, out string token);
        bool RemoveToken(string clientId);
    }
}
