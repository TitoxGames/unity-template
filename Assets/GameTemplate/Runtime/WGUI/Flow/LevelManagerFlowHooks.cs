using UnityEngine;
using WPuzzle;

namespace GameTemplate.Runtime.WGUI.Flow
{
    /// <summary>Default <see cref="IGameFlowHooks"/> for games built on the template's <see cref="LevelManager"/>.</summary>
    public class LevelManagerFlowHooks : MonoBehaviour, IGameFlowHooks
    {
        [Tooltip("Skips Home on launch until level 1 is cleared, so a new player starts playing right away.")]
        [SerializeField] private bool skipHomeBeforeFirstClear = true;

        public bool SkipHomeOnLaunch => skipHomeBeforeFirstClear && CurrentLevel <= 1;

        public int CurrentLevel => LevelDataBlock.Instance.CurrentLevelNumber;

        public void StartLevel()
        {
            LevelManager.Instance.LoadLevelByNumber(CurrentLevel);
        }

        public void RetryLevel()
        {
            LevelManager.Instance.ReloadLevel();
        }

        public void NextLevel()
        {
            LevelManager.Instance.LoadNextLevel();
        }

        public void QuitLevel()
        {
            LevelManager.Instance.UnloadCurrentLevel();
        }
    }
}
