namespace GameTemplate.Runtime.WGUI.Flow
{
    /// <summary>
    /// The game-side work behind each flow move. Called while the transition covers the screen, so a level can be swapped
    /// without the player seeing it.
    /// </summary>
    public interface IGameFlowHooks
    {
        /// <summary>Goes straight from loading into the current level, e.g. for a new player who has not cleared one yet.</summary>
        bool SkipHomeOnLaunch { get; }

        /// <summary>The level the Play button starts, as shown to the player.</summary>
        int CurrentLevel { get; }

        void StartLevel();

        void RetryLevel();

        void NextLevel();

        /// <summary>Leaves the running level for Home.</summary>
        void QuitLevel();
    }
}
