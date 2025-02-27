
namespace IoTFlow.MQTT.Interfaces.Services
{
    public interface IIotFlowApiService<TLoginRequest, TUserResponse, TRefreshRequest>
    {
        Task<TUserResponse> LoginUserAsync(TLoginRequest request, CancellationToken cancellationToken = default);
        Task<TUserResponse> RefreshUserAsync(TRefreshRequest request, CancellationToken cancellationToken = default);
        Task<bool> IsDeviceExist(string deviceGuid, CancellationToken cancellationToken = default);
        Task<bool> SetDeviceAlive(string deviceGuid, bool isAlive, CancellationToken cancellationToken = default);
        Task SetDeviceMethodsAsync(string deviceGuid, string methods, CancellationToken cancellationToken = default);
        Task HandleDeviceResponseAsync(string deviceGuid, string correlationId, bool success, string message, string resultValue, CancellationToken cancellationToken = default);
    }
}
