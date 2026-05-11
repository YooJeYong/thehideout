using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hidenet.Core
{
    public class SceneLoader : MonoSingleton<SceneLoader>
    {
        private string loadedStageScene;

        public void LoadStage(string sceneName, Action onComplete = null)
        {
            StartCoroutine(LoadStageRoutine(sceneName, onComplete));
        }

        public void UnloadCurrentStage(Action onComplete = null)
        {
            if (string.IsNullOrEmpty(loadedStageScene)) return;
            StartCoroutine(UnloadStageRoutine(loadedStageScene, onComplete));
        }

        private IEnumerator LoadStageRoutine(string sceneName, Action onComplete)
        {
            if (!string.IsNullOrEmpty(loadedStageScene))
                yield return UnloadStageRoutine(loadedStageScene, null);

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return op;

            loadedStageScene = sceneName;
            Debug.Log($"[SceneLoader] Loaded: {sceneName}");
            onComplete?.Invoke();
        }

        private IEnumerator UnloadStageRoutine(string sceneName, Action onComplete)
        {
            var op = SceneManager.UnloadSceneAsync(sceneName);
            yield return op;

            loadedStageScene = null;
            Debug.Log($"[SceneLoader] Unloaded: {sceneName}");
            onComplete?.Invoke();
        }
    }
}
