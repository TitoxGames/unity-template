using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TitoX.UI.Flow
{
    /// <summary>The game's ad service as template screens see it. Pacing and No Ads stay on the game side.</summary>
    public interface IAdsGate
    {
        bool IsRewardedReady { get; }

        /// <summary>Calls back with true only when the reward was earned.</summary>
        void ShowRewarded(string placement, Action<bool> onFinished);

        /// <summary>Calls back when the ad closes, or right away when none is shown.</summary>
        void TryShowInterstitial(string placement, Action onClosed);
    }

    public static class UIAds
    {
        /// <summary>Set once at startup by the game. Without it rewarded ads are never ready and interstitials are skipped.</summary>
        public static IAdsGate Current { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
        }

        public static bool IsRewardedReady => Current != null && Current.IsRewardedReady;

        public static UniTask<bool> ShowRewardedAsync(string placement)
        {
            if (Current == null || string.IsNullOrEmpty(placement))
                return UniTask.FromResult(false);

            var completion = new UniTaskCompletionSource<bool>();
            Current.ShowRewarded(placement, isRewarded => completion.TrySetResult(isRewarded));
            return completion.Task;
        }

        public static UniTask TryShowInterstitialAsync(string placement)
        {
            if (Current == null || string.IsNullOrEmpty(placement))
                return UniTask.CompletedTask;

            var completion = new UniTaskCompletionSource();
            Current.TryShowInterstitial(placement, () => completion.TrySetResult());
            return completion.Task;
        }
    }
}
