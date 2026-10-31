using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WCore;

namespace GameTemplate.Runtime.WGUI
{
    public class UIManager : Singleton<UIManager>
    {
#if ODIN_INSPECTOR
        [InlineEditor]
#endif
        [SerializeField] private UIConfigSO uiConfig;
        [SerializeField] private Transform _parent;
        private readonly Dictionary<Type, AsyncOperationHandle<GameObject>> _loaded = new();
        [SerializeField] private readonly Dictionary<Type, UIBase> _spawned = new();
        private readonly Dictionary<Type, UniTask<UIBase>> _spawning = new();
        private readonly List<UIBase> _popupStack = new();
        [SerializeField] private UIBase _currentFullScreen;
        [SerializeField] private UIBase _currentPopup;
        private UINotification _currentNotification;


        public bool HasPopupOnTop => _currentPopup != null || _popupStack.Count > 0;
        private UniTask _preloadTask;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }
        private void OnDestroy()
        {
            Dispose();
        }

        #endregion

        #region Initialization / Dispose

        private void Initialize()
        {
            if (uiConfig == null)
            {
                Debug.LogError("[UIManager] UIConfigExtend is null.");
                return;
            }
        }

        private void Dispose()
        {
            foreach (AsyncOperationHandle<GameObject> handle in _loaded.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            _loaded.Clear();

            _spawned.Clear();
            _popupStack.Clear();

            _currentFullScreen = null;
            _currentPopup = null;
            _currentNotification = null;
        }



        #endregion

        #region Public API

        /// <summary>The config asset, e.g. to map an AssetReference to its screen type.</summary>
        public UIConfigSO Config => uiConfig;

        /// <summary><typeparamref name="T"/> may be a base class; the single registered subclass of it is shown.</summary>
        public void Show<T>(Action<T> onInit = null, Action onComplete = null) where T : UIBase
        {
            ShowInternalAsync(typeof(T), WrapInit(onInit), onComplete).Forget();
        }

        /// <summary><typeparamref name="T"/> may be a base class; the single registered subclass of it is shown.</summary>
        public async UniTask<T> ShowAsync<T>(Action<T> onInit = null, Action onComplete = null) where T : UIBase
        {
            UIBase ui = await ShowInternalAsync(typeof(T), WrapInit(onInit), onComplete);
            return ui as T;
        }

        public void Show(Type type, Action<UIBase> onInit = null, Action onComplete = null)
        {
            ShowInternalAsync(type, onInit, onComplete).Forget();
        }

        public UniTask<UIBase> ShowAsync(Type type, Action<UIBase> onInit = null, Action onComplete = null)
        {
            return ShowInternalAsync(type, onInit, onComplete);
        }

        private static Action<UIBase> WrapInit<T>(Action<T> onInit) where T : UIBase
        {
            if (onInit == null)
                return null;

            return ui => onInit(ui as T);
        }

        private async UniTask<UIBase> ShowInternalAsync(Type requestedType, Action<UIBase> onInit, Action onComplete)
        {
            Type type = uiConfig.ResolveType(requestedType);
            UIConfig config = type != null ? uiConfig.GetConfig(type) : null;
            if (config == null)
            {
                Debug.LogError($"[UIManager] No UIConfig for {requestedType?.Name}");
                return null;
            }
            UIBase newUI = await GetOrSpawn(type);
            if (newUI == null)
                return null;
            onInit?.Invoke(newUI);
            newUI.SetAnimationDuration(config.AnimationDuration);
            if (newUI is UINotification notification && notification.gameObject.activeSelf)
            {
                await notification.RestartShowAsync();
                onComplete?.Invoke();
                return newUI;
            }
            HandleCurrentUIBeforeShow(newUI, config);
            try
            {
                await newUI.ShowAsync(config.ShowAnimationType);

                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[UIManager] Show animation for {type.Name} was canceled.");

            }
            return newUI;

        }

        private async UniTask<UIBase> GetOrSpawn(Type type)
        {
            if (_spawned.TryGetValue(type, out UIBase cached) && cached != null)
                return cached;

            if (_spawning.TryGetValue(type, out UniTask<UIBase> existingTask))
            {
                Debug.LogWarning($"[UIManager] {type.Name} đang được tải/tạo, đợi tiến trình cũ...");
                return await existingTask;
            }

            UniTask<UIBase> spawnTask = InternallySpawnAndRegister(type).Preserve();
            _spawning[type] = spawnTask;

            try
            {
                return await spawnTask;
            }
            finally
            {

                _spawning.Remove(type);
            }
        }

        private async UniTask<UIBase> InternallySpawnAndRegister(Type type)
        {
            UIConfig config = uiConfig.GetConfig(type);
            if (config == null || config.AssetReference == null)
            {
                Debug.LogError($"[UIManager] Missing prefab config for {type.Name}");
                return null;
            }

            UIBase ui = await Spawn(type, config);
            if (ui == null) return null;

            ui.gameObject.SetActive(false);

            if (!_spawned.TryAdd(type, ui))
            {
                Debug.LogError($"[UIManager] Failed to add spawned UI for {type.Name}");
                Destroy(ui.gameObject);
                return null;
            }

            SetCanvasProperties(ui, config);

            return ui;
        }

        private async UniTask<UIBase> Spawn(Type type, UIConfig config)
        {
            UIBase cachedPrefab = null;
            if (_loaded.TryGetValue(type, out AsyncOperationHandle<GameObject> cachedHandle))
            {
                cachedPrefab = cachedHandle.Result.GetComponent<UIBase>();
            }
            else
            {
                var handle = config.AssetReference.LoadAssetAsync<GameObject>();
                await handle;
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError(handle.OperationException);
                    return null;
                }
                cachedPrefab = handle.Result.GetComponent<UIBase>();
                _loaded.TryAdd(type, handle);
            }

            if (_parent == null) _parent = transform;
            return Instantiate(cachedPrefab, _parent);
        }

        public void Hide<T>(Action onComplete = null) where T : UIBase
        {
            HideInternal(typeof(T), onComplete).Forget();
        }
        public void Hide(Type type, Action onComplete = null)
        {
            HideInternal(type, onComplete).Forget();
        }
        public void Hide(UIBase ui)
        {
            HideUI(ui).Forget();
        }
        private async UniTaskVoid HideInternal(Type requestedType, Action onComplete = null)
        {
            if (!TryGetSpawned(requestedType, out UIBase ui))
                return;

            try
            {
                await HideUI(ui);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[UIManager] Hide {requestedType.Name} was canceled.");
            }
        }

        /// <summary>Hides every open popup except the given types (and their registered subclasses).</summary>
        public UniTask HideAllPopupsAsync(ICollection<Type> keep = null)
        {
            List<UniTask> tasks = new();
            foreach (UIBase popup in new List<UIBase>(_popupStack))
            {
                if (popup == null || (keep != null && keep.Contains(popup.GetType())))
                    continue;

                tasks.Add(HideUI(popup));
            }

            return UniTask.WhenAll(tasks);
        }

        /// <summary>True while the screen (or the registered subclass of a base type) is active.</summary>
        public bool IsVisible<T>() where T : UIBase
        {
            return IsVisible(typeof(T));
        }

        public bool IsVisible(Type type)
        {
            return TryGetSpawned(type, out UIBase ui) && ui.gameObject.activeSelf;
        }

        /// <summary>The spawned instance, without showing it.</summary>
        public bool TryGet<T>(out T ui) where T : UIBase
        {
            bool found = TryGetSpawned(typeof(T), out UIBase spawned);
            ui = spawned as T;
            return found && ui != null;
        }

        private bool TryGetSpawned(Type requestedType, out UIBase ui)
        {
            ui = null;
            if (requestedType == null)
                return false;

            Type type = uiConfig.ResolveType(requestedType) ?? requestedType;
            return _spawned.TryGetValue(type, out ui) && ui != null;
        }

        public void HideAll(Action onComplete = null)
        {
            HideAllInternal(onComplete).Forget();
        }

        private async UniTaskVoid HideAllInternal(Action onComplete = null)
        {
            List<UniTask> tasks = new();
            foreach (UIBase ui in new List<UIBase>(_spawned.Values))
            {
                tasks.Add(HideUI(ui));
            }

            try
            {
                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("[UIManager] HideAll was canceled.");
            }

            onComplete?.Invoke();
        }

        private async UniTask HideUI(UIBase ui)
        {
            if (ui == null)
                return;

            UIConfig config = uiConfig.GetConfig(ui.GetType());
            if (config == null)
                return;

            await ui.HideAsync(config.HideAnimationType);

            HandleUIClose(ui);

            if (config.LifeCyclePolicy == UILifeCyclePolicy.ReleaseInstance)
            {
                Type type = ui.GetType();

                Destroy(ui.gameObject);
                _spawned.Remove(type);

                if (_loaded.TryGetValue(type, out var handle))
                {
                    if (handle.IsValid())
                        Addressables.Release(handle);

                    _loaded.Remove(type);
                }
            }
        }

        #endregion

        #region Show Flow



        private void HandleCurrentUIBeforeShow(UIBase newUI, UIConfig config)
        {
            switch (config.CanvasType)
            {
                case CanvasType.FullScreen:
                    HandleFullScreenBeforeShow(newUI);
                    break;


                case CanvasType.Popup:
                    HandlePopupBeforeShow(newUI);
                    break;

                case CanvasType.Notification:
                    _currentNotification = newUI as UINotification;
                    break;

            }
        }

        private void HandleFullScreenBeforeShow(UIBase newUI)
        {
            if (_currentFullScreen != null && _currentFullScreen != newUI)
            {
                Type type = _currentFullScreen.GetType();
                HideUI(_currentFullScreen).Forget();
            }
            _currentFullScreen = newUI;
        }

        private void HandlePopupBeforeShow(UIBase newUI)
        {
            if (newUI == null)
                return;
            _popupStack.Remove(newUI);
            _popupStack.Add(newUI);
            _currentPopup = newUI;

            RefreshPopupCanvasOrders();
        }


        #endregion


        #region Hide / Close


        private void HandleUIClose(UIBase ui)
        {
            if (ui == null)
                return;

            _popupStack.Remove(ui);

            if (_currentFullScreen == ui)
                _currentFullScreen = null;

            if (_currentPopup == ui)
                _currentPopup = _popupStack.Count > 0 ? _popupStack[^1] : null;

            if (_currentNotification == ui)
                _currentNotification = null;
            RefreshPopupCanvasOrders();
        }

        #endregion

        #region Canvas Sorting

        private void SetCanvasProperties(UIBase ui, UIConfig config)
        {
            if (ui == null || ui.Canvas == null)
                return;

            if (config == null)
            {
                Debug.LogError($"[UIManager] Missing UIConfig for {ui.GetType().Name}");
                return;
            }

            CanvasType canvasType = config.CanvasType;
            int baseOrder = GetBaseOrder(canvasType);

            ui.Canvas.overrideSorting = true;
            ui.Canvas.sortingOrder = uiConfig.ResolveSortingOrder(config, baseOrder);


            if (canvasType == CanvasType.Popup)
            {
                int popupIndex = _popupStack.IndexOf(ui);
                if (popupIndex < 0)
                    popupIndex = _popupStack.Count;

                ui.Canvas.sortingOrder += popupIndex;
            }
        }


        private void RefreshPopupCanvasOrders()
        {
            for (int i = _popupStack.Count - 1; i >= 0; i--)
            {
                UIBase popup = _popupStack[i];

                if (popup == null)
                {
                    _popupStack.RemoveAt(i);
                    continue;
                }

                UIConfig config = uiConfig.GetConfig(popup.GetType());
                SetCanvasProperties(popup, config);
            }

            _currentPopup = _popupStack.Count > 0 ? _popupStack[^1] : null;
        }

        private int GetBaseOrder(CanvasType type)
        {
            return type switch
            {
                CanvasType.FullScreen => 0,
                CanvasType.Popup => 200,
                CanvasType.Notification => 1000,
                _ => 0
            };
        }

        #endregion

    }
}
