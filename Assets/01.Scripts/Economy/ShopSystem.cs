using _01.Scripts.Core;
using _01.Scripts.Core.Inventory;
using _01.Scripts.Farming;
using UnityEngine;

namespace _01.Scripts.Economy
{
    public class ShopSystem : MonoBehaviour
    {
        public static ShopSystem Instance { get; private set; }

        private ShopData _currentShopData;
        public ShopData CurrentShopData => _currentShopData;

        private InventoryManager _inventoryManager;
        private GameManager _gameManager;
        private UIManager _uiManager; // 알림용

        // 상점 UI 업데이트를 위한 이벤트
        public delegate void OnShopInventoryChanged(ShopData shopData);
        public event OnShopInventoryChanged onShopInventoryChangedCallback;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject); // 상점 시스템은 씬에 따라 다를 수 있으므로 DontDestroyOnLoad는 선택적
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            _inventoryManager = InventoryManager.Instance;
            _gameManager = GameManager.Instance;
            _uiManager = UIManager.Instance;

            if (_inventoryManager == null) Debug.LogError("InventoryManager not found by ShopSystem.");
            if (_gameManager == null) Debug.LogError("GameManager not found by ShopSystem.");
            if (_uiManager == null) Debug.LogWarning("UIManager not found by ShopSystem, notifications might not work.");
        }

        // 특정 상점 데이터를 로드하고 UI를 열도록 요청
        public void OpenShop(ShopData shopDataToLoad, IInteractable shopkeeper = null)
        {
            if (shopDataToLoad == null)
            {
                Debug.LogError("Cannot open shop with null ShopData.");
                return;
            }

            _currentShopData = shopDataToLoad;
            _currentShopData.InitializeShopStock(); // 상점 열 때마다 재고 초기화 (또는 다른 정책 사용 가능)

            Debug.Log($"Opening shop: {_currentShopData.shopName}");

            // GameManager에 상태 변경 요청
            _gameManager?.ChangeState(GameManager.GameState.ShopMenu);

            // ShopUI에 상점 열기 알림 (ShopUI가 이 이벤트를 구독하거나 직접 호출)
            onShopInventoryChangedCallback?.Invoke(_currentShopData);

            // TODO: ShopUI.Instance.OpenShopPanel(_currentShopData); 와 같이 직접 호출할 수도 있음
        }

        public void CloseShop()
        {
            if (_currentShopData == null) return; // 이미 닫혀있음

            Debug.Log($"Closing shop: {_currentShopData.shopName}");
            _currentShopData = null;

            // GameManager에 상태 변경 요청 (이전 상태로 돌아가도록)
            if (_gameManager != null && _gameManager.CurrentState == GameManager.GameState.ShopMenu)
            {
                _gameManager.ChangeState(_gameManager.PreviousState);
            }

            // ShopUI에 상점 닫기 알림
            onShopInventoryChangedCallback?.Invoke(null); // null을 전달하여 UI 닫도록 유도
            // TODO: ShopUI.Instance.CloseShopPanel();
        }

        // 플레이어가 상점에서 아이템 구매
        public bool PlayerBuysItem(ShopItemEntry itemEntry, int quantity = 1)
        {
            if (_currentShopData == null || itemEntry == null || _inventoryManager == null || _gameManager == null)
            {
                Debug.LogError("ShopSystem not ready for transaction (Buy).");
                return false;
            }
            if (quantity <= 0) return false;

            ItemData itemToBuy = itemEntry.itemData;
            int buyPricePerItem = itemEntry.GetBuyPrice();
            int totalCost = buyPricePerItem * quantity;

            // 1. 재고 확인
            if (!itemEntry.HasInfiniteStock() && itemEntry.currentStock < quantity)
            {
                _uiManager?.ShowNotification("재고가 부족합니다.");
                Debug.LogWarning($"Not enough stock for {itemToBuy.itemName}. Requested: {quantity}, Available: {itemEntry.currentStock}");
                return false;
            }

            // 2. 플레이어 화폐 확인
            if (!_gameManager.SpendCurrency(totalCost))
            {
                // SpendCurrency 내부에서 "골드 부족" 알림 처리됨
                Debug.LogWarning($"Player cannot afford {itemToBuy.itemName}. Cost: {totalCost}, Player Gold: {_gameManager.CurrentPlayerCurrency}");
                return false;
            }

            // 3. 플레이어 인벤토리에 아이템 추가 시도
            if (!_inventoryManager.AddItem(itemToBuy, quantity))
            {
                // 인벤토리가 가득 찬 경우, 돈은 돌려주고 거래 취소
                _gameManager.AddCurrency(totalCost); // 환불
                _uiManager?.ShowNotification("인벤토리가 가득 찼습니다. 구매가 취소되었습니다.");
                Debug.LogWarning($"Inventory full. Purchase of {itemToBuy.itemName} cancelled, currency refunded.");
                return false;
            }

            // 4. 상점 재고 감소 (무제한 재고가 아닐 경우)
            if (!itemEntry.HasInfiniteStock())
            {
                itemEntry.currentStock -= quantity;
            }

            _uiManager?.ShowNotification($"{itemToBuy.itemName} {quantity}개를 구매했습니다. (-{totalCost} 골드)");
            Debug.Log($"Player bought {quantity} of {itemToBuy.itemName} for {totalCost}. Stock left: {(itemEntry.HasInfiniteStock() ? "Infinite" : itemEntry.currentStock.ToString())}");

            onShopInventoryChangedCallback?.Invoke(_currentShopData); // UI 업데이트
            return true;
        }

        // 플레이어가 상점에 아이템 판매
        public bool PlayerSellsItem(InventorySlot playerItemSlot, int quantity = 1)
        {
            if (_currentShopData == null || playerItemSlot == null || playerItemSlot.IsEmpty() || _inventoryManager == null || _gameManager == null)
            {
                Debug.LogError("ShopSystem not ready for transaction (Sell).");
                return false;
            }
            if (quantity <= 0) return false;
            if (playerItemSlot.quantity < quantity)
            {
                Debug.LogWarning("Player does not have enough quantity in the slot to sell.");
                return false; // 슬롯에 판매할 만큼 수량 없음
            }


            ItemData itemToSell = playerItemSlot.itemData;

            // 해당 아이템을 상점에서 매입하는지 확인 (ShopData에 해당 아이템이 있는지, 혹은 기본 매입 정책이 있는지)
            // 여기서는 간단히 ShopData에 등록된 아이템만 매입 가능하다고 가정.
            // 또는, 모든 아이템에 대해 ItemData의 기본 sellPrice를 사용할 수도 있음.
            ShopItemEntry shopEntryForItem = null;
            foreach(var entry in _currentShopData.itemsToSell)
            {
                if(entry.itemData.itemID == itemToSell.itemID)
                {
                    shopEntryForItem = entry;
                    break;
                }
            }

            int sellPricePerItem;
            if (shopEntryForItem != null) // 상점이 원래 파는 아이템이라면 해당 상점의 판매가(GetSellPrice) 기준
            {
                sellPricePerItem = shopEntryForItem.GetSellPrice();
            }
            else // 상점이 원래 팔지 않는 아이템이라면 ItemData의 기본 판매가 기준 (선택적: 아예 매입 안할 수도 있음)
            {
                // 정책: 상점에서 취급 안하는 아이템은 매입 안함
                _uiManager?.ShowNotification("이 상점에서는 해당 아이템을 매입하지 않습니다.");
                Debug.LogWarning($"Shop {_currentShopData.shopName} does not buy {itemToSell.itemName}.");
                return false;
                // 정책: ItemData의 기본 판매가로 매입
                // sellPricePerItem = itemToSell.sellPrice;
            }


            int totalGain = sellPricePerItem * quantity;

            // 1. 플레이어 인벤토리에서 아이템 제거
            if (!_inventoryManager.RemoveItem(itemToSell, quantity))
            {
                // 이 경우는 거의 없어야 함 (위에서 수량 체크 했으므로)
                Debug.LogError($"Failed to remove {itemToSell.itemName} from player inventory during sell operation.");
                return false;
            }

            // 2. 플레이어에게 화폐 지급
            _gameManager.AddCurrency(totalGain);

            // 3. (선택적) 상점 재고 증가 (플레이어가 판 아이템을 상점이 다시 되팔 경우)
            // 여기서는 플레이어가 판 아이템이 상점 재고에 추가되지 않는다고 가정.
            // 만약 추가한다면:
            // if (shopEntryForItem != null && !shopEntryForItem.HasInfiniteStock())
            // {
            //    shopEntryForItem.currentStock += quantity;
            // }

            _uiManager?.ShowNotification($"{itemToSell.itemName} {quantity}개를 판매했습니다. (+{totalGain} 골드)");
            Debug.Log($"Player sold {quantity} of {itemToSell.itemName} for {totalGain}.");

            onShopInventoryChangedCallback?.Invoke(_currentShopData); // UI 업데이트
            return true;
        }
    }
}