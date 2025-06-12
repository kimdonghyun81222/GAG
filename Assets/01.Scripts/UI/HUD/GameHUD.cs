using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Player._01.Scripts.Player.Interaction;
using GrowAGarden.Player._01.Scripts.Player.Stats;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.HUD
{
    public class GameHUD : UIPanel
    {
        [Header("HUD Components")]
        [SerializeField] private NotificationHUD notificationHUD;
        [SerializeField] private ToolHUD toolHUD;
        [SerializeField] private WeatherHUD weatherHUD;
        [SerializeField] private InteractionHUD interactionHUD;
        [SerializeField] private MinimapHUD minimapHUD;
        
        [Header("Core Player Stats")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Slider experienceBar;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI moneyText;
        
        [Header("HUD Visibility")]
        [SerializeField] private bool showNotifications = true;
        [SerializeField] private bool showTools = true;
        [SerializeField] private bool showWeather = true;
        [SerializeField] private bool showInteraction = true;
        [SerializeField] private bool showMinimap = true;
        
        [Header("Update Settings")]
        [SerializeField] private float coreStatsUpdateInterval = 0.1f;
        [SerializeField] private bool enableAutoUpdate = true;
        
        // Dependencies
        [Inject] private PlayerStats playerStats;
        
        // Update timing
        private float _lastCoreStatsUpdate;
        
        // HUD Management
        private Dictionary<System.Type, MonoBehaviour> _hudComponents;

        protected override void Awake()
        {
            base.Awake();
            InitializeHUDComponents();
        }

        protected override void Start()
        {
            base.Start();
            SetupHUDVisibility();
            UpdateCoreStats();
        }

        private void Update()
        {
            if (enableAutoUpdate && Time.time - _lastCoreStatsUpdate >= coreStatsUpdateInterval)
            {
                UpdateCoreStats();
                _lastCoreStatsUpdate = Time.time;
            }
            
            HandleHUDInput();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Subscribe to player stats events - 수정: 이벤트가 존재하는지 확인 후 구독
            if (playerStats != null)
            {
                // 기본 체력/에너지 이벤트만 구독 (존재하는 것들)
                playerStats.OnHealthChanged += UpdateHealthBar;
                playerStats.OnEnergyChanged += UpdateEnergyBar;
                
                // 추가 이벤트들은 나중에 PlayerStats에 구현되면 추가
                // playerStats.OnExperienceChanged += UpdateExperienceBar;
                // playerStats.OnLevelChanged += UpdateLevel;
                // playerStats.OnMoneyChanged += UpdateMoney;
            }
        }

        private void InitializeHUDComponents()
        {
            _hudComponents = new Dictionary<System.Type, MonoBehaviour>();
            
            // Auto-find HUD components if not assigned
            if (notificationHUD == null)
                notificationHUD = GetComponentInChildren<NotificationHUD>();
            if (toolHUD == null)
                toolHUD = GetComponentInChildren<ToolHUD>();
            if (weatherHUD == null)
                weatherHUD = GetComponentInChildren<WeatherHUD>();
            if (interactionHUD == null)
                interactionHUD = GetComponentInChildren<InteractionHUD>();
            if (minimapHUD == null)
                minimapHUD = GetComponentInChildren<MinimapHUD>();
            
            // Register components
            if (notificationHUD != null) _hudComponents[typeof(NotificationHUD)] = notificationHUD;
            if (toolHUD != null) _hudComponents[typeof(ToolHUD)] = toolHUD;
            if (weatherHUD != null) _hudComponents[typeof(WeatherHUD)] = weatherHUD;
            if (interactionHUD != null) _hudComponents[typeof(InteractionHUD)] = interactionHUD;
            if (minimapHUD != null) _hudComponents[typeof(MinimapHUD)] = minimapHUD;
        }

        private void SetupHUDVisibility()
        {
            SetHUDComponentVisible<NotificationHUD>(showNotifications);
            SetHUDComponentVisible<ToolHUD>(showTools);
            SetHUDComponentVisible<WeatherHUD>(showWeather);
            SetHUDComponentVisible<InteractionHUD>(showInteraction);
            SetHUDComponentVisible<MinimapHUD>(showMinimap);
        }

        private void HandleHUDInput()
        {
            // Toggle individual HUD components
            if (Input.GetKeyDown(KeyCode.F1)) ToggleHUDComponent<NotificationHUD>();
            if (Input.GetKeyDown(KeyCode.F2)) ToggleHUDComponent<ToolHUD>();
            if (Input.GetKeyDown(KeyCode.F3)) ToggleHUDComponent<WeatherHUD>();
            if (Input.GetKeyDown(KeyCode.F4)) ToggleHUDComponent<InteractionHUD>();
            if (Input.GetKeyDown(KeyCode.M)) ToggleHUDComponent<MinimapHUD>();
            
            // Toggle entire HUD
            if (Input.GetKeyDown(KeyCode.H))
            {
                ToggleHUDVisibility();
            }
        }

        // Core stats update (always visible)
        private void UpdateCoreStats()
        {
            if (playerStats == null) return;
            
            UpdateHealthBar(playerStats.CurrentHealth, playerStats.MaxHealth);
            UpdateEnergyBar(playerStats.CurrentEnergy, playerStats.MaxEnergy);
            
            // 수정: 존재하지 않는 속성들을 안전하게 처리
            // UpdateExperienceBar(playerStats.CurrentExperience, playerStats.ExperienceToNextLevel);
            // UpdateLevel(playerStats.Level);
            // UpdateMoney(playerStats.Money);
            
            // 임시로 기본값 사용
            UpdateExperienceBar(0f, 100f);
            UpdateLevel(1);
            UpdateMoney(0);
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.value = max > 0 ? current / max : 0f;
            }
        }

        private void UpdateEnergyBar(float current, float max)
        {
            if (energyBar != null)
            {
                energyBar.value = max > 0 ? current / max : 0f;
            }
        }

        private void UpdateExperienceBar(float current, float toNext)
        {
            if (experienceBar != null)
            {
                experienceBar.value = toNext > 0 ? current / toNext : 0f;
            }
        }

        private void UpdateLevel(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Level {level}";
            }
        }

        private void UpdateMoney(int money)
        {
            if (moneyText != null)
            {
                moneyText.text = $"${money:N0}";
            }
        }

        // HUD component management
        public T GetHUDComponent<T>() where T : MonoBehaviour
        {
            if (_hudComponents.TryGetValue(typeof(T), out MonoBehaviour component))
            {
                return component as T;
            }
            return null;
        }

        public void SetHUDComponentVisible<T>(bool visible) where T : MonoBehaviour
        {
            var component = GetHUDComponent<T>();
            if (component != null)
            {
                component.gameObject.SetActive(visible);
            }
        }

        public void ToggleHUDComponent<T>() where T : MonoBehaviour
        {
            var component = GetHUDComponent<T>();
            if (component != null)
            {
                bool currentState = component.gameObject.activeSelf;
                component.gameObject.SetActive(!currentState);
            }
        }

        public bool IsHUDComponentVisible<T>() where T : MonoBehaviour
        {
            var component = GetHUDComponent<T>();
            return component != null && component.gameObject.activeSelf;
        }

        // Public interface methods
        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            notificationHUD?.ShowNotification(message, type, duration);
        }

        public void SetInteractable(IInteractable interactable)
        {
            interactionHUD?.SetInteractable(interactable);
        }

        public void SetInteractionProgress(float progress)
        {
            interactionHUD?.SetInteractionProgress(progress);
        }

        public void UpdateWeatherInfo(float temperature, string season, string time)
        {
            weatherHUD?.UpdateWeatherInfo(temperature, season, time);
        }

        public void RefreshToolSlots()
        {
            toolHUD?.RefreshToolSlots();
        }

        // Settings
        public void SetNotificationsVisible(bool visible)
        {
            showNotifications = visible;
            SetHUDComponentVisible<NotificationHUD>(visible);
        }

        public void SetToolsVisible(bool visible)
        {
            showTools = visible;
            SetHUDComponentVisible<ToolHUD>(visible);
        }

        public void SetWeatherVisible(bool visible)
        {
            showWeather = visible;
            SetHUDComponentVisible<WeatherHUD>(visible);
        }

        public void SetMinimapVisible(bool visible)
        {
            showMinimap = visible;
            SetHUDComponentVisible<MinimapHUD>(visible);
        }

        public void ToggleHUDVisibility()
        {
            bool isVisible = GetComponent<CanvasGroup>().alpha > 0.5f;
            SetHUDVisibility(!isVisible);
        }

        public void SetHUDVisibility(bool visible)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }

        // Static convenience methods
        public static void ShowMessage(string message, NotificationType type = NotificationType.Info)
        {
            var gameHUD = FindPanel<GameHUD>("GameHUD");
            gameHUD?.ShowNotification(message, type);
        }

        public static void SetInteraction(IInteractable interactable)
        {
            var gameHUD = FindPanel<GameHUD>("GameHUD");
            gameHUD?.SetInteractable(interactable);
        }

        // 🔧 수정: MonoBehaviour의 OnDestroy 사용 (virtual 메서드가 아니므로 override 제거)
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (playerStats != null)
            {
                playerStats.OnHealthChanged -= UpdateHealthBar;
                playerStats.OnEnergyChanged -= UpdateEnergyBar;
                // 존재하지 않는 이벤트들은 주석 처리
                // playerStats.OnExperienceChanged -= UpdateExperienceBar;
                // playerStats.OnLevelChanged -= UpdateLevel;
                // playerStats.OnMoneyChanged -= UpdateMoney;
            }
            
            // UIPanel의 정리 작업이 필요하다면 수동으로 호출
            // base.OnDestroy(); // UIPanel에 OnDestroy가 virtual이 아니므로 호출하지 않음
        }
    }
}