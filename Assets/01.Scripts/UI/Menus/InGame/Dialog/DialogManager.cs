using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame.Dialog
{
    public class DialogManager : MonoBehaviour
    {
        [Header("Dialog Settings")]
        [SerializeField] private DialogCollection defaultDialogCollection;
        [SerializeField] private bool autoStartOnAwake = false;
        [SerializeField] private string autoStartDialogId;
        
        [Header("Dialog Control")]
        [SerializeField] private bool pauseGameDuringDialog = true;
        [SerializeField] private bool allowSkipText = true;
        [SerializeField] private bool allowSkipDialog = false;
        [SerializeField] private KeyCode skipKey = KeyCode.Space;
        [SerializeField] private KeyCode nextKey = KeyCode.Return;
        
        [Header("Audio")]
        [SerializeField] private AudioSource dialogAudioSource;
        [SerializeField] private AudioClip dialogStartSound;
        [SerializeField] private AudioClip dialogEndSound;
        [SerializeField] private AudioClip choiceSelectSound;
        [SerializeField] private float voiceVolume = 1f;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onDialogStart;
        [SerializeField] private UnityEvent onDialogEnd;
        [SerializeField] private UnityEvent<string> onDialogChanged;
        [SerializeField] private UnityEvent<int> onChoiceSelected;
        
        // Singleton pattern
        public static DialogManager Instance { get; private set; }
        
        // Current dialog state
        private DialogCollection _currentCollection;
        private DialogData _currentDialog;
        private bool _isDialogActive = false;
        private bool _isWaitingForInput = false;
        private bool _isTyping = false;
        private Coroutine _typingCoroutine;
        private Coroutine _autoAdvanceCoroutine;
        
        // Dialog history
        private Stack<string> _dialogHistory = new Stack<string>();
        private List<DialogHistoryEntry> _conversationHistory = new List<DialogHistoryEntry>();
        
        // UI reference
        private DialogUI _dialogUI;
        
        // Input handling
        private bool _inputEnabled = true;
        private float _lastInputTime = 0f;
        private const float INPUT_COOLDOWN = 0.1f;
        
        // Properties
        public bool IsDialogActive => _isDialogActive;
        public bool IsWaitingForInput => _isWaitingForInput;
        public bool IsTyping => _isTyping;
        public DialogData CurrentDialog => _currentDialog;
        public DialogCollection CurrentCollection => _currentCollection;
        public List<DialogHistoryEntry> ConversationHistory => _conversationHistory;
        
        // Events (C# events for type safety)
        public event Action OnDialogStarted;
        public event Action OnDialogEnded;
        public event Action<DialogData> OnDialogChanged;
        public event Action<int, DialogChoice> OnChoiceSelected;
        public event Action<string> OnDialogTextCompleted;

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetupDialogUI();
            
            if (autoStartOnAwake && !string.IsNullOrEmpty(autoStartDialogId))
            {
                StartDialog(autoStartDialogId);
            }
        }

        private void Update()
        {
            if (_isDialogActive && _inputEnabled)
            {
                HandleInput();
            }
        }

        private void InitializeManager()
        {
            // Setup audio source
            if (dialogAudioSource == null)
            {
                dialogAudioSource = gameObject.AddComponent<AudioSource>();
                dialogAudioSource.playOnAwake = false;
                dialogAudioSource.volume = voiceVolume;
            }
            
            // Set default collection
            if (defaultDialogCollection != null)
            {
                _currentCollection = defaultDialogCollection;
            }
            
            Debug.Log("DialogManager initialized successfully");
        }

        private void SetupDialogUI()
        {
            // Find or create dialog UI
            _dialogUI = FindObjectOfType<DialogUI>();
            
            if (_dialogUI == null)
            {
                Debug.LogWarning("DialogUI not found in scene. Dialog system may not work properly.");
            }
            else
            {
                // Setup UI callbacks
                _dialogUI.OnChoiceClicked += OnChoiceClicked;
                _dialogUI.OnDialogClicked += OnDialogClicked;
            }
        }

        #region Public Interface

        /// <summary>
        /// Start a dialog with the specified ID from the current collection
        /// </summary>
        public bool StartDialog(string dialogId)
        {
            return StartDialog(dialogId, _currentCollection);
        }

        /// <summary>
        /// Start a dialog with the specified ID from a specific collection
        /// </summary>
        public bool StartDialog(string dialogId, DialogCollection collection)
        {
            if (string.IsNullOrEmpty(dialogId))
            {
                Debug.LogError("Cannot start dialog: dialogId is null or empty");
                return false;
            }

            if (collection == null)
            {
                Debug.LogError($"Cannot start dialog '{dialogId}': collection is null");
                return false;
            }

            var dialogData = collection.GetDialog(dialogId);
            if (dialogData == null)
            {
                Debug.LogError($"Dialog not found: {dialogId} in collection: {collection.collectionName}");
                return false;
            }

            return StartDialog(dialogData, collection);
        }

        /// <summary>
        /// Start a dialog with specific dialog data
        /// </summary>
        public bool StartDialog(DialogData dialogData, DialogCollection collection = null)
        {
            if (dialogData == null)
            {
                Debug.LogError("Cannot start dialog: dialogData is null");
                return false;
            }

            if (!dialogData.IsValid())
            {
                Debug.LogError($"Cannot start dialog: dialogData is invalid ({dialogData.dialogId})");
                return false;
            }

            // Set collection if provided
            if (collection != null)
            {
                _currentCollection = collection;
            }

            // Stop current dialog if running
            if (_isDialogActive)
            {
                StopDialog();
            }

            // Start new dialog
            _currentDialog = dialogData;
            _isDialogActive = true;
            _isWaitingForInput = false;
            _inputEnabled = true;

            // Clear history for new conversation
            _dialogHistory.Clear();

            // Pause game if required
            if (pauseGameDuringDialog)
            {
                Time.timeScale = 0f;
            }

            // Show dialog UI
            if (_dialogUI != null)
            {
                _dialogUI.ShowDialog();
            }

            // Display the dialog
            DisplayCurrentDialog();

            // Play start sound
            PlayDialogSound(dialogStartSound);

            // Fire events
            onDialogStart?.Invoke();
            OnDialogStarted?.Invoke();

            Debug.Log($"Started dialog: {dialogData.dialogId}");
            return true;
        }

        /// <summary>
        /// Move to the next dialog in sequence
        /// </summary>
        public void ShowNextDialog()
        {
            if (!_isDialogActive || _currentDialog == null)
            {
                Debug.LogWarning("Cannot show next dialog: no active dialog");
                return;
            }

            // If dialog has choices, wait for choice selection
            if (_currentDialog.HasChoices())
            {
                Debug.LogWarning("Cannot auto-advance: dialog has choices");
                return;
            }

            // If dialog is end dialog, end the conversation
            if (_currentDialog.isEndDialog)
            {
                EndDialog();
                return;
            }

            // Move to next dialog
            if (!string.IsNullOrEmpty(_currentDialog.nextDialogId))
            {
                string nextId = _currentDialog.nextDialogId;
                
                // Add current dialog to history
                _dialogHistory.Push(_currentDialog.dialogId);
                
                // Load next dialog
                var nextDialog = _currentCollection.GetDialog(nextId);
                if (nextDialog != null)
                {
                    _currentDialog = nextDialog;
                    DisplayCurrentDialog();
                }
                else
                {
                    Debug.LogError($"Next dialog not found: {nextId}");
                    EndDialog();
                }
            }
            else
            {
                Debug.LogWarning("No next dialog specified");
                EndDialog();
            }
        }

        /// <summary>
        /// Select a choice by index
        /// </summary>
        public void SelectChoice(int choiceIndex)
        {
            if (!_isDialogActive || _currentDialog == null)
            {
                Debug.LogWarning("Cannot select choice: no active dialog");
                return;
            }

            if (!_currentDialog.HasChoices())
            {
                Debug.LogWarning("Cannot select choice: current dialog has no choices");
                return;
            }

            var choice = _currentDialog.GetChoice(choiceIndex);
            if (choice == null)
            {
                Debug.LogError($"Invalid choice index: {choiceIndex}");
                return;
            }

            if (!choice.IsAvailable())
            {
                Debug.LogWarning($"Choice not available: {choice.choiceText}");
                return;
            }

            // Add to conversation history
            AddToConversationHistory(_currentDialog, choice);

            // Play choice sound
            PlayDialogSound(choiceSelectSound);

            // Fire events
            onChoiceSelected?.Invoke(choiceIndex);
            OnChoiceSelected?.Invoke(choiceIndex, choice);

            // Add current dialog to history
            _dialogHistory.Push(_currentDialog.dialogId);

            // Move to target dialog
            if (!string.IsNullOrEmpty(choice.targetDialogId))
            {
                var targetDialog = _currentCollection.GetDialog(choice.targetDialogId);
                if (targetDialog != null)
                {
                    _currentDialog = targetDialog;
                    DisplayCurrentDialog();
                }
                else
                {
                    Debug.LogError($"Target dialog not found: {choice.targetDialogId}");
                    EndDialog();
                }
            }
            else
            {
                // Choice has no target, end dialog
                EndDialog();
            }

            Debug.Log($"Selected choice: {choice.choiceText} -> {choice.targetDialogId}");
        }

        /// <summary>
        /// End the current dialog
        /// </summary>
        public void EndDialog()
        {
            if (!_isDialogActive)
            {
                Debug.LogWarning("Cannot end dialog: no active dialog");
                return;
            }

            // Stop any ongoing coroutines
            StopAllDialogCoroutines();

            // Add final dialog to conversation history
            if (_currentDialog != null)
            {
                AddToConversationHistory(_currentDialog, null);
            }

            // Reset state
            _isDialogActive = false;
            _isWaitingForInput = false;
            _isTyping = false;
            _inputEnabled = false;
            _currentDialog = null;

            // Resume game
            if (pauseGameDuringDialog)
            {
                Time.timeScale = 1f;
            }

            // Hide dialog UI
            if (_dialogUI != null)
            {
                _dialogUI.HideDialog();
            }

            // Play end sound
            PlayDialogSound(dialogEndSound);

            // Fire events
            onDialogEnd?.Invoke();
            OnDialogEnded?.Invoke();

            Debug.Log("Dialog ended");
        }

        /// <summary>
        /// Go back to previous dialog if available
        /// </summary>
        public bool GoBackToPreviousDialog()
        {
            if (!_isDialogActive || _dialogHistory.Count == 0)
            {
                Debug.LogWarning("Cannot go back: no previous dialog in history");
                return false;
            }

            string previousDialogId = _dialogHistory.Pop();
            var previousDialog = _currentCollection.GetDialog(previousDialogId);
            
            if (previousDialog != null)
            {
                _currentDialog = previousDialog;
                DisplayCurrentDialog();
                Debug.Log($"Went back to dialog: {previousDialogId}");
                return true;
            }
            else
            {
                Debug.LogError($"Previous dialog not found: {previousDialogId}");
                return false;
            }
        }

        /// <summary>
        /// Skip current typing animation
        /// </summary>
        public void SkipTyping()
        {
            if (_isTyping && allowSkipText && _typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
                _isTyping = false;

                // Show full text immediately
                if (_dialogUI != null && _currentDialog != null)
                {
                    _dialogUI.SetDialogText(_currentDialog.dialogText, true);
                    OnTypingCompleted();
                }
            }
        }

        /// <summary>
        /// Skip entire dialog (if allowed)
        /// </summary>
        public void SkipDialog()
        {
            if (allowSkipDialog && _isDialogActive)
            {
                EndDialog();
            }
        }

        /// <summary>
        /// Set the current dialog collection
        /// </summary>
        public void SetDialogCollection(DialogCollection collection)
        {
            if (collection != null)
            {
                _currentCollection = collection;
                Debug.Log($"Dialog collection set to: {collection.collectionName}");
            }
            else
            {
                Debug.LogWarning("Cannot set dialog collection: collection is null");
            }
        }

        #endregion

        #region Private Methods

        private void DisplayCurrentDialog()
        {
            if (_currentDialog == null || _dialogUI == null)
            {
                Debug.LogError("Cannot display dialog: currentDialog or dialogUI is null");
                return;
            }

            // Stop any ongoing processes
            StopAllDialogCoroutines();

            // Update UI with dialog data
            _dialogUI.SetSpeakerName(_currentDialog.speakerName);
            _dialogUI.SetSpeakerPortrait(_currentDialog.speakerPortrait);

            // Start typing animation
            if (_currentDialog.textSpeed > 0)
            {
                _typingCoroutine = StartCoroutine(TypeText(_currentDialog.dialogText, _currentDialog.textSpeed));
            }
            else
            {
                // Show text immediately
                _dialogUI.SetDialogText(_currentDialog.dialogText, true);
                OnTypingCompleted();
            }

            // Play voice clip if available
            if (_currentDialog.voiceClip != null && dialogAudioSource != null)
            {
                dialogAudioSource.clip = _currentDialog.voiceClip;
                dialogAudioSource.Play();
            }

            // Fire events
            onDialogChanged?.Invoke(_currentDialog.dialogId);
            OnDialogChanged?.Invoke(_currentDialog);

            Debug.Log($"Displaying dialog: {_currentDialog.dialogId}");
        }

        private IEnumerator TypeText(string text, float speed)
        {
            _isTyping = true;
            _isWaitingForInput = false;

            float delay = 1f / speed;
            string currentText = "";

            for (int i = 0; i <= text.Length; i++)
            {
                currentText = text.Substring(0, i);
                _dialogUI.SetDialogText(currentText, false);

                yield return new WaitForSecondsRealtime(delay);
            }

            _isTyping = false;
            OnTypingCompleted();
        }

        private void OnTypingCompleted()
        {
            _isWaitingForInput = true;

            // Fire event
            OnDialogTextCompleted?.Invoke(_currentDialog.dialogText);

            // Show choices if available
            if (_currentDialog.HasChoices())
            {
                ShowChoices();
            }
            else
            {
                // Auto-advance if specified
                if (_currentDialog.autoAdvanceDelay > 0)
                {
                    _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(_currentDialog.autoAdvanceDelay));
                }
            }
        }

        private void ShowChoices()
        {
            if (_currentDialog == null || !_currentDialog.HasChoices() || _dialogUI == null)
                return;

            // Filter available choices
            var availableChoices = new List<DialogChoice>();
            foreach (var choice in _currentDialog.choices)
            {
                if (choice.IsAvailable())
                {
                    availableChoices.Add(choice);
                }
            }

            if (availableChoices.Count > 0)
            {
                _dialogUI.ShowChoices(availableChoices);
            }
            else
            {
                Debug.LogWarning("No available choices for current dialog");
                // Auto-advance to next dialog or end
                ShowNextDialog();
            }
        }

        private IEnumerator AutoAdvanceAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            if (_isDialogActive && _isWaitingForInput)
            {
                ShowNextDialog();
            }
        }

        private void HandleInput()
        {
            if (Time.unscaledTime - _lastInputTime < INPUT_COOLDOWN)
                return;

            // Skip typing
            if (UnityEngine.Input.GetKeyDown(skipKey))
            {
                if (_isTyping)
                {
                    SkipTyping();
                }
                else if (_isWaitingForInput && !_currentDialog.HasChoices())
                {
                    ShowNextDialog();
                }

                _lastInputTime = Time.unscaledTime;
            }

            // Next dialog
            if (UnityEngine.Input.GetKeyDown(nextKey))
            {
                if (!_isTyping && _isWaitingForInput && !_currentDialog.HasChoices())
                {
                    ShowNextDialog();
                }

                _lastInputTime = Time.unscaledTime;
            }

            // Number key choices
            if (_currentDialog != null && _currentDialog.HasChoices())
            {
                for (int i = 1; i <= 9 && i <= _currentDialog.choices.Count; i++)
                {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0 + i))
                    {
                        SelectChoice(i - 1);
                        _lastInputTime = Time.unscaledTime;
                        break;
                    }
                }
            }

            // ESC to skip dialog
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && allowSkipDialog)
            {
                SkipDialog();
                _lastInputTime = Time.unscaledTime;
            }
        }

        private void OnChoiceClicked(int choiceIndex)
        {
            SelectChoice(choiceIndex);
        }

        private void OnDialogClicked()
        {
            if (_isTyping)
            {
                SkipTyping();
            }
            else if (_isWaitingForInput && !_currentDialog.HasChoices())
            {
                ShowNextDialog();
            }
        }

        private void StopAllDialogCoroutines()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }

            _isTyping = false;
        }

        private void PlayDialogSound(AudioClip clip)
        {
            if (clip != null && dialogAudioSource != null)
            {
                dialogAudioSource.PlayOneShot(clip);
            }
        }

        private void AddToConversationHistory(DialogData dialog, DialogChoice selectedChoice)
        {
            var historyEntry = new DialogHistoryEntry
            {
                dialogId = dialog.dialogId,
                speakerName = dialog.speakerName,
                dialogText = dialog.dialogText,
                selectedChoice = selectedChoice?.choiceText,
                timestamp = System.DateTime.Now
            };

            _conversationHistory.Add(historyEntry);

            // Limit history size
            if (_conversationHistory.Count > 100)
            {
                _conversationHistory.RemoveAt(0);
            }
        }

        private void StopDialog()
        {
            StopAllDialogCoroutines();
            _isDialogActive = false;
            _isWaitingForInput = false;
            _isTyping = false;
            _inputEnabled = false;
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// Check if a specific dialog exists in the current collection
        /// </summary>
        public bool HasDialog(string dialogId)
        {
            return _currentCollection != null && _currentCollection.HasDialog(dialogId);
        }

        /// <summary>
        /// Get all available dialog IDs from current collection
        /// </summary>
        public List<string> GetAllDialogIds()
        {
            return _currentCollection?.GetAllDialogIds() ?? new List<string>();
        }

        /// <summary>
        /// Clear conversation history
        /// </summary>
        public void ClearConversationHistory()
        {
            _conversationHistory.Clear();
            Debug.Log("Conversation history cleared");
        }

        /// <summary>
        /// Enable or disable input handling
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
        }

        /// <summary>
        /// Get the current dialog history stack
        /// </summary>
        public string[] GetDialogHistoryStack()
        {
            return _dialogHistory.ToArray();
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up events
            if (_dialogUI != null)
            {
                _dialogUI.OnChoiceClicked -= OnChoiceClicked;
                _dialogUI.OnDialogClicked -= OnDialogClicked;
            }

            // Resume game time
            if (pauseGameDuringDialog && _isDialogActive)
            {
                Time.timeScale = 1f;
            }
        }
    }

    // Helper class for conversation history
    [System.Serializable]
    public class DialogHistoryEntry
    {
        public string dialogId;
        public string speakerName;
        public string dialogText;
        public string selectedChoice;
        public System.DateTime timestamp;
        
        public override string ToString()
        {
            return $"[{timestamp:HH:mm:ss}] {speakerName}: {dialogText}" + 
                   (string.IsNullOrEmpty(selectedChoice) ? "" : $" -> {selectedChoice}");
        }
    }
}