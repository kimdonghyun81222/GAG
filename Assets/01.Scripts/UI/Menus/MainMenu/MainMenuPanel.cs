using System.Collections;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.MainMenu
{
    public class MainMenuPanel : UIPanel
    {
        [Header("Main Menu Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitGameButton;
        
        [Header("Sub Panels")]
        [SerializeField] private NewGamePanel newGamePanel;
        [SerializeField] private LoadGamePanel loadGamePanel;
        [SerializeField] private GameObject creditsPanel;
        
        [Header("Main Menu UI")]
        [SerializeField] private TextMeshProUGUI gameTitle;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject mainButtonContainer;
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableMenuAnimations = true;
        [SerializeField] private float buttonAnimationDelay = 0.1f;
        [SerializeField] private float titleAnimationDuration = 1f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip menuMusic;
        
        [Header("Background Effects")]
        [SerializeField] private ParticleSystem backgroundParticles;
        [SerializeField] private bool enableBackgroundAnimation = true;
        [SerializeField] private float backgroundAnimationSpeed = 1f;
        
        [Header("Scene Management")]
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private string loadingSceneName = "LoadingScene";
        [SerializeField] private bool useLoadingScreen = true;
        
        // State management
        private bool _isTransitioning = false;
        private Coroutine _backgroundAnimationCoroutine;
        private CanvasGroup _canvasGroup;

        protected override void Awake()
        {
            base.Awake();
            InitializeMainMenu();
        }

        protected override void Start()
        {
            base.Start();
            SetupMainMenu();
            PlayMenuAnimations();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Check for existing save files
            UpdateLoadGameButton();
        }

        private void InitializeMainMenu()
        {
            // Get or add CanvasGroup for fading
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Auto-find sub panels if not assigned
            if (newGamePanel == null)
                newGamePanel = FindObjectOfType<NewGamePanel>();
            
            if (loadGamePanel == null)
                loadGamePanel = FindObjectOfType<LoadGamePanel>();
            
            // Set initial state
            _canvasGroup.alpha = 0f;
            
            // Set game title and version
            if (gameTitle != null)
            {
                gameTitle.text = "Grow A Garden";
            }
            
            if (versionText != null)
            {
                versionText.text = $"Version {Application.version}";
            }
        }

        private void SetupMainMenu()
        {
            // Setup button listeners
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
                AddButtonEffects(newGameButton);
            }
            
            if (loadGameButton != null)
            {
                loadGameButton.onClick.AddListener(OnLoadGameClicked);
                AddButtonEffects(loadGameButton);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
                AddButtonEffects(settingsButton);
            }
            
            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OnCreditsClicked);
                AddButtonEffects(creditsButton);
            }
            
            if (quitGameButton != null)
            {
                quitGameButton.onClick.AddListener(OnQuitGameClicked);
                AddButtonEffects(quitGameButton);
            }
            
            // Start background effects
            if (enableBackgroundAnimation)
            {
                StartBackgroundAnimation();
            }
            
            // Start background particles
            if (backgroundParticles != null)
            {
                backgroundParticles.Play();
            }
        }

        private void AddButtonEffects(Button button)
        {
            if (button == null) return;
            
            // Add hover effects
            var buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null)
            {
                // Add EventTrigger for hover effects
                var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                }
                
                // Pointer Enter
                var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((eventData) => OnButtonHover(button));
                eventTrigger.triggers.Add(pointerEnter);
                
                // Pointer Exit
                var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((eventData) => OnButtonExitHover(button));
                eventTrigger.triggers.Add(pointerExit);
            }
        }

        private void OnButtonHover(Button button)
        {
            // Scale animation
            if (enableMenuAnimations)
            {
                StartCoroutine(ScaleButton(button.transform, Vector3.one * 1.05f, 0.1f));
            }
        }

        private void OnButtonExitHover(Button button)
        {
            // Reset scale
            if (enableMenuAnimations)
            {
                StartCoroutine(ScaleButton(button.transform, Vector3.one, 0.1f));
            }
        }

        private IEnumerator ScaleButton(Transform buttonTransform, Vector3 targetScale, float duration)
        {
            Vector3 startScale = buttonTransform.localScale;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                buttonTransform.localScale = Vector3.Lerp(startScale, targetScale, fadeInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            buttonTransform.localScale = targetScale;
        }

        private void PlayMenuAnimations()
        {
            if (!enableMenuAnimations) 
            {
                _canvasGroup.alpha = 1f;
                return;
            }
            
            StartCoroutine(AnimateMenuEntrance());
        }

        private IEnumerator AnimateMenuEntrance()
        {
            // Fade in main menu
            yield return StartCoroutine(FadeInMenu());
            
            // Animate title
            if (gameTitle != null)
            {
                yield return StartCoroutine(AnimateTitle());
            }
            
            // Animate buttons one by one
            yield return StartCoroutine(AnimateButtons());
        }

        private IEnumerator FadeInMenu()
        {
            float elapsedTime = 0f;
            float duration = 0.5f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                
                _canvasGroup.alpha = fadeInCurve.Evaluate(progress);
                
                yield return null;
            }
            
            _canvasGroup.alpha = 1f;
        }

        private IEnumerator AnimateTitle()
        {
            var titleTransform = gameTitle.transform;
            var startScale = titleTransform.localScale;
            titleTransform.localScale = Vector3.zero;
            
            float elapsedTime = 0f;
            
            while (elapsedTime < titleAnimationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / titleAnimationDuration;
                
                titleTransform.localScale = Vector3.Lerp(Vector3.zero, startScale, fadeInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            titleTransform.localScale = startScale;
        }

        private IEnumerator AnimateButtons()
        {
            var buttons = new Button[] { newGameButton, loadGameButton, settingsButton, creditsButton, quitGameButton };
            
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    StartCoroutine(AnimateButtonEntrance(button.transform));
                    yield return new WaitForSecondsRealtime(buttonAnimationDelay);
                }
            }
        }

        private IEnumerator AnimateButtonEntrance(Transform buttonTransform)
        {
            var startPos = buttonTransform.localPosition;
            var offsetPos = startPos + Vector3.left * 200f;
            buttonTransform.localPosition = offsetPos;
            
            var canvasGroup = buttonTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = buttonTransform.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            
            float elapsedTime = 0f;
            float duration = 0.3f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / duration;
                float curveValue = fadeInCurve.Evaluate(progress);
                
                buttonTransform.localPosition = Vector3.Lerp(offsetPos, startPos, curveValue);
                canvasGroup.alpha = curveValue;
                
                yield return null;
            }
            
            buttonTransform.localPosition = startPos;
            canvasGroup.alpha = 1f;
        }

        private void StartBackgroundAnimation()
        {
            if (_backgroundAnimationCoroutine != null)
            {
                StopCoroutine(_backgroundAnimationCoroutine);
            }
            
            _backgroundAnimationCoroutine = StartCoroutine(AnimateBackground());
        }

        private IEnumerator AnimateBackground()
        {
            if (backgroundImage == null) yield break;
            
            var rectTransform = backgroundImage.rectTransform;
            var startPos = rectTransform.anchoredPosition;
            
            while (enableBackgroundAnimation)
            {
                float time = Time.time * backgroundAnimationSpeed;
                float offsetX = Mathf.Sin(time * 0.5f) * 10f;
                float offsetY = Mathf.Cos(time * 0.3f) * 5f;
                
                rectTransform.anchoredPosition = startPos + new Vector2(offsetX, offsetY);
                
                yield return null;
            }
            
            rectTransform.anchoredPosition = startPos;
        }

        private void Update()
        {
            HandleMenuInput();
        }

        private void HandleMenuInput()
        {
            if (_isTransitioning) return;
            
            // ESC to quit
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnQuitGameClicked();
            }
            
            // Enter to start new game
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnNewGameClicked();
            }
            
            // Keyboard navigation
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Handle keyboard navigation between buttons
                NavigateButtons(Input.GetKeyDown(KeyCode.UpArrow) ? -1 : 1);
            }
        }

        private void NavigateButtons(int direction)
        {
            // Simple keyboard navigation implementation
            var buttons = new Button[] { newGameButton, loadGameButton, settingsButton, creditsButton, quitGameButton };
            
            // Find currently selected button and move to next/previous
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].gameObject == UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject)
                {
                    int newIndex = (i + direction + buttons.Length) % buttons.Length;
                    if (buttons[newIndex] != null && buttons[newIndex].interactable)
                    {
                        buttons[newIndex].Select();
                    }
                    break;
                }
            }
        }

        private void UpdateLoadGameButton()
        {
            if (loadGameButton == null) return;
            
            // Check if save files exist
            bool hasSaveFiles = true; // 임시로 항상 true
            loadGameButton.interactable = hasSaveFiles;
            
            // Visual feedback for disabled button
            var buttonColors = loadGameButton.colors;
            buttonColors.disabledColor = Color.gray;
            loadGameButton.colors = buttonColors;
        }

        // Button click handlers
        private void OnNewGameClicked()
        {
            if (_isTransitioning) return;
            
            PlayButtonClickSound();
            
            if (newGamePanel != null)
            {
                HideMainMenu();
                // 🔧 수정: ShowPanel() 대신 gameObject.SetActive(true) 사용
                newGamePanel.gameObject.SetActive(true);
            }
            else
            {
                // Direct new game start
                StartNewGame();
            }
        }

        private void OnLoadGameClicked()
        {
            if (_isTransitioning) return;
            
            PlayButtonClickSound();
            
            if (loadGamePanel != null)
            {
                HideMainMenu();
                // 🔧 수정: ShowPanel() 대신 gameObject.SetActive(true) 사용
                loadGamePanel.gameObject.SetActive(true);
            }
            else
            {
                // Direct load game
                LoadGame();
            }
        }

        private void OnSettingsClicked()
        {
            if (_isTransitioning) return;
            
            PlayButtonClickSound();
            
            Debug.Log("Settings panel not implemented yet!");
        }

        private void OnCreditsClicked()
        {
            if (_isTransitioning) return;
            
            PlayButtonClickSound();
            
            if (creditsPanel != null)
            {
                HideMainMenu();
                creditsPanel.SetActive(true);
            }
        }

        private void OnQuitGameClicked()
        {
            if (_isTransitioning) return;
            
            PlayButtonClickSound();
            QuitGame();
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        // Game flow methods
        private void StartNewGame()
        {
            _isTransitioning = true;
            
            // Load game scene
            StartCoroutine(LoadGameScene());
        }

        private void LoadGame()
        {
            _isTransitioning = true;
            
            // Load game scene
            StartCoroutine(LoadGameScene());
        }

        private IEnumerator LoadGameScene()
        {
            // Fade out menu
            yield return StartCoroutine(FadeOutMenu());
            
            // Stop background effects
            if (backgroundParticles != null)
            {
                backgroundParticles.Stop();
            }
            
            // Load scene
            if (useLoadingScreen && !string.IsNullOrEmpty(loadingSceneName))
            {
                SceneManager.LoadScene(loadingSceneName);
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }

        private IEnumerator FadeOutMenu()
        {
            float elapsedTime = 0f;
            float duration = 0.5f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = 1f - (elapsedTime / duration);
                
                _canvasGroup.alpha = progress;
                
                yield return null;
            }
            
            _canvasGroup.alpha = 0f;
        }

        private void QuitGame()
        {
            _isTransitioning = true;
            
            StartCoroutine(QuitGameCoroutine());
        }

        private IEnumerator QuitGameCoroutine()
        {
            // Fade out menu
            yield return StartCoroutine(FadeOutMenu());
            
            // Quit application
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // Panel management
        private void HideMainMenu()
        {
            if (mainButtonContainer != null)
            {
                mainButtonContainer.SetActive(false);
            }
        }

        public void ShowMainMenu()
        {
            if (mainButtonContainer != null)
            {
                mainButtonContainer.SetActive(true);
            }
            
            UpdateLoadGameButton();
        }

        // Public interface for sub panels to return to main menu
        public void ReturnToMainMenu()
        {
            ShowMainMenu();
            
            if (newGamePanel != null) newGamePanel.gameObject.SetActive(false);
            if (loadGamePanel != null) loadGamePanel.gameObject.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            // Stop background animation
            if (_backgroundAnimationCoroutine != null)
            {
                StopCoroutine(_backgroundAnimationCoroutine);
            }
            
            // Stop all other coroutines
            StopAllCoroutines();
        }
    }
}