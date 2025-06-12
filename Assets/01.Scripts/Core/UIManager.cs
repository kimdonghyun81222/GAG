using TMPro;
using UnityEngine;

namespace _01.Scripts.Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI currencyText;
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 2f;

        // 다른 UI 패널 참조들 (예: 인벤토리, 상점, 일시정지 메뉴 등)
        [SerializeField] private GameObject inventoryPanel_UI; // InventoryUI 스크립트가 붙어있는 패널
        [SerializeField] private GameObject shopPanel_UI;      // ShopUI 스크립트가 붙어있는 패널
        [SerializeField] private GameObject pauseMenuPanel_UI; // 일시정지 메뉴 패널

        private Coroutine _notificationCoroutine;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // UI 매니저도 씬 전환 시 유지될 수 있음 (선택적)
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (notificationPanel != null) notificationPanel.SetActive(false);
            // 초기 UI 상태 설정
            if(inventoryPanel_UI != null) inventoryPanel_UI.SetActive(false);
            if(shopPanel_UI != null) shopPanel_UI.SetActive(false);
            if(pauseMenuPanel_UI != null) pauseMenuPanel_UI.SetActive(false);
        }

        private void Start()
        {
            // 게임 시작 시 초기 화폐 표시
            if (GameManager.Instance != null)
            {
                UpdateCurrencyDisplay(GameManager.Instance.CurrentPlayerCurrency);
            }
        }

        public void UpdateCurrencyDisplay(int amount)
        {
            if (currencyText != null)
            {
                currencyText.text = $"골드: {amount}";
            }
        }

        public void ShowNotification(string message)
        {
            if (notificationPanel == null || notificationText == null) return;

            notificationText.text = message;
            notificationPanel.SetActive(true);

            if (_notificationCoroutine != null)
            {
                StopCoroutine(_notificationCoroutine);
            }
            _notificationCoroutine = StartCoroutine(HideNotificationAfterDelay());
        }

        private System.Collections.IEnumerator HideNotificationAfterDelay()
        {
            yield return new WaitForSeconds(notificationDuration);
            if (notificationPanel != null) notificationPanel.SetActive(false);
        }

        // GameManager에서 호출될 메서드
        public void OnGameStateChanged(GameManager.GameState previousState, GameManager.GameState newState)
        {
            // 이전 상태에 따라 특정 UI 닫기
            // if (previousState == GameManager.GameState.InventoryMenu && inventoryPanel_UI != null) inventoryPanel_UI.SetActive(false);
            // if (previousState == GameManager.GameState.ShopMenu && shopPanel_UI != null) shopPanel_UI.SetActive(false);
            // if (previousState == GameManager.GameState.PauseMenu && pauseMenuPanel_UI != null) pauseMenuPanel_UI.SetActive(false);


            // 새 상태에 따라 특정 UI 열기
            if (newState == GameManager.GameState.InventoryMenu && inventoryPanel_UI != null)
            {
                inventoryPanel_UI.SetActive(true); // InventoryUI의 자체 로직으로 열리도록 유도할 수도 있음
            }
            else if (inventoryPanel_UI != null && newState != GameManager.GameState.InventoryMenu) // 인벤토리 상태 아니면 닫기
            {
                inventoryPanel_UI.SetActive(false);
            }


            if (newState == GameManager.GameState.ShopMenu && shopPanel_UI != null)
            {
                // ShopUI는 ShopSystem에 의해 별도로 관리되므로 여기서 직접 제어하지 않을 수 있음
                // shopPanel_UI.SetActive(true);
            }
            else if (shopPanel_UI != null && newState != GameManager.GameState.ShopMenu)
            {
                // shopPanel_UI.SetActive(false);
            }


            if (newState == GameManager.GameState.PauseMenu && pauseMenuPanel_UI != null)
            {
                pauseMenuPanel_UI.SetActive(true);
            }
            else if (pauseMenuPanel_UI != null && newState != GameManager.GameState.PauseMenu)
            {
                pauseMenuPanel_UI.SetActive(false);
            }

            // Debug.Log($"UIManager: GameState changed from {previousState} to {newState}. UI updated accordingly.");
        }
    }
}