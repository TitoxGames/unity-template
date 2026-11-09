using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WPuzzle
{
    [CreateAssetMenu(menuName = "WLevel/Level Configs")]
    public class LevelConfigSOs : ScriptableObject
    {
        [TableList] [SerializeField] private List<LevelConfig> _levels;

        public List<LevelConfig> Levels => _levels;

        private void OnValidate()
        {
            for (int i = 0; i < _levels.Count; i++)
            {
                _levels[i].SetLevelNumber(i + 1);
            }
        }
    }

    [Serializable]
    public class LevelConfig
    {
        [SerializeField] private int _levelNumber;
        [SerializeField] private AssetReferenceT<LevelDataSO> _gameData;

        public int LevelNumber => _levelNumber;
        public AssetReferenceT<LevelDataSO> GameData => _gameData;

        public void SetLevelNumber(int number)
        {
            _levelNumber = number;
        }
    }
}