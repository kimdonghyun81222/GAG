using System.Collections;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame
{
    public class PauseMenuPanel : UIPanel
    {
        [Header("Pause Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button questsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button saveGameButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitGameButton;
        
        [Header("Sub Panels")]
        [SerializeField] private InventoryPanel inventoryPanel;
        [SerializeField] private QuestPanel questPanel;
        [SerializeField] private GameObject settingsPanel;
        
        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI farmNameText;
        [SerializeField] private TextMeshProUGUI playTimeText;
        [SerializeField] private TextMeshProUGUI currentSeasonText;
        [SerializeField] private TextMeshProUGUI currentDayText;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private Image playerAvatarImage;
        
        [Header("Quick Stats")]
        [SerializeField] private Transform quickStatsContainer;
        [SerializeField] private GameObject statItemPrefab;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Slider experienceBar;
        [SerializeField] private TextMeshProUGUI levelText;
        
        [Header("Animation Settings")]
        [SerializeField] private bool enablePauseAnimations = true;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float scaleInDuration = 0.2f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve scaleInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Blur Effect")]
        [SerializeField] private GameObject backgroundBlur;
        [SerializeField] private Image blurOverlay;
        [SerializeField] private Color blurColor = new Color(0f, 0f, 0f, 0.5f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip pauseOpenSound;
        [SerializeField] private AudioClip pauseCloseSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;
        
        [Header("Confirmation Dialogs")]
        [SerializeField] private GameObject saveConfirmationPanel;
        [SerializeField] private GameObject mainMenuConfirmationPanel;
        [SerializeField] private GameObject quitConfirmationPanel;
        [SerializeField] private TextMeshProUGUI confirmationText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        
        // Game state management
        private bool _isPaused = false;
        private bool _wasTimeScaleZero = false;
        private float _previousTimeScale = 1f;
        private CanvasGroup _canvasGroup;
        
        // UI state
        private PauseMenuState _currentState = PauseMenuState.Main;
        private System.Action _pendingConfirmAction;
        
        // Player data cache
        private PlayerData _cachedPlayerData;
        
        // Animation coroutines
        private Coroutine _fadeCoroutine;
        private Coroutine _scaleCoroutine;

        protected override void Awake()
        {
            base.Awake();
            InitializePauseMenu();
        }

        protected override void Start()
        {
            base.Start();
            SetupPauseMenu();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initially hide this panel
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandlePauseInput();
            
            if (_isPaused)
            {
                UpdateGameInfo();
            }
        }

        private void InitializePauseMenu()
        {
            // Get or add CanvasGroup for fading
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Auto-find sub panels if not assigned
            if (inventoryPanel == null)
                inventoryPanel = FindObjectOfType<InventoryPanel>();
            
            if (questPanel == null)
                questPanel = FindObjectOfType<QuestPanel>();
            
            // Setup blur overlay
            if (blurOverlay != null)
            {
                blurOverlay.color = blurColor;
            }
            
            // Create default stat item prefab if none exists
            if (statItemPrefab == null)
            {
                CreateDefaultStatItemPrefab();
            }
        }

        private void SetupPauseMenu()
        {
            // Setup main menu buttons
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(ResumeGame);
                AddButtonEffects(resumeButton);
            }
            
            if (inventoryButton != null)
            {
                inventoryButton.onClick.AddListener(OpenInventory);
                AddButtonEffects(inventoryButton);
            }
            
            if (questsButton != null)
            {
                questsButton.onClick.AddListener(OpenQuests);
                AddButtonEffects(questsButton);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OpenSettings);
                AddButtonEffects(settingsButton);
            }
            
            if (saveGameButton != null)
            {
                saveGameButton.onClick.AddListener(SaveGame);
                AddButtonEffects(saveGameButton);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ShowMainMenuConfirmation);
                AddButtonEffects(mainMenuButton);
            }
            
            if (quitGameButton != null)
            {
                quitGameButton.onClick.AddListener(ShowQuitConfirmation);
                AddButtonEffects(quitGameButton);
            }
            
            // Setup confirmation dialog buttons
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(ConfirmAction);
            
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(CancelConfirmation);
            
            // Hide confirmation panels initially
            HideAllConfirmationPanels();
        }

        private void AddButtonEffects(Button button)
        {
            if (button == null) return;
            
            // Add hover effects
            var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // Pointer Enter
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((eventData) => OnButtonHover(button));
            eventTrigger.triggers.Add(pointerEnter);
            
            // Pointer Exit
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((eventData) => OnButtonExitHover(button));
            eventTrigger.triggers.Add(pointerExit);
        }

        private void OnButtonHover(Button button)
        {
            PlayButtonHoverSound();
            
            if (enablePauseAnimations)
            {
                StartCoroutine(ScaleButton(button.transform, Vector3.one * 1.05f, 0.1f));
            }
        }

        private void OnButtonExitHover(Button button)
        {
            if (enablePauseAnimations)
            {
                StartCoroutine(ScaleButton(button.transform, Vector3.one, 0.1f));
            }
        }

        private IEnumerator ScaleButton(Transform buttonTransform, Vector3 targetScale, float duration)
        {
            Vector3 startScale = buttonTransform.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, scaleInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            buttonTransform.localScale = targetScale;
        }

        private void CreateDefaultStatItemPrefab()
        {
            var statObj = new GameObject("StatItem");
            statObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = statObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30f;
            layoutElement.flexibleWidth = 1f;
            
            // Horizontal layout
            var layout = statObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            
            // Stat name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(statObj.transform, false);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Stat Name";
            nameText.fontSize = 12f;
            nameText.color = Color.white;
            
            // Stat value
            var valueObj = new GameObject("Value");
            valueObj.transform.SetParent(statObj.transform, false);
            var valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = "100";
            valueText.fontSize = 12f;
            valueText.color = Color.yellow;
            valueText.alignment = TextAlignmentOptions.Right;
            
            statItemPrefab = statObj;
            statItemPrefab.SetActive(false);
        }

        // Input handling
        private void HandlePauseInput()
        {
            // ESC key to toggle pause
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_currentState == PauseMenuState.Main)
                {
                    if (_isPaused)
                    {
                        ResumeGame();
                    }
                    else
                    {
                        PauseGame();
                    }
                }
                else
                {
                    // Return to main pause menu
                    ReturnToMainPauseMenu();
                }
            }
            
            // Tab key for inventory
            if (Input.GetKeyDown(KeyCode.Tab) && !_isPaused)
            {
                PauseGame();
                OpenInventory();
            }
            
            // M key for map
            if (Input.GetKeyDown(KeyCode.M) && !_isPaused)
            {
                PauseGame();
                // OpenMap(); // Will implement later
            }
            
            // J key for journal/quests
            if (Input.GetKeyDown(KeyCode.J) && !_isPaused)
            {
                PauseGame();
                OpenQuests();
            }
        }

        // Game state management
        public void PauseGame()
        {
            if (_isPaused) return;
            
            _isPaused = true;
            _wasTimeScaleZero = (Time.timeScale == 0f);
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            
            // Show pause menu
            gameObject.SetActive(true);
            _currentState = PauseMenuState.Main;
            
            // Cache player data
            CachePlayerData();
            
            // Update UI
            UpdateGameInfo();
            UpdateQuickStats();
            
            // Play sound
            PlayPauseOpenSound();
            
            // Animate in
            if (enablePauseAnimations)
            {
                StartCoroutine(AnimatePauseMenuIn());
            }
            else
            {
                _canvasGroup.alpha = 1f;
                transform.localScale = Vector3.one;
            }
            
            // Enable blur effect
            if (backgroundBlur != null)
            {
                backgroundBlur.SetActive(true);
            }
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;
            
            StartCoroutine(ResumeGameCoroutine());
        }

        private IEnumerator ResumeGameCoroutine()
        {
            // Play sound
            PlayPauseCloseSound();
            
            // Animate out
            if (enablePauseAnimations)
            {
                yield return StartCoroutine(AnimatePauseMenuOut());
            }
            
            // Restore game state
            _isPaused = false;
            
            if (!_wasTimeScaleZero)
            {
                Time.timeScale = _previousTimeScale;
            }
            
            // Hide pause menu
            gameObject.SetActive(false);
            
            // Disable blur effect
            if (backgroundBlur != null)
            {
                backgroundBlur.SetActive(false);
            }
            
            // Hide any open sub panels
            HideAllSubPanels();
            
            _currentState = PauseMenuState.Main;
        }

        private IEnumerator AnimatePauseMenuIn()
        {
            // Start from invisible and small
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            
            // Animate fade and scale in parallel
            var fadeCoroutine = StartCoroutine(AnimateFade(0f, 1f, fadeInDuration));
            var scaleCoroutine = StartCoroutine(AnimateScale(Vector3.zero, Vector3.one, scaleInDuration));
            
            // Wait for both animations to complete
            yield return fadeCoroutine;
            yield return scaleCoroutine;
        }

        private IEnumerator AnimatePauseMenuOut()
        {
            // Animate fade and scale out parallel
            var fadeCoroutine = StartCoroutine(AnimateFade(1f, 0f, fadeInDuration));
            var scaleCoroutine = StartCoroutine(AnimateScale(Vector3.one, Vector3.one * 0.8f, scaleInDuration));
            
            // Wait for both animations to complete
            yield return fadeCoroutine;
            yield return scaleCoroutine;
        }

        private IEnumerator AnimateFade(float startAlpha, float endAlpha, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, fadeInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            _canvasGroup.alpha = endAlpha;
        }

        private IEnumerator AnimateScale(Vector3 startScale, Vector3 endScale, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                transform.localScale = Vector3.Lerp(startScale, endScale, scaleInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            transform.localScale = endScale;
        }

        // Player data management
        private void CachePlayerData()
        {
            // This would normally get data from game managers
            _cachedPlayerData = new PlayerData
            {
                playerName = "Player", // Get from PlayerManager
                farmName = "My Farm", // Get from FarmManager  
                playTime = "12:34:56", // Get from GameTimeManager
                currentSeason = "Spring", // Get from SeasonManager
                currentDay = 15, // Get from SeasonManager
                money = 12500, // Get from PlayerManager
                level = 5, // Get from PlayerManager
                health = 85f, // Get from PlayerManager
                maxHealth = 100f, // Get from PlayerManager
                energy = 67f, // Get from PlayerManager
                maxEnergy = 100f, // Get from PlayerManager
                experience = 750f, // Get from PlayerManager
                experienceToNext = 1000f // Get from PlayerManager
            };
        }

        private void UpdateGameInfo()
        {
            if (_cachedPlayerData == null) return;
            
            if (playerNameText != null)
                playerNameText.text = _cachedPlayerData.playerName;
            
            if (farmNameText != null)
                farmNameText.text = _cachedPlayerData.farmName;
            
            if (playTimeText != null)
                playTimeText.text = $"Play Time: {_cachedPlayerData.playTime}";
            
            if (currentSeasonText != null)
                currentSeasonText.text = _cachedPlayerData.currentSeason;
            
            if (currentDayText != null)
                currentDayText.text = $"Day {_cachedPlayerData.currentDay}";
            
            if (moneyText != null)
                moneyText.text = $"${_cachedPlayerData.money:N0}";
            
            if (levelText != null)
                levelText.text = $"Lv.{_cachedPlayerData.level}";
        }

        private void UpdateQuickStats()
        {
            if (_cachedPlayerData == null) return;
            
            // Update health bar
            if (healthBar != null)
            {
                healthBar.value = _cachedPlayerData.health / _cachedPlayerData.maxHealth;
            }
            
            // Update energy bar
            if (energyBar != null)
            {
                energyBar.value = _cachedPlayerData.energy / _cachedPlayerData.maxEnergy;
            }
            
            // Update experience bar
            if (experienceBar != null)
            {
                experienceBar.value = _cachedPlayerData.experience / _cachedPlayerData.experienceToNext;
            }
            
            // Update quick stats display
            UpdateQuickStatsDisplay();
        }

        private void UpdateQuickStatsDisplay()
        {
            if (quickStatsContainer == null || statItemPrefab == null) return;
            
            // Clear existing stats
            foreach (Transform child in quickStatsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add current stats
            CreateStatItem("Health", $"{_cachedPlayerData.health:F0}/{_cachedPlayerData.maxHealth:F0}");
            CreateStatItem("Energy", $"{_cachedPlayerData.energy:F0}/{_cachedPlayerData.maxEnergy:F0}");
            CreateStatItem("Experience", $"{_cachedPlayerData.experience:F0}/{_cachedPlayerData.experienceToNext:F0}");
            CreateStatItem("Money", $"${_cachedPlayerData.money:N0}");
        }

        private void CreateStatItem(string statName, string statValue)
        {
            var statObj = Instantiate(statItemPrefab, quickStatsContainer);
            statObj.SetActive(true);
            
            var texts = statObj.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = statName;
                texts[1].text = statValue;
            }
        }

        // Menu navigation
        private void OpenInventory()
        {
            PlayButtonClickSound();
            
            if (inventoryPanel != null)
            {
                _currentState = PauseMenuState.Inventory;
                HideMainMenu();
                inventoryPanel.gameObject.SetActive(true);
            }
        }

        private void OpenQuests()
        {
            PlayButtonClickSound();
            
            if (questPanel != null)
            {
                _currentState = PauseMenuState.Quests;
                HideMainMenu();
                questPanel.gameObject.SetActive(true);
            }
        }

        private void OpenSettings()
        {
            PlayButtonClickSound();
            
            if (settingsPanel != null)
            {
                _currentState = PauseMenuState.Settings;
                HideMainMenu();
                settingsPanel.SetActive(true);
            }
        }

        private void ReturnToMainPauseMenu()
        {
            PlayButtonClickSound();
            
            _currentState = PauseMenuState.Main;
            ShowMainMenu();
            HideAllSubPanels();
        }

        private void HideMainMenu()
        {
            // Hide main pause menu buttons but keep the panel active
            if (resumeButton != null) resumeButton.transform.parent.gameObject.SetActive(false);
        }

        private void ShowMainMenu()
        {
            // Show main pause menu buttons
            if (resumeButton != null) resumeButton.transform.parent.gameObject.SetActive(true);
        }

        private void HideAllSubPanels()
        {
            if (inventoryPanel != null) inventoryPanel.gameObject.SetActive(false);
            if (questPanel != null) questPanel.gameObject.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        // Game actions
        private void SaveGame()
        {
            PlayButtonClickSound();
            ShowSaveConfirmation();
        }

        private void ShowSaveConfirmation()
        {
            ShowConfirmationDialog(
                "Save Game",
                "Do you want to save your current progress?",
                PerformSaveGame,
                saveConfirmationPanel
            );
        }

        private void PerformSaveGame()
        {
            // This would call the SaveManager
            Debug.Log("Game saved successfully!");
            
            // Show success feedback
            // Could show a toast notification here
            
            HideAllConfirmationPanels();
        }

        private void ShowMainMenuConfirmation()
        {
            PlayButtonClickSound();
            
            ShowConfirmationDialog(
                "Return to Main Menu",
                "Are you sure you want to return to the main menu?\nAny unsaved progress will be lost!",
                ReturnToMainMenu,
                mainMenuConfirmationPanel
            );
        }

        private void ReturnToMainMenu()
        {
            // Save current game state
            PerformSaveGame();
            
            // Restore time scale
            Time.timeScale = 1f;
            
            // Load main menu scene
            SceneManager.LoadScene("MainMenuScene");
        }

        private void ShowQuitConfirmation()
        {
            PlayButtonClickSound();
            
            ShowConfirmationDialog(
                "Quit Game",
                "Are you sure you want to quit the game?\nAny unsaved progress will be lost!",
                QuitGame,
                quitConfirmationPanel
            );
        }

        private void QuitGame()
        {
            // Save current game state
            PerformSaveGame();
            
            // Quit application
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Confirmation dialog system
        private void ShowConfirmationDialog(string title, string message, System.Action confirmAction, GameObject specificPanel = null)
        {
            _pendingConfirmAction = confirmAction;
            
            if (confirmationText != null)
            {
                confirmationText.text = $"{title}\n\n{message}";
            }
            
            // Show the specific confirmation panel or a generic one
            if (specificPanel != null)
            {
                specificPanel.SetActive(true);
            }
            else
            {
                // Show generic confirmation dialog
                if (saveConfirmationPanel != null)
                    saveConfirmationPanel.SetActive(true);
            }
        }

        private void ConfirmAction()
        {
            PlayButtonClickSound();
            
            _pendingConfirmAction?.Invoke();
            _pendingConfirmAction = null;
            
            HideAllConfirmationPanels();
        }

        private void CancelConfirmation()
        {
            PlayButtonClickSound();
            
            _pendingConfirmAction = null;
            HideAllConfirmationPanels();
        }

        private void HideAllConfirmationPanels()
        {
            if (saveConfirmationPanel != null) saveConfirmationPanel.SetActive(false);
            if (mainMenuConfirmationPanel != null) mainMenuConfirmationPanel.SetActive(false);
            if (quitConfirmationPanel != null) quitConfirmationPanel.SetActive(false);
        }

        // Audio methods
        private void PlayPauseOpenSound()
        {
            Debug.Log("Pause open sound would play here");
        }

        private void PlayPauseCloseSound()
        {
            Debug.Log("Pause close sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        private void PlayButtonHoverSound()
        {
            Debug.Log("Button hover sound would play here");
        }

        // Public interface
        public bool IsPaused => _isPaused;
        public PauseMenuState CurrentState => _currentState;
        
        public void TogglePause()
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        private void OnDestroy()
        {
            // Ensure time scale is restored
            if (_isPaused && !_wasTimeScaleZero)
            {
                Time.timeScale = _previousTimeScale;
            }
            
            // Stop all coroutines
            StopAllCoroutines();
        }
    }

    // Helper enums and data structures
    public enum PauseMenuState
    {
        Main,
        Inventory,
        Quests,
        Settings,
        Crafting,
        Map
    }

    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public string farmName;
        public string playTime;
        public string currentSeason;
        public int currentDay;
        public int money;
        public int level;
        public float health;
        public float maxHealth;
        public float energy;
        public float maxEnergy;
        public float experience;
        public float experienceToNext;
    }
}