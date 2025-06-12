using System;
using UnityEngine;

namespace GrowAGarden.GameSystems._01.Scripts.Farming.GameSystem.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        [Header("Slot Data")]
        [SerializeField] private ItemData item;
        [SerializeField] private int quantity;
        [SerializeField] private int maxQuantity;
        [SerializeField] private bool isLocked;
        
        [Header("Slot Properties")]
        [SerializeField] private int slotIndex;
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private bool isQuickAccessSlot;
        
        // Properties
        public ItemData Item => item;
        public int Quantity => quantity;
        public int MaxQuantity => maxQuantity;
        public bool IsEmpty => item == null || quantity <= 0;
        public bool IsFull => !IsEmpty && quantity >= maxQuantity;
        public bool IsLocked => isLocked;
        public int SlotIndex => slotIndex;
        public Vector2Int GridPosition => gridPosition;
        public bool IsQuickAccessSlot => isQuickAccessSlot;
        public float FillPercentage => maxQuantity > 0 ? (float)quantity / maxQuantity : 0f;
        
        // Events
        public event Action<InventorySlot> OnSlotChanged;
        public event Action<InventorySlot, ItemData, int> OnItemAdded;
        public event Action<InventorySlot, ItemData, int> OnItemRemoved;
        public event Action<InventorySlot> OnSlotCleared;

        // Constructors
        public InventorySlot()
        {
            Clear();
        }
        
        public InventorySlot(int slotIndex, Vector2Int gridPosition, bool isQuickAccess = false)
        {
            this.slotIndex = slotIndex;
            this.gridPosition = gridPosition;
            this.isQuickAccessSlot = isQuickAccess;
            Clear();
        }
        
        public InventorySlot(ItemData item, int quantity, int slotIndex, Vector2Int gridPosition)
        {
            this.slotIndex = slotIndex;
            this.gridPosition = gridPosition;
            SetItem(item, quantity);
        }

        // Core slot operations
        public void SetItem(ItemData newItem, int newQuantity)
        {
            if (isLocked) return;
            
            var oldItem = item;
            var oldQuantity = quantity;
            
            item = newItem;
            quantity = newItem != null ? Mathf.Max(0, newQuantity) : 0;
            maxQuantity = newItem?.maxStackSize ?? 0;
            
            // Clamp quantity to max
            if (quantity > maxQuantity && maxQuantity > 0)
            {
                quantity = maxQuantity;
            }
            
            // Clear if quantity is 0
            if (quantity <= 0)
            {
                item = null;
                quantity = 0;
                maxQuantity = 0;
            }
            
            // Fire events
            OnSlotChanged?.Invoke(this);
            
            if (oldItem != item || oldQuantity != quantity)
            {
                if (newItem != null && newQuantity > 0)
                {
                    OnItemAdded?.Invoke(this, newItem, newQuantity);
                }
                
                if (oldItem != null && oldQuantity > 0 && (oldItem != newItem || oldQuantity > newQuantity))
                {
                    OnItemRemoved?.Invoke(this, oldItem, oldQuantity - (oldItem == newItem ? newQuantity : 0));
                }
            }
        }

        public bool AddItem(ItemData itemToAdd, int quantityToAdd)
        {
            if (isLocked || itemToAdd == null || quantityToAdd <= 0) return false;
            
            // If slot is empty, set the item
            if (IsEmpty)
            {
                SetItem(itemToAdd, quantityToAdd);
                return true;
            }
            
            // If item matches and can stack
            if (CanStackWith(itemToAdd))
            {
                int newQuantity = quantity + quantityToAdd;
                int actualQuantityAdded = Mathf.Min(quantityToAdd, maxQuantity - quantity);
                
                if (actualQuantityAdded > 0)
                {
                    SetItem(item, quantity + actualQuantityAdded);
                    return actualQuantityAdded == quantityToAdd;
                }
            }
            
            return false;
        }

        public bool RemoveItem(int quantityToRemove)
        {
            if (isLocked || IsEmpty || quantityToRemove <= 0) return false;
            
            int actualQuantityRemoved = Mathf.Min(quantityToRemove, quantity);
            int newQuantity = quantity - actualQuantityRemoved;
            
            if (newQuantity <= 0)
            {
                Clear();
            }
            else
            {
                SetItem(item, newQuantity);
            }
            
            return actualQuantityRemoved == quantityToRemove;
        }

        public void Clear()
        {
            if (isLocked) return;
            
            var oldItem = item;
            var oldQuantity = quantity;
            
            item = null;
            quantity = 0;
            maxQuantity = 0;
            
            OnSlotChanged?.Invoke(this);
            OnSlotCleared?.Invoke(this);
            
            if (oldItem != null && oldQuantity > 0)
            {
                OnItemRemoved?.Invoke(this, oldItem, oldQuantity);
            }
        }

        // Utility methods
        public bool CanAcceptItem(ItemData itemToCheck)
        {
            if (isLocked || itemToCheck == null) return false;
            
            if (IsEmpty) return true;
            
            return CanStackWith(itemToCheck) && !IsFull;
        }

        public bool CanStackWith(ItemData itemToCheck)
        {
            if (item == null || itemToCheck == null) return false;
            
            return item.CanStackWith(itemToCheck);
        }

        public int GetAvailableSpace()
        {
            if (isLocked) return 0;
            if (IsEmpty) return int.MaxValue; // Can accept any stackable item
            
            return maxQuantity - quantity;
        }

        public bool HasRoom(int quantityToAdd)
        {
            if (isLocked) return false;
            if (IsEmpty) return true;
            
            return GetAvailableSpace() >= quantityToAdd;
        }

        // Slot management
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            OnSlotChanged?.Invoke(this);
        }

        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }

        public void SetGridPosition(Vector2Int position)
        {
            gridPosition = position;
        }

        public void SetQuickAccessSlot(bool isQuickAccess)
        {
            isQuickAccessSlot = isQuickAccess;
        }

        // Information methods
        public string GetDisplayText()
        {
            if (IsEmpty) return "Empty";
            
            string text = item.GetDisplayName();
            
            if (item.isStackable && quantity > 1)
            {
                text += $" x{quantity}";
            }
            
            return text;
        }

        public string GetTooltipText()
        {
            if (IsEmpty) return "Empty Slot";
            
            string tooltip = $"<b>{item.GetDisplayName()}</b>\n";
            tooltip += $"<color=#{ColorUtility.ToHtmlStringRGB(item.rarityColor)}>{item.GetRarityText()}</color>\n\n";
            tooltip += item.GetDisplayDescription();
            
            if (item.isStackable && quantity > 1)
            {
                tooltip += $"\n\nQuantity: {quantity}";
            }
            
            if (item.canBeSold)
            {
                tooltip += $"\nValue: {item.GetValueText()}";
            }
            
            return tooltip;
        }

        // Serialization support
        public InventorySlotData ToSlotData()
        {
            return new InventorySlotData
            {
                itemId = item?.itemId ?? "",
                quantity = quantity,
                slotIndex = slotIndex,
                isLocked = isLocked
            };
        }

        public void FromSlotData(InventorySlotData data, ItemData itemData)
        {
            slotIndex = data.slotIndex;
            isLocked = data.isLocked;
            
            if (itemData != null && data.quantity > 0)
            {
                SetItem(itemData, data.quantity);
            }
            else
            {
                Clear();
            }
        }
    }

    [System.Serializable]
    public class InventorySlotData
    {
        public string itemId;
        public int quantity;
        public int slotIndex;
        public bool isLocked;
    }
}