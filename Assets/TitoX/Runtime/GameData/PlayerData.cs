using MemoryPack;
using WData;

namespace TitoX.GameData
{
    [MemoryPackable]
    public partial class PlayerData
    {
        public CurrencyData Currency = new();
        public BoosterData Booster = new();
    }
    
    public sealed class Player : DataBlock<Player, PlayerData>
    {
        public CurrencyService Currency { get; private set; }

        public BoosterService Booster { get; private set; }

        protected override void Init()
        {
            base.Init();

            Currency = new CurrencyService(Data.Currency, (() => Save()));
            Booster = new BoosterService(Data.Booster, (() => Save()));
        }
    }
    
    
}