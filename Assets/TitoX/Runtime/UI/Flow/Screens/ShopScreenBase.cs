using TitoX.Core.EventBus;
using TitoX.UI.Components;

namespace TitoX.UI.Flow
{
    /// <summary>Full screen opened from the Shop tab of <see cref="HomeBottomBarScreenBase"/>.</summary>
    public class ShopScreenBase : UIBase
    {
        protected override void Awake()
        {
            base.Awake();
            UIFeedback.BindClicks(this);
            ButtonContentTint.Bind(this);
        }

        protected override void InitData()
        {
            base.InitData();
            EventBus<HomeTabShownEvent>.Post(new HomeTabShownEvent(HomeTab.Shop));
        }
    }
}
