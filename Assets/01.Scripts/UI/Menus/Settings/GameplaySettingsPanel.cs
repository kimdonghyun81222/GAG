using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.Settings
{
    public class GameplaySettingsPanel : UIPanel
    {
        [Header("Difficulty Settings")]
        [SerializeField] private TMP_Dropdown difficultyDropdown;
        [SerializeField] private Slider difficultyCustomSlider;
        [SerializeField] private TextMeshProUGUI difficultyDescription;
        [SerializeField] private Toggle customDifficultyToggle;
        [SerializeField] private Button resetDifficultyButton;
        
        [Header("Save & Load Settings")]
        [SerializeField] private Toggle autosaveToggle;
        [SerializeField] private Slider autosaveIntervalSlider;
        [SerializeField] private TextMeshProUGUI autosaveIntervalText;
        [SerializeField] private TMP_Dropdown autosaveFrequencyDropdown;
        [SerializeField] private Toggle saveOnExitToggle;
        [SerializeField] private Toggle cloudSaveToggle;
        [SerializeField] private TextMeshProUGUI saveInfoText;
        [SerializeField] private Button manageSavesButton;
        
        [Header("Tutorial & Help")]
        [SerializeField] private Toggle showTutorialsToggle;
        [SerializeField] private Toggle showHintsToggle;
        [SerializeField] private Toggle showTooltipsToggle;
        [SerializeField] private Slider tutorialSpeedSlider;
        [SerializeField] private TextMeshProUGUI tutorialSpeedText;
        [SerializeField] private Toggle skipSeenTutorialsToggle;
        [SerializeField] private Button resetTutorialsButton;
        
        [Header("Gameplay Assistance")]
        [SerializeField] private Toggle pauseOnFocusLossToggle;
        [SerializeField] private Toggle autoPickupToggle;
        [SerializeField] private Toggle quickStackToggle;
        [SerializeField] private Toggle autoSortInventoryToggle;
        [SerializeField] private Toggle showDamageNumbersToggle;
        [SerializeField] private Toggle healthRegenerationToggle;
        [SerializeField] private Slider healthRegenRateSlider;
        [SerializeField] private TextMeshProUGUI healthRegenRateText;
        
        [Header("Time & Weather")]
        [SerializeField] private Slider gameSpeedSlider;
        [SerializeField] private TextMeshProUGUI gameSpeedText;
        [SerializeField] private Toggle pauseTimeInMenusToggle;
        [SerializeField] private Toggle dynamicWeatherToggle;
        [SerializeField] private Slider weatherIntensitySlider;
        [SerializeField] private TextMeshProUGUI weatherIntensityText;
        [SerializeField] private TMP_Dropdown seasonLengthDropdown;
        
        [Header("Economy & Resources")]
        [SerializeField] private Slider resourceSpawnRateSlider;
        [SerializeField] private TextMeshProUGUI resourceSpawnRateText;
        [SerializeField] private Slider shopPriceModifierSlider;
        [SerializeField] private TextMeshProUGUI shopPriceModifierText;
        [SerializeField] private Toggle infiniteResourcesToggle;
        [SerializeField] private Toggle debugModeToggle;
        [SerializeField] private Slider experienceMultiplierSlider;
        [SerializeField] private TextMeshProUGUI experienceMultiplierText;
        
        [Header("Performance & Quality")]
        [SerializeField] private Toggle limitFPSInBackgroundToggle;
        [SerializeField] private Slider backgroundFPSSlider;
        [SerializeField] private TextMeshProUGUI backgroundFPSText;
        [SerializeField] private Toggle enableVSyncInGameToggle;
        [SerializeField] private Toggle reducedAnimationsToggle;
        [SerializeField] private Toggle simplifiedParticlesToggle;
        
        [Header("Social & Multiplayer")]
        [SerializeField] private Toggle allowMultiplayerToggle;
        [SerializeField] private Toggle friendsOnlyToggle;
        [SerializeField] private Toggle showOnlineStatusToggle;
        [SerializeField] private Toggle allowInvitesToggle;
        [SerializeField] private TMP_Dropdown privacyLevelDropdown;
        [SerializeField] private Toggle crossPlatformToggle;
        
        [Header("Notifications")]
        [SerializeField] private Toggle enableNotificationsToggle;
        [SerializeField] private Toggle achievementNotificationsToggle;
        [SerializeField] private Toggle questNotificationsToggle;
        [SerializeField] private Toggle friendNotificationsToggle;
        [SerializeField] private Slider notificationDurationSlider;
        [SerializeField] private TextMeshProUGUI notificationDurationText;
        
        [Header("Advanced Gameplay")]
        [SerializeField] private Toggle experimentalFeaturesToggle;
        [SerializeField] private Toggle betaContentToggle;
        [SerializeField] private Toggle developerModeToggle;
        [SerializeField] private Toggle showFPSCounterToggle;
        [SerializeField] private Toggle enableConsoleToggle;
        [SerializeField] private Button exportSettingsButton;
        [SerializeField] private Button importSettingsButton;
        
        [Header("Presets")]
        [SerializeField] private Button casualPresetButton;
        [SerializeField] private Button normalPresetButton;
        [SerializeField] private Button hardcorePresetButton;
        [SerializeField] private Button customPresetButton;
        [SerializeField] private Button speedrunPresetButton;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem settingsChangeEffect;
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private TextMeshProUGUI confirmationText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        
        [Header("Audio")]
        [SerializeField] private AudioClip settingsChangeSound;
        [SerializeField] private AudioClip presetChangeSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip warningSound;
        
        // Gameplay settings data
        private GameplaySettings _currentGameplaySettings;
        private bool _isApplyingSettings = false;
        
        // Confirmation dialog state
        private System.Action _pendingConfirmAction;
        private string _pendingConfirmMessage;
        
        // Dropdown options
        private readonly string[] _difficultyOptions = { "Easy", "Normal", "Hard", "Expert", "Custom" };
        private readonly string[] _autosaveFrequencyOptions = { "Every Action", "Every 5 Minutes", "Every 10 Minutes", "Every 30 Minutes", "Hourly", "Manual Only" };
        private readonly string[] _seasonLengthOptions = { "7 Days", "14 Days", "28 Days", "56 Days", "Custom" };
        private readonly string[] _privacyLevelOptions = { "Public", "Friends Only", "Private" };

        protected override void Awake()
        {
            base.Awake();
            InitializeGameplaySettings();
        }

        protected override void Start()
        {
            base.Start();
            SetupGameplayPanel();
            LoadGameplaySettings();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Start hidden
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleGameplayInput();
        }

        private void InitializeGameplaySettings()
        {
            // Initialize current settings
            _currentGameplaySettings = new GameplaySettings();
        }

        private void SetupGameplayPanel()
        {
            // Setup difficulty controls
            SetupDifficultyControls();
            
            // Setup save & load controls
            SetupSaveLoadControls();
            
            // Setup tutorial controls
            SetupTutorialControls();
            
            // Setup gameplay assistance controls
            SetupGameplayAssistanceControls();
            
            // Setup time & weather controls
            SetupTimeWeatherControls();
            
            // Setup economy controls
            SetupEconomyControls();
            
            // Setup performance controls
            SetupPerformanceControls();
            
            // Setup social controls
            SetupSocialControls();
            
            // Setup notification controls
            SetupNotificationControls();
            
            // Setup advanced controls
            SetupAdvancedControls();
            
            // Setup preset buttons
            SetupPresetButtons();
            
            // Setup confirmation dialog
            SetupConfirmationDialog();
        }

        private void SetupDifficultyControls()
        {
            if (difficultyDropdown != null)
            {
                difficultyDropdown.ClearOptions();
                difficultyDropdown.AddOptions(_difficultyOptions.ToList());
                difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
            }
            
            if (difficultyCustomSlider != null)
            {
                difficultyCustomSlider.minValue = 0.1f;
                difficultyCustomSlider.maxValue = 3f;
                difficultyCustomSlider.onValueChanged.AddListener(OnCustomDifficultyChanged);
            }
            
            if (customDifficultyToggle != null)
                customDifficultyToggle.onValueChanged.AddListener(OnCustomDifficultyToggleChanged);
            
            if (resetDifficultyButton != null)
                resetDifficultyButton.onClick.AddListener(ResetDifficulty);
        }

        private void SetupSaveLoadControls()
        {
            if (autosaveToggle != null)
                autosaveToggle.onValueChanged.AddListener(OnAutosaveToggleChanged);
            
            if (autosaveIntervalSlider != null)
            {
                autosaveIntervalSlider.minValue = 1f;
                autosaveIntervalSlider.maxValue = 60f;
                autosaveIntervalSlider.onValueChanged.AddListener(OnAutosaveIntervalChanged);
            }
            
            if (autosaveFrequencyDropdown != null)
            {
                autosaveFrequencyDropdown.ClearOptions();
                autosaveFrequencyDropdown.AddOptions(_autosaveFrequencyOptions.ToList());
                autosaveFrequencyDropdown.onValueChanged.AddListener(OnAutosaveFrequencyChanged);
            }
            
            if (saveOnExitToggle != null)
                saveOnExitToggle.onValueChanged.AddListener(OnSaveOnExitChanged);
            
            if (cloudSaveToggle != null)
                cloudSaveToggle.onValueChanged.AddListener(OnCloudSaveChanged);
            
            if (manageSavesButton != null)
                manageSavesButton.onClick.AddListener(ManageSaves);
        }

        private void SetupTutorialControls()
        {
            if (showTutorialsToggle != null)
                showTutorialsToggle.onValueChanged.AddListener(OnShowTutorialsChanged);
            
            if (showHintsToggle != null)
                showHintsToggle.onValueChanged.AddListener(OnShowHintsChanged);
            
            if (showTooltipsToggle != null)
                showTooltipsToggle.onValueChanged.AddListener(OnShowTooltipsChanged);
            
            if (tutorialSpeedSlider != null)
            {
                tutorialSpeedSlider.minValue = 0.5f;
                tutorialSpeedSlider.maxValue = 3f;
                tutorialSpeedSlider.onValueChanged.AddListener(OnTutorialSpeedChanged);
            }
            
            if (skipSeenTutorialsToggle != null)
                skipSeenTutorialsToggle.onValueChanged.AddListener(OnSkipSeenTutorialsChanged);
            
            if (resetTutorialsButton != null)
                resetTutorialsButton.onClick.AddListener(ResetTutorials);
        }

        private void SetupGameplayAssistanceControls()
        {
            if (pauseOnFocusLossToggle != null)
                pauseOnFocusLossToggle.onValueChanged.AddListener(OnPauseOnFocusLossChanged);
            
            if (autoPickupToggle != null)
                autoPickupToggle.onValueChanged.AddListener(OnAutoPickupChanged);
            
            if (quickStackToggle != null)
                quickStackToggle.onValueChanged.AddListener(OnQuickStackChanged);
            
            if (autoSortInventoryToggle != null)
                autoSortInventoryToggle.onValueChanged.AddListener(OnAutoSortInventoryChanged);
            
            if (showDamageNumbersToggle != null)
                showDamageNumbersToggle.onValueChanged.AddListener(OnShowDamageNumbersChanged);
            
            if (healthRegenerationToggle != null)
                healthRegenerationToggle.onValueChanged.AddListener(OnHealthRegenerationChanged);
            
            if (healthRegenRateSlider != null)
            {
                healthRegenRateSlider.minValue = 0.1f;
                healthRegenRateSlider.maxValue = 5f;
                healthRegenRateSlider.onValueChanged.AddListener(OnHealthRegenRateChanged);
            }
        }

        private void SetupTimeWeatherControls()
        {
            if (gameSpeedSlider != null)
            {
                gameSpeedSlider.minValue = 0.25f;
                gameSpeedSlider.maxValue = 4f;
                gameSpeedSlider.onValueChanged.AddListener(OnGameSpeedChanged);
            }
            
            if (pauseTimeInMenusToggle != null)
                pauseTimeInMenusToggle.onValueChanged.AddListener(OnPauseTimeInMenusChanged);
            
            if (dynamicWeatherToggle != null)
                dynamicWeatherToggle.onValueChanged.AddListener(OnDynamicWeatherChanged);
            
            if (weatherIntensitySlider != null)
            {
                weatherIntensitySlider.minValue = 0f;
                weatherIntensitySlider.maxValue = 2f;
                weatherIntensitySlider.onValueChanged.AddListener(OnWeatherIntensityChanged);
            }
            
            if (seasonLengthDropdown != null)
            {
                seasonLengthDropdown.ClearOptions();
                seasonLengthDropdown.AddOptions(_seasonLengthOptions.ToList());
                seasonLengthDropdown.onValueChanged.AddListener(OnSeasonLengthChanged);
            }
        }

        private void SetupEconomyControls()
        {
            if (resourceSpawnRateSlider != null)
            {
                resourceSpawnRateSlider.minValue = 0.1f;
                resourceSpawnRateSlider.maxValue = 5f;
                resourceSpawnRateSlider.onValueChanged.AddListener(OnResourceSpawnRateChanged);
            }
            
            if (shopPriceModifierSlider != null)
            {
                shopPriceModifierSlider.minValue = 0.1f;
                shopPriceModifierSlider.maxValue = 3f;
                shopPriceModifierSlider.onValueChanged.AddListener(OnShopPriceModifierChanged);
            }
            
            if (infiniteResourcesToggle != null)
                infiniteResourcesToggle.onValueChanged.AddListener(OnInfiniteResourcesChanged);
            
            if (debugModeToggle != null)
                debugModeToggle.onValueChanged.AddListener(OnDebugModeChanged);
            
            if (experienceMultiplierSlider != null)
            {
                experienceMultiplierSlider.minValue = 0.1f;
                experienceMultiplierSlider.maxValue = 10f;
                experienceMultiplierSlider.onValueChanged.AddListener(OnExperienceMultiplierChanged);
            }
        }

        private void SetupPerformanceControls()
        {
            if (limitFPSInBackgroundToggle != null)
                limitFPSInBackgroundToggle.onValueChanged.AddListener(OnLimitFPSInBackgroundChanged);
            
            if (backgroundFPSSlider != null)
            {
                backgroundFPSSlider.minValue = 5f;
                backgroundFPSSlider.maxValue = 60f;
                backgroundFPSSlider.onValueChanged.AddListener(OnBackgroundFPSChanged);
            }
            
            if (enableVSyncInGameToggle != null)
                enableVSyncInGameToggle.onValueChanged.AddListener(OnEnableVSyncInGameChanged);
            
            if (reducedAnimationsToggle != null)
                reducedAnimationsToggle.onValueChanged.AddListener(OnReducedAnimationsChanged);
            
            if (simplifiedParticlesToggle != null)
                simplifiedParticlesToggle.onValueChanged.AddListener(OnSimplifiedParticlesChanged);
        }

        private void SetupSocialControls()
        {
            if (allowMultiplayerToggle != null)
                allowMultiplayerToggle.onValueChanged.AddListener(OnAllowMultiplayerChanged);
            
            if (friendsOnlyToggle != null)
                friendsOnlyToggle.onValueChanged.AddListener(OnFriendsOnlyChanged);
            
            if (showOnlineStatusToggle != null)
                showOnlineStatusToggle.onValueChanged.AddListener(OnShowOnlineStatusChanged);
            
            if (allowInvitesToggle != null)
                allowInvitesToggle.onValueChanged.AddListener(OnAllowInvitesChanged);
            
            if (privacyLevelDropdown != null)
            {
                privacyLevelDropdown.ClearOptions();
                privacyLevelDropdown.AddOptions(_privacyLevelOptions.ToList());
                privacyLevelDropdown.onValueChanged.AddListener(OnPrivacyLevelChanged);
            }
            
            if (crossPlatformToggle != null)
                crossPlatformToggle.onValueChanged.AddListener(OnCrossPlatformChanged);
        }

        private void SetupNotificationControls()
        {
            if (enableNotificationsToggle != null)
                enableNotificationsToggle.onValueChanged.AddListener(OnEnableNotificationsChanged);
            
            if (achievementNotificationsToggle != null)
                achievementNotificationsToggle.onValueChanged.AddListener(OnAchievementNotificationsChanged);
            
            if (questNotificationsToggle != null)
                questNotificationsToggle.onValueChanged.AddListener(OnQuestNotificationsChanged);
            
            if (friendNotificationsToggle != null)
                friendNotificationsToggle.onValueChanged.AddListener(OnFriendNotificationsChanged);
            
            if (notificationDurationSlider != null)
            {
                notificationDurationSlider.minValue = 1f;
                notificationDurationSlider.maxValue = 10f;
                notificationDurationSlider.onValueChanged.AddListener(OnNotificationDurationChanged);
            }
        }

        private void SetupAdvancedControls()
        {
            if (experimentalFeaturesToggle != null)
                experimentalFeaturesToggle.onValueChanged.AddListener(OnExperimentalFeaturesChanged);
            
            if (betaContentToggle != null)
                betaContentToggle.onValueChanged.AddListener(OnBetaContentChanged);
            
            if (developerModeToggle != null)
                developerModeToggle.onValueChanged.AddListener(OnDeveloperModeChanged);
            
            if (showFPSCounterToggle != null)
                showFPSCounterToggle.onValueChanged.AddListener(OnShowFPSCounterChanged);
            
            if (enableConsoleToggle != null)
                enableConsoleToggle.onValueChanged.AddListener(OnEnableConsoleChanged);
            
            if (exportSettingsButton != null)
                exportSettingsButton.onClick.AddListener(ExportSettings);
            
            if (importSettingsButton != null)
                importSettingsButton.onClick.AddListener(ImportSettings);
        }

        private void SetupPresetButtons()
        {
            if (casualPresetButton != null)
                casualPresetButton.onClick.AddListener(() => ApplyGameplayPreset(GameplayPreset.Casual));
            
            if (normalPresetButton != null)
                normalPresetButton.onClick.AddListener(() => ApplyGameplayPreset(GameplayPreset.Normal));
            
            if (hardcorePresetButton != null)
                hardcorePresetButton.onClick.AddListener(() => ApplyGameplayPreset(GameplayPreset.Hardcore));
            
            if (speedrunPresetButton != null)
                speedrunPresetButton.onClick.AddListener(() => ApplyGameplayPreset(GameplayPreset.Speedrun));
            
            if (customPresetButton != null)
                customPresetButton.onClick.AddListener(() => ApplyGameplayPreset(GameplayPreset.Custom));
        }

        private void SetupConfirmationDialog()
        {
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(ConfirmAction);
            
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(CancelConfirmation);
            
            HideConfirmationDialog();
        }

        // Input handling
        private void HandleGameplayInput()
        {
            // ESC to close confirmation dialog
            if (Input.GetKeyDown(KeyCode.Escape) && confirmationDialog != null && confirmationDialog.activeInHierarchy)
            {
                CancelConfirmation();
            }
        }

        private void LoadGameplaySettings()
        {
            // Load gameplay settings from PlayerPrefs
            _currentGameplaySettings.difficulty = (GameplayDifficulty)PlayerPrefs.GetInt("GameplayDifficulty", 1);
            _currentGameplaySettings.customDifficultyMultiplier = PlayerPrefs.GetFloat("CustomDifficultyMultiplier", 1f);
            _currentGameplaySettings.autosaveEnabled = PlayerPrefs.GetInt("AutosaveEnabled", 1) == 1;
            _currentGameplaySettings.autosaveInterval = PlayerPrefs.GetFloat("AutosaveInterval", 5f);
            _currentGameplaySettings.autosaveFrequency = PlayerPrefs.GetInt("AutosaveFrequency", 1);
            _currentGameplaySettings.saveOnExit = PlayerPrefs.GetInt("SaveOnExit", 1) == 1;
            _currentGameplaySettings.cloudSave = PlayerPrefs.GetInt("CloudSave", 0) == 1;
            
            _currentGameplaySettings.showTutorials = PlayerPrefs.GetInt("ShowTutorials", 1) == 1;
            _currentGameplaySettings.showHints = PlayerPrefs.GetInt("ShowHints", 1) == 1;
            _currentGameplaySettings.showTooltips = PlayerPrefs.GetInt("ShowTooltips", 1) == 1;
            _currentGameplaySettings.tutorialSpeed = PlayerPrefs.GetFloat("TutorialSpeed", 1f);
            _currentGameplaySettings.skipSeenTutorials = PlayerPrefs.GetInt("SkipSeenTutorials", 0) == 1;
            
            _currentGameplaySettings.pauseOnFocusLoss = PlayerPrefs.GetInt("PauseOnFocusLoss", 1) == 1;
            _currentGameplaySettings.autoPickup = PlayerPrefs.GetInt("AutoPickup", 0) == 1;
            _currentGameplaySettings.quickStack = PlayerPrefs.GetInt("QuickStack", 1) == 1;
            _currentGameplaySettings.autoSortInventory = PlayerPrefs.GetInt("AutoSortInventory", 0) == 1;
            _currentGameplaySettings.showDamageNumbers = PlayerPrefs.GetInt("ShowDamageNumbers", 1) == 1;
            _currentGameplaySettings.healthRegeneration = PlayerPrefs.GetInt("HealthRegeneration", 1) == 1;
            _currentGameplaySettings.healthRegenRate = PlayerPrefs.GetFloat("HealthRegenRate", 1f);
            
            _currentGameplaySettings.gameSpeed = PlayerPrefs.GetFloat("GameSpeed", 1f);
            _currentGameplaySettings.pauseTimeInMenus = PlayerPrefs.GetInt("PauseTimeInMenus", 1) == 1;
            _currentGameplaySettings.dynamicWeather = PlayerPrefs.GetInt("DynamicWeather", 1) == 1;
            _currentGameplaySettings.weatherIntensity = PlayerPrefs.GetFloat("WeatherIntensity", 1f);
            _currentGameplaySettings.seasonLength = PlayerPrefs.GetInt("SeasonLength", 2);
            
            _currentGameplaySettings.resourceSpawnRate = PlayerPrefs.GetFloat("ResourceSpawnRate", 1f);
            _currentGameplaySettings.shopPriceModifier = PlayerPrefs.GetFloat("ShopPriceModifier", 1f);
            _currentGameplaySettings.infiniteResources = PlayerPrefs.GetInt("InfiniteResources", 0) == 1;
            _currentGameplaySettings.debugMode = PlayerPrefs.GetInt("DebugMode", 0) == 1;
            _currentGameplaySettings.experienceMultiplier = PlayerPrefs.GetFloat("ExperienceMultiplier", 1f);
            
            _currentGameplaySettings.limitFPSInBackground = PlayerPrefs.GetInt("LimitFPSInBackground", 1) == 1;
            _currentGameplaySettings.backgroundFPS = PlayerPrefs.GetFloat("BackgroundFPS", 30f);
            _currentGameplaySettings.enableVSyncInGame = PlayerPrefs.GetInt("EnableVSyncInGame", 1) == 1;
            _currentGameplaySettings.reducedAnimations = PlayerPrefs.GetInt("ReducedAnimations", 0) == 1;
            _currentGameplaySettings.simplifiedParticles = PlayerPrefs.GetInt("SimplifiedParticles", 0) == 1;
            
            _currentGameplaySettings.allowMultiplayer = PlayerPrefs.GetInt("AllowMultiplayer", 1) == 1;
            _currentGameplaySettings.friendsOnly = PlayerPrefs.GetInt("FriendsOnly", 0) == 1;
            _currentGameplaySettings.showOnlineStatus = PlayerPrefs.GetInt("ShowOnlineStatus", 1) == 1;
            _currentGameplaySettings.allowInvites = PlayerPrefs.GetInt("AllowInvites", 1) == 1;
            _currentGameplaySettings.privacyLevel = PlayerPrefs.GetInt("PrivacyLevel", 0);
            _currentGameplaySettings.crossPlatform = PlayerPrefs.GetInt("CrossPlatform", 1) == 1;
            
            _currentGameplaySettings.enableNotifications = PlayerPrefs.GetInt("EnableNotifications", 1) == 1;
            _currentGameplaySettings.achievementNotifications = PlayerPrefs.GetInt("AchievementNotifications", 1) == 1;
            _currentGameplaySettings.questNotifications = PlayerPrefs.GetInt("QuestNotifications", 1) == 1;
            _currentGameplaySettings.friendNotifications = PlayerPrefs.GetInt("FriendNotifications", 1) == 1;
            _currentGameplaySettings.notificationDuration = PlayerPrefs.GetFloat("NotificationDuration", 5f);
            
            _currentGameplaySettings.experimentalFeatures = PlayerPrefs.GetInt("ExperimentalFeatures", 0) == 1;
            _currentGameplaySettings.betaContent = PlayerPrefs.GetInt("BetaContent", 0) == 1;
            _currentGameplaySettings.developerMode = PlayerPrefs.GetInt("DeveloperMode", 0) == 1;
            _currentGameplaySettings.showFPSCounter = PlayerPrefs.GetInt("ShowFPSCounter", 0) == 1;
            _currentGameplaySettings.enableConsole = PlayerPrefs.GetInt("EnableConsole", 0) == 1;
            
            ApplySettingsToUI(_currentGameplaySettings);
        }

        private void ApplySettingsToUI(GameplaySettings settings)
        {
            _isApplyingSettings = true;
            
            // Difficulty settings
            if (difficultyDropdown != null)
                difficultyDropdown.value = (int)settings.difficulty;
            
            if (difficultyCustomSlider != null)
            {
                difficultyCustomSlider.value = settings.customDifficultyMultiplier;
                difficultyCustomSlider.gameObject.SetActive(settings.difficulty == GameplayDifficulty.Custom);
            }
            
            if (customDifficultyToggle != null)
                customDifficultyToggle.isOn = settings.difficulty == GameplayDifficulty.Custom;
            
            UpdateDifficultyDescription(settings.difficulty);
            
            // Save & load settings
            if (autosaveToggle != null)
                autosaveToggle.isOn = settings.autosaveEnabled;
            
            if (autosaveIntervalSlider != null)
            {
                autosaveIntervalSlider.value = settings.autosaveInterval;
                UpdateAutosaveIntervalText(settings.autosaveInterval);
            }
            
            if (autosaveFrequencyDropdown != null)
                autosaveFrequencyDropdown.value = settings.autosaveFrequency;
            
            if (saveOnExitToggle != null)
                saveOnExitToggle.isOn = settings.saveOnExit;
            
            if (cloudSaveToggle != null)
                cloudSaveToggle.isOn = settings.cloudSave;
            
            UpdateSaveInfo();
            
            // Tutorial settings
            if (showTutorialsToggle != null)
                showTutorialsToggle.isOn = settings.showTutorials;
            
            if (showHintsToggle != null)
                showHintsToggle.isOn = settings.showHints;
            
            if (showTooltipsToggle != null)
                showTooltipsToggle.isOn = settings.showTooltips;
            
            if (tutorialSpeedSlider != null)
            {
                tutorialSpeedSlider.value = settings.tutorialSpeed;
                UpdateTutorialSpeedText(settings.tutorialSpeed);
            }
            
            if (skipSeenTutorialsToggle != null)
                skipSeenTutorialsToggle.isOn = settings.skipSeenTutorials;
            
            // Gameplay assistance
            if (pauseOnFocusLossToggle != null)
                pauseOnFocusLossToggle.isOn = settings.pauseOnFocusLoss;
            
            if (autoPickupToggle != null)
                autoPickupToggle.isOn = settings.autoPickup;
            
            if (quickStackToggle != null)
                quickStackToggle.isOn = settings.quickStack;
            
            if (autoSortInventoryToggle != null)
                autoSortInventoryToggle.isOn = settings.autoSortInventory;
            
            if (showDamageNumbersToggle != null)
                showDamageNumbersToggle.isOn = settings.showDamageNumbers;
            
            if (healthRegenerationToggle != null)
                healthRegenerationToggle.isOn = settings.healthRegeneration;
            
            if (healthRegenRateSlider != null)
            {
                healthRegenRateSlider.value = settings.healthRegenRate;
                UpdateHealthRegenRateText(settings.healthRegenRate);
            }
            
            // Time & weather
            if (gameSpeedSlider != null)
            {
                gameSpeedSlider.value = settings.gameSpeed;
                UpdateGameSpeedText(settings.gameSpeed);
            }
            
            if (pauseTimeInMenusToggle != null)
                pauseTimeInMenusToggle.isOn = settings.pauseTimeInMenus;
            
            if (dynamicWeatherToggle != null)
                dynamicWeatherToggle.isOn = settings.dynamicWeather;
            
            if (weatherIntensitySlider != null)
            {
                weatherIntensitySlider.value = settings.weatherIntensity;
                UpdateWeatherIntensityText(settings.weatherIntensity);
            }
            
            if (seasonLengthDropdown != null)
                seasonLengthDropdown.value = settings.seasonLength;
            
            // Economy & resources
            if (resourceSpawnRateSlider != null)
            {
                resourceSpawnRateSlider.value = settings.resourceSpawnRate;
                UpdateResourceSpawnRateText(settings.resourceSpawnRate);
            }
            
            if (shopPriceModifierSlider != null)
            {
                shopPriceModifierSlider.value = settings.shopPriceModifier;
                UpdateShopPriceModifierText(settings.shopPriceModifier);
            }
            
            if (infiniteResourcesToggle != null)
                infiniteResourcesToggle.isOn = settings.infiniteResources;
            
            if (debugModeToggle != null)
                debugModeToggle.isOn = settings.debugMode;
            
            if (experienceMultiplierSlider != null)
            {
                experienceMultiplierSlider.value = settings.experienceMultiplier;
                UpdateExperienceMultiplierText(settings.experienceMultiplier);
            }
            
            // Performance & quality
            if (limitFPSInBackgroundToggle != null)
                limitFPSInBackgroundToggle.isOn = settings.limitFPSInBackground;
            
            if (backgroundFPSSlider != null)
            {
                backgroundFPSSlider.value = settings.backgroundFPS;
                UpdateBackgroundFPSText(settings.backgroundFPS);
            }
            
            if (enableVSyncInGameToggle != null)
                enableVSyncInGameToggle.isOn = settings.enableVSyncInGame;
            
            if (reducedAnimationsToggle != null)
                reducedAnimationsToggle.isOn = settings.reducedAnimations;
            
            if (simplifiedParticlesToggle != null)
                simplifiedParticlesToggle.isOn = settings.simplifiedParticles;
            
            // Social & multiplayer
            if (allowMultiplayerToggle != null)
                allowMultiplayerToggle.isOn = settings.allowMultiplayer;
            
            if (friendsOnlyToggle != null)
                friendsOnlyToggle.isOn = settings.friendsOnly;
            
            if (showOnlineStatusToggle != null)
                showOnlineStatusToggle.isOn = settings.showOnlineStatus;
            
            if (allowInvitesToggle != null)
                allowInvitesToggle.isOn = settings.allowInvites;
            
            if (privacyLevelDropdown != null)
                privacyLevelDropdown.value = settings.privacyLevel;
            
            if (crossPlatformToggle != null)
                crossPlatformToggle.isOn = settings.crossPlatform;
            
            // Notifications
            if (enableNotificationsToggle != null)
                enableNotificationsToggle.isOn = settings.enableNotifications;
            
            if (achievementNotificationsToggle != null)
                achievementNotificationsToggle.isOn = settings.achievementNotifications;
            
            if (questNotificationsToggle != null)
                questNotificationsToggle.isOn = settings.questNotifications;
            
            if (friendNotificationsToggle != null)
                friendNotificationsToggle.isOn = settings.friendNotifications;
            
            if (notificationDurationSlider != null)
            {
                notificationDurationSlider.value = settings.notificationDuration;
                UpdateNotificationDurationText(settings.notificationDuration);
            }
            
            // Advanced
            if (experimentalFeaturesToggle != null)
                experimentalFeaturesToggle.isOn = settings.experimentalFeatures;
            
            if (betaContentToggle != null)
                betaContentToggle.isOn = settings.betaContent;
            
            if (developerModeToggle != null)
                developerModeToggle.isOn = settings.developerMode;
            
            if (showFPSCounterToggle != null)
                showFPSCounterToggle.isOn = settings.showFPSCounter;
            
            if (enableConsoleToggle != null)
                enableConsoleToggle.isOn = settings.enableConsole;
            
            _isApplyingSettings = false;
        }

        // Event handlers - Difficulty
        private void OnDifficultyChanged(int difficulty)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.difficulty = (GameplayDifficulty)difficulty;
            UpdateDifficultyDescription(_currentGameplaySettings.difficulty);
            
            if (difficultyCustomSlider != null)
                difficultyCustomSlider.gameObject.SetActive(difficulty == 4); // Custom
            
            NotifySettingsChanged();
        }

        private void OnCustomDifficultyChanged(float multiplier)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.customDifficultyMultiplier = multiplier;
            UpdateDifficultyDescription(GameplayDifficulty.Custom);
            NotifySettingsChanged();
        }

        private void OnCustomDifficultyToggleChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            if (enabled)
            {
                _currentGameplaySettings.difficulty = GameplayDifficulty.Custom;
                if (difficultyDropdown != null)
                    difficultyDropdown.value = 4;
            }
            
            NotifySettingsChanged();
        }

        private void ResetDifficulty()
        {
            PlayButtonClickSound();
            
            _currentGameplaySettings.difficulty = GameplayDifficulty.Normal;
            _currentGameplaySettings.customDifficultyMultiplier = 1f;
            
            if (difficultyDropdown != null)
                difficultyDropdown.value = 1;
            
            if (difficultyCustomSlider != null)
                difficultyCustomSlider.value = 1f;
            
            UpdateDifficultyDescription(GameplayDifficulty.Normal);
            NotifySettingsChanged();
        }

        private void UpdateDifficultyDescription(GameplayDifficulty difficulty)
        {
            if (difficultyDescription == null) return;
            
            string description = difficulty switch
            {
                GameplayDifficulty.Easy => "Easier enemies, more resources, helpful hints enabled",
                GameplayDifficulty.Normal => "Balanced gameplay experience for most players",
                GameplayDifficulty.Hard => "Stronger enemies, fewer resources, limited saves",
                GameplayDifficulty.Expert => "Very challenging, for experienced players only",
                GameplayDifficulty.Custom => $"Custom difficulty (x{_currentGameplaySettings.customDifficultyMultiplier:F1} multiplier)",
                _ => "Standard gameplay experience"
            };
            
            difficultyDescription.text = description;
        }

        // Event handlers - Save & Load
        private void OnAutosaveToggleChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.autosaveEnabled = enabled;
            NotifySettingsChanged();
        }

        private void OnAutosaveIntervalChanged(float interval)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.autosaveInterval = interval;
            UpdateAutosaveIntervalText(interval);
            NotifySettingsChanged();
        }

        private void OnAutosaveFrequencyChanged(int frequency)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.autosaveFrequency = frequency;
            NotifySettingsChanged();
        }

        private void OnSaveOnExitChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.saveOnExit = enabled;
            NotifySettingsChanged();
        }

        private void OnCloudSaveChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.cloudSave = enabled;
            UpdateSaveInfo();
            NotifySettingsChanged();
        }

        private void ManageSaves()
        {
            PlayButtonClickSound();
            Debug.Log("Save management functionality would be implemented here");
        }

        private void UpdateAutosaveIntervalText(float interval)
        {
            if (autosaveIntervalText != null)
                autosaveIntervalText.text = $"{interval:F0} minutes";
        }

        private void UpdateSaveInfo()
        {
            if (saveInfoText != null)
            {
                string info = _currentGameplaySettings.cloudSave ? 
                    "Cloud saves enabled - syncing across devices" : 
                    "Local saves only - stored on this device";
                saveInfoText.text = info;
            }
        }

        // Event handlers - Tutorial
        private void OnShowTutorialsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.showTutorials = enabled;
            NotifySettingsChanged();
        }

        private void OnShowHintsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.showHints = enabled;
            NotifySettingsChanged();
        }

        private void OnShowTooltipsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.showTooltips = enabled;
            NotifySettingsChanged();
        }

        private void OnTutorialSpeedChanged(float speed)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.tutorialSpeed = speed;
            UpdateTutorialSpeedText(speed);
            NotifySettingsChanged();
        }

        private void OnSkipSeenTutorialsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.skipSeenTutorials = enabled;
            NotifySettingsChanged();
        }

        private void ResetTutorials()
        {
            ShowConfirmationDialog(
                "Reset all tutorial progress?\nThis will show all tutorials again.",
                () => {
                    PlayButtonClickSound();
                    Debug.Log("Tutorial progress would be reset here");
                }
            );
        }

        private void UpdateTutorialSpeedText(float speed)
        {
            if (tutorialSpeedText != null)
                tutorialSpeedText.text = $"{speed:F1}x speed";
        }

        // Event handlers - Gameplay Assistance
        private void OnPauseOnFocusLossChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.pauseOnFocusLoss = enabled;
            NotifySettingsChanged();
        }

        private void OnAutoPickupChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.autoPickup = enabled;
            NotifySettingsChanged();
        }

        private void OnQuickStackChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.quickStack = enabled;
            NotifySettingsChanged();
        }

        private void OnAutoSortInventoryChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.autoSortInventory = enabled;
            NotifySettingsChanged();
        }

        private void OnShowDamageNumbersChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.showDamageNumbers = enabled;
            NotifySettingsChanged();
        }

        private void OnHealthRegenerationChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.healthRegeneration = enabled;
            NotifySettingsChanged();
        }

        private void OnHealthRegenRateChanged(float rate)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.healthRegenRate = rate;
            UpdateHealthRegenRateText(rate);
            NotifySettingsChanged();
        }

        private void UpdateHealthRegenRateText(float rate)
        {
            if (healthRegenRateText != null)
                healthRegenRateText.text = $"{rate:F1}x rate";
        }

        // Event handlers - Time & Weather
        private void OnGameSpeedChanged(float speed)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.gameSpeed = speed;
            UpdateGameSpeedText(speed);
            NotifySettingsChanged();
        }

        private void OnPauseTimeInMenusChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.pauseTimeInMenus = enabled;
            NotifySettingsChanged();
        }

        private void OnDynamicWeatherChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.dynamicWeather = enabled;
            NotifySettingsChanged();
        }

        private void OnWeatherIntensityChanged(float intensity)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.weatherIntensity = intensity;
            UpdateWeatherIntensityText(intensity);
            NotifySettingsChanged();
        }

        private void OnSeasonLengthChanged(int length)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.seasonLength = length;
            NotifySettingsChanged();
        }

        private void UpdateGameSpeedText(float speed)
        {
            if (gameSpeedText != null)
                gameSpeedText.text = $"{speed:F2}x speed";
        }

        private void UpdateWeatherIntensityText(float intensity)
        {
            if (weatherIntensityText != null)
                weatherIntensityText.text = $"{intensity:F1}x intensity";
        }

        // Event handlers - Economy
        private void OnResourceSpawnRateChanged(float rate)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.resourceSpawnRate = rate;
            UpdateResourceSpawnRateText(rate);
            NotifySettingsChanged();
        }

        private void OnShopPriceModifierChanged(float modifier)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.shopPriceModifier = modifier;
            UpdateShopPriceModifierText(modifier);
            NotifySettingsChanged();
        }

        private void OnInfiniteResourcesChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            if (enabled)
            {
                ShowConfirmationDialog(
                    "Enable infinite resources?\nThis may affect game balance and achievements.",
                    () => {
                        _currentGameplaySettings.infiniteResources = true;
                        NotifySettingsChanged();
                    }
                );
            }
            else
            {
                _currentGameplaySettings.infiniteResources = false;
                NotifySettingsChanged();
            }
        }

        private void OnDebugModeChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            if (enabled)
            {
                ShowConfirmationDialog(
                    "Enable debug mode?\nThis is intended for developers and may cause instability.",
                    () => {
                        _currentGameplaySettings.debugMode = true;
                        NotifySettingsChanged();
                    }
                );
            }
            else
            {
                _currentGameplaySettings.debugMode = false;
                NotifySettingsChanged();
            }
        }

        private void OnExperienceMultiplierChanged(float multiplier)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.experienceMultiplier = multiplier;
            UpdateExperienceMultiplierText(multiplier);
            NotifySettingsChanged();
        }

        private void UpdateResourceSpawnRateText(float rate)
        {
            if (resourceSpawnRateText != null)
                resourceSpawnRateText.text = $"{rate:F1}x rate";
        }

        private void UpdateShopPriceModifierText(float modifier)
        {
            if (shopPriceModifierText != null)
                shopPriceModifierText.text = $"{modifier:F1}x prices";
        }

        private void UpdateExperienceMultiplierText(float multiplier)
        {
            if (experienceMultiplierText != null)
                experienceMultiplierText.text = $"{multiplier:F1}x XP";
        }

        // Event handlers - Performance
        private void OnLimitFPSInBackgroundChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.limitFPSInBackground = enabled;
            NotifySettingsChanged();
        }

        private void OnBackgroundFPSChanged(float fps)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.backgroundFPS = fps;
            UpdateBackgroundFPSText(fps);
            NotifySettingsChanged();
        }

        private void OnEnableVSyncInGameChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.enableVSyncInGame = enabled;
            NotifySettingsChanged();
        }

        private void OnReducedAnimationsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.reducedAnimations = enabled;
            NotifySettingsChanged();
        }

        private void OnSimplifiedParticlesChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.simplifiedParticles = enabled;
            NotifySettingsChanged();
        }

        private void UpdateBackgroundFPSText(float fps)
        {
            if (backgroundFPSText != null)
                backgroundFPSText.text = $"{fps:F0} FPS";
        }

        // Event handlers - Social
        private void OnAllowMultiplayerChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.allowMultiplayer = enabled;
            NotifySettingsChanged();
        }

        private void OnFriendsOnlyChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.friendsOnly = enabled;
            NotifySettingsChanged();
        }

        private void OnShowOnlineStatusChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.showOnlineStatus = enabled;
            NotifySettingsChanged();
        }

        private void OnAllowInvitesChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.allowInvites = enabled;
            NotifySettingsChanged();
        }

        private void OnPrivacyLevelChanged(int level)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.privacyLevel = level;
            NotifySettingsChanged();
        }

        private void OnCrossPlatformChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.crossPlatform = enabled;
            NotifySettingsChanged();
        }

        // Event handlers - Notifications
        private void OnEnableNotificationsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.enableNotifications = enabled;
            NotifySettingsChanged();
        }

        private void OnAchievementNotificationsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.achievementNotifications = enabled;
            NotifySettingsChanged();
        }

        private void OnQuestNotificationsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.questNotifications = enabled;
            NotifySettingsChanged();
        }

        private void OnFriendNotificationsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.friendNotifications = enabled;
            NotifySettingsChanged();
        }

        private void OnNotificationDurationChanged(float duration)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.notificationDuration = duration;
            UpdateNotificationDurationText(duration);
            NotifySettingsChanged();
        }

        private void UpdateNotificationDurationText(float duration)
        {
            if (notificationDurationText != null)
                notificationDurationText.text = $"{duration:F0} seconds";
        }

        // Event handlers - Advanced
        private void OnExperimentalFeaturesChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            if (enabled)
            {
                ShowConfirmationDialog(
                    "Enable experimental features?\nThese features are unstable and may cause issues.",
                    () => {
                        _currentGameplaySettings.experimentalFeatures = true;
                        NotifySettingsChanged();
                    }
                );
            }
            else
            {
                _currentGameplaySettings.experimentalFeatures = false;
                NotifySettingsChanged();
            }
        }

        private void OnBetaContentChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.betaContent = enabled;
            NotifySettingsChanged();
        }

        private void OnDeveloperModeChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            if (enabled)
            {
                ShowConfirmationDialog(
                    "Enable developer mode?\nThis provides access to debug tools and cheats.",
                    () => {
                        _currentGameplaySettings.developerMode = true;
                        NotifySettingsChanged();
                    }
                );
            }
            else
            {
                _currentGameplaySettings.developerMode = false;
                NotifySettingsChanged();
            }
        }

        private void OnShowFPSCounterChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.showFPSCounter = enabled;
            NotifySettingsChanged();
        }

        private void OnEnableConsoleChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGameplaySettings.enableConsole = enabled;
            NotifySettingsChanged();
        }

        private void ExportSettings()
        {
            PlayButtonClickSound();
            Debug.Log("Settings export functionality would be implemented here");
        }

        private void ImportSettings()
        {
            PlayButtonClickSound();
            Debug.Log("Settings import functionality would be implemented here");
        }

        // Preset management
        private void ApplyGameplayPreset(GameplayPreset preset)
        {
            PlayPresetChangeSound();
            
            switch (preset)
            {
                case GameplayPreset.Casual:
                    ApplyCasualPreset();
                    break;
                case GameplayPreset.Normal:
                    ApplyNormalPreset();
                    break;
                case GameplayPreset.Hardcore:
                    ApplyHardcorePreset();
                    break;
                case GameplayPreset.Speedrun:
                    ApplySpeedrunPreset();
                    break;
            }
            
            ApplySettingsToUI(_currentGameplaySettings);
            NotifySettingsChanged();
            
            if (settingsChangeEffect != null)
                settingsChangeEffect.Play();
        }

        private void ApplyCasualPreset()
        {
            _currentGameplaySettings.difficulty = GameplayDifficulty.Easy;
            _currentGameplaySettings.autosaveEnabled = true;
            _currentGameplaySettings.autosaveInterval = 2f;
            _currentGameplaySettings.showTutorials = true;
            _currentGameplaySettings.showHints = true;
            _currentGameplaySettings.autoPickup = true;
            _currentGameplaySettings.healthRegeneration = true;
            _currentGameplaySettings.healthRegenRate = 2f;
            _currentGameplaySettings.pauseOnFocusLoss = true;
        }

        private void ApplyNormalPreset()
        {
            _currentGameplaySettings.difficulty = GameplayDifficulty.Normal;
            _currentGameplaySettings.autosaveEnabled = true;
            _currentGameplaySettings.autosaveInterval = 5f;
            _currentGameplaySettings.showTutorials = true;
            _currentGameplaySettings.showHints = false;
            _currentGameplaySettings.autoPickup = false;
            _currentGameplaySettings.healthRegeneration = true;
            _currentGameplaySettings.healthRegenRate = 1f;
            _currentGameplaySettings.pauseOnFocusLoss = true;
        }

        private void ApplyHardcorePreset()
        {
            _currentGameplaySettings.difficulty = GameplayDifficulty.Expert;
            _currentGameplaySettings.autosaveEnabled = false;
            _currentGameplaySettings.showTutorials = false;
            _currentGameplaySettings.showHints = false;
            _currentGameplaySettings.autoPickup = false;
            _currentGameplaySettings.healthRegeneration = false;
            _currentGameplaySettings.pauseOnFocusLoss = false;
            _currentGameplaySettings.showDamageNumbers = false;
        }

        private void ApplySpeedrunPreset()
        {
            _currentGameplaySettings.difficulty = GameplayDifficulty.Normal;
            _currentGameplaySettings.gameSpeed = 1.5f;
            _currentGameplaySettings.autosaveEnabled = false;
            _currentGameplaySettings.showTutorials = false;
            _currentGameplaySettings.skipSeenTutorials = true;
            _currentGameplaySettings.autoPickup = true;
            _currentGameplaySettings.quickStack = true;
            _currentGameplaySettings.pauseTimeInMenus = false;
            _currentGameplaySettings.showFPSCounter = true;
        }

        // Confirmation dialog
        private void ShowConfirmationDialog(string message, System.Action confirmAction)
        {
            if (confirmationDialog == null) return;
            
            _pendingConfirmAction = confirmAction;
            _pendingConfirmMessage = message;
            
            if (confirmationText != null)
                confirmationText.text = message;
            
            confirmationDialog.SetActive(true);
            PlayWarningSound();
        }

        private void HideConfirmationDialog()
        {
            if (confirmationDialog != null)
            {
                confirmationDialog.SetActive(false);
            }
            
            _pendingConfirmAction = null;
            _pendingConfirmMessage = null;
        }

        private void ConfirmAction()
        {
            PlayButtonClickSound();
            
            _pendingConfirmAction?.Invoke();
            HideConfirmationDialog();
        }

        private void CancelConfirmation()
        {
            PlayButtonClickSound();
            
            // Revert any toggle that might have triggered this
            if (_pendingConfirmMessage != null)
            {
                ApplySettingsToUI(_currentGameplaySettings);
            }
            
            HideConfirmationDialog();
        }

        // Settings interface
        public void ApplySettings(SettingsData settings)
        {
            _currentGameplaySettings.autosaveEnabled = settings.autosaveEnabled;
            _currentGameplaySettings.autosaveInterval = settings.autosaveInterval;
            _currentGameplaySettings.showTutorials = settings.showTutorials;
            _currentGameplaySettings.pauseOnFocusLoss = settings.pauseOnFocusLoss;
            
            ApplySettingsToUI(_currentGameplaySettings);
        }

        public void CollectSettings(ref SettingsData settings)
        {
            settings.autosaveEnabled = _currentGameplaySettings.autosaveEnabled;
            settings.autosaveInterval = _currentGameplaySettings.autosaveInterval;
            settings.showTutorials = _currentGameplaySettings.showTutorials;
            settings.pauseOnFocusLoss = _currentGameplaySettings.pauseOnFocusLoss;
        }

        private void NotifySettingsChanged()
        {
            var settingsPanel = GetComponentInParent<SettingsPanel>();
            settingsPanel?.OnSettingsChanged();
        }

        // Audio methods
        private void PlaySettingsChangeSound()
        {
            Debug.Log("Settings change sound would play here");
        }

        private void PlayPresetChangeSound()
        {
            Debug.Log("Preset change sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        private void PlayWarningSound()
        {
            Debug.Log("Warning sound would play here");
        }

        // Public interface
        public GameplaySettings CurrentGameplaySettings => _currentGameplaySettings;
    }

    // Data structures and enums
    [System.Serializable]
    public class GameplaySettings
    {
        [Header("Difficulty")]
        public GameplayDifficulty difficulty = GameplayDifficulty.Normal;
        public float customDifficultyMultiplier = 1f;
        
        [Header("Save & Load")]
        public bool autosaveEnabled = true;
        public float autosaveInterval = 5f;
        public int autosaveFrequency = 1;
        public bool saveOnExit = true;
        public bool cloudSave = false;
        
        [Header("Tutorial & Help")]
        public bool showTutorials = true;
        public bool showHints = true;
        public bool showTooltips = true;
        public float tutorialSpeed = 1f;
        public bool skipSeenTutorials = false;
        
        [Header("Gameplay Assistance")]
        public bool pauseOnFocusLoss = true;
        public bool autoPickup = false;
        public bool quickStack = true;
        public bool autoSortInventory = false;
        public bool showDamageNumbers = true;
        public bool healthRegeneration = true;
        public float healthRegenRate = 1f;
        
        [Header("Time & Weather")]
        public float gameSpeed = 1f;
        public bool pauseTimeInMenus = true;
        public bool dynamicWeather = true;
        public float weatherIntensity = 1f;
        public int seasonLength = 2;
        
        [Header("Economy & Resources")]
        public float resourceSpawnRate = 1f;
        public float shopPriceModifier = 1f;
        public bool infiniteResources = false;
        public bool debugMode = false;
        public float experienceMultiplier = 1f;
        
        [Header("Performance & Quality")]
        public bool limitFPSInBackground = true;
        public float backgroundFPS = 30f;
        public bool enableVSyncInGame = true;
        public bool reducedAnimations = false;
        public bool simplifiedParticles = false;
        
        [Header("Social & Multiplayer")]
        public bool allowMultiplayer = true;
        public bool friendsOnly = false;
        public bool showOnlineStatus = true;
        public bool allowInvites = true;
        public int privacyLevel = 0;
        public bool crossPlatform = true;
        
        [Header("Notifications")]
        public bool enableNotifications = true;
        public bool achievementNotifications = true;
        public bool questNotifications = true;
        public bool friendNotifications = true;
        public float notificationDuration = 5f;
        
        [Header("Advanced")]
        public bool experimentalFeatures = false;
        public bool betaContent = false;
        public bool developerMode = false;
        public bool showFPSCounter = false;
        public bool enableConsole = false;
        
        public GameplaySettings Clone()
        {
            return new GameplaySettings
            {
                difficulty = difficulty,
                customDifficultyMultiplier = customDifficultyMultiplier,
                autosaveEnabled = autosaveEnabled,
                autosaveInterval = autosaveInterval,
                autosaveFrequency = autosaveFrequency,
                saveOnExit = saveOnExit,
                cloudSave = cloudSave,
                
                showTutorials = showTutorials,
                showHints = showHints,
                showTooltips = showTooltips,
                tutorialSpeed = tutorialSpeed,
                skipSeenTutorials = skipSeenTutorials,
                
                pauseOnFocusLoss = pauseOnFocusLoss,
                autoPickup = autoPickup,
                quickStack = quickStack,
                autoSortInventory = autoSortInventory,
                showDamageNumbers = showDamageNumbers,
                healthRegeneration = healthRegeneration,
                healthRegenRate = healthRegenRate,
                
                gameSpeed = gameSpeed,
                pauseTimeInMenus = pauseTimeInMenus,
                dynamicWeather = dynamicWeather,
                weatherIntensity = weatherIntensity,
                seasonLength = seasonLength,
                
                resourceSpawnRate = resourceSpawnRate,
                shopPriceModifier = shopPriceModifier,
                infiniteResources = infiniteResources,
                debugMode = debugMode,
                experienceMultiplier = experienceMultiplier,
                
                limitFPSInBackground = limitFPSInBackground,
                backgroundFPS = backgroundFPS,
                enableVSyncInGame = enableVSyncInGame,
                reducedAnimations = reducedAnimations,
                simplifiedParticles = simplifiedParticles,
                
                allowMultiplayer = allowMultiplayer,
                friendsOnly = friendsOnly,
                showOnlineStatus = showOnlineStatus,
                allowInvites = allowInvites,
                privacyLevel = privacyLevel,
                crossPlatform = crossPlatform,
                
                enableNotifications = enableNotifications,
                achievementNotifications = achievementNotifications,
                questNotifications = questNotifications,
                friendNotifications = friendNotifications,
                notificationDuration = notificationDuration,
                
                experimentalFeatures = experimentalFeatures,
                betaContent = betaContent,
                developerMode = developerMode,
                showFPSCounter = showFPSCounter,
                enableConsole = enableConsole
            };
        }
    }

    public enum GameplayDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Expert = 3,
        Custom = 4
    }

    public enum GameplayPreset
    {
        Casual,
        Normal,
        Hardcore,
        Speedrun,
        Custom
    }
}