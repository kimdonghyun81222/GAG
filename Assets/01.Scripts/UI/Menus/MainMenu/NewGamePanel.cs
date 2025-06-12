using System.Collections;
using System.Collections.Generic;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.MainMenu
{
    public class NewGamePanel : UIPanel
    {
        [Header("Player Customization")]
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button randomNameButton;
        [SerializeField] private Slider difficultySlider;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI difficultyDescription;
        
        [Header("Farm Customization")]
        [SerializeField] private TMP_InputField farmNameInput;
        [SerializeField] private Button randomFarmNameButton;
        [SerializeField] private TMP_Dropdown farmTypeDropdown;
        [SerializeField] private Image farmPreviewImage;
        [SerializeField] private TextMeshProUGUI farmTypeDescription;
        
        [Header("Character Avatar")]
        [SerializeField] private Image characterPreview;
        [SerializeField] private Button previousAvatarButton;
        [SerializeField] private Button nextAvatarButton;
        [SerializeField] private TextMeshProUGUI avatarNameText;
        [SerializeField] private Sprite[] availableAvatars;
        
        [Header("Game Options")]
        [SerializeField] private Toggle seedMoneyToggle;
        [SerializeField] private Slider seedMoneySlider;
        [SerializeField] private TextMeshProUGUI seedMoneyText;
        [SerializeField] private Toggle tutorialToggle;
        [SerializeField] private Toggle seasonalEventsToggle;
        [SerializeField] private Toggle quickStartToggle;
        
        [Header("Navigation Buttons")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button resetToDefaultButton;
        
        [Header("Preview Panel")]
        [SerializeField] private GameObject gamePreviewContainer;
        [SerializeField] private TextMeshProUGUI previewPlayerName;
        [SerializeField] private TextMeshProUGUI previewFarmName;
        [SerializeField] private TextMeshProUGUI previewDifficulty;
        [SerializeField] private TextMeshProUGUI previewStartingMoney;
        
        [Header("Validation")]
        [SerializeField] private TextMeshProUGUI validationText;
        [SerializeField] private Color validColor = Color.green;
        [SerializeField] private Color invalidColor = Color.red;
        
        [Header("Audio")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip characterSelectSound;
        [SerializeField] private AudioClip validationSuccessSound;
        [SerializeField] private AudioClip validationErrorSound;
        
        [Header("Animation")]
        [SerializeField] private bool enablePanelAnimations = true;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // 🔧 수정: Dependencies를 임시로 주석 처리 (나중에 구현)
        // [Inject] private SaveManager saveManager;
        // [Inject] private AudioManager audioManager;
        
        // Game configuration data
        private NewGameConfig _gameConfig;
        private int _currentAvatarIndex = 0;
        private bool _isValidating = false;
        
        // Difficulty settings
        private readonly DifficultySettings[] _difficultySettings = new DifficultySettings[]
        {
            new DifficultySettings { name = "Relaxed", description = "Take your time and enjoy farming at your own pace. Extra starting resources and forgiving mechanics.", startingMoney = 2000, cropGrowthMultiplier = 0.8f },
            new DifficultySettings { name = "Normal", description = "The standard farming experience. Balanced challenge with fair resource management.", startingMoney = 1000, cropGrowthMultiplier = 1.0f },
            new DifficultySettings { name = "Challenging", description = "A true test of your farming skills. Limited resources and faster time progression.", startingMoney = 500, cropGrowthMultiplier = 1.2f },
            new DifficultySettings { name = "Expert", description = "For master farmers only. Minimal starting resources and realistic farming challenges.", startingMoney = 250, cropGrowthMultiplier = 1.5f }
        };
        
        // Farm types
        private readonly FarmTypeSettings[] _farmTypes = new FarmTypeSettings[]
        {
            new FarmTypeSettings { name = "Traditional Farm", description = "A classic farm with balanced soil and moderate weather patterns.", biome = "Temperate", specialFeature = "Versatile growing conditions" },
            new FarmTypeSettings { name = "Mountain Valley", description = "High altitude farming with cooler temperatures and pure mountain air.", biome = "Alpine", specialFeature = "Premium crop quality bonus" },
            new FarmTypeSettings { name = "Coastal Farm", description = "Near the ocean with sandy soil and frequent rain. Great for certain crops.", biome = "Coastal", specialFeature = "Enhanced fish farming" },
            new FarmTypeSettings { name = "Desert Oasis", description = "Limited water but rich mineral soil. Challenging but rewarding.", biome = "Arid", specialFeature = "Rare desert crops available" }
        };
        
        // Random name lists
        private readonly string[] _playerNames = { "Alex", "Jordan", "Casey", "Morgan", "River", "Sage", "Phoenix", "Rowan", "Quinn", "Avery" };
        private readonly string[] _farmNames = { "Sunny Acres", "Green Valley", "Golden Fields", "Rainbow Ranch", "Peaceful Pastures", "Harvest Haven", "Eden Gardens", "Moonrise Farm", "Starlight Sanctuary", "Wildflower Woods" };

        protected override void Awake()
        {
            base.Awake();
            InitializeGameConfig();
        }

        protected override void Start()
        {
            base.Start();
            SetupNewGamePanel();
            SetDefaultValues();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 🔧 수정: 초기에 이 패널을 숨김
            gameObject.SetActive(false);
        }

        private void InitializeGameConfig()
        {
            _gameConfig = new NewGameConfig();
            
            // Set defaults
            _gameConfig.playerName = "Farmer";
            _gameConfig.farmName = "My Farm";
            _gameConfig.difficulty = 1; // Normal
            _gameConfig.farmType = 0; // Traditional
            _gameConfig.avatarIndex = 0;
            _gameConfig.startingMoney = 1000;
            _gameConfig.enableTutorial = true;
            _gameConfig.enableSeasonalEvents = true;
            _gameConfig.quickStart = false;
        }

        private void SetupNewGamePanel()
        {
            // Setup input field listeners
            if (playerNameInput != null)
            {
                playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
                playerNameInput.onEndEdit.AddListener(ValidatePlayerName);
            }
            
            if (farmNameInput != null)
            {
                farmNameInput.onValueChanged.AddListener(OnFarmNameChanged);
                farmNameInput.onEndEdit.AddListener(ValidateFarmName);
            }
            
            // Setup button listeners
            if (randomNameButton != null)
                randomNameButton.onClick.AddListener(GenerateRandomPlayerName);
            
            if (randomFarmNameButton != null)
                randomFarmNameButton.onClick.AddListener(GenerateRandomFarmName);
            
            if (previousAvatarButton != null)
                previousAvatarButton.onClick.AddListener(PreviousAvatar);
            
            if (nextAvatarButton != null)
                nextAvatarButton.onClick.AddListener(NextAvatar);
            
            if (startGameButton != null)
                startGameButton.onClick.AddListener(StartNewGame);
            
            if (backButton != null)
                backButton.onClick.AddListener(ReturnToMainMenu);
            
            if (resetToDefaultButton != null)
                resetToDefaultButton.onClick.AddListener(ResetToDefaults);
            
            // Setup slider listeners
            if (difficultySlider != null)
            {
                difficultySlider.minValue = 0;
                difficultySlider.maxValue = _difficultySettings.Length - 1;
                difficultySlider.wholeNumbers = true;
                difficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
            }
            
            if (seedMoneySlider != null)
            {
                seedMoneySlider.onValueChanged.AddListener(OnSeedMoneyChanged);
            }
            
            // Setup dropdown
            if (farmTypeDropdown != null)
            {
                SetupFarmTypeDropdown();
                farmTypeDropdown.onValueChanged.AddListener(OnFarmTypeChanged);
            }
            
            // Setup toggles
            if (seedMoneyToggle != null)
                seedMoneyToggle.onValueChanged.AddListener(OnSeedMoneyToggleChanged);
            
            if (tutorialToggle != null)
                tutorialToggle.onValueChanged.AddListener(OnTutorialToggleChanged);
            
            if (seasonalEventsToggle != null)
                seasonalEventsToggle.onValueChanged.AddListener(OnSeasonalEventsToggleChanged);
            
            if (quickStartToggle != null)
                quickStartToggle.onValueChanged.AddListener(OnQuickStartToggleChanged);
        }

        private void SetDefaultValues()
        {
            // Set UI to default values
            if (playerNameInput != null)
                playerNameInput.text = _gameConfig.playerName;
            
            if (farmNameInput != null)
                farmNameInput.text = _gameConfig.farmName;
            
            if (difficultySlider != null)
                difficultySlider.value = _gameConfig.difficulty;
            
            if (farmTypeDropdown != null)
                farmTypeDropdown.value = _gameConfig.farmType;
            
            if (tutorialToggle != null)
                tutorialToggle.isOn = _gameConfig.enableTutorial;
            
            if (seasonalEventsToggle != null)
                seasonalEventsToggle.isOn = _gameConfig.enableSeasonalEvents;
            
            if (quickStartToggle != null)
                quickStartToggle.isOn = _gameConfig.quickStart;
            
            // Update displays
            UpdateDifficultyDisplay();
            UpdateFarmTypeDisplay();
            UpdateAvatarDisplay();
            UpdateGamePreview();
            ValidateAllSettings();
        }

        private void SetupFarmTypeDropdown()
        {
            if (farmTypeDropdown == null) return;
            
            farmTypeDropdown.ClearOptions();
            var options = new List<string>();
            
            foreach (var farmType in _farmTypes)
            {
                options.Add(farmType.name);
            }
            
            farmTypeDropdown.AddOptions(options);
        }

        // Input validation methods
        private void OnPlayerNameChanged(string newName)
        {
            _gameConfig.playerName = newName;
            UpdateGamePreview();
        }

        private void ValidatePlayerName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
            {
                ShowValidationMessage("Player name cannot be empty!", false);
                return;
            }
            
            if (playerName.Length < 2)
            {
                ShowValidationMessage("Player name must be at least 2 characters!", false);
                return;
            }
            
            if (playerName.Length > 20)
            {
                ShowValidationMessage("Player name must be 20 characters or less!", false);
                return;
            }
            
            // Check for invalid characters
            if (System.Text.RegularExpressions.Regex.IsMatch(playerName, @"[^a-zA-Z0-9\s]"))
            {
                ShowValidationMessage("Player name can only contain letters, numbers, and spaces!", false);
                return;
            }
            
            ShowValidationMessage("Player name looks good!", true);
        }

        private void OnFarmNameChanged(string newName)
        {
            _gameConfig.farmName = newName;
            UpdateGamePreview();
        }

        private void ValidateFarmName(string farmName)
        {
            if (string.IsNullOrWhiteSpace(farmName))
            {
                ShowValidationMessage("Farm name cannot be empty!", false);
                return;
            }
            
            if (farmName.Length < 2)
            {
                ShowValidationMessage("Farm name must be at least 2 characters!", false);
                return;
            }
            
            if (farmName.Length > 30)
            {
                ShowValidationMessage("Farm name must be 30 characters or less!", false);
                return;
            }
            
            ShowValidationMessage("Farm name looks good!", true);
        }

        private void OnDifficultyChanged(float value)
        {
            _gameConfig.difficulty = Mathf.RoundToInt(value);
            UpdateDifficultyDisplay();
            UpdateGamePreview();
        }

        private void UpdateDifficultyDisplay()
        {
            if (_gameConfig.difficulty < 0 || _gameConfig.difficulty >= _difficultySettings.Length) return;
            
            var difficulty = _difficultySettings[_gameConfig.difficulty];
            
            if (difficultyText != null)
                difficultyText.text = difficulty.name;
            
            if (difficultyDescription != null)
                difficultyDescription.text = difficulty.description;
            
            // Update seed money if custom seed money is disabled
            if (seedMoneyToggle != null && !seedMoneyToggle.isOn)
            {
                _gameConfig.startingMoney = difficulty.startingMoney;
                UpdateSeedMoneyDisplay();
            }
        }

        private void OnFarmTypeChanged(int index)
        {
            _gameConfig.farmType = index;
            UpdateFarmTypeDisplay();
            UpdateGamePreview();
        }

        private void UpdateFarmTypeDisplay()
        {
            if (_gameConfig.farmType < 0 || _gameConfig.farmType >= _farmTypes.Length) return;
            
            var farmType = _farmTypes[_gameConfig.farmType];
            
            if (farmTypeDescription != null)
                farmTypeDescription.text = $"{farmType.description}\nBiome: {farmType.biome}\nSpecial: {farmType.specialFeature}";
            
            // Update preview image if available
            if (farmPreviewImage != null)
            {
                // Load farm preview sprite based on farm type
                // This would typically load from Resources or Asset Bundle
                // farmPreviewImage.sprite = LoadFarmPreviewSprite(_gameConfig.farmType);
            }
        }

        // Avatar management
        private void PreviousAvatar()
        {
            PlayButtonSound();
            
            _currentAvatarIndex = (_currentAvatarIndex - 1 + availableAvatars.Length) % availableAvatars.Length;
            _gameConfig.avatarIndex = _currentAvatarIndex;
            
            UpdateAvatarDisplay();
            PlayCharacterSelectSound();
        }

        private void NextAvatar()
        {
            PlayButtonSound();
            
            _currentAvatarIndex = (_currentAvatarIndex + 1) % availableAvatars.Length;
            _gameConfig.avatarIndex = _currentAvatarIndex;
            
            UpdateAvatarDisplay();
            PlayCharacterSelectSound();
        }

        private void UpdateAvatarDisplay()
        {
            if (availableAvatars == null || availableAvatars.Length == 0) return;
            
            if (characterPreview != null && _currentAvatarIndex < availableAvatars.Length)
            {
                characterPreview.sprite = availableAvatars[_currentAvatarIndex];
            }
            
            if (avatarNameText != null)
            {
                avatarNameText.text = $"Avatar {_currentAvatarIndex + 1}";
            }
        }

        // Money and option management
        private void OnSeedMoneyToggleChanged(bool customEnabled)
        {
            if (seedMoneySlider != null)
            {
                seedMoneySlider.gameObject.SetActive(customEnabled);
            }
            
            if (!customEnabled)
            {
                // Reset to difficulty-based money
                var difficulty = _difficultySettings[_gameConfig.difficulty];
                _gameConfig.startingMoney = difficulty.startingMoney;
                UpdateSeedMoneyDisplay();
            }
            
            UpdateGamePreview();
        }

        private void OnSeedMoneyChanged(float value)
        {
            _gameConfig.startingMoney = Mathf.RoundToInt(value);
            UpdateSeedMoneyDisplay();
            UpdateGamePreview();
        }

        private void UpdateSeedMoneyDisplay()
        {
            if (seedMoneyText != null)
            {
                seedMoneyText.text = $"${_gameConfig.startingMoney:N0}";
            }
            
            if (seedMoneySlider != null)
            {
                seedMoneySlider.value = _gameConfig.startingMoney;
            }
        }

        private void OnTutorialToggleChanged(bool enabled)
        {
            _gameConfig.enableTutorial = enabled;
            UpdateGamePreview();
        }

        private void OnSeasonalEventsToggleChanged(bool enabled)
        {
            _gameConfig.enableSeasonalEvents = enabled;
            UpdateGamePreview();
        }

        private void OnQuickStartToggleChanged(bool enabled)
        {
            _gameConfig.quickStart = enabled;
            UpdateGamePreview();
        }

        // Random generation
        private void GenerateRandomPlayerName()
        {
            PlayButtonSound();
            
            string randomName = _playerNames[Random.Range(0, _playerNames.Length)];
            
            if (playerNameInput != null)
            {
                playerNameInput.text = randomName;
            }
            
            _gameConfig.playerName = randomName;
            UpdateGamePreview();
        }

        private void GenerateRandomFarmName()
        {
            PlayButtonSound();
            
            string randomName = _farmNames[Random.Range(0, _farmNames.Length)];
            
            if (farmNameInput != null)
            {
                farmNameInput.text = randomName;
            }
            
            _gameConfig.farmName = randomName;
            UpdateGamePreview();
        }

        // Game preview
        private void UpdateGamePreview()
        {
            if (gamePreviewContainer == null) return;
            
            if (previewPlayerName != null)
                previewPlayerName.text = _gameConfig.playerName;
            
            if (previewFarmName != null)
                previewFarmName.text = _gameConfig.farmName;
            
            if (previewDifficulty != null && _gameConfig.difficulty < _difficultySettings.Length)
                previewDifficulty.text = _difficultySettings[_gameConfig.difficulty].name;
            
            if (previewStartingMoney != null)
                previewStartingMoney.text = $"${_gameConfig.startingMoney:N0}";
        }

        // Validation
        private void ValidateAllSettings()
        {
            _isValidating = true;
            
            bool isValid = true;
            string errorMessage = "";
            
            // Validate player name
            if (string.IsNullOrWhiteSpace(_gameConfig.playerName) || _gameConfig.playerName.Length < 2)
            {
                isValid = false;
                errorMessage = "Invalid player name";
            }
            
            // Validate farm name
            if (string.IsNullOrWhiteSpace(_gameConfig.farmName) || _gameConfig.farmName.Length < 2)
            {
                isValid = false;
                errorMessage = "Invalid farm name";
            }
            
            // Update start button
            if (startGameButton != null)
            {
                startGameButton.interactable = isValid;
            }
            
            if (!isValid)
            {
                ShowValidationMessage(errorMessage, false);
            }
            else
            {
                ShowValidationMessage("Ready to start your farming adventure!", true);
            }
            
            _isValidating = false;
        }

        private void ShowValidationMessage(string message, bool isValid)
        {
            if (validationText == null) return;
            
            validationText.text = message;
            validationText.color = isValid ? validColor : invalidColor;
            
            // 🔧 수정: AudioManager 사용을 임시로 주석 처리
            // Play validation sound
            // if (audioManager != null)
            // {
            //     if (isValid && validationSuccessSound != null)
            //     {
            //         audioManager.PlaySFX(validationSuccessSound);
            //     }
            //     else if (!isValid && validationErrorSound != null)
            //     {
            //         audioManager.PlaySFX(validationErrorSound);
            //     }
            // }
        }

        // Navigation
        private void StartNewGame()
        {
            PlayButtonSound();
            
            // Final validation
            ValidateAllSettings();
            
            if (!startGameButton.interactable) return;
            
            // 🔧 수정: SaveManager 사용을 임시로 주석 처리
            // Save game configuration
            // if (saveManager != null)
            // {
            //     saveManager.CreateNewGame(_gameConfig);
            // }
            
            // Start the game
            StartCoroutine(LoadGameScene());
        }

        private IEnumerator LoadGameScene()
        {
            // Animate panel out
            if (enablePanelAnimations)
            {
                yield return StartCoroutine(AnimatePanelOut());
            }
            
            // Load game scene
            SceneManager.LoadScene("GameScene");
        }

        private void ReturnToMainMenu()
        {
            PlayButtonSound();
            
            var mainMenuPanel = FindObjectOfType<MainMenuPanel>();
            if (mainMenuPanel != null)
            {
                mainMenuPanel.ReturnToMainMenu();
            }
            
            // 🔧 수정: Hide() 대신 gameObject.SetActive(false) 사용
            gameObject.SetActive(false);
        }

        private void ResetToDefaults()
        {
            PlayButtonSound();
            
            InitializeGameConfig();
            SetDefaultValues();
        }

        // Animation methods
        // 🔧 수정: Show() 메서드를 올바르게 구현
        public void ShowPanel()
        {
            gameObject.SetActive(true);
            
            if (enablePanelAnimations)
            {
                StartCoroutine(AnimatePanelIn());
            }
        }

        private IEnumerator AnimatePanelIn()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) yield break;
            
            Vector3 startPos = rectTransform.localPosition + Vector3.right * Screen.width;
            Vector3 endPos = rectTransform.localPosition;
            
            rectTransform.localPosition = startPos;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                
                rectTransform.localPosition = Vector3.Lerp(startPos, endPos, slideInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            rectTransform.localPosition = endPos;
        }

        private IEnumerator AnimatePanelOut()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) yield break;
            
            Vector3 startPos = rectTransform.localPosition;
            Vector3 endPos = startPos + Vector3.left * Screen.width;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                
                rectTransform.localPosition = Vector3.Lerp(startPos, endPos, slideInCurve.Evaluate(progress));
                
                yield return null;
            }
        }

        // Audio helpers
        private void PlayButtonSound()
        {
            // 🔧 수정: AudioManager 사용을 임시로 주석 처리
            // if (audioManager != null && buttonClickSound != null)
            // {
            //     audioManager.PlaySFX(buttonClickSound);
            // }
            
            Debug.Log("Button sound would play here");
        }

        private void PlayCharacterSelectSound()
        {
            // 🔧 수정: AudioManager 사용을 임시로 주석 처리
            // if (audioManager != null && characterSelectSound != null)
            // {
            //     audioManager.PlaySFX(characterSelectSound);
            // }
            
            Debug.Log("Character select sound would play here");
        }
    }

    // Data structures
    [System.Serializable]
    public class NewGameConfig
    {
        public string playerName;
        public string farmName;
        public int difficulty;
        public int farmType;
        public int avatarIndex;
        public int startingMoney;
        public bool enableTutorial;
        public bool enableSeasonalEvents;
        public bool quickStart;
    }

    [System.Serializable]
    public class DifficultySettings
    {
        public string name;
        public string description;
        public int startingMoney;
        public float cropGrowthMultiplier;
    }

    [System.Serializable]
    public class FarmTypeSettings
    {
        public string name;
        public string description;
        public string biome;
        public string specialFeature;
    }
}