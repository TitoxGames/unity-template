using UnityEngine;

namespace GameTemplate.Runtime.WGUI.Components
{
    /// <summary>Uniformly scales a centered rect of <see cref="designSize"/> so it always covers its parent, cropping the overflow.</summary>
    [RequireComponent(typeof(RectTransform))]
    public class ScaleToCoverParent : MonoBehaviour
    {
        [SerializeField] private Vector2 designSize = new Vector2(1080f, 1920f);
        [Tooltip("Applied on top of the cover scale; above 1 crops more.")]
        [SerializeField, Min(0.01f)] private float scaleMultiplier = 1f;

        private RectTransform rectTransform;
        private Vector2 lastParentSize;

        private void OnEnable()
        {
            rectTransform = (RectTransform)transform;
            lastParentSize = Vector2.zero;
            Refresh();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (!(rectTransform.parent is RectTransform parent))
            {
                return;
            }

            Vector2 parentSize = parent.rect.size;
            if (parentSize == lastParentSize || parentSize.x <= 0f || parentSize.y <= 0f)
            {
                return;
            }

            lastParentSize = parentSize;
            rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = designSize;
            float scale = Mathf.Max(parentSize.x / designSize.x, parentSize.y / designSize.y) * scaleMultiplier;
            rectTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
