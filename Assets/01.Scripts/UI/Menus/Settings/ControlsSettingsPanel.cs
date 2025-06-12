using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.Settings
{
    public class ControlsSettingsPanel : UIPanel
    {
        [Header("Input Method")]
        [SerializeField] private TMP_Dropdown inputMethodDropdown;
        [SerializeField] private Toggle keyboardMouseToggle;
        [SerializeField] private Toggle gamepadToggle;
        [SerializeField] private Toggle touchControlsToggle;
        
        [Header("Mouse Settings")]
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private TextMeshProUGUI mouseSensitivityText;
        [SerializeField] private Toggle invertMouseXToggle;
        [SerializeField] private Toggle invertMouseYToggle;
        [SerializeField] private Toggle rawInputToggle;
        [SerializeField] private Slider mouseAccelerationSlider;
        [SerializeField] private TextMeshProUGUI mouseAccelerationText;
        
        [Header("Gamepad Settings")]
        [SerializeField] private Slider gamepadSensitivitySlider;
        [SerializeField] private TextMeshProUGUI gamepadSensitivityText;
        [SerializeField] private Toggle invertGamepadXToggle;
        [SerializeField] private Toggle invertGamepadYToggle;
        [SerializeField] private Slider gamepadDeadzoneSlider;
        [SerializeField] private TextMeshProUGUI gamepadDeadzoneText;
        [SerializeField] private Toggle gamepadVibrationToggle;
        [SerializeField] private Slider vibrationIntensitySlider;
        [SerializeField] private TextMeshProUGUI vibrationIntensityText;
        
        [Header("Key Bindings")]
        [SerializeField] private Transform keyBindingsContainer;
        [SerializeField] private GameObject keyBindingItemPrefab;
        [SerializeField] private ScrollRect keyBindingsScrollRect;
        [SerializeField] private Button resetKeyBindingsButton;
        [SerializeField] private Button importBindingsButton;
        [SerializeField] private Button exportBindingsButton;
        
        [Header("Input Testing")]
        [SerializeField] private GameObject inputTestPanel;
        [SerializeField] private Button showInputTestButton;
        [SerializeField] private TextMeshProUGUI currentInputText;
        [SerializeField] private Image inputVisualization;
        [SerializeField] private Toggle enableInputTestToggle;
        
        [Header("Accessibility")]
        [SerializeField] private Toggle holdToToggleToggle;
        [SerializeField] private Slider holdDelaySlider;
        [SerializeField] private TextMeshProUGUI holdDelayText;
        [SerializeField] private Toggle oneHandedModeToggle;
        [SerializeField] private Toggle colorBlindAssistToggle;
        [SerializeField] private Toggle buttonPromptsToggle;
        
        [Header("Advanced")]
        [SerializeField] private Toggle autoRunToggle;
        [SerializeField] private Toggle autoAimToggle;
        [SerializeField] private Slider autoAimStrengthSlider;
        [SerializeField] private TextMeshProUGUI autoAimStrengthText;
        [SerializeField] private Toggle mouseSmoothingToggle;
        [SerializeField] private Slider inputBufferSlider;
        [SerializeField] private TextMeshProUGUI inputBufferText;
        
        [Header("Key Binding Dialog")]
        [SerializeField] private GameObject keyBindingDialog;
        [SerializeField] private TextMeshProUGUI bindingActionText;
        [SerializeField] private TextMeshProUGUI bindingInstructionText;
        [SerializeField] private Button cancelBindingButton;
        [SerializeField] private TextMeshProUGUI detectedInputText;
        
        [Header("Presets")]
        [SerializeField] private Button defaultPresetButton;
        [SerializeField] private Button fpsPresetButton;
        [SerializeField] private Button strategyPresetButton;
        [SerializeField] private Button casualPresetButton;
        [SerializeField] private Button customPresetButton;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem keyBindingEffect;
        [SerializeField] private GameObject inputFeedbackEffect;
        
        [Header("Audio")]
        [SerializeField] private AudioClip keyBindSound;
        [SerializeField] private AudioClip inputTestSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip errorSound;
        
        // Controls data
        private ControlsSettings _currentControlsSettings;
        private bool _isApplyingSettings = false;
        private bool _isWaitingForKeyBind = false;
        
        // Key bindings
        private Dictionary<InputAction, KeyBinding> _keyBindings = new Dictionary<InputAction, KeyBinding>();
        private List<KeyBindingItemUI> _keyBindingItems = new List<KeyBindingItemUI>();
        private InputAction _currentBindingAction;
        
        // Input detection
        private bool _inputTestActive = false;
        private Vector2 _lastMousePosition;
        private Vector2 _currentGamepadInput;
        
        // Default key bindings
        private readonly Dictionary<InputAction, KeyCode[]> _defaultKeyBindings = new Dictionary<InputAction, KeyCode[]>
        {
            { InputAction.MoveForward, new[] { KeyCode.W } },
            { InputAction.MoveBackward, new[] { KeyCode.S } },
            { InputAction.MoveLeft, new[] { KeyCode.A } },
            { InputAction.MoveRight, new[] { KeyCode.D } },
            { InputAction.Jump, new[] { KeyCode.Space } },
            { InputAction.Run, new[] { KeyCode.LeftShift } },
            { InputAction.Crouch, new[] { KeyCode.LeftControl } },
            { InputAction.Interact, new[] { KeyCode.E } },
            { InputAction.Attack, new[] { KeyCode.Mouse0 } },
            { InputAction.Block, new[] { KeyCode.Mouse1 } },
            { InputAction.Inventory, new[] { KeyCode.Tab } },
            { InputAction.Map, new[] { KeyCode.M } },
            { InputAction.Pause, new[] { KeyCode.Escape } },
            { InputAction.Chat, new[] { KeyCode.Return } },
            { InputAction.QuickSlot1, new[] { KeyCode.Alpha1 } },
            { InputAction.QuickSlot2, new[] { KeyCode.Alpha2 } },
            { InputAction.QuickSlot3, new[] { KeyCode.Alpha3 } },
            { InputAction.QuickSlot4, new[] { KeyCode.Alpha4 } },
            { InputAction.QuickSlot5, new[] { KeyCode.Alpha5 } }
        };

        protected override void Awake()
        {
            base.Awake();
            InitializeControlsSettings();
        }

        protected override void Start()
        {
            base.Start();
            SetupControlsPanel();
            LoadControlsSettings();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Start hidden
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                HandleKeyBindingInput();
                UpdateInputTesting();
                UpdateGamepadInput();
            }
        }

        private void InitializeControlsSettings()
        {
            // Initialize current settings
            _currentControlsSettings = new ControlsSettings();
            
            // Initialize key bindings with defaults
            InitializeDefaultKeyBindings();
            
            // Create default key binding item prefab if none exists
            if (keyBindingItemPrefab == null)
            {
                CreateDefaultKeyBindingItemPrefab();
            }
        }

        private void SetupControlsPanel()
        {
            // Setup input method controls
            SetupInputMethodControls();
            
            // Setup mouse controls
            SetupMouseControls();
            
            // Setup gamepad controls
            SetupGamepadControls();
            
            // Setup accessibility controls
            SetupAccessibilityControls();
            
            // Setup advanced controls
            SetupAdvancedControls();
            
            // Setup key binding controls
            SetupKeyBindingControls();
            
            // Setup preset buttons
            SetupPresetButtons();
            
            // Setup input testing
            SetupInputTesting();
            
            // Create key binding items
            CreateKeyBindingItems();
            
            // Initialize dialog
            HideKeyBindingDialog();
        }

        private void SetupInputMethodControls()
        {
            if (inputMethodDropdown != null)
            {
                inputMethodDropdown.ClearOptions();
                inputMethodDropdown.AddOptions(new List<string> { "Keyboard & Mouse", "Gamepad", "Touch", "Auto-Detect" });
                inputMethodDropdown.onValueChanged.AddListener(OnInputMethodChanged);
            }
            
            if (keyboardMouseToggle != null)
                keyboardMouseToggle.onValueChanged.AddListener(OnKeyboardMouseToggleChanged);
            
            if (gamepadToggle != null)
                gamepadToggle.onValueChanged.AddListener(OnGamepadToggleChanged);
            
            if (touchControlsToggle != null)
                touchControlsToggle.onValueChanged.AddListener(OnTouchControlsToggleChanged);
        }

        private void SetupMouseControls()
        {
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.minValue = 0.1f;
                mouseSensitivitySlider.maxValue = 5f;
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            }
            
            if (invertMouseXToggle != null)
                invertMouseXToggle.onValueChanged.AddListener(OnInvertMouseXChanged);
            
            if (invertMouseYToggle != null)
                invertMouseYToggle.onValueChanged.AddListener(OnInvertMouseYChanged);
            
            if (rawInputToggle != null)
                rawInputToggle.onValueChanged.AddListener(OnRawInputChanged);
            
            if (mouseAccelerationSlider != null)
            {
                mouseAccelerationSlider.minValue = 0f;
                mouseAccelerationSlider.maxValue = 2f;
                mouseAccelerationSlider.onValueChanged.AddListener(OnMouseAccelerationChanged);
            }
            
            if (mouseSmoothingToggle != null)
                mouseSmoothingToggle.onValueChanged.AddListener(OnMouseSmoothingChanged);
        }

        private void SetupGamepadControls()
        {
            if (gamepadSensitivitySlider != null)
            {
                gamepadSensitivitySlider.minValue = 0.1f;
                gamepadSensitivitySlider.maxValue = 5f;
                gamepadSensitivitySlider.onValueChanged.AddListener(OnGamepadSensitivityChanged);
            }
            
            if (invertGamepadXToggle != null)
                invertGamepadXToggle.onValueChanged.AddListener(OnInvertGamepadXChanged);
            
            if (invertGamepadYToggle != null)
                invertGamepadYToggle.onValueChanged.AddListener(OnInvertGamepadYChanged);
            
            if (gamepadDeadzoneSlider != null)
            {
                gamepadDeadzoneSlider.minValue = 0f;
                gamepadDeadzoneSlider.maxValue = 0.5f;
                gamepadDeadzoneSlider.onValueChanged.AddListener(OnGamepadDeadzoneChanged);
            }
            
            if (gamepadVibrationToggle != null)
                gamepadVibrationToggle.onValueChanged.AddListener(OnGamepadVibrationChanged);
            
            if (vibrationIntensitySlider != null)
            {
                vibrationIntensitySlider.minValue = 0f;
                vibrationIntensitySlider.maxValue = 1f;
                vibrationIntensitySlider.onValueChanged.AddListener(OnVibrationIntensityChanged);
            }
        }

        private void SetupAccessibilityControls()
        {
            if (holdToToggleToggle != null)
                holdToToggleToggle.onValueChanged.AddListener(OnHoldToToggleChanged);
            
            if (holdDelaySlider != null)
            {
                holdDelaySlider.minValue = 0.1f;
                holdDelaySlider.maxValue = 2f;
                holdDelaySlider.onValueChanged.AddListener(OnHoldDelayChanged);
            }
            
            if (oneHandedModeToggle != null)
                oneHandedModeToggle.onValueChanged.AddListener(OnOneHandedModeChanged);
            
            if (colorBlindAssistToggle != null)
                colorBlindAssistToggle.onValueChanged.AddListener(OnColorBlindAssistChanged);
            
            if (buttonPromptsToggle != null)
                buttonPromptsToggle.onValueChanged.AddListener(OnButtonPromptsChanged);
        }

        private void SetupAdvancedControls()
        {
            if (autoRunToggle != null)
                autoRunToggle.onValueChanged.AddListener(OnAutoRunChanged);
            
            if (autoAimToggle != null)
                autoAimToggle.onValueChanged.AddListener(OnAutoAimChanged);
            
            if (autoAimStrengthSlider != null)
            {
                autoAimStrengthSlider.minValue = 0f;
                autoAimStrengthSlider.maxValue = 1f;
                autoAimStrengthSlider.onValueChanged.AddListener(OnAutoAimStrengthChanged);
            }
            
            if (inputBufferSlider != null)
            {
                inputBufferSlider.minValue = 0f;
                inputBufferSlider.maxValue = 10f;
                inputBufferSlider.onValueChanged.AddListener(OnInputBufferChanged);
            }
        }

        private void SetupKeyBindingControls()
        {
            if (resetKeyBindingsButton != null)
                resetKeyBindingsButton.onClick.AddListener(ResetKeyBindings);
            
            if (importBindingsButton != null)
                importBindingsButton.onClick.AddListener(ImportKeyBindings);
            
            if (exportBindingsButton != null)
                exportBindingsButton.onClick.AddListener(ExportKeyBindings);
            
            if (cancelBindingButton != null)
                cancelBindingButton.onClick.AddListener(CancelKeyBinding);
        }

        private void SetupPresetButtons()
        {
            if (defaultPresetButton != null)
                defaultPresetButton.onClick.AddListener(() => ApplyControlsPreset(ControlsPreset.Default));
            
            if (fpsPresetButton != null)
                fpsPresetButton.onClick.AddListener(() => ApplyControlsPreset(ControlsPreset.FPS));
            
            if (strategyPresetButton != null)
                strategyPresetButton.onClick.AddListener(() => ApplyControlsPreset(ControlsPreset.Strategy));
            
            if (casualPresetButton != null)
                casualPresetButton.onClick.AddListener(() => ApplyControlsPreset(ControlsPreset.Casual));
            
            if (customPresetButton != null)
                customPresetButton.onClick.AddListener(() => ApplyControlsPreset(ControlsPreset.Custom));
        }

        private void SetupInputTesting()
        {
            if (showInputTestButton != null)
                showInputTestButton.onClick.AddListener(ToggleInputTest);
            
            if (enableInputTestToggle != null)
                enableInputTestToggle.onValueChanged.AddListener(OnEnableInputTestChanged);
            
            if (inputTestPanel != null)
                inputTestPanel.SetActive(false);
        }

        private void CreateDefaultKeyBindingItemPrefab()
        {
            var itemObj = new GameObject("KeyBindingItem");
            itemObj.AddComponent<RectTransform>();
            
            // Layout element
            var layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 50f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = itemObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Horizontal layout
            var layout = itemObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 15f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            
            // Action name
            var actionNameObj = new GameObject("ActionName");
            actionNameObj.transform.SetParent(itemObj.transform, false);
            var actionNameText = actionNameObj.AddComponent<TextMeshProUGUI>();
            actionNameText.text = "Action Name";
            actionNameText.fontSize = 14f;
            actionNameText.color = Color.white;
            actionNameText.alignment = TextAlignmentOptions.Left;
            
            var actionNameLayout = actionNameObj.AddComponent<LayoutElement>();
            actionNameLayout.preferredWidth = 150f;
            
            // Primary key button
            var primaryKeyObj = new GameObject("PrimaryKey");
            primaryKeyObj.transform.SetParent(itemObj.transform, false);
            var primaryKeyButton = primaryKeyObj.AddComponent<Button>();
            var primaryKeyText = primaryKeyObj.AddComponent<TextMeshProUGUI>();
            primaryKeyText.text = "Key";
            primaryKeyText.fontSize = 12f;
            primaryKeyText.color = Color.white;
            primaryKeyText.alignment = TextAlignmentOptions.Center;
            
            var primaryKeyLayout = primaryKeyObj.AddComponent<LayoutElement>();
            primaryKeyLayout.preferredWidth = 100f;
            
            // Secondary key button
            var secondaryKeyObj = new GameObject("SecondaryKey");
            secondaryKeyObj.transform.SetParent(itemObj.transform, false);
            var secondaryKeyButton = secondaryKeyObj.AddComponent<Button>();
            var secondaryKeyText = secondaryKeyObj.AddComponent<TextMeshProUGUI>();
            secondaryKeyText.text = "Alt Key";
            secondaryKeyText.fontSize = 12f;
            secondaryKeyText.color = Color.white;
            secondaryKeyText.alignment = TextAlignmentOptions.Center;
            
            var secondaryKeyLayout = secondaryKeyObj.AddComponent<LayoutElement>();
            secondaryKeyLayout.preferredWidth = 100f;
            
            // Reset button
            var resetObj = new GameObject("Reset");
            resetObj.transform.SetParent(itemObj.transform, false);
            var resetButton = resetObj.AddComponent<Button>();
            var resetText = resetObj.AddComponent<TextMeshProUGUI>();
            resetText.text = "Reset";
            resetText.fontSize = 10f;
            resetText.color = Color.red;
            resetText.alignment = TextAlignmentOptions.Center;
            
            var resetLayout = resetObj.AddComponent<LayoutElement>();
            resetLayout.preferredWidth = 60f;
            
            // Add KeyBindingItemUI component
            var keyBindingItemUI = itemObj.AddComponent<KeyBindingItemUI>();
            
            keyBindingItemPrefab = itemObj;
            keyBindingItemPrefab.SetActive(false);
        }

        private void InitializeDefaultKeyBindings()
        {
            _keyBindings.Clear();
            
            foreach (var kvp in _defaultKeyBindings)
            {
                _keyBindings[kvp.Key] = new KeyBinding
                {
                    action = kvp.Key,
                    primaryKey = kvp.Value.Length > 0 ? kvp.Value[0] : KeyCode.None,
                    secondaryKey = kvp.Value.Length > 1 ? kvp.Value[1] : KeyCode.None
                };
            }
        }

        private void LoadControlsSettings()
        {
            // Load controls settings from PlayerPrefs
            _currentControlsSettings.inputMethod = (InputMethod)PlayerPrefs.GetInt("InputMethod", 0);
            _currentControlsSettings.mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
            _currentControlsSettings.invertMouseX = PlayerPrefs.GetInt("InvertMouseX", 0) == 1;
            _currentControlsSettings.invertMouseY = PlayerPrefs.GetInt("InvertMouseY", 0) == 1;
            _currentControlsSettings.rawInput = PlayerPrefs.GetInt("RawInput", 1) == 1;
            _currentControlsSettings.mouseAcceleration = PlayerPrefs.GetFloat("MouseAcceleration", 1f);
            _currentControlsSettings.mouseSmoothing = PlayerPrefs.GetInt("MouseSmoothing", 0) == 1;
            
            _currentControlsSettings.gamepadSensitivity = PlayerPrefs.GetFloat("GamepadSensitivity", 1f);
            _currentControlsSettings.invertGamepadX = PlayerPrefs.GetInt("InvertGamepadX", 0) == 1;
            _currentControlsSettings.invertGamepadY = PlayerPrefs.GetInt("InvertGamepadY", 0) == 1;
            _currentControlsSettings.gamepadDeadzone = PlayerPrefs.GetFloat("GamepadDeadzone", 0.2f);
            _currentControlsSettings.gamepadVibration = PlayerPrefs.GetInt("GamepadVibration", 1) == 1;
            _currentControlsSettings.vibrationIntensity = PlayerPrefs.GetFloat("VibrationIntensity", 1f);
            
            _currentControlsSettings.holdToToggle = PlayerPrefs.GetInt("HoldToToggle", 0) == 1;
            _currentControlsSettings.holdDelay = PlayerPrefs.GetFloat("HoldDelay", 0.5f);
            _currentControlsSettings.oneHandedMode = PlayerPrefs.GetInt("OneHandedMode", 0) == 1;
            _currentControlsSettings.colorBlindAssist = PlayerPrefs.GetInt("ColorBlindAssist", 0) == 1;
            _currentControlsSettings.buttonPrompts = PlayerPrefs.GetInt("ButtonPrompts", 1) == 1;
            
            _currentControlsSettings.autoRun = PlayerPrefs.GetInt("AutoRun", 0) == 1;
            _currentControlsSettings.autoAim = PlayerPrefs.GetInt("AutoAim", 0) == 1;
            _currentControlsSettings.autoAimStrength = PlayerPrefs.GetFloat("AutoAimStrength", 0.5f);
            _currentControlsSettings.inputBuffer = PlayerPrefs.GetFloat("InputBuffer", 3f);
            
            // Load key bindings
            LoadKeyBindings();
            
            ApplySettingsToUI(_currentControlsSettings);
        }

        private void LoadKeyBindings()
        {
            foreach (var action in System.Enum.GetValues(typeof(InputAction)).Cast<InputAction>())
            {
                string primaryKeyName = PlayerPrefs.GetString($"KeyBinding_{action}_Primary", "");
                string secondaryKeyName = PlayerPrefs.GetString($"KeyBinding_{action}_Secondary", "");
                
                KeyCode primaryKey = KeyCode.None;
                KeyCode secondaryKey = KeyCode.None;
                
                if (!string.IsNullOrEmpty(primaryKeyName) && System.Enum.TryParse(primaryKeyName, out primaryKey))
                {
                    // Successfully parsed primary key
                }
                else if (_defaultKeyBindings.ContainsKey(action))
                {
                    primaryKey = _defaultKeyBindings[action][0];
                }
                
                if (!string.IsNullOrEmpty(secondaryKeyName) && System.Enum.TryParse(secondaryKeyName, out secondaryKey))
                {
                    // Successfully parsed secondary key
                }
                else if (_defaultKeyBindings.ContainsKey(action) && _defaultKeyBindings[action].Length > 1)
                {
                    secondaryKey = _defaultKeyBindings[action][1];
                }
                
                _keyBindings[action] = new KeyBinding
                {
                    action = action,
                    primaryKey = primaryKey,
                    secondaryKey = secondaryKey
                };
            }
        }

        private void SaveKeyBindings()
        {
            foreach (var kvp in _keyBindings)
            {
                PlayerPrefs.SetString($"KeyBinding_{kvp.Key}_Primary", kvp.Value.primaryKey.ToString());
                PlayerPrefs.SetString($"KeyBinding_{kvp.Key}_Secondary", kvp.Value.secondaryKey.ToString());
            }
        }

        private void ApplySettingsToUI(ControlsSettings settings)
        {
            _isApplyingSettings = true;
            
            // Input method
            if (inputMethodDropdown != null)
                inputMethodDropdown.value = (int)settings.inputMethod;
            
            if (keyboardMouseToggle != null)
                keyboardMouseToggle.isOn = settings.inputMethod == InputMethod.KeyboardMouse;
            
            if (gamepadToggle != null)
                gamepadToggle.isOn = settings.inputMethod == InputMethod.Gamepad;
            
            if (touchControlsToggle != null)
                touchControlsToggle.isOn = settings.inputMethod == InputMethod.Touch;
            
            // Mouse settings
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.value = settings.mouseSensitivity;
                UpdateMouseSensitivityText(settings.mouseSensitivity);
            }
            
            if (invertMouseXToggle != null)
                invertMouseXToggle.isOn = settings.invertMouseX;
            
            if (invertMouseYToggle != null)
                invertMouseYToggle.isOn = settings.invertMouseY;
            
            if (rawInputToggle != null)
                rawInputToggle.isOn = settings.rawInput;
            
            if (mouseAccelerationSlider != null)
            {
                mouseAccelerationSlider.value = settings.mouseAcceleration;
                UpdateMouseAccelerationText(settings.mouseAcceleration);
            }
            
            if (mouseSmoothingToggle != null)
                mouseSmoothingToggle.isOn = settings.mouseSmoothing;
            
            // Gamepad settings
            if (gamepadSensitivitySlider != null)
            {
                gamepadSensitivitySlider.value = settings.gamepadSensitivity;
                UpdateGamepadSensitivityText(settings.gamepadSensitivity);
            }
            
            if (invertGamepadXToggle != null)
                invertGamepadXToggle.isOn = settings.invertGamepadX;
            
            if (invertGamepadYToggle != null)
                invertGamepadYToggle.isOn = settings.invertGamepadY;
            
            if (gamepadDeadzoneSlider != null)
            {
                gamepadDeadzoneSlider.value = settings.gamepadDeadzone;
                UpdateGamepadDeadzoneText(settings.gamepadDeadzone);
            }
            
            if (gamepadVibrationToggle != null)
                gamepadVibrationToggle.isOn = settings.gamepadVibration;
            
            if (vibrationIntensitySlider != null)
            {
                vibrationIntensitySlider.value = settings.vibrationIntensity;
                UpdateVibrationIntensityText(settings.vibrationIntensity);
            }
            
            // Accessibility settings
            if (holdToToggleToggle != null)
                holdToToggleToggle.isOn = settings.holdToToggle;
            
            if (holdDelaySlider != null)
            {
                holdDelaySlider.value = settings.holdDelay;
                UpdateHoldDelayText(settings.holdDelay);
            }
            
            if (oneHandedModeToggle != null)
                oneHandedModeToggle.isOn = settings.oneHandedMode;
            
            if (colorBlindAssistToggle != null)
                colorBlindAssistToggle.isOn = settings.colorBlindAssist;
            
            if (buttonPromptsToggle != null)
                buttonPromptsToggle.isOn = settings.buttonPrompts;
            
            // Advanced settings
            if (autoRunToggle != null)
                autoRunToggle.isOn = settings.autoRun;
            
            if (autoAimToggle != null)
                autoAimToggle.isOn = settings.autoAim;
            
            if (autoAimStrengthSlider != null)
            {
                autoAimStrengthSlider.value = settings.autoAimStrength;
                UpdateAutoAimStrengthText(settings.autoAimStrength);
            }
            
            if (inputBufferSlider != null)
            {
                inputBufferSlider.value = settings.inputBuffer;
                UpdateInputBufferText(settings.inputBuffer);
            }
            
            _isApplyingSettings = false;
        }

        // Event handlers
        private void OnInputMethodChanged(int method)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.inputMethod = (InputMethod)method;
            NotifySettingsChanged();
        }

        private void OnKeyboardMouseToggleChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            if (enabled)
            {
                _currentControlsSettings.inputMethod = InputMethod.KeyboardMouse;
                if (inputMethodDropdown != null)
                    inputMethodDropdown.value = 0;
                NotifySettingsChanged();
            }
        }

        private void OnGamepadToggleChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            if (enabled)
            {
                _currentControlsSettings.inputMethod = InputMethod.Gamepad;
                if (inputMethodDropdown != null)
                    inputMethodDropdown.value = 1;
                NotifySettingsChanged();
            }
        }

        private void OnTouchControlsToggleChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            if (enabled)
            {
                _currentControlsSettings.inputMethod = InputMethod.Touch;
                if (inputMethodDropdown != null)
                    inputMethodDropdown.value = 2;
                NotifySettingsChanged();
            }
        }

        private void OnMouseSensitivityChanged(float sensitivity)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.mouseSensitivity = sensitivity;
            UpdateMouseSensitivityText(sensitivity);
            NotifySettingsChanged();
        }

        private void OnInvertMouseXChanged(bool invert)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.invertMouseX = invert;
            NotifySettingsChanged();
        }

        private void OnInvertMouseYChanged(bool invert)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.invertMouseY = invert;
            NotifySettingsChanged();
        }

        private void OnRawInputChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.rawInput = enabled;
            NotifySettingsChanged();
        }

        private void OnMouseAccelerationChanged(float acceleration)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.mouseAcceleration = acceleration;
            UpdateMouseAccelerationText(acceleration);
            NotifySettingsChanged();
        }

        private void OnMouseSmoothingChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.mouseSmoothing = enabled;
            NotifySettingsChanged();
        }

        private void OnGamepadSensitivityChanged(float sensitivity)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.gamepadSensitivity = sensitivity;
            UpdateGamepadSensitivityText(sensitivity);
            NotifySettingsChanged();
        }

        private void OnInvertGamepadXChanged(bool invert)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.invertGamepadX = invert;
            NotifySettingsChanged();
        }

        private void OnInvertGamepadYChanged(bool invert)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.invertGamepadY = invert;
            NotifySettingsChanged();
        }

        private void OnGamepadDeadzoneChanged(float deadzone)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.gamepadDeadzone = deadzone;
            UpdateGamepadDeadzoneText(deadzone);
            NotifySettingsChanged();
        }

        private void OnGamepadVibrationChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.gamepadVibration = enabled;
            NotifySettingsChanged();
        }

        private void OnVibrationIntensityChanged(float intensity)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.vibrationIntensity = intensity;
            UpdateVibrationIntensityText(intensity);
            NotifySettingsChanged();
        }

        private void OnHoldToToggleChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.holdToToggle = enabled;
            NotifySettingsChanged();
        }

        private void OnHoldDelayChanged(float delay)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.holdDelay = delay;
            UpdateHoldDelayText(delay);
            NotifySettingsChanged();
        }

        private void OnOneHandedModeChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.oneHandedMode = enabled;
            NotifySettingsChanged();
        }

        private void OnColorBlindAssistChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.colorBlindAssist = enabled;
            NotifySettingsChanged();
        }

        private void OnButtonPromptsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.buttonPrompts = enabled;
            NotifySettingsChanged();
        }

        private void OnAutoRunChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.autoRun = enabled;
            NotifySettingsChanged();
        }

        private void OnAutoAimChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.autoAim = enabled;
            NotifySettingsChanged();
        }

        private void OnAutoAimStrengthChanged(float strength)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.autoAimStrength = strength;
            UpdateAutoAimStrengthText(strength);
            NotifySettingsChanged();
        }

        private void OnInputBufferChanged(float buffer)
        {
            if (_isApplyingSettings) return;
            
            _currentControlsSettings.inputBuffer = buffer;
            UpdateInputBufferText(buffer);
            NotifySettingsChanged();
        }

        private void OnEnableInputTestChanged(bool enabled)
        {
            _inputTestActive = enabled;
        }

        // Text updates
        private void UpdateMouseSensitivityText(float sensitivity)
        {
            if (mouseSensitivityText != null)
                mouseSensitivityText.text = $"{sensitivity:F2}";
        }

        private void UpdateMouseAccelerationText(float acceleration)
        {
            if (mouseAccelerationText != null)
                mouseAccelerationText.text = $"{acceleration:F2}x";
        }

        private void UpdateGamepadSensitivityText(float sensitivity)
        {
            if (gamepadSensitivityText != null)
                gamepadSensitivityText.text = $"{sensitivity:F2}";
        }

        private void UpdateGamepadDeadzoneText(float deadzone)
        {
            if (gamepadDeadzoneText != null)
                gamepadDeadzoneText.text = $"{deadzone:F2}";
        }

        private void UpdateVibrationIntensityText(float intensity)
        {
            if (vibrationIntensityText != null)
                vibrationIntensityText.text = $"{intensity * 100f:F0}%";
        }

        private void UpdateHoldDelayText(float delay)
        {
            if (holdDelayText != null)
                holdDelayText.text = $"{delay:F1}s";
        }

        private void UpdateAutoAimStrengthText(float strength)
        {
            if (autoAimStrengthText != null)
                autoAimStrengthText.text = $"{strength * 100f:F0}%";
        }

        private void UpdateInputBufferText(float buffer)
        {
            if (inputBufferText != null)
                inputBufferText.text = $"{buffer:F0} frames";
        }

        // Key binding management
        private void CreateKeyBindingItems()
        {
            if (keyBindingsContainer == null) return;
            
            _keyBindingItems.Clear();
            
            foreach (var action in System.Enum.GetValues(typeof(InputAction)).Cast<InputAction>())
            {
                CreateKeyBindingItem(action);
            }
        }

        private void CreateKeyBindingItem(InputAction action)
        {
            if (keyBindingItemPrefab == null || keyBindingsContainer == null) return;
            
            var itemObj = Instantiate(keyBindingItemPrefab, keyBindingsContainer);
            itemObj.SetActive(true);
            
            var keyBindingItem = itemObj.GetComponent<KeyBindingItemUI>();
            if (keyBindingItem != null)
            {
                keyBindingItem.Initialize(action, _keyBindings[action], this);
                _keyBindingItems.Add(keyBindingItem);
            }
        }

        public void StartKeyBinding(InputAction action, bool isPrimary)
        {
            _currentBindingAction = action;
            _isWaitingForKeyBind = true;
            
            ShowKeyBindingDialog(action, isPrimary);
            PlayKeyBindSound();
        }

        private void ShowKeyBindingDialog(InputAction action, bool isPrimary)
        {
            if (keyBindingDialog == null) return;
            
            keyBindingDialog.SetActive(true);
            
            if (bindingActionText != null)
                bindingActionText.text = $"Binding: {GetActionDisplayName(action)}";
            
            if (bindingInstructionText != null)
                bindingInstructionText.text = isPrimary ? "Press a key for primary binding..." : "Press a key for secondary binding...";
            
            if (detectedInputText != null)
                detectedInputText.text = "Waiting for input...";
        }

        private void HideKeyBindingDialog()
        {
            if (keyBindingDialog != null)
            {
                keyBindingDialog.SetActive(false);
            }
            
            _isWaitingForKeyBind = false;
            _currentBindingAction = InputAction.None;
        }

        private void HandleKeyBindingInput()
        {
            if (!_isWaitingForKeyBind) return;
            
            // Check for key input
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    if (key == KeyCode.Escape)
                    {
                        CancelKeyBinding();
                        return;
                    }
                    
                    ApplyKeyBinding(_currentBindingAction, key);
                    return;
                }
            }
            
            // Update detected input display
            UpdateDetectedInputDisplay();
        }

        private void UpdateDetectedInputDisplay()
        {
            if (detectedInputText == null) return;
            
            string inputText = "Waiting for input...";
            
            // Check for any key being held
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(key))
                {
                    inputText = $"Detected: {GetKeyDisplayName(key)}";
                    break;
                }
            }
            
            detectedInputText.text = inputText;
        }

        private void ApplyKeyBinding(InputAction action, KeyCode key)
        {
            if (!_keyBindings.ContainsKey(action)) return;
            
            var binding = _keyBindings[action];
            
            // Determine if this is primary or secondary based on current state
            bool isPrimary = binding.primaryKey == KeyCode.None || 
                           (binding.primaryKey != KeyCode.None && binding.secondaryKey == KeyCode.None);
            
            if (isPrimary)
            {
                binding.primaryKey = key;
            }
            else
            {
                binding.secondaryKey = key;
            }
            
            // Update UI
            var keyBindingItem = _keyBindingItems.FirstOrDefault(item => item.Action == action);
            keyBindingItem?.UpdateDisplay(binding);
            
            // Save settings
            SaveKeyBindings();
            NotifySettingsChanged();
            
            // Show effect
            if (keyBindingEffect != null)
                keyBindingEffect.Play();
            
            HideKeyBindingDialog();
        }

        private void CancelKeyBinding()
        {
            PlayButtonClickSound();
            HideKeyBindingDialog();
        }

        public void ResetKeyBinding(InputAction action)
        {
            if (_defaultKeyBindings.ContainsKey(action))
            {
                var defaultKeys = _defaultKeyBindings[action];
                _keyBindings[action] = new KeyBinding
                {
                    action = action,
                    primaryKey = defaultKeys.Length > 0 ? defaultKeys[0] : KeyCode.None,
                    secondaryKey = defaultKeys.Length > 1 ? defaultKeys[1] : KeyCode.None
                };
                
                // Update UI
                var keyBindingItem = _keyBindingItems.FirstOrDefault(item => item.Action == action);
                keyBindingItem?.UpdateDisplay(_keyBindings[action]);
                
                SaveKeyBindings();
                NotifySettingsChanged();
            }
        }

        private void ResetKeyBindings()
        {
            PlayButtonClickSound();
            
            InitializeDefaultKeyBindings();
            
            // Update all UI items
            foreach (var item in _keyBindingItems)
            {
                if (_keyBindings.ContainsKey(item.Action))
                {
                    item.UpdateDisplay(_keyBindings[item.Action]);
                }
            }
            
            SaveKeyBindings();
            NotifySettingsChanged();
        }

        private void ImportKeyBindings()
        {
            PlayButtonClickSound();
            Debug.Log("Key binding import functionality would be implemented here");
        }

        private void ExportKeyBindings()
        {
            PlayButtonClickSound();
            Debug.Log("Key binding export functionality would be implemented here");
        }

        // Input testing
        private void ToggleInputTest()
        {
            if (inputTestPanel != null)
            {
                bool isActive = inputTestPanel.activeInHierarchy;
                inputTestPanel.SetActive(!isActive);
                
                if (!isActive)
                {
                    PlayInputTestSound();
                }
            }
        }

        private void UpdateInputTesting()
        {
            if (!_inputTestActive || inputTestPanel == null || !inputTestPanel.activeInHierarchy) return;
            
            string inputInfo = "Input Test:\n";
            
            // Mouse input
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseDelta = mousePos - _lastMousePosition;
            inputInfo += $"Mouse: ({mousePos.x:F0}, {mousePos.y:F0}) Δ({mouseDelta.x:F1}, {mouseDelta.y:F1})\n";
            _lastMousePosition = mousePos;
            
            // Keyboard input
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(key))
                {
                    inputInfo += $"Key Held: {GetKeyDisplayName(key)}\n";
                }
            }
            
            // Gamepad input
            if (_currentGamepadInput.magnitude > 0.1f)
            {
                inputInfo += $"Gamepad: ({_currentGamepadInput.x:F2}, {_currentGamepadInput.y:F2})\n";
            }
            
            if (currentInputText != null)
                currentInputText.text = inputInfo;
        }

        private void UpdateGamepadInput()
        {
            // Update gamepad input for testing
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            _currentGamepadInput = new Vector2(horizontal, vertical);
            
            // Apply deadzone
            if (_currentGamepadInput.magnitude < _currentControlsSettings.gamepadDeadzone)
            {
                _currentGamepadInput = Vector2.zero;
            }
        }

        // Preset management
        private void ApplyControlsPreset(ControlsPreset preset)
        {
            PlayButtonClickSound();
            
            switch (preset)
            {
                case ControlsPreset.Default:
                    ApplyDefaultPreset();
                    break;
                case ControlsPreset.FPS:
                    ApplyFPSPreset();
                    break;
                case ControlsPreset.Strategy:
                    ApplyStrategyPreset();
                    break;
                case ControlsPreset.Casual:
                    ApplyCasualPreset();
                    break;
            }
            
            ApplySettingsToUI(_currentControlsSettings);
            NotifySettingsChanged();
        }

        private void ApplyDefaultPreset()
        {
            _currentControlsSettings.mouseSensitivity = 1f;
            _currentControlsSettings.gamepadSensitivity = 1f;
            _currentControlsSettings.autoAim = false;
            _currentControlsSettings.autoRun = false;
        }

        private void ApplyFPSPreset()
        {
            _currentControlsSettings.mouseSensitivity = 1.5f;
            _currentControlsSettings.gamepadSensitivity = 1.2f;
            _currentControlsSettings.autoAim = false;
            _currentControlsSettings.autoRun = true;
            _currentControlsSettings.rawInput = true;
        }

        private void ApplyStrategyPreset()
        {
            _currentControlsSettings.mouseSensitivity = 0.8f;
            _currentControlsSettings.gamepadSensitivity = 0.6f;
            _currentControlsSettings.autoAim = false;
            _currentControlsSettings.autoRun = false;
        }

        private void ApplyCasualPreset()
        {
            _currentControlsSettings.mouseSensitivity = 0.7f;
            _currentControlsSettings.gamepadSensitivity = 0.8f;
            _currentControlsSettings.autoAim = true;
            _currentControlsSettings.autoRun = true;
            _currentControlsSettings.autoAimStrength = 0.3f;
        }

        // Utility methods
        public string GetActionDisplayName(InputAction action)
        {
            return action switch
            {
                InputAction.MoveForward => "Move Forward",
                InputAction.MoveBackward => "Move Backward",
                InputAction.MoveLeft => "Move Left",
                InputAction.MoveRight => "Move Right",
                InputAction.Jump => "Jump",
                InputAction.Run => "Run",
                InputAction.Crouch => "Crouch",
                InputAction.Interact => "Interact",
                InputAction.Attack => "Attack",
                InputAction.Block => "Block",
                InputAction.Inventory => "Inventory",
                InputAction.Map => "Map",
                InputAction.Pause => "Pause",
                InputAction.Chat => "Chat",
                InputAction.QuickSlot1 => "Quick Slot 1",
                InputAction.QuickSlot2 => "Quick Slot 2",
                InputAction.QuickSlot3 => "Quick Slot 3",
                InputAction.QuickSlot4 => "Quick Slot 4",
                InputAction.QuickSlot5 => "Quick Slot 5",
                _ => action.ToString()
            };
        }

        private string GetKeyDisplayName(KeyCode key)
        {
            return key switch
            {
                KeyCode.LeftShift => "L Shift",
                KeyCode.RightShift => "R Shift",
                KeyCode.LeftControl => "L Ctrl",
                KeyCode.RightControl => "R Ctrl",
                KeyCode.LeftAlt => "L Alt",
                KeyCode.RightAlt => "R Alt",
                KeyCode.Mouse0 => "LMB",
                KeyCode.Mouse1 => "RMB",
                KeyCode.Mouse2 => "MMB",
                KeyCode.Mouse3 => "Mouse 3",
                KeyCode.Mouse4 => "Mouse 4",
                KeyCode.Mouse5 => "Mouse 5",
                KeyCode.Mouse6 => "Mouse 6",
                _ => key.ToString()
            };
        }

        // Settings interface
        public void ApplySettings(SettingsData settings)
        {
            _currentControlsSettings.mouseSensitivity = settings.mouseSensitivity;
            _currentControlsSettings.invertMouseY = settings.invertYAxis;
            _currentControlsSettings.autoRun = settings.autoRun;
            
            ApplySettingsToUI(_currentControlsSettings);
        }

        public void CollectSettings(ref SettingsData settings)
        {
            settings.mouseSensitivity = _currentControlsSettings.mouseSensitivity;
            settings.invertYAxis = _currentControlsSettings.invertMouseY;
            settings.autoRun = _currentControlsSettings.autoRun;
        }

        private void NotifySettingsChanged()
        {
            var settingsPanel = GetComponentInParent<SettingsPanel>();
            settingsPanel?.OnSettingsChanged();
        }

        // Audio methods
        private void PlayKeyBindSound()
        {
            Debug.Log("Key bind sound would play here");
        }

        private void PlayInputTestSound()
        {
            Debug.Log("Input test sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        // Public interface
        public ControlsSettings CurrentControlsSettings => _currentControlsSettings;
        public Dictionary<InputAction, KeyBinding> KeyBindings => _keyBindings;
    }

    // Helper component for key binding items
    public class KeyBindingItemUI : MonoBehaviour
    {
        private InputAction _action;
        private KeyBinding _keyBinding;
        private ControlsSettingsPanel _controlsPanel;
        
        // UI components
        private TextMeshProUGUI _actionNameText;
        private Button _primaryKeyButton;
        private Button _secondaryKeyButton;
        private Button _resetButton;
        private TextMeshProUGUI _primaryKeyText;
        private TextMeshProUGUI _secondaryKeyText;

        public void Initialize(InputAction action, KeyBinding keyBinding, ControlsSettingsPanel controlsPanel)
        {
            _action = action;
            _keyBinding = keyBinding;
            _controlsPanel = controlsPanel;
            
            // Find UI components
            var texts = GetComponentsInChildren<TextMeshProUGUI>();
            var buttons = GetComponentsInChildren<Button>();
            
            if (texts.Length >= 3)
            {
                _actionNameText = texts[0];
                _primaryKeyText = texts[1];
                _secondaryKeyText = texts[2];
            }
            
            if (buttons.Length >= 3)
            {
                _primaryKeyButton = buttons[0];
                _secondaryKeyButton = buttons[1];
                _resetButton = buttons[2];
            }
            
            // Setup button events
            if (_primaryKeyButton != null)
                _primaryKeyButton.onClick.AddListener(() => _controlsPanel.StartKeyBinding(_action, true));
            
            if (_secondaryKeyButton != null)
                _secondaryKeyButton.onClick.AddListener(() => _controlsPanel.StartKeyBinding(_action, false));
            
            if (_resetButton != null)
                _resetButton.onClick.AddListener(() => _controlsPanel.ResetKeyBinding(_action));
            
            UpdateDisplay(keyBinding);
        }

        public void UpdateDisplay(KeyBinding keyBinding)
        {
            _keyBinding = keyBinding;
            
            if (_actionNameText != null)
                _actionNameText.text = GetActionDisplayName(_action);
            
            if (_primaryKeyText != null)
                _primaryKeyText.text = GetKeyDisplayName(_keyBinding.primaryKey);
            
            if (_secondaryKeyText != null)
                _secondaryKeyText.text = GetKeyDisplayName(_keyBinding.secondaryKey);
        }

        private string GetActionDisplayName(InputAction action)
        {
            // Use the same logic as the main panel
            return _controlsPanel?.GetActionDisplayName(action) ?? action.ToString();
        }

        private string GetKeyDisplayName(KeyCode key)
        {
            if (key == KeyCode.None) return "None";
            
            return key switch
            {
                KeyCode.LeftShift => "L Shift",
                KeyCode.RightShift => "R Shift",
                KeyCode.LeftControl => "L Ctrl",
                KeyCode.RightControl => "R Ctrl",
                KeyCode.LeftAlt => "L Alt",
                KeyCode.RightAlt => "R Alt",
                KeyCode.Mouse0 => "LMB",
                KeyCode.Mouse1 => "RMB",
                KeyCode.Mouse2 => "MMB",
                _ => key.ToString()
            };
        }

        public InputAction Action => _action;
    }

    // Data structures and enums
    [System.Serializable]
    public class ControlsSettings
    {
        [Header("Input Method")]
        public InputMethod inputMethod = InputMethod.KeyboardMouse;
        
        [Header("Mouse")]
        public float mouseSensitivity = 1f;
        public bool invertMouseX = false;
        public bool invertMouseY = false;
        public bool rawInput = true;
        public float mouseAcceleration = 1f;
        public bool mouseSmoothing = false;
        
        [Header("Gamepad")]
        public float gamepadSensitivity = 1f;
        public bool invertGamepadX = false;
        public bool invertGamepadY = false;
        public float gamepadDeadzone = 0.2f;
        public bool gamepadVibration = true;
        public float vibrationIntensity = 1f;
        
        [Header("Accessibility")]
        public bool holdToToggle = false;
        public float holdDelay = 0.5f;
        public bool oneHandedMode = false;
        public bool colorBlindAssist = false;
        public bool buttonPrompts = true;
        
        [Header("Advanced")]
        public bool autoRun = false;
        public bool autoAim = false;
        public float autoAimStrength = 0.5f;
        public float inputBuffer = 3f;
    }

    [System.Serializable]
    public class KeyBinding
    {
        public InputAction action;
        public KeyCode primaryKey;
        public KeyCode secondaryKey;
    }

    public enum InputMethod
    {
        KeyboardMouse,
        Gamepad,
        Touch,
        AutoDetect
    }

    public enum InputAction
    {
        None,
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        Jump,
        Run,
        Crouch,
        Interact,
        Attack,
        Block,
        Inventory,
        Map,
        Pause,
        Chat,
        QuickSlot1,
        QuickSlot2,
        QuickSlot3,
        QuickSlot4,
        QuickSlot5
    }

    public enum ControlsPreset
    {
        Default,
        FPS,
        Strategy,
        Casual,
        Custom
    }
}