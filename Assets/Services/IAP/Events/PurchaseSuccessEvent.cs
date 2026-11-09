using TitoX.Core.EventBus;

namespace Tito.Services.IAP.Events
{
    public readonly struct PurchaseSuccessEvent: IEvent
    {
        public readonly string ProductId;
        public readonly string TransactionId;
        public readonly string Receipt;

        public PurchaseSuccessEvent(
            string productId,
            string transactionId,
            string receipt)
        {
            ProductId = productId;
            TransactionId = transactionId;
            Receipt = receipt;
        }
    }
}