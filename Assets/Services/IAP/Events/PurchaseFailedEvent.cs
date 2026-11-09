using TitoX.Core.EventBus;

namespace Tito.Services.IAP.Events
{
    public readonly struct PurchaseFailedEvent: IEvent
    {
        public readonly string ProductId;
        public readonly PurchaseStatus Status;
        public readonly string Error;

        public PurchaseFailedEvent(
            string productId,
            PurchaseStatus status,
            string error)
        {
            ProductId = productId;
            Status = status;
            Error = error;
        }
    }
}