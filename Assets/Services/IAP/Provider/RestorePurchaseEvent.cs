using TitoX.Core.EventBus;

namespace Tito.Services.IAP.Provider
{
    public struct RestorePurchaseEvent: IEvent
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        
        public RestorePurchaseEvent(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }
    }
}