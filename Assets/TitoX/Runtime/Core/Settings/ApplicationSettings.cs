using UnityEngine;

namespace TitoX.Core.Settings
{
    [CreateAssetMenu(fileName = "ApplicationSettings", menuName = "TitoX/Application Settings")]
    public class ApplicationSettings : ScriptableObject
    {
        [Header("Performance Settings")]
        [SerializeField] private int targetFPS = 60;
        
        // Properties
        public int TargetFPS => targetFPS;
        
        private void OnValidate()
        {
            // Ensure target FPS is reasonable
            targetFPS = Mathf.Clamp(targetFPS, 30, 120);
        }
    }
}