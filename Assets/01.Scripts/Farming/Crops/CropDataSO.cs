using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Crops
{
    [CreateAssetMenu(fileName = "CropData", menuName = "GrowAGarden/Farming/Crop Data")]
    public class CropDataSO : ScriptableObject
    {
        [Header("Basic Information")]
        public string cropName;
        public string description;
        public Sprite cropIcon;
        public CropType cropType = CropType.Vegetable;
        public CropCategory category = CropCategory.Food;
        
        [Header("Growing Stages")]
        public List<CropStage> growthStages = new List<CropStage>();
        
        [Header("Growth Settings")]
        [Min(0f)] public float baseGrowthTime = 120f; // seconds
        [Range(0f, 1f)] public float waterRequirement = 0.5f;
        [Range(0f, 1f)] public float sunlightRequirement = 0.7f;
        [Range(-10f, 40f)] public float optimalTemperature = 20f;
        [Range(0f, 20f)] public float temperatureTolerance = 5f;
        
        [Header("Seasonal Growth")]
        public List<SeasonGrowthModifier> seasonModifiers = new List<SeasonGrowthModifier>();
        
        [Header("Weather Effects")]
        public WeatherEffects weatherEffects = new WeatherEffects();
        
        [Header("Harvest")]
        public int minHarvestAmount = 1;
        public int maxHarvestAmount = 3;
        public bool isRenewableCrop = false; // Can harvest multiple times
        public float renewalTime = 60f; // Time between harvests for renewable crops
        
        [Header("Seeds")]
        public GameObject seedPrefab;
        public int seedCost = 10;
        public int seedPackSize = 5; // How many seeds per purchase
        
        [Header("Economic")]
        public int baseValue = 50;
        public float qualityMultiplier = 1.5f; // Perfect quality crops sell for more
        
        [Header("Requirements")]
        public ToolType requiredTool = ToolType.None;
        public int requiredFarmingLevel = 1;
        public List<string> requiredUpgrades = new List<string>();
        
        // Validation
        private void OnValidate()
        {
            if (growthStages.Count == 0)
            {
                growthStages.Add(new CropStage { stageName = "Seed", duration = 0.2f });
                growthStages.Add(new CropStage { stageName = "Sprout", duration = 0.3f });
                growthStages.Add(new CropStage { stageName = "Growing", duration = 0.3f });
                growthStages.Add(new CropStage { stageName = "Mature", duration = 0.2f });
            }
            
            // Ensure stage durations add up to 1.0
            float totalDuration = 0f;
            foreach (var stage in growthStages)
            {
                totalDuration += stage.duration;
            }
            
            if (totalDuration > 0f && Mathf.Abs(totalDuration - 1f) > 0.01f)
            {
                // Normalize durations
                for (int i = 0; i < growthStages.Count; i++)
                {
                    growthStages[i].duration /= totalDuration;
                }
            }
            
            baseGrowthTime = Mathf.Max(1f, baseGrowthTime);
            minHarvestAmount = Mathf.Max(1, minHarvestAmount);
            maxHarvestAmount = Mathf.Max(minHarvestAmount, maxHarvestAmount);
            baseValue = Mathf.Max(1, baseValue);
        }
        
        // Utility methods - Season을 직접 enum으로 사용
        public float GetTotalGrowthTime(CropSeason season = CropSeason.Spring)
        {
            float modifiedTime = baseGrowthTime;
            
            var seasonMod = seasonModifiers.Find(s => s.season == season);
            if (seasonMod != null)
            {
                modifiedTime *= seasonMod.growthTimeMultiplier;
            }
            
            return modifiedTime;
        }
        
        public float GetStageTime(int stageIndex, CropSeason season = CropSeason.Spring)
        {
            if (stageIndex < 0 || stageIndex >= growthStages.Count) return 0f;
            
            float totalTime = GetTotalGrowthTime(season);
            return totalTime * growthStages[stageIndex].duration;
        }
        
        public bool CanGrowInSeason(CropSeason season)
        {
            var seasonMod = seasonModifiers.Find(s => s.season == season);
            return seasonMod?.canGrow ?? true;
        }
        
        public CropQuality GetQualityForConditions(float waterLevel, float sunlightLevel, float temperature)
        {
            float waterScore = 1f - Mathf.Abs(waterLevel - waterRequirement);
            float sunScore = 1f - Mathf.Abs(sunlightLevel - sunlightRequirement);
            float tempScore = 1f - Mathf.Max(0f, Mathf.Abs(temperature - optimalTemperature) - temperatureTolerance) / 10f;
            
            float averageScore = (waterScore + sunScore + tempScore) / 3f;
            
            if (averageScore >= 0.9f) return CropQuality.Perfect;
            if (averageScore >= 0.7f) return CropQuality.Good;
            if (averageScore >= 0.5f) return CropQuality.Average;
            return CropQuality.Poor;
        }
        
        public int GetHarvestAmount(CropQuality quality)
        {
            int baseAmount = Random.Range(minHarvestAmount, maxHarvestAmount + 1);
            
            return quality switch
            {
                CropQuality.Perfect => Mathf.RoundToInt(baseAmount * 1.5f),
                CropQuality.Good => Mathf.RoundToInt(baseAmount * 1.2f),
                CropQuality.Average => baseAmount,
                CropQuality.Poor => Mathf.Max(1, Mathf.RoundToInt(baseAmount * 0.7f)),
                _ => baseAmount
            };
        }
        
        public int GetSellValue(CropQuality quality)
        {
            float multiplier = quality switch
            {
                CropQuality.Perfect => qualityMultiplier,
                CropQuality.Good => 1.2f,
                CropQuality.Average => 1f,
                CropQuality.Poor => 0.7f,
                _ => 1f
            };
            
            return Mathf.RoundToInt(baseValue * multiplier);
        }
        
        // Season 변환 메서드 (Environment Season과 호환)
        public static CropSeason ConvertFromEnvironmentSeason(int environmentSeason)
        {
            return (CropSeason)environmentSeason;
        }
    }
    
    [System.Serializable]
    public class CropStage
    {
        public string stageName;
        [Range(0f, 1f)] public float duration = 0.25f; // Percentage of total growth time
        public GameObject stagePrefab;
        public Sprite stageIcon;
        public string stageDescription;
        public bool canHarvest = false;
        public bool canWater = true;
        public Color stageColor = Color.green;
    }
    
    [System.Serializable]
    public class SeasonGrowthModifier
    {
        public CropSeason season;
        public bool canGrow = true;
        public float growthTimeMultiplier = 1f;
        public float qualityModifier = 1f;
        public float yieldModifier = 1f;
    }
    
    [System.Serializable]
    public class WeatherEffects
    {
        [Header("Rain Effects")]
        public float rainWateringBonus = 0.3f;
        public float stormDamageChance = 0.1f;
        
        [Header("Temperature Effects")]
        public float frostDamageThreshold = -2f;
        public float heatStressThreshold = 35f;
        public float extremeWeatherDamage = 0.2f;
        
        [Header("Wind Effects")]
        public float windDamageThreshold = 0.8f;
        public float windDamageAmount = 0.1f;
        
        [Header("Drought Effects")]
        public float droughtThreshold = 0.2f; // Days without water
        public float droughtDamagePerDay = 0.15f;
    }
    
    // Farming 전용 Season enum (Environment와 독립)
    public enum CropSeason
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }
    
    public enum CropType
    {
        Vegetable,
        Fruit,
        Grain,
        Herb,
        Flower,
        Tree,
        Berry
    }
    
    public enum CropCategory
    {
        Food,
        Cash,
        Crafting,
        Decorative,
        Special
    }
    
    public enum CropQuality
    {
        Poor,
        Average,
        Good,
        Perfect
    }
    
    public enum ToolType
    {
        None,
        Hoe,
        WateringCan,
        Sickle,
        Axe,
        Pickaxe
    }
}