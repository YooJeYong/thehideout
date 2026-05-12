using Hidenet.Audio;
using Hidenet.Core;
using UnityEngine;

namespace Hidenet.Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private GameObject mainMenuUI;
        [SerializeField] private GameObject playerMove;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            mainMenuUI?.SetActive(true);
            playerMove?.SetActive(false);
            Hidenet.Audio.SoundManager.Instance.PlayBGM("BGM/BGM_Main_Menu.asset");
        }

        private void StartGame()
        {
            mainMenuUI?.SetActive(false);
            playerMove?.SetActive(true);
        }

        public void NewGame()
        {
            StartGame();
            StoryManager.Instance.SetStep(StoryStep.Jane_Greeting_0);
            StageManager.Instance.LoadStage(0);
        }

        public void LoadGame()
        {
            // TODO: SaveManager 구현 후 저장 데이터 로드
            // var save = SaveManager.Load();
            // StoryManager.Instance.SetStep(save.step);
            // StageManager.Instance.LoadStage(save.stageIndex);

            StartGame();
            StageManager.Instance.LoadStage(0);
        }

        public void GoToTitle()
        {
            StageManager.Instance.UnloadCurrentStage();
            ResourceManager.Instance.ReleaseAll();
            mainMenuUI?.SetActive(true);
            playerMove?.SetActive(false);
        }

        public void OpenSettings()
        {
            // TODO: SettingsUI 구현 후 연결
            Debug.Log("[GameManager] OpenSettings");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
