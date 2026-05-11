using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hidenet.Core
{
    [System.Serializable]
    public class SceneReference
    {
#if UNITY_EDITOR
        public SceneAsset sceneAsset;
#endif
        [SerializeField] private string sceneName;

        public string SceneName => sceneName;
    }
}
