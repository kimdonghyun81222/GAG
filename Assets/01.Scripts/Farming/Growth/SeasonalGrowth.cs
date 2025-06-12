using System;
using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Growth
{
    [Provide]
    public class SeasonalGrowth : MonoBehaviour, IDependencyProvider
    {
        [Header("Seasonal Settings")]
        [SerializeField] private float seasonLength = 30f; // Days per season
        [SerializeField] private bool autoAdvanceSeasons = true;
        [SerializeField] private bool debugMode = false;
        
        [Header("Season Data")]
        [SerializeField] private List<SeasonData> seasonData = new List<SeasonData>();
        
        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 3f; // Days for smooth transition
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Current state
        private Season _currentSeason = Season.Spring;
        private float _seasonProgress = 0f;
        private float _dayCounter = 0f;
        private bool _isTransitioning = false;
        private float _transitionProgress = 0f;
        private Season _transitionTargetSeason;
        
        // Environmental factors
        private float _currentTemperature = 20f;
        private float _currentMoisture = 0.5f;
        private float _currentSunlight = 0.8f;
        private float _dayLightHours = 12f;
        
        // Properties
        public Season CurrentSeason => _currentSeason;
        public float SeasonProgress => _seasonProgress;
        public bool IsTransitioning => _isTransitioning;
        public float CurrentTemperature => _currentTemperature;
        public float CurrentMoisture => _currentMoisture;
        public float CurrentSunlight => _currentSunlight;
        public float DayLightHours => _dayLightHours;
        public int CurrentDay => Mathf.FloorToInt(_dayCounter);
        public float TimeOfDay => (_dayCounter % 1f) * 24f;
        
        // Events
        public event Action<Season, Season> OnSeasonChanged; // old, new
        public event Action<Season> OnSeasonTransitionStarted;
        public event Action<Season> OnSeasonTransitionCompleted;
        public event Action OnDayChanged;
        public event Action<float, float, float> OnEnvironmentChanged; // temp, moisture, sunlight

        private void Awake()
        {
            InitializeSeasonData();
        }

        private void Start()
        {
            UpdateEnvironmentalFactors();
        }

        private void Update()
        {
            if (autoAdvanceSeasons)
            {
                UpdateSeasonalCycle();
            }
            
            UpdateEnvironmentalFactors();
        }
        
        [Provide]
        public SeasonalGrowth ProvideSeasonalGrowth() => this;

        private void InitializeSeasonData()
        {
            // Create default season data if none exists
            if (seasonData.Count == 0)
            {
                seasonData.Add(CreateDefaultSeasonData(Season.Spring));
                seasonData.Add(CreateDefaultSeasonData(Season.Summer));
                seasonData.Add(CreateDefaultSeasonData(Season.Autumn));
                seasonData.Add(CreateDefaultSeasonData(Season.Winter));
            }
        }

        private SeasonData CreateDefaultSeasonData(Season season)
        {
            return season switch
            {
                Season.Spring => new SeasonData
                {
                    season = Season.Spring,
                    baseTemperature = 18f,
                    temperatureVariation = 8f,
                    baseMoisture = 0.7f,
                    moistureVariation = 0.2f,
                    baseSunlight = 0.75f,
                    sunlightVariation = 0.15f,
                    dayLightHours = 14f,
                    growthMultiplier = 1.2f,
                    seasonColor = new Color(0.5f, 1f, 0.5f)
                },
                Season.Summer => new SeasonData
                {
                    season = Season.Summer,
                    baseTemperature = 28f,
                    temperatureVariation = 6f,
                    baseMoisture = 0.4f,
                    moistureVariation = 0.3f,
                    baseSunlight = 0.9f,
                    sunlightVariation = 0.1f,
                    dayLightHours = 16f,
                    growthMultiplier = 1.0f,
                    seasonColor = new Color(1f, 1f, 0.3f)
                },
                Season.Autumn => new SeasonData
                {
                    season = Season.Autumn,
                    baseTemperature = 15f,
                    temperatureVariation = 10f,
                    baseMoisture = 0.6f,
                    moistureVariation = 0.25f,
                    baseSunlight = 0.6f,
                    sunlightVariation = 0.2f,
                    dayLightHours = 12f,
                    growthMultiplier = 0.8f,
                    seasonColor = new Color(1f, 0.6f, 0.2f)
                },
                Season.Winter => new SeasonData
                {
                    season = Season.Winter,
                    baseTemperature = 5f,
                    temperatureVariation = 12f,
                    baseMoisture = 0.8f,
                    moistureVariation = 0.15f,
                    baseSunlight = 0.4f,
                    sunlightVariation = 0.1f,
                    dayLightHours = 10f,
                    growthMultiplier = 0.3f,
                    seasonColor = new Color(0.7f, 0.9f, 1f)
                },
                _ => new SeasonData()
            };
        }

        private void UpdateSeasonalCycle()
        {
            float lastDayCounter = _dayCounter;
            _dayCounter += Time.deltaTime / 86400f; // Convert seconds to days
            
            // Check for day change
            if (Mathf.FloorToInt(_dayCounter) != Mathf.FloorToInt(lastDayCounter))
            {
                OnDayChanged?.Invoke();
                
                if (debugMode)
                {
                    Debug.Log($"Day {CurrentDay} - Season: {_currentSeason} ({_seasonProgress:P0})");
                }
            }
            
            // Update season progress
            _seasonProgress = (_dayCounter % seasonLength) / seasonLength;
            
            // Check for season change
            Season expectedSeason = GetSeasonForDay(CurrentDay);
            if (expectedSeason != _currentSeason && !_isTransitioning)
            {
                StartSeasonTransition(expectedSeason);
            }
            
            // Update transition
            if (_isTransitioning)
            {
                UpdateSeasonTransition();
            }
        }

        private Season GetSeasonForDay(int day)
        {
            int seasonIndex = Mathf.FloorToInt(day / seasonLength) % 4;
            return (Season)seasonIndex;
        }

        private void StartSeasonTransition(Season targetSeason)
        {
            _isTransitioning = true;
            _transitionProgress = 0f;
            _transitionTargetSeason = targetSeason;
            
            OnSeasonTransitionStarted?.Invoke(targetSeason);
            
            if (debugMode)
            {
                Debug.Log($"Starting transition from {_currentSeason} to {targetSeason}");
            }
        }

        private void UpdateSeasonTransition()
        {
            _transitionProgress += Time.deltaTime / (transitionDuration * 86400f); // Convert to days
            
            if (_transitionProgress >= 1f)
            {
                CompleteSeasonTransition();
            }
        }

        private void CompleteSeasonTransition()
        {
            Season oldSeason = _currentSeason;
            _currentSeason = _transitionTargetSeason;
            _isTransitioning = false;
            _transitionProgress = 0f;
            
            OnSeasonChanged?.Invoke(oldSeason, _currentSeason);
            OnSeasonTransitionCompleted?.Invoke(_currentSeason);
            
            if (debugMode)
            {
                Debug.Log($"Season changed from {oldSeason} to {_currentSeason}");
            }
        }

        private void UpdateEnvironmentalFactors()
        {
            var currentData = GetCurrentSeasonData();
            var targetData = _isTransitioning ? GetSeasonData(_transitionTargetSeason) : currentData;
            
            float blendFactor = _isTransitioning ? transitionCurve.Evaluate(_transitionProgress) : 0f;
            
            // Calculate time-based variations
            float timeOfDay = TimeOfDay;
            float dayProgress = timeOfDay / 24f;
            
            // Temperature varies throughout the day
            float tempVariation = Mathf.Sin(dayProgress * Mathf.PI * 2f - Mathf.PI * 0.5f) * 0.3f;
            
            // Sunlight varies with time of day and season
            float sunlightTimeEffect = CalculateSunlightForTime(timeOfDay, currentData.dayLightHours);
            
            // Apply base values with variations
            float baseTemp = Mathf.Lerp(currentData.baseTemperature, targetData.baseTemperature, blendFactor);
            float baseMoist = Mathf.Lerp(currentData.baseMoisture, targetData.baseMoisture, blendFactor);
            float baseSun = Mathf.Lerp(currentData.baseSunlight, targetData.baseSunlight, blendFactor);
            
            // Apply variations
            _currentTemperature = baseTemp + tempVariation * currentData.temperatureVariation;
            _currentMoisture = Mathf.Clamp01(baseMoist + UnityEngine.Random.Range(-currentData.moistureVariation, currentData.moistureVariation) * 0.1f);
            _currentSunlight = Mathf.Clamp01(baseSun * sunlightTimeEffect);
            
            // Daylight hours
            _dayLightHours = Mathf.Lerp(currentData.dayLightHours, targetData.dayLightHours, blendFactor);
            
            OnEnvironmentChanged?.Invoke(_currentTemperature, _currentMoisture, _currentSunlight);
        }

        private float CalculateSunlightForTime(float timeOfDay, float dayLightHours)
        {
            float sunriseHour = 12f - dayLightHours * 0.5f;
            float sunsetHour = 12f + dayLightHours * 0.5f;
            
            if (timeOfDay < sunriseHour || timeOfDay > sunsetHour)
            {
                return 0f; // Night
            }
            
            // Calculate sun position (0 at sunrise/sunset, 1 at noon)
            float dayProgress = (timeOfDay - sunriseHour) / dayLightHours;
            return Mathf.Sin(dayProgress * Mathf.PI);
        }

        private SeasonData GetCurrentSeasonData()
        {
            return GetSeasonData(_currentSeason);
        }

        private SeasonData GetSeasonData(Season season)
        {
            foreach (var data in seasonData)
            {
                if (data.season == season)
                {
                    return data;
                }
            }
            
            // Return default if not found
            return CreateDefaultSeasonData(season);
        }

        // Public methods
        public void SetSeason(Season season)
        {
            if (_currentSeason != season)
            {
                Season oldSeason = _currentSeason;
                _currentSeason = season;
                _seasonProgress = 0f;
                _isTransitioning = false;
                
                OnSeasonChanged?.Invoke(oldSeason, _currentSeason);
                
                if (debugMode)
                {
                    Debug.Log($"Season manually set to {season}");
                }
            }
        }

        public void AdvanceSeason()
        {
            Season nextSeason = GetNextSeason(_currentSeason);
            StartSeasonTransition(nextSeason);
        }

        private Season GetNextSeason(Season current)
        {
            return current switch
            {
                Season.Spring => Season.Summer,
                Season.Summer => Season.Autumn,
                Season.Autumn => Season.Winter,
                Season.Winter => Season.Spring,
                _ => Season.Spring
            };
        }

        public void SetSeasonLength(float days)
        {
            seasonLength = Mathf.Max(1f, days);
        }

        public void SetDay(int day)
        {
            _dayCounter = day;
            Season expectedSeason = GetSeasonForDay(day);
            
            if (expectedSeason != _currentSeason)
            {
                SetSeason(expectedSeason);
            }
        }

        public void SetTimeOfDay(float hours)
        {
            float dayPart = hours / 24f;
            _dayCounter = CurrentDay + dayPart;
        }

        public float GetGrowthMultiplierForSeason(Season season)
        {
            var data = GetSeasonData(season);
            return data.growthMultiplier;
        }

        public float GetCurrentGrowthMultiplier()
        {
            var currentData = GetCurrentSeasonData();
            
            if (_isTransitioning)
            {
                var targetData = GetSeasonData(_transitionTargetSeason);
                return Mathf.Lerp(currentData.growthMultiplier, targetData.growthMultiplier, _transitionProgress);
            }
            
            return currentData.growthMultiplier;
        }

        public Color GetCurrentSeasonColor()
        {
            var currentData = GetCurrentSeasonData();
            
            if (_isTransitioning)
            {
                var targetData = GetSeasonData(_transitionTargetSeason);
                return Color.Lerp(currentData.seasonColor, targetData.seasonColor, _transitionProgress);
            }
            
            return currentData.seasonColor;
        }

        public bool IsOptimalSeasonForCrop(Season cropPreferredSeason)
        {
            return _currentSeason == cropPreferredSeason;
        }

        public float GetSeasonalCropModifier(Season cropPreferredSeason)
        {
            if (IsOptimalSeasonForCrop(cropPreferredSeason))
                return 1.2f; // 20% bonus in optimal season
            
            // Calculate season distance (how far from optimal)
            int currentIndex = (int)_currentSeason;
            int preferredIndex = (int)cropPreferredSeason;
            int distance = Mathf.Min(Mathf.Abs(currentIndex - preferredIndex), 4 - Mathf.Abs(currentIndex - preferredIndex));
            
            return distance switch
            {
                0 => 1.2f,  // Optimal season
                1 => 1.0f,  // Adjacent season
                2 => 0.7f,  // Opposite season
                _ => 0.8f   // Fallback
            };
        }

        // Weather simulation
        public void SimulateWeather()
        {
            var currentData = GetCurrentSeasonData();
            
            // Add some randomness to environmental factors
            _currentMoisture += UnityEngine.Random.Range(-0.1f, 0.1f);
            _currentMoisture = Mathf.Clamp01(_currentMoisture);
            
            _currentTemperature += UnityEngine.Random.Range(-2f, 2f);
            
            // Seasonal moisture adjustments
            if (_currentSeason == Season.Summer)
            {
                _currentMoisture *= 0.95f; // Gradual drying
            }
            else if (_currentSeason == Season.Winter)
            {
                _currentMoisture = Mathf.Min(1f, _currentMoisture * 1.02f); // Gradual moisture increase
            }
        }

        // Debug methods
        public void DEBUG_SetSeason(Season season)
        {
            if (!debugMode) return;
            SetSeason(season);
        }

        public void DEBUG_AdvanceDay()
        {
            if (!debugMode) return;
            _dayCounter += 1f;
        }

        public void DEBUG_AdvanceHour()
        {
            if (!debugMode) return;
            _dayCounter += 1f / 24f;
        }

        public void DEBUG_PrintSeasonInfo()
        {
            if (!debugMode) return;
            
            Debug.Log($"Season Info:\n" +
                     $"Current: {_currentSeason} ({_seasonProgress:P0})\n" +
                     $"Day: {CurrentDay}\n" +
                     $"Time: {TimeOfDay:F1}h\n" +
                     $"Temp: {_currentTemperature:F1}°C\n" +
                     $"Moisture: {_currentMoisture:P0}\n" +
                     $"Sunlight: {_currentSunlight:P0}\n" +
                     $"Growth Multiplier: {GetCurrentGrowthMultiplier():F2}x");
        }
    }

    [System.Serializable]
    public class SeasonData
    {
        [Header("Season Identity")]
        public Season season;
        public string seasonName;
        public Color seasonColor = Color.white;
        
        [Header("Temperature")]
        public float baseTemperature = 20f;
        public float temperatureVariation = 5f;
        
        [Header("Moisture")]
        public float baseMoisture = 0.5f;
        public float moistureVariation = 0.2f;
        
        [Header("Sunlight")]
        public float baseSunlight = 0.8f;
        public float sunlightVariation = 0.1f;
        public float dayLightHours = 12f;
        
        [Header("Growth Effects")]
        public float growthMultiplier = 1f;
        public List<SeasonalEffect> seasonalEffects = new List<SeasonalEffect>();
        
        [Header("Weather Patterns")]
        public List<WeatherPattern> weatherPatterns = new List<WeatherPattern>();
    }

    [System.Serializable]
    public class SeasonalEffect
    {
        public string effectName;
        public GrowthEffectType effectType;
        public float effectValue;
        public bool isPercentage = true;
    }

    [System.Serializable]
    public class WeatherPattern
    {
        public WeatherType weatherType;
        public float probability = 0.1f;
        public float duration = 1f; // Days
        public float intensity = 1f;
    }

    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }
}