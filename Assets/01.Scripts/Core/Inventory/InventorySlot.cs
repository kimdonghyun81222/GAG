// using UnityEngine; // ItemData 참조를 위해 필요할 수 있으나, ItemData는 UnityEngine 기본 타입을 많이 안씀

using _01.Scripts.Farming;
using UnityEngine;

namespace _01.Scripts.Core.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData itemData; // 현재 슬롯의 아이템 데이터 (ScriptableObject 참조)
        public int quantity;      // 현재 슬롯의 아이템 수량

        // 기본 생성자 (빈 슬롯으로 초기화)
        public InventorySlot()
        {
            ClearSlot();
        }

        // (선택적) 아이템과 수량으로 초기화하는 생성자
        public InventorySlot(ItemData item, int qty)
        {
            itemData = item;
            quantity = item != null ? Mathf.Clamp(qty, 0, item.maxStackSize) : 0;
            if (item == null || quantity == 0) ClearSlot();
        }

        public bool IsEmpty()
        {
            return itemData == null || quantity <= 0;
        }

        public void AddItem(ItemData newItem, int addQuantity)
        {
            if (newItem == null || addQuantity <= 0) return;

            if (IsEmpty() || itemData.itemID == newItem.itemID && newItem.isStackable)
            {
                if (IsEmpty()) // 슬롯이 비어있었다면 새 아이템으로 설정
                {
                    itemData = newItem;
                }
                int newQuantity = quantity + addQuantity;
                quantity = Mathf.Clamp(newQuantity, 0, itemData.maxStackSize); // 최대 스택 초과 방지
            }
            // else 다른 아이템이 있거나, 스택 불가능한 아이템이면 추가 안함 (InventoryManager에서 처리)
        }

        public void RemoveQuantity(int amountToRemove)
        {
            if (amountToRemove <= 0) return;
            quantity -= amountToRemove;
            if (quantity <= 0)
            {
                ClearSlot();
            }
        }

        public void ClearSlot()
        {
            itemData = null;
            quantity = 0;
        }
    }
}