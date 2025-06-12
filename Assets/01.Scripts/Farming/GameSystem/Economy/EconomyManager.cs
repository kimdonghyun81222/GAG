using System;
using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.GameSystems._01.Scripts.Farming.GameSystem.Economy
{
    [Provide]
    public class EconomyManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Economy Settings")]
        [SerializeField] private List<CurrencyData> availableCurrencies = new List<CurrencyData>();
        [SerializeField] private CurrencyData primaryCurrency;
        [SerializeField] private bool enableTransactions = true;
        [SerializeField] private bool debugMode = false;
        
        [Header("Transaction Settings")]
        [SerializeField] private float transactionDelay = 0.1f;
        [SerializeField] private int maxTransactionsPerFrame = 10;
        
        // Currency storage
        private Dictionary<CurrencyData, int> _currencyAmounts = new Dictionary<CurrencyData, int>();
        private Queue<TransactionRequest> _pendingTransactions = new Queue<TransactionRequest>();
        private int _transactionsThisFrame = 0;
        
        // Properties
        public CurrencyData PrimaryCurrency => primaryCurrency;
        public bool TransactionsEnabled => enableTransactions;
        public int CurrencyTypeCount => availableCurrencies.Count;
        
        // Events
        public event Action<CurrencyData, int, int> OnCurrencyChanged; // currency, oldAmount, newAmount
        public event Action<TransactionResult> OnTransactionCompleted;
        public event Action<string> OnTransactionFailed;

        private void Awake()
        {
            InitializeCurrencies();
        }

        private void Update()
        {
            ProcessPendingTransactions();
        }
        
        [Provide]
        public EconomyManager ProvideEconomyManager() => this;

        private void InitializeCurrencies()
        {
            // Set primary currency if not set
            if (primaryCurrency == null && availableCurrencies.Count > 0)
            {
                primaryCurrency = availableCurrencies[0];
            }
            
            // Initialize currency amounts
            foreach (var currency in availableCurrencies)
            {
                if (currency != null)
                {
                    _currencyAmounts[currency] = currency.startingAmount;
                    
                    if (debugMode)
                    {
                        Debug.Log($"Initialized {currency.currencyName}: {currency.FormatAmount(currency.startingAmount)}");
                    }
                }
            }
        }

        private void ProcessPendingTransactions()
        {
            _transactionsThisFrame = 0;
            
            while (_pendingTransactions.Count > 0 && _transactionsThisFrame < maxTransactionsPerFrame)
            {
                var transaction = _pendingTransactions.Dequeue();
                ProcessTransactionImmediate(transaction);
                _transactionsThisFrame++;
            }
        }

        // Currency management methods
        public int GetCurrencyAmount(CurrencyData currency)
        {
            if (currency == null) return 0;
            
            _currencyAmounts.TryGetValue(currency, out int amount);
            return amount;
        }

        public int GetPrimaryCurrencyAmount()
        {
            return GetCurrencyAmount(primaryCurrency);
        }

        public bool HasCurrency(CurrencyData currency, int amount)
        {
            if (currency == null || amount <= 0) return false;
            
            return GetCurrencyAmount(currency) >= amount;
        }

        public bool HasPrimaryCurrency(int amount)
        {
            return HasCurrency(primaryCurrency, amount);
        }

        public bool CanAfford(CurrencyData currency, int amount)
        {
            return HasCurrency(currency, amount);
        }

        public bool CanAffordPrimary(int amount)
        {
            return HasPrimaryCurrency(amount);
        }

        // Transaction methods
        public void AddCurrency(CurrencyData currency, int amount)
        {
            if (!enableTransactions || currency == null || amount <= 0) return;
            
            var transaction = new TransactionRequest
            {
                currency = currency,
                amount = amount,
                transactionType = TransactionType.Add,
                reason = "Add Currency"
            };
            
            QueueTransaction(transaction);
        }

        public void AddPrimaryCurrency(int amount)
        {
            AddCurrency(primaryCurrency, amount);
        }

        public bool SpendCurrency(CurrencyData currency, int amount, string reason = "Purchase")
        {
            if (!enableTransactions || currency == null || amount <= 0) return false;
            
            if (!HasCurrency(currency, amount))
            {
                OnTransactionFailed?.Invoke($"Insufficient {currency.currencyName}: need {currency.FormatAmount(amount)}, have {currency.FormatAmount(GetCurrencyAmount(currency))}");
                return false;
            }
            
            var transaction = new TransactionRequest
            {
                currency = currency,
                amount = amount,
                transactionType = TransactionType.Spend,
                reason = reason
            };
            
            QueueTransaction(transaction);
            return true;
        }

        public bool SpendPrimaryCurrency(int amount, string reason = "Purchase")
        {
            return SpendCurrency(primaryCurrency, amount, reason);
        }

        public bool TransferCurrency(CurrencyData fromCurrency, CurrencyData toCurrency, int amount)
        {
            if (!enableTransactions || fromCurrency == null || toCurrency == null || amount <= 0) return false;
            
            if (!HasCurrency(fromCurrency, amount)) return false;
            
            // Convert amounts if different currencies
            int convertedAmount = amount;
            if (fromCurrency != toCurrency)
            {
                int baseAmount = fromCurrency.ConvertToBaseCurrency(amount);
                convertedAmount = toCurrency.ConvertFromBaseCurrency(baseAmount);
            }
            
            var spendTransaction = new TransactionRequest
            {
                currency = fromCurrency,
                amount = amount,
                transactionType = TransactionType.Spend,
                reason = $"Transfer to {toCurrency.currencyName}"
            };
            
            var addTransaction = new TransactionRequest
            {
                currency = toCurrency,
                amount = convertedAmount,
                transactionType = TransactionType.Add,
                reason = $"Transfer from {fromCurrency.currencyName}"
            };
            
            QueueTransaction(spendTransaction);
            QueueTransaction(addTransaction);
            
            return true;
        }

        private void QueueTransaction(TransactionRequest transaction)
        {
            if (transactionDelay <= 0f)
            {
                ProcessTransactionImmediate(transaction);
            }
            else
            {
                _pendingTransactions.Enqueue(transaction);
            }
        }

        private void ProcessTransactionImmediate(TransactionRequest transaction)
        {
            if (transaction.currency == null) return;
            
            int oldAmount = GetCurrencyAmount(transaction.currency);
            int newAmount = oldAmount;
            bool success = false;
            
            switch (transaction.transactionType)
            {
                case TransactionType.Add:
                    newAmount = transaction.currency.ClampAmount(oldAmount + transaction.amount);
                    success = true;
                    break;
                    
                case TransactionType.Spend:
                    if (oldAmount >= transaction.amount)
                    {
                        newAmount = oldAmount - transaction.amount;
                        success = true;
                    }
                    break;
                    
                case TransactionType.Set:
                    newAmount = transaction.currency.ClampAmount(transaction.amount);
                    success = true;
                    break;
            }
            
            if (success)
            {
                _currencyAmounts[transaction.currency] = newAmount;
                
                OnCurrencyChanged?.Invoke(transaction.currency, oldAmount, newAmount);
                
                var result = new TransactionResult
                {
                    currency = transaction.currency,
                    amount = transaction.amount,
                    transactionType = transaction.transactionType,
                    oldAmount = oldAmount,
                    newAmount = newAmount,
                    reason = transaction.reason,
                    success = true,
                    timestamp = DateTime.Now
                };
                
                OnTransactionCompleted?.Invoke(result);
                
                if (debugMode)
                {
                    Debug.Log($"Transaction: {transaction.transactionType} {transaction.currency.FormatAmount(transaction.amount)} {transaction.currency.currencyName} | {transaction.currency.FormatAmount(oldAmount)} → {transaction.currency.FormatAmount(newAmount)} | {transaction.reason}");
                }
            }
            else
            {
                OnTransactionFailed?.Invoke($"Failed to {transaction.transactionType.ToString().ToLower()} {transaction.currency.FormatAmount(transaction.amount)} {transaction.currency.currencyName}: {transaction.reason}");
                
                if (debugMode)
                {
                    Debug.LogWarning($"Transaction failed: {transaction.transactionType} {transaction.amount} {transaction.currency.currencyName} - {transaction.reason}");
                }
            }
        }

        // Currency conversion methods
        public int ConvertCurrency(CurrencyData fromCurrency, CurrencyData toCurrency, int amount)
        {
            if (fromCurrency == null || toCurrency == null) return 0;
            
            if (fromCurrency == toCurrency) return amount;
            
            int baseAmount = fromCurrency.ConvertToBaseCurrency(amount);
            return toCurrency.ConvertFromBaseCurrency(baseAmount);
        }

        public float GetExchangeRate(CurrencyData fromCurrency, CurrencyData toCurrency)
        {
            if (fromCurrency == null || toCurrency == null) return 0f;
            
            if (fromCurrency == toCurrency) return 1f;
            
            return fromCurrency.conversionRate / toCurrency.conversionRate;
        }

        // Currency management
        public void AddCurrencyType(CurrencyData currency)
        {
            if (currency != null && !availableCurrencies.Contains(currency))
            {
                availableCurrencies.Add(currency);
                _currencyAmounts[currency] = currency.startingAmount;
                
                if (primaryCurrency == null)
                {
                    primaryCurrency = currency;
                }
                
                if (debugMode)
                {
                    Debug.Log($"Added currency type: {currency.currencyName}");
                }
            }
        }

        public void RemoveCurrencyType(CurrencyData currency)
        {
            if (currency != null && availableCurrencies.Contains(currency))
            {
                availableCurrencies.Remove(currency);
                _currencyAmounts.Remove(currency);
                
                if (primaryCurrency == currency && availableCurrencies.Count > 0)
                {
                    primaryCurrency = availableCurrencies[0];
                }
                
                if (debugMode)
                {
                    Debug.Log($"Removed currency type: {currency.currencyName}");
                }
            }
        }

        public List<CurrencyData> GetAllCurrencies()
        {
            return new List<CurrencyData>(availableCurrencies);
        }

        public Dictionary<CurrencyData, int> GetAllCurrencyAmounts()
        {
            return new Dictionary<CurrencyData, int>(_currencyAmounts);
        }

        // Settings
        public void SetTransactionsEnabled(bool enabled)
        {
            enableTransactions = enabled;
            
            if (!enabled)
            {
                _pendingTransactions.Clear();
            }
        }

        public void SetTransactionDelay(float delay)
        {
            transactionDelay = Mathf.Max(0f, delay);
        }

        public void SetPrimaryCurrency(CurrencyData currency)
        {
            if (currency != null && availableCurrencies.Contains(currency))
            {
                primaryCurrency = currency;
            }
        }

        // Administrative methods
        public void SetCurrencyAmount(CurrencyData currency, int amount)
        {
            if (currency == null) return;
            
            var transaction = new TransactionRequest
            {
                currency = currency,
                amount = amount,
                transactionType = TransactionType.Set,
                reason = "Admin Set"
            };
            
            QueueTransaction(transaction);
        }

        public void ResetCurrency(CurrencyData currency)
        {
            if (currency != null)
            {
                SetCurrencyAmount(currency, currency.startingAmount);
            }
        }

        public void ResetAllCurrencies()
        {
            foreach (var currency in availableCurrencies)
            {
                ResetCurrency(currency);
            }
        }

        // Save/Load placeholder methods
        public EconomySaveData GetSaveData()
        {
            var saveData = new EconomySaveData();
            
            foreach (var kvp in _currencyAmounts)
            {
                if (kvp.Key != null)
                {
                    saveData.currencyAmounts.Add(new CurrencyAmount
                    {
                        currencyName = kvp.Key.currencyName,
                        amount = kvp.Value
                    });
                }
            }
            
            return saveData;
        }

        public void LoadSaveData(EconomySaveData saveData)
        {
            if (saveData?.currencyAmounts == null) return;
            
            foreach (var currencyAmount in saveData.currencyAmounts)
            {
                var currency = availableCurrencies.Find(c => c.currencyName == currencyAmount.currencyName);
                if (currency != null)
                {
                    SetCurrencyAmount(currency, currencyAmount.amount);
                }
            }
        }
    }

    // Supporting classes and enums
    public enum TransactionType
    {
        Add,
        Spend,
        Set
    }

    [System.Serializable]
    public class TransactionRequest
    {
        public CurrencyData currency;
        public int amount;
        public TransactionType transactionType;
        public string reason;
    }

    [System.Serializable]
    public class TransactionResult
    {
        public CurrencyData currency;
        public int amount;
        public TransactionType transactionType;
        public int oldAmount;
        public int newAmount;
        public string reason;
        public bool success;
        public DateTime timestamp;
    }

    [System.Serializable]
    public class EconomySaveData
    {
        public List<CurrencyAmount> currencyAmounts = new List<CurrencyAmount>();
    }

    [System.Serializable]
    public class CurrencyAmount
    {
        public string currencyName;
        public int amount;
    }
}