using System;
using TitoX.Core.EventBus;
using TitoX.UI.Components;
using UnityEngine;

namespace TitoX.UI.Flow
{
    /// <summary>
    /// Tab bar switching between the Shop, Home and Collections screens. List it with the Home screen in the Home state
    /// of UIFlowConfig, as a Popup sorted between full screens and popups, so it stays up while the tabs swap underneath.
    /// </summary>
    public class HomeBottomBarScreenBase : UIBase
    {
        [SerializeField] protected HomeTabButton[] tabs;

        private HomeTab current = HomeTab.Home;

        public HomeTab Current => current;

        protected override void Awake()
        {
            base.Awake();
            UIFeedback.BindClicks(this);

            foreach (HomeTabButton tab in tabs)
                tab.Clicked += HandleTabClicked;

            EventBus<HomeTabShownEvent>.Subscribe(HandleTabShown);
        }

        protected override void OnDestroy()
        {
            EventBus<HomeTabShownEvent>.Unsubscribe(HandleTabShown);

            foreach (HomeTabButton tab in tabs)
                tab.Clicked -= HandleTabClicked;

            base.OnDestroy();
        }

        protected override void InitData()
        {
            base.InitData();
            RefreshTabs(false);
        }

        /// <summary>The screen shown for <paramref name="tab"/>; a base type opens its single registered subclass.</summary>
        protected virtual Type GetScreenType(HomeTab tab)
        {
            return tab switch
            {
                HomeTab.Shop => typeof(ShopScreenBase),
                HomeTab.Collections => typeof(CollectionsScreenBase),
                _ => typeof(HomeScreenBase),
            };
        }

        protected virtual void HandleTabClicked(HomeTab tab)
        {
            if (tab == current)
                return;

            UIFeedback.Haptic();
            UIManager.Instance.Show(GetScreenType(tab));
        }

        private void HandleTabShown(HomeTabShownEvent e)
        {
            current = e.Tab;

            // The tab buttons only cache their layout once the bar has been shown.
            if (gameObject.activeInHierarchy)
                RefreshTabs(true);
        }

        private void RefreshTabs(bool animate)
        {
            foreach (HomeTabButton tab in tabs)
                tab.SetSelected(tab.Tab == current, animate);
        }
    }
}
