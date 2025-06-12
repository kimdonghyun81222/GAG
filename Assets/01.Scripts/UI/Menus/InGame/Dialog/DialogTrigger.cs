using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame.Dialog
{
    public class DialogTrigger : MonoBehaviour
    {
        [Header("NPC Configuration")]
        [SerializeField] private NPCData npcData;
        [SerializeField] private bool useCustomDialog = false;
        [SerializeField] private DialogCollection customDialogCollection;
        [SerializeField] private string customStartDialogId;
        
        [Header("Trigger Settings")]
        [SerializeField] private TriggerType triggerType = TriggerType.OnTriggerEnter;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private LayerMask playerLayerMask = 1;
        [SerializeField] private bool requiresPlayerInput = true;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        
        [Header("Interaction Range")]
        [SerializeField] private bool useCustomRange = false;
        [SerializeField] private float customInteractionRange = 2f;
        [SerializeField] private bool showRangeGizmo = true;
        [SerializeField] private Color rangeGizmoColor = Color.yellow;
        
        [Header("UI Indicators")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private Canvas interactionCanvas;
        [SerializeField] private float promptOffset = 1.5f;
        [SerializeField] private bool rotatePromptToPlayer = true;
        
        [Header("Cooldown")]
        [SerializeField] private bool hasCooldown = false;
        [SerializeField] private float cooldownDuration = 1f;
        [SerializeField] private bool showCooldownTimer = false;
        
        [Header("Conditions")]
        [SerializeField] private bool checkConditionsBeforeInteraction = true;
        [SerializeField] private List<TriggerCondition> triggerConditions = new List<TriggerCondition>();
        
        [Header("Animation")]
        [SerializeField] private Animator npcAnimator;
        [SerializeField] private string interactionTriggerName = "StartInteraction";
        [SerializeField] private string idleTriggerName = "EndInteraction";
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip interactionStartSound;
        [SerializeField] private AudioClip interactionEndSound;
        [SerializeField] private AudioClip cannotInteractSound;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onInteractionAvailable;
        [SerializeField] private UnityEvent onInteractionUnavailable;
        [SerializeField] private UnityEvent onInteractionStarted;
        [SerializeField] private UnityEvent onInteractionEnded;
        [SerializeField] private UnityEvent onCooldownStarted;
        [SerializeField] private UnityEvent onCooldownEnded;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;
        [SerializeField] private bool showDebugInfo = false;
        
        // State management
        private bool _playerInRange = false;
        private bool _canInteract = false;
        private bool _isInteracting = false;
        private bool _isOnCooldown = false;
        private float _cooldownTimer = 0f;
        private GameObject _currentPlayer;
        private Camera _playerCamera;
        
        // Components
        private Collider _triggerCollider;
        private SphereCollider _sphereCollider;
        private BoxCollider _boxCollider;
        
        // UI Management
        private bool _promptVisible = false;
        private Coroutine _promptUpdateCoroutine;
        
        // Events
        public event Action<DialogTrigger, GameObject> OnPlayerEntered;
        public event Action<DialogTrigger, GameObject> OnPlayerExited;
        public event Action<DialogTrigger> OnInteractionStarted;
        public event Action<DialogTrigger> OnInteractionEnded;
        public event Action<DialogTrigger> OnCooldownStarted;
        public event Action<DialogTrigger> OnCooldownEnded;
        
        // Properties
        public bool PlayerInRange => _playerInRange;
        public bool CanInteract => _canInteract;
        public bool IsInteracting => _isInteracting;
        public bool IsOnCooldown => _isOnCooldown;
        public float CooldownTimer => _cooldownTimer;
        public NPCData NPCData => npcData;
        public GameObject CurrentPlayer => _currentPlayer;
        
        private void Awake()
        {
            InitializeTrigger();
        }
        
        private void Start()
        {
            SetupTrigger();
        }
        
        private void Update()
        {
            UpdateTrigger();
        }
        
        private void InitializeTrigger()
        {
            // Get or create collider for trigger
            _triggerCollider = GetComponent<Collider>();
            if (_triggerCollider == null)
            {
                CreateDefaultTriggerCollider();
            }
            else
            {
                _triggerCollider.isTrigger = true;
            }
            
            // Setup audio source
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1f; // 3D sound
                }
            }
            
            // Setup animator
            if (npcAnimator == null)
            {
                npcAnimator = GetComponentInChildren<Animator>();
            }
            
            // Validate NPC data
            if (npcData != null)
            {
                if (!npcData.IsValid())
                {
                    Debug.LogWarning($"NPC Data validation failed for {gameObject.name}");
                }
            }
            else if (!useCustomDialog)
            {
                Debug.LogWarning($"No NPC Data assigned to DialogTrigger on {gameObject.name}");
            }
        }
        
        private void SetupTrigger()
        {
            // Setup interaction range
            SetupInteractionRange();
            
            // Setup interaction prompt
            SetupInteractionPrompt();
            
            // Initial state
            UpdateInteractionState();
            
            if (enableDebugLog)
            {
                Debug.Log($"DialogTrigger setup completed for {gameObject.name}");
            }
        }
        
        private void UpdateTrigger()
        {
            // Update cooldown
            UpdateCooldown();
            
            // Handle input
            if (_playerInRange && !_isInteracting)
            {
                HandlePlayerInput();
            }
            
            // Update interaction state
            UpdateInteractionState();
            
            // Update UI prompt
            UpdateInteractionPrompt();
        }
        
        #region Trigger Events
        
        private void OnTriggerEnter(Collider other)
        {
            if (triggerType == TriggerType.OnTriggerEnter || triggerType == TriggerType.OnTriggerStay)
            {
                HandlePlayerEnter(other);
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (triggerType == TriggerType.OnTriggerStay)
            {
                if (!_playerInRange)
                {
                    HandlePlayerEnter(other);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other.gameObject))
            {
                HandlePlayerExit(other);
            }
        }
        
        #endregion
        
        #region Player Detection
        
        private void HandlePlayerEnter(Collider other)
        {
            if (IsPlayer(other.gameObject) && !_playerInRange)
            {
                _currentPlayer = other.gameObject;
                _playerInRange = true;
                _playerCamera = _currentPlayer.GetComponentInChildren<Camera>();
                
                // Fire events
                OnPlayerEntered?.Invoke(this, _currentPlayer);
                onInteractionAvailable?.Invoke();
                
                // Show interaction prompt
                ShowInteractionPrompt();
                
                if (enableDebugLog)
                {
                    Debug.Log($"Player entered interaction range of {gameObject.name}");
                }
            }
        }
        
        private void HandlePlayerExit(Collider other)
        {
            if (IsPlayer(other.gameObject) && _playerInRange)
            {
                _playerInRange = false;
                _currentPlayer = null;
                _playerCamera = null;
                
                // End interaction if active
                if (_isInteracting)
                {
                    EndInteraction();
                }
                
                // Fire events
                OnPlayerExited?.Invoke(this, other.gameObject);
                onInteractionUnavailable?.Invoke();
                
                // Hide interaction prompt
                HideInteractionPrompt();
                
                if (enableDebugLog)
                {
                    Debug.Log($"Player exited interaction range of {gameObject.name}");
                }
            }
        }
        
        private bool IsPlayer(GameObject obj)
        {
            // Check by tag
            if (!string.IsNullOrEmpty(playerTag) && obj.CompareTag(playerTag))
                return true;
            
            // Check by layer
            if (((1 << obj.layer) & playerLayerMask) != 0)
                return true;
            
            return false;
        }
        
        #endregion
        
        #region Input Handling
        
        private void HandlePlayerInput()
        {
            if (!requiresPlayerInput)
            {
                // Auto-start interaction
                if (_canInteract && !_isInteracting)
                {
                    StartInteraction();
                }
                return;
            }
            
            // Check for interaction key
            KeyCode keyToUse = npcData != null ? npcData.interactionKey : interactionKey;
            
            if (Input.GetKeyDown(keyToUse))
            {
                if (_canInteract && !_isInteracting)
                {
                    StartInteraction();
                }
                else if (!_canInteract)
                {
                    PlayCannotInteractFeedback();
                }
            }
        }
        
        #endregion
        
        #region Interaction Management
        
        public bool StartInteraction()
        {
            if (!_canInteract || _isInteracting || _isOnCooldown)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"Cannot start interaction: CanInteract={_canInteract}, IsInteracting={_isInteracting}, IsOnCooldown={_isOnCooldown}");
                }
                return false;
            }
            
            _isInteracting = true;
            
            // Hide interaction prompt
            HideInteractionPrompt();
            
            // Play start sound
            PlayAudioClip(interactionStartSound);
            
            // Trigger animation
            TriggerAnimation(interactionTriggerName);
            
            // Start dialog
            bool dialogStarted = false;
            
            if (useCustomDialog)
            {
                if (customDialogCollection != null && !string.IsNullOrEmpty(customStartDialogId))
                {
                    dialogStarted = DialogManager.Instance.StartDialog(customStartDialogId, customDialogCollection);
                }
            }
            else if (npcData != null)
            {
                dialogStarted = npcData.StartInteraction();
            }
            
            if (dialogStarted)
            {
                // Subscribe to dialog end event
                DialogManager.Instance.OnDialogEnded += OnDialogEnded;
                
                // Fire events
                OnInteractionStarted?.Invoke(this);
                onInteractionStarted?.Invoke();
                
                if (enableDebugLog)
                {
                    Debug.Log($"Interaction started with {gameObject.name}");
                }
                
                return true;
            }
            else
            {
                // Failed to start dialog, reset state
                _isInteracting = false;
                ShowInteractionPrompt();
                
                if (enableDebugLog)
                {
                    Debug.LogError($"Failed to start dialog for {gameObject.name}");
                }
                
                return false;
            }
        }
        
        public void EndInteraction()
        {
            if (!_isInteracting) return;
            
            _isInteracting = false;
            
            // Unsubscribe from dialog events
            if (DialogManager.Instance != null)
            {
                DialogManager.Instance.OnDialogEnded -= OnDialogEnded;
            }
            
            // End NPC interaction
            if (npcData != null)
            {
                npcData.EndInteraction();
            }
            
            // Play end sound
            PlayAudioClip(interactionEndSound);
            
            // Trigger animation
            TriggerAnimation(idleTriggerName);
            
            // Start cooldown
            if (hasCooldown)
            {
                StartCooldown();
            }
            
            // Show interaction prompt if player still in range
            if (_playerInRange)
            {
                ShowInteractionPrompt();
            }
            
            // Fire events
            OnInteractionEnded?.Invoke(this);
            onInteractionEnded?.Invoke();
            
            if (enableDebugLog)
            {
                Debug.Log($"Interaction ended with {gameObject.name}");
            }
        }
        
        private void OnDialogEnded()
        {
            EndInteraction();
        }
        
        #endregion
        
        #region Conditions
        
        private bool CheckInteractionConditions()
        {
            // Check NPC conditions
            if (npcData != null && !npcData.CanInteractNow())
            {
                return false;
            }
            
            // Check trigger conditions
            if (checkConditionsBeforeInteraction)
            {
                foreach (var condition in triggerConditions)
                {
                    if (!EvaluateCondition(condition))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        private bool EvaluateCondition(TriggerCondition condition)
        {
            switch (condition.conditionType)
            {
                case TriggerConditionType.HasItem:
                    return PlayerHasItem(condition.itemId, condition.itemQuantity);
                
                case TriggerConditionType.QuestComplete:
                    return IsQuestCompleted(condition.questId);
                
                case TriggerConditionType.QuestActive:
                    return IsQuestActive(condition.questId);
                
                case TriggerConditionType.TimeOfDay:
                    return IsCurrentTimeInRange(condition.startTime, condition.endTime);
                
                case TriggerConditionType.PlayerLevel:
                    return GetPlayerLevel() >= condition.playerLevel;
                
                case TriggerConditionType.Custom:
                    return EvaluateCustomCondition(condition.customCondition);
                
                default:
                    return true;
            }
        }
        
        #endregion
        
        #region State Management
        
        private void UpdateInteractionState()
        {
            bool oldCanInteract = _canInteract;
            _canInteract = _playerInRange && !_isInteracting && !_isOnCooldown && CheckInteractionConditions();
            
            if (oldCanInteract != _canInteract)
            {
                if (_canInteract)
                {
                    onInteractionAvailable?.Invoke();
                }
                else
                {
                    onInteractionUnavailable?.Invoke();
                }
            }
        }
        
        #endregion
        
        #region Cooldown System
        
        private void UpdateCooldown()
        {
            if (_isOnCooldown)
            {
                _cooldownTimer -= Time.deltaTime;
                
                if (_cooldownTimer <= 0f)
                {
                    EndCooldown();
                }
            }
        }
        
        private void StartCooldown()
        {
            _isOnCooldown = true;
            _cooldownTimer = cooldownDuration;
            
            OnCooldownStarted?.Invoke(this);
            onCooldownStarted?.Invoke();
            
            if (enableDebugLog)
            {
                Debug.Log($"Cooldown started for {gameObject.name} ({cooldownDuration}s)");
            }
        }
        
        private void EndCooldown()
        {
            _isOnCooldown = false;
            _cooldownTimer = 0f;
            
            OnCooldownEnded?.Invoke(this);
            onCooldownEnded?.Invoke();
            
            if (enableDebugLog)
            {
                Debug.Log($"Cooldown ended for {gameObject.name}");
            }
        }
        
        #endregion
        
        #region UI Management
        
        private void SetupInteractionPrompt()
        {
            if (interactionPrompt != null)
            {
                // Setup canvas if needed
                if (interactionCanvas == null)
                {
                    interactionCanvas = interactionPrompt.GetComponentInParent<Canvas>();
                }
                
                // Start hidden
                interactionPrompt.SetActive(false);
            }
        }
        
        private void ShowInteractionPrompt()
        {
            if (interactionPrompt != null && _canInteract && !_isInteracting)
            {
                interactionPrompt.SetActive(true);
                _promptVisible = true;
                
                // Start prompt update coroutine
                if (_promptUpdateCoroutine == null)
                {
                    _promptUpdateCoroutine = StartCoroutine(UpdatePromptPosition());
                }
            }
        }
        
        private void HideInteractionPrompt()
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
                _promptVisible = false;
                
                // Stop prompt update coroutine
                if (_promptUpdateCoroutine != null)
                {
                    StopCoroutine(_promptUpdateCoroutine);
                    _promptUpdateCoroutine = null;
                }
            }
        }
        
        private void UpdateInteractionPrompt()
        {
            if (_promptVisible && !_canInteract)
            {
                HideInteractionPrompt();
            }
            else if (!_promptVisible && _canInteract && _playerInRange && !_isInteracting)
            {
                ShowInteractionPrompt();
            }
        }
        
        private IEnumerator UpdatePromptPosition()
        {
            while (_promptVisible && interactionPrompt != null)
            {
                // Position prompt above NPC
                Vector3 worldPosition = transform.position + Vector3.up * promptOffset;
                
                if (interactionCanvas != null && interactionCanvas.renderMode == RenderMode.WorldSpace)
                {
                    interactionPrompt.transform.position = worldPosition;
                }
                else if (_playerCamera != null)
                {
                    Vector3 screenPosition = _playerCamera.WorldToScreenPoint(worldPosition);
                    interactionPrompt.transform.position = screenPosition;
                }
                
                // Rotate to face player
                if (rotatePromptToPlayer && _currentPlayer != null)
                {
                    Vector3 directionToPlayer = (_currentPlayer.transform.position - interactionPrompt.transform.position).normalized;
                    if (directionToPlayer != Vector3.zero)
                    {
                        interactionPrompt.transform.rotation = Quaternion.LookRotation(directionToPlayer);
                    }
                }
                
                yield return null;
            }
        }
        
        #endregion
        
        #region Audio Management
        
        private void PlayAudioClip(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void PlayCannotInteractFeedback()
        {
            PlayAudioClip(cannotInteractSound);
            
            if (enableDebugLog)
            {
                Debug.Log($"Cannot interact with {gameObject.name} - conditions not met");
            }
        }
        
        #endregion
        
        #region Animation Management
        
        private void TriggerAnimation(string triggerName)
        {
            if (npcAnimator != null && !string.IsNullOrEmpty(triggerName))
            {
                npcAnimator.SetTrigger(triggerName);
            }
        }
        
        #endregion
        
        #region Setup Methods
        
        private void CreateDefaultTriggerCollider()
        {
            float range = useCustomRange ? customInteractionRange : 
                         (npcData != null ? npcData.interactionRange : 2f);
            
            _sphereCollider = gameObject.AddComponent<SphereCollider>();
            _sphereCollider.isTrigger = true;
            _sphereCollider.radius = range;
            _triggerCollider = _sphereCollider;
            
            if (enableDebugLog)
            {
                Debug.Log($"Created default sphere collider with radius {range} for {gameObject.name}");
            }
        }
        
        private void SetupInteractionRange()
        {
            float range = useCustomRange ? customInteractionRange : 
                         (npcData != null ? npcData.interactionRange : 2f);
            
            if (_sphereCollider != null)
            {
                _sphereCollider.radius = range;
            }
            else if (_boxCollider != null)
            {
                _boxCollider.size = Vector3.one * range;
            }
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Manually trigger interaction (for testing or scripted events)
        /// </summary>
        public void TriggerInteraction()
        {
            if (_canInteract)
            {
                StartInteraction();
            }
        }
        
        /// <summary>
        /// Force end current interaction
        /// </summary>
        public void ForceEndInteraction()
        {
            if (_isInteracting)
            {
                EndInteraction();
            }
        }
        
        /// <summary>
        /// Set NPC data at runtime
        /// </summary>
        public void SetNPCData(NPCData newNPCData)
        {
            npcData = newNPCData;
            SetupInteractionRange();
            UpdateInteractionState();
        }
        
        /// <summary>
        /// Enable or disable this trigger
        /// </summary>
        public void SetTriggerEnabled(bool enabled)
        {
            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = enabled;
            }
            
            this.enabled = enabled;
        }
        
        /// <summary>
        /// Get interaction progress (for UI)
        /// </summary>
        public float GetInteractionProgress()
        {
            if (_isOnCooldown)
            {
                return 1f - (_cooldownTimer / cooldownDuration);
            }
            
            return _canInteract ? 1f : 0f;
        }
        
        #endregion
        
        #region Stub Methods (To be implemented based on game systems)
        
        private bool PlayerHasItem(string itemId, int quantity)
        {
            // TODO: Implement item checking
            return true;
        }
        
        private bool IsQuestCompleted(string questId)
        {
            // TODO: Implement quest checking
            return false;
        }
        
        private bool IsQuestActive(string questId)
        {
            // TODO: Implement quest checking
            return false;
        }
        
        private bool IsCurrentTimeInRange(float startTime, float endTime)
        {
            float currentTime = Time.time % 86400f; // Seconds in a day
            return currentTime >= startTime && currentTime <= endTime;
        }
        
        private int GetPlayerLevel()
        {
            // TODO: Implement player level checking
            return 1;
        }
        
        private bool EvaluateCustomCondition(string condition)
        {
            // TODO: Implement custom condition evaluation
            return true;
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmos()
        {
            if (!showRangeGizmo) return;
            
            float range = useCustomRange ? customInteractionRange : 
                         (npcData != null ? npcData.interactionRange : 2f);
            
            Gizmos.color = rangeGizmoColor;
            Gizmos.DrawWireSphere(transform.position, range);
            
            if (showDebugInfo && _playerInRange)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, range * 1.1f);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showRangeGizmo) return;
            
            float range = useCustomRange ? customInteractionRange : 
                         (npcData != null ? npcData.interactionRange : 2f);
            
            Gizmos.color = rangeGizmoColor * 0.3f;
            Gizmos.DrawSphere(transform.position, range);
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Clean up events
            if (DialogManager.Instance != null)
            {
                DialogManager.Instance.OnDialogEnded -= OnDialogEnded;
            }
            
            // Stop coroutines
            if (_promptUpdateCoroutine != null)
            {
                StopCoroutine(_promptUpdateCoroutine);
            }
        }
        
#if UNITY_EDITOR
        [ContextMenu("Test Start Interaction")]
        private void TestStartInteraction()
        {
            if (Application.isPlaying)
            {
                TriggerInteraction();
            }
            else
            {
                Debug.Log("Test interaction is only available in play mode");
            }
        }
        
        [ContextMenu("Validate Setup")]
        private void ValidateSetup()
        {
            List<string> issues = new List<string>();
            
            if (npcData == null && !useCustomDialog)
                issues.Add("No NPC Data assigned and not using custom dialog");
            
            if (useCustomDialog && (customDialogCollection == null || string.IsNullOrEmpty(customStartDialogId)))
                issues.Add("Custom dialog enabled but collection or start ID not set");
            
            if (GetComponent<Collider>() == null)
                issues.Add("No Collider component found");
            
            if (issues.Count > 0)
            {
                Debug.LogWarning($"DialogTrigger validation issues:\n{string.Join("\n", issues)}");
            }
            else
            {
                Debug.Log("DialogTrigger setup is valid!");
            }
        }
#endif
    }
    
    // Supporting enums and classes
    public enum TriggerType
    {
        OnTriggerEnter,
        OnTriggerStay,
        Manual
    }
    
    public enum TriggerConditionType
    {
        HasItem,
        QuestComplete,
        QuestActive,
        TimeOfDay,
        PlayerLevel,
        Custom
    }
    
    [System.Serializable]
    public class TriggerCondition
    {
        public TriggerConditionType conditionType;
        public string itemId;
        public int itemQuantity = 1;
        public string questId;
        public float startTime;
        public float endTime;
        public int playerLevel;
        public string customCondition;
    }
}