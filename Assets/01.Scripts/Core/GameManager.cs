using _01.Scripts.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts.Core
{
    public class GameManager : MonoBehaviour, ISaveable
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Playing,
            PauseMenu,
            InventoryMenu,
            ShopMenu,
            Dialogue,
            LoadingScreen,
            MainMenu // MainMenu 상태 추가
        }

        public GameState CurrentState { get; private set; } = GameState.MainMenu; // 초기 상태를 MainMenu로 변경
        public GameState PreviousState { get; private set; } = GameState.MainMenu;

        [Header("Player Stats")]
        [SerializeField] private int startingPlayerCurrency = 100;
        public int CurrentPlayerCurrency { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoadedActions; // 씬 로드 시 액션 추가
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.RegisterSaveable(this);
            }
            else
            {
                Invoke(nameof(RegisterSelfToSaveLoadManager), 0.01f);
            }
        }
        private void RegisterSelfToSaveLoadManager()
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.RegisterSaveable(this);
            }
            else
            {
                Debug.LogWarning("GameManager: SaveLoadManager not found to register.");
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedActions;
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.UnregisterSaveable(this);
            }
        }

        private void InitializeGame()
        {
            CurrentPlayerCurrency = startingPlayerCurrency;
            // 초기 상태는 Awake에서 MainMenu로 설정됨.
            // 실제 씬 로드 후 상태를 결정하도록 OnSceneLoadedActions에서 처리할 수 있음.
        }

        // 씬이 로드될 때 호출될 메서드
        void OnSceneLoadedActions(Scene scene, LoadSceneMode mode)
        {
            // 현재 로드된 씬에 따라 게임 상태 결정
            if (scene.name == "MainMenuScene") // 메인 메뉴 씬 이름 (예시)
            {
                ChangeState(GameState.MainMenu);
            }
            else if (scene.name == "MainGameScene") // 메인 게임 씬 이름 (예시)
            {
                // 게임을 새로 시작하거나, 로드한 경우가 아니라면 Playing 상태
                // (로드 시에는 SaveLoadManager가 상태를 복원할 수 있음)
                // 여기서는 일단 Playing으로 설정. 로드 로직에서 덮어쓸 수 있음.
                if (CurrentState != GameState.LoadingScreen) // 로딩 중이 아닐때만 변경 (이중 변경 방지)
                {
                    ChangeState(GameState.Playing);
                }
            }
            // 다른 씬들에 대한 상태 설정 추가 가능
        }


        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState && CurrentState != GameState.Playing) return;

            PreviousState = CurrentState;
            CurrentState = newState;
            // Debug.Log($"Game State Changed: From {PreviousState} To {CurrentState}");

            switch (CurrentState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
                case GameState.PauseMenu:
                case GameState.InventoryMenu:
                case GameState.ShopMenu:
                case GameState.Dialogue:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
                case GameState.LoadingScreen:
                case GameState.MainMenu: // MainMenu 상태 추가
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }
            UIManager.Instance?.OnGameStateChanged(PreviousState, CurrentState);
        }

        public bool SpendCurrency(int amount)
        {
            if (amount <= 0) return true;
            if (CurrentPlayerCurrency >= amount)
            {
                CurrentPlayerCurrency -= amount;
                UIManager.Instance?.UpdateCurrencyDisplay(CurrentPlayerCurrency);
                return true;
            }
            UIManager.Instance?.ShowNotification("골드가 부족합니다.");
            return false;
        }

        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;
            CurrentPlayerCurrency += amount;
            UIManager.Instance?.UpdateCurrencyDisplay(CurrentPlayerCurrency);
        }

        public void PopulateSaveData(GameSaveData saveData)
        {
            if (saveData.playerData == null)
            {
                saveData.playerData = new PlayerData_Serializable();
            }
            saveData.playerData.currentCurrency = this.CurrentPlayerCurrency;
        }

        public void LoadFromSaveData(GameSaveData saveData)
        {
            if (saveData.playerData != null)
            {
                this.CurrentPlayerCurrency = saveData.playerData.currentCurrency;
                UIManager.Instance?.UpdateCurrencyDisplay(this.CurrentPlayerCurrency);
            }
            else
            {
                Debug.LogWarning("GameManager: No player data found in save file to load.");
            }
            // 로드 후, 현재 씬에 맞는 게임 상태로 변경 (SaveLoadManager가 씬을 로드했다면 그 씬에 맞게)
            // 예를 들어, MainGameScene으로 로드되었다면 Playing 상태로 변경되어야 함.
            // 이 부분은 SaveLoadManager의 ApplyLoadedData 후 또는 OnSceneLoadedActions에서 처리될 수 있음.
            // 여기서는 일단 화폐만 로드하고, 상태는 씬 로드 콜백에 맡김.
        }
    }
}