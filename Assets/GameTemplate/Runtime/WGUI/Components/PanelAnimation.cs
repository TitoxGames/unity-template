using DG.Tweening;
using UnityEngine;

namespace GameTemplate.Runtime.WGUI.Components
{
    public class PanelAnimation : MonoBehaviour
    {
        public enum AnimationType
        {
            FromTop,
            FromBottom,
            FromLeft,
            FromRight,
            Scale,
            FadeIn,
            FadeOut
        }

        [Header("Animation")]
        [SerializeField] private AnimationType animationType = AnimationType.FromTop;

        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float delay = 0f;

        [SerializeField] private Ease ease = Ease.OutBack;

        [Header("Settings")]
        [SerializeField] private bool playOnEnable = true;

        [SerializeField] private bool deactivateOnHide = false;

        [Header("Distance")]
        [Tooltip("Khoảng cách panel di chuyển khi bắt đầu animation")]
        [SerializeField] private float offset = 1000f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private Vector2 originalPosition;
        private Vector3 originalScale;
        private Vector2 targetPosition;

        private Sequence currentSequence;

        public bool IsAnimating => currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying();

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            EnsureCanvasGroup();

            originalPosition = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale;
            targetPosition = originalPosition;
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Show();
        }

        private void OnDisable()
        {
            KillTweens();
            ResetVisualState();
        }

        /// <summary>
        /// Hiển thị panel với animation
        /// </summary>
        public void Show()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            EnsureCanvasGroup();
            KillTweens();

            targetPosition = originalPosition;
            rectTransform.anchoredPosition = originalPosition;
            gameObject.SetActive(true);

            PrepareShowStartState();

            currentSequence = DOTween.Sequence()
                .SetDelay(delay)
                .SetUpdate(true);

            AppendShowAnimation(currentSequence);
        }

        /// <summary>
        /// Ẩn panel với animation
        /// </summary>
        public void Hide()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            EnsureCanvasGroup();
            KillTweens();

            currentSequence = DOTween.Sequence()
                .SetDelay(delay)
                .SetUpdate(true);

            AppendHideAnimation(currentSequence);

            currentSequence.OnComplete(() =>
            {
                if (deactivateOnHide)
                    gameObject.SetActive(false);
            });
        }

        private void PrepareShowStartState()
        {
            switch (animationType)
            {
                case AnimationType.FromTop:
                case AnimationType.FromBottom:
                case AnimationType.FromLeft:
                case AnimationType.FromRight:
                    rectTransform.anchoredPosition = GetSlideStartPosition();
                    break;

                case AnimationType.Scale:
                    rectTransform.localScale = Vector3.zero;
                    break;

                case AnimationType.FadeIn:
                    canvasGroup.alpha = 0f;
                    break;

                case AnimationType.FadeOut:
                    canvasGroup.alpha = 1f;
                    break;
            }
        }

        private void AppendShowAnimation(Sequence sequence)
        {
            switch (animationType)
            {
                case AnimationType.FromTop:
                case AnimationType.FromBottom:
                case AnimationType.FromLeft:
                case AnimationType.FromRight:
                    sequence.Join(rectTransform
                        .DOAnchorPos(targetPosition, duration)
                        .SetEase(ease));
                    break;

                case AnimationType.Scale:
                    sequence.Join(rectTransform
                        .DOScale(originalScale, duration)
                        .SetEase(ease));
                    break;

                case AnimationType.FadeIn:
                    sequence.Join(canvasGroup
                        .DOFade(1f, duration)
                        .SetEase(ease));
                    break;

                case AnimationType.FadeOut:
                    break;
            }
        }

        private void AppendHideAnimation(Sequence sequence)
        {
            switch (animationType)
            {
                case AnimationType.FromTop:
                case AnimationType.FromBottom:
                case AnimationType.FromLeft:
                case AnimationType.FromRight:
                    sequence.Join(rectTransform
                        .DOAnchorPos(GetSlideStartPosition(), duration)
                        .SetEase(ease));
                    break;

                case AnimationType.Scale:
                    sequence.Join(rectTransform
                        .DOScale(Vector3.zero, duration)
                        .SetEase(ease));
                    break;

                case AnimationType.FadeIn:
                    sequence.Join(canvasGroup
                        .DOFade(0f, duration)
                        .SetEase(ease));
                    break;

                case AnimationType.FadeOut:
                    sequence.Join(canvasGroup
                        .DOFade(0f, duration)
                        .SetEase(ease));
                    break;
            }
        }

        private Vector2 GetSlideStartPosition()
        {
            var position = targetPosition;

            switch (animationType)
            {
                case AnimationType.FromTop:
                    position.y += offset;
                    break;

                case AnimationType.FromBottom:
                    position.y -= offset;
                    break;

                case AnimationType.FromLeft:
                    position.x -= offset;
                    break;

                case AnimationType.FromRight:
                    position.x += offset;
                    break;
            }

            return position;
        }

        /// <summary>
        /// Reset panel về vị trí ban đầu
        /// </summary>
        public void ResetPosition()
        {
            KillTweens();

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            targetPosition = originalPosition;
            ResetVisualState();
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup != null)
                return;

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void KillTweens()
        {
            currentSequence?.Kill();
            currentSequence = null;
        }

        private void ResetVisualState()
        {
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = originalPosition;
                rectTransform.localScale = originalScale;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }
    }
}
