using UnityEngine;

namespace TitoX.Core.ObjectPool
{
    public class ReturnToPool : MonoBehaviour
    {
        [HideInInspector] public Pool pool;
        public void OnDisable()
        {
            pool.AddToPool(gameObject);
        }
    }
}