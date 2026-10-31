using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameTemplate.Runtime.Core.WCore.EventBus;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameTemplate.Runtime.WGUI.Flow
{
    /// <summary>
    /// Owns screen navigation: Loading → Home ⇄ Gameplay → Win | Lose → Gameplay | Home.
    /// Screens only request moves through <see cref="UIFlow"/>; the screens of each state come from <see cref="UIFlowConfig"/>
    /// and the level work from <see cref="IGameFlowHooks"/>. One request runs at a time; requests made meanwhile are refused.
    /// </summary>
    public class UIFlowController : MonoBehaviour
    {
        [SerializeField] private UIFlowConfig config;

        [Tooltip("Component implementing IGameFlowHooks. Leave empty when the game registers hooks with UIFlow.SetHooks.")]
        [SerializeField] private MonoBehaviour hooksComponent;

        [SerializeField] private bool runOnStart = true;

        private readonly List<Type> stateScreens = new();
        private bool isBusy;

        public static UIFlowController Instance { get; private set; }

        public UIFlowState State { get; private set; } = UIFlowState.None;

        /// <summary>True while a request, including its transition, is running.</summary>
        public bool IsBusy => isBusy;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[UIFlow] A second UIFlowController was disabled; keep one per game.");
                enabled = false;
                return;
            }

            Instance = this;

            if (config == null)
                Debug.LogError("[UIFlow] UIFlowConfig is not assigned.");

            if (hooksComponent != null)
            {
                if (hooksComponent is IGameFlowHooks hooks)
                    UIFlow.SetHooks(hooks);
                else
                    Debug.LogError($"[UIFlow] {hooksComponent.GetType().Name} does not implement IGameFlowHooks.");
            }
        }

        private void Start()
        {
            if (runOnStart)
                RunAsync().Forget();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Shows loading, then Home, or the current level when <see cref="IGameFlowHooks.SkipHomeOnLaunch"/>.</summary>
        public async UniTask RunAsync()
        {
            if (State != UIFlowState.None || !TryBegin(nameof(RunAsync)))
                return;

            try
            {
                UIBase loading = null;
                if (TryResolve(config.LoadingScreen, out Type loadingType))
                {
                    SetState(UIFlowState.Loading);
                    loading = await UIManager.Instance.ShowAsync(loadingType);
                    if (loading is ILoadingScreen loadingScreen)
                        await loadingScreen.PlayAsync();
                }

                IGameFlowHooks hooks = UIFlow.Hooks;
                if (hooks != null && hooks.SkipHomeOnLaunch)
                {
                    hooks.StartLevel();
                    await EnterStateAsync(UIFlowState.Gameplay);
                }
                else
                {
                    await EnterStateAsync(UIFlowState.Home);
                }

                if (loading != null && !stateScreens.Contains(loadingType))
                    UIManager.Instance.Hide(loading);
            }
            finally
            {
                isBusy = false;
            }
        }

        public bool Play()
        {
            return Request(nameof(Play), UIFlowState.Gameplay, hooks => hooks.StartLevel(), true, UIFlowState.Home);
        }

        public bool Retry()
        {
            return Request(nameof(Retry), UIFlowState.Gameplay, hooks => hooks.RetryLevel(), true,
                UIFlowState.Gameplay, UIFlowState.Win, UIFlowState.Lose);
        }

        public bool Next()
        {
            return Request(nameof(Next), UIFlowState.Gameplay, hooks => hooks.NextLevel(), true, UIFlowState.Win);
        }

        public bool Home()
        {
            return Request(nameof(Home), UIFlowState.Home, hooks => hooks.QuitLevel(), true,
                UIFlowState.Gameplay, UIFlowState.Win, UIFlowState.Lose);
        }

        /// <summary>Called by gameplay once the level is won and its reward is saved.</summary>
        public bool Win()
        {
            return Request(nameof(Win), UIFlowState.Win, null, false, UIFlowState.Gameplay);
        }

        /// <summary>Called by gameplay once the level is lost.</summary>
        public bool Lose()
        {
            return Request(nameof(Lose), UIFlowState.Lose, null, false, UIFlowState.Gameplay);
        }

        private bool Request(string name, UIFlowState target, Action<IGameFlowHooks> levelAction, bool useTransition,
            params UIFlowState[] allowedFrom)
        {
            if (Array.IndexOf(allowedFrom, State) < 0)
            {
                Debug.LogWarning($"[UIFlow] {name} ignored in state {State}.");
                return false;
            }

            if (levelAction != null && UIFlow.Hooks == null)
            {
                Debug.LogError($"[UIFlow] {name} needs IGameFlowHooks: assign one on UIFlowController or call UIFlow.SetHooks.");
                return false;
            }

            if (!TryBegin(name))
                return false;

            RunRequestAsync(target, levelAction, useTransition).Forget();
            return true;
        }

        private async UniTaskVoid RunRequestAsync(UIFlowState target, Action<IGameFlowHooks> levelAction, bool useTransition)
        {
            try
            {
                if (useTransition)
                {
                    await TransitionAsync(target, levelAction);
                }
                else
                {
                    levelAction?.Invoke(UIFlow.Hooks);
                    await EnterStateAsync(target);
                }
            }
            finally
            {
                isBusy = false;
            }
        }

        private bool TryBegin(string name)
        {
            if (isBusy)
            {
                Debug.LogWarning($"[UIFlow] {name} ignored: another flow change is running.");
                return false;
            }

            isBusy = true;
            return true;
        }

        private async UniTask TransitionAsync(UIFlowState target, Action<IGameFlowHooks> levelAction)
        {
            UIBase transitionScreen = null;
            if (TryResolve(config.TransitionScreen, out Type transitionType))
                transitionScreen = await UIManager.Instance.ShowAsync(transitionType);

            if (transitionScreen is not IScreenTransition transition)
            {
                if (transitionScreen != null)
                {
                    Debug.LogError($"[UIFlow] {transitionType.Name} does not implement IScreenTransition.");
                    UIManager.Instance.Hide(transitionScreen);
                }

                levelAction?.Invoke(UIFlow.Hooks);
                await EnterStateAsync(target);
                return;
            }

            UniTask enterState = UniTask.CompletedTask;
            try
            {
                await transition.PlayAsync(() =>
                {
                    levelAction?.Invoke(UIFlow.Hooks);
                    enterState = EnterStateAsync(target, transitionType);
                });

                await enterState;
            }
            finally
            {
                UIManager.Instance.Hide(transitionScreen);
            }
        }

        private async UniTask EnterStateAsync(UIFlowState target, Type transitionType = null)
        {
            List<Type> nextScreens = ResolveScreens(target);

            foreach (Type type in stateScreens)
            {
                if (!nextScreens.Contains(type))
                    UIManager.Instance.Hide(type);
            }

            if (config.ClosePopupsOnStateChange)
            {
                // Previous state screens are already hiding above; hiding them twice would release them twice.
                var keep = new HashSet<Type>(nextScreens);
                keep.UnionWith(stateScreens);
                if (transitionType != null)
                    keep.Add(transitionType);

                UIManager.Instance.HideAllPopupsAsync(keep).Forget();
            }

            stateScreens.Clear();
            stateScreens.AddRange(nextScreens);
            SetState(target);

            var shows = new UniTask[nextScreens.Count];
            for (int i = 0; i < nextScreens.Count; i++)
            {
                shows[i] = UIManager.Instance.ShowAsync(nextScreens[i]).AsUniTask();
            }

            await UniTask.WhenAll(shows);
        }

        private List<Type> ResolveScreens(UIFlowState state)
        {
            var types = new List<Type>();
            foreach (AssetReference reference in config.GetScreens(state))
            {
                if (TryResolve(reference, out Type type) && !types.Contains(type))
                    types.Add(type);
            }

            return types;
        }

        private static bool TryResolve(AssetReference reference, out Type type)
        {
            type = null;
            if (reference == null || string.IsNullOrEmpty(reference.AssetGUID))
                return false;

            if (UIManager.Instance.Config != null && UIManager.Instance.Config.TryGetType(reference, out type))
                return true;

            Debug.LogError($"[UIFlow] Screen {reference.AssetGUID} is not registered in the UIManager's UIConfigSO.");
            return false;
        }

        private void SetState(UIFlowState state)
        {
            if (State == state)
                return;

            UIFlowState previous = State;
            State = state;
            EventBus<UIFlowStateChangedEvent>.Post(new UIFlowStateChangedEvent(previous, state));
        }
    }
}
