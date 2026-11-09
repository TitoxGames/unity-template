using System;

namespace TitoX.GameData
{
    public abstract class PlayerService<TData>
    {
        protected readonly TData Data;
        private readonly Action save;

        protected PlayerService(TData data, Action save)
        {
            Data = data;
            this.save = save;
        }

        protected void Save()
        {
            save();
        }
    }
}