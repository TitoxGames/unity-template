using System;
using DG.Tweening;
using TitoX.UI.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace TitoX.UI.Components
{
    /// <summary>One tab of <see cref="HomeBottomBarScreenBase"/>: shows the selected background and label, and lifts and grows the icon.</summary>
    [RequireComponent(typeof(Button))]
    public class HomeTabButton : MonoBehaviour
    {
        [SerializeField] private HomeTab tab;
        [SerializeField] private GameObject selectedBackground;
        [SerializeField] private RectTransform icon;
        [Tooltip("Optional. Only shown while the tab is selected.")]
        [SerializeField] private GameObject label;
        [SerializeField] private float selectedIconScale = 1.25f;
        [SerializeField] private float selectedIconRaise = 30f;
        [SerializeField] private float duration = 0.2f;

        private Button button;
        private Vector2 iconRestPosition;

        public event Action<HomeTab> Clicked;

        public HomeTab Tab => tab;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClicked);

            if (icon != null)
                iconRestPosition = icon.anchoredPosition;
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClicked);

            if (icon != null)
                icon.DOKill();
        }

        public void SetSelected(bool selected, bool animate)
        {
            if (selectedBackground != null)
                selectedBackground.SetActive(selected);

            if (label != null)
                label.SetActive(selected);

            if (icon == null)
                return;

            float scale = selected ? selectedIconScale : 1f;
            Vector2 position = selected ? iconRestPosition + Vector2.up * selectedIconRaise : iconRestPosition;

            icon.DOKill();
            if (!animate)
            {
                icon.localScale = Vector3.one * scale;
                icon.anchoredPosition = position;
                return;
            }

            icon.DOScale(scale, duration).SetEase(Ease.OutBack);
            icon.DOAnchorPos(position, duration).SetEase(Ease.OutQuad);
        }

        private void HandleClicked()
        {
            Clicked?.Invoke(tab);
        }
    }
}
