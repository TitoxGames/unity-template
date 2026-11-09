using System;
using Cysharp.Threading.Tasks;

namespace TitoX.UI.Flow
{
    /// <summary>A full-screen cut (iris, fade...) played between flow states. The flow shows and hides the screen around it.</summary>
    public interface IScreenTransition
    {
        /// <summary>Covers the screen, calls <paramref name="onCovered"/> while nothing is visible, then uncovers it.</summary>
        UniTask PlayAsync(Action onCovered);
    }

    /// <summary>The first screen on launch. The flow moves on once <see cref="PlayAsync"/> completes.</summary>
    public interface ILoadingScreen
    {
        UniTask PlayAsync();
    }
}
