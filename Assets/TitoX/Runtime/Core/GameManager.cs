using System;
using TitoX.Core.Settings;
using TitoX.GameData;
using UnityEngine;
using UnityEngine.Events;
using TitoX.Core;
using TitoX.Core.Time;
using WPuzzle;

namespace TitoX.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }
    
    public class GameManager: Singleton<GameManager>
    {
        [SerializeField] private ApplicationSettings applicationSettings;

        public bool IsInitializeOnStart;

        public bool InitSettings;
        
        public GameState CurrentState { get; private set; } = GameState.Playing;
        
        public GameplayTime GameplayTimer => _gameplayTimer;
        private GameplayTime _gameplayTimer;
        
        public UnityEvent onGameInitialized;
        public UnityEvent onGamePaused;
        public UnityEvent onGameResumed;
        
        private bool _isInitialized;
        
        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState != GameState.Playing)
                return;
                
            CurrentState = GameState.Paused;
            
            // Pause gameplay time
            _gameplayTimer?.Pause();
            
            // Pause time scale
            UnityEngine.Time.timeScale = 0f;
            
            // Trigger game paused event
            onGamePaused?.Invoke();
        }
        
        /// <summary>
        /// Resumes the game.
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
                return;
                
            CurrentState = GameState.Playing;
            
            // Resume time scale
            UnityEngine.Time.timeScale = 1f;
            
            // Resume gameplay time
            _gameplayTimer?.Resume();
            
            // Trigger game resumed event
            onGameResumed?.Invoke();
        }

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
                return;

            if (IsInitializeOnStart)
            {
                Initialize();
            }
        }

        private void Update()
        {
            // Tick gameplay time every frame
            _gameplayTimer?.Tick();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
        

        public void Initialize()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            if (applicationSettings == null)
            {
                applicationSettings = ScriptableObject.CreateInstance<ApplicationSettings>();
            }

            Application.targetFrameRate = applicationSettings.TargetFPS;

            if (InitSettings)
            {
                SettingManager.Instance.Sync();
            }

            _gameplayTimer = GameplayTime.Instance;

            if (_gameplayTimer == null)
            {
                _gameplayTimer = GameplayTime.CreateGlobal();
                _gameplayTimer.Restart();
            }

            onGameInitialized?.Invoke();
        }
    }
}