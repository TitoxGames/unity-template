using TitoX.Core.EventBus;

namespace TitoX.GameData
{
    public struct PlayerBoosterChangedEvent: IEvent
    {
        public string BoosterId;
        public int Amount;
        
        public PlayerBoosterChangedEvent(string boosterId, int amount)
        {
            BoosterId = boosterId;
            Amount = amount;
        }
    }
}