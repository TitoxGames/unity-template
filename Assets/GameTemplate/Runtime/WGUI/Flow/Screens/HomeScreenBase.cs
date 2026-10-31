using GameTemplate.Runtime.WGUI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameTemplate.Runtime.WGUI.Flow
{
    public class HomeScreenBase : UIBase
    {
        [SerializeField] protected Button playButton;
        [Tooltip("Optional.")]
        [SerializeField] protected TMP_Text levelText;
        [SerializeField] private string levelFormat = "Level {0}";

        protected override void Awake()
        {
            base.Awake();
            UIFeedback.BindClicks(this);
            ButtonContentTint.Bind(this);
            playButton.onClick.AddListener(HandlePlayClicked);
        }

        protected override void OnDestroy()
        {
            playButton.onClick.RemoveListener(HandlePlayClicked);
            base.OnDestroy();
        }

        protected override void InitData()
        {
            playButton.interactable = true;
            RefreshLevel(UIFlow.CurrentLevel);
        }

        protected virtual void RefreshLevel(int level)
        {
            if (levelText != null)
                levelText.text = string.Format(levelFormat, level);
        }

        protected virtual void HandlePlayClicked()
        {
            UIFeedback.Haptic();
            playButton.interactable = false;
            if (!UIFlow.Play())
                playButton.interactable = true;
        }
    }
}
