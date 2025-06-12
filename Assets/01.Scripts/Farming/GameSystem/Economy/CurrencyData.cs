using UnityEngine;

namespace GrowAGarden.GameSystems._01.Scripts.Farming.GameSystem.Economy
{
    [CreateAssetMenu(fileName = "CurrencyData", menuName = "GrowAGarden/Economy/Currency Data")]
    public class CurrencyData : ScriptableObject
    {
        [Header("Currency Information")]
        public string currencyName = "Gold";
        public string currencySymbol = "G";
        public Sprite currencyIcon;
        public Color currencyColor = Color.yellow;
        
        [Header("Display Settings")]
        public bool showSymbolBefore = false; // true: $100, false: 100$
        public string displayFormat = "N0"; // Number formatting
        public int maxDisplayValue = 999999999;
        
        [Header("Conversion")]
        public bool isBaseCurrency = true;
        public float conversionRate = 1f; // Rate to base currency
        
        [Header("Limits")]
        public int startingAmount = 100;
        public int maximumAmount = int.MaxValue;
        public int minimumAmount = 0;
        
        // Validation
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(currencyName))
                currencyName = "Gold";
            
            if (string.IsNullOrEmpty(currencySymbol))
                currencySymbol = "G";
            
            conversionRate = Mathf.Max(0.001f, conversionRate);
            startingAmount = Mathf.Max(0, startingAmount);
            maximumAmount = Mathf.Max(startingAmount, maximumAmount);
        }
        
        // Utility methods
        public string FormatAmount(int amount)
        {
            // Clamp amount to display limits
            int displayAmount = Mathf.Clamp(amount, 0, maxDisplayValue);
            
            // Format the number
            string formattedNumber = displayAmount.ToString(displayFormat);
            
            // Add currency symbol
            if (showSymbolBefore)
                return $"{currencySymbol}{formattedNumber}";
            else
                return $"{formattedNumber}{currencySymbol}";
        }
        
        public bool IsValidAmount(int amount)
        {
            return amount >= minimumAmount && amount <= maximumAmount;
        }
        
        public int ClampAmount(int amount)
        {
            return Mathf.Clamp(amount, minimumAmount, maximumAmount);
        }
        
        public int ConvertToBaseCurrency(int amount)
        {
            return Mathf.RoundToInt(amount * conversionRate);
        }
        
        public int ConvertFromBaseCurrency(int baseAmount)
        {
            return Mathf.RoundToInt(baseAmount / conversionRate);
        }
    }
}