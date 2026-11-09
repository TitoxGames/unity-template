using UnityEngine;

namespace TitoX.GameData
{
    [CreateAssetMenu(menuName = "TitoX/Currency Config", fileName = "Currency Config")]
    public class CurrencyConfigSO: ScriptableObject
    {
        public CurrencyType CurrencyType;
        public Sprite Sprite;
    }
}