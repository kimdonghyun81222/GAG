using UnityEngine;

namespace GrowAGarden.GameSystems._01.Scripts.Farming.GameSystem.Inventory
{
    [CreateAssetMenu(fileName = "InventoryData", menuName = "GrowAGarden/Inventory/Inventory Data")]
    public class InventoryData : ScriptableObject
    {
        [Header("Inventory Settings")]
        public string inventoryName = "Player Inventory";
        public int maxSlots = 20;
        public int maxStackSize = 99;
        public bool allowDuplicateTypes = true;
        public bool autoSort = false;
        
        [Header("Slot Configuration")]
        public int rows = 4;
        public int columns = 5;
        
        [Header("Restrictions")]
        public bool allowAllItemTypes = true;
        public ItemType[] allowedItemTypes;
        public ItemRarity maxAllowedRarity = ItemRarity.Legendary;
        
        [Header("Special Features")]
        public bool hasQuickAccessSlots = false;
        public int quickAccessSlotCount = 8;
        public bool persistBetweenScenes = true;
        
        // Validation
        private void OnValidate()
        {
            maxSlots = Mathf.Max(1, maxSlots);
            maxStackSize = Mathf.Max(1, maxStackSize);
            rows = Mathf.Max(1, rows);
            columns = Mathf.Max(1, columns);
            quickAccessSlotCount = Mathf.Clamp(quickAccessSlotCount, 0, maxSlots);
            
            // Ensure rows * columns matches maxSlots
            if (rows * columns != maxSlots)
            {
                maxSlots = rows * columns;
            }
        }
        
        // Utility methods
        public bool CanAcceptItemType(ItemType itemType)
        {
            if (allowAllItemTypes) return true;
            
            if (allowedItemTypes == null || allowedItemTypes.Length == 0) return false;
            
            foreach (var allowedType in allowedItemTypes)
            {
                if (allowedType == itemType) return true;
            }
            
            return false;
        }
        
        public bool CanAcceptItemRarity(ItemRarity rarity)
        {
            return rarity <= maxAllowedRarity;
        }
        
        public Vector2Int GetSlotPosition(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots) return Vector2Int.zero;
            
            int row = slotIndex / columns;
            int col = slotIndex % columns;
            
            return new Vector2Int(col, row);
        }
        
        public int GetSlotIndex(Vector2Int position)
        {
            if (position.x < 0 || position.x >= columns || position.y < 0 || position.y >= rows)
                return -1;
            
            return position.y * columns + position.x;
        }
        
        public bool IsQuickAccessSlot(int slotIndex)
        {
            return hasQuickAccessSlots && slotIndex < quickAccessSlotCount;
        }
    }
    
    public enum ItemType
    {
        None,
        Consumable,
        Tool,
        Seed,
        Crop,
        Material,
        Equipment,
        Quest,
        Special
    }
    
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}