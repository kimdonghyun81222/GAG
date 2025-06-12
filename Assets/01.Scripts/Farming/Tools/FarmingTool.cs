using System;
using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Core._01.Scripts.Core.Entities;
using GrowAGarden.Farming._01.Scripts.Farming.Fields;
using GrowAGarden.Player._01.Scripts.Player.Interaction;
using GrowAGarden.Player._01.Scripts.Player.Stats;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Tools
{
    public class FarmingTool : Entity, IInteractable
    {
        [Header("Tool Settings")]
        [SerializeField] private ToolData toolData;
        [SerializeField] private bool isEquipped = false;
        [SerializeField] private bool debugMode = false;
        
        [Header("Visual Components")]
        [SerializeField] private Renderer toolRenderer;
        [SerializeField] private Animator toolAnimator;
        [SerializeField] private Transform effectPoint;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        
        // Dependencies
        [Inject] private PlayerStats playerStats;
        [Inject] private FieldManager fieldManager;
        
        // Tool state
        private float _currentDurability;
        private float _lastUseTime;
        private float _lastRepairTime;
        private bool _isInUse = false;
        private bool _needsRepair = false;
        private Dictionary<string, float> _abilityCooldowns = new Dictionary<string, float>();
        
        // Use state
        private Vector3 _targetPosition;
        private FieldTile _targetTile;
        private ToolAction _currentAction;
        private float _useProgress = 0f;
        private bool _isBeingInteracted = false;
        
        // Properties
        public ToolData ToolData => toolData;
        public bool IsEquipped => isEquipped;
        public bool IsInUse => _isInUse;
        public bool IsBroken => _currentDurability <= 0f;
        public bool NeedsRepair => _needsRepair;
        public float DurabilityPercentage => toolData?.maxDurability > 0 ? _currentDurability / toolData.maxDurability : 0f;
        public float CurrentDurability => _currentDurability;
        public float UseProgress => _useProgress;
        public bool CanUse => !IsBroken && !_isInUse && playerStats != null;
        
        // IInteractable implementation
        public bool IsBeingInteracted => _isBeingInteracted;
        
        // Events
        public event Action<FarmingTool> OnToolEquipped;
        public event Action<FarmingTool> OnToolUnequipped;
        public event Action<FarmingTool, ToolAction> OnToolUsed;
        public event Action<FarmingTool> OnToolBroken;
        public event Action<FarmingTool> OnToolRepaired;
        public event Action<FarmingTool, float> OnDurabilityChanged;

        protected override void Awake()
        {
            base.Awake();
            
            if (toolRenderer == null)
                toolRenderer = GetComponent<Renderer>();
            
            if (toolAnimator == null)
                toolAnimator = GetComponent<Animator>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (effectPoint == null)
                effectPoint = transform;
        }

        protected override void Start()
        {
            base.Start();
            
            if (toolData != null)
            {
                InitializeTool();
            }
        }

        private void Update()
        {
            UpdateToolState();
            UpdateAbilityCooldowns();
            UpdateAutoRepair();
            UpdateUseProgress();
        }

        public override void Initialize()
        {
            base.Initialize();
            
            if (toolData != null)
            {
                SetEntityName($"{toolData.toolName}");
                InitializeTool();
            }
        }

        private void InitializeTool()
        {
            _currentDurability = toolData.durability;
            _lastUseTime = Time.time;
            _lastRepairTime = Time.time;
            
            UpdateVisuals();
        }

        private void UpdateToolState()
        {
            // Check if tool needs repair
            bool wasNeedingRepair = _needsRepair;
            _needsRepair = toolData != null && toolData.NeedsRepair();
            
            if (!wasNeedingRepair && _needsRepair && debugMode)
            {
                Debug.Log($"Tool {toolData.toolName} needs repair");
            }
        }

        private void UpdateAbilityCooldowns()
        {
            if (toolData?.specialAbilities == null) return;
            
            var keys = new List<string>(_abilityCooldowns.Keys);
            foreach (string key in keys)
            {
                _abilityCooldowns[key] = Mathf.Max(0f, _abilityCooldowns[key] - Time.deltaTime);
            }
        }

        private void UpdateAutoRepair()
        {
            if (toolData == null || !toolData.autoRepair || _isInUse) return;
            
            if (_currentDurability < toolData.maxDurability)
            {
                float repairAmount = toolData.repairRate * Time.deltaTime;
                RepairTool(repairAmount);
            }
        }

        private void UpdateUseProgress()
        {
            if (!_isInUse) return;
            
            float useSpeed = toolData?.GetEffectiveSpeed() ?? 1f;
            _useProgress += Time.deltaTime * useSpeed;
            
            if (_useProgress >= 1f)
            {
                CompleteUse();
            }
        }

        // Tool usage methods
        public bool UseTool(ToolAction action, Vector3 targetPosition)
        {
            if (!CanUseAction(action)) return false;
            
            _currentAction = action;
            _targetPosition = targetPosition;
            _targetTile = fieldManager?.GetTileAt(targetPosition);
            
            if (_targetTile == null && RequiresFieldTile(action))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"No field tile found at position {targetPosition} for action {action}");
                }
                return false;
            }
            
            StartUse(action);
            return true;
        }

        public bool UseToolOnTile(ToolAction action, FieldTile tile)
        {
            if (!CanUseAction(action) || tile == null) return false;
            
            _currentAction = action;
            _targetTile = tile;
            _targetPosition = tile.transform.position;
            
            StartUse(action);
            return true;
        }

        private bool CanUseAction(ToolAction action)
        {
            if (!CanUse || toolData == null) return false;
            
            // Check if tool can perform this action
            if (!toolData.CanPerformAction(action)) return false;
            
            // Check energy requirements
            if (playerStats != null && !playerStats.HasEnergy(toolData.energyCost))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"Not enough energy to use {toolData.toolName}");
                }
                return false;
            }
            
            return true;
        }

        private bool RequiresFieldTile(ToolAction action)
        {
            return action switch
            {
                ToolAction.Till => true,
                ToolAction.Water => true,
                ToolAction.Plant => true,
                ToolAction.Fertilize => true,
                ToolAction.Harvest => true,
                ToolAction.Clear => true,
                _ => false
            };
        }

        private void StartUse(ToolAction action)
        {
            _isInUse = true;
            _useProgress = 0f;
            _lastUseTime = Time.time;
            
            // Consume energy
            if (playerStats != null)
            {
                playerStats.DrainEnergy(toolData.energyCost);
            }
            
            // Play animations and effects
            PlayUseAnimation(action);
            PlayUseSound();
            PlayUseParticles();
            
            if (debugMode)
            {
                Debug.Log($"Started using {toolData.toolName} for {action}");
            }
        }

        private void CompleteUse()
        {
            _isInUse = false;
            _useProgress = 0f;
            
            // Perform the actual action
            bool success = PerformAction(_currentAction);
            
            if (success)
            {
                // Reduce durability
                ReduceDurability(CalculateDurabilityLoss());
                
                // Trigger events
                OnToolUsed?.Invoke(this, _currentAction);
                
                if (debugMode)
                {
                    Debug.Log($"Completed {_currentAction} with {toolData.toolName}");
                }
            }
            
            // Reset use state
            _currentAction = ToolAction.Till;
            _targetTile = null;
            _targetPosition = Vector3.zero;
        }

        private bool PerformAction(ToolAction action)
        {
            if (_targetTile == null && RequiresFieldTile(action))
            {
                return false;
            }
            
            // Check for critical hit
            bool isCritical = UnityEngine.Random.value <= toolData.criticalChance;
            float effectiveness = isCritical ? toolData.criticalMultiplier : 1f;
            
            return action switch
            {
                ToolAction.Till => PerformTill(effectiveness),
                ToolAction.Water => PerformWater(effectiveness),
                ToolAction.Harvest => PerformHarvest(effectiveness),
                ToolAction.Plant => PerformPlant(effectiveness),
                ToolAction.Fertilize => PerformFertilize(effectiveness),
                ToolAction.Clear => PerformClear(effectiveness),
                _ => false
            };
        }

        private bool PerformTill(float effectiveness)
        {
            if (_targetTile == null) return false;
            
            // Apply to area if tool has area of effect
            if (toolData.areaOfEffect > 1)
            {
                return PerformAreaTill(effectiveness);
            }
            
            return _targetTile.Till();
        }

        private bool PerformAreaTill(float effectiveness)
        {
            if (fieldManager == null || _targetTile == null) return false;
            
            int range = toolData.areaOfEffect / 2;
            Vector2Int center = _targetTile.Position;
            
            int successCount = 0;
            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    Vector2Int pos = center + new Vector2Int(x, y);
                    FieldTile tile = fieldManager.GetTile(pos);
                    
                    if (tile != null && tile.Till())
                    {
                        successCount++;
                    }
                }
            }
            
            return successCount > 0;
        }

        private bool PerformWater(float effectiveness)
        {
            if (_targetTile == null) return false;
            
            float waterAmount = 0.3f * effectiveness * toolData.efficiency;
            
            if (toolData.areaOfEffect > 1)
            {
                return PerformAreaWater(waterAmount);
            }
            
            return _targetTile.Water(waterAmount);
        }

        private bool PerformAreaWater(float waterAmount)
        {
            if (fieldManager == null || _targetTile == null) return false;
            
            int range = toolData.areaOfEffect / 2;
            Vector2Int center = _targetTile.Position;
            
            int successCount = 0;
            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    Vector2Int pos = center + new Vector2Int(x, y);
                    FieldTile tile = fieldManager.GetTile(pos);
                    
                    if (tile != null && tile.Water(waterAmount))
                    {
                        successCount++;
                    }
                }
            }
            
            return successCount > 0;
        }

        private bool PerformHarvest(float effectiveness)
        {
            if (_targetTile == null || !_targetTile.CanHarvest) return false;
            
            var result = _targetTile.HarvestCrop();
            
            // Apply effectiveness to yield (if critical)
            if (result != null && effectiveness > 1f)
            {
                result.amount = Mathf.RoundToInt(result.amount * effectiveness);
                result.value = Mathf.RoundToInt(result.value * effectiveness);
            }
            
            return result != null;
        }

        private bool PerformPlant(float effectiveness)
        {
            // This would need seed data - for now just return false
            // In a full implementation, this would plant seeds from inventory
            return false;
        }

        private bool PerformFertilize(float effectiveness)
        {
            if (_targetTile == null) return false;
            
            float fertilizeAmount = 0.2f * effectiveness * toolData.efficiency;
            return _targetTile.Fertilize(fertilizeAmount);
        }

        private bool PerformClear(float effectiveness)
        {
            if (_targetTile == null) return false;
            
            _targetTile.ClearCrop();
            return true;
        }

        // Durability management
        private float CalculateDurabilityLoss()
        {
            float baseLoss = 1f / toolData.efficiency;
            
            // Apply special abilities that might reduce durability loss
            foreach (var ability in toolData.specialAbilities)
            {
                if (ability.abilityType == ToolAbilityType.DurabilityBoost)
                {
                    baseLoss *= (1f - ability.value);
                }
            }
            
            return baseLoss;
        }

        private void ReduceDurability(float amount)
        {
            float oldDurability = _currentDurability;
            _currentDurability = Mathf.Max(0f, _currentDurability - amount);
            
            OnDurabilityChanged?.Invoke(this, _currentDurability - oldDurability);
            
            if (oldDurability > 0f && _currentDurability <= 0f)
            {
                BreakTool();
            }
            
            UpdateVisuals();
        }

        public void RepairTool(float amount)
        {
            if (toolData == null) return;
            
            float oldDurability = _currentDurability;
            _currentDurability = Mathf.Min(toolData.maxDurability, _currentDurability + amount);
            _lastRepairTime = Time.time;
            
            if (oldDurability <= 0f && _currentDurability > 0f)
            {
                OnToolRepaired?.Invoke(this);
            }
            
            OnDurabilityChanged?.Invoke(this, _currentDurability - oldDurability);
            UpdateVisuals();
        }

        public void FullRepair()
        {
            if (toolData != null)
            {
                RepairTool(toolData.maxDurability);
            }
        }

        private void BreakTool()
        {
            _isInUse = false;
            OnToolBroken?.Invoke(this);
            
            if (toolData?.breakSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(toolData.breakSound);
            }
            
            if (debugMode)
            {
                Debug.Log($"Tool {toolData.toolName} broke!");
            }
        }

        // Equipment management
        public void Equip()
        {
            if (isEquipped) return;
            
            isEquipped = true;
            OnToolEquipped?.Invoke(this);
            
            if (debugMode)
            {
                Debug.Log($"Equipped {toolData.toolName}");
            }
        }

        public void Unequip()
        {
            if (!isEquipped) return;
            
            isEquipped = false;
            _isInUse = false; // Cancel any current use
            OnToolUnequipped?.Invoke(this);
            
            if (debugMode)
            {
                Debug.Log($"Unequipped {toolData.toolName}");
            }
        }

        // Visual and audio methods
        private void UpdateVisuals()
        {
            if (toolRenderer == null || toolData == null) return;
            
            // Update color based on durability
            float durabilityPercent = DurabilityPercentage;
            Color baseColor = toolData.GetRarityColor();
            
            if (durabilityPercent <= 0f)
            {
                baseColor = Color.red; // Broken
            }
            else if (durabilityPercent <= 0.25f)
            {
                baseColor = Color.Lerp(Color.red, Color.yellow, durabilityPercent * 4f); // Damaged
            }
            
            toolRenderer.material.color = baseColor;
        }

        private void PlayUseAnimation(ToolAction action)
        {
            if (toolAnimator == null) return;
            
            string animationTrigger = action switch
            {
                ToolAction.Till => "Till",
                ToolAction.Water => "Water",
                ToolAction.Harvest => "Harvest",
                ToolAction.Plant => "Plant",
                ToolAction.Fertilize => "Fertilize",
                ToolAction.Clear => "Clear",
                _ => "Use"
            };
            
            toolAnimator.SetTrigger(animationTrigger);
            toolAnimator.speed = toolData?.animationSpeed ?? 1f;
        }

        private void PlayUseSound()
        {
            if (audioSource == null || toolData?.useSound == null) return;
            
            audioSource.PlayOneShot(toolData.useSound);
        }

        private void PlayUseParticles()
        {
            if (toolData?.useParticles == null || effectPoint == null) return;
            
            var particles = Instantiate(toolData.useParticles, effectPoint.position, effectPoint.rotation);
            Destroy(particles.gameObject, 3f);
        }

        // IInteractable implementation
        public bool CanInteract()
        {
            return !isEquipped && !_isBeingInteracted;
        }

        public bool Interact()
        {
            if (!CanInteract()) return false;
            
            _isBeingInteracted = true;
            
            // Pick up the tool
            Equip();
            
            _isBeingInteracted = false;
            return true;
        }

        public void CancelInteraction()
        {
            _isBeingInteracted = false;
        }

        public string GetInteractionText()
        {
            if (isEquipped) return $"Drop {toolData?.toolName ?? "Tool"}";
            return $"Pick up {toolData?.toolName ?? "Tool"}";
        }

        public void OnLookEnter()
        {
            // Could add highlight effect here
        }

        public void OnLookExit()
        {
            // Remove highlight effect
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        // Tool data management
        public void SetToolData(ToolData newToolData)
        {
            toolData = newToolData;
            if (toolData != null)
            {
                SetEntityName($"{toolData.toolName}");
                InitializeTool();
            }
        }

        // Ability system
        public bool CanUseAbility(string abilityName)
        {
            if (toolData?.specialAbilities == null) return false;
            
            var ability = toolData.specialAbilities.Find(a => a.abilityName == abilityName);
            if (ability == null || ability.isPassive) return false;
            
            _abilityCooldowns.TryGetValue(abilityName, out float cooldown);
            return cooldown <= 0f;
        }

        public bool UseAbility(string abilityName)
        {
            if (!CanUseAbility(abilityName)) return false;
            
            var ability = toolData.specialAbilities.Find(a => a.abilityName == abilityName);
            if (ability == null) return false;
            
            // Set cooldown
            _abilityCooldowns[abilityName] = ability.cooldown;
            
            // Apply ability effect
            ApplyAbilityEffect(ability);
            
            if (debugMode)
            {
                Debug.Log($"Used ability: {abilityName}");
            }
            
            return true;
        }

        private void ApplyAbilityEffect(ToolAbility ability)
        {
            // This would implement the actual ability effects
            // For now, just log what would happen
            if (debugMode)
            {
                Debug.Log($"Applied ability effect: {ability.abilityName} ({ability.abilityType})");
            }
        }

        // Debug methods
        public void DEBUG_ReduceDurability(float amount)
        {
            if (!debugMode) return;
            ReduceDurability(amount);
        }

        public void DEBUG_BreakTool()
        {
            if (!debugMode) return;
            _currentDurability = 0f;
            BreakTool();
        }

        public void DEBUG_FullRepair()
        {
            if (!debugMode) return;
            FullRepair();
        }

        public void DEBUG_TestAction(ToolAction action)
        {
            if (!debugMode) return;
            
            Vector3 testPosition = transform.position + transform.forward * 2f;
            UseTool(action, testPosition);
        }
    }
}