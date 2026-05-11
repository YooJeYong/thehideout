using UnityEngine;

namespace Hidenet.Core
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;
        private static bool applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }

                if (instance != null)
                {
                    return instance;
                }

                instance = FindAnyObjectByType<T>();
                if (instance != null)
                {
                    return instance;
                }

                var go = new GameObject($"[{typeof(T).Name}]");
                instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
                return instance;
            }
        }

        public static bool HasInstance => instance != null && !applicationIsQuitting;

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this as T;

            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
