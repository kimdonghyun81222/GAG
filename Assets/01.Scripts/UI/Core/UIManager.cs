using System;
using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Core
{
    [Provide]
    public class UIManager : MonoBehaviour, IDependencyProvider
    {
        [Header("UI Management")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GraphicRaycaster raycaster;
        [SerializeField] private bool debugMode = false;
        
        [Header("UI Panels")]
        [SerializeField] private List<UIPanel> registeredPanels = new List<UIPanel>();
        
        [Header("Settings")]
        [SerializeField] private float defaultTransitionDuration = 0.3f;
        [SerializeField] private bool allowMultiplePanels = false;
        [SerializeField] private bool pauseGameOnPanel = true;
        
        // Panel management
        private Dictionary<string, UIPanel> _panelRegistry = new Dictionary<string, UIPanel>();
        private Stack<UIPanel> _panelStack = new Stack<UIPanel>();
        private UIPanel _currentPanel;
        
        // UI state
        private bool _uiActive = false;
        private float _originalTimeScale = 1f;
        
        // Properties
        public Canvas MainCanvas => mainCanvas;
        public bool IsUIActive => _uiActive;
        public UIPanel CurrentPanel => _currentPanel;
        public int PanelStackCount => _panelStack.Count;
        public bool HasActivePanels => _panelStack.Count > 0;
        
        // Events
        public event Action<UIPanel> OnPanelOpened;
        public event Action<UIPanel> OnPanelClosed;
        public event Action<bool> OnUIStateChanged;
        public event Action OnAllPanelsClosed;

        private void Awake()
        {
            InitializeUI();
            RegisterPanels();
        }

        private void Start()
        {
            _originalTimeScale = Time.timeScale;
        }

        private void Update()
        {
            HandleInput();
        }
        
        [Provide]
        public UIManager ProvideUIManager() => this;

        private void InitializeUI()
        {
            if (mainCanvas == null)
                mainCanvas = GetComponent<Canvas>();
            
            if (mainCanvas == null)
                mainCanvas = FindObjectOfType<Canvas>();
            
            if (raycaster == null && mainCanvas != null)
                raycaster = mainCanvas.GetComponent<GraphicRaycaster>();
        }

        private void RegisterPanels()
        {
            // Register panels from the serialized list
            foreach (var panel in registeredPanels)
            {
                if (panel != null)
                {
                    RegisterPanel(panel);
                }
            }
            
            // Auto-find panels in children
            var foundPanels = GetComponentsInChildren<UIPanel>(true);
            foreach (var panel in foundPanels)
            {
                if (!_panelRegistry.ContainsKey(panel.PanelId))
                {
                    RegisterPanel(panel);
                }
            }
        }

        private void HandleInput()
        {
            // Handle escape key to close panels
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (HasActivePanels)
                {
                    CloseTopPanel();
                }
            }
        }

        // Panel registration
        public void RegisterPanel(UIPanel panel)
        {
            if (panel == null) return;
            
            string panelId = panel.PanelId;
            if (string.IsNullOrEmpty(panelId))
            {
                panelId = panel.name;
                panel.SetPanelId(panelId);
            }
            
            if (!_panelRegistry.ContainsKey(panelId))
            {
                _panelRegistry[panelId] = panel;
                panel.Initialize(this);
                
                if (debugMode)
                {
                    Debug.Log($"Registered UI panel: {panelId}");
                }
            }
        }

        public void UnregisterPanel(UIPanel panel)
        {
            if (panel == null) return;
            
            string panelId = panel.PanelId;
            if (_panelRegistry.ContainsKey(panelId))
            {
                _panelRegistry.Remove(panelId);
                
                if (debugMode)
                {
                    Debug.Log($"Unregistered UI panel: {panelId}");
                }
            }
        }

        // Panel operations
        public void OpenPanel(string panelId)
        {
            if (!_panelRegistry.TryGetValue(panelId, out UIPanel panel))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"Panel not found: {panelId}");
                }
                return;
            }
            
            OpenPanel(panel);
        }

        public void OpenPanel(UIPanel panel)
        {
            if (panel == null) return;
            
            // Close current panel if only one is allowed
            if (!allowMultiplePanels && _currentPanel != null && _currentPanel != panel)
            {
                ClosePanel(_currentPanel, false);
            }
            
            // Add to stack if not already there
            if (!_panelStack.Contains(panel))
            {
                _panelStack.Push(panel);
            }
            
            _currentPanel = panel;
            panel.Open();
            
            UpdateUIState();
            OnPanelOpened?.Invoke(panel);
            
            if (debugMode)
            {
                Debug.Log($"Opened panel: {panel.PanelId}");
            }
        }

        public void ClosePanel(string panelId, bool removeFromStack = true)
        {
            if (!_panelRegistry.TryGetValue(panelId, out UIPanel panel))
            {
                if (debugMode)
                {
                    Debug.LogWarning($"Panel not found: {panelId}");
                }
                return;
            }
            
            ClosePanel(panel, removeFromStack);
        }

        public void ClosePanel(UIPanel panel, bool removeFromStack = true)
        {
            if (panel == null) return;
            
            panel.Close();
            
            if (removeFromStack && _panelStack.Contains(panel))
            {
                // Convert to list, remove, and rebuild stack
                var stackList = new List<UIPanel>(_panelStack);
                stackList.Remove(panel);
                _panelStack.Clear();
                
                for (int i = stackList.Count - 1; i >= 0; i--)
                {
                    _panelStack.Push(stackList[i]);
                }
            }
            
            // Update current panel
            if (_currentPanel == panel)
            {
                _currentPanel = _panelStack.Count > 0 ? _panelStack.Peek() : null;
            }
            
            UpdateUIState();
            OnPanelClosed?.Invoke(panel);
            
            if (debugMode)
            {
                Debug.Log($"Closed panel: {panel.PanelId}");
            }
        }

        public void CloseTopPanel()
        {
            if (_panelStack.Count > 0)
            {
                var topPanel = _panelStack.Pop();
                ClosePanel(topPanel, false);
            }
        }

        public void CloseAllPanels()
        {
            var panelsToClose = new List<UIPanel>(_panelStack);
            
            foreach (var panel in panelsToClose)
            {
                ClosePanel(panel, false);
            }
            
            _panelStack.Clear();
            _currentPanel = null;
            
            UpdateUIState();
            OnAllPanelsClosed?.Invoke();
            
            if (debugMode)
            {
                Debug.Log("Closed all panels");
            }
        }

        public void TogglePanel(string panelId)
        {
            if (!_panelRegistry.TryGetValue(panelId, out UIPanel panel)) return;
            
            if (panel.IsOpen)
            {
                ClosePanel(panel);
            }
            else
            {
                OpenPanel(panel);
            }
        }

        // UI state management
        private void UpdateUIState()
        {
            bool wasActive = _uiActive;
            _uiActive = HasActivePanels;
            
            if (pauseGameOnPanel)
            {
                Time.timeScale = _uiActive ? 0f : _originalTimeScale;
            }
            
            if (wasActive != _uiActive)
            {
                OnUIStateChanged?.Invoke(_uiActive);
            }
        }

        // Panel queries
        public UIPanel GetPanel(string panelId)
        {
            _panelRegistry.TryGetValue(panelId, out UIPanel panel);
            return panel;
        }

        public T GetPanel<T>(string panelId) where T : UIPanel
        {
            return GetPanel(panelId) as T;
        }

        public List<UIPanel> GetAllPanels()
        {
            return new List<UIPanel>(_panelRegistry.Values);
        }

        public List<UIPanel> GetOpenPanels()
        {
            return new List<UIPanel>(_panelStack);
        }

        public bool IsPanelOpen(string panelId)
        {
            var panel = GetPanel(panelId);
            return panel != null && panel.IsOpen;
        }

        public bool IsPanelRegistered(string panelId)
        {
            return _panelRegistry.ContainsKey(panelId);
        }

        // Settings
        public void SetPauseGameOnPanel(bool pause)
        {
            pauseGameOnPanel = pause;
            UpdateUIState();
        }

        public void SetAllowMultiplePanels(bool allow)
        {
            allowMultiplePanels = allow;
            
            if (!allow && _panelStack.Count > 1)
            {
                // Keep only the top panel
                var topPanel = _panelStack.Peek();
                CloseAllPanels();
                OpenPanel(topPanel);
            }
        }

        public void SetDefaultTransitionDuration(float duration)
        {
            defaultTransitionDuration = Mathf.Max(0f, duration);
        }

        // Utility methods
        public void ShowNotification(string message, float duration = 3f)
        {
            var notificationPanel = GetPanel("NotificationPanel");
            if (notificationPanel is NotificationPanel notification)
            {
                notification.ShowNotification(message, duration);
            }
            else if (debugMode)
            {
                Debug.Log($"Notification: {message}");
            }
        }

        public void ShowConfirmation(string message, Action onConfirm, Action onCancel = null)
        {
            var confirmationPanel = GetPanel("ConfirmationDialog");
            if (confirmationPanel is ConfirmationDialog confirmation)
            {
                confirmation.ShowConfirmation(message, onConfirm, onCancel);
            }
            else if (debugMode)
            {
                Debug.Log($"Confirmation: {message}");
            }
        }

        // Debug methods
        public void DEBUG_ListAllPanels()
        {
            if (!debugMode) return;
            
            Debug.Log("Registered Panels:");
            foreach (var kvp in _panelRegistry)
            {
                Debug.Log($"- {kvp.Key}: {(kvp.Value.IsOpen ? "Open" : "Closed")}");
            }
        }

        public void DEBUG_OpenPanel(string panelId)
        {
            if (!debugMode) return;
            OpenPanel(panelId);
        }

        public void DEBUG_ClosePanel(string panelId)
        {
            if (!debugMode) return;
            ClosePanel(panelId);
        }

        public void DEBUG_TogglePanel(string panelId)
        {
            if (!debugMode) return;
            TogglePanel(panelId);
        }
    }
}