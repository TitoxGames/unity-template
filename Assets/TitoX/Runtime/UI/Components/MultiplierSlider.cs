using System;
using UnityEngine;

namespace TitoX.UI.Components
{
    /// <summary>Pointer ping-pongs across equally sized segments; the segment under it is the current multiplier.</summary>
    public class MultiplierSlider : MonoBehaviour
    {
        [SerializeField] private RectTransform track;
        [SerializeField] private RectTransform pointer;
        [SerializeField] private int[] multipliers = { 2, 3, 5, 3, 2 };
        [Tooltip("Seconds for the pointer to travel from one end of the track to the other.")]
        [SerializeField] private float sweepDuration = 1f;

        private float elapsed;

        public event Action<int> MultiplierChanged;

        public int CurrentMultiplier { get; private set; }
        public bool IsRunning { get; private set; }

        public void Play()
        {
            elapsed = 0f;
            CurrentMultiplier = 0;
            IsRunning = true;
            UpdatePointer();
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Resume()
        {
            IsRunning = true;
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            elapsed += Time.unscaledDeltaTime;
            UpdatePointer();
        }

        private void UpdatePointer()
        {
            float t = Mathf.PingPong(elapsed / Mathf.Max(sweepDuration, 0.01f), 1f);
            float halfWidth = track.rect.width * 0.5f;
            pointer.anchoredPosition = new Vector2(Mathf.Lerp(-halfWidth, halfWidth, t), pointer.anchoredPosition.y);

            int index = Mathf.Min((int)(t * multipliers.Length), multipliers.Length - 1);
            int multiplier = multipliers[index];
            if (multiplier == CurrentMultiplier)
            {
                return;
            }

            CurrentMultiplier = multiplier;
            MultiplierChanged?.Invoke(multiplier);
        }
    }
}
