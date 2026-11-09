using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TitoX.Utils
{
    public class AudioSourceVariable: MonoBehaviour
    { 
        [Required]
        [SerializeField] private AudioSource source;
        [SerializeField] private Vector2 pitchRange;

        [SerializeField] private bool constantPitchChange = false;
        [SerializeField] private bool limitMultiplePlays = false;

        [ShowIf(nameof(limitMultiplePlays))]
        [SerializeField] private float timeBetweenPlays = 0.1f;

        [ShowIf(nameof(limitMultiplePlays))]
        [SerializeField] private string uniqueID;

        [SerializeField] private bool incrementPitch = false;

        [ShowIf(nameof(incrementPitch))]
        [SerializeField] private string uniquePitchID;

        [ShowIf(nameof(incrementPitch))]
        [SerializeField] private float pitchIncrement = 0.1f;

        [ShowIf(nameof(incrementPitch))]
        [SerializeField] private float pitchIncrementMax = 0.5f;

        [ShowIf(nameof(incrementPitch))]
        [SerializeField] private float pitchIncrementMaxTime = 2f;

        private static readonly Dictionary<string, float> dictionaryTimer = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> dictionaryPitchScaleTime = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> dictionaryPitchNumber = new Dictionary<string, float>();

        private void Awake()
        {
            source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        }

        private void Update()
        {
            if (constantPitchChange)
                source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        }

        private void OnValidate()
        {
            if (source == null)
                source = GetComponent<AudioSource>();
        }

        public void Play()
        {
            if (limitMultiplePlays)
            {
                if (!dictionaryTimer.ContainsKey(uniqueID))
                    dictionaryTimer.Add(uniqueID, 0f);

                if (Time.time - dictionaryTimer[uniqueID] <= timeBetweenPlays)
                    return;

                dictionaryTimer[uniqueID] = Time.time;
            }

            float pitchAdder = 0f;

            if (incrementPitch)
            {
                if (!dictionaryPitchScaleTime.ContainsKey(uniquePitchID))
                    dictionaryPitchScaleTime.Add(uniquePitchID, Time.time);

                if (Time.time - dictionaryPitchScaleTime[uniquePitchID] >= pitchIncrementMaxTime)
                {
                    dictionaryPitchNumber.Remove(uniquePitchID);
                }

                if (!dictionaryPitchNumber.ContainsKey(uniquePitchID))
                    dictionaryPitchNumber.Add(uniquePitchID, 0f);

                pitchAdder = dictionaryPitchNumber[uniquePitchID];
                dictionaryPitchNumber[uniquePitchID] = Mathf.Min(dictionaryPitchNumber[uniquePitchID] + pitchIncrement, pitchIncrementMax);
                dictionaryPitchScaleTime[uniquePitchID] = Time.time;
            }

            source.pitch = Random.Range(pitchRange.x, pitchRange.y) + pitchAdder;
            source.Play();
        }
    }
}