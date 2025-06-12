using _01.Scripts.Economy;
using _01.Scripts.Farming;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.UI
{
    public class ShopSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image itemIconImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemPriceText;
        [SerializeField] private TextMeshProUGUI itemStockText; // 재고 표시 (선택적)
        [SerializeField] private Button buyButton;
        // [SerializeField] private TMP_InputField quantityToBuyInput; // 구매 수량 입력 (선택적)

        private ShopItemEntry _currentItemEntry;
        private ShopSystem _shopSystem;

        public void InitializeSlot(ShopItemEntry entry, ShopSystem system)
        {
            _currentItemEntry = entry;
            _shopSystem = system;

            if (_currentItemEntry == null || _currentItemEntry.itemData == null)
            {
                ClearSlotDisplay();
                return;
            }

            ItemData data = _currentItemEntry.itemData;
            itemIconImage.sprite = data.itemIcon;
            itemIconImage.enabled = (data.itemIcon != null);
            itemNameText.text = data.itemName;
            itemPriceText.text = $"{_currentItemEntry.GetBuyPrice()} G"; // "G"는 골드 표시

            UpdateStockDisplay();

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        public void UpdateStockDisplay()
        {
            if (_currentItemEntry == null) return;

            if (_currentItemEntry.HasInfiniteStock())
            {
                itemStockText.text = "재고: 무제한";
                buyButton.interactable = true;
            }
            else
            {
                itemStockText.text = $"재고: {_currentItemEntry.currentStock}";
                buyButton.interactable = _currentItemEntry.currentStock > 0;
            }
            itemStockText.enabled = true;
        }


        private void OnBuyButtonClicked()
        {
            if (_shopSystem == null || _currentItemEntry == null) return;

            // int quantity = 1; // 기본 1개 구매
            // if (quantityToBuyInput != null && !string.IsNullOrEmpty(quantityToBuyInput.text))
            // {
            //    if (int.TryParse(quantityToBuyInput.text, out int parsedQuantity) && parsedQuantity > 0)
            //    {
            //        quantity = parsedQuantity;
            //    }
            // }
            // 현재는 1개씩만 구매하는 것으로 단순화
            bool success = _shopSystem.PlayerBuysItem(_currentItemEntry, 1);
            if (success)
            {
                // 구매 성공 시 재고 UI 등 업데이트 (ShopSystem의 onShopInventoryChangedCallback이 처리)
                // UpdateStockDisplay(); // 직접 호출도 가능하나, 이벤트 기반이 더 좋음
            }
        }

        public void ClearSlotDisplay()
        {
            itemIconImage.enabled = false;
            itemNameText.text = "";
            itemPriceText.text = "";
            itemStockText.text = "";
            itemStockText.enabled = false;
            buyButton.interactable = false;
            _currentItemEntry = null;
        }
    }
}