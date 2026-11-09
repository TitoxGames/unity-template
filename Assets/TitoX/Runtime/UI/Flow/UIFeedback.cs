using UnityEngine;
using UnityEngine.UI;

namespace TitoX.UI.Flow
{
    public enum HapticStrength
    {
        Light,
        Medium,
        Heavy,
    }

    /// <summary>Game-side click sound and haptics, so template screens never reference a sound manager or haptic plugin.</summary>
    public interface IUIFeedback
    {
        void PlayClick();

        void PlayHaptic(HapticStrength strength);
    }

    public static class UIFeedback
    {
        /// <summary>Set once at startup by the game. Feedback is silent while null.</summary>
        public static IUIFeedback Current { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
        }

        public static void Click()
        {
            Current?.PlayClick();
        }

        public static void Haptic(HapticStrength strength = HapticStrength.Light)
        {
            Current?.PlayHaptic(strength);
        }

        /// <summary>Plays the click sound for every Button under <paramref name="root"/>, including inactive ones.</summary>
        public static void BindClicks(Component root)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                button.onClick.RemoveListener(Click);
                button.onClick.AddListener(Click);
            }
        }

        public static void UnbindClick(Button button)
        {
            if (button != null)
                button.onClick.RemoveListener(Click);
        }
    }
}
