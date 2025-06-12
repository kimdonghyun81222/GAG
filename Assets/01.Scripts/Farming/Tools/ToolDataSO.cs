using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Tools
{
    [CreateAssetMenu(fileName = "ToolData", menuName = "GrowAGarden/Farming/Tool Data")]
    public class ToolData : ScriptableObject
    {
        [Header("Basic Information")]
        public string toolName = "Basic Tool";
        public string description = "A basic farming tool";
        public Sprite toolIcon;
        public GameObject toolPrefab;
        public ToolType toolType = ToolType.Hoe;
        public ToolRarity rarity = ToolRarity.Common;
        
        [Header("Tool Properties")]
        public float durability = 100f;
        public float maxDurability = 100f;
        public float efficiency = 1f;
        public float range = 1f;
        public float useSpeed = 1f;
        public float energyCost = 10f;
        
        [Header("Tool Effects")]
        public int areaOfEffect = 1; // 1 = single tile, 3 = 3x3, etc.
        public bool canTill = false;
        public bool canWater = false;
        public bool canHarvest = false;
        public bool canPlant = false;
        public bool canFertilize = false;
        public bool canClear = false;
        
        [Header("Special Abilities")]
        public List<ToolAbility> specialAbilities = new List<ToolAbility>();
        public float criticalChance = 0f;
        public float criticalMultiplier = 1.5f;
        public bool autoRepair = false;
        public float repairRate = 1f; // Per second when not in use
        
        [Header("Requirements")]
        public int requiredLevel = 1;
        public List<string> requiredSkills = new List<string>();
        public int unlockCost = 0;
        
        [Header("Economic")]
        public int purchasePrice = 100;
        public int sellPrice = 50;
        public int repairCost = 10;
        
        [Header("Visual & Audio")]
        public Material toolMaterial;
        public AudioClip useSound;
        public AudioClip breakSound;
        public ParticleSystem useParticles;
        public float animationSpeed = 1f;
        
        [Header("Upgrade System")]
        public bool canBeUpgraded = true;
        public ToolData upgradedVersion;
        public List<UpgradeRequirement> upgradeRequirements = new List<UpgradeRequirement>();
        
        // Validation
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(toolName))
                toolName = toolType.ToString() + " Tool";
            
            durability = Mathf.Clamp(durability, 0f, maxDurability);
            maxDurability = Mathf.Max(1f, maxDurability);
            efficiency = Mathf.Max(0.1f, efficiency);
            range = Mathf.Max(0.1f, range);
            useSpeed = Mathf.Max(0.1f, useSpeed);
            energyCost = Mathf.Max(0f, energyCost);
            
            areaOfEffect = Mathf.Max(1, areaOfEffect);
            if (areaOfEffect % 2 == 0) areaOfEffect++; // Ensure odd numbers for symmetrical areas
            
            criticalChance = Mathf.Clamp01(criticalChance);
            criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
            repairRate = Mathf.Max(0f, repairRate);
            
            purchasePrice = Mathf.Max(0, purchasePrice);
            sellPrice = Mathf.Max(0, sellPrice);
            repairCost = Mathf.Max(0, repairCost);
        }
        
        // Utility methods
        public bool CanPerformAction(ToolAction action)
        {
            return action switch
            {
                ToolAction.Till => canTill,
                ToolAction.Water => canWater,
                ToolAction.Harvest => canHarvest,
                ToolAction.Plant => canPlant,
                ToolAction.Fertilize => canFertilize,
                ToolAction.Clear => canClear,
                _ => false
            };
        }
        
        public float GetEffectiveRange()
        {
            return range * efficiency;
        }
        
        public float GetEffectiveSpeed()
        {
            return useSpeed * efficiency;
        }
        
        public float GetDurabilityPercentage()
        {
            return maxDurability > 0 ? durability / maxDurability : 0f;
        }
        
        public bool IsBroken()
        {
            return durability <= 0f;
        }
        
        public bool NeedsRepair()
        {
            return durability < maxDurability * 0.9f;
        }
        
        public int GetRepairCost()
        {
            float damagePercentage = 1f - GetDurabilityPercentage();
            return Mathf.RoundToInt(repairCost * damagePercentage);
        }
        
        public Color GetRarityColor()
        {
            return rarity switch
            {
                ToolRarity.Common => Color.white,
                ToolRarity.Uncommon => Color.green,
                ToolRarity.Rare => Color.blue,
                ToolRarity.Epic => Color.magenta,
                ToolRarity.Legendary => Color.yellow,
                _ => Color.white
            };
        }
        
        public string GetToolDescription()
        {
            string desc = description + "\n\n";
            desc += $"Efficiency: {efficiency:F1}x\n";
            desc += $"Range: {range:F1}m\n";
            desc += $"Energy Cost: {energyCost:F0}\n";
            desc += $"Area: {areaOfEffect}x{areaOfEffect}\n";
            desc += $"Durability: {durability:F0}/{maxDurability:F0}";
            
            if (criticalChance > 0)
            {
                desc += $"\nCritical Chance: {criticalChance:P0}";
            }
            
            return desc;
        }
    }
    
    [System.Serializable]
    public class ToolAbility
    {
        public string abilityName;
        public string description;
        public ToolAbilityType abilityType;
        public float value;
        public float duration;
        public float cooldown;
        public bool isPassive;
    }
    
    [System.Serializable]
    public class UpgradeRequirement
    {
        public string requirementName;
        public UpgradeRequirementType requirementType;
        public int amount;
        public string itemId; // For item requirements
    }
    
    public enum ToolType
    {
        Hoe,
        WateringCan,
        Sickle,
        Scythe,
        Shovel,
        Pickaxe,
        Axe,
        Shears,
        Fertilizer,
        Seeds,
        Multitool
    }
    
    public enum ToolRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    public enum ToolAction
    {
        Till,
        Water,
        Harvest,
        Plant,
        Fertilize,
        Clear,
        Dig,
        Cut
    }
    
    public enum ToolAbilityType
    {
        EfficiencyBoost,
        RangeBoost,
        SpeedBoost,
        EnergySaving,
        DurabilityBoost,
        CriticalBoost,
        AutoRepair,
        AreaBoost
    }
    
    public enum UpgradeRequirementType
    {
        PlayerLevel,
        Currency,
        Item,
        Skill,
        Achievement
    }
}