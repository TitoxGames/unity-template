using TitoX.Core.EventBus;

namespace TitoX.GameData
{
    public struct PlayerCurrencyChangedEvent: IEvent
    {
        public CurrencyType CurrencyType { get; }
        public double Value { get; }
        
        public PlayerCurrencyChangedEvent(CurrencyType currencyType, double value)
        {
            CurrencyType = currencyType;
            Value = value;
        }
    }
}