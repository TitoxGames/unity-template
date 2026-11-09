using UnityEngine;
using UnityEngine.UI;

namespace TitoX.UI.Components
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BackgroundFullScreen : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool centerOnCamera = true;

        private RectTransform rectTransform;
        private Image image;
        private SpriteRenderer spriteRenderer;

        private DrivenRectTransformTracker drivenTracker;
        private Vector2 lastAreaSize;
        private Vector2 lastSpriteSize;

        private void OnEnable()
        {
            rectTransform = transform as RectTransform;
            image = GetComponent<Image>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            lastAreaSize = Vector2.zero;

            if (rectTransform != null)
            {
                drivenTracker.Add(this, rectTransform,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.Pivot |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.SizeDelta);
            }

            Refresh();
        }

        private void OnDisable()
        {
            drivenTracker.Clear();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (rectTransform != null)
            {
                FitRectTransform();
            }
            else if (spriteRenderer != null)
            {
                FitSpriteRenderer();
            }
        }

        private void FitRectTransform()
        {
            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent == null)
                return;

            Vector2 spriteSize = GetSpriteSize(image != null ? image.sprite : null);
            Vector2 areaSize = parent.rect.size;
            if (!HasChanged(areaSize, spriteSize))
                return;

            Vector2 center = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = center;
            rectTransform.anchorMax = center;
            rectTransform.pivot = center;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = CalculateCoverSize(areaSize, spriteSize);
        }

        private void FitSpriteRenderer()
        {
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null)
                return;

            Vector2 spriteSize = GetSpriteSize(spriteRenderer.sprite, true);
            Vector2 areaSize = GetCameraViewSize(cam);
            if (!HasChanged(areaSize, spriteSize))
                return;

            Vector2 coverSize = CalculateCoverSize(areaSize, spriteSize);
            float scale = coverSize.x / spriteSize.x;
            transform.localScale = new Vector3(scale, scale, transform.localScale.z);

            if (centerOnCamera)
            {
                Vector3 camPosition = cam.transform.position;
                transform.position = new Vector3(camPosition.x, camPosition.y, transform.position.z);
            }
        }

        private Vector2 GetCameraViewSize(Camera cam)
        {
            float height;
            if (cam.orthographic)
            {
                height = cam.orthographicSize * 2f;
            }
            else
            {
                float distance = Mathf.Abs(transform.position.z - cam.transform.position.z);
                height = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            }

            return new Vector2(height * cam.aspect, height);
        }

        private bool HasChanged(Vector2 areaSize, Vector2 spriteSize)
        {
            if (areaSize.x <= 0f || areaSize.y <= 0f || spriteSize.x <= 0f || spriteSize.y <= 0f)
                return false;

            if (areaSize == lastAreaSize && spriteSize == lastSpriteSize)
                return false;

            lastAreaSize = areaSize;
            lastSpriteSize = spriteSize;
            return true;
        }

        private static Vector2 GetSpriteSize(Sprite sprite, bool inWorldUnits = false)
        {
            if (sprite == null)
                return Vector2.zero;

            return inWorldUnits ? (Vector2)sprite.bounds.size : sprite.rect.size;
        }

        private static Vector2 CalculateCoverSize(Vector2 areaSize, Vector2 spriteSize)
        {
            float scale = Mathf.Max(areaSize.x / spriteSize.x, areaSize.y / spriteSize.y);
            return spriteSize * scale;
        }
    }
}
