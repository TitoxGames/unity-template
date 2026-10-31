using Cysharp.Threading.Tasks;
using GameTemplate.Runtime.WGUI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace GameTemplate.Runtime.WGUI.Flow
{
    /// <summary>
    /// Continue runs <see cref="BeforeContinueAsync"/> (reward animation...), the interstitial, then moves to the next level.
    /// Subclasses add the reward itself, e.g. a rewarded-ad multiplier that ends with <see cref="Continue"/>.
    /// </summary>
    public class WinScreenBase : UIBase
    {
        [SerializeField] protected Button continueButton;
        [Tooltip("Interstitial placement offered after Continue. Empty shows none.")]
        [SerializeField] private string continueInterstitialPlacement = "LevelComplete";

        private bool isContinuing;

        protected override void Awake()
        {
            base.Awake();
            UIFeedback.BindClicks(this);
            ButtonContentTint.Bind(this);
            continueButton.onClick.AddListener(HandleContinueClicked);
        }

        protected override void OnDestroy()
        {
            continueButton.onClick.RemoveListener(HandleContinueClicked);
            base.OnDestroy();
        }

        protected override void InitData()
        {
            isContinuing = false;
            SetButtonsInteractable(true);
            UIFeedback.Haptic(HapticStrength.Medium);
        }

        private void HandleContinueClicked()
        {
            UIFeedback.Haptic();
            Continue();
        }

        protected void Continue()
        {
            if (isContinuing)
                return;

            ContinueAsync().Forget();
        }

        private async UniTaskVoid ContinueAsync()
        {
            isContinuing = true;
            SetButtonsInteractable(false);

            await BeforeContinueAsync();
            await UIAds.TryShowInterstitialAsync(continueInterstitialPlacement);

            if (!UIFlow.Next())
            {
                isContinuing = false;
                SetButtonsInteractable(true);
            }
        }

        protected virtual UniTask BeforeContinueAsync()
        {
            return UniTask.CompletedTask;
        }

        protected virtual void SetButtonsInteractable(bool interactable)
        {
            continueButton.interactable = interactable;
        }
    }
}
