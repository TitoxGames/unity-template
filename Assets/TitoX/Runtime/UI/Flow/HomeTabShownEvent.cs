using TitoX.Core.EventBus;

namespace TitoX.UI.Flow
{
    /// <summary>Posted by the Shop, Home and Collections screens when they show, so the bottom bar follows them however they were opened.</summary>
    public readonly struct HomeTabShownEvent : IEvent
    {
        public HomeTab Tab { get; }

        public HomeTabShownEvent(HomeTab tab)
        {
            Tab = tab;
        }
    }
}
