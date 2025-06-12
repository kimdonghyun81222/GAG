using System.Collections;
using GrowAGarden.UI._01.Scripts.UI.Core;
using GrowAGarden.UI._01.Scripts.UI.Menus.InGame;
using GrowAGarden.UI._01.Scripts.UI.Menus.MainMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.Settings
{
    public class SettingsPanel : UIPanel
    {
        [Header("Settings Tabs")]
        [SerializeField] private Transform tabContainer;
        [SerializeField] private Button graphicsTabButton;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button controlsTabButton;
        [SerializeField] private Button gameplayTabButton;
        [SerializeField] private Button accessibilityTabButton;
        
        [Header("Settings Panels")]
        [SerializeField] private GraphicsSettingsPanel graphicsSettingsPanel;
        [SerializeField] private AudioSettingsPanel audioSettingsPanel;
        [SerializeField] private ControlsSettingsPanel controlsSettingsPanel;
        [SerializeField] private GameplaySettingsPanel gameplaySettingsPanel;
        [SerializeField] private AccessibilitySettingsPanel accessibilitySettingsPanel;
        
        [Header("Main Controls")]
        [SerializeField] private Button resetToDefaultButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button closeButton;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private TextMeshProUGUI confirmationText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        
        [Header("Settings Info")]
        [SerializeField] private TextMeshProUGUI settingsVersionText;
        [SerializeField] private TextMeshProUGUI lastSavedText;
        [SerializeField] private TextMeshProUGUI settingsStatusText;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem settingsChangeEffect;
        [SerializeField] private GameObject loadingIndicator;
        
        [Header("Audio")]
        [SerializeField] private AudioClip settingsOpenSound;
        [SerializeField] private AudioClip settingsCloseSound;
        [SerializeField] private AudioClip settingsApplySound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip tabChangeSound;
        
        [Header("Animation")]
        [SerializeField] private bool enableSettingsAnimations = true;
        [SerializeField] private float tabSwitchDuration = 0.3f;
        [SerializeField] private AnimationCurve tabSwitchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Settings management
        private SettingsTab _currentTab = SettingsTab.Graphics;
        private SettingsData _originalSettings;
        private SettingsData _currentSettings;
        private bool _hasUnsavedChanges = false;
        
        // Confirmation state
        private System.Action _pendingConfirmAction;
        private string _pendingConfirmMessage;

        protected override void Awake()
        {
            base.Awake();
            InitializeSettings();
        }

        protected override void Start()
        {
            base.Start();
            SetupSettingsPanel();
            LoadSettings();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initially hide this panel
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleSettingsInput();
            UpdateSettingsStatus();
        }

        private void InitializeSettings()
        {
            // Auto-find settings panels if not assigned
            if (graphicsSettingsPanel == null)
                graphicsSettingsPanel = GetComponentInChildren<GraphicsSettingsPanel>();
            
            if (audioSettingsPanel == null)
                audioSettingsPanel = GetComponentInChildren<AudioSettingsPanel>();
            
            if (controlsSettingsPanel == null)
                controlsSettingsPanel = GetComponentInChildren<ControlsSettingsPanel>();
            
            if (gameplaySettingsPanel == null)
                gameplaySettingsPanel = GetComponentInChildren<GameplaySettingsPanel>();
            
            if (accessibilitySettingsPanel == null)
                accessibilitySettingsPanel = GetComponentInChildren<AccessibilitySettingsPanel>();
        }

        private void SetupSettingsPanel()
        {
            // Setup tab buttons
            if (graphicsTabButton != null)
            {
                graphicsTabButton.onClick.AddListener(() => SwitchTab(SettingsTab.Graphics));
                SetTabButtonActive(graphicsTabButton, true);
            }
            
            if (audioTabButton != null)
                audioTabButton.onClick.AddListener(() => SwitchTab(SettingsTab.Audio));
            
            if (controlsTabButton != null)
                controlsTabButton.onClick.AddListener(() => SwitchTab(SettingsTab.Controls));
            
            if (gameplayTabButton != null)
                gameplayTabButton.onClick.AddListener(() => SwitchTab(SettingsTab.Gameplay));
            
            if (accessibilityTabButton != null)
                accessibilityTabButton.onClick.AddListener(() => SwitchTab(SettingsTab.Accessibility));
            
            // Setup main control buttons
            if (resetToDefaultButton != null)
                resetToDefaultButton.onClick.AddListener(ShowResetConfirmation);
            
            if (applyButton != null)
                applyButton.onClick.AddListener(ApplySettings);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(CancelSettings);
            
            if (acceptButton != null)
                acceptButton.onClick.AddListener(AcceptSettings);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseSettings);
            
            // Setup confirmation dialog
            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(ConfirmAction);
            
            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(CancelConfirmation);
            
            // Initialize displays
            HideConfirmationDialog();
            UpdateSettingsInfo();
            SwitchTab(SettingsTab.Graphics);
        }

        // Input handling
        private void HandleSettingsInput()
        {
            // ESC to close settings or cancel confirmation
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (confirmationDialog != null && confirmationDialog.activeInHierarchy)
                {
                    CancelConfirmation();
                }
                else if (_hasUnsavedChanges)
                {
                    ShowUnsavedChangesConfirmation();
                }
                else
                {
                    CloseSettings();
                }
            }
            
            // Enter to apply settings
            if (Input.GetKeyDown(KeyCode.Return) && _hasUnsavedChanges)
            {
                ApplySettings();
            }
            
            // Tab keys for quick tab switching
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SwitchTab(SettingsTab.Graphics);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SwitchTab(SettingsTab.Audio);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SwitchTab(SettingsTab.Controls);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                SwitchTab(SettingsTab.Gameplay);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                SwitchTab(SettingsTab.Accessibility);
        }

        // Settings data management
        private void LoadSettings()
        {
            // Load settings from PlayerPrefs or SettingsManager
            _currentSettings = LoadSettingsFromStorage();
            _originalSettings = _currentSettings.Clone();
            
            // Apply to all settings panels
            ApplySettingsToUI();
            
            _hasUnsavedChanges = false;
            UpdateButtonStates();
        }

        private SettingsData LoadSettingsFromStorage()
        {
            // This would normally load from SettingsManager
            return new SettingsData
            {
                // Graphics settings
                resolution = new Resolution { width = Screen.currentResolution.width, height = Screen.currentResolution.height },
                fullscreenMode = Screen.fullScreenMode,
                vsyncEnabled = QualitySettings.vSyncCount > 0,
                qualityLevel = QualitySettings.GetQualityLevel(),
                targetFrameRate = Application.targetFrameRate,
                renderScale = 1f,
                shadowQuality = QualitySettings.shadowResolution,
                textureQuality = QualitySettings.globalTextureMipmapLimit,
                
                // Audio settings
                masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f),
                musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f),
                sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f),
                voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f),
                ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.6f),
                audioEnabled = PlayerPrefs.GetInt("AudioEnabled", 1) == 1,
                
                // Controls settings
                mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f),
                invertYAxis = PlayerPrefs.GetInt("InvertYAxis", 0) == 1,
                autoRun = PlayerPrefs.GetInt("AutoRun", 0) == 1,
                
                // Gameplay settings
                autosaveEnabled = PlayerPrefs.GetInt("AutosaveEnabled", 1) == 1,
                autosaveInterval = PlayerPrefs.GetFloat("AutosaveInterval", 300f),
                showTutorials = PlayerPrefs.GetInt("ShowTutorials", 1) == 1,
                pauseOnFocusLoss = PlayerPrefs.GetInt("PauseOnFocusLoss", 1) == 1,
                
                // Accessibility settings
                subtitlesEnabled = PlayerPrefs.GetInt("SubtitlesEnabled", 0) == 1,
                colorBlindSupport = PlayerPrefs.GetInt("ColorBlindSupport", 0) == 1,
                fontSize = PlayerPrefs.GetFloat("FontSize", 1f),
                uiScale = PlayerPrefs.GetFloat("UIScale", 1f)
            };
        }

        private void SaveSettingsToStorage(SettingsData settings)
        {
            // Graphics settings
            PlayerPrefs.SetInt("ScreenWidth", settings.resolution.width);
            PlayerPrefs.SetInt("ScreenHeight", settings.resolution.height);
            PlayerPrefs.SetInt("FullscreenMode", (int)settings.fullscreenMode);
            PlayerPrefs.SetInt("VSync", settings.vsyncEnabled ? 1 : 0);
            PlayerPrefs.SetInt("QualityLevel", settings.qualityLevel);
            PlayerPrefs.SetInt("TargetFrameRate", settings.targetFrameRate);
            PlayerPrefs.SetFloat("RenderScale", settings.renderScale);
            PlayerPrefs.SetInt("ShadowQuality", (int)settings.shadowQuality);
            PlayerPrefs.SetInt("TextureQuality", settings.textureQuality);
            
            // Audio settings
            PlayerPrefs.SetFloat("MasterVolume", settings.masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", settings.musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", settings.sfxVolume);
            PlayerPrefs.SetFloat("VoiceVolume", settings.voiceVolume);
            PlayerPrefs.SetFloat("AmbientVolume", settings.ambientVolume);
            PlayerPrefs.SetInt("AudioEnabled", settings.audioEnabled ? 1 : 0);
            
            // Controls settings
            PlayerPrefs.SetFloat("MouseSensitivity", settings.mouseSensitivity);
            PlayerPrefs.SetInt("InvertYAxis", settings.invertYAxis ? 1 : 0);
            PlayerPrefs.SetInt("AutoRun", settings.autoRun ? 1 : 0);
            
            // Gameplay settings
            PlayerPrefs.SetInt("AutosaveEnabled", settings.autosaveEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("AutosaveInterval", settings.autosaveInterval);
            PlayerPrefs.SetInt("ShowTutorials", settings.showTutorials ? 1 : 0);
            PlayerPrefs.SetInt("PauseOnFocusLoss", settings.pauseOnFocusLoss ? 1 : 0);
            
            // Accessibility settings
            PlayerPrefs.SetInt("SubtitlesEnabled", settings.subtitlesEnabled ? 1 : 0);
            PlayerPrefs.SetInt("ColorBlindSupport", settings.colorBlindSupport ? 1 : 0);
            PlayerPrefs.SetFloat("FontSize", settings.fontSize);
            PlayerPrefs.SetFloat("UIScale", settings.uiScale);
            
            PlayerPrefs.Save();
            
            // Update last saved time
            PlayerPrefs.SetString("SettingsLastSaved", System.DateTime.Now.ToString());
        }

        private void ApplySettingsToUI()
        {
            // Apply settings to each panel
            if (graphicsSettingsPanel != null)
                graphicsSettingsPanel.ApplySettings(_currentSettings);
            
            if (audioSettingsPanel != null)
                audioSettingsPanel.ApplySettings(_currentSettings);
            
            if (controlsSettingsPanel != null)
                controlsSettingsPanel.ApplySettings(_currentSettings);
            
            if (gameplaySettingsPanel != null)
                gameplaySettingsPanel.ApplySettings(_currentSettings);
            
            if (accessibilitySettingsPanel != null)
                accessibilitySettingsPanel.ApplySettings(_currentSettings);
        }

        private void ApplySettingsToGame()
        {
            StartCoroutine(ApplySettingsCoroutine());
        }

        private IEnumerator ApplySettingsCoroutine()
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);
            
            yield return new WaitForSecondsRealtime(0.1f);
            
            // Apply graphics settings
            Screen.SetResolution(_currentSettings.resolution.width, _currentSettings.resolution.height, _currentSettings.fullscreenMode);
            QualitySettings.vSyncCount = _currentSettings.vsyncEnabled ? 1 : 0;
            QualitySettings.SetQualityLevel(_currentSettings.qualityLevel);
            Application.targetFrameRate = _currentSettings.targetFrameRate;
            QualitySettings.shadowResolution = _currentSettings.shadowQuality;
            QualitySettings.globalTextureMipmapLimit = _currentSettings.textureQuality;
            
            yield return new WaitForSecondsRealtime(0.1f);
            
            // Apply audio settings
            AudioListener.volume = _currentSettings.masterVolume;
            // Apply other audio settings through AudioManager
            
            yield return new WaitForSecondsRealtime(0.1f);
            
            // Apply other settings through respective managers
            
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
            
            PlaySettingsApplySound();
            
            if (settingsChangeEffect != null)
                settingsChangeEffect.Play();
        }

        // Tab management
        private void SwitchTab(SettingsTab tab)
        {
            if (_currentTab == tab) return;
            
            PlayTabChangeSound();
            
            StartCoroutine(SwitchTabCoroutine(tab));
        }

        private IEnumerator SwitchTabCoroutine(SettingsTab newTab)
        {
            // Hide current panel
            var currentPanel = GetCurrentSettingsPanel();
            if (currentPanel != null && enableSettingsAnimations)
            {
                yield return StartCoroutine(AnimatePanelOut(currentPanel.transform));
            }
            else if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }
            
            // Update current tab
            _currentTab = newTab;
            
            // Update tab button states
            SetTabButtonActive(graphicsTabButton, newTab == SettingsTab.Graphics);
            SetTabButtonActive(audioTabButton, newTab == SettingsTab.Audio);
            SetTabButtonActive(controlsTabButton, newTab == SettingsTab.Controls);
            SetTabButtonActive(gameplayTabButton, newTab == SettingsTab.Gameplay);
            SetTabButtonActive(accessibilityTabButton, newTab == SettingsTab.Accessibility);
            
            // Show new panel
            var newPanel = GetCurrentSettingsPanel();
            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
                
                if (enableSettingsAnimations)
                {
                    yield return StartCoroutine(AnimatePanelIn(newPanel.transform));
                }
            }
        }

        private UIPanel GetCurrentSettingsPanel()
        {
            return _currentTab switch
            {
                SettingsTab.Graphics => graphicsSettingsPanel,
                SettingsTab.Audio => audioSettingsPanel,
                SettingsTab.Controls => controlsSettingsPanel,
                SettingsTab.Gameplay => gameplaySettingsPanel,
                SettingsTab.Accessibility => accessibilitySettingsPanel,
                _ => null
            };
        }

        private IEnumerator AnimatePanelOut(Transform panel)
        {
            Vector3 startPos = panel.localPosition;
            Vector3 endPos = startPos + Vector3.right * 300f;
            
            float elapsedTime = 0f;
            while (elapsedTime < tabSwitchDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / tabSwitchDuration;
                
                panel.localPosition = Vector3.Lerp(startPos, endPos, tabSwitchCurve.Evaluate(progress));
                
                var canvasGroup = panel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - progress;
                }
                
                yield return null;
            }
            
            panel.gameObject.SetActive(false);
        }

        private IEnumerator AnimatePanelIn(Transform panel)
        {
            Vector3 startPos = panel.localPosition + Vector3.left * 300f;
            Vector3 endPos = panel.localPosition;
            
            var canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
            }
            
            panel.localPosition = startPos;
            canvasGroup.alpha = 0f;
            
            float elapsedTime = 0f;
            while (elapsedTime < tabSwitchDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / tabSwitchDuration;
                
                panel.localPosition = Vector3.Lerp(startPos, endPos, tabSwitchCurve.Evaluate(progress));
                canvasGroup.alpha = progress;
                
                yield return null;
            }
            
            panel.localPosition = endPos;
            canvasGroup.alpha = 1f;
        }

        private void SetTabButtonActive(Button button, bool active)
        {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = active ? Color.yellow : Color.white;
            button.colors = colors;
        }

        // Settings actions
        public void OnSettingsChanged()
        {
            _hasUnsavedChanges = true;
            UpdateButtonStates();
            UpdateSettingsStatus();
        }

        private void ApplySettings()
        {
            PlayButtonClickSound();
            
            // Collect settings from all panels
            CollectSettingsFromUI();
            
            // Apply to game
            ApplySettingsToGame();
            
            // Save to storage
            SaveSettingsToStorage(_currentSettings);
            
            // Update state
            _originalSettings = _currentSettings.Clone();
            _hasUnsavedChanges = false;
            
            UpdateButtonStates();
            UpdateSettingsInfo();
        }

        private void CancelSettings()
        {
            PlayButtonClickSound();
            
            if (_hasUnsavedChanges)
            {
                ShowCancelConfirmation();
            }
            else
            {
                CloseSettings();
            }
        }

        private void AcceptSettings()
        {
            ApplySettings();
            CloseSettings();
        }

        private void ResetToDefaults()
        {
            PlayButtonClickSound();
            
            _currentSettings = CreateDefaultSettings();
            ApplySettingsToUI();
            OnSettingsChanged();
        }

        private void CollectSettingsFromUI()
        {
            // Collect from each panel
            if (graphicsSettingsPanel != null)
                graphicsSettingsPanel.CollectSettings(ref _currentSettings);
            
            if (audioSettingsPanel != null)
                audioSettingsPanel.CollectSettings(ref _currentSettings);
            
            if (controlsSettingsPanel != null)
                controlsSettingsPanel.CollectSettings(ref _currentSettings);
            
            if (gameplaySettingsPanel != null)
                gameplaySettingsPanel.CollectSettings(ref _currentSettings);
            
            if (accessibilitySettingsPanel != null)
                accessibilitySettingsPanel.CollectSettings(ref _currentSettings);
        }

        private SettingsData CreateDefaultSettings()
        {
            return new SettingsData
            {
                // Default graphics settings
                resolution = Screen.currentResolution,
                fullscreenMode = FullScreenMode.FullScreenWindow,
                vsyncEnabled = true,
                qualityLevel = QualitySettings.names.Length - 1, // Highest quality
                targetFrameRate = 60,
                renderScale = 1f,
                shadowQuality = ShadowResolution.High,
                textureQuality = 0,
                
                // Default audio settings
                masterVolume = 1f,
                musicVolume = 0.8f,
                sfxVolume = 1f,
                voiceVolume = 1f,
                ambientVolume = 0.6f,
                audioEnabled = true,
                
                // Default controls settings
                mouseSensitivity = 1f,
                invertYAxis = false,
                autoRun = false,
                
                // Default gameplay settings
                autosaveEnabled = true,
                autosaveInterval = 300f, // 5 minutes
                showTutorials = true,
                pauseOnFocusLoss = true,
                
                // Default accessibility settings
                subtitlesEnabled = false,
                colorBlindSupport = false,
                fontSize = 1f,
                uiScale = 1f
            };
        }

        // Confirmation dialogs
        private void ShowResetConfirmation()
        {
            ShowConfirmationDialog(
                "Reset all settings to default values?\nThis cannot be undone.",
                ResetToDefaults
            );
        }

        private void ShowCancelConfirmation()
        {
            ShowConfirmationDialog(
                "You have unsaved changes.\nAre you sure you want to discard them?",
                () => {
                    _currentSettings = _originalSettings.Clone();
                    ApplySettingsToUI();
                    _hasUnsavedChanges = false;
                    UpdateButtonStates();
                    CloseSettings();
                }
            );
        }

        private void ShowUnsavedChangesConfirmation()
        {
            ShowConfirmationDialog(
                "You have unsaved changes.\nDo you want to apply them before closing?",
                AcceptSettings
            );
        }

        private void ShowConfirmationDialog(string message, System.Action confirmAction)
        {
            if (confirmationDialog == null) return;
            
            _pendingConfirmAction = confirmAction;
            _pendingConfirmMessage = message;
            
            if (confirmationText != null)
                confirmationText.text = message;
            
            confirmationDialog.SetActive(true);
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
            HideConfirmationDialog();
        }

        // UI updates
        private void UpdateButtonStates()
        {
            if (applyButton != null)
                applyButton.interactable = _hasUnsavedChanges;
            
            if (cancelButton != null)
                cancelButton.interactable = _hasUnsavedChanges;
            
            if (acceptButton != null)
                acceptButton.interactable = true;
        }

        private void UpdateSettingsStatus()
        {
            if (settingsStatusText != null)
            {
                settingsStatusText.text = _hasUnsavedChanges ? "Unsaved Changes" : "All Changes Saved";
                settingsStatusText.color = _hasUnsavedChanges ? Color.yellow : Color.green;
            }
        }

        private void UpdateSettingsInfo()
        {
            if (settingsVersionText != null)
            {
                settingsVersionText.text = $"Settings Version: {Application.version}";
            }
            
            if (lastSavedText != null)
            {
                string lastSaved = PlayerPrefs.GetString("SettingsLastSaved", "Never");
                lastSavedText.text = $"Last Saved: {lastSaved}";
            }
        }

        // Navigation
        private void CloseSettings()
        {
            PlaySettingsCloseSound();
            gameObject.SetActive(false);
            
            // Return to previous menu
            var pauseMenu = FindObjectOfType<PauseMenuPanel>();
            if (pauseMenu != null && pauseMenu.IsPaused)
            {
                // Return to pause menu
            }
            else
            {
                // Return to main menu
                var mainMenu = FindObjectOfType<MainMenuPanel>();
                if (mainMenu != null)
                {
                    // Return to main menu
                }
            }
        }

        // Audio methods
        private void PlaySettingsOpenSound()
        {
            Debug.Log("Settings open sound would play here");
        }

        private void PlaySettingsCloseSound()
        {
            Debug.Log("Settings close sound would play here");
        }

        private void PlaySettingsApplySound()
        {
            Debug.Log("Settings apply sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        private void PlayTabChangeSound()
        {
            Debug.Log("Tab change sound would play here");
        }

        // Public interface
        public SettingsTab CurrentTab => _currentTab;
        public bool HasUnsavedChanges => _hasUnsavedChanges;
        public SettingsData CurrentSettings => _currentSettings;
        
        public void OpenSettings()
        {
            gameObject.SetActive(true);
            PlaySettingsOpenSound();
            LoadSettings();
        }
        
        public void SwitchToTab(SettingsTab tab)
        {
            SwitchTab(tab);
        }
    }

    // Data structures and enums
    [System.Serializable]
    public class SettingsData
    {
        [Header("Graphics")]
        public Resolution resolution;
        public FullScreenMode fullscreenMode;
        public bool vsyncEnabled;
        public int qualityLevel;
        public int targetFrameRate;
        public float renderScale;
        public ShadowResolution shadowQuality;
        public int textureQuality;
        
        [Header("Audio")]
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float voiceVolume;
        public float ambientVolume;
        public bool audioEnabled;
        
        [Header("Controls")]
        public float mouseSensitivity;
        public bool invertYAxis;
        public bool autoRun;
        
        [Header("Gameplay")]
        public bool autosaveEnabled;
        public float autosaveInterval;
        public bool showTutorials;
        public bool pauseOnFocusLoss;
        
        [Header("Accessibility")]
        public bool subtitlesEnabled;
        public bool colorBlindSupport;
        public float fontSize;
        public float uiScale;
        
        public SettingsData Clone()
        {
            return new SettingsData
            {
                resolution = resolution,
                fullscreenMode = fullscreenMode,
                vsyncEnabled = vsyncEnabled,
                qualityLevel = qualityLevel,
                targetFrameRate = targetFrameRate,
                renderScale = renderScale,
                shadowQuality = shadowQuality,
                textureQuality = textureQuality,
                
                masterVolume = masterVolume,
                musicVolume = musicVolume,
                sfxVolume = sfxVolume,
                voiceVolume = voiceVolume,
                ambientVolume = ambientVolume,
                audioEnabled = audioEnabled,
                
                mouseSensitivity = mouseSensitivity,
                invertYAxis = invertYAxis,
                autoRun = autoRun,
                
                autosaveEnabled = autosaveEnabled,
                autosaveInterval = autosaveInterval,
                showTutorials = showTutorials,
                pauseOnFocusLoss = pauseOnFocusLoss,
                
                subtitlesEnabled = subtitlesEnabled,
                colorBlindSupport = colorBlindSupport,
                fontSize = fontSize,
                uiScale = uiScale
            };
        }
    }

    public enum SettingsTab
    {
        Graphics,
        Audio,
        Controls,
        Gameplay,
        Accessibility
    }
}