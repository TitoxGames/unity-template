using UnityEngine;

namespace TitoX.GameData
{
    [CreateAssetMenu(
        fileName = "BoosterConfig",
        menuName = "TitoX/Booster Config")]
    public class BoosterConfigSO : ScriptableObject
    {
        public string Id;

        public Sprite Icon;

        public string DisplayName;

        [TextArea]
        public string Description;

        public int DefaultAmount;
    }
}