using System;
using TitoX.Core.EventBus;

namespace TitoX.GameData
{
    public class BoosterService: PlayerService<BoosterData>
    {
        private readonly BoosterData data;
        private readonly Action save;

        public BoosterService(BoosterData data, Action save) : base(data, save)
        {
            this.data = data;
            this.save = save;
        }
        
        public void Register(string id, int defaultAmount)
        {
            if (data.Get(id) != null)
                return;

            data.Add(id, defaultAmount);

            save();
        }

        public int GetAmount(string id)
        {
            return data.GetAmount(id);
        }

        public bool HasEnough(string id)
        {
            return data.GetAmount(id) > 0;
        }

        public void Add(string id, int amount)
        {
            if (amount <= 0)
                return;

            data.Add(id, amount);

            save();

            EventBus<PlayerBoosterChangedEvent>.Post(
                new PlayerBoosterChangedEvent(
                    id,
                    data.GetAmount(id)));
        }

        public bool Use(string id)
        {
            if (!data.Use(id))
                return false;

            save();

            EventBus<PlayerBoosterChangedEvent>.Post(
                new PlayerBoosterChangedEvent(
                    id,
                    data.GetAmount(id)));

            return true;
        }
    }
}