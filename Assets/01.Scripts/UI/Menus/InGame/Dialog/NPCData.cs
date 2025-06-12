using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame.Dialog
{
    [CreateAssetMenu(fileName = "New NPC", menuName = "Grow A Garden/Dialog/NPC Data")]
    public class NPCData : ScriptableObject
    {
        [Header("Basic Information")]
        public string npcId;
        public string npcName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Visual")]
        public Sprite portrait;
        public Sprite fullBodySprite;
        public GameObject npcPrefab;
        public Color nameColor = Color.white;
        
        [Header("Dialog")]
        public DialogCollection dialogCollection;
        public string startDialogId;
        public string defaultGreetingId;
        public string farewellDialogId;
        
        [Header("Interaction")]
        public float interactionRange = 2f;
        public bool canInteract = true;
        public bool requiresPlayerAttention = true;
        public KeyCode interactionKey = KeyCode.E;
        
        [Header("Voice")]
        public AudioClip[] voiceClips;
        public float voicePitch = 1f;
        public float voiceVolume = 1f;
        
        [Header("Relationship")]
        public int initialRelationshipLevel = 0;
        public int maxRelationshipLevel = 100;
        public int minRelationshipLevel = -100;
        public RelationshipType relationshipType = RelationshipType.Neutral;
        
        [Header("Schedule")]
        public bool hasSchedule = false;
        public List<NPCScheduleEntry> schedule = new List<NPCScheduleEntry>();
        
        [Header("Conditions")]
        public List<NPCCondition> availabilityConditions = new List<NPCCondition>();
        public List<NPCCondition> interactionConditions = new List<NPCCondition>();
        
        [Header("Quests")]
        public List<string> questIds = new List<string>();
        public bool isQuestGiver = false;
        public bool isQuestTarget = false;
        
        [Header("Shop")]
        public bool isShopkeeper = false;
        public string shopId;
        public List<string> soldItemIds = new List<string>();
        
        [Header("Advanced")]
        public NPCPersonality personality = NPCPersonality.Friendly;
        public List<string> tags = new List<string>();
        public Dictionary<string, object> customData = new Dictionary<string, object>();
        
        [Header("Debug")]
        public bool enableDebugLog = false;
        public bool showInEditor = true;

        // Runtime data
        [System.NonSerialized]
        private int _currentRelationshipLevel;
        [System.NonSerialized]
        private bool _hasBeenMet = false;
        [System.NonSerialized]
        private DateTime _lastInteractionTime;
        [System.NonSerialized]
        private Dictionary<string, object> _runtimeData = new Dictionary<string, object>();
        [System.NonSerialized]
        private List<string> _completedQuests = new List<string>();
        [System.NonSerialized]
        private NPCState _currentState = NPCState.Idle;

        // Events
        public event Action<NPCData> OnInteractionStarted;
        public event Action<NPCData> OnInteractionEnded;
        public event Action<NPCData, int> OnRelationshipChanged;
        public event Action<NPCData, NPCState> OnStateChanged;

        // Properties
        public int CurrentRelationshipLevel 
        { 
            get => _currentRelationshipLevel; 
            set => SetRelationshipLevel(value); 
        }
        
        public bool HasBeenMet 
        { 
            get => _hasBeenMet; 
            set => _hasBeenMet = value; 
        }
        
        public DateTime LastInteractionTime => _lastInteractionTime;
        public NPCState CurrentState => _currentState;

        private void OnEnable()
        {
            InitializeNPC();
        }

        private void InitializeNPC()
        {
            // Initialize runtime data
            _currentRelationshipLevel = initialRelationshipLevel;
            _hasBeenMet = false;
            _lastInteractionTime = DateTime.MinValue;
            _currentState = NPCState.Idle;
            
            // Generate ID if empty
            if (string.IsNullOrEmpty(npcId))
            {
                npcId = name.Replace(" ", "_").ToLower();
            }
            
            // Validate data
            ValidateNPCData();
            
            if (enableDebugLog)
            {
                Debug.Log($"NPC Data initialized: {npcName} (ID: {npcId})");
            }
        }

        #region Validation

        public bool IsValid()
        {
            var errors = GetValidationErrors();
            return errors.Count == 0;
        }

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(npcId))
                errors.Add("NPC ID is empty");
            
            if (string.IsNullOrEmpty(npcName))
                errors.Add("NPC Name is empty");
            
            if (dialogCollection == null)
                errors.Add("Dialog Collection is not assigned");
            
            if (string.IsNullOrEmpty(startDialogId))
                errors.Add("Start Dialog ID is empty");
            
            if (dialogCollection != null && !string.IsNullOrEmpty(startDialogId))
            {
                if (!dialogCollection.HasDialog(startDialogId))
                    errors.Add($"Start Dialog ID '{startDialogId}' not found in collection");
            }
            
            if (!string.IsNullOrEmpty(defaultGreetingId) && dialogCollection != null)
            {
                if (!dialogCollection.HasDialog(defaultGreetingId))
                    errors.Add($"Default Greeting ID '{defaultGreetingId}' not found in collection");
            }
            
            if (!string.IsNullOrEmpty(farewellDialogId) && dialogCollection != null)
            {
                if (!dialogCollection.HasDialog(farewellDialogId))
                    errors.Add($"Farewell Dialog ID '{farewellDialogId}' not found in collection");
            }
            
            if (interactionRange <= 0f)
                errors.Add("Interaction range must be greater than 0");
            
            if (maxRelationshipLevel <= minRelationshipLevel)
                errors.Add("Max relationship level must be greater than min relationship level");
            
            if (initialRelationshipLevel < minRelationshipLevel || initialRelationshipLevel > maxRelationshipLevel)
                errors.Add("Initial relationship level is outside the valid range");
            
            return errors;
        }

        private void ValidateNPCData()
        {
            if (!IsValid())
            {
                var errors = GetValidationErrors();
                Debug.LogWarning($"NPC Data validation failed for {npcName}:\n{string.Join("\n", errors)}");
            }
        }

        #endregion

        #region Dialog Management

        /// <summary>
        /// Get the appropriate dialog ID based on current state and conditions
        /// </summary>
        public string GetCurrentDialogId()
        {
            // Check if NPC is available for interaction
            if (!CanInteractNow())
            {
                return null;
            }
            
            // First meeting
            if (!_hasBeenMet && !string.IsNullOrEmpty(defaultGreetingId))
            {
                return defaultGreetingId;
            }
            
            // Quest-related dialogs
            string questDialogId = GetQuestDialogId();
            if (!string.IsNullOrEmpty(questDialogId))
            {
                return questDialogId;
            }
            
            // Relationship-based dialogs
            string relationshipDialogId = GetRelationshipDialogId();
            if (!string.IsNullOrEmpty(relationshipDialogId))
            {
                return relationshipDialogId;
            }
            
            // Default dialog
            return startDialogId;
        }

        private string GetQuestDialogId()
        {
            // TODO: Implement quest-based dialog selection
            // This would check active quests and return appropriate dialog
            return null;
        }

        private string GetRelationshipDialogId()
        {
            // TODO: Implement relationship-based dialog selection
            // This would check relationship level and return appropriate dialog
            return null;
        }

        /// <summary>
        /// Start interaction with this NPC
        /// </summary>
        public bool StartInteraction()
        {
            if (!CanInteractNow())
            {
                if (enableDebugLog)
                {
                    Debug.Log($"Cannot interact with {npcName}: conditions not met");
                }
                return false;
            }
            
            string dialogId = GetCurrentDialogId();
            if (string.IsNullOrEmpty(dialogId))
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning($"No dialog available for {npcName}");
                }
                return false;
            }
            
            // Update state
            _hasBeenMet = true;
            _lastInteractionTime = DateTime.Now;
            SetState(NPCState.Talking);
            
            // Start dialog
            bool dialogStarted = DialogManager.Instance.StartDialog(dialogId, dialogCollection);
            
            if (dialogStarted)
            {
                OnInteractionStarted?.Invoke(this);
                
                if (enableDebugLog)
                {
                    Debug.Log($"Started interaction with {npcName} using dialog: {dialogId}");
                }
            }
            
            return dialogStarted;
        }

        /// <summary>
        /// End interaction with this NPC
        /// </summary>
        public void EndInteraction()
        {
            SetState(NPCState.Idle);
            OnInteractionEnded?.Invoke(this);
            
            if (enableDebugLog)
            {
                Debug.Log($"Ended interaction with {npcName}");
            }
        }

        #endregion

        #region Conditions

        /// <summary>
        /// Check if NPC can be interacted with right now
        /// </summary>
        public bool CanInteractNow()
        {
            if (!canInteract)
                return false;
            
            // Check availability conditions
            foreach (var condition in availabilityConditions)
            {
                if (!EvaluateCondition(condition))
                    return false;
            }
            
            // Check interaction conditions
            foreach (var condition in interactionConditions)
            {
                if (!EvaluateCondition(condition))
                    return false;
            }
            
            // Check schedule
            if (hasSchedule && !IsAvailableAtCurrentTime())
                return false;
            
            return true;
        }

        private bool EvaluateCondition(NPCCondition condition)
        {
            switch (condition.conditionType)
            {
                case NPCConditionType.QuestComplete:
                    return IsQuestCompleted(condition.questId);
                
                case NPCConditionType.QuestActive:
                    return IsQuestActive(condition.questId);
                
                case NPCConditionType.RelationshipLevel:
                    return _currentRelationshipLevel >= condition.relationshipLevel;
                
                case NPCConditionType.HasItem:
                    return PlayerHasItem(condition.itemId, condition.itemQuantity);
                
                case NPCConditionType.TimeOfDay:
                    return IsCurrentTimeInRange(condition.startTime, condition.endTime);
                
                case NPCConditionType.DayOfWeek:
                    return DateTime.Now.DayOfWeek == condition.dayOfWeek;
                
                case NPCConditionType.HasBeenMet:
                    return _hasBeenMet == condition.boolValue;
                
                case NPCConditionType.Custom:
                    return EvaluateCustomCondition(condition.customCondition);
                
                default:
                    return true;
            }
        }

        private bool IsAvailableAtCurrentTime()
        {
            if (!hasSchedule || schedule.Count == 0)
                return true;
            
            var currentTime = DateTime.Now.TimeOfDay;
            var currentDay = DateTime.Now.DayOfWeek;
            
            foreach (var entry in schedule)
            {
                if (entry.dayOfWeek == currentDay || entry.dayOfWeek == DayOfWeek.Sunday) // Sunday as "any day"
                {
                    if (currentTime >= entry.startTime && currentTime <= entry.endTime)
                    {
                        return entry.isAvailable;
                    }
                }
            }
            
            return false; // Not available if no schedule entry matches
        }

        #endregion

        #region Relationship System

        private void SetRelationshipLevel(int newLevel)
        {
            int clampedLevel = Mathf.Clamp(newLevel, minRelationshipLevel, maxRelationshipLevel);
            int oldLevel = _currentRelationshipLevel;
            
            if (clampedLevel != oldLevel)
            {
                _currentRelationshipLevel = clampedLevel;
                OnRelationshipChanged?.Invoke(this, clampedLevel);
                
                if (enableDebugLog)
                {
                    Debug.Log($"Relationship with {npcName} changed from {oldLevel} to {clampedLevel}");
                }
            }
        }

        public void ModifyRelationship(int amount)
        {
            CurrentRelationshipLevel += amount;
        }

        public RelationshipTier GetRelationshipTier()
        {
            float percentage = (float)(_currentRelationshipLevel - minRelationshipLevel) / 
                             (maxRelationshipLevel - minRelationshipLevel);
            
            if (percentage >= 0.8f) return RelationshipTier.Excellent;
            if (percentage >= 0.6f) return RelationshipTier.Good;
            if (percentage >= 0.4f) return RelationshipTier.Neutral;
            if (percentage >= 0.2f) return RelationshipTier.Poor;
            return RelationshipTier.Hostile;
        }

        #endregion

        #region State Management

        private void SetState(NPCState newState)
        {
            if (_currentState != newState)
            {
                NPCState oldState = _currentState;
                _currentState = newState;
                OnStateChanged?.Invoke(this, newState);
                
                if (enableDebugLog)
                {
                    Debug.Log($"{npcName} state changed from {oldState} to {newState}");
                }
            }
        }

        #endregion

        #region Quest Integration

        public bool IsQuestGiver(string questId)
        {
            return isQuestGiver && questIds.Contains(questId);
        }

        public bool IsQuestTarget(string questId)
        {
            return isQuestTarget && questIds.Contains(questId);
        }

        public void MarkQuestCompleted(string questId)
        {
            if (!_completedQuests.Contains(questId))
            {
                _completedQuests.Add(questId);
                
                if (enableDebugLog)
                {
                    Debug.Log($"Quest {questId} marked as completed for {npcName}");
                }
            }
        }

        public bool IsQuestCompleted(string questId)
        {
            return _completedQuests.Contains(questId);
        }

        private bool IsQuestActive(string questId)
        {
            // TODO: Implement quest system integration
            return false;
        }

        #endregion

        #region Utility Methods

        public void SetRuntimeData(string key, object value)
        {
            _runtimeData[key] = value;
        }

        public T GetRuntimeData<T>(string key, T defaultValue = default(T))
        {
            if (_runtimeData.ContainsKey(key))
            {
                try
                {
                    return (T)_runtimeData[key];
                }
                catch
                {
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }

        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }

        public void AddTag(string tag)
        {
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        public void RemoveTag(string tag)
        {
            tags.Remove(tag);
        }

        public AudioClip GetRandomVoiceClip()
        {
            if (voiceClips == null || voiceClips.Length == 0)
                return null;
            
            return voiceClips[UnityEngine.Random.Range(0, voiceClips.Length)];
        }

        public void ResetNPC()
        {
            _currentRelationshipLevel = initialRelationshipLevel;
            _hasBeenMet = false;
            _lastInteractionTime = DateTime.MinValue;
            _completedQuests.Clear();
            _runtimeData.Clear();
            _currentState = NPCState.Idle;
            
            if (enableDebugLog)
            {
                Debug.Log($"Reset NPC data for {npcName}");
            }
        }

        #endregion

        #region Stub Methods (To be implemented based on game systems)

        private bool PlayerHasItem(string itemId, int quantity)
        {
            // TODO: Implement item checking
            return true;
        }

        private bool IsCurrentTimeInRange(TimeSpan startTime, TimeSpan endTime)
        {
            var currentTime = DateTime.Now.TimeOfDay;
            return currentTime >= startTime && currentTime <= endTime;
        }

        private bool EvaluateCustomCondition(string customCondition)
        {
            // TODO: Implement custom condition evaluation
            return true;
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Validate NPC Data")]
        private void ValidateInEditor()
        {
            if (IsValid())
            {
                Debug.Log($"NPC Data '{npcName}' is valid!");
            }
            else
            {
                var errors = GetValidationErrors();
                Debug.LogError($"NPC Data '{npcName}' has validation errors:\n{string.Join("\n", errors)}");
            }
        }
        
        [ContextMenu("Reset NPC")]
        private void ResetInEditor()
        {
            ResetNPC();
            Debug.Log($"Reset NPC: {npcName}");
        }
        
        [ContextMenu("Test Interaction")]
        private void TestInteractionInEditor()
        {
            if (Application.isPlaying)
            {
                StartInteraction();
            }
            else
            {
                Debug.Log("Test interaction is only available in play mode");
            }
        }
#endif
    }

    // Supporting classes and enums
    public enum RelationshipType
    {
        Hostile,
        Unfriendly,
        Neutral,
        Friendly,
        Romantic
    }

    public enum RelationshipTier
    {
        Hostile,
        Poor,
        Neutral,
        Good,
        Excellent
    }

    public enum NPCPersonality
    {
        Friendly,
        Grumpy,
        Shy,
        Outgoing,
        Mysterious,
        Wise,
        Humorous,
        Serious
    }

    public enum NPCState
    {
        Idle,
        Talking,
        Busy,
        Sleeping,
        Working,
        Unavailable
    }

    public enum NPCConditionType
    {
        QuestComplete,
        QuestActive,
        RelationshipLevel,
        HasItem,
        TimeOfDay,
        DayOfWeek,
        HasBeenMet,
        Custom
    }

    [System.Serializable]
    public class NPCScheduleEntry
    {
        public DayOfWeek dayOfWeek;
        public TimeSpan startTime;
        public TimeSpan endTime;
        public bool isAvailable = true;
        public string activityDescription;
        public Vector3 location;
    }

    [System.Serializable]
    public class NPCCondition
    {
        public NPCConditionType conditionType;
        public string questId;
        public int relationshipLevel;
        public string itemId;
        public int itemQuantity = 1;
        public TimeSpan startTime;
        public TimeSpan endTime;
        public DayOfWeek dayOfWeek;
        public bool boolValue;
        public string customCondition;
    }
}