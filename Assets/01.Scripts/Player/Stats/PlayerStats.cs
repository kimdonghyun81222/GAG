using System;
using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.Player._01.Scripts.Player.Stats
{
    [Provide]
    public class PlayerStats : MonoBehaviour, IDependencyProvider
    {
        [Header("Basic Stats")]
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private int currentExperience = 0;
        [SerializeField] private int experienceToNextLevel = 100;
        
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float healthRegenRate = 1f;
        [SerializeField] private float healthRegenDelay = 5f;
        
        [Header("Energy")]
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float currentEnergy = 100f;
        [SerializeField] private float energyRegenRate = 5f;
        [SerializeField] private float energyDrainRate = 10f;
        
        [Header("Hunger")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float currentHunger = 100f;
        [SerializeField] private float hungerDecayRate = 2f;
        
        [Header("Experience Settings")]
        [SerializeField] private float experienceMultiplier = 1f;
        [SerializeField] private AnimationCurve experienceCurve = AnimationCurve.Linear(1f, 100f, 100f, 10000f);
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        // Regeneration tracking
        private float lastDamageTime;
        private float lastEnergyUseTime;
        
        // Stat modifiers
        private Dictionary<string, StatModifier> activeModifiers = new Dictionary<string, StatModifier>();
        
        // Properties
        public int PlayerLevel => playerLevel;
        public int CurrentExperience => currentExperience;
        public int ExperienceToNextLevel => experienceToNextLevel;
        public float ExperienceProgress => experienceToNextLevel > 0 ? (float)currentExperience / experienceToNextLevel : 1f;
        
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        
        public float MaxEnergy => maxEnergy;
        public float CurrentEnergy => currentEnergy;
        public float EnergyPercentage => maxEnergy > 0 ? currentEnergy / maxEnergy : 0f;
        
        public float MaxHunger => maxHunger;
        public float CurrentHunger => currentHunger;
        public float HungerPercentage => maxHunger > 0 ? currentHunger / maxHunger : 0f;
        
        public bool IsAlive => currentHealth > 0f;
        public bool IsHealthy => HealthPercentage > 0.8f;
        public bool IsHungry => HungerPercentage < 0.3f;
        public bool IsTired => EnergyPercentage < 0.2f;
        
        // Events
        public event Action<int, int> OnLevelUp; // oldLevel, newLevel
        public event Action<int, int> OnExperienceGained; // amount, newTotal
        public event Action<float, float> OnHealthChanged; // oldHealth, newHealth
        public event Action<float, float> OnEnergyChanged; // oldEnergy, newEnergy
        public event Action<float, float> OnHungerChanged; // oldHunger, newHunger
        public event Action OnPlayerDied;
        public event Action OnPlayerRevived;
        public event Action<string, StatModifier> OnModifierAdded;
        public event Action<string, StatModifier> OnModifierRemoved;

        private void Awake()
        {
            // Initialize values
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;
            currentHunger = maxHunger;
            lastDamageTime = Time.time;
            lastEnergyUseTime = Time.time;
            
            CalculateExperienceToNextLevel();
        }

        private void Update()
        {
            UpdateRegeneration();
            UpdateHungerDecay();
            UpdateModifiers();
        }
        
        [Provide]
        public PlayerStats ProvidePlayerStats() => this;

        private void UpdateRegeneration()
        {
            // Health regeneration
            if (currentHealth < maxHealth && currentHealth > 0f)
            {
                if (Time.time - lastDamageTime >= healthRegenDelay)
                {
                    ChangeHealth(healthRegenRate * Time.deltaTime);
                }
            }
            
            // Energy regeneration
            if (currentEnergy < maxEnergy)
            {
                if (Time.time - lastEnergyUseTime >= 1f) // 1 second delay after energy use
                {
                    ChangeEnergy(energyRegenRate * Time.deltaTime);
                }
            }
        }

        private void UpdateHungerDecay()
        {
            if (currentHunger > 0f)
            {
                ChangeHunger(-hungerDecayRate * Time.deltaTime);
            }
        }

        private void UpdateModifiers()
        {
            // Update and remove expired modifiers
            var expiredModifiers = new List<string>();
            
            foreach (var kvp in activeModifiers)
            {
                var modifier = kvp.Value;
                modifier.remainingDuration -= Time.deltaTime;
                
                if (modifier.remainingDuration <= 0f)
                {
                    expiredModifiers.Add(kvp.Key);
                }
            }
            
            foreach (var modifierName in expiredModifiers)
            {
                RemoveModifier(modifierName);
            }
        }

        // Experience management
        public void GainExperience(int amount)
        {
            if (amount <= 0) return;
            
            int adjustedAmount = Mathf.RoundToInt(amount * experienceMultiplier);
            int oldExperience = currentExperience;
            
            currentExperience += adjustedAmount;
            OnExperienceGained?.Invoke(adjustedAmount, currentExperience);
            
            // Check for level up
            while (currentExperience >= experienceToNextLevel)
            {
                LevelUp();
            }
            
            if (debugMode)
            {
                Debug.Log($"Gained {adjustedAmount} experience (total: {currentExperience})");
            }
        }

        private void LevelUp()
        {
            int oldLevel = playerLevel;
            playerLevel++;
            currentExperience -= experienceToNextLevel;
            
            CalculateExperienceToNextLevel();
            
            // Level up bonuses
            ApplyLevelUpBonuses();
            
            OnLevelUp?.Invoke(oldLevel, playerLevel);
            
            if (debugMode)
            {
                Debug.Log($"Level up! Level {oldLevel} -> {playerLevel}");
            }
        }

        private void CalculateExperienceToNextLevel()
        {
            experienceToNextLevel = Mathf.RoundToInt(experienceCurve.Evaluate(playerLevel));
        }

        private void ApplyLevelUpBonuses()
        {
            // Increase max stats slightly each level
            float healthIncrease = 5f;
            float energyIncrease = 3f;
            float hungerIncrease = 2f;
            
            maxHealth += healthIncrease;
            maxEnergy += energyIncrease;
            maxHunger += hungerIncrease;
            
            // Restore some health/energy on level up
            ChangeHealth(maxHealth * 0.2f);
            ChangeEnergy(maxEnergy * 0.3f);
        }

        // Health management
        public void ChangeHealth(float amount)
        {
            float oldHealth = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            
            if (amount < 0f)
            {
                lastDamageTime = Time.time;
            }
            
            OnHealthChanged?.Invoke(oldHealth, currentHealth);
            
            // Check for death
            if (oldHealth > 0f && currentHealth <= 0f)
            {
                Die();
            }
            // Check for revival
            else if (oldHealth <= 0f && currentHealth > 0f)
            {
                Revive();
            }
        }

        public void Heal(float amount)
        {
            ChangeHealth(Mathf.Abs(amount));
        }

        public void TakeDamage(float amount)
        {
            ChangeHealth(-Mathf.Abs(amount));
        }

        public void SetHealth(float health)
        {
            ChangeHealth(health - currentHealth);
        }

        // Energy management
        public void ChangeEnergy(float amount)
        {
            float oldEnergy = currentEnergy;
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);
            
            if (amount < 0f)
            {
                lastEnergyUseTime = Time.time;
            }
            
            OnEnergyChanged?.Invoke(oldEnergy, currentEnergy);
        }

        public void RestoreEnergy(float amount)
        {
            ChangeEnergy(Mathf.Abs(amount));
        }

        public void DrainEnergy(float amount)
        {
            ChangeEnergy(-Mathf.Abs(amount));
        }

        public bool HasEnergy(float amount)
        {
            return currentEnergy >= amount;
        }

        public bool UseEnergy(float amount)
        {
            if (!HasEnergy(amount)) return false;
            
            DrainEnergy(amount);
            return true;
        }

        // Hunger management
        public void ChangeHunger(float amount)
        {
            float oldHunger = currentHunger;
            currentHunger = Mathf.Clamp(currentHunger + amount, 0f, maxHunger);
            
            OnHungerChanged?.Invoke(oldHunger, currentHunger);
        }

        public void Feed(float amount)
        {
            ChangeHunger(Mathf.Abs(amount));
        }

        public void MakeHungry(float amount)
        {
            ChangeHunger(-Mathf.Abs(amount));
        }

        // Life/Death management
        private void Die()
        {
            OnPlayerDied?.Invoke();
            
            if (debugMode)
            {
                Debug.Log("Player died!");
            }
        }

        private void Revive()
        {
            OnPlayerRevived?.Invoke();
            
            if (debugMode)
            {
                Debug.Log("Player revived!");
            }
        }

        public void Respawn()
        {
            currentHealth = maxHealth;
            currentEnergy = maxEnergy * 0.5f;
            currentHunger = maxHunger * 0.7f;
            
            OnHealthChanged?.Invoke(0f, currentHealth);
            OnEnergyChanged?.Invoke(currentEnergy, currentEnergy);
            OnHungerChanged?.Invoke(currentHunger, currentHunger);
            
            if (debugMode)
            {
                Debug.Log("Player respawned!");
            }
        }

        // Stat modifiers
        public void AddModifier(string name, StatModifier modifier)
        {
            if (activeModifiers.ContainsKey(name))
            {
                RemoveModifier(name);
            }
            
            activeModifiers[name] = modifier;
            ApplyModifier(modifier);
            OnModifierAdded?.Invoke(name, modifier);
            
            if (debugMode)
            {
                Debug.Log($"Added modifier: {name} ({modifier.type})");
            }
        }

        public void RemoveModifier(string name)
        {
            if (activeModifiers.TryGetValue(name, out StatModifier modifier))
            {
                RemoveModifier(modifier);
                activeModifiers.Remove(name);
                OnModifierRemoved?.Invoke(name, modifier);
                
                if (debugMode)
                {
                    Debug.Log($"Removed modifier: {name}");
                }
            }
        }

        private void ApplyModifier(StatModifier modifier)
        {
            switch (modifier.type)
            {
                case StatType.Health:
                    if (modifier.isPercentage)
                        maxHealth *= (1f + modifier.value);
                    else
                        maxHealth += modifier.value;
                    break;
                    
                case StatType.Energy:
                    if (modifier.isPercentage)
                        maxEnergy *= (1f + modifier.value);
                    else
                        maxEnergy += modifier.value;
                    break;
                    
                case StatType.Hunger:
                    if (modifier.isPercentage)
                        maxHunger *= (1f + modifier.value);
                    else
                        maxHunger += modifier.value;
                    break;
                    
                case StatType.ExperienceMultiplier:
                    experienceMultiplier += modifier.value;
                    break;
            }
        }

        private void RemoveModifier(StatModifier modifier)
        {
            switch (modifier.type)
            {
                case StatType.Health:
                    if (modifier.isPercentage)
                        maxHealth /= (1f + modifier.value);
                    else
                        maxHealth -= modifier.value;
                    break;
                    
                case StatType.Energy:
                    if (modifier.isPercentage)
                        maxEnergy /= (1f + modifier.value);
                    else
                        maxEnergy -= modifier.value;
                    break;
                    
                case StatType.Hunger:
                    if (modifier.isPercentage)
                        maxHunger /= (1f + modifier.value);
                    else
                        maxHunger -= modifier.value;
                    break;
                    
                case StatType.ExperienceMultiplier:
                    experienceMultiplier -= modifier.value;
                    break;
            }
            
            // Ensure max values don't go below minimum
            maxHealth = Mathf.Max(1f, maxHealth);
            maxEnergy = Mathf.Max(1f, maxEnergy);
            maxHunger = Mathf.Max(1f, maxHunger);
            experienceMultiplier = Mathf.Max(0.1f, experienceMultiplier);
            
            // Clamp current values
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            currentHunger = Mathf.Min(currentHunger, maxHunger);
        }

        // Save/Load methods
        public PlayerStatsData GetSaveData()
        {
            return new PlayerStatsData
            {
                playerLevel = this.playerLevel,
                currentExperience = this.currentExperience,
                maxHealth = this.maxHealth,
                currentHealth = this.currentHealth,
                maxEnergy = this.maxEnergy,
                currentEnergy = this.currentEnergy,
                maxHunger = this.maxHunger,
                currentHunger = this.currentHunger
            };
        }

        public void LoadSaveData(PlayerStatsData data)
        {
            playerLevel = data.playerLevel;
            currentExperience = data.currentExperience;
            maxHealth = data.maxHealth;
            currentHealth = data.currentHealth;
            maxEnergy = data.maxEnergy;
            currentEnergy = data.currentEnergy;
            maxHunger = data.maxHunger;
            currentHunger = data.currentHunger;
            
            CalculateExperienceToNextLevel();
        }

        // Debug methods
        public void DEBUG_GainExperience(int amount)
        {
            if (!debugMode) return;
            GainExperience(amount);
        }

        public void DEBUG_LevelUp()
        {
            if (!debugMode) return;
            GainExperience(experienceToNextLevel - currentExperience);
        }

        public void DEBUG_TakeDamage(float amount)
        {
            if (!debugMode) return;
            TakeDamage(amount);
        }

        public void DEBUG_FullHeal()
        {
            if (!debugMode) return;
            SetHealth(maxHealth);
            RestoreEnergy(maxEnergy);
            Feed(maxHunger);
        }
    }

    [System.Serializable]
    public class StatModifier
    {
        public StatType type;
        public float value;
        public float duration;
        public bool isPercentage;
        public bool isPermanent;
        
        [System.NonSerialized] public float remainingDuration;
        
        public StatModifier(StatType type, float value, float duration = 0f, bool isPercentage = false)
        {
            this.type = type;
            this.value = value;
            this.duration = duration;
            this.isPercentage = isPercentage;
            this.isPermanent = duration <= 0f;
            this.remainingDuration = duration;
        }
    }

    public enum StatType
    {
        Health,
        Energy,
        Hunger,
        ExperienceMultiplier,
        MovementSpeed,
        JumpHeight
    }

    [System.Serializable]
    public class PlayerStatsData
    {
        public int playerLevel;
        public int currentExperience;
        public float maxHealth;
        public float currentHealth;
        public float maxEnergy;
        public float currentEnergy;
        public float maxHunger;
        public float currentHunger;
    }
}