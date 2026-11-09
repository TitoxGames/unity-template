using UnityEngine;

namespace TitoX.Core.Factory
{
    public abstract class Factory<TView, TArgs> : MonoBehaviour where TView : MonoBehaviour
    {
        public abstract TView Create(TArgs args);
    }
}