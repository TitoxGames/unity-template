using System;
using System.Collections;
using System.Collections.Generic;
using TitoX.Core.EventBus;
using UnityEngine;

namespace TitoX.Core.Time
{
    /// <summary>
    /// Manages gameplay time independently from Unity's Time.time.
    /// Supports pause, resume, speed multiplier, and is affected by Unity's TimeScale.
    /// Can be used as a singleton or as multiple instances for different time contexts (e.g., level time, round time).
    /// </summary>
    public class GameplayTime
    {
        /// <summary>
        /// Singleton instance for global gameplay time.
        /// </summary>
        public static GameplayTime Instance { get; private set; }
        
        // --- State ---
        
        /// <summary>
        /// Whether this gameplay timer is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }
        
        /// <summary>
        /// Whether this gameplay timer is paused.
        /// </summary>
        public bool IsPaused { get; private set; }
        
        /// <summary>
        /// Total elapsed gameplay time in seconds (only counts when running and not paused).
        /// Affected by both custom speed multiplier and Unity's TimeScale.
        /// </summary>
        public float ElapsedTime { get; private set; }
        
        /// <summary>
        /// Total elapsed unscaled gameplay time in seconds (not affected by speed multiplier, but still pauses when paused).
        /// </summary>
        public float ElapsedUnscaledTime { get; private set; }
        
        /// <summary>
        /// The delta time for the current frame, taking into account pause state and speed multiplier.
        /// Affected by Unity's Time.timeScale.
        /// </summary>
        public float DeltaTime { get; private set; }
        
        /// <summary>
        /// The unscaled delta time for the current frame, taking into account pause state only.
        /// NOT affected by speed multiplier, but IS affected by Unity's Time.timeScale.
        /// </summary>
        public float UnscaledDeltaTime { get; private set; }
        
        /// <summary>
        /// Custom speed multiplier for this gameplay timer. Default is 1.0.
        /// The final delta time = Unity's Time.deltaTime * SpeedMultiplier (when running and not paused).
        /// </summary>
        public float SpeedMultiplier
        {
            get => _speedMultiplier;
            set
            {
                if (value < 0f)
                {
                    Debug.LogWarning("[GameplayTime] SpeedMultiplier cannot be negative. Clamped to 0.");
                    value = 0f;
                }
                
                if (Math.Abs(_speedMultiplier - value) > float.Epsilon)
                {
                    _speedMultiplier = value;
                    EventBus<GameplayTimeEvent>.Post(new GameplayTimeEvent(GameplayTimeEvent.EventType.SpeedChanged, _name));
                }
            }
        }
        private float _speedMultiplier = 1f;
        
        /// <summary>
        /// Optional countdown time. If set to > 0 when starting, the timer will count down
        /// and fire an event when it reaches zero.
        /// </summary>
        public float CountdownTime { get; private set; }
        
        /// <summary>
        /// Remaining time when in countdown mode.
        /// </summary>
        public float RemainingTime => Mathf.Max(0f, CountdownTime - ElapsedTime);
        
        /// <summary>
        /// Whether this timer is in countdown mode.
        /// </summary>
        public bool IsCountdown => CountdownTime > 0f;
        
        /// <summary>
        /// Whether countdown has completed (only valid in countdown mode).
        /// </summary>
        public bool IsCountdownComplete => IsCountdown && ElapsedTime >= CountdownTime;
        
        /// <summary>
        /// Name identifier for this gameplay time instance.
        /// </summary>
        public string Name => _name;
        
        // --- Events ---
        
        /// <summary>
        /// Invoked when this timer is started.
        /// </summary>
        public event Action OnStarted;
        
        /// <summary>
        /// Invoked when this timer is paused.
        /// </summary>
        public event Action OnPaused;
        
        /// <summary>
        /// Invoked when this timer is resumed.
        /// </summary>
        public event Action OnResumed;
        
        /// <summary>
        /// Invoked when this timer is stopped.
        /// </summary>
        public event Action OnStopped;
        
        /// <summary>
        /// Invoked when this timer is reset.
        /// </summary>
        public event Action OnReset;
        
        /// <summary>
        /// Invoked when countdown reaches zero (only in countdown mode).
        /// </summary>
        public event Action OnCountdownComplete;
        
        /// <summary>
        /// Invoked every frame while the timer is running. Passes the delta time.
        /// </summary>
        public event Action<float> OnTick;
        
        // --- Private ---
        private readonly string _name;
        private bool _countdownEventFired;
        
        // --- Constructor ---
        
        /// <summary>
        /// Creates a new GameplayTime instance.
        /// </summary>
        /// <param name="name">Name identifier for this timer.</param>
        /// <param name="countdownTime">If > 0, the timer will work in countdown mode.</param>
        public GameplayTime(string name = "Default", float countdownTime = 0f)
        {
            _name = name;
            CountdownTime = countdownTime;
            ElapsedTime = 0f;
            ElapsedUnscaledTime = 0f;
            DeltaTime = 0f;
            UnscaledDeltaTime = 0f;
            IsRunning = false;
            IsPaused = false;
            _countdownEventFired = false;
        }
        
        // --- Static Methods ---
        
        /// <summary>
        /// Creates and sets the singleton instance.
        /// </summary>
        /// <param name="countdownTime">Optional countdown time for the global timer.</param>
        /// <returns>The created GameplayTime instance.</returns>
        public static GameplayTime CreateGlobal(float countdownTime = 0f)
        {
            Instance = new GameplayTime("GamePlayTimer", countdownTime);
            return Instance;
        }
        
        /// <summary>
        /// Destroys the singleton instance.
        /// </summary>
        public static void DestroyGlobal()
        {
            if (Instance != null)
            {
                Instance.Stop();
                Instance = null;
            }
        }
        
        // --- Public Methods ---
        
        /// <summary>
        /// Starts the gameplay timer. If already running, this does nothing.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                Debug.LogWarning($"[GameplayTime:{_name}] Timer is already running.");
                return;
            }
            
            IsRunning = true;
            IsPaused = false;
            
            OnStarted?.Invoke();
            EventBus<GameplayTimeEvent>.Post(new GameplayTimeEvent(GameplayTimeEvent.EventType.Started, _name));
        }
        
        /// <summary>
        /// Pauses the gameplay timer. The timer will stop accumulating time.
        /// </summary>
        public void Pause()
        {
            if (!IsRunning || IsPaused)
                return;
            
            IsPaused = true;
            DeltaTime = 0f;
            UnscaledDeltaTime = 0f;
            
            OnPaused?.Invoke();
            EventBus<GameplayTimeEvent>.Post(new GameplayTimeEvent(GameplayTimeEvent.EventType.Paused, _name));
        }
        
        /// <summary>
        /// Resumes the gameplay timer after being paused.
        /// </summary>
        public void Resume()
        {
            if (!IsRunning || !IsPaused)
                return;
            
            IsPaused = false;
            
            OnResumed?.Invoke();
            EventBus<GameplayTimeEvent>.Post(new GameplayTimeEvent(GameplayTimeEvent.EventType.Resumed, _name));
        }
        
        /// <summary>
        /// Stops the gameplay timer completely.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                return;
            
            IsRunning = false;
            IsPaused = false;
            DeltaTime = 0f;
            UnscaledDeltaTime = 0f;
            
            OnStopped?.Invoke();
            EventBus<GameplayTimeEvent>.Post(new GameplayTimeEvent(GameplayTimeEvent.EventType.Stopped, _name));
        }
        
        /// <summary>
        /// Resets the elapsed time to zero. Timer state (running/paused) is preserved.
        /// </summary>
        /// <param name="newCountdownTime">Optionally set a new countdown time. Pass -1 to keep current.</param>
        public void Reset(float newCountdownTime = -1f)
        {
            ElapsedTime = 0f;
            ElapsedUnscaledTime = 0f;
            DeltaTime = 0f;
            UnscaledDeltaTime = 0f;
            _countdownEventFired = false;
            
            if (newCountdownTime >= 0f)
            {
                CountdownTime = newCountdownTime;
            }
            
            OnReset?.Invoke();
            EventBus<GameplayTimeEvent>.Post(new GameplayTimeEvent(GameplayTimeEvent.EventType.Reset, _name));
        }
        
        /// <summary>
        /// Restarts the timer: resets time to zero and starts running.
        /// </summary>
        /// <param name="newCountdownTime">Optionally set a new countdown time. Pass -1 to keep current.</param>
        public void Restart(float newCountdownTime = -1f)
        {
            IsRunning = false;
            Reset(newCountdownTime);
            Start();
        }
        
        /// <summary>
        /// Adds bonus time to the timer. In countdown mode, this extends the countdown.
        /// </summary>
        /// <param name="seconds">Seconds to add. Can be negative to subtract time.</param>
        public void AddTime(float seconds)
        {
            if (IsCountdown)
            {
                CountdownTime += seconds;
                if (CountdownTime < 0f)
                    CountdownTime = 0f;
            }
            else
            {
                ElapsedTime += seconds;
                if (ElapsedTime < 0f)
                    ElapsedTime = 0f;
            }
        }
        
        /// <summary>
        /// Must be called every frame (typically from a MonoBehaviour's Update method).
        /// This updates the timer's delta time and elapsed time based on Unity's Time.deltaTime.
        /// </summary>
        public void Tick()
        {
            if (!IsRunning || IsPaused)
            {
                DeltaTime = 0f;
                UnscaledDeltaTime = 0f;
                return;
            }
            
            // Use Time.deltaTime which is already affected by Time.timeScale
            // So when Unity pauses (timeScale = 0), our timer also pauses automatically
            float unityDeltaTime = UnityEngine.Time.deltaTime;
            
            UnscaledDeltaTime = unityDeltaTime;
            DeltaTime = unityDeltaTime * _speedMultiplier;
            
            ElapsedTime += DeltaTime;
            ElapsedUnscaledTime += UnscaledDeltaTime;
            
            // Check countdown completion
            if (IsCountdown && !_countdownEventFired && ElapsedTime >= CountdownTime)
            {
                _countdownEventFired = true;
                OnCountdownComplete?.Invoke();
                EventBus<GameplayTimeEvent>.Post(new GameplayTimeEvent(GameplayTimeEvent.EventType.CountdownComplete, _name));
            }
            
            OnTick?.Invoke(DeltaTime);
        }
        
        /// <summary>
        /// Gets the elapsed time as a formatted string (HH:MM:SS or MM:SS).
        /// </summary>
        /// <param name="useRemainingTime">If true and in countdown mode, formats the remaining time instead.</param>
        /// <returns>Formatted time string.</returns>
        public string GetFormattedTime(bool useRemainingTime = false)
        {
            float time = useRemainingTime && IsCountdown ? RemainingTime : ElapsedTime;
            
            int totalSeconds = Mathf.FloorToInt(time);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            
            return hours > 0 
                ? $"{hours:00}:{minutes:00}:{seconds:00}" 
                : $"{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// Gets the elapsed time as a formatted string with milliseconds (HH:MM:SS.mmm or MM:SS.mmm).
        /// </summary>
        /// <param name="useRemainingTime">If true and in countdown mode, formats the remaining time instead.</param>
        /// <returns>Formatted time string with milliseconds.</returns>
        public string GetFormattedTimePrecise(bool useRemainingTime = false)
        {
            float time = useRemainingTime && IsCountdown ? RemainingTime : ElapsedTime;
            
            int totalSeconds = Mathf.FloorToInt(time);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            int milliseconds = Mathf.FloorToInt((time - totalSeconds) * 1000);
            
            return hours > 0 
                ? $"{hours:00}:{minutes:00}:{seconds:00}.{milliseconds:000}" 
                : $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }
        
        /// <summary>
        /// Returns countdown progress as a 0-1 value (0 = just started, 1 = complete).
        /// Only meaningful in countdown mode.
        /// </summary>
        public float GetCountdownProgress()
        {
            if (!IsCountdown || CountdownTime <= 0f)
                return 0f;
            
            return Mathf.Clamp01(ElapsedTime / CountdownTime);
        }
        
        /// <summary>
        /// Returns countdown remaining progress as a 0-1 value (1 = just started, 0 = complete).
        /// Only meaningful in countdown mode.
        /// </summary>
        public float GetCountdownRemainingProgress()
        {
            return 1f - GetCountdownProgress();
        }
        
        public override string ToString()
        {
            return $"[GameplayTime:{_name}] Running={IsRunning}, Paused={IsPaused}, " +
                   $"Elapsed={ElapsedTime:F2}s, Speed={_speedMultiplier:F2}x" +
                   (IsCountdown ? $", Remaining={RemainingTime:F2}s" : "");
        }
    }
}
