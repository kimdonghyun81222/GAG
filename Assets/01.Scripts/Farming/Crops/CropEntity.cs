using System;
using GrowAGarden.Core._01.Scripts.Core.Entities;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Crops
{
    public class CropEntity : Entity
    {
        [Header("Crop Settings")]
        [SerializeField] private CropDataSO cropData;
        [SerializeField] private bool autoStartGrowing = true;
        [SerializeField] private bool debugMode = false;
        
        // Growth state
        private CropStageType _currentStage = CropStageType.Seed;
        private int _currentStageIndex = 0;
        private float _currentGrowthTime = 0f;
        private float _totalGrowthTime = 0f;
        private bool _isGrowing = false;
        private bool _isHarvestReady = false;
        private bool _isDead = false;
        
        // Conditions
        private float _waterLevel = 0.5f;
        private float _sunlightLevel = 0.8f;
        private float _soilHealth = 1f;
        private float _lastWateredTime = 0f;
        private CropQuality _quality = CropQuality.Average;
        
        // Weather simulation (독립적)
        private float _currentTemperature = 20f;
        private bool _isRaining = false;
        private CropSeason _currentSeason = CropSeason.Spring;
        
        // Visual
        private GameObject _currentStageObject;
        private Renderer _cropRenderer;
        
        // Properties
        public CropDataSO CropData => cropData;
        public CropStageType CurrentStage => _currentStage;
        public int CurrentStageIndex => _currentStageIndex;
        public float GrowthProgress => _totalGrowthTime > 0 ? _currentGrowthTime / _totalGrowthTime : 0f;
        public float StageProgress => GetCurrentStageProgress();
        public bool IsGrowing => _isGrowing && !_isDead;
        public bool IsHarvestReady => _isHarvestReady && !_isDead;
        public bool IsDead => _isDead;
        public CropQuality Quality => _quality;
        public float WaterLevel => _waterLevel;
        public float SunlightLevel => _sunlightLevel;
        public float SoilHealth => _soilHealth;
        public float CurrentTemperature => _currentTemperature;
        public CropSeason CurrentSeason => _currentSeason;
        
        // Events
        public event Action<CropEntity> OnStageChanged;
        public event Action<CropEntity> OnHarvestReady;
        public event Action<CropEntity> OnCropDied;
        public event Action<CropEntity> OnCropHarvested;
        public event Action<CropEntity, float> OnWatered;

        protected override void Awake()
        {
            base.Awake();
            
            if (cropData != null)
            {
                _totalGrowthTime = cropData.GetTotalGrowthTime(_currentSeason);
            }
            
            // Initialize last watered time
            _lastWateredTime = Time.time;
        }

        protected override void Start()
        {
            base.Start();
            
            if (autoStartGrowing && cropData != null)
            {
                StartGrowing();
            }
        }

        private void Update()
        {
            if (!IsGrowing) return;
            
            UpdateGrowth();
            UpdateConditions();
            UpdateQuality();
        }

        public override void Initialize()
        {
            base.Initialize();
            
            if (cropData != null)
            {
                SetEntityName($"{cropData.cropName} Crop");
                UpdateVisualStage();
            }
        }

        // Growth methods
        public void StartGrowing()
        {
            if (cropData == null || _isDead) return;
            
            _isGrowing = true;
            _currentGrowthTime = 0f;
            _currentStageIndex = 0;
            _currentStage = CropStageType.Seed;
            _isHarvestReady = false;
            
            UpdateVisualStage();
            
            if (debugMode)
            {
                Debug.Log($"Crop {cropData.cropName} started growing");
            }
        }

        public void StopGrowing()
        {
            _isGrowing = false;
            
            if (debugMode)
            {
                Debug.Log($"Crop {cropData.cropName} stopped growing");
            }
        }

        private void UpdateGrowth()
        {
            if (!_isGrowing || _isDead || cropData == null) return;
            
            // Calculate growth rate based on conditions
            float growthRate = CalculateGrowthRate();
            
            // Update growth time
            _currentGrowthTime += Time.deltaTime * growthRate;
            
            // Check for stage progression
            CheckStageProgression();
            
            // Check for harvest readiness
            if (_currentStageIndex >= cropData.growthStages.Count - 1)
            {
                if (!_isHarvestReady)
                {
                    SetHarvestReady();
                }
            }
        }

        private float CalculateGrowthRate()
        {
            float baseRate = 1f;
            
            // Water factor
            float waterFactor = Mathf.Lerp(0.5f, 1.5f, Mathf.Clamp01(_waterLevel / cropData.waterRequirement));
            
            // Sunlight factor
            float sunFactor = Mathf.Lerp(0.5f, 1.5f, Mathf.Clamp01(_sunlightLevel / cropData.sunlightRequirement));
            
            // Temperature factor
            float tempDiff = Mathf.Abs(_currentTemperature - cropData.optimalTemperature);
            float tempFactor = tempDiff <= cropData.temperatureTolerance ? 1f : 
                               Mathf.Lerp(1f, 0.3f, (tempDiff - cropData.temperatureTolerance) / 10f);
            
            // Soil health factor
            float soilFactor = Mathf.Lerp(0.7f, 1.3f, _soilHealth);
            
            // Season factor
            var seasonMod = cropData.seasonModifiers.Find(s => s.season == _currentSeason);
            float seasonFactor = seasonMod?.growthTimeMultiplier ?? 1f;
            
            return baseRate * waterFactor * sunFactor * tempFactor * soilFactor / seasonFactor;
        }

        private void CheckStageProgression()
        {
            if (cropData.growthStages.Count == 0) return;
            
            float accumulatedTime = 0f;
            
            for (int i = 0; i <= _currentStageIndex && i < cropData.growthStages.Count; i++)
            {
                float stageTime = cropData.GetStageTime(i, _currentSeason);
                accumulatedTime += stageTime;
                
                if (_currentGrowthTime >= accumulatedTime && i > _currentStageIndex)
                {
                    AdvanceToStage(i);
                    break;
                }
            }
        }

        private void AdvanceToStage(int newStageIndex)
        {
            if (newStageIndex >= cropData.growthStages.Count) return;
            
            _currentStageIndex = newStageIndex;
            _currentStage = (CropStageType)newStageIndex;
            
            UpdateVisualStage();
            OnStageChanged?.Invoke(this);
            
            if (debugMode)
            {
                Debug.Log($"Crop {cropData.cropName} advanced to stage: {cropData.growthStages[newStageIndex].stageName}");
            }
        }

        private void SetHarvestReady()
        {
            _isHarvestReady = true;
            _isGrowing = false;
            
            OnHarvestReady?.Invoke(this);
            
            if (debugMode)
            {
                Debug.Log($"Crop {cropData.cropName} is ready for harvest");
            }
        }

        // Condition management
        private void UpdateConditions()
        {
            // Simple weather simulation
            UpdateWeatherSimulation();
            
            // Water evaporation
            float evaporationRate = 0.05f * Time.deltaTime; // Reduced base evaporation
            if (_isRaining)
            {
                // Rain adds water
                _waterLevel = Mathf.Min(1f, _waterLevel + cropData.weatherEffects.rainWateringBonus * Time.deltaTime);
                _lastWateredTime = Time.time;
            }
            else
            {
                // Higher temperature increases evaporation
                float tempMultiplier = _currentTemperature > 25f ? 1.5f : 1f;
                evaporationRate *= tempMultiplier;
            }
            
            _waterLevel = Mathf.Max(0f, _waterLevel - evaporationRate);
            
            // Sunlight varies with weather and time of day
            float timeOfDay = (Time.time % 86400f) / 86400f; // 24 hour cycle
            float baseSunlight = Mathf.Sin(timeOfDay * Mathf.PI); // Day/night cycle
            baseSunlight = Mathf.Max(0f, baseSunlight); // No negative sunlight
            
            _sunlightLevel = _isRaining ? baseSunlight * 0.3f : baseSunlight;
            
            // Check for crop death conditions
            CheckDeathConditions();
        }

        private void UpdateWeatherSimulation()
        {
            // Simple weather simulation for standalone operation
            float timeOfDay = (Time.time % 86400f) / 86400f; // 24 hour cycle
            
            // Temperature varies with time of day and season
            float baseTemp = _currentSeason switch
            {
                CropSeason.Spring => 15f,
                CropSeason.Summer => 25f,
                CropSeason.Autumn => 10f,
                CropSeason.Winter => 0f,
                _ => 20f
            };
            
            // Daily temperature variation
            float dailyVariation = Mathf.Sin(timeOfDay * Mathf.PI * 2f) * 8f; // ±8°C variation
            _currentTemperature = baseTemp + dailyVariation;
            
            // Simple rain chance based on time
            float rainChance = Mathf.PerlinNoise(Time.time * 0.001f, 0f);
            _isRaining = rainChance > 0.7f; // 30% chance of rain
        }

        private void CheckDeathConditions()
        {
            if (_isDead) return;
            
            // Drought death
            float timeSinceWatered = Time.time - _lastWateredTime;
            float droughtThresholdSeconds = cropData.weatherEffects.droughtThreshold * 86400f; // Convert days to seconds
            
            if (timeSinceWatered > droughtThresholdSeconds)
            {
                float droughtDays = timeSinceWatered / 86400f;
                float droughtDamage = cropData.weatherEffects.droughtDamagePerDay * droughtDays;
                
                if (droughtDamage > 1f)
                {
                    Die("Drought");
                    return;
                }
            }
            
            // Extreme weather death
            if (_currentTemperature < cropData.weatherEffects.frostDamageThreshold || 
                _currentTemperature > cropData.weatherEffects.heatStressThreshold)
            {
                if (UnityEngine.Random.value < cropData.weatherEffects.extremeWeatherDamage * Time.deltaTime)
                {
                    Die(_currentTemperature < cropData.weatherEffects.frostDamageThreshold ? "Frost" : "Heat Stress");
                    return;
                }
            }
            
            // Check if crop can grow in current season
            if (!cropData.CanGrowInSeason(_currentSeason))
            {
                if (UnityEngine.Random.value < 0.1f * Time.deltaTime) // 10% chance per second to die in wrong season
                {
                    Die("Wrong Season");
                    return;
                }
            }
        }

        private void UpdateQuality()
        {
            if (cropData == null) return;
            
            _quality = cropData.GetQualityForConditions(_waterLevel, _sunlightLevel, _currentTemperature);
        }

        // Interaction methods
        public bool Water(float amount = 0.3f)
        {
            if (_isDead || !CanBeWatered()) return false;
            
            float oldWaterLevel = _waterLevel;
            _waterLevel = Mathf.Min(1f, _waterLevel + amount);
            _lastWateredTime = Time.time;
            
            OnWatered?.Invoke(this, amount);
            
            if (debugMode)
            {
                Debug.Log($"Watered {cropData.cropName}: {oldWaterLevel:F2} → {_waterLevel:F2}");
            }
            
            return true;
        }

        public bool CanBeWatered()
        {
            if (_isDead || cropData == null) return false;
            
            var currentStageData = GetCurrentStageData();
            return currentStageData?.canWater ?? true;
        }

        public CropHarvestResult Harvest()
        {
            if (!CanBeHarvested()) return null;
            
            var result = new CropHarvestResult
            {
                cropData = cropData,
                quality = _quality,
                amount = cropData.GetHarvestAmount(_quality),
                value = cropData.GetSellValue(_quality),
                experience = CalculateExperience()
            };
            
            OnCropHarvested?.Invoke(this);
            
            if (cropData.isRenewableCrop)
            {
                // Reset to previous stage for renewable crops
                _currentStageIndex = Mathf.Max(0, _currentStageIndex - 1);
                _isHarvestReady = false;
                _currentGrowthTime = 0f;
                
                // Calculate new growth time for renewal
                _totalGrowthTime = cropData.renewalTime;
                
                StartGrowing();
            }
            else
            {
                // Destroy crop after harvest
                DestroyEntity();
            }
            
            if (debugMode)
            {
                Debug.Log($"Harvested {result.amount}x {cropData.cropName} ({_quality} quality)");
            }
            
            return result;
        }

        public bool CanBeHarvested()
        {
            if (_isDead || !_isHarvestReady || cropData == null) return false;
            
            var currentStageData = GetCurrentStageData();
            return currentStageData?.canHarvest ?? false;
        }

        private int CalculateExperience()
        {
            int baseExp = 10;
            float qualityMultiplier = _quality switch
            {
                CropQuality.Perfect => 2f,
                CropQuality.Good => 1.5f,
                CropQuality.Average => 1f,
                CropQuality.Poor => 0.5f,
                _ => 1f
            };
            
            return Mathf.RoundToInt(baseExp * qualityMultiplier);
        }

        private void Die(string cause)
        {
            _isDead = true;
            _isGrowing = false;
            _isHarvestReady = false;
            
            UpdateVisualStage(); // Show dead crop visual
            OnCropDied?.Invoke(this);
            
            if (debugMode)
            {
                Debug.Log($"Crop {cropData.cropName} died from: {cause}");
            }
        }

        // Visual updates
        private void UpdateVisualStage()
        {
            if (cropData == null) return;
            
            // Remove current stage object
            if (_currentStageObject != null)
            {
                DestroyImmediate(_currentStageObject);
            }
            
            GameObject stagePrefab = null;
            
            if (_isDead)
            {
                // TODO: Add dead crop prefab
                stagePrefab = null;
            }
            else if (_currentStageIndex < cropData.growthStages.Count)
            {
                stagePrefab = cropData.growthStages[_currentStageIndex].stagePrefab;
            }
            
            if (stagePrefab != null)
            {
                _currentStageObject = Instantiate(stagePrefab, transform);
                _cropRenderer = _currentStageObject.GetComponent<Renderer>();
                
                // Apply quality-based color tinting
                if (_cropRenderer != null)
                {
                    Color qualityColor = _quality switch
                    {
                        CropQuality.Perfect => Color.green,
                        CropQuality.Good => Color.yellow,
                        CropQuality.Average => Color.white,
                        CropQuality.Poor => Color.red,
                        _ => Color.white
                    };
                    
                    _cropRenderer.material.color = qualityColor;
                }
            }
        }

        // Utility methods
        private float GetCurrentStageProgress()
        {
            if (cropData == null || _currentStageIndex >= cropData.growthStages.Count) return 1f;
            
            float stageStartTime = 0f;
            for (int i = 0; i < _currentStageIndex; i++)
            {
                stageStartTime += cropData.GetStageTime(i, _currentSeason);
            }
            
            float stageTime = cropData.GetStageTime(_currentStageIndex, _currentSeason);
            float timeIntoStage = _currentGrowthTime - stageStartTime;
            
            return stageTime > 0 ? Mathf.Clamp01(timeIntoStage / stageTime) : 1f;
        }

        private CropStage GetCurrentStageData()
        {
            if (cropData == null || _currentStageIndex >= cropData.growthStages.Count) return null;
            return cropData.growthStages[_currentStageIndex];
        }

        // Crop data management
        public void SetCropData(CropDataSO newCropData)
        {
            cropData = newCropData;
            if (cropData != null)
            {
                _totalGrowthTime = cropData.GetTotalGrowthTime(_currentSeason);
                SetEntityName($"{cropData.cropName} Crop");
            }
        }

        // Public setters for external weather system integration
        public void SetCurrentTemperature(float temperature)
        {
            _currentTemperature = temperature;
        }

        public void SetCurrentSeason(CropSeason season)
        {
            _currentSeason = season;
            if (cropData != null)
            {
                _totalGrowthTime = cropData.GetTotalGrowthTime(_currentSeason);
            }
        }

        public void SetCurrentSeason(int seasonIndex)
        {
            if (seasonIndex >= 0 && seasonIndex < 4)
            {
                SetCurrentSeason((CropSeason)seasonIndex);
            }
        }

        public void SetIsRaining(bool raining)
        {
            _isRaining = raining;
        }

        public void SetSoilHealth(float health)
        {
            _soilHealth = Mathf.Clamp01(health);
        }

        // Status methods
        public string GetStatusDescription()
        {
            if (_isDead) return "Dead";
            if (_isHarvestReady) return "Ready for Harvest";
            if (_isGrowing) return $"Growing ({GrowthProgress * 100:F0}%)";
            return "Dormant";
        }

        public string GetConditionsDescription()
        {
            return $"Water: {_waterLevel * 100:F0}% | Sun: {_sunlightLevel * 100:F0}% | Temp: {_currentTemperature:F1}°C | Quality: {_quality}";
        }

        // Debug methods
        public void DEBUG_SetGrowthProgress(float progress)
        {
            if (!debugMode || cropData == null) return;
            
            _currentGrowthTime = Mathf.Clamp01(progress) * _totalGrowthTime;
            CheckStageProgression();
        }

        public void DEBUG_SetWaterLevel(float level)
        {
            if (!debugMode) return;
            _waterLevel = Mathf.Clamp01(level);
        }

        public void DEBUG_KillCrop()
        {
            if (!debugMode) return;
            Die("Debug Kill");
        }

        public void DEBUG_MakeHarvestReady()
        {
            if (!debugMode) return;
            _currentStageIndex = cropData.growthStages.Count - 1;
            _currentStage = (CropStageType)_currentStageIndex;
            SetHarvestReady();
        }

        public void DEBUG_SetSeason(CropSeason season)
        {
            if (!debugMode) return;
            SetCurrentSeason(season);
        }
    }
    
    // Enum은 CropEntity 내부에만 정의
    public enum CropStageType
    {
        Seed = 0,
        Sprout = 1,
        Growing = 2,
        Mature = 3,
        Dead = -1
    }
    
    [System.Serializable]
    public class CropHarvestResult
    {
        public CropDataSO cropData;
        public CropQuality quality;
        public int amount;
        public int value;
        public int experience;
        
        public override string ToString()
        {
            return $"{amount}x {cropData?.cropName} ({quality}) - {value} gold, {experience} exp";
        }
    }
}