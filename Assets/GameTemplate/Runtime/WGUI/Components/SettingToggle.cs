using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTemplate.Runtime.WGUI.Components
{
    public class SettingToggle : MonoBehaviour
    {
        [SerializeField] private Button button;
        [Tooltip("Graphics darkened by offTint while the setting is off.")]
        [SerializeField] private Graphic[] graphics;
        [SerializeField] private Color offTint = new Color(0.4f, 0.4f, 0.4f, 1f);

        private bool isOn;
        private Color[] onColors;

        public event Action<bool> OnValueChanged;

        public Button Button => button;

        public bool IsOn => isOn;

        private void Awake()
        {
            button.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClicked);
        }

        /// <summary>Updates the visuals without raising <see cref="OnValueChanged"/>.</summary>
        public void SetValue(bool value)
        {
            isOn = value;
            onColors ??= Array.ConvertAll(graphics, graphic => graphic.color);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].color = value ? onColors[i] : onColors[i] * offTint;
            }
        }

        private void HandleClicked()
        {
            SetValue(!isOn);
            OnValueChanged?.Invoke(isOn);
        }
    }
}
