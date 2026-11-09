using TitoX.Core.EventBus;

namespace TitoX.Core.Settings
{
    public struct SettingEvent: IEvent
    {
        public GameSettings Settings { get; private set; }
        
        public SettingEvent(GameSettings settings)
        {
            Settings = settings;
        }
    }
}