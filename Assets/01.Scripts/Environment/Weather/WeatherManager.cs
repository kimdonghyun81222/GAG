using _01.Scripts.Environment.Weather;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.Environment._01.Scripts.Environment.Weather
{
    [Provide]
    public class WeatherManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Weather Settings")]
        [SerializeField] private WeatherDatabase weatherDatabase;
        [SerializeField] private WeatherData currentWeatherData;
        [SerializeField] private bool autoWeatherChanges = true;
        [SerializeField] private float weatherChangeInterval = 300f; // 5 minutes
        [SerializeField] private Season currentSeason = Season.Spring;
        
        [Header("Environment References")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform particleParent;
        [SerializeField] private AudioSource ambientAudioSource;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        // Current state
        private WeatherType _currentWeatherType = WeatherType.Clear;
        private WeatherType _targetWeatherType = WeatherType.Clear;
        private bool _isTransitioning = false;
        private float _transitionProgress = 0f;
        private float _weatherTimer = 0f;
        
        // Particle systems
        private GameObject _currentParticleSystem;
        private ParticleSystem _activeParticles;
        
        // Original settings
        private Color _originalSunColor;
        private float _originalSunIntensity;
        private Color _originalAmbientColor;
        private float _originalAmbientIntensity;
        private Color _originalCameraBackgroundColor;
        
        // Properties
        public WeatherType CurrentWeatherType => _currentWeatherType;
        public WeatherData CurrentWeatherData => currentWeatherData;
        public Season CurrentSeason => currentSeason;
        public bool IsTransitioning => _isTransitioning;
        public float TransitionProgress => _transitionProgress;
        public bool AutoWeatherChanges => autoWeatherChanges;
        
        // Events
        public System.Action<WeatherType> OnWeatherChanged;
        public System.Action<WeatherType, WeatherType> OnWeatherTransitionStarted;
        public System.Action<WeatherType> OnWeatherTransitionCompleted;
        public System.Action<Season> OnSeasonChanged;

        private void Awake()
        {
            if (weatherDatabase != null)
            {
                weatherDatabase.Initialize();
            }
            
            StoreOriginalSettings();
        }

        private void Start()
        {
            // Initialize with current weather
            if (currentWeatherData != null)
            {
                SetWeatherImmediate(currentWeatherData.weatherType);
            }
            else
            {
                SetWeatherImmediate(WeatherType.Clear);
            }
        }

        private void Update()
        {
            UpdateWeatherTimer();
            UpdateTransition();
        }
        
        [Provide]
        public WeatherManager ProvideWeatherManager() => this;

        private void StoreOriginalSettings()
        {
            if (sunLight != null)
            {
                _originalSunColor = sunLight.color;
                _originalSunIntensity = sunLight.intensity;
            }
            
            _originalAmbientColor = RenderSettings.ambientLight;
            _originalAmbientIntensity = RenderSettings.ambientIntensity;
            
            if (mainCamera != null)
            {
                _originalCameraBackgroundColor = mainCamera.backgroundColor;
            }
        }

        private void UpdateWeatherTimer()
        {
            if (!autoWeatherChanges) return;
            
            _weatherTimer += Time.deltaTime;
            
            if (_weatherTimer >= weatherChangeInterval)
            {
                _weatherTimer = 0f;
                ChangeToRandomWeather();
            }
        }

        private void UpdateTransition()
        {
            if (!_isTransitioning) return;
            
            var targetData = weatherDatabase.GetWeatherData(_targetWeatherType);
            if (targetData == null) return;
            
            _transitionProgress += Time.deltaTime / targetData.transitionDuration;
            
            if (_transitionProgress >= 1f)
            {
                CompleteTransition();
            }
            else
            {
                ApplyTransitionState(targetData);
            }
        }

        // Public weather control methods
        public void ChangeWeather(WeatherType newWeatherType)
        {
            if (_currentWeatherType == newWeatherType || _isTransitioning) return;
            
            var targetData = weatherDatabase.GetWeatherData(newWeatherType);
            if (targetData == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"Weather data for {newWeatherType} not found");
                }
                return;
            }
            
            StartWeatherTransition(newWeatherType, targetData);
        }

        public void SetWeatherImmediate(WeatherType weatherType)
        {
            var weatherData = weatherDatabase.GetWeatherData(weatherType);
            if (weatherData == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"Weather data for {weatherType} not found");
                }
                return;
            }
            
            _currentWeatherType = weatherType;
            _targetWeatherType = weatherType;
            currentWeatherData = weatherData;
            _isTransitioning = false;
            _transitionProgress = 1f;
            
            ApplyWeatherSettings(weatherData);
            OnWeatherChanged?.Invoke(weatherType);
            
            if (debugMode)
            {
                Debug.Log($"Weather set immediately to: {weatherType}");
            }
        }

        public void ChangeToRandomWeather()
        {
            var possibleWeathers = weatherDatabase.GetPossibleWeatherForSeason(currentSeason);
            if (possibleWeathers.Count > 0)
            {
                // Remove current weather from possibilities for variety
                possibleWeathers.Remove(_currentWeatherType);
                
                if (possibleWeathers.Count > 0)
                {
                    var randomWeather = weatherDatabase.GetRandomWeatherForSeason(currentSeason);
                    ChangeWeather(randomWeather);
                }
            }
        }

        private void StartWeatherTransition(WeatherType targetType, WeatherData targetData)
        {
            _targetWeatherType = targetType;
            _isTransitioning = true;
            _transitionProgress = 0f;
            
            OnWeatherTransitionStarted?.Invoke(_currentWeatherType, targetType);
            
            if (debugMode)
            {
                Debug.Log($"Starting weather transition: {_currentWeatherType} → {targetType}");
            }
        }

        private void ApplyTransitionState(WeatherData targetData)
        {
            float t = targetData.transitionCurve.Evaluate(_transitionProgress);
            
            // Interpolate lighting
            if (sunLight != null)
            {
                sunLight.intensity = Mathf.Lerp(currentWeatherData.sunIntensity, targetData.sunIntensity, t);
                sunLight.color = Color.Lerp(currentWeatherData.sunColor, targetData.sunColor, t);
            }
            
            // Interpolate ambient lighting
            RenderSettings.ambientLight = Color.Lerp(currentWeatherData.ambientColor, targetData.ambientColor, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(currentWeatherData.ambientIntensity, targetData.ambientIntensity, t);
            
            // Interpolate camera background
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = Color.Lerp(currentWeatherData.skyColor, targetData.skyColor, t);
            }
            
            // Update fog
            UpdateFogSettings(targetData, t);
            
            // Update particles at halfway point
            if (_transitionProgress >= 0.5f && _currentParticleSystem == null)
            {
                UpdateParticleSystem(targetData);
            }
        }

        private void CompleteTransition()
        {
            _isTransitioning = false;
            _transitionProgress = 1f;
            _currentWeatherType = _targetWeatherType;
            
            var targetData = weatherDatabase.GetWeatherData(_targetWeatherType);
            currentWeatherData = targetData;
            
            ApplyWeatherSettings(targetData);
            
            OnWeatherTransitionCompleted?.Invoke(_currentWeatherType);
            OnWeatherChanged?.Invoke(_currentWeatherType);
            
            if (debugMode)
            {
                Debug.Log($"Weather transition completed: {_currentWeatherType}");
            }
        }

        private void ApplyWeatherSettings(WeatherData data)
        {
            if (data == null) return;
            
            // Apply lighting
            if (sunLight != null)
            {
                sunLight.intensity = data.sunIntensity;
                sunLight.color = data.sunColor;
            }
            
            RenderSettings.ambientLight = data.ambientColor;
            RenderSettings.ambientIntensity = data.ambientIntensity;
            
            // Apply camera settings
            if (mainCamera != null)
            {
                mainCamera.backgroundColor = data.skyColor;
            }
            
            // Apply fog
            UpdateFogSettings(data, 1f);
            
            // Update particles
            UpdateParticleSystem(data);
            
            // Update ambient audio
            UpdateAmbientAudio(data);
        }

        private void UpdateFogSettings(WeatherData data, float t)
        {
            if (data.enableFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = Color.Lerp(currentWeatherData?.fogColor ?? data.fogColor, data.fogColor, t);
                RenderSettings.fogDensity = Mathf.Lerp(currentWeatherData?.fogDensity ?? 0f, data.fogDensity, t);
                RenderSettings.fogStartDistance = Mathf.Lerp(currentWeatherData?.fogStartDistance ?? data.fogStartDistance, data.fogStartDistance, t);
                RenderSettings.fogEndDistance = Mathf.Lerp(currentWeatherData?.fogEndDistance ?? data.fogEndDistance, data.fogEndDistance, t);
            }
            else
            {
                RenderSettings.fog = false;
            }
        }

        private void UpdateParticleSystem(WeatherData data)
        {
            // Remove current particles
            if (_currentParticleSystem != null)
            {
                if (_activeParticles != null)
                {
                    _activeParticles.Stop();
                }
                Destroy(_currentParticleSystem, 2f);
                _currentParticleSystem = null;
                _activeParticles = null;
            }
            
            // Add new particles
            GameObject particlePrefab = GetParticlePrefabForWeather(data);
            if (particlePrefab != null)
            {
                Transform parent = particleParent != null ? particleParent : transform;
                _currentParticleSystem = Instantiate(particlePrefab, parent);
                _activeParticles = _currentParticleSystem.GetComponent<ParticleSystem>();
                
                if (_activeParticles != null)
                {
                    _activeParticles.Play();
                }
            }
        }

        private GameObject GetParticlePrefabForWeather(WeatherData data)
        {
            return data.weatherType switch
            {
                WeatherType.Rain or WeatherType.Storm => data.rainParticles,
                WeatherType.Snow or WeatherType.Blizzard => data.snowParticles,
                WeatherType.Fog => data.fogParticles,
                WeatherType.Dust => data.dustParticles,
                _ => null
            };
        }

        private void UpdateAmbientAudio(WeatherData data)
        {
            if (ambientAudioSource == null) return;
            
            if (data.ambientSound != null)
            {
                ambientAudioSource.clip = data.ambientSound;
                ambientAudioSource.volume = data.ambientVolume;
                ambientAudioSource.loop = true;
                
                if (!ambientAudioSource.isPlaying)
                {
                    ambientAudioSource.Play();
                }
            }
            else
            {
                ambientAudioSource.Stop();
            }
        }

        // Season management
        public void SetSeason(Season season)
        {
            if (currentSeason != season)
            {
                currentSeason = season;
                OnSeasonChanged?.Invoke(season);
                
                if (debugMode)
                {
                    Debug.Log($"Season changed to: {season}");
                }
            }
        }

        // Settings
        public void SetAutoWeatherChanges(bool enabled)
        {
            autoWeatherChanges = enabled;
            _weatherTimer = 0f;
        }

        public void SetWeatherChangeInterval(float interval)
        {
            weatherChangeInterval = Mathf.Max(10f, interval);
        }

        public void SetWeatherDatabase(WeatherDatabase database)
        {
            weatherDatabase = database;
            if (database != null)
            {
                database.Initialize();
            }
        }

        // Utility methods
        public bool IsWeatherType(WeatherType type)
        {
            return _currentWeatherType == type;
        }

        public float GetCurrentTemperature()
        {
            return currentWeatherData?.temperature ?? 20f;
        }

        public float GetCurrentHumidity()
        {
            return currentWeatherData?.humidity ?? 0.5f;
        }

        public float GetCurrentWindStrength()
        {
            return currentWeatherData?.windStrength ?? 0.3f;
        }

        public string GetWeatherDescription()
        {
            return currentWeatherData?.weatherName ?? _currentWeatherType.ToString();
        }

        private void OnDestroy()
        {
            if (_currentParticleSystem != null)
            {
                Destroy(_currentParticleSystem);
            }
        }
    }
}