using System;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Farming._01.Scripts.Farming.Growth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.HUD
{
    public class WeatherHUD : MonoBehaviour
    {
        [Header("Weather Display")]
        [SerializeField] private GameObject weatherContainer;
        [SerializeField] private Image weatherIcon;
        [SerializeField] private TextMeshProUGUI weatherStatusText;
        [SerializeField] private Image temperatureIcon;
        [SerializeField] private TextMeshProUGUI temperatureText;
        
        [Header("Time Display")]
        [SerializeField] private GameObject timeContainer;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private Slider dayProgressBar;
        [SerializeField] private Image sunMoonIcon;
        
        [Header("Season Display")]
        [SerializeField] private GameObject seasonContainer;
        [SerializeField] private TextMeshProUGUI seasonText;
        [SerializeField] private Image seasonIcon;
        [SerializeField] private Slider seasonProgressBar;
        [SerializeField] private TextMeshProUGUI seasonProgressText;
        
        [Header("Environmental Info")]
        [SerializeField] private GameObject environmentContainer;
        [SerializeField] private Slider humidityBar;
        [SerializeField] private TextMeshProUGUI humidityText;
        [SerializeField] private Slider windSpeedBar;
        [SerializeField] private TextMeshProUGUI windSpeedText;
        [SerializeField] private Image windDirectionIndicator;
        
        [Header("Weather Icons")]
        [SerializeField] private Sprite clearIcon;
        [SerializeField] private Sprite cloudyIcon;
        [SerializeField] private Sprite rainyIcon;
        [SerializeField] private Sprite stormyIcon;
        [SerializeField] private Sprite sunnyIcon;
        [SerializeField] private Sprite overcastIcon;
        [SerializeField] private Sprite foggyIcon;
        [SerializeField] private Sprite windyIcon;
        [SerializeField] private Sprite snowyIcon;
        
        [Header("Season Icons")]
        [SerializeField] private Sprite springIcon;
        [SerializeField] private Sprite summerIcon;
        [SerializeField] private Sprite autumnIcon;
        [SerializeField] private Sprite winterIcon;
        
        [Header("Time Icons")]
        [SerializeField] private Sprite sunIcon;
        [SerializeField] private Sprite moonIcon;
        [SerializeField] private Sprite sunriseIcon;
        [SerializeField] private Sprite sunsetIcon;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem rainEffect;
        [SerializeField] private ParticleSystem snowEffect;
        [SerializeField] private ParticleSystem fogEffect;
        [SerializeField] private GameObject lightningEffect;
        
        [Header("Color Themes")]
        [SerializeField] private WeatherColorTheme clearTheme = new WeatherColorTheme { backgroundColor = new Color(0.5f, 0.8f, 1f, 0.8f), textColor = Color.white };
        [SerializeField] private WeatherColorTheme rainyTheme = new WeatherColorTheme { backgroundColor = new Color(0.3f, 0.3f, 0.6f, 0.8f), textColor = Color.white };
        [SerializeField] private WeatherColorTheme stormyTheme = new WeatherColorTheme { backgroundColor = new Color(0.2f, 0.2f, 0.4f, 0.9f), textColor = Color.yellow };
        [SerializeField] private WeatherColorTheme sunnyTheme = new WeatherColorTheme { backgroundColor = new Color(1f, 0.9f, 0.3f, 0.8f), textColor = Color.black };
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableWeatherTransitions = true;
        [SerializeField] private float transitionDuration = 2f;
        [SerializeField] private bool enableTemperatureAnimation = true;
        [SerializeField] private float temperatureAnimationSpeed = 1f;
        
        [Header("Update Settings")]
        [SerializeField] private float weatherUpdateInterval = 1f;
        [SerializeField] private float timeUpdateInterval = 0.1f;
        [SerializeField] private bool enableRealTimeUpdates = true;
        
        [Header("Display Options")]
        [SerializeField] private bool show24HourFormat = false;
        [SerializeField] private bool showSeconds = false;
        [SerializeField] private bool showWeatherEffects = true;
        [SerializeField] private bool showEnvironmentalData = true;
        [SerializeField] private TemperatureUnit temperatureUnit = TemperatureUnit.Celsius;
        
        // Dependencies
        [Inject] private SeasonalGrowth seasonalGrowth;
        
        // Current weather state
        private WeatherType _currentWeather = WeatherType.Clear;
        private float _currentTemperature = 20f;
        private float _currentHumidity = 0.5f;
        private float _currentWindSpeed = 0f;
        private float _currentWindDirection = 0f;
        
        // Time tracking
        private float _lastWeatherUpdate;
        private float _lastTimeUpdate;
        
        // Animation
        private Coroutine _weatherTransitionCoroutine;
        private WeatherColorTheme _currentColorTheme;
        
        // Weather simulation
        private float _weatherChangeTimer = 0f;
        private float _weatherChangeDuration = 300f; // 5 minutes default

        private void Awake()
        {
            InitializeWeatherHUD();
        }

        private void Start()
        {
            InitializeWeatherState();
            // 🔧 수정: UpdateAllDisplays 메서드를 개별 업데이트 메서드들로 분리
            UpdateWeatherDisplay();
            UpdateEnvironmentalDisplay();
            UpdateSeasonDisplay();
            UpdateTimeDisplay();
        }

        private void Update()
        {
            if (enableRealTimeUpdates)
            {
                UpdateTimeDisplay();
                UpdateWeatherSystem();
            }
        }

        private void InitializeWeatherHUD()
        {
            // Auto-find containers if not assigned
            if (weatherContainer == null)
            {
                var weatherObj = transform.Find("Weather");
                if (weatherObj != null) weatherContainer = weatherObj.gameObject;
            }
            
            if (timeContainer == null)
            {
                var timeObj = transform.Find("Time");
                if (timeObj != null) timeContainer = timeObj.gameObject;
            }
            
            if (seasonContainer == null)
            {
                var seasonObj = transform.Find("Season");
                if (seasonObj != null) seasonContainer = seasonObj.gameObject;
            }
            
            if (environmentContainer == null)
            {
                var envObj = transform.Find("Environment");
                if (envObj != null) environmentContainer = envObj.gameObject;
            }
            
            // Set initial color theme
            _currentColorTheme = clearTheme;
            
            // Subscribe to seasonal growth events if available
            if (seasonalGrowth != null)
            {
                seasonalGrowth.OnSeasonChanged += OnSeasonChanged;
                seasonalGrowth.OnEnvironmentChanged += OnEnvironmentChanged;
                seasonalGrowth.OnDayChanged += OnDayChanged;
            }
        }

        private void InitializeWeatherState()
        {
            // Initialize with current seasonal data if available
            if (seasonalGrowth != null)
            {
                _currentTemperature = seasonalGrowth.CurrentTemperature;
                _currentHumidity = seasonalGrowth.CurrentMoisture;
            }
            
            // Set initial weather based on season
            SetWeatherBasedOnSeason();
        }

        private void SetWeatherBasedOnSeason()
        {
            if (seasonalGrowth == null) return;
            
            Season currentSeason = seasonalGrowth.CurrentSeason;
            
            // Set weather patterns based on season
            WeatherType[] seasonalWeathers = currentSeason switch
            {
                Season.Spring => new[] { WeatherType.Rainy, WeatherType.Cloudy, WeatherType.Clear },
                Season.Summer => new[] { WeatherType.Sunny, WeatherType.Clear, WeatherType.Overcast },
                Season.Autumn => new[] { WeatherType.Cloudy, WeatherType.Windy, WeatherType.Rainy },
                Season.Winter => new[] { WeatherType.Overcast, WeatherType.Foggy, WeatherType.Stormy },
                _ => new[] { WeatherType.Clear }
            };
            
            _currentWeather = seasonalWeathers[UnityEngine.Random.Range(0, seasonalWeathers.Length)];
        }

        private void UpdateTimeDisplay()
        {
            if (Time.time - _lastTimeUpdate < timeUpdateInterval) return;
            
            _lastTimeUpdate = Time.time;
            
            if (seasonalGrowth != null)
            {
                // Get time from seasonal system
                float timeOfDay = seasonalGrowth.TimeOfDay;
                int day = seasonalGrowth.CurrentDay;
                
                UpdateTimeText(timeOfDay);
                UpdateDateText(day);
                UpdateDayProgress(timeOfDay);
                UpdateSunMoonIcon(timeOfDay);
            }
            else
            {
                // Fallback to system time
                DateTime now = DateTime.Now;
                UpdateTimeText(now.Hour + now.Minute / 60f + now.Second / 3600f);
                UpdateDateText(now.DayOfYear);
                UpdateDayProgress(now.Hour + now.Minute / 60f);
                UpdateSunMoonIcon(now.Hour + now.Minute / 60f);
            }
        }

        private void UpdateWeatherSystem()
        {
            if (Time.time - _lastWeatherUpdate < weatherUpdateInterval) return;
            
            _lastWeatherUpdate = Time.time;
            
            // Simulate weather changes
            _weatherChangeTimer += weatherUpdateInterval;
            if (_weatherChangeTimer >= _weatherChangeDuration)
            {
                SimulateWeatherChange();
                _weatherChangeTimer = 0f;
            }
            
            // Update weather displays
            UpdateWeatherDisplay();
            UpdateEnvironmentalDisplay();
            UpdateSeasonDisplay();
            
            // Update visual effects
            if (showWeatherEffects)
            {
                UpdateWeatherEffects();
            }
        }

        private void SimulateWeatherChange()
        {
            // 30% chance of weather change
            if (UnityEngine.Random.value < 0.3f)
            {
                SetWeatherBasedOnSeason();
                
                // Trigger weather transition animation
                if (enableWeatherTransitions)
                {
                    StartWeatherTransition();
                }
            }
            
            // Simulate temperature fluctuation
            if (seasonalGrowth != null)
            {
                float baseTemp = seasonalGrowth.CurrentTemperature;
                _currentTemperature = baseTemp + UnityEngine.Random.Range(-3f, 3f);
            }
            
            // Simulate humidity changes
            _currentHumidity += UnityEngine.Random.Range(-0.1f, 0.1f);
            _currentHumidity = Mathf.Clamp01(_currentHumidity);
            
            // Simulate wind
            _currentWindSpeed = UnityEngine.Random.Range(0f, 20f);
            _currentWindDirection = UnityEngine.Random.Range(0f, 360f);
        }

        private void UpdateTimeText(float timeOfDay)
        {
            if (timeText == null) return;
            
            int hours = Mathf.FloorToInt(timeOfDay);
            int minutes = Mathf.FloorToInt((timeOfDay - hours) * 60f);
            int seconds = Mathf.FloorToInt(((timeOfDay - hours) * 60f - minutes) * 60f);
            
            string timeString;
            
            if (show24HourFormat)
            {
                if (showSeconds)
                {
                    timeString = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
                }
                else
                {
                    timeString = $"{hours:D2}:{minutes:D2}";
                }
            }
            else
            {
                string ampm = hours >= 12 ? "PM" : "AM";
                int displayHours = hours == 0 ? 12 : (hours > 12 ? hours - 12 : hours);
                
                if (showSeconds)
                {
                    timeString = $"{displayHours}:{minutes:D2}:{seconds:D2} {ampm}";
                }
                else
                {
                    timeString = $"{displayHours}:{minutes:D2} {ampm}";
                }
            }
            
            timeText.text = timeString;
        }

        private void UpdateDateText(int day)
        {
            if (dateText == null) return;
            
            int year = 1; // Game year
            
            // Calculate which day in the season (assuming 30 days per season)
            Season currentSeason = seasonalGrowth?.CurrentSeason ?? Season.Spring;
            int dayInSeason = (day % 120) % 30 + 1; // 4 seasons * 30 days
            
            dateText.text = $"Day {dayInSeason}, Year {year}";
        }

        private void UpdateDayProgress(float timeOfDay)
        {
            if (dayProgressBar == null) return;
            
            float progress = timeOfDay / 24f;
            dayProgressBar.value = progress;
        }

        private void UpdateSunMoonIcon(float timeOfDay)
        {
            if (sunMoonIcon == null) return;
            
            Sprite iconToUse = timeOfDay switch
            {
                >= 6f and < 8f => sunriseIcon ?? sunIcon,
                >= 8f and < 18f => sunIcon,
                >= 18f and < 20f => sunsetIcon ?? moonIcon,
                _ => moonIcon
            };
            
            if (iconToUse != null)
            {
                sunMoonIcon.sprite = iconToUse;
            }
        }

        private void UpdateWeatherDisplay()
        {
            // Update weather icon
            if (weatherIcon != null)
            {
                weatherIcon.sprite = GetWeatherIcon(_currentWeather);
            }
            
            // Update weather status text
            if (weatherStatusText != null)
            {
                weatherStatusText.text = GetWeatherDescription(_currentWeather);
            }
            
            // Update temperature
            if (temperatureText != null)
            {
                string tempUnit = temperatureUnit == TemperatureUnit.Celsius ? "°C" : "°F";
                float displayTemp = temperatureUnit == TemperatureUnit.Celsius ? 
                    _currentTemperature : (_currentTemperature * 9f / 5f + 32f);
                
                temperatureText.text = $"{displayTemp:F1}{tempUnit}";
            }
            
            // Animate temperature changes
            if (enableTemperatureAnimation && temperatureIcon != null)
            {
                float normalizedTemp = Mathf.InverseLerp(-10f, 40f, _currentTemperature);
                Color tempColor = Color.Lerp(Color.cyan, Color.red, normalizedTemp);
                temperatureIcon.color = Color.Lerp(temperatureIcon.color, tempColor, Time.deltaTime * temperatureAnimationSpeed);
            }
        }

        private void UpdateEnvironmentalDisplay()
        {
            if (!showEnvironmentalData) return;
            
            // Update humidity
            if (humidityBar != null)
            {
                humidityBar.value = _currentHumidity;
            }
            
            if (humidityText != null)
            {
                humidityText.text = $"{_currentHumidity * 100f:F0}%";
            }
            
            // Update wind speed
            if (windSpeedBar != null)
            {
                windSpeedBar.value = _currentWindSpeed / 20f; // Normalize to 0-1
            }
            
            if (windSpeedText != null)
            {
                windSpeedText.text = $"{_currentWindSpeed:F1} km/h";
            }
            
            // Update wind direction
            if (windDirectionIndicator != null)
            {
                windDirectionIndicator.transform.rotation = Quaternion.Euler(0f, 0f, -_currentWindDirection);
            }
        }

        private void UpdateSeasonDisplay()
        {
            if (seasonalGrowth == null) return;
            
            Season currentSeason = seasonalGrowth.CurrentSeason;
            float seasonProgress = seasonalGrowth.SeasonProgress;
            
            // Update season text
            if (seasonText != null)
            {
                seasonText.text = currentSeason.ToString();
            }
            
            // Update season icon
            if (seasonIcon != null)
            {
                seasonIcon.sprite = GetSeasonIcon(currentSeason);
            }
            
            // Update season progress
            if (seasonProgressBar != null)
            {
                seasonProgressBar.value = seasonProgress;
            }
            
            if (seasonProgressText != null)
            {
                seasonProgressText.text = $"{seasonProgress * 100f:F0}%";
            }
        }

        private void UpdateWeatherEffects()
        {
            // Control particle effects based on weather
            bool shouldShowRain = _currentWeather == WeatherType.Rainy || _currentWeather == WeatherType.Stormy;
            bool shouldShowSnow = _currentWeather == WeatherType.Clear && seasonalGrowth?.CurrentSeason == Season.Winter;
            bool shouldShowFog = _currentWeather == WeatherType.Foggy;
            bool shouldShowLightning = _currentWeather == WeatherType.Stormy;
            
            if (rainEffect != null)
            {
                if (shouldShowRain && !rainEffect.isPlaying)
                {
                    rainEffect.Play();
                }
                else if (!shouldShowRain && rainEffect.isPlaying)
                {
                    rainEffect.Stop();
                }
            }
            
            if (snowEffect != null)
            {
                if (shouldShowSnow && !snowEffect.isPlaying)
                {
                    snowEffect.Play();
                }
                else if (!shouldShowSnow && snowEffect.isPlaying)
                {
                    snowEffect.Stop();
                }
            }
            
            if (fogEffect != null)
            {
                if (shouldShowFog && !fogEffect.isPlaying)
                {
                    fogEffect.Play();
                }
                else if (!shouldShowFog && fogEffect.isPlaying)
                {
                    fogEffect.Stop();
                }
            }
            
            if (lightningEffect != null)
            {
                lightningEffect.SetActive(shouldShowLightning);
            }
        }

        private void StartWeatherTransition()
        {
            if (_weatherTransitionCoroutine != null)
            {
                StopCoroutine(_weatherTransitionCoroutine);
            }
            
            _weatherTransitionCoroutine = StartCoroutine(AnimateWeatherTransition());
        }

        private System.Collections.IEnumerator AnimateWeatherTransition()
        {
            WeatherColorTheme targetTheme = GetWeatherColorTheme(_currentWeather);
            WeatherColorTheme startTheme = _currentColorTheme;
            
            float elapsedTime = 0f;
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / transitionDuration;
                
                // Interpolate color theme
                var currentTheme = new WeatherColorTheme
                {
                    backgroundColor = Color.Lerp(startTheme.backgroundColor, targetTheme.backgroundColor, progress),
                    textColor = Color.Lerp(startTheme.textColor, targetTheme.textColor, progress)
                };
                
                ApplyColorTheme(currentTheme);
                
                yield return null;
            }
            
            _currentColorTheme = targetTheme;
            ApplyColorTheme(_currentColorTheme);
        }

        private void ApplyColorTheme(WeatherColorTheme theme)
        {
            // Apply background color to weather container
            if (weatherContainer != null)
            {
                var background = weatherContainer.GetComponent<Image>();
                if (background != null)
                {
                    background.color = theme.backgroundColor;
                }
            }
            
            // Apply text color to weather text elements
            if (weatherStatusText != null)
                weatherStatusText.color = theme.textColor;
            
            if (temperatureText != null)
                temperatureText.color = theme.textColor;
        }

        // Helper methods
        private Sprite GetWeatherIcon(WeatherType weather)
        {
            return weather switch
            {
                WeatherType.Clear => clearIcon,
                WeatherType.Cloudy => cloudyIcon,
                WeatherType.Rainy => rainyIcon,
                WeatherType.Stormy => stormyIcon,
                WeatherType.Sunny => sunnyIcon,
                WeatherType.Overcast => overcastIcon,
                WeatherType.Foggy => foggyIcon,
                WeatherType.Windy => windyIcon,
                _ => clearIcon
            };
        }

        private string GetWeatherDescription(WeatherType weather)
        {
            return weather switch
            {
                WeatherType.Clear => "Clear Sky",
                WeatherType.Cloudy => "Partly Cloudy",
                WeatherType.Rainy => "Rain",
                WeatherType.Stormy => "Thunderstorm",
                WeatherType.Sunny => "Sunny",
                WeatherType.Overcast => "Overcast",
                WeatherType.Foggy => "Foggy",
                WeatherType.Windy => "Windy",
                _ => "Unknown"
            };
        }

        private Sprite GetSeasonIcon(Season season)
        {
            return season switch
            {
                Season.Spring => springIcon,
                Season.Summer => summerIcon,
                Season.Autumn => autumnIcon,
                Season.Winter => winterIcon,
                _ => springIcon
            };
        }

        private WeatherColorTheme GetWeatherColorTheme(WeatherType weather)
        {
            return weather switch
            {
                WeatherType.Clear => clearTheme,
                WeatherType.Rainy => rainyTheme,
                WeatherType.Stormy => stormyTheme,
                WeatherType.Sunny => sunnyTheme,
                _ => clearTheme
            };
        }

        // Event handlers
        private void OnSeasonChanged(Season oldSeason, Season newSeason)
        {
            SetWeatherBasedOnSeason();
        }

        private void OnEnvironmentChanged(float temperature, float moisture, float sunlight)
        {
            _currentTemperature = temperature;
            _currentHumidity = moisture;
        }

        private void OnDayChanged()
        {
            // Chance for weather change on new day
            if (UnityEngine.Random.value < 0.5f)
            {
                SetWeatherBasedOnSeason();
            }
        }

        // Public interface
        public void UpdateWeatherInfo(float temperature, string season, string time)
        {
            _currentTemperature = temperature;
            
            if (temperatureText != null)
            {
                string tempUnit = temperatureUnit == TemperatureUnit.Celsius ? "°C" : "°F";
                float displayTemp = temperatureUnit == TemperatureUnit.Celsius ? 
                    temperature : (temperature * 9f / 5f + 32f);
                temperatureText.text = $"{displayTemp:F1}{tempUnit}";
            }
            
            if (seasonText != null)
            {
                seasonText.text = season;
            }
            
            if (timeText != null)
            {
                timeText.text = time;
            }
        }

        public void SetWeather(WeatherType weather)
        {
            _currentWeather = weather;
            
            if (enableWeatherTransitions)
            {
                StartWeatherTransition();
            }
            else
            {
                UpdateWeatherDisplay();
            }
        }

        public void SetTemperatureUnit(TemperatureUnit unit)
        {
            temperatureUnit = unit;
            UpdateWeatherDisplay();
        }

        public void SetTimeFormat(bool use24Hour, bool showSecondsDisplay)
        {
            show24HourFormat = use24Hour;
            showSeconds = showSecondsDisplay;
        }

        public void SetWeatherEffectsEnabled(bool enabled)
        {
            showWeatherEffects = enabled;
            
            if (!enabled)
            {
                // Stop all weather effects
                if (rainEffect != null && rainEffect.isPlaying) rainEffect.Stop();
                if (snowEffect != null && snowEffect.isPlaying) snowEffect.Stop();
                if (fogEffect != null && fogEffect.isPlaying) fogEffect.Stop();
                if (lightningEffect != null) lightningEffect.SetActive(false);
            }
        }

        public void SetEnvironmentalDataVisible(bool visible)
        {
            showEnvironmentalData = visible;
            
            if (environmentContainer != null)
            {
                environmentContainer.SetActive(visible);
            }
        }

        // Properties
        public WeatherType CurrentWeather => _currentWeather;
        public float CurrentTemperature => _currentTemperature;
        public float CurrentHumidity => _currentHumidity;
        public float CurrentWindSpeed => _currentWindSpeed;
        public float CurrentWindDirection => _currentWindDirection;

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (seasonalGrowth != null)
            {
                seasonalGrowth.OnSeasonChanged -= OnSeasonChanged;
                seasonalGrowth.OnEnvironmentChanged -= OnEnvironmentChanged;
                seasonalGrowth.OnDayChanged -= OnDayChanged;
            }
            
            // Stop all coroutines
            StopAllCoroutines();
        }
    }

    [System.Serializable]
    public class WeatherColorTheme
    {
        public Color backgroundColor = Color.white;
        public Color textColor = Color.black;
    }

    public enum WeatherType
    {
        Clear,
        Cloudy,
        Rainy,
        Stormy,
        Sunny,
        Overcast,
        Foggy,
        Windy
    }

    public enum TemperatureUnit
    {
        Celsius,
        Fahrenheit
    }
}