using TitoX.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace TitoX.UI.Flow
{
    public class LoseScreenBase : UIBase
    {
        [SerializeField] protected Button retryButton;
        [Tooltip("Optional.")]
        [SerializeField] protected Button homeButton;

        protected override void Awake()
        {
            base.Awake();
            UIFeedback.BindClicks(this);
            ButtonContentTint.Bind(this);
            retryButton.onClick.AddListener(HandleRetryClicked);
            if (homeButton != null)
                homeButton.onClick.AddListener(HandleHomeClicked);
        }

        protected override void OnDestroy()
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            if (homeButton != null)
                homeButton.onClick.RemoveListener(HandleHomeClicked);

            base.OnDestroy();
        }

        protected override void InitData()
        {
            SetButtonsInteractable(true);
        }

        protected virtual void HandleRetryClicked()
        {
            UIFeedback.Haptic();
            SetButtonsInteractable(false);
            if (!UIFlow.Retry())
                SetButtonsInteractable(true);
        }

        protected virtual void HandleHomeClicked()
        {
            UIFeedback.Haptic();
            SetButtonsInteractable(false);
            if (!UIFlow.Home())
                SetButtonsInteractable(true);
        }

        protected virtual void SetButtonsInteractable(bool interactable)
        {
            retryButton.interactable = interactable;
            if (homeButton != null)
                homeButton.interactable = interactable;
        }
    }
}
