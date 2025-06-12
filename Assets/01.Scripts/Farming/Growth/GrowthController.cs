using System;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Farming._01.Scripts.Farming.Crops;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Growth
{
    [Provide]
    public class GrowthController : MonoBehaviour, IDependencyProvider
    {
        [Header("Growth Management")]
        [SerializeField] private List<GrowthModifier> globalModifiers = new List<GrowthModifier>();
        [SerializeField] private bool enableGrowthProcessing = true;
        [SerializeField] private float updateInterval = 1f; // Seconds between updates
        [SerializeField] private bool debugMode = false;
        
        [Header("Performance")]
        [SerializeField] private int maxCropsPerFrame = 50;
        [SerializeField] private bool useFrameDistribution = true;
        
        // Dependencies
        [Inject] private SeasonalGrowth seasonalGrowth;
        
        // Managed crops
        private List<CropEntity> _managedCrops = new List<CropEntity>();
        private Dictionary<CropEntity, List<ActiveModifier>> _cropModifiers = new Dictionary<CropEntity, List<ActiveModifier>>();
        private Queue<CropEntity> _updateQueue = new Queue<CropEntity>();
        
        // Timing
        private float _lastUpdateTime;
        private int _cropsProcessedThisFrame;
        
        // Properties
        public int ManagedCropCount => _managedCrops.Count;
        public bool GrowthProcessingEnabled => enableGrowthProcessing;
        public float UpdateInterval => updateInterval;
        
        // Events
        public event Action<CropEntity> OnCropRegistered;
        public event Action<CropEntity> OnCropUnregistered;
        public event Action<CropEntity, GrowthModifier> OnModifierApplied;
        public event Action<CropEntity, GrowthModifier> OnModifierRemoved;
        public event Action<CropEntity, float> OnGrowthRateCalculated;

        private void Awake()
        {
            _lastUpdateTime = Time.time;
        }

        private void Start()
        {
            if (seasonalGrowth == null)
            {
                seasonalGrowth = FindObjectOfType<SeasonalGrowth>();
                if (seasonalGrowth == null && debugMode)
                {
                    Debug.LogWarning("SeasonalGrowth not found! Some growth features may not work.");
                }
            }
        }

        private void Update()
        {
            if (!enableGrowthProcessing) return;
            
            if (Time.time - _lastUpdateTime >= updateInterval)
            {
                ProcessGrowthUpdates();
                _lastUpdateTime = Time.time;
            }
            
            if (useFrameDistribution)
            {
                ProcessDistributedUpdates();
            }
        }
        
        [Provide]
        public GrowthController ProvideGrowthController() => this;

        // Crop management
        public void RegisterCrop(CropEntity crop)
        {
            if (crop == null || _managedCrops.Contains(crop)) return;
            
            _managedCrops.Add(crop);
            _cropModifiers[crop] = new List<ActiveModifier>();
            
            // Apply global modifiers
            foreach (var modifier in globalModifiers)
            {
                ApplyModifierToCrop(crop, modifier, isGlobal: true);
            }
            
            OnCropRegistered?.Invoke(crop);
            
            if (debugMode)
            {
                Debug.Log($"Registered crop: {crop.CropData?.cropName ?? "Unknown"}");
            }
        }

        public void UnregisterCrop(CropEntity crop)
        {
            if (crop == null || !_managedCrops.Contains(crop)) return;
            
            _managedCrops.Remove(crop);
            
            // Remove all modifiers
            if (_cropModifiers.ContainsKey(crop))
            {
                _cropModifiers[crop].Clear();
                _cropModifiers.Remove(crop);
            }
            
            OnCropUnregistered?.Invoke(crop);
            
            if (debugMode)
            {
                Debug.Log($"Unregistered crop: {crop.CropData?.cropName ?? "Unknown"}");
            }
        }

        // Growth processing
        private void ProcessGrowthUpdates()
        {
            _cropsProcessedThisFrame = 0;
            
            // Fill update queue if empty
            if (_updateQueue.Count == 0)
            {
                foreach (var crop in _managedCrops)
                {
                    if (crop != null && crop.IsGrowing)
                    {
                        _updateQueue.Enqueue(crop);
                    }
                }
            }
            
            // Process crops in queue
            while (_updateQueue.Count > 0 && _cropsProcessedThisFrame < maxCropsPerFrame)
            {
                var crop = _updateQueue.Dequeue();
                if (crop != null && crop.IsGrowing)
                {
                    ProcessCropGrowth(crop);
                    _cropsProcessedThisFrame++;
                }
            }
        }

        private void ProcessDistributedUpdates()
        {
            // Additional processing for frame distribution
            const int maxAdditionalPerFrame = 10;
            int additionalProcessed = 0;
            
            foreach (var crop in _managedCrops)
            {
                if (additionalProcessed >= maxAdditionalPerFrame) break;
                
                if (crop != null && crop.IsGrowing)
                {
                    UpdateCropModifiers(crop);
                    additionalProcessed++;
                }
            }
        }

        private void ProcessCropGrowth(CropEntity crop)
        {
            if (crop?.CropData == null) return;
            
            // Create growth context
            var context = CreateGrowthContext(crop);
            
            // Calculate growth rate with all modifiers
            float growthRate = CalculateGrowthRate(crop, context);
            
            // Apply growth rate to crop (would need to be implemented in CropEntity)
            // For now, just fire event
            OnGrowthRateCalculated?.Invoke(crop, growthRate);
            
            if (debugMode && UnityEngine.Random.value < 0.01f) // Log occasionally
            {
                Debug.Log($"Crop {crop.CropData.cropName} growth rate: {growthRate:F2}x");
            }
        }

        private GrowthContext CreateGrowthContext(CropEntity crop)
        {
            float temperature = seasonalGrowth?.CurrentTemperature ?? crop.CurrentTemperature;
            float moisture = seasonalGrowth?.CurrentMoisture ?? 0.5f;
            float sunlight = seasonalGrowth?.CurrentSunlight ?? 0.8f;
            float soilHealth = crop.SoilHealth;
            int season = seasonalGrowth != null ? (int)seasonalGrowth.CurrentSeason : 0;
            
            var context = new GrowthContext(temperature, moisture, sunlight, soilHealth, season);
            
            if (seasonalGrowth != null)
            {
                context.timeOfDay = seasonalGrowth.TimeOfDay;
                context.weather = WeatherType.Clear; // Default weather
            }
            
            // Map crop stage to growth stage
            context.currentStage = MapCropStageToGrowthStage(crop.CurrentStage);
            context.stageProgress = crop.StageProgress;
            
            return context;
        }

        private GrowthStage MapCropStageToGrowthStage(Crops.CropStageType cropStage)
        {
            return cropStage switch
            {
                Crops.CropStageType.Seed => GrowthStage.Seed,
                Crops.CropStageType.Sprout => GrowthStage.Seedling,
                Crops.CropStageType.Growing => GrowthStage.Vegetative,
                Crops.CropStageType.Mature => GrowthStage.Mature,
                _ => GrowthStage.Vegetative
            };
        }

        private float CalculateGrowthRate(CropEntity crop, GrowthContext context)
        {
            float baseRate = 1f;
            
            // Apply seasonal growth multiplier
            if (seasonalGrowth != null)
            {
                baseRate *= seasonalGrowth.GetCurrentGrowthMultiplier();
            }
            
            // Apply all active modifiers
            if (_cropModifiers.ContainsKey(crop))
            {
                foreach (var activeModifier in _cropModifiers[crop])
                {
                    if (activeModifier.IsActive && activeModifier.modifier.CanApplyToStage(context.currentStage))
                    {
                        float growthEffect = activeModifier.modifier.CalculateTotalEffect(GrowthEffectType.GrowthRate, context);
                        baseRate *= (1f + growthEffect);
                    }
                }
            }
            
            // Environmental factors
            baseRate *= CalculateEnvironmentalFactor(crop, context);
            
            return Mathf.Max(0.1f, baseRate); // Minimum 10% growth rate
        }

        private float CalculateEnvironmentalFactor(CropEntity crop, GrowthContext context)
        {
            if (crop?.CropData == null) return 1f;
            
            float factor = 1f;
            
            // Temperature factor
            float tempOptimal = crop.CropData.optimalTemperature;
            float tempTolerance = crop.CropData.temperatureTolerance;
            float tempDiff = Mathf.Abs(context.temperature - tempOptimal);
            
            if (tempDiff > tempTolerance)
            {
                factor *= Mathf.Lerp(1f, 0.3f, (tempDiff - tempTolerance) / 20f);
            }
            
            // Water factor
            float waterOptimal = crop.CropData.waterRequirement;
            float waterDiff = Mathf.Abs(context.moisture - waterOptimal);
            factor *= Mathf.Lerp(1f, 0.5f, waterDiff);
            
            // Sunlight factor
            float sunOptimal = crop.CropData.sunlightRequirement;
            float sunDiff = Mathf.Abs(context.sunlight - sunOptimal);
            factor *= Mathf.Lerp(1f, 0.6f, sunDiff);
            
            // Soil health factor
            factor *= Mathf.Lerp(0.5f, 1.2f, context.soilHealth);
            
            return factor;
        }

        // Modifier management
        public void ApplyModifierToCrop(CropEntity crop, GrowthModifier modifier, float duration = 0f, bool isGlobal = false)
        {
            if (crop == null || modifier == null) return;
            if (!_cropModifiers.ContainsKey(crop)) return;
            
            // Check if modifier already exists
            var existingModifier = _cropModifiers[crop].FirstOrDefault(m => m.modifier == modifier);
            
            if (existingModifier != null)
            {
                if (modifier.stackable && existingModifier.stackCount < modifier.maxStacks)
                {
                    existingModifier.stackCount++;
                    existingModifier.RefreshDuration(duration > 0 ? duration : modifier.duration);
                }
                else
                {
                    existingModifier.RefreshDuration(duration > 0 ? duration : modifier.duration);
                }
            }
            else
            {
                var activeModifier = new ActiveModifier(modifier, duration > 0 ? duration : modifier.duration, isGlobal);
                _cropModifiers[crop].Add(activeModifier);
            }
            
            OnModifierApplied?.Invoke(crop, modifier);
            
            if (debugMode)
            {
                Debug.Log($"Applied modifier {modifier.modifierName} to {crop.CropData?.cropName ?? "Unknown"}");
            }
        }

        public void RemoveModifierFromCrop(CropEntity crop, GrowthModifier modifier)
        {
            if (crop == null || modifier == null) return;
            if (!_cropModifiers.ContainsKey(crop)) return;
            
            var activeModifier = _cropModifiers[crop].FirstOrDefault(m => m.modifier == modifier);
            if (activeModifier != null)
            {
                _cropModifiers[crop].Remove(activeModifier);
                OnModifierRemoved?.Invoke(crop, modifier);
                
                if (debugMode)
                {
                    Debug.Log($"Removed modifier {modifier.modifierName} from {crop.CropData?.cropName ?? "Unknown"}");
                }
            }
        }

        public void ApplyGlobalModifier(GrowthModifier modifier, float duration = 0f)
        {
            if (modifier == null) return;
            
            if (!globalModifiers.Contains(modifier))
            {
                globalModifiers.Add(modifier);
            }
            
            // Apply to all registered crops
            foreach (var crop in _managedCrops)
            {
                ApplyModifierToCrop(crop, modifier, duration, isGlobal: true);
            }
            
            if (debugMode)
            {
                Debug.Log($"Applied global modifier: {modifier.modifierName}");
            }
        }

        public void RemoveGlobalModifier(GrowthModifier modifier)
        {
            if (modifier == null) return;
            
            globalModifiers.Remove(modifier);
            
            // Remove from all crops
            foreach (var crop in _managedCrops)
            {
                RemoveModifierFromCrop(crop, modifier);
            }
            
            if (debugMode)
            {
                Debug.Log($"Removed global modifier: {modifier.modifierName}");
            }
        }

        private void UpdateCropModifiers(CropEntity crop)
        {
            if (!_cropModifiers.ContainsKey(crop)) return;
            
            var modifiers = _cropModifiers[crop];
            var expiredModifiers = new List<ActiveModifier>();
            
            foreach (var modifier in modifiers)
            {
                modifier.Update(Time.deltaTime);
                
                if (!modifier.IsActive && !modifier.isGlobal)
                {
                    expiredModifiers.Add(modifier);
                }
            }
            
            // Remove expired modifiers
            foreach (var expired in expiredModifiers)
            {
                modifiers.Remove(expired);
                OnModifierRemoved?.Invoke(crop, expired.modifier);
            }
        }

        // Query methods
        public List<CropEntity> GetManagedCrops()
        {
            return new List<CropEntity>(_managedCrops);
        }

        public List<ActiveModifier> GetCropModifiers(CropEntity crop)
        {
            if (_cropModifiers.ContainsKey(crop))
            {
                return new List<ActiveModifier>(_cropModifiers[crop]);
            }
            return new List<ActiveModifier>();
        }

        public float GetEffectiveGrowthRate(CropEntity crop)
        {
            if (crop == null) return 1f;
            
            var context = CreateGrowthContext(crop);
            return CalculateGrowthRate(crop, context);
        }

        public bool HasModifier(CropEntity crop, GrowthModifier modifier)
        {
            if (!_cropModifiers.ContainsKey(crop)) return false;
            return _cropModifiers[crop].Any(m => m.modifier == modifier);
        }

        // Settings
        public void SetGrowthProcessingEnabled(bool enabled)
        {
            enableGrowthProcessing = enabled;
        }

        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.1f, interval);
        }

        public void SetMaxCropsPerFrame(int maxCrops)
        {
            maxCropsPerFrame = Mathf.Max(1, maxCrops);
        }

        // Debug methods
        public void DEBUG_ApplyRandomModifier(CropEntity crop)
        {
            if (!debugMode || crop == null || globalModifiers.Count == 0) return;
            
            var randomModifier = globalModifiers[UnityEngine.Random.Range(0, globalModifiers.Count)];
            ApplyModifierToCrop(crop, randomModifier, UnityEngine.Random.Range(10f, 60f));
        }

        public void DEBUG_PrintCropInfo(CropEntity crop)
        {
            if (!debugMode || crop == null) return;
            
            var context = CreateGrowthContext(crop);
            float growthRate = CalculateGrowthRate(crop, context);
            var modifiers = GetCropModifiers(crop);
            
            Debug.Log($"Crop Info: {crop.CropData?.cropName}\n" +
                     $"Growth Rate: {growthRate:F2}x\n" +
                     $"Temperature: {context.temperature:F1}°C\n" +
                     $"Moisture: {context.moisture:P0}\n" +
                     $"Sunlight: {context.sunlight:P0}\n" +
                     $"Active Modifiers: {modifiers.Count}");
        }

        public void DEBUG_ClearAllModifiers()
        {
            if (!debugMode) return;
            
            foreach (var crop in _managedCrops)
            {
                _cropModifiers[crop].Clear();
            }
            
            Debug.Log("Cleared all modifiers from all crops");
        }
    }

    [System.Serializable]
    public class ActiveModifier
    {
        public GrowthModifier modifier;
        public float remainingDuration;
        public float originalDuration;
        public int stackCount;
        public bool isGlobal;
        
        public bool IsActive => remainingDuration > 0f || originalDuration <= 0f; // Permanent if duration is 0
        public float Progress => originalDuration > 0 ? (originalDuration - remainingDuration) / originalDuration : 1f;
        
        public ActiveModifier(GrowthModifier mod, float duration, bool global = false)
        {
            modifier = mod;
            remainingDuration = duration;
            originalDuration = duration;
            stackCount = 1;
            isGlobal = global;
        }
        
        public void Update(float deltaTime)
        {
            if (remainingDuration > 0f)
            {
                remainingDuration -= deltaTime;
            }
        }
        
        public void RefreshDuration(float newDuration)
        {
            remainingDuration = newDuration;
            originalDuration = newDuration;
        }
        
        public float GetEffectiveValue(GrowthEffectType effectType)
        {
            float baseValue = modifier.CalculateTotalEffect(effectType, new GrowthContext(20f, 0.5f, 0.8f, 1f, 0));
            return baseValue * stackCount;
        }
    }
}