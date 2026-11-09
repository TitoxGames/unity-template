using System.Collections.Generic;
using MemoryPack;

namespace TitoX.GameData
{
    [MemoryPackable]
    public partial class BoosterItem
    {
        public string Id;

        public int Amount;
    }

    [MemoryPackable]
    public partial class BoosterData
    {
        public List<BoosterItem> Items = new();

        public BoosterItem Get(string id)
        {
            return Items.Find(x => x.Id == id);
        }

        public int GetAmount(string id)
        {
            return Get(id)?.Amount ?? 0;
        }

        public void Add(string id, int amount)
        {
            var item = Get(id);

            if (item == null)
            {
                item = new BoosterItem
                {
                    Id = id,
                    Amount = amount
                };

                Items.Add(item);
            }
            else
            {
                item.Amount += amount;
            }
        }

        public bool Use(string id)
        {
            var item = Get(id);

            if (item == null || item.Amount <= 0)
                return false;

            item.Amount--;

            return true;
        }

        public bool HasEnough(string id, int amount = 1)
        {
            return GetAmount(id) >= amount;
        }
    }
}