using System;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Core
{
    public abstract class UIPanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private string panelId;
        [SerializeField] private PanelType panelType = PanelType.Overlay;
        [SerializeField] private bool closeOnEscape = true;
        [SerializeField] private bool closeOnClickOutside = false;
        
        [Header("Animation")]
        [SerializeField] private bool useAnimation = true;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Background")]
        [SerializeField] private bool hasBackground = true;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);
        
        // State
        private bool _isOpen = false;
        private bool _isAnimating = false;
        private UIManager _uiManager;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        
        // Properties
        public string PanelId => panelId;
        public PanelType Type => panelType;
        public bool IsOpen => _isOpen;
        public bool IsAnimating => _isAnimating;
        public bool CloseOnEscape => closeOnEscape;
        public UIManager UIManager => _uiManager;
        
        // Events
        public event Action<UIPanel> OnOpened;
        public event Action<UIPanel> OnClosed;
        public event Action<UIPanel> OnOpenStarted;
        public event Action<UIPanel> OnCloseStarted;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            _rectTransform = GetComponent<RectTransform>();
            
            InitializeBackground();
            
            // Start closed
            gameObject.SetActive(false);
        }

        protected virtual void Start()
        {
            if (string.IsNullOrEmpty(panelId))
            {
                panelId = gameObject.name;
            }
        }

        private void InitializeBackground()
        {
            if (hasBackground && backgroundImage == null)
            {
                // Create background if none exists
                var bgObject = new GameObject("Background");
                bgObject.transform.SetParent(transform, false);
                bgObject.transform.SetAsFirstSibling();
                
                backgroundImage = bgObject.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
                
                var bgRect = backgroundImage.rectTransform;
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                
                if (closeOnClickOutside)
                {
                    var button = bgObject.AddComponent<Button>();
                    button.onClick.AddListener(() => Close());
                }
            }
        }

        // Initialization
        public virtual void Initialize(UIManager uiManager)
        {
            _uiManager = uiManager;
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
            // Override in derived classes
        }

        // Panel operations
        public virtual void Open()
        {
            if (_isOpen || _isAnimating) return;
            
            gameObject.SetActive(true);
            OnOpenStarted?.Invoke(this);
            
            if (useAnimation)
            {
                StartOpenAnimation();
            }
            else
            {
                CompleteOpen();
            }
            
            OnOpen();
        }

        public virtual void Close()
        {
            if (!_isOpen || _isAnimating) return;
            
            OnCloseStarted?.Invoke(this);
            
            if (useAnimation)
            {
                StartCloseAnimation();
            }
            else
            {
                CompleteClose();
            }
            
            OnClose();
        }

        private void StartOpenAnimation()
        {
            _isAnimating = true;
            
            // Set initial state
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            
            if (_rectTransform != null)
            {
                _rectTransform.localScale = Vector3.one * 0.8f;
            }
            
            // Animate
            AnimatePanel(0f, 1f, () => CompleteOpen());
        }

        private void StartCloseAnimation()
        {
            _isAnimating = true;
            _canvasGroup.interactable = false;
            
            // Animate
            AnimatePanel(1f, 0f, () => CompleteClose());
        }

        private void AnimatePanel(float fromAlpha, float toAlpha, Action onComplete)
        {
            float elapsedTime = 0f;
            Vector3 fromScale = _rectTransform != null ? _rectTransform.localScale : Vector3.one;
            Vector3 toScale = toAlpha > 0.5f ? Vector3.one : Vector3.one * 0.8f;
            
            void UpdateAnimation()
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / animationDuration;
                float curveValue = animationCurve.Evaluate(progress);
                
                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, curveValue);
                
                if (_rectTransform != null)
                {
                    _rectTransform.localScale = Vector3.Lerp(fromScale, toScale, curveValue);
                }
                
                if (progress >= 1f)
                {
                    onComplete?.Invoke();
                }
                else
                {
                    // Continue animation next frame
                    UnityEngine.Events.UnityAction updateAction = null;
                    updateAction = () =>
                    {
                        UpdateAnimation();
                    };
                    
                    // Use a simple timer approach
                    StartCoroutine(WaitAndExecute(Time.unscaledDeltaTime, updateAction));
                }
            }
            
            UpdateAnimation();
        }

        private System.Collections.IEnumerator WaitAndExecute(float delay, UnityEngine.Events.UnityAction action)
        {
            yield return new WaitForSecondsRealtime(delay);
            action?.Invoke();
        }

        private void CompleteOpen()
        {
            _isOpen = true;
            _isAnimating = false;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            
            if (_rectTransform != null)
            {
                _rectTransform.localScale = Vector3.one;
            }
            
            OnOpened?.Invoke(this);
        }

        private void CompleteClose()
        {
            _isOpen = false;
            _isAnimating = false;
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            
            gameObject.SetActive(false);
            OnClosed?.Invoke(this);
        }

        // Virtual methods for override
        protected virtual void OnOpen()
        {
            // Override in derived classes
        }

        protected virtual void OnClose()
        {
            // Override in derived classes
        }

        // Utility methods
        public void SetPanelId(string id)
        {
            panelId = id;
        }

        public void SetInteractable(bool interactable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = interactable;
            }
        }

        public void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }
        }

        public void SetCloseOnEscape(bool closeOnEscape)
        {
            this.closeOnEscape = closeOnEscape;
        }

        public void SetAnimationDuration(float duration)
        {
            animationDuration = Mathf.Max(0f, duration);
        }

        // Static helper methods
        public static T FindPanel<T>(string panelId) where T : UIPanel
        {
            var uiManager = FindObjectOfType<UIManager>();
            return uiManager?.GetPanel<T>(panelId);
        }

        public static void OpenPanel(string panelId)
        {
            var uiManager = FindObjectOfType<UIManager>();
            uiManager?.OpenPanel(panelId);
        }

        public static void ClosePanel(string panelId)
        {
            var uiManager = FindObjectOfType<UIManager>();
            uiManager?.ClosePanel(panelId);
        }
    }

    public enum PanelType
    {
        Overlay,
        Modal,
        Popup,
        HUD,
        Menu
    }
}