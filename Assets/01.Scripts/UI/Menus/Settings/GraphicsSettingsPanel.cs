using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.Settings
{
    public class GraphicsSettingsPanel : UIPanel
    {
        [Header("Display Settings")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown fullscreenModeDropdown;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Slider targetFrameRateSlider;
        [SerializeField] private TextMeshProUGUI targetFrameRateText;
        
        [Header("Quality Settings")]
        [SerializeField] private TMP_Dropdown qualityPresetDropdown;
        [SerializeField] private Slider renderScaleSlider;
        [SerializeField] private TextMeshProUGUI renderScaleText;
        [SerializeField] private TMP_Dropdown shadowQualityDropdown;
        [SerializeField] private TMP_Dropdown textureQualityDropdown;
        [SerializeField] private TMP_Dropdown antiAliasingDropdown;
        [SerializeField] private TMP_Dropdown anisotropicFilteringDropdown;
        
        [Header("Advanced Settings")]
        [SerializeField] private Toggle realtimeReflectionsToggle;
        [SerializeField] private Toggle screenSpaceAmbientOcclusionToggle;
        [SerializeField] private Toggle bloomToggle;
        [SerializeField] private Toggle motionBlurToggle;
        [SerializeField] private Toggle depthOfFieldToggle;
        [SerializeField] private Slider lodBiasSlider;
        [SerializeField] private TextMeshProUGUI lodBiasText;
        
        [Header("Performance Monitoring")]
        [SerializeField] private GameObject performancePanel;
        [SerializeField] private TextMeshProUGUI fpsCounter;
        [SerializeField] private TextMeshProUGUI frameTimeText;
        [SerializeField] private TextMeshProUGUI memoryUsageText;
        [SerializeField] private Toggle showPerformanceToggle;
        [SerializeField] private Button benchmarkButton;
        
        [Header("Presets")]
        [SerializeField] private Button lowPresetButton;
        [SerializeField] private Button mediumPresetButton;
        [SerializeField] private Button highPresetButton;
        [SerializeField] private Button ultraPresetButton;
        [SerializeField] private Button customPresetButton;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem qualityChangeEffect;
        [SerializeField] private GameObject loadingOverlay;
        
        [Header("Audio")]
        [SerializeField] private AudioClip qualityChangeSound;
        [SerializeField] private AudioClip presetChangeSound;
        
        // Graphics data
        private Resolution[] _availableResolutions;
        private GraphicsSettings _currentGraphicsSettings;
        private bool _isApplyingSettings = false;
        
        // Performance monitoring
        private float _frameTimeAccumulator = 0f;
        private int _frameCount = 0;
        private float _updateInterval = 1f;
        private float _lastUpdateTime = 0f;
        
        // Preset definitions
        private readonly Dictionary<QualityPreset, GraphicsSettings> _qualityPresets = new Dictionary<QualityPreset, GraphicsSettings>();
        
        // Available options
        private readonly string[] _shadowQualityOptions = { "Disabled", "Hard Shadows Only", "All", "High Resolution" };
        private readonly string[] _textureQualityOptions = { "Eighth Res", "Quarter Res", "Half Res", "Full Res" };
        private readonly string[] _antiAliasingOptions = { "Disabled", "2x Multi Sampling", "4x Multi Sampling", "8x Multi Sampling" };
        private readonly string[] _anisotropicOptions = { "Disabled", "Per Texture", "Forced On" };
        private readonly string[] _fullscreenModeOptions = { "Exclusive Fullscreen", "Fullscreen Window", "Maximized Window", "Windowed" };

        protected override void Awake()
        {
            base.Awake();
            InitializeGraphicsSettings();
        }

        protected override void Start()
        {
            base.Start();
            SetupGraphicsPanel();
            LoadGraphicsSettings();
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
                UpdatePerformanceMonitoring();
            }
        }

        private void InitializeGraphicsSettings()
        {
            // Get available resolutions
            _availableResolutions = Screen.resolutions.Where(resolution => resolution.refreshRate >= 60).ToArray();
            
            // Initialize quality presets
            InitializeQualityPresets();
            
            // Initialize current settings
            _currentGraphicsSettings = new GraphicsSettings();
        }

        private void SetupGraphicsPanel()
        {
            // Setup resolution dropdown
            SetupResolutionDropdown();
            
            // Setup fullscreen mode dropdown
            SetupFullscreenModeDropdown();
            
            // Setup quality preset dropdown
            SetupQualityPresetDropdown();
            
            // Setup other dropdowns
            SetupShadowQualityDropdown();
            SetupTextureQualityDropdown();
            SetupAntiAliasingDropdown();
            SetupAnisotropicFilteringDropdown();
            
            // Setup sliders
            SetupSliders();
            
            // Setup toggles
            SetupToggles();
            
            // Setup preset buttons
            SetupPresetButtons();
            
            // Setup other buttons
            SetupOtherButtons();
            
            // Initialize performance panel
            SetupPerformancePanel();
        }

        private void SetupResolutionDropdown()
        {
            if (resolutionDropdown == null) return;
            
            resolutionDropdown.ClearOptions();
            
            var resolutionOptions = new List<string>();
            foreach (var resolution in _availableResolutions)
            {
                resolutionOptions.Add($"{resolution.width} x {resolution.height} @ {resolution.refreshRate}Hz");
            }
            
            resolutionDropdown.AddOptions(resolutionOptions);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        private void SetupFullscreenModeDropdown()
        {
            if (fullscreenModeDropdown == null) return;
            
            fullscreenModeDropdown.ClearOptions();
            fullscreenModeDropdown.AddOptions(_fullscreenModeOptions.ToList());
            fullscreenModeDropdown.onValueChanged.AddListener(OnFullscreenModeChanged);
        }

        private void SetupQualityPresetDropdown()
        {
            if (qualityPresetDropdown == null) return;
            
            qualityPresetDropdown.ClearOptions();
            var qualityOptions = QualitySettings.names.ToList();
            qualityOptions.Add("Custom");
            qualityPresetDropdown.AddOptions(qualityOptions);
            qualityPresetDropdown.onValueChanged.AddListener(OnQualityPresetChanged);
        }

        private void SetupShadowQualityDropdown()
        {
            if (shadowQualityDropdown == null) return;
            
            shadowQualityDropdown.ClearOptions();
            shadowQualityDropdown.AddOptions(_shadowQualityOptions.ToList());
            shadowQualityDropdown.onValueChanged.AddListener(OnShadowQualityChanged);
        }

        private void SetupTextureQualityDropdown()
        {
            if (textureQualityDropdown == null) return;
            
            textureQualityDropdown.ClearOptions();
            textureQualityDropdown.AddOptions(_textureQualityOptions.ToList());
            textureQualityDropdown.onValueChanged.AddListener(OnTextureQualityChanged);
        }

        private void SetupAntiAliasingDropdown()
        {
            if (antiAliasingDropdown == null) return;
            
            antiAliasingDropdown.ClearOptions();
            antiAliasingDropdown.AddOptions(_antiAliasingOptions.ToList());
            antiAliasingDropdown.onValueChanged.AddListener(OnAntiAliasingChanged);
        }

        private void SetupAnisotropicFilteringDropdown()
        {
            if (anisotropicFilteringDropdown == null) return;
            
            anisotropicFilteringDropdown.ClearOptions();
            anisotropicFilteringDropdown.AddOptions(_anisotropicOptions.ToList());
            anisotropicFilteringDropdown.onValueChanged.AddListener(OnAnisotropicFilteringChanged);
        }

        private void SetupSliders()
        {
            // Target frame rate slider
            if (targetFrameRateSlider != null)
            {
                targetFrameRateSlider.minValue = 30f;
                targetFrameRateSlider.maxValue = 144f;
                targetFrameRateSlider.wholeNumbers = true;
                targetFrameRateSlider.onValueChanged.AddListener(OnTargetFrameRateChanged);
            }
            
            // Render scale slider
            if (renderScaleSlider != null)
            {
                renderScaleSlider.minValue = 0.5f;
                renderScaleSlider.maxValue = 2f;
                renderScaleSlider.onValueChanged.AddListener(OnRenderScaleChanged);
            }
            
            // LOD bias slider
            if (lodBiasSlider != null)
            {
                lodBiasSlider.minValue = 0.5f;
                lodBiasSlider.maxValue = 2f;
                lodBiasSlider.onValueChanged.AddListener(OnLodBiasChanged);
            }
        }

        private void SetupToggles()
        {
            if (vsyncToggle != null)
                vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            
            if (realtimeReflectionsToggle != null)
                realtimeReflectionsToggle.onValueChanged.AddListener(OnRealtimeReflectionsChanged);
            
            if (screenSpaceAmbientOcclusionToggle != null)
                screenSpaceAmbientOcclusionToggle.onValueChanged.AddListener(OnSSAOChanged);
            
            if (bloomToggle != null)
                bloomToggle.onValueChanged.AddListener(OnBloomChanged);
            
            if (motionBlurToggle != null)
                motionBlurToggle.onValueChanged.AddListener(OnMotionBlurChanged);
            
            if (depthOfFieldToggle != null)
                depthOfFieldToggle.onValueChanged.AddListener(OnDepthOfFieldChanged);
            
            if (showPerformanceToggle != null)
                showPerformanceToggle.onValueChanged.AddListener(OnShowPerformanceChanged);
        }

        private void SetupPresetButtons()
        {
            if (lowPresetButton != null)
                lowPresetButton.onClick.AddListener(() => ApplyQualityPreset(QualityPreset.Low));
            
            if (mediumPresetButton != null)
                mediumPresetButton.onClick.AddListener(() => ApplyQualityPreset(QualityPreset.Medium));
            
            if (highPresetButton != null)
                highPresetButton.onClick.AddListener(() => ApplyQualityPreset(QualityPreset.High));
            
            if (ultraPresetButton != null)
                ultraPresetButton.onClick.AddListener(() => ApplyQualityPreset(QualityPreset.Ultra));
            
            if (customPresetButton != null)
                customPresetButton.onClick.AddListener(() => ApplyQualityPreset(QualityPreset.Custom));
        }

        private void SetupOtherButtons()
        {
            if (benchmarkButton != null)
                benchmarkButton.onClick.AddListener(RunBenchmark);
        }

        private void SetupPerformancePanel()
        {
            if (performancePanel != null)
            {
                performancePanel.SetActive(false);
            }
            
            _lastUpdateTime = Time.unscaledTime;
        }

        private void InitializeQualityPresets()
        {
            // Low preset
            _qualityPresets[QualityPreset.Low] = new GraphicsSettings
            {
                qualityLevel = 0,
                renderScale = 0.75f,
                shadowQuality = ShadowResolution.Low,
                textureQuality = 3,
                antiAliasing = 0,
                anisotropicFiltering = AnisotropicFiltering.Disable,
                realtimeReflections = false,
                ssao = false,
                bloom = false,
                motionBlur = false,
                depthOfField = false,
                lodBias = 0.7f
            };
            
            // Medium preset
            _qualityPresets[QualityPreset.Medium] = new GraphicsSettings
            {
                qualityLevel = 2,
                renderScale = 1f,
                shadowQuality = ShadowResolution.Medium,
                textureQuality = 1,
                antiAliasing = 2,
                anisotropicFiltering = AnisotropicFiltering.Enable,
                realtimeReflections = false,
                ssao = true,
                bloom = true,
                motionBlur = false,
                depthOfField = false,
                lodBias = 1f
            };
            
            // High preset
            _qualityPresets[QualityPreset.High] = new GraphicsSettings
            {
                qualityLevel = 4,
                renderScale = 1f,
                shadowQuality = ShadowResolution.High,
                textureQuality = 0,
                antiAliasing = 4,
                anisotropicFiltering = AnisotropicFiltering.ForceEnable,
                realtimeReflections = true,
                ssao = true,
                bloom = true,
                motionBlur = true,
                depthOfField = true,
                lodBias = 1.5f
            };
            
            // Ultra preset
            _qualityPresets[QualityPreset.Ultra] = new GraphicsSettings
            {
                qualityLevel = 5,
                renderScale = 1.25f,
                shadowQuality = ShadowResolution.VeryHigh,
                textureQuality = 0,
                antiAliasing = 8,
                anisotropicFiltering = AnisotropicFiltering.ForceEnable,
                realtimeReflections = true,
                ssao = true,
                bloom = true,
                motionBlur = true,
                depthOfField = true,
                lodBias = 2f
            };
        }

        private void LoadGraphicsSettings()
        {
            // Load current graphics settings from Unity/PlayerPrefs
            _currentGraphicsSettings.resolution = Screen.currentResolution;
            _currentGraphicsSettings.fullscreenMode = Screen.fullScreenMode;
            _currentGraphicsSettings.vsync = QualitySettings.vSyncCount > 0;
            _currentGraphicsSettings.qualityLevel = QualitySettings.GetQualityLevel();
            _currentGraphicsSettings.targetFrameRate = Application.targetFrameRate == -1 ? 60 : Application.targetFrameRate;
            _currentGraphicsSettings.renderScale = PlayerPrefs.GetFloat("RenderScale", 1f);
            _currentGraphicsSettings.shadowQuality = QualitySettings.shadowResolution;
            _currentGraphicsSettings.textureQuality = QualitySettings.globalTextureMipmapLimit;
            _currentGraphicsSettings.antiAliasing = QualitySettings.antiAliasing;
            _currentGraphicsSettings.anisotropicFiltering = QualitySettings.anisotropicFiltering;
            _currentGraphicsSettings.realtimeReflections = PlayerPrefs.GetInt("RealtimeReflections", 1) == 1;
            _currentGraphicsSettings.ssao = PlayerPrefs.GetInt("SSAO", 1) == 1;
            _currentGraphicsSettings.bloom = PlayerPrefs.GetInt("Bloom", 1) == 1;
            _currentGraphicsSettings.motionBlur = PlayerPrefs.GetInt("MotionBlur", 0) == 1;
            _currentGraphicsSettings.depthOfField = PlayerPrefs.GetInt("DepthOfField", 0) == 1;
            _currentGraphicsSettings.lodBias = QualitySettings.lodBias;
            
            ApplySettingsToUI(_currentGraphicsSettings);
        }

        private void ApplySettingsToUI(GraphicsSettings settings)
        {
            _isApplyingSettings = true;
            
            // Resolution
            if (resolutionDropdown != null)
            {
                int resolutionIndex = System.Array.FindIndex(_availableResolutions, 
                    r => r.width == settings.resolution.width && r.height == settings.resolution.height);
                resolutionDropdown.value = Mathf.Max(0, resolutionIndex);
            }
            
            // Fullscreen mode
            if (fullscreenModeDropdown != null)
                fullscreenModeDropdown.value = (int)settings.fullscreenMode;
            
            // VSync
            if (vsyncToggle != null)
                vsyncToggle.isOn = settings.vsync;
            
            // Quality level
            if (qualityPresetDropdown != null)
                qualityPresetDropdown.value = settings.qualityLevel;
            
            // Target frame rate
            if (targetFrameRateSlider != null)
            {
                targetFrameRateSlider.value = settings.targetFrameRate;
                UpdateTargetFrameRateText(settings.targetFrameRate);
            }
            
            // Render scale
            if (renderScaleSlider != null)
            {
                renderScaleSlider.value = settings.renderScale;
                UpdateRenderScaleText(settings.renderScale);
            }
            
            // Shadow quality
            if (shadowQualityDropdown != null)
                shadowQualityDropdown.value = (int)settings.shadowQuality;
            
            // Texture quality
            if (textureQualityDropdown != null)
                textureQualityDropdown.value = settings.textureQuality;
            
            // Anti-aliasing
            if (antiAliasingDropdown != null)
            {
                int aaIndex = settings.antiAliasing switch
                {
                    0 => 0,
                    2 => 1,
                    4 => 2,
                    8 => 3,
                    _ => 0
                };
                antiAliasingDropdown.value = aaIndex;
            }
            
            // Anisotropic filtering
            if (anisotropicFilteringDropdown != null)
                anisotropicFilteringDropdown.value = (int)settings.anisotropicFiltering;
            
            // Advanced settings
            if (realtimeReflectionsToggle != null)
                realtimeReflectionsToggle.isOn = settings.realtimeReflections;
            
            if (screenSpaceAmbientOcclusionToggle != null)
                screenSpaceAmbientOcclusionToggle.isOn = settings.ssao;
            
            if (bloomToggle != null)
                bloomToggle.isOn = settings.bloom;
            
            if (motionBlurToggle != null)
                motionBlurToggle.isOn = settings.motionBlur;
            
            if (depthOfFieldToggle != null)
                depthOfFieldToggle.isOn = settings.depthOfField;
            
            // LOD bias
            if (lodBiasSlider != null)
            {
                lodBiasSlider.value = settings.lodBias;
                UpdateLodBiasText(settings.lodBias);
            }
            
            _isApplyingSettings = false;
        }

        // Event handlers
        private void OnResolutionChanged(int index)
        {
            if (_isApplyingSettings || index >= _availableResolutions.Length) return;
            
            _currentGraphicsSettings.resolution = _availableResolutions[index];
            NotifySettingsChanged();
        }

        private void OnFullscreenModeChanged(int index)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.fullscreenMode = (FullScreenMode)index;
            NotifySettingsChanged();
        }

        private void OnVSyncChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.vsync = enabled;
            NotifySettingsChanged();
        }

        private void OnQualityPresetChanged(int index)
        {
            if (_isApplyingSettings) return;
            
            if (index < QualitySettings.names.Length)
            {
                _currentGraphicsSettings.qualityLevel = index;
                ApplyUnityQualityLevel(index);
            }
            
            NotifySettingsChanged();
        }

        private void OnTargetFrameRateChanged(float frameRate)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.targetFrameRate = Mathf.RoundToInt(frameRate);
            UpdateTargetFrameRateText(_currentGraphicsSettings.targetFrameRate);
            NotifySettingsChanged();
        }

        private void OnRenderScaleChanged(float scale)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.renderScale = scale;
            UpdateRenderScaleText(scale);
            NotifySettingsChanged();
        }

        private void OnShadowQualityChanged(int index)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.shadowQuality = (ShadowResolution)index;
            NotifySettingsChanged();
        }

        private void OnTextureQualityChanged(int index)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.textureQuality = index;
            NotifySettingsChanged();
        }

        private void OnAntiAliasingChanged(int index)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.antiAliasing = index switch
            {
                0 => 0,
                1 => 2,
                2 => 4,
                3 => 8,
                _ => 0
            };
            NotifySettingsChanged();
        }

        private void OnAnisotropicFilteringChanged(int index)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.anisotropicFiltering = (AnisotropicFiltering)index;
            NotifySettingsChanged();
        }

        private void OnRealtimeReflectionsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.realtimeReflections = enabled;
            NotifySettingsChanged();
        }

        private void OnSSAOChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.ssao = enabled;
            NotifySettingsChanged();
        }

        private void OnBloomChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.bloom = enabled;
            NotifySettingsChanged();
        }

        private void OnMotionBlurChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.motionBlur = enabled;
            NotifySettingsChanged();
        }

        private void OnDepthOfFieldChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.depthOfField = enabled;
            NotifySettingsChanged();
        }

        private void OnLodBiasChanged(float bias)
        {
            if (_isApplyingSettings) return;
            
            _currentGraphicsSettings.lodBias = bias;
            UpdateLodBiasText(bias);
            NotifySettingsChanged();
        }

        private void OnShowPerformanceChanged(bool show)
        {
            if (performancePanel != null)
            {
                performancePanel.SetActive(show);
            }
        }

        // Quality preset management
        private void ApplyQualityPreset(QualityPreset preset)
        {
            PlayPresetChangeSound();
            
            if (_qualityPresets.ContainsKey(preset))
            {
                var presetSettings = _qualityPresets[preset];
                
                // Copy preset settings to current settings (preserving resolution and fullscreen mode)
                var resolution = _currentGraphicsSettings.resolution;
                var fullscreenMode = _currentGraphicsSettings.fullscreenMode;
                var targetFrameRate = _currentGraphicsSettings.targetFrameRate;
                var vsync = _currentGraphicsSettings.vsync;
                
                _currentGraphicsSettings = presetSettings.Clone();
                _currentGraphicsSettings.resolution = resolution;
                _currentGraphicsSettings.fullscreenMode = fullscreenMode;
                _currentGraphicsSettings.targetFrameRate = targetFrameRate;
                _currentGraphicsSettings.vsync = vsync;
                
                ApplySettingsToUI(_currentGraphicsSettings);
                NotifySettingsChanged();
                
                // Update preset button states
                UpdatePresetButtonStates(preset);
                
                if (qualityChangeEffect != null)
                {
                    qualityChangeEffect.Play();
                }
            }
        }

        private void UpdatePresetButtonStates(QualityPreset activePreset)
        {
            SetPresetButtonActive(lowPresetButton, activePreset == QualityPreset.Low);
            SetPresetButtonActive(mediumPresetButton, activePreset == QualityPreset.Medium);
            SetPresetButtonActive(highPresetButton, activePreset == QualityPreset.High);
            SetPresetButtonActive(ultraPresetButton, activePreset == QualityPreset.Ultra);
            SetPresetButtonActive(customPresetButton, activePreset == QualityPreset.Custom);
        }

        private void SetPresetButtonActive(Button button, bool active)
        {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = active ? Color.yellow : Color.white;
            button.colors = colors;
        }

        private void ApplyUnityQualityLevel(int qualityLevel)
        {
            QualitySettings.SetQualityLevel(qualityLevel);
            
            // Update UI to reflect Unity's quality settings
            StartCoroutine(RefreshUIAfterQualityChange());
        }

        private IEnumerator RefreshUIAfterQualityChange()
        {
            yield return new WaitForEndOfFrame();
            
            // Reload settings from Unity
            LoadGraphicsSettings();
        }

        // Text updates
        private void UpdateTargetFrameRateText(int frameRate)
        {
            if (targetFrameRateText != null)
            {
                targetFrameRateText.text = frameRate == -1 ? "Unlimited" : $"{frameRate} FPS";
            }
        }

        private void UpdateRenderScaleText(float scale)
        {
            if (renderScaleText != null)
            {
                renderScaleText.text = $"{scale:F2}x ({scale * 100:F0}%)";
            }
        }

        private void UpdateLodBiasText(float bias)
        {
            if (lodBiasText != null)
            {
                lodBiasText.text = $"{bias:F2}";
            }
        }

        // Performance monitoring
        private void UpdatePerformanceMonitoring()
        {
            _frameTimeAccumulator += Time.unscaledDeltaTime;
            _frameCount++;
            
            if (Time.unscaledTime - _lastUpdateTime >= _updateInterval)
            {
                float averageFrameTime = _frameTimeAccumulator / _frameCount;
                float fps = 1f / averageFrameTime;
                
                UpdatePerformanceDisplay(fps, averageFrameTime * 1000f);
                
                _frameTimeAccumulator = 0f;
                _frameCount = 0;
                _lastUpdateTime = Time.unscaledTime;
            }
        }

        private void UpdatePerformanceDisplay(float fps, float frameTimeMs)
        {
            if (fpsCounter != null)
            {
                fpsCounter.text = $"FPS: {fps:F1}";
                fpsCounter.color = GetFPSColor(fps);
            }
            
            if (frameTimeText != null)
            {
                frameTimeText.text = $"Frame Time: {frameTimeMs:F1}ms";
            }
            
            if (memoryUsageText != null)
            {
                long memoryBytes = System.GC.GetTotalMemory(false);
                float memoryMB = memoryBytes / (1024f * 1024f);
                memoryUsageText.text = $"Memory: {memoryMB:F1}MB";
            }
        }

        private Color GetFPSColor(float fps)
        {
            if (fps >= 60f) return Color.green;
            if (fps >= 30f) return Color.yellow;
            return Color.red;
        }

        // Benchmark
        private void RunBenchmark()
        {
            StartCoroutine(BenchmarkCoroutine());
        }

        private IEnumerator BenchmarkCoroutine()
        {
            if (loadingOverlay != null)
                loadingOverlay.SetActive(true);
            
            Debug.Log("Running graphics benchmark...");
            
            // Simulate benchmark
            yield return new WaitForSecondsRealtime(3f);
            
            // Display results
            Debug.Log("Benchmark completed! Recommended settings: High");
            
            if (loadingOverlay != null)
                loadingOverlay.SetActive(false);
        }

        // Settings application
        public void ApplySettings(SettingsData settings)
        {
            _currentGraphicsSettings.resolution = settings.resolution;
            _currentGraphicsSettings.fullscreenMode = settings.fullscreenMode;
            _currentGraphicsSettings.vsync = settings.vsyncEnabled;
            _currentGraphicsSettings.qualityLevel = settings.qualityLevel;
            _currentGraphicsSettings.targetFrameRate = settings.targetFrameRate;
            _currentGraphicsSettings.renderScale = settings.renderScale;
            _currentGraphicsSettings.shadowQuality = settings.shadowQuality;
            _currentGraphicsSettings.textureQuality = settings.textureQuality;
            
            ApplySettingsToUI(_currentGraphicsSettings);
        }

        public void CollectSettings(ref SettingsData settings)
        {
            settings.resolution = _currentGraphicsSettings.resolution;
            settings.fullscreenMode = _currentGraphicsSettings.fullscreenMode;
            settings.vsyncEnabled = _currentGraphicsSettings.vsync;
            settings.qualityLevel = _currentGraphicsSettings.qualityLevel;
            settings.targetFrameRate = _currentGraphicsSettings.targetFrameRate;
            settings.renderScale = _currentGraphicsSettings.renderScale;
            settings.shadowQuality = _currentGraphicsSettings.shadowQuality;
            settings.textureQuality = _currentGraphicsSettings.textureQuality;
        }

        private void NotifySettingsChanged()
        {
            var settingsPanel = GetComponentInParent<SettingsPanel>();
            settingsPanel?.OnSettingsChanged();
        }

        // Audio methods
        private void PlayQualityChangeSound()
        {
            Debug.Log("Quality change sound would play here");
        }

        private void PlayPresetChangeSound()
        {
            Debug.Log("Preset change sound would play here");
        }

        // Public interface
        public GraphicsSettings CurrentGraphicsSettings => _currentGraphicsSettings;
    }

    // Data structures and enums
    [System.Serializable]
    public class GraphicsSettings
    {
        public Resolution resolution;
        public FullScreenMode fullscreenMode;
        public bool vsync;
        public int qualityLevel;
        public int targetFrameRate;
        public float renderScale;
        public ShadowResolution shadowQuality;
        public int textureQuality;
        public int antiAliasing;
        public AnisotropicFiltering anisotropicFiltering;
        public bool realtimeReflections;
        public bool ssao;
        public bool bloom;
        public bool motionBlur;
        public bool depthOfField;
        public float lodBias;
        
        public GraphicsSettings Clone()
        {
            return new GraphicsSettings
            {
                resolution = resolution,
                fullscreenMode = fullscreenMode,
                vsync = vsync,
                qualityLevel = qualityLevel,
                targetFrameRate = targetFrameRate,
                renderScale = renderScale,
                shadowQuality = shadowQuality,
                textureQuality = textureQuality,
                antiAliasing = antiAliasing,
                anisotropicFiltering = anisotropicFiltering,
                realtimeReflections = realtimeReflections,
                ssao = ssao,
                bloom = bloom,
                motionBlur = motionBlur,
                depthOfField = depthOfField,
                lodBias = lodBias
            };
        }
    }

    public enum QualityPreset
    {
        Low,
        Medium,
        High,
        Ultra,
        Custom
    }
}