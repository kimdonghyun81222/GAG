using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.GameSystems._01.Scripts.Farming.GameSystem.Economy
{
    [CreateAssetMenu(fileName = "ShopData", menuName = "GrowAGarden/Economy/Shop Data")]
    public class ShopData : ScriptableObject
    {
        [Header("Shop Information")]
        public string shopName = "General Store";
        public string shopDescription = "Buy and sell various items";
        public Sprite shopIcon;
        public ShopType shopType = ShopType.General;
        
        [Header("Shop Settings")]
        public bool isAvailable = true;
        public float restockInterval = 86400f; // 24 hours in seconds
        public int maxCustomersPerDay = 50;
        public float priceMultiplier = 1f;
        
        [Header("Items for Sale")]
        public List<ShopItem> itemsForSale = new List<ShopItem>();
        
        [Header("Items Shop Buys")]
        public List<ShopBuyItem> itemsShopBuys = new List<ShopBuyItem>();
        
        [Header("Requirements")]
        public int requiredPlayerLevel = 1;
        public List<string> requiredQuests = new List<string>();
        public List<string> requiredUpgrades = new List<string>();
        
        // Validation
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(shopName))
                shopName = "General Store";
            
            priceMultiplier = Mathf.Max(0.1f, priceMultiplier);
            restockInterval = Mathf.Max(60f, restockInterval); // Minimum 1 minute
            maxCustomersPerDay = Mathf.Max(1, maxCustomersPerDay);
            
            // Validate shop items
            foreach (var item in itemsForSale)
            {
                if (item != null)
                {
                    item.ValidateItem();
                }
            }
        }
        
        // Utility methods
        public ShopItem GetShopItem(string itemId)
        {
            return itemsForSale.Find(item => item != null && item.itemId == itemId);
        }
        
        public ShopBuyItem GetBuyItem(string itemId)
        {
            return itemsShopBuys.Find(item => item != null && item.itemId == itemId);
        }
        
        public bool CanPlayerAccess(int playerLevel, List<string> completedQuests = null, List<string> playerUpgrades = null)
        {
            if (!isAvailable) return false;
            if (playerLevel < requiredPlayerLevel) return false;
            
            // Check required quests
            if (requiredQuests.Count > 0 && completedQuests != null)
            {
                foreach (var quest in requiredQuests)
                {
                    if (!completedQuests.Contains(quest))
                        return false;
                }
            }
            
            // Check required upgrades
            if (requiredUpgrades.Count > 0 && playerUpgrades != null)
            {
                foreach (var upgrade in requiredUpgrades)
                {
                    if (!playerUpgrades.Contains(upgrade))
                        return false;
                }
            }
            
            return true;
        }
        
        public int GetAdjustedSellPrice(int basePrice)
        {
            return Mathf.RoundToInt(basePrice * priceMultiplier);
        }
        
        public int GetAdjustedBuyPrice(int basePrice)
        {
            // Shops typically buy at lower prices
            return Mathf.RoundToInt(basePrice * priceMultiplier * 0.6f);
        }
        
        public List<ShopItem> GetAvailableItems()
        {
            var availableItems = new List<ShopItem>();
            
            foreach (var item in itemsForSale)
            {
                if (item != null && item.isAvailable && item.currentStock > 0)
                {
                    availableItems.Add(item);
                }
            }
            
            return availableItems;
        }
    }
    
    [System.Serializable]
    public class ShopItem
    {
        [Header("Item Information")]
        public string itemId;
        public string itemName;
        public string itemDescription;
        public Sprite itemIcon;
        
        [Header("Pricing")]
        public CurrencyData currency;
        public int basePrice = 100;
        public int currentPrice; // Calculated price with modifiers
        
        [Header("Stock")]
        public int maxStock = 10;
        public int currentStock = 10;
        public bool unlimitedStock = false;
        public bool restockable = true;
        
        [Header("Requirements")]
        public int requiredPlayerLevel = 1;
        public int maxQuantityPerPurchase = 99;
        public int maxQuantityPerDay = 10;
        
        [Header("Availability")]
        public bool isAvailable = true;
        public bool isLimitedTime = false;
        public float availabilityDuration = 0f; // In seconds
        
        // Runtime data
        [System.NonSerialized] public int purchasedToday = 0;
        [System.NonSerialized] public float lastRestockTime = 0f;
        
        public void ValidateItem()
        {
            if (string.IsNullOrEmpty(itemId))
                itemId = itemName?.Replace(" ", "_").ToLower() ?? "unknown_item";
            
            basePrice = Mathf.Max(1, basePrice);
            maxStock = Mathf.Max(1, maxStock);
            currentStock = Mathf.Clamp(currentStock, 0, maxStock);
            maxQuantityPerPurchase = Mathf.Max(1, maxQuantityPerPurchase);
            maxQuantityPerDay = Mathf.Max(1, maxQuantityPerDay);
        }
        
        public bool CanPurchase(int quantity, int playerLevel)
        {
            if (!isAvailable) return false;
            if (playerLevel < requiredPlayerLevel) return false;
            if (quantity <= 0) return false;
            if (quantity > maxQuantityPerPurchase) return false;
            if (purchasedToday + quantity > maxQuantityPerDay) return false;
            
            if (!unlimitedStock && currentStock < quantity) return false;
            
            return true;
        }
        
        public void Purchase(int quantity)
        {
            if (!unlimitedStock)
            {
                currentStock = Mathf.Max(0, currentStock - quantity);
            }
            
            purchasedToday += quantity;
        }
        
        public void Restock()
        {
            if (restockable)
            {
                currentStock = maxStock;
                lastRestockTime = Time.time;
            }
        }
        
        public void ResetDailyLimits()
        {
            purchasedToday = 0;
        }
    }
    
    [System.Serializable]
    public class ShopBuyItem
    {
        [Header("Item Information")]
        public string itemId;
        public string itemName;
        public CurrencyData currency;
        
        [Header("Pricing")]
        public int basePrice = 50;
        public float priceMultiplier = 0.6f; // Shops buy at lower prices
        
        [Header("Limits")]
        public int maxQuantityPerDay = 50;
        public bool unlimited = false;
        
        // Runtime data
        [System.NonSerialized] public int soldToday = 0;
        
        public bool CanSell(int quantity)
        {
            if (unlimited) return true;
            return soldToday + quantity <= maxQuantityPerDay;
        }
        
        public void Sell(int quantity)
        {
            soldToday += quantity;
        }
        
        public void ResetDailyLimits()
        {
            soldToday = 0;
        }
        
        public int GetSellPrice()
        {
            return Mathf.RoundToInt(basePrice * priceMultiplier);
        }
    }
    
    public enum ShopType
    {
        General,
        Seeds,
        Tools,
        Food,
        Equipment,
        Crafting,
        Special
    }
}