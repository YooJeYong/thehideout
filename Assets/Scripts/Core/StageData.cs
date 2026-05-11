using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hidenet.Core
{
    [CreateAssetMenu(fileName = "StageData", menuName = "Hidenet/Stage/Stage Data", order = 0)]
    public class StageDataSO : ScriptableObject
    {
#if UNITY_EDITOR
        public SceneAsset[] stages;

        private void OnValidate()
        {
            stageNames = new string[stages.Length];
            for (int i = 0; i < stages.Length; i++)
                stageNames[i] = stages[i] != null ? stages[i].name : "";
        }
#endif
        [HideInInspector] public string[] stageNames;
    }
}
