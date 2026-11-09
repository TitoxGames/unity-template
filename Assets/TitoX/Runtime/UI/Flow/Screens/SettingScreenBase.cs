using TitoX.Core.Settings;
using TitoX.UI.Components;
using UnityEngine;
using UnityEngine.UI;

namespace TitoX.UI.Flow
{
    /// <summary>Sound, music and vibration toggles saved in <see cref="SettingManager"/>; Replay and Home only show inside a level.</summary>
    public class SettingScreenBase : UIBase
    {
        [SerializeField] protected SettingToggle soundToggle;
        [SerializeField] protected SettingToggle musicToggle;
        [SerializeField] protected SettingToggle vibrationToggle;
        [Tooltip("Optional. Shown only inside a level.")]
        [SerializeField] protected Button replayButton;
        [Tooltip("Optional. Shown only inside a level.")]
        [SerializeField] protected Button homeButton;

        protected override void Awake()
        {
            base.Awake();
            UIFeedback.BindClicks(this);
            ButtonContentTint.Bind(this);

            if (soundToggle != null)
            {
                // Clicked after the setting is applied, so turning sound on is audible and turning it off is silent.
                UIFeedback.UnbindClick(soundToggle.Button);
                soundToggle.OnValueChanged += HandleSoundChanged;
            }

            if (musicToggle != null)
                musicToggle.OnValueChanged += HandleMusicChanged;

            if (vibrationToggle != null)
                vibrationToggle.OnValueChanged += HandleVibrationChanged;

            if (replayButton != null)
                replayButton.onClick.AddListener(HandleReplayClicked);

            if (homeButton != null)
                homeButton.onClick.AddListener(HandleHomeClicked);
        }

        protected override void OnDestroy()
        {
            if (soundToggle != null)
                soundToggle.OnValueChanged -= HandleSoundChanged;

            if (musicToggle != null)
                musicToggle.OnValueChanged -= HandleMusicChanged;

            if (vibrationToggle != null)
                vibrationToggle.OnValueChanged -= HandleVibrationChanged;

            if (replayButton != null)
                replayButton.onClick.RemoveListener(HandleReplayClicked);

            if (homeButton != null)
                homeButton.onClick.RemoveListener(HandleHomeClicked);

            base.OnDestroy();
        }

        protected override void InitData()
        {
            SettingManager settings = SettingManager.Instance;
            if (soundToggle != null)
                soundToggle.SetValue(settings.Sfx);

            if (musicToggle != null)
                musicToggle.SetValue(settings.Music);

            if (vibrationToggle != null)
                vibrationToggle.SetValue(settings.Haptic);

            bool isInLevel = UIFlow.State == UIFlowState.Gameplay;
            SetLevelButtonsVisible(isInLevel);
            SetLevelButtonsInteractable(true);
        }

        protected virtual void SetLevelButtonsVisible(bool visible)
        {
            if (replayButton != null)
                replayButton.gameObject.SetActive(visible);

            if (homeButton != null)
                homeButton.gameObject.SetActive(visible);
        }

        protected virtual void HandleSoundChanged(bool isOn)
        {
            SettingManager.Instance.SetSfx(isOn);
            UIFeedback.Click();
        }

        protected virtual void HandleMusicChanged(bool isOn)
        {
            SettingManager.Instance.SetMusic(isOn);
        }

        protected virtual void HandleVibrationChanged(bool isOn)
        {
            SettingManager.Instance.SetHaptic(isOn);
            UIFeedback.Haptic();
        }

        protected virtual void HandleReplayClicked()
        {
            UIFeedback.Haptic();
            SetLevelButtonsInteractable(false);
            if (!UIFlow.Retry())
                SetLevelButtonsInteractable(true);
        }

        protected virtual void HandleHomeClicked()
        {
            UIFeedback.Haptic();
            SetLevelButtonsInteractable(false);
            if (!UIFlow.Home())
                SetLevelButtonsInteractable(true);
        }

        private void SetLevelButtonsInteractable(bool interactable)
        {
            if (replayButton != null)
                replayButton.interactable = interactable;

            if (homeButton != null)
                homeButton.interactable = interactable;
        }
    }
}
