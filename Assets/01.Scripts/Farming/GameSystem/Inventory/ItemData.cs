using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.GameSystems._01.Scripts.Farming.GameSystem.Inventory
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "GrowAGarden/Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        public string itemId;
        public string itemName;
        [TextArea(3, 6)] public string description;
        public Sprite icon;
        public GameObject prefab;
        
        [Header("Item Properties")]
        public ItemType itemType = ItemType.Consumable;
        public ItemRarity rarity = ItemRarity.Common;
        public int maxStackSize = 1;
        public bool isStackable = false;
        public bool isDroppable = true;
        public bool isTradeable = true;
        public bool isConsumable = false;
        
        [Header("Economic")]
        public int baseValue = 10;
        public bool canBeSold = true;
        public bool canBeBought = true;
        
        [Header("Usage")]
        public bool hasUseAction = false;
        public float useTime = 1f;
        public int usesPerItem = 1;
        public bool consumeOnUse = true;
        
        [Header("Requirements")]
        public int requiredLevel = 1;
        public List<string> requiredSkills = new List<string>();
        
        [Header("Effects")]
        public List<ItemEffect> useEffects = new List<ItemEffect>();
        
        [Header("Visual")]
        public Color rarityColor = Color.white;
        public bool hasGlowEffect = false;
        public ParticleSystem itemParticles;
        
        [Header("Audio")]
        public AudioClip pickupSound;
        public AudioClip useSound;
        public AudioClip dropSound;
        
        // Validation
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId))
                itemId = itemName?.Replace(" ", "_").ToLower() ?? "unknown_item";
            
            if (string.IsNullOrEmpty(itemName))
                itemName = "Unknown Item";
            
            maxStackSize = Mathf.Max(1, maxStackSize);
            baseValue = Mathf.Max(0, baseValue);
            useTime = Mathf.Max(0.1f, useTime);
            usesPerItem = Mathf.Max(1, usesPerItem);
            
            // Auto-set stackable based on max stack size
            isStackable = maxStackSize > 1;
            
            // Set rarity color
            rarityColor = GetRarityColor(rarity);
        }
        
        // Utility methods
        public static Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => Color.magenta,
                ItemRarity.Legendary => Color.yellow,
                _ => Color.white
            };
        }
        
        public bool CanUse(int playerLevel = 1, List<string> playerSkills = null)
        {
            if (!hasUseAction) return false;
            if (playerLevel < requiredLevel) return false;
            
            if (requiredSkills.Count > 0 && playerSkills != null)
            {
                foreach (var skill in requiredSkills)
                {
                    if (!playerSkills.Contains(skill))
                        return false;
                }
            }
            
            return true;
        }
        
        public bool CanStackWith(ItemData other)
        {
            if (other == null) return false;
            if (!isStackable || !other.isStackable) return false;
            return itemId == other.itemId;
        }
        
        public string GetDisplayName()
        {
            return itemName;
        }
        
        public string GetDisplayDescription()
        {
            return description;
        }
        
        public string GetRarityText()
        {
            return rarity.ToString();
        }
        
        public string GetValueText()
        {
            return canBeSold ? $"{baseValue}G" : "Cannot be sold";
        }
    }
    
    [System.Serializable]
    public class ItemEffect
    {
        public ItemEffectType effectType;
        public float value;
        public float duration;
        public string targetProperty;
        
        [Header("Conditional")]
        public bool hasCondition = false;
        public string conditionProperty;
        public float conditionValue;
    }
    
    public enum ItemEffectType
    {
        None,
        RestoreHealth,
        RestoreEnergy,
        RestoreHunger,
        BuffAttribute,
        DebuffAttribute,
        GrantExperience,
        GrantCurrency,
        TeleportPlayer,
        Custom
    }
}