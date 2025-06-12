using GrowAGarden.Player._01.Scripts.Player.Interaction;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.HUD
{
    public class InteractionHUD : UIPanel
    {
        [Header("Interaction Display")]
        [SerializeField] private GameObject interactionContainer;
        [SerializeField] private TextMeshProUGUI interactionText;
        [SerializeField] private Image interactionIcon;
        [SerializeField] private TextMeshProUGUI keyPromptText;
        
        [Header("Progress Display")]
        [SerializeField] private GameObject progressContainer;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        
        [Header("Crosshair")]
        [SerializeField] private GameObject crosshair;
        [SerializeField] private Image crosshairImage;
        [SerializeField] private RectTransform crosshairTransform;
        
        [Header("Colors")]
        [SerializeField] private Color defaultCrosshairColor = Color.white;
        [SerializeField] private Color interactableCrosshairColor = Color.green;
        [SerializeField] private Color invalidCrosshairColor = Color.red;
        
        [Header("Animation")]
        [SerializeField] private bool enablePulseAnimation = true;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 1.2f;
        
        [Header("Settings")]
        [SerializeField] private string defaultKeyPrompt = "E";
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        
        // State
        private IInteractable _currentInteractable;
        private bool _isInteracting = false;
        private float _interactionProgress = 0f;
        private CrosshairState _crosshairState = CrosshairState.Default;
        
        // Animation
        private Coroutine _fadeCoroutine;
        private Vector3 _originalCrosshairScale;

        protected override void Awake()
        {
            base.Awake();
            InitializeComponents();
        }

        protected override void Start()
        {
            base.Start();
            
            if (crosshairTransform != null)
            {
                _originalCrosshairScale = crosshairTransform.localScale;
            }
            
            SetInteractionVisible(false);
            SetProgressVisible(false);
        }

        private void Update()
        {
            UpdateCrosshair();
            UpdatePulseAnimation();
        }

        private void InitializeComponents()
        {
            // Auto-find components if not assigned
            if (interactionText == null)
                interactionText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (interactionIcon == null)
                interactionIcon = GetComponentInChildren<Image>();
            
            if (crosshair == null)
            {
                var crosshairObj = transform.Find("Crosshair");
                if (crosshairObj != null)
                {
                    crosshair = crosshairObj.gameObject;
                    crosshairImage = crosshair.GetComponent<Image>();
                    crosshairTransform = crosshair.GetComponent<RectTransform>();
                }
            }
            
            if (keyPromptText == null)
            {
                var keyPrompt = transform.Find("KeyPrompt");
                if (keyPrompt != null)
                {
                    keyPromptText = keyPrompt.GetComponent<TextMeshProUGUI>();
                }
            }
            
            // Set default key prompt
            if (keyPromptText != null)
            {
                keyPromptText.text = defaultKeyPrompt;
            }
        }

        // Public interface
        public void SetInteractable(IInteractable interactable)
        {
            _currentInteractable = interactable;
            
            if (interactable != null)
            {
                ShowInteractionUI(interactable.GetInteractionText()); // 🔧 수정: 메서드명 변경
                SetCrosshairState(interactable.CanInteract() ? CrosshairState.Interactable : CrosshairState.Invalid);
            }
            else
            {
                HideInteractionUI(); // 🔧 수정: 메서드명 변경
                SetCrosshairState(CrosshairState.Default);
            }
        }

        public void SetInteractionProgress(float progress, bool showProgress = true)
        {
            _interactionProgress = Mathf.Clamp01(progress);
            _isInteracting = showProgress && progress > 0f && progress < 1f;
            
            if (showProgress)
            {
                ShowProgress(_interactionProgress);
            }
            else
            {
                HideProgress();
            }
        }

        public void SetCrosshairState(CrosshairState state)
        {
            _crosshairState = state;
        }

        public void SetCrosshairVisible(bool visible)
        {
            if (crosshair != null)
            {
                crosshair.SetActive(visible);
            }
        }

        public void SetKeyPrompt(string key)
        {
            if (keyPromptText != null)
            {
                keyPromptText.text = key;
            }
        }

        // 🔧 수정: 중복 메서드 제거 및 이름 변경
        private void ShowInteractionUI(string text)
        {
            if (interactionText != null)
            {
                interactionText.text = text;
            }
            
            SetInteractionVisible(true);
        }

        private void HideInteractionUI()
        {
            SetInteractionVisible(false);
        }

        private void ShowProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
            
            if (progressText != null)
            {
                progressText.text = $"{progress:P0}";
            }
            
            SetProgressVisible(true);
        }

        private void HideProgress()
        {
            SetProgressVisible(false);
        }

        private void SetInteractionVisible(bool visible)
        {
            if (interactionContainer != null)
            {
                if (_fadeCoroutine != null)
                {
                    StopCoroutine(_fadeCoroutine);
                }
                
                _fadeCoroutine = StartCoroutine(FadeContainer(interactionContainer, visible));
            }
        }

        private void SetProgressVisible(bool visible)
        {
            if (progressContainer != null)
            {
                if (_fadeCoroutine != null)
                {
                    StopCoroutine(_fadeCoroutine);
                }
                
                _fadeCoroutine = StartCoroutine(FadeContainer(progressContainer, visible));
            }
        }

        private System.Collections.IEnumerator FadeContainer(GameObject container, bool fadeIn)
        {
            var canvasGroup = container.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = container.AddComponent<CanvasGroup>();
            }
            
            float duration = fadeIn ? fadeInDuration : fadeOutDuration;
            float startAlpha = canvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;
            
            container.SetActive(true);
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
            
            if (!fadeIn)
            {
                container.SetActive(false);
            }
        }

        private void UpdateCrosshair()
        {
            if (crosshairImage == null) return;
            
            Color targetColor = _crosshairState switch
            {
                CrosshairState.Default => defaultCrosshairColor,
                CrosshairState.Interactable => interactableCrosshairColor,
                CrosshairState.Invalid => invalidCrosshairColor,
                _ => defaultCrosshairColor
            };
            
            crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 5f);
        }

        private void UpdatePulseAnimation()
        {
            if (!enablePulseAnimation || crosshairTransform == null || _crosshairState != CrosshairState.Interactable)
            {
                // Reset scale when not pulsing
                if (crosshairTransform != null && crosshairTransform.localScale != _originalCrosshairScale)
                {
                    crosshairTransform.localScale = Vector3.Lerp(crosshairTransform.localScale, _originalCrosshairScale, Time.deltaTime * 5f);
                }
                return;
            }
            
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            float scale = Mathf.Lerp(1f, pulseScale, pulse);
            
            crosshairTransform.localScale = _originalCrosshairScale * scale;
        }

        // Utility methods
        public bool HasInteractable()
        {
            return _currentInteractable != null;
        }

        public bool IsInteracting()
        {
            return _isInteracting;
        }

        public IInteractable GetCurrentInteractable()
        {
            return _currentInteractable;
        }

        public float GetInteractionProgress()
        {
            return _interactionProgress;
        }

        // 🔧 수정: 중복 static 메서드 제거
        public static void ShowInteractionStatic(string text)
        {
            var hud = FindPanel<InteractionHUD>("InteractionHUD");
            hud?.ShowInteractionUI(text);
        }

        public static void HideInteractionStatic()
        {
            var hud = FindPanel<InteractionHUD>("InteractionHUD");
            hud?.HideInteractionUI();
        }

        public static void SetProgress(float progress)
        {
            var hud = FindPanel<InteractionHUD>("InteractionHUD");
            hud?.SetInteractionProgress(progress);
        }

        public static void SetCrosshair(CrosshairState state)
        {
            var hud = FindPanel<InteractionHUD>("InteractionHUD");
            hud?.SetCrosshairState(state);
        }
    }

    public enum CrosshairState
    {
        Default,
        Interactable,
        Invalid
    }
}