using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GameTemplate.Runtime.WGUI.Components
{
    /// <summary>
    /// Copies reward icons where they sit, then lifts each copy, holds it and fades it out. The copies live under this
    /// transform so they draw above masks and scroll views; they are removed when it is disabled.
    /// </summary>
    public class RewardIconRiseEffect : MonoBehaviour
    {
        [SerializeField] private float riseDistance = 160f;
        [SerializeField] private float riseDuration = 0.45f;
        [SerializeField] private float riseScale = 1.25f;
        [SerializeField] private float holdDuration = 0.4f;
        [SerializeField] private float fadeDuration = 0.45f;
        [Tooltip("How much higher the icon drifts while it fades out.")]
        [SerializeField] private float fadeRiseDistance = 80f;
        [Tooltip("Delay between consecutive icons of one reward.")]
        [SerializeField] private float stagger = 0.08f;

        private readonly List<GameObject> copies = new();

        private void OnDisable()
        {
            foreach (GameObject copy in copies)
            {
                if (copy != null)
                {
                    Destroy(copy);
                }
            }

            copies.Clear();
        }

        public void Play(IReadOnlyList<Image> icons)
        {
            for (int i = 0; i < icons.Count; i++)
            {
                PlayIcon(icons[i], i * stagger);
            }
        }

        private void PlayIcon(Image source, float delay)
        {
            var copyObject = new GameObject($"Rising{source.name}", typeof(RectTransform), typeof(Image));
            copies.Add(copyObject);

            var copy = (RectTransform)copyObject.transform;
            copy.SetParent(transform, false);
            copy.SetAsLastSibling();
            RectTransform sourceRect = source.rectTransform;
            copy.sizeDelta = sourceRect.rect.size * (sourceRect.lossyScale.x / transform.lossyScale.x);
            copy.position = sourceRect.TransformPoint(sourceRect.rect.center);

            var image = copyObject.GetComponent<Image>();
            image.sprite = source.sprite;
            image.preserveAspect = source.preserveAspect;
            image.raycastTarget = false;

            float unit = transform.lossyScale.y;
            Vector3 risePosition = copy.position + Vector3.up * (riseDistance * unit);
            Vector3 fadePosition = risePosition + Vector3.up * (fadeRiseDistance * unit);
            DOTween.Sequence()
                .AppendInterval(delay)
                .Append(copy.DOMove(risePosition, riseDuration).SetEase(Ease.OutCubic))
                .Join(copy.DOScale(riseScale, riseDuration).SetEase(Ease.OutBack))
                .AppendInterval(holdDuration)
                .Append(copy.DOMove(fadePosition, fadeDuration).SetEase(Ease.InQuad))
                .Join(image.DOFade(0f, fadeDuration).SetEase(Ease.InQuad))
                .SetUpdate(true)
                .SetLink(copyObject)
                .OnComplete(() =>
                {
                    copies.Remove(copyObject);
                    Destroy(copyObject);
                });
        }
    }
}
