using Hidenet.Core;
using UnityEngine;

namespace Hidenet.Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            // TODO: SaveManager 구현 후 저장 데이터 로드
            // var save = SaveManager.Load();
            // StoryManager.Instance.SetStep(save.step);
            // StageManager.Instance.LoadStage(save.stageIndex);

            StageManager.Instance.LoadStage(0);
        }
    }
}
