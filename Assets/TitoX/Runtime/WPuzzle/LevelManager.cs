using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TitoX.Core;

namespace WPuzzle
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Title("Required References")]
        [SerializeField] private LevelConfigSOs _levelConfigSOs;
        [SerializeField] private LevelBase _levelPrefab;

        [Title("Configs")]
        [SerializeField] private Transform _levelContainer;
        [SerializeField] private bool _loadLevelOnStart;

        [Space(10)]
        [SerializeField] private bool _isLoop;

        [ShowIf("_isLoop")]
        [SerializeField] private int _startLoopLevelNumber = 1;

        private LevelBase _currentLevel;
        private AsyncOperationHandle<LevelDataSO> _currentLevelHandle;
        private bool _isLevelLoading;

        public int MaxLevel => _levelConfigSOs.Levels.Count;

        public event Action OnLevelLoaded;

        #region LifeCycle Methods

        protected override void Awake()
        {
            base.Awake();

            if (_levelConfigSOs == null)
            {
                Debug.LogError("LevelConfigSOs is not assigned in LevelManager");
                return;
            }

            if (_levelPrefab == null)
            {
                Debug.LogError("LevelPrefab is not assigned in LevelManager");
                return;
            }

            if (_loadLevelOnStart)
            {
                StartLoadLevel();
            }
        }

        #endregion

        #region Private Methods

        private async UniTask StartLevel()
        {
            int currentLevel = LevelDataBlock.Instance.CurrentLevelNumber;

            LoadLevelByNumber(currentLevel);

            await UniTask.CompletedTask;
        }

        private LevelConfig GetLevelByNumber(int levelNumber)
        {
            foreach (var config in _levelConfigSOs.Levels)
            {
                if (config.LevelNumber == levelNumber)
                {
                    return config;
                }
            }

            Debug.LogError(
                $"No level with number {levelNumber} found in LevelConfigSOs");

            return null;
        }

        /// <summary>
        /// Converts the player's logical level number
        /// into the actual level config number that should be loaded.
        ///
        /// Example:
        /// MaxLevel = 200
        /// StartLoopLevel = 50
        ///
        /// Logical 200 -> Config 200
        /// Logical 201 -> Config 50
        /// Logical 202 -> Config 51
        /// Logical 203 -> Config 52
        /// </summary>
        private int GetConfigLevelNumber(int logicalLevelNumber)
        {
            if (!_isLoop || logicalLevelNumber <= MaxLevel)
            {
                return logicalLevelNumber;
            }

            int loopLength = MaxLevel - _startLoopLevelNumber + 1;

            return _startLoopLevelNumber +
                   ((logicalLevelNumber - _startLoopLevelNumber) % loopLength);
        }

        private async UniTask LoadLevelAsync(
            LevelConfig levelConfig,
            int logicalLevelNumber)
        {
            if (_isLevelLoading)
            {
                return;
            }

            if (levelConfig == null)
            {
                Debug.LogError(
                    $"Cannot load level. LevelConfig is null. " +
                    $"Logical Level: {logicalLevelNumber}");

                return;
            }

            _isLevelLoading = true;

            try
            {
                // Unload previous level and release its Addressable handle
                UnloadCurrentLevel();

                // Load LevelDataSO from Addressables
                _currentLevelHandle =
                    levelConfig.GameData.LoadAssetAsync<LevelDataSO>();

                await _currentLevelHandle;

                if (!_currentLevelHandle.IsValid())
                {
                    Debug.LogError(
                        $"Invalid Addressable handle for level " +
                        $"{logicalLevelNumber}");

                    return;
                }

                LevelDataSO levelData = _currentLevelHandle.Result;

                if (levelData == null)
                {
                    Debug.LogError(
                        $"Failed to load LevelDataSO for level " +
                        $"{logicalLevelNumber}");

                    return;
                }

                // Instantiate level prefab
                _currentLevel = Instantiate(
                    _levelPrefab,
                    _levelContainer);

                /*
                 * IMPORTANT:
                 *
                 * levelConfig.LevelNumber is the config/data level.
                 * logicalLevelNumber is the player's actual progression.
                 *
                 * Example:
                 * Player Level = 201
                 * Config Level = 50
                 *
                 * We must initialize LevelBase with 201,
                 * not 50.
                 */
                _currentLevel.Initialize(
                    levelData,
                    logicalLevelNumber);

                // Save player's logical level
                LevelDataBlock.Instance.SetCurrentLevelNumber(
                    logicalLevelNumber);

                // Notify listeners
                OnLevelLoaded?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _isLevelLoading = false;
            }
        }

        public void UnloadCurrentLevel()
        {
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
                _currentLevel = null;
            }

            if (_currentLevelHandle.IsValid())
            {
                Addressables.Release(_currentLevelHandle);
                _currentLevelHandle = default;
            }
        }

        #endregion

        #region Public Methods

        public void StartLoadLevel()
        {
            StartLevel().Forget();
        }

        public void ReloadLevel()
        {
            if (_currentLevel == null)
            {
                Debug.LogError("Cannot reload level because current level is null.");
                return;
            }

            if (!_currentLevelHandle.IsValid())
            {
                Debug.LogError(
                    "Cannot reload level because current level handle is invalid.");

                return;
            }

            int currentLogicalLevelNumber = _currentLevel.LevelNumber;

            LevelDataSO levelData = _currentLevelHandle.Result;

            if (levelData == null)
            {
                Debug.LogError(
                    $"Cannot reload level {currentLogicalLevelNumber}. " +
                    "LevelDataSO is null.");

                return;
            }

            // Destroy current instance only.
            // Do NOT release Addressable handle because
            // we are going to reuse the same LevelDataSO.
            Destroy(_currentLevel.gameObject);
            _currentLevel = null;

            // Recreate current level using the same logical level number
            _currentLevel = Instantiate(
                _levelPrefab,
                _levelContainer);

            _currentLevel.Initialize(
                levelData,
                currentLogicalLevelNumber);
        }

        public void LoadNextLevel()
        {
            if (_currentLevel == null)
            {
                Debug.LogError("No current level to load next from.");
                return;
            }

            int currentLogicalLevelNumber =
                _currentLevel.LevelNumber;

            int nextLogicalLevelNumber =
                currentLogicalLevelNumber + 1;

            // Reached the end and looping is disabled
            if (nextLogicalLevelNumber > MaxLevel && !_isLoop)
            {
                Debug.LogWarning(
                    $"Already at the last level ({currentLogicalLevelNumber}). " +
                    "Cannot load next level.");

                return;
            }

            /*
             * Important:
             *
             * We always increment the logical level.
             *
             * Example:
             *
             * Current = 200
             * Next    = 201
             *
             * LoadLevelByNumber(201)
             *      ↓
             * Config Level = 50
             *      ↓
             * Load Addressable Level 50
             *      ↓
             * CurrentLevel.LevelNumber = 201
             */

            LoadLevelByNumber(nextLogicalLevelNumber);
        }

        public void LoadLevelByNumber(int levelNumber)
        {
            if (levelNumber <= 0)
            {
                Debug.LogError(
                    $"Invalid level number: {levelNumber}");

                return;
            }

            // This is the actual player progression level
            int logicalLevelNumber = levelNumber;

            // This is the level config/addressable that will actually be loaded
            int configLevelNumber =
                GetConfigLevelNumber(logicalLevelNumber);

            LevelConfig levelConfig =
                GetLevelByNumber(configLevelNumber);

            if (levelConfig == null)
            {
                Debug.LogError(
                    $"No level config found. " +
                    $"Logical Level: {logicalLevelNumber}, " +
                    $"Config Level: {configLevelNumber}");

                return;
            }

            Debug.Log(
                $"Loading Level - " +
                $"Logical: {logicalLevelNumber}, " +
                $"Config: {configLevelNumber}");

            LoadLevelAsync(
                levelConfig,
                logicalLevelNumber).Forget();
        }

        #endregion
    }
}