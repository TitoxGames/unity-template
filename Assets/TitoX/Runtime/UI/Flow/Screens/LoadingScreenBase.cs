using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TitoX.UI.Flow
{
    /// <summary>Fills a progress bar over <see cref="duration"/>, and also waits for <see cref="WaitUntilReadyAsync"/>.</summary>
    public class LoadingScreenBase : UIBase, ILoadingScreen
    {
        [Tooltip("Filled by its anchorMax.x, from 0 to the end progress.")]
        [SerializeField] private RectTransform progressFill;
        [SerializeField] private TMP_Text progressText;
        [SerializeField, Min(0f)] private float duration = 2f;
        [Tooltip("Where the bar stops; below 1 avoids a full bar sitting on screen while the next state appears.")]
        [SerializeField, Range(0f, 1f)] private float endProgress = 1f;

        protected override void Awake()
        {
            base.Awake();
            SetProgress(0f);
        }

        public async UniTask PlayAsync()
        {
            CancellationToken token = this.GetCancellationTokenOnDestroy();
            SetProgress(0f);

            await UniTask.WhenAll(
                DOVirtual.Float(0f, endProgress, duration, SetProgress)
                    .SetEase(Ease.Linear)
                    .SetUpdate(true)
                    .ToUniTask(cancellationToken: token),
                WaitUntilReadyAsync(token));
        }

        /// <summary>Real startup work (SDK init, preloading) the bar should wait for.</summary>
        protected virtual UniTask WaitUntilReadyAsync(CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        protected virtual void SetProgress(float progress)
        {
            if (progressFill != null)
                progressFill.anchorMax = new Vector2(progress, progressFill.anchorMax.y);

            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
        }
    }
}
