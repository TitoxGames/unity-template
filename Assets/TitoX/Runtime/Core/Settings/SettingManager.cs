using TitoX.Core.EventBus;
using MemoryPack;
using WData;

namespace TitoX.Core.Settings
{
    [MemoryPackable]
    public partial class GameSettings
    {
        public bool Music { get; set; } = true;
        public bool Sfx { get; set; } = true;
        public bool Haptic { get; set; } = true;
    }

    public sealed class SettingManager
        : DataBlock<SettingManager, GameSettings>
    {
        public bool Music => Data.Music;
        public bool Sfx => Data.Sfx;
        public bool Haptic => Data.Haptic;

        public void SetMusic(bool value)
        {
            Data.Music = value;
            SaveAndNotify();
        }

        public void SetSfx(bool value)
        {
            Data.Sfx = value;
            SaveAndNotify();
        }

        public void SetHaptic(bool value)
        {
            Data.Haptic = value;
            SaveAndNotify();
        }

        private void SaveAndNotify()
        {
            Save(true);
            EventBus<SettingEvent>.Post(new SettingEvent(Data));
        }

        public void Sync()
        {
            EventBus<SettingEvent>.Post(new SettingEvent(Data));
        }
    }
}