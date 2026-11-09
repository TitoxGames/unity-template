using System;

namespace TitoX.Core
{
    public class AppLifeCycle : Singleton<AppLifeCycle>
    {
        public event Action OnApplicationQuitEvent;
        public event Action<bool> OnApplicationPauseEvent;

        private void OnApplicationQuit()
        {
            OnApplicationQuitEvent?.Invoke();
        }

        private void OnApplicationPause(bool pause)
        {
            OnApplicationPauseEvent?.Invoke(pause);
        }
    }
}