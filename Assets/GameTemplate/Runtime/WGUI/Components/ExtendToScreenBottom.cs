using UnityEngine;

namespace GameTemplate.Runtime.WGUI.Components
{
    /// <summary>
    /// Stretches the bottom edge of each target down to the bottom of the screen,
    /// filling the area below the safe area. Targets must have their bottom anchored
    /// to this object's bottom edge.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ExtendToScreenBottom : MonoBehaviour
    {
        [SerializeField] private RectTransform[] targets;

        private readonly Vector3[] rootCorners = new Vector3[4];
        private Canvas canvas;
        private float lastExtension = -1f;

        private void OnEnable()
        {
            canvas = GetComponentInParent<Canvas>();
            lastExtension = -1f;
        }

        private void LateUpdate()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                    return;
            }

            RectTransform rectTransform = (RectTransform)transform;
            RectTransform root = (RectTransform)canvas.rootCanvas.transform;
            root.GetWorldCorners(rootCorners);
            float screenBottom = rectTransform.InverseTransformPoint(rootCorners[0]).y;
            float extension = Mathf.Max(0f, rectTransform.rect.yMin - screenBottom);

            if (Mathf.Approximately(extension, lastExtension))
                return;

            lastExtension = extension;
            foreach (RectTransform target in targets)
            {
                if (target == null)
                    continue;

                Vector2 offsetMin = target.offsetMin;
                offsetMin.y = -extension;
                target.offsetMin = offsetMin;
            }
        }
    }
}
