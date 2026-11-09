using UnityEngine;

namespace TitoX.UI.Flow
{
    /// <summary>
    /// What screens and gameplay call to move between states. Each request returns false when it was refused
    /// (wrong state, or another change is still running), so a button can re-enable itself.
    /// </summary>
    public static class UIFlow
    {
        public static IGameFlowHooks Hooks { get; private set; }

        public static UIFlowState State => Controller != null ? Controller.State : UIFlowState.None;

        public static bool IsBusy => Controller != null && Controller.IsBusy;

        public static bool IsInLevel => State == UIFlowState.Gameplay || State == UIFlowState.Win || State == UIFlowState.Lose;

        public static int CurrentLevel => Hooks?.CurrentLevel ?? 1;

        private static UIFlowController Controller => UIFlowController.Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Hooks = null;
        }

        public static void SetHooks(IGameFlowHooks hooks)
        {
            Hooks = hooks;
        }

        public static bool Play()
        {
            return TryGetController(out UIFlowController controller) && controller.Play();
        }

        public static bool Retry()
        {
            return TryGetController(out UIFlowController controller) && controller.Retry();
        }

        public static bool Next()
        {
            return TryGetController(out UIFlowController controller) && controller.Next();
        }

        public static bool Home()
        {
            return TryGetController(out UIFlowController controller) && controller.Home();
        }

        public static bool Win()
        {
            return TryGetController(out UIFlowController controller) && controller.Win();
        }

        public static bool Lose()
        {
            return TryGetController(out UIFlowController controller) && controller.Lose();
        }

        private static bool TryGetController(out UIFlowController controller)
        {
            controller = Controller;
            if (controller != null)
                return true;

            Debug.LogError("[UIFlow] No UIFlowController in the scene.");
            return false;
        }
    }
}
