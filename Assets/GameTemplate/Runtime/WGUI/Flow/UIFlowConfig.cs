using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameTemplate.Runtime.WGUI.Flow
{
    /// <summary>
    /// Which screens each flow state shows. Every reference must be a prefab registered in the UIManager's UIConfigSO.
    /// A screen listed in two consecutive states stays up between them (e.g. a top bar in Home and Gameplay).
    /// </summary>
    [CreateAssetMenu(menuName = "WGUI/UI Flow Config")]
    public class UIFlowConfig : ScriptableObject
    {
        [Serializable]
        public class StateScreens
        {
            public UIFlowState State;
            public List<AssetReference> Screens = new();
        }

        [Tooltip("Shown first on launch; implement ILoadingScreen to hold the flow until it finishes. Empty skips loading.")]
        [SerializeField] private AssetReference loadingScreen;

        [Tooltip("Covers Play, Retry, Next and Home; must implement IScreenTransition. Empty switches instantly.")]
        [SerializeField] private AssetReference transitionScreen;

        [Tooltip("Closes popups opened outside the flow (Settings, Shop...) whenever the state changes.")]
        [SerializeField] private bool closePopupsOnStateChange = true;

        [SerializeField] private List<StateScreens> states = new();

        public AssetReference LoadingScreen => loadingScreen;

        public AssetReference TransitionScreen => transitionScreen;

        public bool ClosePopupsOnStateChange => closePopupsOnStateChange;

        public IReadOnlyList<AssetReference> GetScreens(UIFlowState state)
        {
            foreach (StateScreens entry in states)
            {
                if (entry.State == state)
                    return entry.Screens;
            }

            return Array.Empty<AssetReference>();
        }
    }
}
