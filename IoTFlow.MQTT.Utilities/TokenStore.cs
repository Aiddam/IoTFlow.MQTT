using IoTFlow.MQTT.Interfaces.Utilities;
using System.Collections.Concurrent;

namespace IoTFlow.MQTT.Utilities
{
    public class TokenStore : ITokenStore
    {
        private readonly ConcurrentDictionary<string, string> _tokens = new ConcurrentDictionary<string, string>();

        public void SetToken(string clientId, string token) => _tokens[clientId] = token;
        public bool TryGetToken(string clientId, out string token) => _tokens.TryGetValue(clientId, out token);
        public bool RemoveToken(string clientId) => _tokens.TryRemove(clientId, out _);
    }
}
