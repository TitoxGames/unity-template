using Sirenix.OdinInspector;
using UnityEngine;

namespace TitoX.UI
{

    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [Title("Config")]
        [SerializeField] private bool _conformX = true;

        [SerializeField] private bool _conformY = true;

        [Space]
        [SerializeField] private bool _refreshOnUpdate = false;

        private RectTransform _rectTransform = null!;

        private Rect _lastSafeArea = Rect.zero;

        private Vector2Int _lastScreenSize = Vector2Int.zero;

        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;


        public RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        protected void OnEnable()
        {

            Refresh();
        }

        protected void Update()
        {

            if (_refreshOnUpdate)
                Refresh();
        }

        private void Refresh()
        {
            Rect safeArea = Screen.safeArea;

            if (IsSafeAreaUnchanged(safeArea))
                return;

            _lastScreenSize.x = Screen.width;
            _lastScreenSize.y = Screen.height;
            _lastOrientation = Screen.orientation;

            ApplySafeArea(safeArea);
        }

        private void ApplySafeArea(Rect r)
        {
            _lastSafeArea = r;

            if (!_conformX)
            {
                r.x = 0;
                r.width = Screen.width;
            }

            if (!_conformY)
            {
                r.y = 0;
                r.height = Screen.height;
            }

            if (Screen.width > 0 && Screen.height > 0)
            {
                Vector2 anchorMin = r.position;
                Vector2 anchorMax = r.position + r.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                if (IsAnchorValid(anchorMin) && IsAnchorValid(anchorMax))
                {
                    RectTransform.anchorMin = anchorMin;
                    RectTransform.anchorMax = anchorMax;
                }
            }

        }

        private bool IsSafeAreaUnchanged(Rect safeArea)
        {
            return safeArea == _lastSafeArea
                   && Screen.width == _lastScreenSize.x
                   && Screen.height == _lastScreenSize.y
                   && Screen.orientation == _lastOrientation;
        }

        private static bool IsAnchorValid(Vector2 anchor)
        {
            return anchor.x >= 0f && anchor.y >= 0f && !float.IsNaN(anchor.x) && !float.IsNaN(anchor.y);
        }
    }
}