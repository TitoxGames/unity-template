using GameTemplate.Runtime.Core.WCore.EventBus;

namespace GameTemplate.Runtime.WGUI.Flow
{
    public enum UIFlowState
    {
        None,
        Loading,
        Home,
        Gameplay,
        Win,
        Lose,
    }

    public readonly struct UIFlowStateChangedEvent : IEvent
    {
        public UIFlowState Previous { get; }
        public UIFlowState Current { get; }

        public UIFlowStateChangedEvent(UIFlowState previous, UIFlowState current)
        {
            Previous = previous;
            Current = current;
        }
    }
}
