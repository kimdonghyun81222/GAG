using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame.Dialog
{
    public class DialogUI : MonoBehaviour
    {
        [Header("Main Dialog Panel")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private CanvasGroup dialogCanvasGroup;
        [SerializeField] private Image dialogBackground;
        
        [Header("Speaker Info")]
        [SerializeField] private GameObject speakerPanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private Image speakerPortrait;
        [SerializeField] private GameObject speakerPortraitFrame;
        
        [Header("Dialog Text")]
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private ScrollRect dialogScrollRect;
        [SerializeField] private Image dialogTextBackground;
        
        [Header("Choice System")]
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private VerticalLayoutGroup choiceLayoutGroup;
        
        [Header("Continue Indicator")]
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private Image continueArrow;
        [SerializeField] private AnimationCurve continueArrowCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableAnimations = true;
        [SerializeField] private float showAnimationDuration = 0.3f;
        [SerializeField] private float hideAnimationDuration = 0.2f;
        [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem dialogShowEffect;
        [SerializeField] private ParticleSystem choiceShowEffect;
        [SerializeField] private Color typingTextColor = Color.white;
        [SerializeField] private Color completedTextColor = Color.white;
        
        [Header("Layout Settings")]
        [SerializeField] private float choiceButtonHeight = 50f;
        [SerializeField] private float choiceButtonSpacing = 10f;
        [SerializeField] private int maxVisibleChoices = 5;
        [SerializeField] private bool autoSizeDialogPanel = true;
        
        [Header("Input Settings")]
        [SerializeField] private bool allowClickToAdvance = true;
        [SerializeField] private bool allowClickToSkip = true;
        [SerializeField] private Button dialogClickArea;
        
        // UI State
        private bool _isVisible = false;
        private bool _isAnimating = false;
        private bool _isShowingChoices = false;
        private List<GameObject> _choiceButtons = new List<GameObject>();
        
        // Animation
        private Coroutine _showCoroutine;
        private Coroutine _hideCoroutine;
        private Coroutine _continueIndicatorCoroutine;
        
        // Layout
        private RectTransform _dialogPanelRect;
        private RectTransform _choiceContainerRect;
        
        // Events
        public event Action<int> OnChoiceClicked;
        public event Action OnDialogClicked;
        public event Action OnDialogShown;
        public event Action OnDialogHidden;
        
        // Properties
        public bool IsVisible => _isVisible;
        public bool IsAnimating => _isAnimating;
        public bool IsShowingChoices => _isShowingChoices;

        private void Awake()
        {
            InitializeUI();
        }

        private void Start()
        {
            SetupUI();
            
            // Start hidden
            HideDialogImmediate();
        }

        private void InitializeUI()
        {
            // Get rect transforms
            if (dialogPanel != null)
            {
                _dialogPanelRect = dialogPanel.GetComponent<RectTransform>();
            }
            
            if (choiceContainer != null)
            {
                _choiceContainerRect = choiceContainer.GetComponent<RectTransform>();
            }
            
            // Setup canvas group
            if (dialogCanvasGroup == null && dialogPanel != null)
            {
                dialogCanvasGroup = dialogPanel.GetComponent<CanvasGroup>();
                if (dialogCanvasGroup == null)
                {
                    dialogCanvasGroup = dialogPanel.AddComponent<CanvasGroup>();
                }
            }
            
            // Create default choice button if needed
            if (choiceButtonPrefab == null)
            {
                CreateDefaultChoiceButton();
            }
            
            // Setup click area
            if (dialogClickArea == null && dialogPanel != null)
            {
                SetupDialogClickArea();
            }
        }

        private void SetupUI()
        {
            // Setup choice layout
            if (choiceLayoutGroup != null)
            {
                choiceLayoutGroup.spacing = choiceButtonSpacing;
                choiceLayoutGroup.childControlHeight = true;
                choiceLayoutGroup.childControlWidth = true;
                choiceLayoutGroup.childForceExpandHeight = false;
                choiceLayoutGroup.childForceExpandWidth = true;
            }
            
            // Setup dialog click handler
            if (dialogClickArea != null)
            {
                dialogClickArea.onClick.AddListener(OnDialogAreaClicked);
            }
            
            // Hide choice panel initially
            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
            
            // Hide continue indicator initially
            if (continueIndicator != null)
            {
                continueIndicator.SetActive(false);
            }
            
            Debug.Log("DialogUI setup completed");
        }

        #region Public Interface

        /// <summary>
        /// Show the dialog panel
        /// </summary>
        public void ShowDialog()
        {
            if (_isVisible) return;
            
            gameObject.SetActive(true);
            
            if (enableAnimations)
            {
                if (_hideCoroutine != null)
                {
                    StopCoroutine(_hideCoroutine);
                    _hideCoroutine = null;
                }
                
                _showCoroutine = StartCoroutine(ShowDialogAnimated());
            }
            else
            {
                ShowDialogImmediate();
            }
            
            // Play show effect
            if (dialogShowEffect != null)
            {
                dialogShowEffect.Play();
            }
        }

        /// <summary>
        /// Hide the dialog panel
        /// </summary>
        public void HideDialog()
        {
            if (!_isVisible) return;
            
            if (enableAnimations)
            {
                if (_showCoroutine != null)
                {
                    StopCoroutine(_showCoroutine);
                    _showCoroutine = null;
                }
                
                _hideCoroutine = StartCoroutine(HideDialogAnimated());
            }
            else
            {
                HideDialogImmediate();
            }
        }

        /// <summary>
        /// Set the speaker name
        /// </summary>
        public void SetSpeakerName(string speakerName)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = speakerName ?? "";
            }
            
            // Show/hide speaker panel based on whether we have a name
            if (speakerPanel != null)
            {
                speakerPanel.SetActive(!string.IsNullOrEmpty(speakerName));
            }
        }

        /// <summary>
        /// Set the speaker portrait
        /// </summary>
        public void SetSpeakerPortrait(Sprite portrait)
        {
            if (speakerPortrait != null)
            {
                speakerPortrait.sprite = portrait;
                speakerPortrait.gameObject.SetActive(portrait != null);
            }
            
            // Show/hide portrait frame
            if (speakerPortraitFrame != null)
            {
                speakerPortraitFrame.SetActive(portrait != null);
            }
        }

        /// <summary>
        /// Set the dialog text
        /// </summary>
        public void SetDialogText(string text, bool isComplete = false)
        {
            if (dialogText != null)
            {
                dialogText.text = text ?? "";
                
                // Change text color based on completion state
                dialogText.color = isComplete ? completedTextColor : typingTextColor;
                
                // Auto-scroll to bottom if needed
                if (dialogScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    dialogScrollRect.verticalNormalizedPosition = 0f;
                }
            }
            
            // Show/hide continue indicator based on completion
            SetContinueIndicatorVisible(isComplete);
        }

        /// <summary>
        /// Show choices
        /// </summary>
        public void ShowChoices(List<DialogChoice> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                HideChoices();
                return;
            }
            
            // Clear existing choices
            ClearChoices();
            
            // Create choice buttons
            for (int i = 0; i < choices.Count; i++)
            {
                CreateChoiceButton(i, choices[i]);
            }
            
            // Show choice panel
            if (choicePanel != null)
            {
                choicePanel.SetActive(true);
            }
            
            _isShowingChoices = true;
            
            // Hide continue indicator when showing choices
            SetContinueIndicatorVisible(false);
            
            // Play choice show effect
            if (choiceShowEffect != null)
            {
                choiceShowEffect.Play();
            }
            
            // Update layout
            UpdateLayout();
            
            Debug.Log($"Showing {choices.Count} choices");
        }

        /// <summary>
        /// Hide choices
        /// </summary>
        public void HideChoices()
        {
            ClearChoices();
            
            if (choicePanel != null)
            {
                choicePanel.SetActive(false);
            }
            
            _isShowingChoices = false;
            
            UpdateLayout();
        }

        #endregion

        #region Private Methods

        private void ShowDialogImmediate()
        {
            _isVisible = true;
            _isAnimating = false;
            
            if (dialogCanvasGroup != null)
            {
                dialogCanvasGroup.alpha = 1f;
                dialogCanvasGroup.interactable = true;
                dialogCanvasGroup.blocksRaycasts = true;
            }
            
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(true);
            }
            
            OnDialogShown?.Invoke();
        }

        private void HideDialogImmediate()
        {
            _isVisible = false;
            _isAnimating = false;
            
            if (dialogCanvasGroup != null)
            {
                dialogCanvasGroup.alpha = 0f;
                dialogCanvasGroup.interactable = false;
                dialogCanvasGroup.blocksRaycasts = false;
            }
            
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
            
            // Hide choices as well
            HideChoices();
            
            // Hide continue indicator
            SetContinueIndicatorVisible(false);
            
            OnDialogHidden?.Invoke();
        }

        private IEnumerator ShowDialogAnimated()
        {
            _isAnimating = true;
            _isVisible = true;
            
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(true);
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < showAnimationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / showAnimationDuration;
                float curveValue = showCurve.Evaluate(progress);
                
                if (dialogCanvasGroup != null)
                {
                    dialogCanvasGroup.alpha = curveValue;
                }
                
                // Scale animation
                if (_dialogPanelRect != null)
                {
                    float scale = Mathf.Lerp(0.8f, 1f, curveValue);
                    _dialogPanelRect.localScale = Vector3.one * scale;
                }
                
                yield return null;
            }
            
            // Ensure final state
            if (dialogCanvasGroup != null)
            {
                dialogCanvasGroup.alpha = 1f;
                dialogCanvasGroup.interactable = true;
                dialogCanvasGroup.blocksRaycasts = true;
            }
            
            if (_dialogPanelRect != null)
            {
                _dialogPanelRect.localScale = Vector3.one;
            }
            
            _isAnimating = false;
            _showCoroutine = null;
            
            OnDialogShown?.Invoke();
        }

        private IEnumerator HideDialogAnimated()
        {
            _isAnimating = true;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < hideAnimationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / hideAnimationDuration;
                float curveValue = hideCurve.Evaluate(progress);
                
                if (dialogCanvasGroup != null)
                {
                    dialogCanvasGroup.alpha = curveValue;
                    dialogCanvasGroup.interactable = false;
                    dialogCanvasGroup.blocksRaycasts = false;
                }
                
                // Scale animation
                if (_dialogPanelRect != null)
                {
                    float scale = Mathf.Lerp(1f, 0.8f, progress);
                    _dialogPanelRect.localScale = Vector3.one * scale;
                }
                
                yield return null;
            }
            
            // Ensure final state
            HideDialogImmediate();
            
            _isAnimating = false;
            _hideCoroutine = null;
            
            OnDialogHidden?.Invoke();
        }

        private void CreateChoiceButton(int index, DialogChoice choice)
        {
            if (choiceButtonPrefab == null || choiceContainer == null)
            {
                Debug.LogError("Cannot create choice button: prefab or container is null");
                return;
            }
            
            var buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
            buttonObj.name = $"Choice_{index}";
            
            // Setup choice text
            var choiceText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (choiceText != null)
            {
                choiceText.text = $"{index + 1}. {choice.choiceText}";
            }
            
            // Setup button click
            var button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int capturedIndex = index; // Capture for closure
                button.onClick.AddListener(() => OnChoiceButtonClicked(capturedIndex));
                
                // Set interactable state
                button.interactable = choice.IsAvailable();
            }
            
            // Setup layout element
            var layoutElement = buttonObj.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = buttonObj.AddComponent<LayoutElement>();
            }
            layoutElement.preferredHeight = choiceButtonHeight;
            layoutElement.flexibleWidth = 1f;
            
            // Add to list for tracking
            _choiceButtons.Add(buttonObj);
            
            // Enable the button
            buttonObj.SetActive(true);
        }

        private void ClearChoices()
        {
            foreach (var button in _choiceButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            
            _choiceButtons.Clear();
        }

        private void CreateDefaultChoiceButton()
        {
            var buttonObj = new GameObject("ChoiceButton");
            buttonObj.AddComponent<RectTransform>();
            
            // Add Image component for background
            var bgImage = buttonObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add Button component
            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bgImage;
            
            // Create text child
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 5f);
            textRect.offsetMax = new Vector2(-10f, -5f);
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Choice Text";
            text.fontSize = 16f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            
            // Setup button colors
            var colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            button.colors = colors;
            
            choiceButtonPrefab = buttonObj;
            choiceButtonPrefab.SetActive(false);
            
            Debug.Log("Created default choice button prefab");
        }

        private void SetupDialogClickArea()
        {
            if (dialogPanel == null) return;
            
            var clickAreaObj = new GameObject("DialogClickArea");
            clickAreaObj.transform.SetParent(dialogPanel.transform, false);
            
            var clickAreaRect = clickAreaObj.AddComponent<RectTransform>();
            clickAreaRect.anchorMin = Vector2.zero;
            clickAreaRect.anchorMax = Vector2.one;
            clickAreaRect.offsetMin = Vector2.zero;
            clickAreaRect.offsetMax = Vector2.zero;
            
            // Add invisible image for raycast target
            var clickAreaImage = clickAreaObj.AddComponent<Image>();
            clickAreaImage.color = Color.clear;
            clickAreaImage.raycastTarget = true;
            
            dialogClickArea = clickAreaObj.AddComponent<Button>();
            dialogClickArea.targetGraphic = clickAreaImage;
            
            // Remove button visual effects
            var colors = dialogClickArea.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = Color.clear;
            colors.pressedColor = Color.clear;
            colors.selectedColor = Color.clear;
            colors.disabledColor = Color.clear;
            dialogClickArea.colors = colors;
            
            Debug.Log("Created dialog click area");
        }

        private void SetContinueIndicatorVisible(bool visible)
        {
            if (continueIndicator == null) return;
            
            continueIndicator.SetActive(visible && !_isShowingChoices);
            
            if (visible && !_isShowingChoices)
            {
                // Start continue indicator animation
                if (_continueIndicatorCoroutine != null)
                {
                    StopCoroutine(_continueIndicatorCoroutine);
                }
                _continueIndicatorCoroutine = StartCoroutine(AnimateContinueIndicator());
            }
            else
            {
                // Stop continue indicator animation
                if (_continueIndicatorCoroutine != null)
                {
                    StopCoroutine(_continueIndicatorCoroutine);
                    _continueIndicatorCoroutine = null;
                }
            }
        }

        private IEnumerator AnimateContinueIndicator()
        {
            if (continueArrow == null) yield break;
            
            float time = 0f;
            Color originalColor = continueArrow.color;
            
            while (true)
            {
                time += Time.unscaledDeltaTime;
                float alpha = continueArrowCurve.Evaluate((time % 2f) / 2f);
                
                continueArrow.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                
                yield return null;
            }
        }

        private void UpdateLayout()
        {
            if (!autoSizeDialogPanel) return;
            
            // Force layout rebuild
            Canvas.ForceUpdateCanvases();
            
            if (choiceLayoutGroup != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(choiceLayoutGroup.GetComponent<RectTransform>());
            }
        }

        private void OnChoiceButtonClicked(int choiceIndex)
        {
            OnChoiceClicked?.Invoke(choiceIndex);
            Debug.Log($"Choice button clicked: {choiceIndex}");
        }

        private void OnDialogAreaClicked()
        {
            if (allowClickToAdvance || allowClickToSkip)
            {
                OnDialogClicked?.Invoke();
                Debug.Log("Dialog area clicked");
            }
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// Set the visibility of the dialog panel without animation
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (visible)
            {
                ShowDialogImmediate();
            }
            else
            {
                HideDialogImmediate();
            }
        }

        /// <summary>
        /// Check if choices are currently being displayed
        /// </summary>
        public bool HasChoicesVisible()
        {
            return _isShowingChoices && _choiceButtons.Count > 0;
        }

        /// <summary>
        /// Get the number of currently visible choices
        /// </summary>
        public int GetVisibleChoiceCount()
        {
            return _choiceButtons.Count;
        }

        /// <summary>
        /// Enable or disable animations
        /// </summary>
        public void SetAnimationsEnabled(bool enabled)
        {
            enableAnimations = enabled;
        }

        /// <summary>
        /// Set the dialog background color
        /// </summary>
        public void SetDialogBackgroundColor(Color color)
        {
            if (dialogBackground != null)
            {
                dialogBackground.color = color;
            }
        }

        /// <summary>
        /// Set the text background color
        /// </summary>
        public void SetTextBackgroundColor(Color color)
        {
            if (dialogTextBackground != null)
            {
                dialogTextBackground.color = color;
            }
        }

        /// <summary>
        /// Force update layout immediately
        /// </summary>
        public void ForceUpdateLayout()
        {
            UpdateLayout();
        }

        #endregion

        private void OnDestroy()
        {
            // Stop all coroutines
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }
            
            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }
            
            if (_continueIndicatorCoroutine != null)
            {
                StopCoroutine(_continueIndicatorCoroutine);
            }
            
            // Clear choice buttons
            ClearChoices();
        }

#if UNITY_EDITOR
        [ContextMenu("Test Show Dialog")]
        private void TestShowDialog()
        {
            ShowDialog();
            SetSpeakerName("Test Speaker");
            SetDialogText("This is a test dialog message.", true);
        }
        
        [ContextMenu("Test Hide Dialog")]
        private void TestHideDialog()
        {
            HideDialog();
        }
        
        [ContextMenu("Test Show Choices")]
        private void TestShowChoices()
        {
            var testChoices = new List<DialogChoice>
            {
                new DialogChoice { choiceText = "Test Choice 1", targetDialogId = "test1" },
                new DialogChoice { choiceText = "Test Choice 2", targetDialogId = "test2" },
                new DialogChoice { choiceText = "Test Choice 3", targetDialogId = "test3" }
            };
            
            ShowChoices(testChoices);
        }
#endif
    }
}