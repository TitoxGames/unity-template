using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TitoX.UI
{
    public class UIGraphicScale : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler,
        IPointerExitHandler, IPointerClickHandler
    {
        [System.Serializable]
        public class Config
        {
            [SerializeField] private Transform _target;

            [SerializeField] private Vector3 _endValue = new Vector3(0.9f, 0.9f, 1.0f);

            [SerializeField] private float _duration = 0.1f;

            [SerializeField] private Ease _ease = Ease.Linear;

            public Transform Target => _target;
            public Vector3 EndValue => _endValue;
            public float Duration => _duration;
            public Ease Ease => _ease;
        }

        [Title("Config")]
        [SerializeField] private Config _config;

        private bool _isDown = false;

        private Tween _tween;

        #region Monobehaviour

        private void OnDestroy()
        {
            _tween?.Kill();
        }

        private void OnDisable()
        {
            _tween?.Restart();
            _tween?.Kill();
            _tween = null;
        }

        #endregion

        #region Function -> Private

        private void InitTween()
        {
            if (_tween != null)
                return;

            Transform target = _config.Target == null ? transform : _config.Target;

            Vector3 scaleStart = target.localScale;

            Vector3 scaleEnd = new Vector3(scaleStart.x * _config.EndValue.x, scaleStart.y * _config.EndValue.y,
                scaleStart.z * _config.EndValue.z);

            _tween = target.DOScale(scaleEnd, _config.Duration)
                .ChangeStartValue(scaleStart)
                .SetEase(_config.Ease)
                .SetAutoKill(false)
                .SetUpdate(true);

            _tween.Restart();
            _tween.Pause();
        }

        private void ScaleUp()
        {
            InitTween();

            _tween.PlayForward();
        }

        private void ScaleDown()
        {
            InitTween();

            _tween.PlayBackwards();
        }

        #endregion

        #region Function -> Public

        public void Construct(Config config)
        {
            _config = config;
        }

        #endregion

        #region IPointer interfaces implement

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _isDown = true;

            ScaleUp();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (_isDown)
                ScaleDown();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_isDown)
                ScaleUp();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            _isDown = false;

            ScaleDown();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
        }

        #endregion
    }
}