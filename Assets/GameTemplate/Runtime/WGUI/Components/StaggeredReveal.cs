using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace GameTemplate.Runtime.WGUI.Components
{
    /// <summary>On enable, reveals the active direct children one after another in sibling order. Each blocks raycasts only once it is in place.</summary>
    public class StaggeredReveal : MonoBehaviour
    {
        private sealed class Item
        {
            public RectTransform Rect;
            public CanvasGroup Group;
            public Vector2 Position;
            public Vector3 Scale;
        }

        [SerializeField] private float startDelay;
        [Tooltip("Seconds between the starts of two consecutive children.")]
        [SerializeField] private float interval = 0.15f;
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private float startScale = 0.7f;
        [Tooltip("Each child starts this far below its place and rises into it.")]
        [SerializeField] private float riseDistance = 60f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        private readonly List<Item> items = new();
        private Sequence sequence;

        private void Awake()
        {
            foreach (Transform child in transform)
            {
                if (child is not RectTransform rect)
                {
                    continue;
                }

                if (!rect.TryGetComponent(out CanvasGroup group))
                {
                    group = rect.gameObject.AddComponent<CanvasGroup>();
                }

                items.Add(new Item { Rect = rect, Group = group, Position = rect.anchoredPosition, Scale = rect.localScale });
            }
        }

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            sequence?.Kill();
            Restore();
        }

        private void Play()
        {
            sequence?.Kill();
            sequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            float startTime = startDelay;
            foreach (Item item in items)
            {
                if (!item.Rect.gameObject.activeSelf)
                {
                    continue;
                }

                item.Group.alpha = 0f;
                item.Group.blocksRaycasts = false;
                item.Rect.localScale = item.Scale * startScale;
                item.Rect.anchoredPosition = item.Position + Vector2.down * riseDistance;

                sequence.Insert(startTime, item.Group.DOFade(1f, duration * 0.6f).SetEase(Ease.OutQuad));
                sequence.Insert(startTime, item.Rect.DOScale(item.Scale, duration).SetEase(scaleEase));
                sequence.Insert(startTime, item.Rect.DOAnchorPos(item.Position, duration).SetEase(Ease.OutCubic));
                sequence.InsertCallback(startTime + duration, () => item.Group.blocksRaycasts = true);
                startTime += interval;
            }
        }

        private void Restore()
        {
            foreach (Item item in items)
            {
                item.Group.alpha = 1f;
                item.Group.blocksRaycasts = true;
                item.Rect.localScale = item.Scale;
                item.Rect.anchoredPosition = item.Position;
            }
        }
    }
}
