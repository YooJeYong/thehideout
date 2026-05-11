using Hidenet.Core;
using UnityEngine;

namespace Hidenet.Managers
{
    public class StageManager : MonoSingleton<StageManager>
    {
        [SerializeField] private StageDataSO stageData;

        public void LoadStage(int index)
        {
            if (stageData == null || index < 0 || index >= stageData.stageNames.Length)
            {
                Debug.LogWarning($"[StageManager] Invalid stage index: {index}");
                return;
            }

            SceneLoader.Instance.LoadStage(stageData.stageNames[index]);
        }

        public void UnloadCurrentStage()
        {
            SceneLoader.Instance.UnloadCurrentStage();
        }
    }
}
