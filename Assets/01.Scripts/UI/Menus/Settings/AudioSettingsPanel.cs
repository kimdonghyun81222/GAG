using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.Settings
{
    public class AudioSettingsPanel : UIPanel
    {
        [Header("Volume Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private Toggle masterMuteToggle;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private Toggle musicMuteToggle;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        [SerializeField] private Toggle sfxMuteToggle;
        [SerializeField] private Slider voiceVolumeSlider;
        [SerializeField] private TextMeshProUGUI voiceVolumeText;
        [SerializeField] private Toggle voiceMuteToggle;
        [SerializeField] private Slider ambientVolumeSlider;
        [SerializeField] private TextMeshProUGUI ambientVolumeText;
        [SerializeField] private Toggle ambientMuteToggle;
        
        [Header("Audio Device Settings")]
        [SerializeField] private TMP_Dropdown audioDeviceDropdown;
        [SerializeField] private TMP_Dropdown sampleRateDropdown;
        [SerializeField] private TMP_Dropdown bufferSizeDropdown;
        [SerializeField] private Button refreshDevicesButton;
        
        [Header("Audio Quality Settings")]
        [SerializeField] private TMP_Dropdown audioQualityDropdown;
        [SerializeField] private Toggle spatialAudioToggle;
        [SerializeField] private Toggle reverbToggle;
        [SerializeField] private Toggle dynamicRangeCompressionToggle;
        [SerializeField] private Slider dopplerLevelSlider;
        [SerializeField] private TextMeshProUGUI dopplerLevelText;
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup masterGroup;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup voiceGroup;
        [SerializeField] private AudioMixerGroup ambientGroup;
        
        [Header("Volume Testing")]
        [SerializeField] private Button testMasterButton;
        [SerializeField] private Button testMusicButton;
        [SerializeField] private Button testSFXButton;
        [SerializeField] private Button testVoiceButton;
        [SerializeField] private Button testAmbientButton;
        [SerializeField] private AudioClip testMasterClip;
        [SerializeField] private AudioClip testMusicClip;
        [SerializeField] private AudioClip testSFXClip;
        [SerializeField] private AudioClip testVoiceClip;
        [SerializeField] private AudioClip testAmbientClip;
        
        [Header("Audio Visualization")]
        [SerializeField] private GameObject audioVisualizerPanel;
        [SerializeField] private Slider[] frequencyBars;
        [SerializeField] private Toggle showVisualizerToggle;
        [SerializeField] private RectTransform waveformDisplay;
        
        [Header("Accessibility")]
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Toggle audioDescriptionsToggle;
        [SerializeField] private Toggle visualAudioCuesToggle;
        [SerializeField] private Slider hearingImpairmentCompensationSlider;
        [SerializeField] private TextMeshProUGUI hearingCompensationText;
        
        [Header("Advanced Settings")]
        [SerializeField] private Toggle enableAudioToggle;
        [SerializeField] private Toggle muteInBackgroundToggle;
        [SerializeField] private Slider audioLatencySlider;
        [SerializeField] private TextMeshProUGUI audioLatencyText;
        [SerializeField] private Button resetAudioButton;
        
        [Header("Audio Info")]
        [SerializeField] private TextMeshProUGUI audioDriverText;
        [SerializeField] private TextMeshProUGUI audioDeviceInfoText;
        [SerializeField] private TextMeshProUGUI audioLatencyInfoText;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem volumeChangeEffect;
        [SerializeField] private AudioSource testAudioSource;
        
        [Header("Audio")]
        [SerializeField] private AudioClip volumeTestSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip settingsChangeSound;
        
        // Audio settings data
        private AudioSettings _currentAudioSettings;
        private bool _isApplyingSettings = false;
        private bool _isTesting = false;
        
        // Audio device management
        private string[] _availableAudioDevices;
        private int[] _availableSampleRates = { 22050, 44100, 48000, 96000 };
        private int[] _availableBufferSizes = { 256, 512, 1024, 2048 };
        
        // Volume states (for mute functionality)
        private Dictionary<VolumeChannel, float> _volumeBeforeMute = new Dictionary<VolumeChannel, float>();
        
        // Audio visualization
        private float[] _spectrumData = new float[256];
        private AudioSource _visualizerAudioSource;
        
        // Audio mixer parameter names
        private const string MASTER_VOLUME_PARAM = "MasterVolume";
        private const string MUSIC_VOLUME_PARAM = "MusicVolume";
        private const string SFX_VOLUME_PARAM = "SFXVolume";
        private const string VOICE_VOLUME_PARAM = "VoiceVolume";
        private const string AMBIENT_VOLUME_PARAM = "AmbientVolume";

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSettings();
        }

        protected override void Start()
        {
            base.Start();
            SetupAudioPanel();
            LoadAudioSettings();
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
                UpdateAudioVisualization();
            }
        }

        private void InitializeAudioSettings()
        {
            // Initialize current settings
            _currentAudioSettings = new AudioSettings();
            
            // Initialize volume states
            _volumeBeforeMute[VolumeChannel.Master] = 1f;
            _volumeBeforeMute[VolumeChannel.Music] = 0.8f;
            _volumeBeforeMute[VolumeChannel.SFX] = 1f;
            _volumeBeforeMute[VolumeChannel.Voice] = 1f;
            _volumeBeforeMute[VolumeChannel.Ambient] = 0.6f;
            
            // Get available audio devices
            RefreshAudioDevices();
            
            // Setup test audio source
            if (testAudioSource == null)
            {
                var audioSourceObj = new GameObject("TestAudioSource");
                audioSourceObj.transform.SetParent(transform);
                testAudioSource = audioSourceObj.AddComponent<AudioSource>();
                testAudioSource.playOnAwake = false;
            }
        }

        private void SetupAudioPanel()
        {
            // Setup volume sliders
            SetupVolumeControls();
            
            // Setup audio device controls
            SetupAudioDeviceControls();
            
            // Setup quality controls
            SetupQualityControls();
            
            // Setup test buttons
            SetupTestButtons();
            
            // Setup accessibility controls
            SetupAccessibilityControls();
            
            // Setup advanced controls
            SetupAdvancedControls();
            
            // Setup visualization
            SetupAudioVisualization();
            
            // Update audio info
            UpdateAudioInfo();
        }

        private void SetupVolumeControls()
        {
            // Master volume
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            
            if (masterMuteToggle != null)
                masterMuteToggle.onValueChanged.AddListener(value => OnMuteChanged(VolumeChannel.Master, value));
            
            // Music volume
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.minValue = 0f;
                musicVolumeSlider.maxValue = 1f;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            
            if (musicMuteToggle != null)
                musicMuteToggle.onValueChanged.AddListener(value => OnMuteChanged(VolumeChannel.Music, value));
            
            // SFX volume
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.minValue = 0f;
                sfxVolumeSlider.maxValue = 1f;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            if (sfxMuteToggle != null)
                sfxMuteToggle.onValueChanged.AddListener(value => OnMuteChanged(VolumeChannel.SFX, value));
            
            // Voice volume
            if (voiceVolumeSlider != null)
            {
                voiceVolumeSlider.minValue = 0f;
                voiceVolumeSlider.maxValue = 1f;
                voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            }
            
            if (voiceMuteToggle != null)
                voiceMuteToggle.onValueChanged.AddListener(value => OnMuteChanged(VolumeChannel.Voice, value));
            
            // Ambient volume
            if (ambientVolumeSlider != null)
            {
                ambientVolumeSlider.minValue = 0f;
                ambientVolumeSlider.maxValue = 1f;
                ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
            }
            
            if (ambientMuteToggle != null)
                ambientMuteToggle.onValueChanged.AddListener(value => OnMuteChanged(VolumeChannel.Ambient, value));
        }

        private void SetupAudioDeviceControls()
        {
            // Audio device dropdown
            if (audioDeviceDropdown != null)
            {
                RefreshAudioDeviceDropdown();
                audioDeviceDropdown.onValueChanged.AddListener(OnAudioDeviceChanged);
            }
            
            // Sample rate dropdown
            if (sampleRateDropdown != null)
            {
                sampleRateDropdown.ClearOptions();
                var sampleRateOptions = new List<string>();
                foreach (int rate in _availableSampleRates)
                {
                    sampleRateOptions.Add($"{rate} Hz");
                }
                sampleRateDropdown.AddOptions(sampleRateOptions);
                sampleRateDropdown.onValueChanged.AddListener(OnSampleRateChanged);
            }
            
            // Buffer size dropdown
            if (bufferSizeDropdown != null)
            {
                bufferSizeDropdown.ClearOptions();
                var bufferSizeOptions = new List<string>();
                foreach (int size in _availableBufferSizes)
                {
                    bufferSizeOptions.Add($"{size} samples");
                }
                bufferSizeDropdown.AddOptions(bufferSizeOptions);
                bufferSizeDropdown.onValueChanged.AddListener(OnBufferSizeChanged);
            }
            
            // Refresh devices button
            if (refreshDevicesButton != null)
                refreshDevicesButton.onClick.AddListener(RefreshAudioDevices);
        }

        private void SetupQualityControls()
        {
            // Audio quality dropdown
            if (audioQualityDropdown != null)
            {
                audioQualityDropdown.ClearOptions();
                audioQualityDropdown.AddOptions(new List<string> { "Low", "Medium", "High", "Ultra" });
                audioQualityDropdown.onValueChanged.AddListener(OnAudioQualityChanged);
            }
            
            // Spatial audio toggle
            if (spatialAudioToggle != null)
                spatialAudioToggle.onValueChanged.AddListener(OnSpatialAudioChanged);
            
            // Reverb toggle
            if (reverbToggle != null)
                reverbToggle.onValueChanged.AddListener(OnReverbChanged);
            
            // Dynamic range compression toggle
            if (dynamicRangeCompressionToggle != null)
                dynamicRangeCompressionToggle.onValueChanged.AddListener(OnDynamicRangeCompressionChanged);
            
            // Doppler level slider
            if (dopplerLevelSlider != null)
            {
                dopplerLevelSlider.minValue = 0f;
                dopplerLevelSlider.maxValue = 5f;
                dopplerLevelSlider.onValueChanged.AddListener(OnDopplerLevelChanged);
            }
        }

        private void SetupTestButtons()
        {
            if (testMasterButton != null)
                testMasterButton.onClick.AddListener(() => TestVolume(VolumeChannel.Master));
            
            if (testMusicButton != null)
                testMusicButton.onClick.AddListener(() => TestVolume(VolumeChannel.Music));
            
            if (testSFXButton != null)
                testSFXButton.onClick.AddListener(() => TestVolume(VolumeChannel.SFX));
            
            if (testVoiceButton != null)
                testVoiceButton.onClick.AddListener(() => TestVolume(VolumeChannel.Voice));
            
            if (testAmbientButton != null)
                testAmbientButton.onClick.AddListener(() => TestVolume(VolumeChannel.Ambient));
        }

        private void SetupAccessibilityControls()
        {
            if (subtitlesToggle != null)
                subtitlesToggle.onValueChanged.AddListener(OnSubtitlesChanged);
            
            if (audioDescriptionsToggle != null)
                audioDescriptionsToggle.onValueChanged.AddListener(OnAudioDescriptionsChanged);
            
            if (visualAudioCuesToggle != null)
                visualAudioCuesToggle.onValueChanged.AddListener(OnVisualAudioCuesChanged);
            
            if (hearingImpairmentCompensationSlider != null)
            {
                hearingImpairmentCompensationSlider.minValue = 0f;
                hearingImpairmentCompensationSlider.maxValue = 1f;
                hearingImpairmentCompensationSlider.onValueChanged.AddListener(OnHearingCompensationChanged);
            }
        }

        private void SetupAdvancedControls()
        {
            if (enableAudioToggle != null)
                enableAudioToggle.onValueChanged.AddListener(OnEnableAudioChanged);
            
            if (muteInBackgroundToggle != null)
                muteInBackgroundToggle.onValueChanged.AddListener(OnMuteInBackgroundChanged);
            
            if (audioLatencySlider != null)
            {
                audioLatencySlider.minValue = 0f;
                audioLatencySlider.maxValue = 100f;
                audioLatencySlider.onValueChanged.AddListener(OnAudioLatencyChanged);
            }
            
            if (resetAudioButton != null)
                resetAudioButton.onClick.AddListener(ResetAudioSettings);
        }

        private void SetupAudioVisualization()
        {
            if (showVisualizerToggle != null)
            {
                showVisualizerToggle.onValueChanged.AddListener(OnShowVisualizerChanged);
            }
            
            if (audioVisualizerPanel != null)
            {
                audioVisualizerPanel.SetActive(false);
            }
            
            // Setup visualizer audio source
            if (_visualizerAudioSource == null)
            {
                var visualizerObj = new GameObject("VisualizerAudioSource");
                visualizerObj.transform.SetParent(transform);
                _visualizerAudioSource = visualizerObj.AddComponent<AudioSource>();
                _visualizerAudioSource.playOnAwake = false;
                _visualizerAudioSource.loop = true;
                _visualizerAudioSource.volume = 0f; // Silent for visualization only
            }
        }

        private void LoadAudioSettings()
        {
            // Load audio settings from PlayerPrefs
            _currentAudioSettings.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            _currentAudioSettings.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            _currentAudioSettings.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            _currentAudioSettings.voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
            _currentAudioSettings.ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.6f);
            _currentAudioSettings.audioEnabled = PlayerPrefs.GetInt("AudioEnabled", 1) == 1;
            _currentAudioSettings.spatialAudio = PlayerPrefs.GetInt("SpatialAudio", 1) == 1;
            _currentAudioSettings.reverb = PlayerPrefs.GetInt("Reverb", 1) == 1;
            _currentAudioSettings.dynamicRangeCompression = PlayerPrefs.GetInt("DynamicRangeCompression", 0) == 1;
            _currentAudioSettings.dopplerLevel = PlayerPrefs.GetFloat("DopplerLevel", 1f);
            _currentAudioSettings.audioQuality = PlayerPrefs.GetInt("AudioQuality", 2);
            _currentAudioSettings.sampleRate = PlayerPrefs.GetInt("SampleRate", 44100);
            _currentAudioSettings.bufferSize = PlayerPrefs.GetInt("BufferSize", 1024);
            _currentAudioSettings.subtitles = PlayerPrefs.GetInt("Subtitles", 0) == 1;
            _currentAudioSettings.audioDescriptions = PlayerPrefs.GetInt("AudioDescriptions", 0) == 1;
            _currentAudioSettings.visualAudioCues = PlayerPrefs.GetInt("VisualAudioCues", 0) == 1;
            _currentAudioSettings.hearingCompensation = PlayerPrefs.GetFloat("HearingCompensation", 0f);
            _currentAudioSettings.muteInBackground = PlayerPrefs.GetInt("MuteInBackground", 1) == 1;
            _currentAudioSettings.audioLatency = PlayerPrefs.GetFloat("AudioLatency", 20f);
            
            ApplySettingsToUI(_currentAudioSettings);
        }

        private void ApplySettingsToUI(AudioSettings settings)
        {
            _isApplyingSettings = true;
            
            // Volume sliders
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = settings.masterVolume;
                UpdateVolumeText(masterVolumeText, settings.masterVolume);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = settings.musicVolume;
                UpdateVolumeText(musicVolumeText, settings.musicVolume);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = settings.sfxVolume;
                UpdateVolumeText(sfxVolumeText, settings.sfxVolume);
            }
            
            if (voiceVolumeSlider != null)
            {
                voiceVolumeSlider.value = settings.voiceVolume;
                UpdateVolumeText(voiceVolumeText, settings.voiceVolume);
            }
            
            if (ambientVolumeSlider != null)
            {
                ambientVolumeSlider.value = settings.ambientVolume;
                UpdateVolumeText(ambientVolumeText, settings.ambientVolume);
            }
            
            // Quality settings
            if (audioQualityDropdown != null)
                audioQualityDropdown.value = settings.audioQuality;
            
            if (spatialAudioToggle != null)
                spatialAudioToggle.isOn = settings.spatialAudio;
            
            if (reverbToggle != null)
                reverbToggle.isOn = settings.reverb;
            
            if (dynamicRangeCompressionToggle != null)
                dynamicRangeCompressionToggle.isOn = settings.dynamicRangeCompression;
            
            if (dopplerLevelSlider != null)
            {
                dopplerLevelSlider.value = settings.dopplerLevel;
                UpdateDopplerLevelText(settings.dopplerLevel);
            }
            
            // Device settings
            if (sampleRateDropdown != null)
            {
                int sampleRateIndex = System.Array.IndexOf(_availableSampleRates, settings.sampleRate);
                sampleRateDropdown.value = Mathf.Max(0, sampleRateIndex);
            }
            
            if (bufferSizeDropdown != null)
            {
                int bufferSizeIndex = System.Array.IndexOf(_availableBufferSizes, settings.bufferSize);
                bufferSizeDropdown.value = Mathf.Max(0, bufferSizeIndex);
            }
            
            // Accessibility settings
            if (subtitlesToggle != null)
                subtitlesToggle.isOn = settings.subtitles;
            
            if (audioDescriptionsToggle != null)
                audioDescriptionsToggle.isOn = settings.audioDescriptions;
            
            if (visualAudioCuesToggle != null)
                visualAudioCuesToggle.isOn = settings.visualAudioCues;
            
            if (hearingImpairmentCompensationSlider != null)
            {
                hearingImpairmentCompensationSlider.value = settings.hearingCompensation;
                UpdateHearingCompensationText(settings.hearingCompensation);
            }
            
            // Advanced settings
            if (enableAudioToggle != null)
                enableAudioToggle.isOn = settings.audioEnabled;
            
            if (muteInBackgroundToggle != null)
                muteInBackgroundToggle.isOn = settings.muteInBackground;
            
            if (audioLatencySlider != null)
            {
                audioLatencySlider.value = settings.audioLatency;
                UpdateAudioLatencyText(settings.audioLatency);
            }
            
            // Apply to audio mixer
            ApplyVolumeToMixer();
            
            _isApplyingSettings = false;
        }

        // Volume event handlers
        private void OnMasterVolumeChanged(float volume)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.masterVolume = volume;
            _volumeBeforeMute[VolumeChannel.Master] = volume;
            UpdateVolumeText(masterVolumeText, volume);
            ApplyVolumeToMixer();
            NotifySettingsChanged();
            
            if (volumeChangeEffect != null && !_isTesting)
            {
                volumeChangeEffect.Play();
            }
        }

        private void OnMusicVolumeChanged(float volume)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.musicVolume = volume;
            _volumeBeforeMute[VolumeChannel.Music] = volume;
            UpdateVolumeText(musicVolumeText, volume);
            ApplyVolumeToMixer();
            NotifySettingsChanged();
        }

        private void OnSFXVolumeChanged(float volume)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.sfxVolume = volume;
            _volumeBeforeMute[VolumeChannel.SFX] = volume;
            UpdateVolumeText(sfxVolumeText, volume);
            ApplyVolumeToMixer();
            NotifySettingsChanged();
        }

        private void OnVoiceVolumeChanged(float volume)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.voiceVolume = volume;
            _volumeBeforeMute[VolumeChannel.Voice] = volume;
            UpdateVolumeText(voiceVolumeText, volume);
            ApplyVolumeToMixer();
            NotifySettingsChanged();
        }

        private void OnAmbientVolumeChanged(float volume)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.ambientVolume = volume;
            _volumeBeforeMute[VolumeChannel.Ambient] = volume;
            UpdateVolumeText(ambientVolumeText, volume);
            ApplyVolumeToMixer();
            NotifySettingsChanged();
        }

        private void OnMuteChanged(VolumeChannel channel, bool muted)
        {
            if (_isApplyingSettings) return;
            
            Slider volumeSlider = GetVolumeSlider(channel);
            if (volumeSlider == null) return;
            
            if (muted)
            {
                // Store current volume and mute
                _volumeBeforeMute[channel] = volumeSlider.value;
                volumeSlider.value = 0f;
            }
            else
            {
                // Restore previous volume
                volumeSlider.value = _volumeBeforeMute[channel];
            }
        }

        private Slider GetVolumeSlider(VolumeChannel channel)
        {
            return channel switch
            {
                VolumeChannel.Master => masterVolumeSlider,
                VolumeChannel.Music => musicVolumeSlider,
                VolumeChannel.SFX => sfxVolumeSlider,
                VolumeChannel.Voice => voiceVolumeSlider,
                VolumeChannel.Ambient => ambientVolumeSlider,
                _ => null
            };
        }

        // Other event handlers
        private void OnAudioDeviceChanged(int index)
        {
            if (_isApplyingSettings || index >= _availableAudioDevices.Length) return;
            
            _currentAudioSettings.audioDevice = _availableAudioDevices[index];
            NotifySettingsChanged();
            UpdateAudioInfo();
        }

        private void OnSampleRateChanged(int index)
        {
            if (_isApplyingSettings || index >= _availableSampleRates.Length) return;
            
            _currentAudioSettings.sampleRate = _availableSampleRates[index];
            NotifySettingsChanged();
        }

        private void OnBufferSizeChanged(int index)
        {
            if (_isApplyingSettings || index >= _availableBufferSizes.Length) return;
            
            _currentAudioSettings.bufferSize = _availableBufferSizes[index];
            NotifySettingsChanged();
        }

        private void OnAudioQualityChanged(int quality)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.audioQuality = quality;
            ApplyAudioQuality(quality);
            NotifySettingsChanged();
        }

        private void OnSpatialAudioChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.spatialAudio = enabled;
            NotifySettingsChanged();
        }

        private void OnReverbChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.reverb = enabled;
            NotifySettingsChanged();
        }

        private void OnDynamicRangeCompressionChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.dynamicRangeCompression = enabled;
            NotifySettingsChanged();
        }

        private void OnDopplerLevelChanged(float level)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.dopplerLevel = level;
            // AudioListener.dopplerFactor = level;
            UpdateDopplerLevelText(level);
            NotifySettingsChanged();
        }

        private void OnSubtitlesChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.subtitles = enabled;
            NotifySettingsChanged();
        }

        private void OnAudioDescriptionsChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.audioDescriptions = enabled;
            NotifySettingsChanged();
        }

        private void OnVisualAudioCuesChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.visualAudioCues = enabled;
            NotifySettingsChanged();
        }

        private void OnHearingCompensationChanged(float compensation)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.hearingCompensation = compensation;
            UpdateHearingCompensationText(compensation);
            NotifySettingsChanged();
        }

        private void OnEnableAudioChanged(bool enabled)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.audioEnabled = enabled;
            AudioListener.volume = enabled ? _currentAudioSettings.masterVolume : 0f;
            NotifySettingsChanged();
        }

        private void OnMuteInBackgroundChanged(bool mute)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.muteInBackground = mute;
            NotifySettingsChanged();
        }

        private void OnAudioLatencyChanged(float latency)
        {
            if (_isApplyingSettings) return;
            
            _currentAudioSettings.audioLatency = latency;
            UpdateAudioLatencyText(latency);
            NotifySettingsChanged();
        }

        private void OnShowVisualizerChanged(bool show)
        {
            if (audioVisualizerPanel != null)
            {
                audioVisualizerPanel.SetActive(show);
            }
        }

        // Audio device management
        private void RefreshAudioDevices()
        {
            // Get available audio devices (this would use Unity's audio device APIs)
            _availableAudioDevices = new string[] { "Default Device", "Speakers", "Headphones" };
            RefreshAudioDeviceDropdown();
            UpdateAudioInfo();
        }

        private void RefreshAudioDeviceDropdown()
        {
            if (audioDeviceDropdown == null) return;
            
            audioDeviceDropdown.ClearOptions();
            audioDeviceDropdown.AddOptions(_availableAudioDevices.ToList());
        }

        // Volume testing
        private void TestVolume(VolumeChannel channel)
        {
            if (_isTesting) return;
            
            AudioClip testClip = GetTestClip(channel);
            if (testClip == null || testAudioSource == null) return;
            
            StartCoroutine(TestVolumeCoroutine(channel, testClip));
        }

        private AudioClip GetTestClip(VolumeChannel channel)
        {
            return channel switch
            {
                VolumeChannel.Master => testMasterClip ?? volumeTestSound,
                VolumeChannel.Music => testMusicClip,
                VolumeChannel.SFX => testSFXClip,
                VolumeChannel.Voice => testVoiceClip,
                VolumeChannel.Ambient => testAmbientClip,
                _ => volumeTestSound
            };
        }

        private IEnumerator TestVolumeCoroutine(VolumeChannel channel, AudioClip clip)
        {
            _isTesting = true;
            
            // Configure test audio source
            testAudioSource.clip = clip;
            testAudioSource.outputAudioMixerGroup = GetMixerGroup(channel);
            testAudioSource.volume = 1f;
            
            // Play test sound
            testAudioSource.Play();
            
            // Wait for clip to finish
            yield return new WaitForSecondsRealtime(clip.length);
            
            _isTesting = false;
        }

        private AudioMixerGroup GetMixerGroup(VolumeChannel channel)
        {
            return channel switch
            {
                VolumeChannel.Master => masterGroup,
                VolumeChannel.Music => musicGroup,
                VolumeChannel.SFX => sfxGroup,
                VolumeChannel.Voice => voiceGroup,
                VolumeChannel.Ambient => ambientGroup,
                _ => masterGroup
            };
        }

        // Audio visualization
        private void UpdateAudioVisualization()
        {
            if (audioVisualizerPanel == null || !audioVisualizerPanel.activeInHierarchy) return;
            
            // Get spectrum data
            AudioListener.GetSpectrumData(_spectrumData, 0, FFTWindow.BlackmanHarris);
            
            // Update frequency bars
            if (frequencyBars != null)
            {
                for (int i = 0; i < frequencyBars.Length && i < _spectrumData.Length; i++)
                {
                    if (frequencyBars[i] != null)
                    {
                        frequencyBars[i].value = _spectrumData[i] * 100f; // Scale for visibility
                    }
                }
            }
        }

        // Audio mixer management
        private void ApplyVolumeToMixer()
        {
            if (audioMixer == null) return;
            
            // Convert linear volume to decibels
            float masterVolumeDb = LinearToDecibel(_currentAudioSettings.masterVolume);
            float musicVolumeDb = LinearToDecibel(_currentAudioSettings.musicVolume);
            float sfxVolumeDb = LinearToDecibel(_currentAudioSettings.sfxVolume);
            float voiceVolumeDb = LinearToDecibel(_currentAudioSettings.voiceVolume);
            float ambientVolumeDb = LinearToDecibel(_currentAudioSettings.ambientVolume);
            
            // Apply to mixer
            audioMixer.SetFloat(MASTER_VOLUME_PARAM, masterVolumeDb);
            audioMixer.SetFloat(MUSIC_VOLUME_PARAM, musicVolumeDb);
            audioMixer.SetFloat(SFX_VOLUME_PARAM, sfxVolumeDb);
            audioMixer.SetFloat(VOICE_VOLUME_PARAM, voiceVolumeDb);
            audioMixer.SetFloat(AMBIENT_VOLUME_PARAM, ambientVolumeDb);
            
            // Apply to AudioListener as well
            AudioListener.volume = _currentAudioSettings.audioEnabled ? _currentAudioSettings.masterVolume : 0f;
        }

        private float LinearToDecibel(float linear)
        {
            if (linear <= 0f) return -80f; // Silence
            return Mathf.Log10(linear) * 20f;
        }

        private void ApplyAudioQuality(int quality)
        {
            // Apply audio quality settings
            switch (quality)
            {
                case 0: // Low
                    // 낮은 품질 설정
                    break;
                case 1: // Medium
                    // 중간 품질 설정
                    break;
                case 2: // High
                    // 높은 품질 설정
                    break;
                case 3: // Ultra
                    // 최고 품질 설정
                    break;
            }
        }

        // Text updates
        private void UpdateVolumeText(TextMeshProUGUI text, float volume)
        {
            if (text != null)
            {
                text.text = $"{volume * 100f:F0}%";
            }
        }

        private void UpdateDopplerLevelText(float level)
        {
            if (dopplerLevelText != null)
            {
                dopplerLevelText.text = $"{level:F1}x";
            }
        }

        private void UpdateHearingCompensationText(float compensation)
        {
            if (hearingCompensationText != null)
            {
                hearingCompensationText.text = $"{compensation * 100f:F0}%";
            }
        }

        private void UpdateAudioLatencyText(float latency)
        {
            if (audioLatencyText != null)
            {
                audioLatencyText.text = $"{latency:F0}ms";
            }
        }

        private void UpdateAudioInfo()
        {
            if (audioDriverText != null)
            {
                audioDriverText.text = "Driver: Default Audio Driver";
            }
            
            if (audioDeviceInfoText != null)
            {
                audioDeviceInfoText.text = $"Device: {_currentAudioSettings.audioDevice ?? "Default"}";
            }
            
            if (audioLatencyInfoText != null)
            {
                audioLatencyInfoText.text = "System Latency: Unknown";
            }
        }

        // Reset functionality
        private void ResetAudioSettings()
        {
            _currentAudioSettings = CreateDefaultAudioSettings();
            ApplySettingsToUI(_currentAudioSettings);
            NotifySettingsChanged();
        }

        private AudioSettings CreateDefaultAudioSettings()
        {
            return new AudioSettings
            {
                masterVolume = 1f,
                musicVolume = 0.8f,
                sfxVolume = 1f,
                voiceVolume = 1f,
                ambientVolume = 0.6f,
                audioEnabled = true,
                spatialAudio = true,
                reverb = true,
                dynamicRangeCompression = false,
                dopplerLevel = 1f,
                audioQuality = 2,
                sampleRate = 44100,
                bufferSize = 1024,
                subtitles = false,
                audioDescriptions = false,
                visualAudioCues = false,
                hearingCompensation = 0f,
                muteInBackground = true,
                audioLatency = 20f
            };
        }

        // Settings interface
        public void ApplySettings(SettingsData settings)
        {
            _currentAudioSettings.masterVolume = settings.masterVolume;
            _currentAudioSettings.musicVolume = settings.musicVolume;
            _currentAudioSettings.sfxVolume = settings.sfxVolume;
            _currentAudioSettings.voiceVolume = settings.voiceVolume;
            _currentAudioSettings.ambientVolume = settings.ambientVolume;
            _currentAudioSettings.audioEnabled = settings.audioEnabled;
            
            ApplySettingsToUI(_currentAudioSettings);
        }

        public void CollectSettings(ref SettingsData settings)
        {
            settings.masterVolume = _currentAudioSettings.masterVolume;
            settings.musicVolume = _currentAudioSettings.musicVolume;
            settings.sfxVolume = _currentAudioSettings.sfxVolume;
            settings.voiceVolume = _currentAudioSettings.voiceVolume;
            settings.ambientVolume = _currentAudioSettings.ambientVolume;
            settings.audioEnabled = _currentAudioSettings.audioEnabled;
        }

        private void NotifySettingsChanged()
        {
            var settingsPanel = GetComponentInParent<SettingsPanel>();
            settingsPanel?.OnSettingsChanged();
        }

        // Public interface
        public AudioSettings CurrentAudioSettings => _currentAudioSettings;
    }

    // Data structures and enums
    [System.Serializable]
    public class AudioSettings
    {
        [Header("Volume")]
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public float voiceVolume = 1f;
        public float ambientVolume = 0.6f;
        
        [Header("Device")]
        public string audioDevice;
        public int sampleRate = 44100;
        public int bufferSize = 1024;
        
        [Header("Quality")]
        public int audioQuality = 2;
        public bool spatialAudio = true;
        public bool reverb = true;
        public bool dynamicRangeCompression = false;
        public float dopplerLevel = 1f;
        
        [Header("Accessibility")]
        public bool subtitles = false;
        public bool audioDescriptions = false;
        public bool visualAudioCues = false;
        public float hearingCompensation = 0f;
        
        [Header("Advanced")]
        public bool audioEnabled = true;
        public bool muteInBackground = true;
        public float audioLatency = 20f;
    }

    public enum VolumeChannel
    {
        Master,
        Music,
        SFX,
        Voice,
        Ambient
    }
}