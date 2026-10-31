using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameTemplate.Runtime.WGUI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace GameTemplate.Runtime.WGUI.Flow
{
    /// <summary>
    /// Closes an iris, pops a logo while the screen is covered, then opens again. Register it with Show/Hide animation
    /// None; runs on unscaled time so it also plays while the game is paused.
    /// </summary>
    public class IrisTransitionScreen : UIBase, IScreenTransition
    {
        [SerializeField] private IrisHole iris;
        [Tooltip("Optional.")]
        [SerializeField] private Image logo;
        [SerializeField] private float closeDuration = 0.3f;
        [SerializeField] private float logoPopDuration = 0.35f;
        [SerializeField] private float holdDuration = 0.8f;
        [SerializeField] private float openDuration = 0.35f;
        [Tooltip("Above every popup and notification.")]
        [SerializeField] private int sortingOrder = 5000;

        private Vector3 logoScale;

        protected override void Awake()
        {
            base.Awake();
            if (logo != null)
                logoScale = logo.transform.localScale;
        }

        protected override void InitData()
        {
            // UIManager re-sorts popups while this is up; nested canvases also drop overrideSorting while inactive.
            Canvas.overrideSorting = true;
            Canvas.sortingOrder = sortingOrder;

            iris.Radius = 1f;
            if (logo != null)
            {
                logo.transform.localScale = Vector3.zero;
                logo.color = new Color(logo.color.r, logo.color.g, logo.color.b, 1f);
            }
        }

        public async UniTask PlayAsync(Action onCovered)
        {
            CancellationToken token = this.GetCancellationTokenOnDestroy();

            await TweenIris(0f, closeDuration).SetEase(Ease.InQuad).ToUniTask(cancellationToken: token);

            onCovered?.Invoke();

            if (logo != null)
            {
                await logo.transform.DOScale(logoScale, logoPopDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .ToUniTask(cancellationToken: token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(holdDuration), ignoreTimeScale: true, cancellationToken: token);

            Sequence open = DOTween.Sequence()
                .SetUpdate(true)
                .Join(TweenIris(1f, openDuration).SetEase(Ease.OutQuad));
            if (logo != null)
                open.Join(logo.DOFade(0f, openDuration));

            await open.ToUniTask(cancellationToken: token);
        }

        private Tween TweenIris(float endRadius, float duration)
        {
            return DOTween.To(() => iris.Radius, value => iris.Radius = value, endRadius, duration).SetUpdate(true);
        }
    }
}
