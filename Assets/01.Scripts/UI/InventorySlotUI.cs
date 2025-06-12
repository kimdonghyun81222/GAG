using _01.Scripts.Core.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.UI
{
    public class InventorySlotUI : MonoBehaviour //, IPointerClickHandler // 슬롯 클릭 이벤트 직접 처리 시
    {
        [Header("Slot UI Elements")]
        public Image itemIconImage;         // 아이템 아이콘 (UnityEngine.UI.Image)
        public TextMeshProUGUI quantityText; // 아이템 수량 (TMPro.TextMeshProUGUI)
        public GameObject highlightBorder;  // 선택/호버 시 하이라이트 (선택적)
        public Button slotButton;           // 슬롯 전체를 버튼으로 만들어 클릭 감지 (선택적)

        private InventorySlot _representedSlotData; // 이 UI 슬롯이 나타내는 실제 인벤토리 슬롯 데이터
        private int _slotIndexInInventory = -1;      // 인벤토리 매니저 내에서의 이 슬롯의 인덱스

        // private InventoryUI _inventoryUIManager; // 부모 UI 매니저 참조 (필요시)

        // 슬롯 초기화 (InventoryUI에서 호출)
        public void InitializeSlot(int slotIndex/*, InventoryUI invUI*/)
        {
            _slotIndexInInventory = slotIndex;
            // _inventoryUIManager = invUI;

            if (itemIconImage == null) Debug.LogWarning($"ItemIconImage not assigned on {gameObject.name}", this);
            if (quantityText == null) Debug.LogWarning($"QuantityText not assigned on {gameObject.name}", this);
            if (highlightBorder != null) highlightBorder.SetActive(false); // 초기에는 하이라이트 숨김

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(OnSlotClicked);
            }
            ClearSlotDisplay(); // 초기에는 빈 슬롯으로 표시
        }

        // 슬롯 데이터 업데이트 (InventoryUI에서 호출)
        public void UpdateSlot(InventorySlot slotData)
        {
            _representedSlotData = slotData;

            if (itemIconImage == null || quantityText == null)
            {
                // Debug.LogWarning("Slot UI elements not fully assigned for UpdateSlot!", this);
                return;
            }

            if (slotData != null && !slotData.IsEmpty() && slotData.itemData != null)
            {
                itemIconImage.sprite = slotData.itemData.itemIcon;
                itemIconImage.enabled = true;

                // 수량이 1개 초과일 때만 표시 (또는 항상 표시)
                bool showQuantity = slotData.quantity > 1;
                quantityText.text = showQuantity ? slotData.quantity.ToString() : "";
                quantityText.enabled = showQuantity;

                if (slotButton != null) slotButton.interactable = true;
            }
            else
            {
                ClearSlotDisplay();
            }
        }

        // 슬롯 표시 비우기
        public void ClearSlotDisplay()
        {
            _representedSlotData = null; // 데이터 참조도 초기화

            if (itemIconImage != null)
            {
                itemIconImage.sprite = null;
                itemIconImage.enabled = false;
            }
            if (quantityText != null)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
            if (slotButton != null) slotButton.interactable = false; // 빈 슬롯은 클릭 안되게 (선택적)
            if (highlightBorder != null) highlightBorder.SetActive(false);
        }

        // 슬롯 클릭 시 호출될 메서드 (Button 사용 시)
        public void OnSlotClicked()
        {
            if (_representedSlotData == null || _representedSlotData.IsEmpty())
            {
                Debug.Log($"Clicked on empty slot (Index: {_slotIndexInInventory}).");
                // 빈 슬롯 클릭 시 로직 (선택적)
                return;
            }

            Debug.Log($"Clicked on slot (Index: {_slotIndexInInventory}) containing: {_representedSlotData.itemData.itemName}");

            // TODO: 아이템 사용, 장착, 상세 정보 보기 등의 로직 연결
            // 예시: 인벤토리 매니저에게 이 슬롯이 선택되었음을 알림
            // InventoryManager.Instance?.SetSelectedSlot(_slotIndexInInventory);

            // 예시: 아이템 사용 (더블클릭이나 우클릭 컨텍스트 메뉴가 더 일반적)
            // if(_representedSlotData.itemData.itemType == ItemType.Consumable)
            // {
            //    InventoryManager.Instance.UseItem(_representedSlotData);
            // }
        }

        // IPointerClickHandler 인터페이스 사용 시 (Button 없이 Image 등에 직접 스크립트 붙일 때)
        // public void OnPointerClick(PointerEventData eventData)
        // {
        //    if (eventData.button == PointerEventData.InputButton.Left)
        //    {
        //        OnSlotClicked();
        //    }
        //    else if (eventData.button == PointerEventData.InputButton.Right)
        //    {
        //        // 우클릭 메뉴 표시 등
        //        Debug.Log($"Right clicked on slot {_slotIndexInInventory}");
        //    }
        // }

        public void SetHighlight(bool active)
        {
            if (highlightBorder != null)
            {
                highlightBorder.SetActive(active);
            }
        }
    }
}