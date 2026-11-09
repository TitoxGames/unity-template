using UnityEngine;

namespace TitoX.Core
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] protected bool canDontDestroyOnLoad = true;
        private static T instance;
        private static readonly object locker = new object();
        public static T Instance
        {
            get
            {
                

                lock (locker)
                {
                    if (instance == null)
                    {
#if UNITY_2023_1_OR_NEWER
                        instance = FindFirstObjectByType<T>();
#else
                        instance = FindObjectOfType<T>();
#endif

                        if (instance == null)
                        {
                            GameObject go = new GameObject(typeof(T).Name);
                            instance = go.AddComponent<T>();
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            CreateInstance();
        }

        private void CreateInstance()
        {
            if (instance == null)
            {
                instance = this as T;

                if (canDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

        }
    }

}