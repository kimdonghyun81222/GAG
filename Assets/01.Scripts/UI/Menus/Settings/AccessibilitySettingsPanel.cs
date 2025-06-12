using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.Settings
{
    public class AccessibilitySettingsPanel : UIPanel   
    {
        [Header("Visual Accessibility")]
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Slider fontSizeSlider;
        [SerializeField] private TextMeshProUGUI fontSizeText;
        [SerializeField] private Slider uiScaleSlider;
        [SerializeField] private TextMeshProUGUI uiScaleText;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Toggle colorBlindSupportToggle;
        [SerializeField] private TMP_Dropdown colorBlindTypeDropdown;
        [SerializeField] private Toggle reducedMotionToggle;
        [SerializeField] private Toggle largerCursorToggle;
        [SerializeField] private Slider cursorSizeSlider;
        [SerializeField] private TextMeshProUGUI cursorSizeText;
        
        [Header("Audio Accessibility")]
        [SerializeField] private Toggle audioDescriptionsToggle;
        [SerializeField] private Toggle visualAudioCuesToggle;
        [SerializeField] private Toggle closedCaptionsToggle;
        [SerializeField] private Slider captionSizeSlider;
        [SerializeField] private TextMeshProUGUI captionSizeText;
        [SerializeField] private TMP_Dropdown captionStyleDropdown;
        [SerializeField] private Toggle soundVisualizationToggle;
        [SerializeField] private Toggle hapticFeedbackToggle;
        [SerializeField] private Slider hapticIntensitySlider;
        [SerializeField] private TextMeshProUGUI hapticIntensityText;
        
        [Header("Motor Accessibility")]
        [SerializeField] private Toggle oneHandedModeToggle;
        [SerializeField] private Toggle autoClickToggle;
        [SerializeField] private Slider autoClickDelaySlider;
        [SerializeField] private TextMeshProUGUI autoClickDelayText;
        [SerializeField] private Toggle mouseKeysToggle;
        [SerializeField] private Toggle stickyKeysToggle;
        [SerializeField] private Toggle slowKeysToggle;
        [SerializeField] private Slider slowKeysDelaySlider;
        [SerializeField] private TextMeshProUGUI slowKeysDelayText;
        [SerializeField] private Toggle bounceKeysToggle;
        [SerializeField] private Slider bounceKeysDelaySlider;
        [SerializeField] private TextMeshProUGUI bounceKeysDelayText;
        
        [Header("Cognitive Accessibility")]
        [SerializeField] private Toggle simplifiedUIToggle;
        [SerializeField] private Toggle extendedTimeoutsToggle;
        [SerializeField] private Slider readingSpeedSlider;
        [SerializeField] private TextMeshProUGUI readingSpeedText;
        [SerializeField] private Toggle pauseOnDialogToggle;
        [SerializeField] private Toggle skipComplexAnimationsToggle;
        [SerializeField] private Toggle useSimpleLanguageToggle;
        [SerializeField] private Toggle showProgressIndicatorsToggle;
        [SerializeField] private Toggle autoSaveFrequentlyToggle;
        
        [Header("Navigation Assistance")]
        [SerializeField] private Toggle screenReaderSupportToggle;
        [SerializeField] private Toggle tabNavigationToggle;
        [SerializeField] private Toggle focusIndicatorToggle;
        [SerializeField] private Slider focusIndicatorSizeSlider;
        [SerializeField] private TextMeshProUGUI focusIndicatorSizeText;
        [SerializeField] private Toggle keyboardOnlyModeToggle;
        [SerializeField] private Toggle voiceNavigationToggle;
        [SerializeField] private Toggle gestureNavigationToggle;
        
        [Header("Text and Reading")]
        [SerializeField] private TMP_Dropdown fontTypeDropdown;
        [SerializeField] private Toggle dyslexiaFriendlyFontToggle;
        [SerializeField] private Slider lineSpacingSlider;
        [SerializeField] private TextMeshProUGUI lineSpacingText;
        [SerializeField] private Slider wordSpacingSlider;
        [SerializeField] private TextMeshProUGUI wordSpacingText;
        [SerializeField] private Toggle textToSpeechToggle;
        [SerializeField] private Slider speechRateSlider;
        [SerializeField] private TextMeshProUGUI speechRateText;
        [SerializeField] private TMP_Dropdown speechVoiceDropdown;
        
        [Header("Color and Contrast")]
        [SerializeField] private Slider contrastSlider;
        [SerializeField] private TextMeshProUGUI contrastText;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private TextMeshProUGUI brightnessText;
        [SerializeField] private Toggle invertColorsToggle;
        [SerializeField] private Toggle grayscaleToggle;
        [SerializeField] private Slider saturationSlider;
        [SerializeField] private TextMeshProUGUI saturationText;
        [SerializeField] private Button colorFilterTestButton;
        
        [Header("Time and Timing")]
        [SerializeField] private Toggle disableTimePresureToggle;
        [SerializeField] private Slider interactionTimeoutSlider;
        [SerializeField] private TextMeshProUGUI interactionTimeoutText;
        [SerializeField] private Toggle pauseableContentToggle;
        [SerializeField] private Toggle noAutoplayToggle;
        [SerializeField] private Slider animationDurationSlider;
        [SerializeField] private TextMeshProUGUI animationDurationText;
        
        [Header("Input Assistance")]
        [SerializeField] private Toggle inputPredictionToggle;
        [SerializeField] private Toggle autoCompleteToggle;
        [SerializeField] private Toggle errorPreventionToggle;
        [SerializeField] private Toggle confirmDangerousActionsToggle;
        [SerializeField] private Toggle undoRedoSupportToggle;
        [SerializeField] private Slider inputToleranceSlider;
        [SerializeField] private TextMeshProUGUI inputToleranceText;
        
        [Header("Profile Management")]
        [SerializeField] private TMP_Dropdown accessibilityProfileDropdown;
        [SerializeField] private Button saveProfileButton;
        [SerializeField] private Button loadProfileButton;
        [SerializeField] private Button deleteProfileButton;
        [SerializeField] private TMP_InputField profileNameInput;
        [SerializeField] private Button createProfileButton;
        
        [Header("Testing and Preview")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private Button showPreviewButton;
        [SerializeField] private TextMeshProUGUI previewText;
        [SerializeField] private Image previewImage;
        [SerializeField] private Button testColorVisionButton;
        [SerializeField] private Button testAudioCuesButton;
        [SerializeField] private Button testNavigationButton;
        
        [Header("Reset and Presets")]
        [SerializeField] private Button resetAllButton;
        [SerializeField] private Button visionImpairmentPresetButton;
        [SerializeField] private Button hearingImpairmentPresetButton;
        [SerializeField] private Button motorImpairmentPresetButton;
        [SerializeField] private Button cognitiveImpairmentPresetButton;
        [SerializeField] private Button universalDesignPresetButton;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem accessibilityChangeEffect;
        [SerializeField] private GameObject testingIndicator;
        
        [Header("Audio")]
        [SerializeField] private AudioClip accessibilityChangeSound;
        [SerializeField] private AudioClip testSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip confirmationSound;
        
        // Accessibility settings data
        private AccessibilitySettings _currentAccessibilitySettings;
        private bool _isApplyingSettings = false;
        private bool _isTestingInProgress = false;
        
        // Profile management
        private List<AccessibilityProfile> _accessibilityProfiles = new List<AccessibilityProfile>();
        private string _currentProfileName = "Default";
        
        // Available options
        private readonly string[] _colorBlindTypes = { "None", "Protanopia", "Deuteranopia", "Tritanopia", "Achromatopsia" };
        private readonly string[] _captionStyles = { "Default", "Large", "High Contrast", "Background Box", "Outline" };
        private readonly string[] _fontTypes = { "Default", "Arial", "OpenDyslexic", "Times New Roman", "Comic Sans" };
        private readonly string[] _speechVoices = { "Default", "Male Voice", "Female Voice", "Robotic Voice" };

        protected override void Awake()
        {
            base.Awake();
            InitializeAccessibilitySettings();
        }

        protected override void Start()
        {
            base.Start();
            SetupAccessibilityPanel();
            LoadAccessibilitySettings();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Start hidden
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleAccessibilityInput();
            
            if (_isTestingInProgress)
            {
                UpdateTesting();
            }
        }

        private void InitializeAccessibilitySettings()
        {
            // Initialize current settings
            _currentAccessibilitySettings = new AccessibilitySettings();
            
            // Load default profiles
            CreateDefaultProfiles();
        }

        private void SetupAccessibilityPanel()
        {
            // Setup visual accessibility controls
            SetupVisualAccessibilityControls();
            
            // Setup audio accessibility controls
            SetupAudioAccessibilityControls();
            
            // Setup motor accessibility controls
            SetupMotorAccessibilityControls();
            
            // Setup cognitive accessibility controls
            SetupCognitiveAccessibilityControls();
            
            // Setup navigation assistance controls
            SetupNavigationAssistanceControls();
            
            // Setup text and reading controls
            SetupTextAndReadingControls();
            
            // Setup color and contrast controls
            SetupColorAndContrastControls();
            
            // Setup time and timing controls
            SetupTimeAndTimingControls();
            
            // Setup input assistance controls
            SetupInputAssistanceControls();
            
            // Setup profile management
            SetupProfileManagement();
            
            // Setup testing and preview
            SetupTestingAndPreview();
            
            // Setup reset and presets
            SetupResetAndPresets();
        }

        private void SetupVisualAccessibilityControls()
        {
            if (subtitlesToggle != null)
                subtitlesToggle.onValueChanged.AddListener(OnSubtitlesChanged);
            
            if (fontSizeSlider != null)
            {
                fontSizeSlider.minValue = 0.5f;
                fontSizeSlider.maxValue = 3f;
                fontSizeSlider.onValueChanged.AddListener(OnFontSizeChanged);
            }
            
            if (uiScaleSlider != null)
            {
                uiScaleSlider.minValue = 0.75f;
                uiScaleSlider.maxValue = 2f;
                uiScaleSlider.onValueChanged.AddListener(OnUIScaleChanged);
            }
            
            if (highContrastToggle != null)
                highContrastToggle.onValueChanged.AddListener(OnHighContrastChanged);
            
            if (colorBlindSupportToggle != null)
                colorBlindSupportToggle.onValueChanged.AddListener(OnColorBlindSupportChanged);
            
            if (colorBlindTypeDropdown != null)
            {
                colorBlindTypeDropdown.ClearOptions();
                colorBlindTypeDropdown.AddOptions(_colorBlindTypes.ToList());
                colorBlindTypeDropdown.onValueChanged.AddListener(OnColorBlindTypeChanged);
            }
            
            if (reducedMotionToggle != null)
                reducedMotionToggle.onValueChanged.AddListener(OnReducedMotionChanged);
            
            if (largerCursorToggle != null)
                largerCursorToggle.onValueChanged.AddListener(OnLargerCursorChanged);
            
            if (cursorSizeSlider != null)
            {
                cursorSizeSlider.minValue = 1f;
                cursorSizeSlider.maxValue = 4f;
                cursorSizeSlider.onValueChanged.AddListener(OnCursorSizeChanged);
            }
        }

        private void SetupAudioAccessibilityControls()
        {
            if (audioDescriptionsToggle != null)
                audioDescriptionsToggle.onValueChanged.AddListener(OnAudioDescriptionsChanged);
            
            if (visualAudioCuesToggle != null)
                visualAudioCuesToggle.onValueChanged.AddListener(OnVisualAudioCuesChanged);
            
            if (closedCaptionsToggle != null)
                closedCaptionsToggle.onValueChanged.AddListener(OnClosedCaptionsChanged);
            
            if (captionSizeSlider != null)
            {
                captionSizeSlider.minValue = 0.5f;
                captionSizeSlider.maxValue = 3f;
                captionSizeSlider.onValueChanged.AddListener(OnCaptionSizeChanged);
            }
            
            if (captionStyleDropdown != null)
            {
                captionStyleDropdown.ClearOptions();
                captionStyleDropdown.AddOptions(_captionStyles.ToList());
                captionStyleDropdown.onValueChanged.AddListener(OnCaptionStyleChanged);
            }
            
            if (soundVisualizationToggle != null)
                soundVisualizationToggle.onValueChanged.AddListener(OnSoundVisualizationChanged);
            
            if (hapticFeedbackToggle != null)
                hapticFeedbackToggle.onValueChanged.AddListener(OnHapticFeedbackChanged);
            
            if (hapticIntensitySlider != null)
            {
                hapticIntensitySlider.minValue = 0.1f;
                hapticIntensitySlider.maxValue = 2f;
                hapticIntensitySlider.onValueChanged.AddListener(OnHapticIntensityChanged);
            }
        }

        private void SetupMotorAccessibilityControls()
        {
            if (oneHandedModeToggle != null)
                oneHandedModeToggle.onValueChanged.AddListener(OnOneHandedModeChanged);
            
            if (autoClickToggle != null)
                autoClickToggle.onValueChanged.AddListener(OnAutoClickChanged);
            
            if (autoClickDelaySlider != null)
            {
                autoClickDelaySlider.minValue = 0.5f;
                autoClickDelaySlider.maxValue = 5f;
                autoClickDelaySlider.onValueChanged.AddListener(OnAutoClickDelayChanged);
            }
            
            if (mouseKeysToggle != null)
                mouseKeysToggle.onValueChanged.AddListener(OnMouseKeysChanged);
            
            if (stickyKeysToggle != null)
                stickyKeysToggle.onValueChanged.AddListener(OnStickyKeysChanged);
            
            if (slowKeysToggle != null)
                slowKeysToggle.onValueChanged.AddListener(OnSlowKeysChanged);
            
            if (slowKeysDelaySlider != null)
            {
                slowKeysDelaySlider.minValue = 0.1f;
                slowKeysDelaySlider.maxValue = 2f;
                slowKeysDelaySlider.onValueChanged.AddListener(OnSlowKeysDelayChanged);
            }
            
            if (bounceKeysToggle != null)
                bounceKeysToggle.onValueChanged.AddListener(OnBounceKeysChanged);
            
            if (bounceKeysDelaySlider != null)
            {
                bounceKeysDelaySlider.minValue = 0.1f;
                bounceKeysDelaySlider.maxValue = 1f;
                bounceKeysDelaySlider.onValueChanged.AddListener(OnBounceKeysDelayChanged);
            }
        }
        
        private void SetupCognitiveAccessibilityControls()
        {
            if (simplifiedUIToggle != null)
                simplifiedUIToggle.onValueChanged.AddListener(OnSimplifiedUIChanged);
            
            if (extendedTimeoutsToggle != null)
                extendedTimeoutsToggle.onValueChanged.AddListener(OnExtendedTimeoutsChanged);
            
            if (readingSpeedSlider != null)
            {
                readingSpeedSlider.minValue = 0.5f;
                readingSpeedSlider.maxValue = 2f;
                readingSpeedSlider.onValueChanged.AddListener(OnReadingSpeedChanged);
            }
            
            if (pauseOnDialogToggle != null)
                pauseOnDialogToggle.onValueChanged.AddListener(OnPauseOnDialogChanged);
            
            if (skipComplexAnimationsToggle != null)
                skipComplexAnimationsToggle.onValueChanged.AddListener(OnSkipComplexAnimationsChanged);
            
            if (useSimpleLanguageToggle != null)
                useSimpleLanguageToggle.onValueChanged.AddListener(OnUseSimpleLanguageChanged);
            
            if (showProgressIndicatorsToggle != null)
                showProgressIndicatorsToggle.onValueChanged.AddListener(OnShowProgressIndicatorsChanged);
            
            if (autoSaveFrequentlyToggle != null)
                autoSaveFrequentlyToggle.onValueChanged.AddListener(OnAutoSaveFrequentlyChanged);
        }

        private void SetupNavigationAssistanceControls()
        {
            if (screenReaderSupportToggle != null)
                screenReaderSupportToggle.onValueChanged.AddListener(OnScreenReaderSupportChanged);
            
            if (tabNavigationToggle != null)
                tabNavigationToggle.onValueChanged.AddListener(OnTabNavigationChanged);
            
            if (focusIndicatorToggle != null)
                focusIndicatorToggle.onValueChanged.AddListener(OnFocusIndicatorChanged);
            
            if (focusIndicatorSizeSlider != null)
            {
                focusIndicatorSizeSlider.minValue = 1f;
                focusIndicatorSizeSlider.maxValue = 5f;
                focusIndicatorSizeSlider.onValueChanged.AddListener(OnFocusIndicatorSizeChanged);
            }
            
            if (keyboardOnlyModeToggle != null)
                keyboardOnlyModeToggle.onValueChanged.AddListener(OnKeyboardOnlyModeChanged);
            
            if (voiceNavigationToggle != null)
                voiceNavigationToggle.onValueChanged.AddListener(OnVoiceNavigationChanged);
            
            if (gestureNavigationToggle != null)
                gestureNavigationToggle.onValueChanged.AddListener(OnGestureNavigationChanged);
        }

        private void SetupTextAndReadingControls()
        {
            if (fontTypeDropdown != null)
            {
                fontTypeDropdown.ClearOptions();
                fontTypeDropdown.AddOptions(_fontTypes.ToList());
                fontTypeDropdown.onValueChanged.AddListener(OnFontTypeChanged);
            }
            
            if (dyslexiaFriendlyFontToggle != null)
                dyslexiaFriendlyFontToggle.onValueChanged.AddListener(OnDyslexiaFriendlyFontChanged);
            
            if (lineSpacingSlider != null)
            {
                lineSpacingSlider.minValue = 1f;
                lineSpacingSlider.maxValue = 3f;
                lineSpacingSlider.onValueChanged.AddListener(OnLineSpacingChanged);
            }
            
            if (wordSpacingSlider != null)
            {
                wordSpacingSlider.minValue = 1f;
                wordSpacingSlider.maxValue = 2f;
                wordSpacingSlider.onValueChanged.AddListener(OnWordSpacingChanged);
            }
            
            if (textToSpeechToggle != null)
                textToSpeechToggle.onValueChanged.AddListener(OnTextToSpeechChanged);
            
            if (speechRateSlider != null)
            {
                speechRateSlider.minValue = 0.5f;
                speechRateSlider.maxValue = 2f;
                speechRateSlider.onValueChanged.AddListener(OnSpeechRateChanged);
            }
            
            if (speechVoiceDropdown != null)
            {
                speechVoiceDropdown.ClearOptions();
                speechVoiceDropdown.AddOptions(_speechVoices.ToList());
                speechVoiceDropdown.onValueChanged.AddListener(OnSpeechVoiceChanged);
            }
        }

        private void SetupColorAndContrastControls()
        {
            if (contrastSlider != null)
            {
                contrastSlider.minValue = 0.5f;
                contrastSlider.maxValue = 2f;
                contrastSlider.onValueChanged.AddListener(OnContrastChanged);
            }
            
            if (brightnessSlider != null)
            {
                brightnessSlider.minValue = 0.5f;
                brightnessSlider.maxValue = 2f;
                brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            }
            
            if (invertColorsToggle != null)
                invertColorsToggle.onValueChanged.AddListener(OnInvertColorsChanged);
            
            if (grayscaleToggle != null)
                grayscaleToggle.onValueChanged.AddListener(OnGrayscaleChanged);
            
            if (saturationSlider != null)
            {
                saturationSlider.minValue = 0f;
                saturationSlider.maxValue = 2f;
                saturationSlider.onValueChanged.AddListener(OnSaturationChanged);
            }
            
            if (colorFilterTestButton != null)
                colorFilterTestButton.onClick.AddListener(TestColorFilters);
        }

        private void SetupTimeAndTimingControls()
        {
            if (disableTimePresureToggle != null)
                disableTimePresureToggle.onValueChanged.AddListener(OnDisableTimePressureChanged);
            
            if (interactionTimeoutSlider != null)
            {
                interactionTimeoutSlider.minValue = 5f;
                interactionTimeoutSlider.maxValue = 60f;
                interactionTimeoutSlider.onValueChanged.AddListener(OnInteractionTimeoutChanged);
            }
            
            if (pauseableContentToggle != null)
                pauseableContentToggle.onValueChanged.AddListener(OnPauseableContentChanged);
            
            if (noAutoplayToggle != null)
                noAutoplayToggle.onValueChanged.AddListener(OnNoAutoplayChanged);
            
            if (animationDurationSlider != null)
            {
                animationDurationSlider.minValue = 0.1f;
                animationDurationSlider.maxValue = 2f;
                animationDurationSlider.onValueChanged.AddListener(OnAnimationDurationChanged);
            }
        }

        private void SetupInputAssistanceControls()
        {
            if (inputPredictionToggle != null)
                inputPredictionToggle.onValueChanged.AddListener(OnInputPredictionChanged);
            
            if (autoCompleteToggle != null)
                autoCompleteToggle.onValueChanged.AddListener(OnAutoCompleteChanged);
            
            if (errorPreventionToggle != null)
                errorPreventionToggle.onValueChanged.AddListener(OnErrorPreventionChanged);
            
            if (confirmDangerousActionsToggle != null)
                confirmDangerousActionsToggle.onValueChanged.AddListener(OnConfirmDangerousActionsChanged);
            
            if (undoRedoSupportToggle != null)
                undoRedoSupportToggle.onValueChanged.AddListener(OnUndoRedoSupportChanged);
            
            if (inputToleranceSlider != null)
            {
                inputToleranceSlider.minValue = 1f;
                inputToleranceSlider.maxValue = 5f;
                inputToleranceSlider.onValueChanged.AddListener(OnInputToleranceChanged);
            }
        }

        private void SetupProfileManagement()
        {
            if (accessibilityProfileDropdown != null)
            {
                RefreshProfileDropdown();
                accessibilityProfileDropdown.onValueChanged.AddListener(OnProfileSelected);
            }
            
            if (saveProfileButton != null)
                saveProfileButton.onClick.AddListener(SaveCurrentProfile);
            
            if (loadProfileButton != null)
                loadProfileButton.onClick.AddListener(LoadSelectedProfile);
            
            if (deleteProfileButton != null)
                deleteProfileButton.onClick.AddListener(DeleteSelectedProfile);
            
            if (createProfileButton != null)
                createProfileButton.onClick.AddListener(CreateNewProfile);
        }
        
        // Cognitive Accessibility Event Handlers 추가
        private void OnSimplifiedUIChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.simplifiedUI = enabled;
            ApplySimplifiedUIChange();
            NotifySettingsChanged();
        }
        
        private void OnExtendedTimeoutsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.extendedTimeouts = enabled;
            NotifySettingsChanged();
        }
        
        private void OnReadingSpeedChanged(float speed)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.readingSpeed = speed;
            UpdateReadingSpeedText(speed);
            NotifySettingsChanged();
        }
        
        private void OnPauseOnDialogChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.pauseOnDialog = enabled;
            NotifySettingsChanged();
        }
        
        private void OnSkipComplexAnimationsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.skipComplexAnimations = enabled;
            ApplyReducedMotionChange();
            NotifySettingsChanged();
        }
        
        private void OnUseSimpleLanguageChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.useSimpleLanguage = enabled;
            NotifySettingsChanged();
        }
        
        private void OnShowProgressIndicatorsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.showProgressIndicators = enabled;
            NotifySettingsChanged();
        }
        
        private void OnAutoSaveFrequentlyChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.autoSaveFrequently = enabled;
            NotifySettingsChanged();
        }
        
        // Navigation Assistance Event Handlers 추가
        private void OnScreenReaderSupportChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.screenReaderSupport = enabled;
            NotifySettingsChanged();
        }
        
        private void OnTabNavigationChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.tabNavigation = enabled;
            NotifySettingsChanged();
        }
        
        private void OnFocusIndicatorChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.focusIndicator = enabled;
            NotifySettingsChanged();
        }
        
        private void OnFocusIndicatorSizeChanged(float size)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.focusIndicatorSize = size;
            UpdateFocusIndicatorSizeText(size);
            NotifySettingsChanged();
        }
        
        private void OnKeyboardOnlyModeChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.keyboardOnlyMode = enabled;
            NotifySettingsChanged();
        }
        
        private void OnVoiceNavigationChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.voiceNavigation = enabled;
            NotifySettingsChanged();
        }
        
        private void OnGestureNavigationChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.gestureNavigation = enabled;
            NotifySettingsChanged();
        }
        
        // Text and Reading Event Handlers 추가
        private void OnFontTypeChanged(int type)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.fontType = type;
            ApplyFontTypeChange();
            NotifySettingsChanged();
        }
        
        private void OnDyslexiaFriendlyFontChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.dyslexiaFriendlyFont = enabled;
            ApplyFontTypeChange();
            NotifySettingsChanged();
        }
        
        private void OnLineSpacingChanged(float spacing)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.lineSpacing = spacing;
            UpdateLineSpacingText(spacing);
            ApplyTextSpacingChange();
            NotifySettingsChanged();
        }
        
        private void OnWordSpacingChanged(float spacing)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.wordSpacing = spacing;
            UpdateWordSpacingText(spacing);
            ApplyTextSpacingChange();
            NotifySettingsChanged();
        }
        
        private void OnTextToSpeechChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.textToSpeech = enabled;
            NotifySettingsChanged();
        }
        
        private void OnSpeechRateChanged(float rate)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.speechRate = rate;
            UpdateSpeechRateText(rate);
            NotifySettingsChanged();
        }
        
        private void OnSpeechVoiceChanged(int voice)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.speechVoice = voice;
            NotifySettingsChanged();
        }
        
        // Color and Contrast Event Handlers 추가
        private void OnContrastChanged(float contrast)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.contrast = contrast;
            UpdateContrastText(contrast);
            ApplyContrastChange();
            NotifySettingsChanged();
        }
        
        private void OnBrightnessChanged(float brightness)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.brightness = brightness;
            UpdateBrightnessText(brightness);
            ApplyBrightnessChange();
            NotifySettingsChanged();
        }
        
        private void OnInvertColorsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.invertColors = enabled;
            ApplyColorInversionChange();
            NotifySettingsChanged();
        }
        
        private void OnGrayscaleChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.grayscale = enabled;
            ApplyGrayscaleChange();
            NotifySettingsChanged();
        }
        
        private void OnSaturationChanged(float saturation)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.saturation = saturation;
            UpdateSaturationText(saturation);
            ApplySaturationChange();
            NotifySettingsChanged();
        }
        
        // Time and Timing Event Handlers 추가
        private void OnDisableTimePressureChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.disableTimePressure = enabled;
            NotifySettingsChanged();
        }
        
        private void OnInteractionTimeoutChanged(float timeout)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.interactionTimeout = timeout;
            UpdateInteractionTimeoutText(timeout);
            NotifySettingsChanged();
        }
        
        private void OnPauseableContentChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.pauseableContent = enabled;
            NotifySettingsChanged();
        }
        
        private void OnNoAutoplayChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.noAutoplay = enabled;
            NotifySettingsChanged();
        }
        
        private void OnAnimationDurationChanged(float duration)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.animationDuration = duration;
            UpdateAnimationDurationText(duration);
            NotifySettingsChanged();
        }
        
        // Input Assistance Event Handlers 추가
        private void OnInputPredictionChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.inputPrediction = enabled;
            NotifySettingsChanged();
        }
        
        private void OnAutoCompleteChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.autoComplete = enabled;
            NotifySettingsChanged();
        }
        
        private void OnErrorPreventionChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.errorPrevention = enabled;
            NotifySettingsChanged();
        }
        
        private void OnConfirmDangerousActionsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.confirmDangerousActions = enabled;
            NotifySettingsChanged();
        }
        
        private void OnUndoRedoSupportChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.undoRedoSupport = enabled;
            NotifySettingsChanged();
        }
        
        private void OnInputToleranceChanged(float tolerance)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.inputTolerance = tolerance;
            UpdateInputToleranceText(tolerance);
            NotifySettingsChanged();
        }
        
        // 추가 Apply 메서드들
        private void ApplySimplifiedUIChange()
        {
            // 간소화된 UI 적용
        }
        
        private void ApplyFontTypeChange()
        {
            // 폰트 타입 변경 적용
        }
        
        private void ApplyTextSpacingChange()
        {
            // 텍스트 간격 변경 적용
        }
        
        private void ApplyContrastChange()
        {
            // 대비 변경 적용
        }
        
        private void ApplyBrightnessChange()
        {
            // 밝기 변경 적용
        }
        
        private void ApplyColorInversionChange()
        {
            // 색상 반전 적용
        }
        
        private void ApplyGrayscaleChange()
        {
            // 그레이스케일 적용
        }
        
        private void ApplySaturationChange()
        {
            // 채도 변경 적용
        }
        
        private void SetupTestingAndPreview()
        {
            if (showPreviewButton != null)
                showPreviewButton.onClick.AddListener(TogglePreview);
            
            if (testColorVisionButton != null)
                testColorVisionButton.onClick.AddListener(TestColorVision);
            
            if (testAudioCuesButton != null)
                testAudioCuesButton.onClick.AddListener(TestAudioCues);
            
            if (testNavigationButton != null)
                testNavigationButton.onClick.AddListener(TestNavigation);
            
            if (previewPanel != null)
                previewPanel.SetActive(false);
        }

        private void SetupResetAndPresets()
        {
            if (resetAllButton != null)
                resetAllButton.onClick.AddListener(ResetAllSettings);
            
            if (visionImpairmentPresetButton != null)
                visionImpairmentPresetButton.onClick.AddListener(() => ApplyAccessibilityPreset(AccessibilityPreset.VisionImpairment));
            
            if (hearingImpairmentPresetButton != null)
                hearingImpairmentPresetButton.onClick.AddListener(() => ApplyAccessibilityPreset(AccessibilityPreset.HearingImpairment));
            
            if (motorImpairmentPresetButton != null)
                motorImpairmentPresetButton.onClick.AddListener(() => ApplyAccessibilityPreset(AccessibilityPreset.MotorImpairment));
            
            if (cognitiveImpairmentPresetButton != null)
                cognitiveImpairmentPresetButton.onClick.AddListener(() => ApplyAccessibilityPreset(AccessibilityPreset.CognitiveImpairment));
            
            if (universalDesignPresetButton != null)
                universalDesignPresetButton.onClick.AddListener(() => ApplyAccessibilityPreset(AccessibilityPreset.UniversalDesign));
        }

        // Input handling
        private void HandleAccessibilityInput()
        {
            // Special accessibility shortcuts
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleHighContrast();
            }
            
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ToggleScreenReader();
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                CycleFontSize();
            }
            
            // Emergency accessibility reset
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F12))
            {
                EmergencyAccessibilityReset();
            }
        }

        private void LoadAccessibilitySettings()
        {
            // Load accessibility settings from PlayerPrefs
            _currentAccessibilitySettings.subtitlesEnabled = PlayerPrefs.GetInt("AccessibilitySubtitles", 0) == 1;
            _currentAccessibilitySettings.fontSize = PlayerPrefs.GetFloat("AccessibilityFontSize", 1f);
            _currentAccessibilitySettings.uiScale = PlayerPrefs.GetFloat("AccessibilityUIScale", 1f);
            _currentAccessibilitySettings.highContrast = PlayerPrefs.GetInt("AccessibilityHighContrast", 0) == 1;
            _currentAccessibilitySettings.colorBlindSupport = PlayerPrefs.GetInt("AccessibilityColorBlindSupport", 0) == 1;
            _currentAccessibilitySettings.colorBlindType = PlayerPrefs.GetInt("AccessibilityColorBlindType", 0);
            _currentAccessibilitySettings.reducedMotion = PlayerPrefs.GetInt("AccessibilityReducedMotion", 0) == 1;
            _currentAccessibilitySettings.largerCursor = PlayerPrefs.GetInt("AccessibilityLargerCursor", 0) == 1;
            _currentAccessibilitySettings.cursorSize = PlayerPrefs.GetFloat("AccessibilityCursorSize", 1f);
            
            _currentAccessibilitySettings.audioDescriptions = PlayerPrefs.GetInt("AccessibilityAudioDescriptions", 0) == 1;
            _currentAccessibilitySettings.visualAudioCues = PlayerPrefs.GetInt("AccessibilityVisualAudioCues", 0) == 1;
            _currentAccessibilitySettings.closedCaptions = PlayerPrefs.GetInt("AccessibilityClosedCaptions", 0) == 1;
            _currentAccessibilitySettings.captionSize = PlayerPrefs.GetFloat("AccessibilityCaptionSize", 1f);
            _currentAccessibilitySettings.captionStyle = PlayerPrefs.GetInt("AccessibilityCaptionStyle", 0);
            _currentAccessibilitySettings.soundVisualization = PlayerPrefs.GetInt("AccessibilitySoundVisualization", 0) == 1;
            _currentAccessibilitySettings.hapticFeedback = PlayerPrefs.GetInt("AccessibilityHapticFeedback", 1) == 1;
            _currentAccessibilitySettings.hapticIntensity = PlayerPrefs.GetFloat("AccessibilityHapticIntensity", 1f);
            
            _currentAccessibilitySettings.oneHandedMode = PlayerPrefs.GetInt("AccessibilityOneHandedMode", 0) == 1;
            _currentAccessibilitySettings.autoClick = PlayerPrefs.GetInt("AccessibilityAutoClick", 0) == 1;
            _currentAccessibilitySettings.autoClickDelay = PlayerPrefs.GetFloat("AccessibilityAutoClickDelay", 2f);
            _currentAccessibilitySettings.mouseKeys = PlayerPrefs.GetInt("AccessibilityMouseKeys", 0) == 1;
            _currentAccessibilitySettings.stickyKeys = PlayerPrefs.GetInt("AccessibilityStickyKeys", 0) == 1;
            _currentAccessibilitySettings.slowKeys = PlayerPrefs.GetInt("AccessibilitySlowKeys", 0) == 1;
            _currentAccessibilitySettings.slowKeysDelay = PlayerPrefs.GetFloat("AccessibilitySlowKeysDelay", 0.5f);
            _currentAccessibilitySettings.bounceKeys = PlayerPrefs.GetInt("AccessibilityBounceKeys", 0) == 1;
            _currentAccessibilitySettings.bounceKeysDelay = PlayerPrefs.GetFloat("AccessibilityBounceKeysDelay", 0.5f);
            
            _currentAccessibilitySettings.simplifiedUI = PlayerPrefs.GetInt("AccessibilitySimplifiedUI", 0) == 1;
            _currentAccessibilitySettings.extendedTimeouts = PlayerPrefs.GetInt("AccessibilityExtendedTimeouts", 0) == 1;
            _currentAccessibilitySettings.readingSpeed = PlayerPrefs.GetFloat("AccessibilityReadingSpeed", 1f);
            _currentAccessibilitySettings.pauseOnDialog = PlayerPrefs.GetInt("AccessibilityPauseOnDialog", 0) == 1;
            _currentAccessibilitySettings.skipComplexAnimations = PlayerPrefs.GetInt("AccessibilitySkipComplexAnimations", 0) == 1;
            _currentAccessibilitySettings.useSimpleLanguage = PlayerPrefs.GetInt("AccessibilityUseSimpleLanguage", 0) == 1;
            _currentAccessibilitySettings.showProgressIndicators = PlayerPrefs.GetInt("AccessibilityShowProgressIndicators", 1) == 1;
            _currentAccessibilitySettings.autoSaveFrequently = PlayerPrefs.GetInt("AccessibilityAutoSaveFrequently", 0) == 1;
            
            _currentAccessibilitySettings.screenReaderSupport = PlayerPrefs.GetInt("AccessibilityScreenReaderSupport", 0) == 1;
            _currentAccessibilitySettings.tabNavigation = PlayerPrefs.GetInt("AccessibilityTabNavigation", 1) == 1;
            _currentAccessibilitySettings.focusIndicator = PlayerPrefs.GetInt("AccessibilityFocusIndicator", 1) == 1;
            _currentAccessibilitySettings.focusIndicatorSize = PlayerPrefs.GetFloat("AccessibilityFocusIndicatorSize", 1f);
            _currentAccessibilitySettings.keyboardOnlyMode = PlayerPrefs.GetInt("AccessibilityKeyboardOnlyMode", 0) == 1;
            _currentAccessibilitySettings.voiceNavigation = PlayerPrefs.GetInt("AccessibilityVoiceNavigation", 0) == 1;
            _currentAccessibilitySettings.gestureNavigation = PlayerPrefs.GetInt("AccessibilityGestureNavigation", 0) == 1;
            
            _currentAccessibilitySettings.fontType = PlayerPrefs.GetInt("AccessibilityFontType", 0);
            _currentAccessibilitySettings.dyslexiaFriendlyFont = PlayerPrefs.GetInt("AccessibilityDyslexiaFriendlyFont", 0) == 1;
            _currentAccessibilitySettings.lineSpacing = PlayerPrefs.GetFloat("AccessibilityLineSpacing", 1f);
            _currentAccessibilitySettings.wordSpacing = PlayerPrefs.GetFloat("AccessibilityWordSpacing", 1f);
            _currentAccessibilitySettings.textToSpeech = PlayerPrefs.GetInt("AccessibilityTextToSpeech", 0) == 1;
            _currentAccessibilitySettings.speechRate = PlayerPrefs.GetFloat("AccessibilitySpeechRate", 1f);
            _currentAccessibilitySettings.speechVoice = PlayerPrefs.GetInt("AccessibilitySpeechVoice", 0);
            
            _currentAccessibilitySettings.contrast = PlayerPrefs.GetFloat("AccessibilityContrast", 1f);
            _currentAccessibilitySettings.brightness = PlayerPrefs.GetFloat("AccessibilityBrightness", 1f);
            _currentAccessibilitySettings.invertColors = PlayerPrefs.GetInt("AccessibilityInvertColors", 0) == 1;
            _currentAccessibilitySettings.grayscale = PlayerPrefs.GetInt("AccessibilityGrayscale", 0) == 1;
            _currentAccessibilitySettings.saturation = PlayerPrefs.GetFloat("AccessibilitySaturation", 1f);
            
            _currentAccessibilitySettings.disableTimePressure = PlayerPrefs.GetInt("AccessibilityDisableTimePressure", 0) == 1;
            _currentAccessibilitySettings.interactionTimeout = PlayerPrefs.GetFloat("AccessibilityInteractionTimeout", 30f);
            _currentAccessibilitySettings.pauseableContent = PlayerPrefs.GetInt("AccessibilityPauseableContent", 1) == 1;
            _currentAccessibilitySettings.noAutoplay = PlayerPrefs.GetInt("AccessibilityNoAutoplay", 0) == 1;
            _currentAccessibilitySettings.animationDuration = PlayerPrefs.GetFloat("AccessibilityAnimationDuration", 1f);
            
            _currentAccessibilitySettings.inputPrediction = PlayerPrefs.GetInt("AccessibilityInputPrediction", 0) == 1;
            _currentAccessibilitySettings.autoComplete = PlayerPrefs.GetInt("AccessibilityAutoComplete", 0) == 1;
            _currentAccessibilitySettings.errorPrevention = PlayerPrefs.GetInt("AccessibilityErrorPrevention", 1) == 1;
            _currentAccessibilitySettings.confirmDangerousActions = PlayerPrefs.GetInt("AccessibilityConfirmDangerousActions", 1) == 1;
            _currentAccessibilitySettings.undoRedoSupport = PlayerPrefs.GetInt("AccessibilityUndoRedoSupport", 1) == 1;
            _currentAccessibilitySettings.inputTolerance = PlayerPrefs.GetFloat("AccessibilityInputTolerance", 1f);
            
            ApplySettingsToUI(_currentAccessibilitySettings);
        }

        private void ApplySettingsToUI(AccessibilitySettings settings)
        {
            _isApplyingSettings = true;
            
            // Visual accessibility
            if (subtitlesToggle != null)
                subtitlesToggle.isOn = settings.subtitlesEnabled;
            
            if (fontSizeSlider != null)
            {
                fontSizeSlider.value = settings.fontSize;
                UpdateFontSizeText(settings.fontSize);
            }
            
            if (uiScaleSlider != null)
            {
                uiScaleSlider.value = settings.uiScale;
                UpdateUIScaleText(settings.uiScale);
            }
            
            if (highContrastToggle != null)
                highContrastToggle.isOn = settings.highContrast;
            
            if (colorBlindSupportToggle != null)
                colorBlindSupportToggle.isOn = settings.colorBlindSupport;
            
            if (colorBlindTypeDropdown != null)
                colorBlindTypeDropdown.value = settings.colorBlindType;
            
            if (reducedMotionToggle != null)
                reducedMotionToggle.isOn = settings.reducedMotion;
            
            if (largerCursorToggle != null)
                largerCursorToggle.isOn = settings.largerCursor;
            
            if (cursorSizeSlider != null)
            {
                cursorSizeSlider.value = settings.cursorSize;
                UpdateCursorSizeText(settings.cursorSize);
            }
            
            // Audio accessibility
            if (audioDescriptionsToggle != null)
                audioDescriptionsToggle.isOn = settings.audioDescriptions;
            
            if (visualAudioCuesToggle != null)
                visualAudioCuesToggle.isOn = settings.visualAudioCues;
            
            if (closedCaptionsToggle != null)
                closedCaptionsToggle.isOn = settings.closedCaptions;
            
            if (captionSizeSlider != null)
            {
                captionSizeSlider.value = settings.captionSize;
                UpdateCaptionSizeText(settings.captionSize);
            }
            
            if (captionStyleDropdown != null)
                captionStyleDropdown.value = settings.captionStyle;
            
            if (soundVisualizationToggle != null)
                soundVisualizationToggle.isOn = settings.soundVisualization;
            
            if (hapticFeedbackToggle != null)
                hapticFeedbackToggle.isOn = settings.hapticFeedback;
            
            if (hapticIntensitySlider != null)
            {
                hapticIntensitySlider.value = settings.hapticIntensity;
                UpdateHapticIntensityText(settings.hapticIntensity);
            }
            
            // Motor accessibility - continue from here...
            if (oneHandedModeToggle != null)
                oneHandedModeToggle.isOn = settings.oneHandedMode;
            
            if (autoClickToggle != null)
                autoClickToggle.isOn = settings.autoClick;
            
            if (autoClickDelaySlider != null)
            {
                autoClickDelaySlider.value = settings.autoClickDelay;
                UpdateAutoClickDelayText(settings.autoClickDelay);
            }
            
            if (mouseKeysToggle != null)
                mouseKeysToggle.isOn = settings.mouseKeys;
            
            if (stickyKeysToggle != null)
                stickyKeysToggle.isOn = settings.stickyKeys;
            
            if (slowKeysToggle != null)
                slowKeysToggle.isOn = settings.slowKeys;
            
            if (slowKeysDelaySlider != null)
            {
                slowKeysDelaySlider.value = settings.slowKeysDelay;
                UpdateSlowKeysDelayText(settings.slowKeysDelay);
            }
            
            if (bounceKeysToggle != null)
                bounceKeysToggle.isOn = settings.bounceKeys;
            
            if (bounceKeysDelaySlider != null)
            {
                bounceKeysDelaySlider.value = settings.bounceKeysDelay;
                UpdateBounceKeysDelayText(settings.bounceKeysDelay);
            }
            
            // Cognitive accessibility
            if (simplifiedUIToggle != null)
                simplifiedUIToggle.isOn = settings.simplifiedUI;
            
            if (extendedTimeoutsToggle != null)
                extendedTimeoutsToggle.isOn = settings.extendedTimeouts;
            
            if (readingSpeedSlider != null)
            {
                readingSpeedSlider.value = settings.readingSpeed;
                UpdateReadingSpeedText(settings.readingSpeed);
            }
            
            if (pauseOnDialogToggle != null)
                pauseOnDialogToggle.isOn = settings.pauseOnDialog;
            
            if (skipComplexAnimationsToggle != null)
                skipComplexAnimationsToggle.isOn = settings.skipComplexAnimations;
            
            if (useSimpleLanguageToggle != null)
                useSimpleLanguageToggle.isOn = settings.useSimpleLanguage;
            
            if (showProgressIndicatorsToggle != null)
                showProgressIndicatorsToggle.isOn = settings.showProgressIndicators;
            
            if (autoSaveFrequentlyToggle != null)
                autoSaveFrequentlyToggle.isOn = settings.autoSaveFrequently;
            
            // Navigation assistance
            if (screenReaderSupportToggle != null)
                screenReaderSupportToggle.isOn = settings.screenReaderSupport;
            
            if (tabNavigationToggle != null)
                tabNavigationToggle.isOn = settings.tabNavigation;
            
            if (focusIndicatorToggle != null)
                focusIndicatorToggle.isOn = settings.focusIndicator;
            
            if (focusIndicatorSizeSlider != null)
            {
                focusIndicatorSizeSlider.value = settings.focusIndicatorSize;
                UpdateFocusIndicatorSizeText(settings.focusIndicatorSize);
            }
            
            if (keyboardOnlyModeToggle != null)
                keyboardOnlyModeToggle.isOn = settings.keyboardOnlyMode;
            
            if (voiceNavigationToggle != null)
                voiceNavigationToggle.isOn = settings.voiceNavigation;
            
            if (gestureNavigationToggle != null)
                gestureNavigationToggle.isOn = settings.gestureNavigation;
            
            // Text and reading
            if (fontTypeDropdown != null)
                fontTypeDropdown.value = settings.fontType;
            
            if (dyslexiaFriendlyFontToggle != null)
                dyslexiaFriendlyFontToggle.isOn = settings.dyslexiaFriendlyFont;
            
            if (lineSpacingSlider != null)
            {
                lineSpacingSlider.value = settings.lineSpacing;
                UpdateLineSpacingText(settings.lineSpacing);
            }
            
            if (wordSpacingSlider != null)
            {
                wordSpacingSlider.value = settings.wordSpacing;
                UpdateWordSpacingText(settings.wordSpacing);
            }
            
            if (textToSpeechToggle != null)
                textToSpeechToggle.isOn = settings.textToSpeech;
            
            if (speechRateSlider != null)
            {
                speechRateSlider.value = settings.speechRate;
                UpdateSpeechRateText(settings.speechRate);
            }
            
            if (speechVoiceDropdown != null)
                speechVoiceDropdown.value = settings.speechVoice;
            
            // Color and contrast
            if (contrastSlider != null)
            {
                contrastSlider.value = settings.contrast;
                UpdateContrastText(settings.contrast);
            }
            
            if (brightnessSlider != null)
            {
                brightnessSlider.value = settings.brightness;
                UpdateBrightnessText(settings.brightness);
            }
            
            if (invertColorsToggle != null)
                invertColorsToggle.isOn = settings.invertColors;
            
            if (grayscaleToggle != null)
                grayscaleToggle.isOn = settings.grayscale;
            
            if (saturationSlider != null)
            {
                saturationSlider.value = settings.saturation;
                UpdateSaturationText(settings.saturation);
            }
            
            // Time and timing
            if (disableTimePresureToggle != null)
                disableTimePresureToggle.isOn = settings.disableTimePressure;
            
            if (interactionTimeoutSlider != null)
            {
                interactionTimeoutSlider.value = settings.interactionTimeout;
                UpdateInteractionTimeoutText(settings.interactionTimeout);
            }
            
            if (pauseableContentToggle != null)
                pauseableContentToggle.isOn = settings.pauseableContent;
            
            if (noAutoplayToggle != null)
                noAutoplayToggle.isOn = settings.noAutoplay;
            
            if (animationDurationSlider != null)
            {
                animationDurationSlider.value = settings.animationDuration;
                UpdateAnimationDurationText(settings.animationDuration);
            }
            
            // Input assistance
            if (inputPredictionToggle != null)
                inputPredictionToggle.isOn = settings.inputPrediction;
            
            if (autoCompleteToggle != null)
                autoCompleteToggle.isOn = settings.autoComplete;
            
            if (errorPreventionToggle != null)
                errorPreventionToggle.isOn = settings.errorPrevention;
            
            if (confirmDangerousActionsToggle != null)
                confirmDangerousActionsToggle.isOn = settings.confirmDangerousActions;
            
            if (undoRedoSupportToggle != null)
                undoRedoSupportToggle.isOn = settings.undoRedoSupport;
            
            if (inputToleranceSlider != null)
            {
                inputToleranceSlider.value = settings.inputTolerance;
                UpdateInputToleranceText(settings.inputTolerance);
            }
            
            _isApplyingSettings = false;
            
            // Apply effects to the game
            ApplyAccessibilityEffects();
        }

        // Event handlers start here - Visual Accessibility
        private void OnSubtitlesChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.subtitlesEnabled = enabled;
            NotifySettingsChanged();
        }

        private void OnFontSizeChanged(float size)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.fontSize = size;
            UpdateFontSizeText(size);
            ApplyFontSizeChange();
            NotifySettingsChanged();
        }

        private void OnUIScaleChanged(float scale)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.uiScale = scale;
            UpdateUIScaleText(scale);
            ApplyUIScaleChange();
            NotifySettingsChanged();
        }

        private void OnHighContrastChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.highContrast = enabled;
            ApplyHighContrastChange();
            NotifySettingsChanged();
        }

        private void OnColorBlindSupportChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.colorBlindSupport = enabled;
            ApplyColorBlindSupportChange();
            NotifySettingsChanged();
        }

        private void OnColorBlindTypeChanged(int type)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.colorBlindType = type;
            ApplyColorBlindSupportChange();
            NotifySettingsChanged();
        }

        private void OnReducedMotionChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.reducedMotion = enabled;
            ApplyReducedMotionChange();
            NotifySettingsChanged();
        }

        private void OnLargerCursorChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.largerCursor = enabled;
            ApplyCursorSizeChange();
            NotifySettingsChanged();
        }

        private void OnCursorSizeChanged(float size)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.cursorSize = size;
            UpdateCursorSizeText(size);
            ApplyCursorSizeChange();
            NotifySettingsChanged();
        }

        // Audio Accessibility Event Handlers
        private void OnAudioDescriptionsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.audioDescriptions = enabled;
            NotifySettingsChanged();
        }

        private void OnVisualAudioCuesChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.visualAudioCues = enabled;
            ApplyVisualAudioCuesChange();
            NotifySettingsChanged();
        }

        private void OnClosedCaptionsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.closedCaptions = enabled;
            NotifySettingsChanged();
        }

        private void OnCaptionSizeChanged(float size)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.captionSize = size;
            UpdateCaptionSizeText(size);
            NotifySettingsChanged();
        }

        private void OnCaptionStyleChanged(int style)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.captionStyle = style;
            NotifySettingsChanged();
        }

        private void OnSoundVisualizationChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.soundVisualization = enabled;
            ApplySoundVisualizationChange();
            NotifySettingsChanged();
        }

        private void OnHapticFeedbackChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.hapticFeedback = enabled;
            NotifySettingsChanged();
        }

        private void OnHapticIntensityChanged(float intensity)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.hapticIntensity = intensity;
            UpdateHapticIntensityText(intensity);
            NotifySettingsChanged();
        }

        // Motor Accessibility Event Handlers
        private void OnOneHandedModeChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.oneHandedMode = enabled;
            ApplyOneHandedModeChange();
            NotifySettingsChanged();
        }

        private void OnAutoClickChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.autoClick = enabled;
            NotifySettingsChanged();
        }

        private void OnAutoClickDelayChanged(float delay)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.autoClickDelay = delay;
            UpdateAutoClickDelayText(delay);
            NotifySettingsChanged();
        }

        private void OnMouseKeysChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.mouseKeys = enabled;
            NotifySettingsChanged();
        }

        private void OnStickyKeysChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.stickyKeys = enabled;
            NotifySettingsChanged();
        }

        private void OnSlowKeysChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.slowKeys = enabled;
            NotifySettingsChanged();
        }

        private void OnSlowKeysDelayChanged(float delay)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.slowKeysDelay = delay;
            UpdateSlowKeysDelayText(delay);
            NotifySettingsChanged();
        }

        private void OnBounceKeysChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.bounceKeys = enabled;
            NotifySettingsChanged();
        }

        private void OnBounceKeysDelayChanged(float delay)
        {
            if (_isApplyingSettings) return;
            
            _currentAccessibilitySettings.bounceKeysDelay = delay;
            UpdateBounceKeysDelayText(delay);
            NotifySettingsChanged();
        }

        // Continue with remaining event handlers...
        // (Due to length constraints, I'll include the key remaining methods)

        // Text update methods
        private void UpdateFontSizeText(float size)
        {
            if (fontSizeText != null)
                fontSizeText.text = $"{size:F1}x";
        }

        private void UpdateUIScaleText(float scale)
        {
            if (uiScaleText != null)
                uiScaleText.text = $"{scale:F1}x";
        }

        private void UpdateCursorSizeText(float size)
        {
            if (cursorSizeText != null)
                cursorSizeText.text = $"{size:F1}x";
        }

        private void UpdateCaptionSizeText(float size)
        {
            if (captionSizeText != null)
                captionSizeText.text = $"{size:F1}x";
        }

        private void UpdateHapticIntensityText(float intensity)
        {
            if (hapticIntensityText != null)
                hapticIntensityText.text = $"{intensity:F1}x";
        }

        private void UpdateAutoClickDelayText(float delay)
        {
            if (autoClickDelayText != null)
                autoClickDelayText.text = $"{delay:F1}s";
        }

        private void UpdateSlowKeysDelayText(float delay)
        {
            if (slowKeysDelayText != null)
                slowKeysDelayText.text = $"{delay:F1}s";
        }

        private void UpdateBounceKeysDelayText(float delay)
        {
            if (bounceKeysDelayText != null)
                bounceKeysDelayText.text = $"{delay:F1}s";
        }

        private void UpdateReadingSpeedText(float speed)
        {
            if (readingSpeedText != null)
                readingSpeedText.text = $"{speed:F1}x";
        }

        private void UpdateFocusIndicatorSizeText(float size)
        {
            if (focusIndicatorSizeText != null)
                focusIndicatorSizeText.text = $"{size:F1}x";
        }

        private void UpdateLineSpacingText(float spacing)
        {
            if (lineSpacingText != null)
                lineSpacingText.text = $"{spacing:F1}x";
        }

        private void UpdateWordSpacingText(float spacing)
        {
            if (wordSpacingText != null)
                wordSpacingText.text = $"{spacing:F1}x";
        }

        private void UpdateSpeechRateText(float rate)
        {
            if (speechRateText != null)
                speechRateText.text = $"{rate:F1}x";
        }

        private void UpdateContrastText(float contrast)
        {
            if (contrastText != null)
                contrastText.text = $"{contrast:F1}x";
        }

        private void UpdateBrightnessText(float brightness)
        {
            if (brightnessText != null)
                brightnessText.text = $"{brightness:F1}x";
        }

        private void UpdateSaturationText(float saturation)
        {
            if (saturationText != null)
                saturationText.text = $"{saturation:F1}x";
        }

        private void UpdateInteractionTimeoutText(float timeout)
        {
            if (interactionTimeoutText != null)
                interactionTimeoutText.text = $"{timeout:F0}s";
        }

        private void UpdateAnimationDurationText(float duration)
        {
            if (animationDurationText != null)
                animationDurationText.text = $"{duration:F1}x";
        }

        private void UpdateInputToleranceText(float tolerance)
        {
            if (inputToleranceText != null)
                inputToleranceText.text = $"{tolerance:F1}x";
        }

        // Apply effects methods
        private void ApplyAccessibilityEffects()
        {
            ApplyFontSizeChange();
            ApplyUIScaleChange();
            ApplyHighContrastChange();
            ApplyColorBlindSupportChange();
            ApplyReducedMotionChange();
            ApplyCursorSizeChange();
            ApplyVisualAudioCuesChange();
            ApplySoundVisualizationChange();
            ApplyOneHandedModeChange();
        }

        private void ApplyFontSizeChange()
        {
            // Apply font size changes to all UI text
            var allTexts = FindObjectsOfType<TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                if (text.gameObject.activeInHierarchy)
                {
                    text.fontSize *= _currentAccessibilitySettings.fontSize;
                }
            }
        }

        private void ApplyUIScaleChange()
        {
            // Apply UI scale changes
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvas.scaleFactor = _currentAccessibilitySettings.uiScale;
            }
        }

        private void ApplyHighContrastChange()
        {
            // Apply high contrast mode
            if (_currentAccessibilitySettings.highContrast)
            {
                // Increase contrast of all UI elements
            }
        }

        private void ApplyColorBlindSupportChange()
        {
            // Apply color blind filters
            if (_currentAccessibilitySettings.colorBlindSupport)
            {
                // Apply appropriate color filters based on type
            }
        }

        private void ApplyReducedMotionChange()
        {
            // Reduce or disable animations
            if (_currentAccessibilitySettings.reducedMotion)
            {
                // Disable or reduce motion effects
            }
        }

        private void ApplyCursorSizeChange()
        {
            // Apply cursor size changes
            if (_currentAccessibilitySettings.largerCursor)
            {
                // Set custom cursor with appropriate size
            }
        }

        private void ApplyVisualAudioCuesChange()
        {
            // Enable visual representations of audio
            if (_currentAccessibilitySettings.visualAudioCues)
            {
                // Show visual indicators for sounds
            }
        }

        private void ApplySoundVisualizationChange()
        {
            // Enable sound visualization
            if (_currentAccessibilitySettings.soundVisualization)
            {
                // Show waveforms or other audio visualizations
            }
        }

        private void ApplyOneHandedModeChange()
        {
            // Adjust UI layout for one-handed use
            if (_currentAccessibilitySettings.oneHandedMode)
            {
                // Reposition UI elements for easier one-handed access
            }
        }

                // Accessibility shortcuts
        private void ToggleHighContrast()
        {
            _currentAccessibilitySettings.highContrast = !_currentAccessibilitySettings.highContrast;
            if (highContrastToggle != null)
                highContrastToggle.isOn = _currentAccessibilitySettings.highContrast;
            
            ApplyHighContrastChange();
            NotifySettingsChanged();
            PlayAccessibilityChangeSound();
        }

        private void ToggleScreenReader()
        {
            _currentAccessibilitySettings.screenReaderSupport = !_currentAccessibilitySettings.screenReaderSupport;
            if (screenReaderSupportToggle != null)
                screenReaderSupportToggle.isOn = _currentAccessibilitySettings.screenReaderSupport;
            
            NotifySettingsChanged();
            PlayAccessibilityChangeSound();
        }

        private void CycleFontSize()
        {
            float[] fontSizes = { 0.75f, 1f, 1.25f, 1.5f, 2f };
            int currentIndex = System.Array.IndexOf(fontSizes, _currentAccessibilitySettings.fontSize);
            int nextIndex = (currentIndex + 1) % fontSizes.Length;
            
            _currentAccessibilitySettings.fontSize = fontSizes[nextIndex];
            if (fontSizeSlider != null)
                fontSizeSlider.value = _currentAccessibilitySettings.fontSize;
            
            ApplyFontSizeChange();
            NotifySettingsChanged();
            PlayAccessibilityChangeSound();
        }

        private void EmergencyAccessibilityReset()
        {
            _currentAccessibilitySettings = CreateDefaultAccessibilitySettings();
            ApplySettingsToUI(_currentAccessibilitySettings);
            NotifySettingsChanged();
            PlayConfirmationSound();
            
            Debug.Log("Emergency accessibility reset activated!");
        }

        // Profile management
        private void CreateDefaultProfiles()
        {
            _accessibilityProfiles.Clear();
            
            // Default profile
            _accessibilityProfiles.Add(new AccessibilityProfile
            {
                name = "Default",
                settings = CreateDefaultAccessibilitySettings()
            });
            
            // Vision impairment profile
            _accessibilityProfiles.Add(new AccessibilityProfile
            {
                name = "Vision Assistance",
                settings = CreateVisionImpairmentSettings()
            });
            
            // Hearing impairment profile
            _accessibilityProfiles.Add(new AccessibilityProfile
            {
                name = "Hearing Assistance",
                settings = CreateHearingImpairmentSettings()
            });
            
            // Motor impairment profile
            _accessibilityProfiles.Add(new AccessibilityProfile
            {
                name = "Motor Assistance",
                settings = CreateMotorImpairmentSettings()
            });
            
            // Cognitive impairment profile
            _accessibilityProfiles.Add(new AccessibilityProfile
            {
                name = "Cognitive Assistance",
                settings = CreateCognitiveImpairmentSettings()
            });
            
            // Universal design profile
            _accessibilityProfiles.Add(new AccessibilityProfile
            {
                name = "Universal Design",
                settings = CreateUniversalDesignSettings()
            });
        }

        private void RefreshProfileDropdown()
        {
            if (accessibilityProfileDropdown == null) return;
            
            accessibilityProfileDropdown.ClearOptions();
            var profileNames = _accessibilityProfiles.Select(p => p.name).ToList();
            accessibilityProfileDropdown.AddOptions(profileNames);
        }

        private void OnProfileSelected(int index)
        {
            if (index >= 0 && index < _accessibilityProfiles.Count)
            {
                _currentProfileName = _accessibilityProfiles[index].name;
            }
        }

        private void SaveCurrentProfile()
        {
            PlayButtonClickSound();
            
            if (string.IsNullOrEmpty(_currentProfileName)) return;
            
            var existingProfile = _accessibilityProfiles.FirstOrDefault(p => p.name == _currentProfileName);
            if (existingProfile != null)
            {
                existingProfile.settings = _currentAccessibilitySettings.Clone();
            }
            
            Debug.Log($"Saved accessibility profile: {_currentProfileName}");
        }

        private void LoadSelectedProfile()
        {
            PlayButtonClickSound();
            
            var selectedProfile = _accessibilityProfiles.FirstOrDefault(p => p.name == _currentProfileName);
            if (selectedProfile != null)
            {
                _currentAccessibilitySettings = selectedProfile.settings.Clone();
                ApplySettingsToUI(_currentAccessibilitySettings);
                NotifySettingsChanged();
                
                Debug.Log($"Loaded accessibility profile: {_currentProfileName}");
            }
        }

        private void DeleteSelectedProfile()
        {
            PlayButtonClickSound();
            
            if (_currentProfileName == "Default") return; // Can't delete default profile
            
            var profileToDelete = _accessibilityProfiles.FirstOrDefault(p => p.name == _currentProfileName);
            if (profileToDelete != null)
            {
                _accessibilityProfiles.Remove(profileToDelete);
                RefreshProfileDropdown();
                
                Debug.Log($"Deleted accessibility profile: {_currentProfileName}");
            }
        }

        private void CreateNewProfile()
        {
            PlayButtonClickSound();
            
            if (profileNameInput == null || string.IsNullOrEmpty(profileNameInput.text)) return;
            
            string newProfileName = profileNameInput.text.Trim();
            if (_accessibilityProfiles.Any(p => p.name == newProfileName))
            {
                Debug.Log("Profile name already exists!");
                return;
            }
            
            _accessibilityProfiles.Add(new AccessibilityProfile
            {
                name = newProfileName,
                settings = _currentAccessibilitySettings.Clone()
            });
            
            RefreshProfileDropdown();
            _currentProfileName = newProfileName;
            
            Debug.Log($"Created new accessibility profile: {newProfileName}");
        }

        // Testing and preview
        private void UpdateTesting()
        {
            // Update any ongoing accessibility tests
        }

        private void TogglePreview()
        {
            if (previewPanel != null)
            {
                bool isActive = previewPanel.activeInHierarchy;
                previewPanel.SetActive(!isActive);
                
                if (!isActive)
                {
                    UpdatePreviewContent();
                }
            }
        }

        private void UpdatePreviewContent()
        {
            if (previewText != null)
            {
                previewText.text = "This is sample text to preview accessibility settings. " +
                                 "You can see how font size, contrast, and other visual settings affect readability.";
                
                // Apply current accessibility settings to preview
                previewText.fontSize *= _currentAccessibilitySettings.fontSize;
            }
        }

        private void TestColorVision()
        {
            StartCoroutine(TestColorVisionCoroutine());
        }

        private IEnumerator TestColorVisionCoroutine()
        {
            PlayTestSound();
            _isTestingInProgress = true;
            
            if (testingIndicator != null)
                testingIndicator.SetActive(true);
            
            Debug.Log("Running color vision test...");
            
            // Simulate color vision test
            yield return new WaitForSecondsRealtime(3f);
            
            Debug.Log("Color vision test completed!");
            
            if (testingIndicator != null)
                testingIndicator.SetActive(false);
            
            _isTestingInProgress = false;
        }

        private void TestAudioCues()
        {
            StartCoroutine(TestAudioCuesCoroutine());
        }

        private IEnumerator TestAudioCuesCoroutine()
        {
            PlayTestSound();
            _isTestingInProgress = true;
            
            if (testingIndicator != null)
                testingIndicator.SetActive(true);
            
            Debug.Log("Testing audio cues...");
            
            // Simulate audio cues test
            yield return new WaitForSecondsRealtime(2f);
            
            Debug.Log("Audio cues test completed!");
            
            if (testingIndicator != null)
                testingIndicator.SetActive(false);
            
            _isTestingInProgress = false;
        }

        private void TestNavigation()
        {
            StartCoroutine(TestNavigationCoroutine());
        }

        private IEnumerator TestNavigationCoroutine()
        {
            PlayTestSound();
            _isTestingInProgress = true;
            
            if (testingIndicator != null)
                testingIndicator.SetActive(true);
            
            Debug.Log("Testing navigation accessibility...");
            
            // Simulate navigation test
            yield return new WaitForSecondsRealtime(4f);
            
            Debug.Log("Navigation test completed!");
            
            if (testingIndicator != null)
                testingIndicator.SetActive(false);
            
            _isTestingInProgress = false;
        }

        private void TestColorFilters()
        {
            PlayTestSound();
            Debug.Log("Testing color filters for accessibility");
        }

        // Reset and presets
        private void ResetAllSettings()
        {
            PlayButtonClickSound();
            
            _currentAccessibilitySettings = CreateDefaultAccessibilitySettings();
            ApplySettingsToUI(_currentAccessibilitySettings);
            NotifySettingsChanged();
            
            if (accessibilityChangeEffect != null)
                accessibilityChangeEffect.Play();
        }

        private void ApplyAccessibilityPreset(AccessibilityPreset preset)
        {
            PlayAccessibilityChangeSound();
            
            switch (preset)
            {
                case AccessibilityPreset.VisionImpairment:
                    _currentAccessibilitySettings = CreateVisionImpairmentSettings();
                    break;
                case AccessibilityPreset.HearingImpairment:
                    _currentAccessibilitySettings = CreateHearingImpairmentSettings();
                    break;
                case AccessibilityPreset.MotorImpairment:
                    _currentAccessibilitySettings = CreateMotorImpairmentSettings();
                    break;
                case AccessibilityPreset.CognitiveImpairment:
                    _currentAccessibilitySettings = CreateCognitiveImpairmentSettings();
                    break;
                case AccessibilityPreset.UniversalDesign:
                    _currentAccessibilitySettings = CreateUniversalDesignSettings();
                    break;
            }
            
            ApplySettingsToUI(_currentAccessibilitySettings);
            NotifySettingsChanged();
            
            if (accessibilityChangeEffect != null)
                accessibilityChangeEffect.Play();
        }

        // Create preset settings
        private AccessibilitySettings CreateDefaultAccessibilitySettings()
        {
            return new AccessibilitySettings
            {
                // All default values
                fontSize = 1f,
                uiScale = 1f,
                contrast = 1f,
                brightness = 1f,
                saturation = 1f,
                cursorSize = 1f,
                captionSize = 1f,
                hapticIntensity = 1f,
                autoClickDelay = 2f,
                slowKeysDelay = 0.5f,
                bounceKeysDelay = 0.5f,
                readingSpeed = 1f,
                focusIndicatorSize = 1f,
                lineSpacing = 1f,
                wordSpacing = 1f,
                speechRate = 1f,
                interactionTimeout = 30f,
                animationDuration = 1f,
                inputTolerance = 1f,
                
                // Default toggles
                tabNavigation = true,
                focusIndicator = true,
                showProgressIndicators = true,
                errorPrevention = true,
                confirmDangerousActions = true,
                undoRedoSupport = true,
                pauseableContent = true,
                hapticFeedback = true
            };
        }

        private AccessibilitySettings CreateVisionImpairmentSettings()
        {
            var settings = CreateDefaultAccessibilitySettings();
            
            // Vision-specific adjustments
            settings.fontSize = 1.5f;
            settings.uiScale = 1.25f;
            settings.highContrast = true;
            settings.largerCursor = true;
            settings.cursorSize = 2f;
            settings.screenReaderSupport = true;
            settings.textToSpeech = true;
            settings.colorBlindSupport = true;
            settings.focusIndicatorSize = 2f;
            settings.reducedMotion = true;
            
            return settings;
        }

        private AccessibilitySettings CreateHearingImpairmentSettings()
        {
            var settings = CreateDefaultAccessibilitySettings();
            
            // Hearing-specific adjustments
            settings.subtitlesEnabled = true;
            settings.closedCaptions = true;
            settings.captionSize = 1.25f;
            settings.visualAudioCues = true;
            settings.soundVisualization = true;
            settings.hapticFeedback = true;
            settings.hapticIntensity = 1.5f;
            
            return settings;
        }

        private AccessibilitySettings CreateMotorImpairmentSettings()
        {
            var settings = CreateDefaultAccessibilitySettings();
            
            // Motor-specific adjustments
            settings.oneHandedMode = true;
            settings.autoClick = true;
            settings.autoClickDelay = 3f;
            settings.stickyKeys = true;
            settings.slowKeys = true;
            settings.slowKeysDelay = 1f;
            settings.mouseKeys = true;
            settings.keyboardOnlyMode = true;
            settings.extendedTimeouts = true;
            settings.interactionTimeout = 60f;
            settings.inputTolerance = 2f;
            
            return settings;
        }

        private AccessibilitySettings CreateCognitiveImpairmentSettings()
        {
            var settings = CreateDefaultAccessibilitySettings();
            
            // Cognitive-specific adjustments
            settings.simplifiedUI = true;
            settings.useSimpleLanguage = true;
            settings.showProgressIndicators = true;
            settings.extendedTimeouts = true;
            settings.pauseOnDialog = true;
            settings.skipComplexAnimations = true;
            settings.autoSaveFrequently = true;
            settings.disableTimePressure = true;
            settings.noAutoplay = true;
            settings.readingSpeed = 0.75f;
            settings.inputPrediction = true;
            settings.autoComplete = true;
            
            return settings;
        }

        private AccessibilitySettings CreateUniversalDesignSettings()
        {
            var settings = CreateDefaultAccessibilitySettings();
            
            // Universal design principles
            settings.fontSize = 1.1f;
            settings.uiScale = 1.1f;
            settings.contrast = 1.1f;
            settings.focusIndicator = true;
            settings.focusIndicatorSize = 1.2f;
            settings.tabNavigation = true;
            settings.showProgressIndicators = true;
            settings.errorPrevention = true;
            settings.confirmDangerousActions = true;
            settings.undoRedoSupport = true;
            settings.extendedTimeouts = true;
            settings.pauseableContent = true;
            
            return settings;
        }

        // Settings interface
        public void ApplySettings(SettingsData settings)
        {
            _currentAccessibilitySettings.subtitlesEnabled = settings.subtitlesEnabled;
            _currentAccessibilitySettings.colorBlindSupport = settings.colorBlindSupport;
            _currentAccessibilitySettings.fontSize = settings.fontSize;
            _currentAccessibilitySettings.uiScale = settings.uiScale;
            
            ApplySettingsToUI(_currentAccessibilitySettings);
        }

        public void CollectSettings(ref SettingsData settings)
        {
            settings.subtitlesEnabled = _currentAccessibilitySettings.subtitlesEnabled;
            settings.colorBlindSupport = _currentAccessibilitySettings.colorBlindSupport;
            settings.fontSize = _currentAccessibilitySettings.fontSize;
            settings.uiScale = _currentAccessibilitySettings.uiScale;
        }

        private void NotifySettingsChanged()
        {
            var settingsPanel = GetComponentInParent<SettingsPanel>();
            settingsPanel?.OnSettingsChanged();
        }

        // Audio methods
        private void PlayAccessibilityChangeSound()
        {
            Debug.Log("Accessibility change sound would play here");
        }

        private void PlayTestSound()
        {
            Debug.Log("Test sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        private void PlayConfirmationSound()
        {
            Debug.Log("Confirmation sound would play here");
        }

        // Public interface
        public AccessibilitySettings CurrentAccessibilitySettings => _currentAccessibilitySettings;
        public List<AccessibilityProfile> AccessibilityProfiles => _accessibilityProfiles;
        
        // Emergency accessibility methods (for critical situations)
        public void EmergencyFontIncrease()
        {
            _currentAccessibilitySettings.fontSize = Mathf.Min(_currentAccessibilitySettings.fontSize + 0.25f, 3f);
            ApplyFontSizeChange();
        }
        
        public void EmergencyHighContrast()
        {
            _currentAccessibilitySettings.highContrast = true;
            ApplyHighContrastChange();
        }
    }

    // Data structures and enums
    [System.Serializable]
    public class AccessibilitySettings
    {
        [Header("Visual Accessibility")]
        public bool subtitlesEnabled = false;
        public float fontSize = 1f;
        public float uiScale = 1f;
        public bool highContrast = false;
        public bool colorBlindSupport = false;
        public int colorBlindType = 0;
        public bool reducedMotion = false;
        public bool largerCursor = false;
        public float cursorSize = 1f;
        
        [Header("Audio Accessibility")]
        public bool audioDescriptions = false;
        public bool visualAudioCues = false;
        public bool closedCaptions = false;
        public float captionSize = 1f;
        public int captionStyle = 0;
        public bool soundVisualization = false;
        public bool hapticFeedback = true;
        public float hapticIntensity = 1f;
        
        [Header("Motor Accessibility")]
        public bool oneHandedMode = false;
        public bool autoClick = false;
        public float autoClickDelay = 2f;
        public bool mouseKeys = false;
        public bool stickyKeys = false;
        public bool slowKeys = false;
        public float slowKeysDelay = 0.5f;
        public bool bounceKeys = false;
        public float bounceKeysDelay = 0.5f;
        
        [Header("Cognitive Accessibility")]
        public bool simplifiedUI = false;
        public bool extendedTimeouts = false;
        public float readingSpeed = 1f;
        public bool pauseOnDialog = false;
        public bool skipComplexAnimations = false;
        public bool useSimpleLanguage = false;
        public bool showProgressIndicators = true;
        public bool autoSaveFrequently = false;
        
        [Header("Navigation Assistance")]
        public bool screenReaderSupport = false;
        public bool tabNavigation = true;
        public bool focusIndicator = true;
        public float focusIndicatorSize = 1f;
        public bool keyboardOnlyMode = false;
        public bool voiceNavigation = false;
        public bool gestureNavigation = false;
        
        [Header("Text and Reading")]
        public int fontType = 0;
        public bool dyslexiaFriendlyFont = false;
        public float lineSpacing = 1f;
        public float wordSpacing = 1f;
        public bool textToSpeech = false;
        public float speechRate = 1f;
        public int speechVoice = 0;
        
        [Header("Color and Contrast")]
        public float contrast = 1f;
        public float brightness = 1f;
        public bool invertColors = false;
        public bool grayscale = false;
        public float saturation = 1f;
        
        [Header("Time and Timing")]
        public bool disableTimePressure = false;
        public float interactionTimeout = 30f;
        public bool pauseableContent = true;
        public bool noAutoplay = false;
        public float animationDuration = 1f;
        
        [Header("Input Assistance")]
        public bool inputPrediction = false;
        public bool autoComplete = false;
        public bool errorPrevention = true;
        public bool confirmDangerousActions = true;
        public bool undoRedoSupport = true;
        public float inputTolerance = 1f;
        
        public AccessibilitySettings Clone()
        {
            return new AccessibilitySettings
            {
                subtitlesEnabled = subtitlesEnabled,
                fontSize = fontSize,
                uiScale = uiScale,
                highContrast = highContrast,
                colorBlindSupport = colorBlindSupport,
                colorBlindType = colorBlindType,
                reducedMotion = reducedMotion,
                largerCursor = largerCursor,
                cursorSize = cursorSize,
                
                audioDescriptions = audioDescriptions,
                visualAudioCues = visualAudioCues,
                closedCaptions = closedCaptions,
                captionSize = captionSize,
                captionStyle = captionStyle,
                soundVisualization = soundVisualization,
                hapticFeedback = hapticFeedback,
                hapticIntensity = hapticIntensity,
                
                oneHandedMode = oneHandedMode,
                autoClick = autoClick,
                autoClickDelay = autoClickDelay,
                mouseKeys = mouseKeys,
                stickyKeys = stickyKeys,
                slowKeys = slowKeys,
                slowKeysDelay = slowKeysDelay,
                bounceKeys = bounceKeys,
                bounceKeysDelay = bounceKeysDelay,
                
                simplifiedUI = simplifiedUI,
                extendedTimeouts = extendedTimeouts,
                readingSpeed = readingSpeed,
                pauseOnDialog = pauseOnDialog,
                skipComplexAnimations = skipComplexAnimations,
                useSimpleLanguage = useSimpleLanguage,
                showProgressIndicators = showProgressIndicators,
                autoSaveFrequently = autoSaveFrequently,
                
                screenReaderSupport = screenReaderSupport,
                tabNavigation = tabNavigation,
                focusIndicator = focusIndicator,
                focusIndicatorSize = focusIndicatorSize,
                keyboardOnlyMode = keyboardOnlyMode,
                voiceNavigation = voiceNavigation,
                gestureNavigation = gestureNavigation,
                
                fontType = fontType,
                dyslexiaFriendlyFont = dyslexiaFriendlyFont,
                lineSpacing = lineSpacing,
                wordSpacing = wordSpacing,
                textToSpeech = textToSpeech,
                speechRate = speechRate,
                speechVoice = speechVoice,
                
                contrast = contrast,
                brightness = brightness,
                invertColors = invertColors,
                grayscale = grayscale,
                saturation = saturation,
                
                disableTimePressure = disableTimePressure,
                interactionTimeout = interactionTimeout,
                pauseableContent = pauseableContent,
                noAutoplay = noAutoplay,
                animationDuration = animationDuration,
                
                inputPrediction = inputPrediction,
                autoComplete = autoComplete,
                errorPrevention = errorPrevention,
                confirmDangerousActions = confirmDangerousActions,
                undoRedoSupport = undoRedoSupport,
                inputTolerance = inputTolerance
            };
        }
    }

    [System.Serializable]
    public class AccessibilityProfile
    {
        public string name;
        public AccessibilitySettings settings;
    }

    public enum AccessibilityPreset
    {
        VisionImpairment,
        HearingImpairment,
        MotorImpairment,
        CognitiveImpairment,
        UniversalDesign
    }
}