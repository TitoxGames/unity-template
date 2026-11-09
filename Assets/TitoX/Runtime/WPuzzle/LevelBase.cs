using System;
using UnityEngine;

namespace WPuzzle
{
    public class LevelBase : MonoBehaviour
    {
        protected int _levelNumber;
        protected LevelDataSO _levelData;
        
        public int LevelNumber => _levelNumber;
        public virtual void Initialize(LevelDataSO levelData, int levelNumber)
        {
            _levelData = levelData;
            _levelNumber = levelNumber;
        }
    }
}