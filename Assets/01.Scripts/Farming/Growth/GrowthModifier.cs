using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Growth
{
    [CreateAssetMenu(fileName = "GrowthModifier", menuName = "GrowAGarden/Farming/Growth Modifier")]
    public class GrowthModifier : ScriptableObject
    {
        [Header("Modifier Information")]
        public string modifierId;
        public string modifierName = "Growth Modifier";
        public string description = "Affects crop growth";
        public GrowthModifierType modifierType = GrowthModifierType.Environmental;
        
        [Header("Growth Effects")]
        public List<GrowthEffect> effects = new List<GrowthEffect>();
        
        [Header("Application")]
        public GrowthApplicationMode applicationMode = GrowthApplicationMode.Continuous;
        public float duration = 0f; // 0 = permanent
        public float intensity = 1f;
        public bool stackable = false;
        public int maxStacks = 1;
        
        [Header("Conditions")]
        public List<GrowthCondition> conditions = new List<GrowthCondition>();
        public bool requireAllConditions = true;
        
        [Header("Timing")]
        public List<GrowthStage> applicableStages = new List<GrowthStage>();
        public bool applyToAllStages = true;
        
        [Header("Visual Effects")]
        public Color modifierColor = Color.white;
        public Sprite modifierIcon;
        public ParticleSystem effectParticles;
        
        // Validation
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(modifierId))
                modifierId = modifierName?.Replace(" ", "_").ToLower() ?? "unknown_modifier";
            
            intensity = Mathf.Max(0f, intensity);
            duration = Mathf.Max(0f, duration);
            maxStacks = Mathf.Max(1, maxStacks);
            
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    effect.Validate();
                }
            }
        }
        
        // Utility methods
        public bool CanApplyToStage(GrowthStage stage)
        {
            if (applyToAllStages) return true;
            return applicableStages.Contains(stage);
        }
        
        public bool ConditionsAreMet(GrowthContext context)
        {
            if (conditions.Count == 0) return true;
            
            bool allMet = true;
            bool anyMet = false;
            
            foreach (var condition in conditions)
            {
                bool conditionMet = EvaluateCondition(condition, context);
                
                if (conditionMet)
                    anyMet = true;
                else
                    allMet = false;
            }
            
            return requireAllConditions ? allMet : anyMet;
        }
        
        private bool EvaluateCondition(GrowthCondition condition, GrowthContext context)
        {
            return condition.conditionType switch
            {
                GrowthConditionType.Temperature => EvaluateTemperatureCondition(condition, context.temperature),
                GrowthConditionType.Moisture => EvaluateMoistureCondition(condition, context.moisture),
                GrowthConditionType.Sunlight => EvaluateSunlightCondition(condition, context.sunlight),
                GrowthConditionType.SoilHealth => EvaluateSoilHealthCondition(condition, context.soilHealth),
                GrowthConditionType.Season => EvaluateSeasonCondition(condition, context.season),
                GrowthConditionType.TimeOfDay => EvaluateTimeCondition(condition, context.timeOfDay),
                GrowthConditionType.Weather => EvaluateWeatherCondition(condition, context.weather),
                _ => true
            };
        }
        
        private bool EvaluateTemperatureCondition(GrowthCondition condition, float temperature)
        {
            return condition.comparisonType switch
            {
                ComparisonType.GreaterThan => temperature > condition.value,
                ComparisonType.LessThan => temperature < condition.value,
                ComparisonType.EqualTo => Mathf.Abs(temperature - condition.value) < 0.1f,
                ComparisonType.Between => temperature >= condition.value && temperature <= condition.maxValue,
                _ => true
            };
        }
        
        private bool EvaluateMoistureCondition(GrowthCondition condition, float moisture)
        {
            return condition.comparisonType switch
            {
                ComparisonType.GreaterThan => moisture > condition.value,
                ComparisonType.LessThan => moisture < condition.value,
                ComparisonType.EqualTo => Mathf.Abs(moisture - condition.value) < 0.01f,
                ComparisonType.Between => moisture >= condition.value && moisture <= condition.maxValue,
                _ => true
            };
        }
        
        private bool EvaluateSunlightCondition(GrowthCondition condition, float sunlight)
        {
            return condition.comparisonType switch
            {
                ComparisonType.GreaterThan => sunlight > condition.value,
                ComparisonType.LessThan => sunlight < condition.value,
                ComparisonType.EqualTo => Mathf.Abs(sunlight - condition.value) < 0.01f,
                ComparisonType.Between => sunlight >= condition.value && sunlight <= condition.maxValue,
                _ => true
            };
        }
        
        private bool EvaluateSoilHealthCondition(GrowthCondition condition, float soilHealth)
        {
            return condition.comparisonType switch
            {
                ComparisonType.GreaterThan => soilHealth > condition.value,
                ComparisonType.LessThan => soilHealth < condition.value,
                ComparisonType.EqualTo => Mathf.Abs(soilHealth - condition.value) < 0.01f,
                ComparisonType.Between => soilHealth >= condition.value && soilHealth <= condition.maxValue,
                _ => true
            };
        }
        
        private bool EvaluateSeasonCondition(GrowthCondition condition, int season)
        {
            return season == (int)condition.value;
        }
        
        private bool EvaluateTimeCondition(GrowthCondition condition, float timeOfDay)
        {
            return condition.comparisonType switch
            {
                ComparisonType.GreaterThan => timeOfDay > condition.value,
                ComparisonType.LessThan => timeOfDay < condition.value,
                ComparisonType.Between => timeOfDay >= condition.value && timeOfDay <= condition.maxValue,
                _ => true
            };
        }
        
        private bool EvaluateWeatherCondition(GrowthCondition condition, WeatherType weather)
        {
            return weather == (WeatherType)condition.value;
        }
        
        public float CalculateTotalEffect(GrowthEffectType effectType, GrowthContext context)
        {
            float totalEffect = 0f;
            
            if (!ConditionsAreMet(context)) return totalEffect;
            
            foreach (var effect in effects)
            {
                if (effect.effectType == effectType)
                {
                    float effectValue = effect.value * intensity;
                    
                    if (effect.isPercentage)
                        effectValue = effectValue / 100f;
                    
                    totalEffect += effectValue;
                }
            }
            
            return totalEffect;
        }
        
        public string GetModifierDescription()
        {
            string desc = description + "\n\n";
            desc += $"Type: {modifierType}\n";
            desc += $"Application: {applicationMode}\n";
            
            if (duration > 0)
                desc += $"Duration: {duration:F1}s\n";
            
            desc += $"Intensity: {intensity:F1}x\n\n";
            
            desc += "Effects:\n";
            foreach (var effect in effects)
            {
                desc += $"• {effect.GetDescription()}\n";
            }
            
            if (conditions.Count > 0)
            {
                desc += "\nConditions:\n";
                foreach (var condition in conditions)
                {
                    desc += $"• {condition.GetDescription()}\n";
                }
            }
            
            return desc;
        }
    }
    
    [System.Serializable]
    public class GrowthEffect
    {
        [Header("Effect Details")]
        public GrowthEffectType effectType;
        public float value;
        public bool isPercentage = true;
        
        [Header("Application")]
        public EffectApplicationType applicationType = EffectApplicationType.Multiplicative;
        public AnimationCurve effectCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        
        public void Validate()
        {
            if (isPercentage)
            {
                value = Mathf.Clamp(value, -100f, 1000f);
            }
        }
        
        public string GetDescription()
        {
            string prefix = value >= 0 ? "+" : "";
            string suffix = isPercentage ? "%" : "";
            string effectName = effectType.ToString();
            
            return $"{effectName}: {prefix}{value:F1}{suffix}";
        }
        
        public float ApplyEffect(float baseValue, float normalizedTime = 1f)
        {
            float curveMultiplier = effectCurve.Evaluate(normalizedTime);
            float effectiveValue = value * curveMultiplier;
            
            return applicationType switch
            {
                EffectApplicationType.Additive => baseValue + effectiveValue,
                EffectApplicationType.Multiplicative => baseValue * (1f + (isPercentage ? effectiveValue / 100f : effectiveValue)),
                EffectApplicationType.Override => effectiveValue,
                _ => baseValue
            };
        }
    }
    
    [System.Serializable]
    public class GrowthCondition
    {
        public GrowthConditionType conditionType;
        public ComparisonType comparisonType;
        public float value;
        public float maxValue; // For Between comparisons
        
        public string GetDescription()
        {
            string conditionName = conditionType.ToString();
            string comparison = comparisonType switch
            {
                ComparisonType.GreaterThan => ">",
                ComparisonType.LessThan => "<",
                ComparisonType.EqualTo => "=",
                ComparisonType.Between => "between",
                _ => "?"
            };
            
            if (comparisonType == ComparisonType.Between)
            {
                return $"{conditionName} {comparison} {value:F1} and {maxValue:F1}";
            }
            else
            {
                return $"{conditionName} {comparison} {value:F1}";
            }
        }
    }
    
    [System.Serializable]
    public class GrowthContext
    {
        public float temperature;
        public float moisture;
        public float sunlight;
        public float soilHealth;
        public int season;
        public float timeOfDay; // 0-24 hours
        public WeatherType weather;
        public GrowthStage currentStage;
        public float stageProgress; // 0-1
        
        public GrowthContext(float temp, float moist, float sun, float soil, int seas)
        {
            temperature = temp;
            moisture = moist;
            sunlight = sun;
            soilHealth = soil;
            season = seas;
            timeOfDay = 12f; // Default noon
            weather = WeatherType.Clear;
        }
    }
    
    public enum GrowthModifierType
    {
        Environmental,
        Nutritional,
        Chemical,
        Magical,
        Genetic,
        Seasonal,
        Weather,
        Tool,
        Fertilizer,
        Disease,
        Pest
    }
    
    public enum GrowthApplicationMode
    {
        Continuous,
        OnApplication,
        Periodic,
        OnStageChange,
        OnConditionMet
    }
    
    public enum GrowthEffectType
    {
        GrowthRate,
        YieldMultiplier,
        QualityBonus,
        SizeModifier,
        WaterEfficiency,
        NutrientUptake,
        DiseaseResistance,
        PestResistance,
        HarvestTime,
        SeedProduction,
        MutationChance,
        StageSkip
    }
    
    public enum GrowthConditionType
    {
        Temperature,
        Moisture,
        Sunlight,
        SoilHealth,
        Season,
        TimeOfDay,
        Weather,
        CropAge,
        GrowthStage
    }
    
    public enum ComparisonType
    {
        GreaterThan,
        LessThan,
        EqualTo,
        Between
    }
    
    public enum EffectApplicationType
    {
        Additive,
        Multiplicative,
        Override
    }
    
    public enum GrowthStage
    {
        Seed = 0,
        Germination = 1,
        Seedling = 2,
        Vegetative = 3,
        Flowering = 4,
        Fruiting = 5,
        Mature = 6,
        Harvest = 7
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
}