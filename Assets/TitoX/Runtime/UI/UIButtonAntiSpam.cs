using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace TitoX.UI
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Button))]
    public class UIButtonAntiSpam : MonoBehaviour
    {
        public enum ReleaseMode
        {
            Manual,
            OnEnable,
            AfterDelay,
            AfterTask
        }

        [Header("Settings")]
        [SerializeField]
        private ReleaseMode releaseMode = ReleaseMode.OnEnable;

        [SerializeField]
        [Min(0)]
        [ShowIf(nameof(releaseMode), ReleaseMode.AfterDelay)]
        private float delay = 0.5f;

        private Button button;
        private bool isLocked;

        public bool IsLocked => isLocked;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnEnable()
        {
            if (releaseMode == ReleaseMode.OnEnable)
            {
                Unlock();
            }
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (isLocked)
                return;

            Lock();

            switch (releaseMode)
            {
                case ReleaseMode.AfterDelay:
                    DelayUnlock().Forget();
                    break;

                case ReleaseMode.Manual:
                case ReleaseMode.AfterTask:
                case ReleaseMode.OnEnable:
                    break;
            }
        }

        public void Lock()
        {
            if (isLocked)
                return;

            isLocked = true;
            button.interactable = false;
        }

        public void Unlock()
        {
            isLocked = false;
            button.interactable = true;
        }

        private async UniTaskVoid DelayUnlock()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

            Unlock();
        }

        /// <summary>
        /// Should use when ReleaseMode = AfterTask
        /// </summary>
        public async UniTask Run(Func<UniTask> task)
        {
            if (releaseMode != ReleaseMode.AfterTask)
            {
                Debug.LogWarning($"{name}: Run() should be use with ReleaseMode.AfterTask");
            }

            if (isLocked)
                return;

            Lock();

            try
            {
                await task();
            }
            finally
            {
                Unlock();
            }
        }

        /// <summary>
        /// Overload for action not callback value
        /// </summary>
        public async UniTask<T> Run<T>(Func<UniTask<T>> task)
        {
            if (releaseMode != ReleaseMode.AfterTask)
            {
                Debug.LogWarning($"{name}: Run() should be use with ReleaseMode.AfterTask");
            }

            if (isLocked)
                return default;

            Lock();

            try
            {
                return await task();
            }
            finally
            {
                Unlock();
            }
        }
    }
}