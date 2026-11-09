using TitoX.Core.EventBus;

namespace TitoX.Core.Time
{
    /// <summary>
    /// Event triggered when a GameplayTime instance changes state.
    /// </summary>
    public struct GameplayTimeEvent : IEvent
    {
        public EventType Type { get; }
        
        /// <summary>
        /// The name of the GameplayTime instance that triggered this event.
        /// </summary>
        public string TimerName { get; }

        public enum EventType
        {
            Started,
            Paused,
            Resumed,
            Stopped,
            Reset,
            SpeedChanged,
            CountdownComplete
        }

        public GameplayTimeEvent(EventType type, string timerName)
        {
            Type = type;
            TimerName = timerName;
        }
    }
}