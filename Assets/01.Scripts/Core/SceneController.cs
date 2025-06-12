using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts.Core
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene"; // 실제 메인 메뉴 씬 이름으로 변경
        [SerializeField] private string mainGameSceneName = "MainGameScene"; // 실제 주 게임 씬 이름으로 변경
        // [SerializeField] private string loadingSceneName = "LoadingScene"; // 로딩 씬이 있다면

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // 씬 컨트롤러도 유지 (선택적)
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // --- Public Methods to be called by UI Buttons or other scripts ---

        public void LoadMainMenu()
        {
            // 로딩 씬을 거치지 않고 바로 로드
            StartCoroutine(LoadSceneRoutine(mainMenuSceneName, GameManager.GameState.MainMenu));
        }

        public void LoadMainGame()
        {
            // 새 게임 시작 시
            // TODO: 새 게임 시작 시 기존 저장 데이터 처리 여부 (예: 초기화)
            StartCoroutine(LoadSceneRoutine(mainGameSceneName, GameManager.GameState.Playing));
        }

        public void LoadMainGameFromSave()
        {
            // 게임 로드 시에는 SaveLoadManager가 씬 전환 및 데이터 적용을 담당.
            // 이 메서드는 UI에서 "Continue" 또는 "Load Game" 버튼에 연결되어
            // SaveLoadManager.Instance.TriggerLoadGame(fileName)을 호출하도록 할 수 있음.
            // 여기서는 직접 씬 로드 대신 SaveLoadManager 사용을 권장.
            // 예시: SaveLoadManager.Instance.TriggerLoadGame("MyDefaultSave");
            Debug.Log("To load a game, use SaveLoadManager.TriggerLoadGame(). This function is a placeholder.");
            // 만약 SaveLoadManager가 특정 씬으로 로드한 후 상태를 자동으로 설정하지 않는다면,
            // 여기서 상태를 Playing으로 설정할 수 있지만, SaveLoadManager와 역할 중복 가능성.
        }


        // --- Private Coroutine for Scene Loading ---

        private IEnumerator LoadSceneRoutine(string sceneName, GameManager.GameState targetStateAfterLoad)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameManager.GameState.LoadingScreen);
            }

            // 로딩 씬을 사용하는 경우:
            // AsyncOperation asyncLoadLoading = SceneManager.LoadSceneAsync(loadingSceneName);
            // while (!asyncLoadLoading.isDone)
            // {
            //     yield return null;
            // }
            // UIManager.Instance?.UpdateLoadingProgress(0f); // 로딩 바 초기화

            AsyncOperation asyncLoadTarget = SceneManager.LoadSceneAsync(sceneName);
            asyncLoadTarget.allowSceneActivation = false; // 바로 활성화하지 않음 (로딩 바 제어 시)

            while (!asyncLoadTarget.isDone)
            {
                // UIManager.Instance?.UpdateLoadingProgress(asyncLoadTarget.progress);
                if (asyncLoadTarget.progress >= 0.9f) // 거의 다 로드됨
                {
                    // UIManager.Instance?.UpdateLoadingProgress(1f);
                    asyncLoadTarget.allowSceneActivation = true; // 씬 활성화
                }
                yield return null;
            }

            // 씬 로드가 완료된 후 GameManager 상태 변경
            // (GameManager의 OnSceneLoadedActions에서도 씬 이름 기반으로 상태를 설정하므로,
            //  여기서의 상태 변경은 해당 콜백보다 우선시되거나, 또는 콜백에 위임할 수 있음)
            if (GameManager.Instance != null)
            {
                // GameManager.Instance.ChangeState(targetStateAfterLoad);
                // GameManager의 OnSceneLoadedActions가 올바른 씬 이름을 기준으로 상태를 설정하므로
                // 여기서는 LoadingScreen 상태를 해제하는 것만으로도 충분할 수 있습니다.
                // 또는 명시적으로 targetStateAfterLoad를 설정합니다.
                // 현재 GameManager의 OnSceneLoadedActions가 있으므로, 그곳에서 상태가 설정될 것입니다.
                // 만약 이 SceneController에서 상태를 강제하고 싶다면 아래 주석 해제:
                // GameManager.Instance.ChangeState(targetStateAfterLoad);
            }
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 테스트 시 종료
#endif
        }
    }
}