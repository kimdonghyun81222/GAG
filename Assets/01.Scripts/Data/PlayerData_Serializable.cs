using System.Collections.Generic;

namespace _01.Scripts.Data
{
    [System.Serializable]
    public class PlayerData_Serializable
    {
        public int currentCurrency;
        public List<InventorySlot_Serializable> inventorySlots;
        // public Vector3 playerPosition; // 플레이어 위치 저장 시
        // public Quaternion playerRotation; // 플레이어 회전 저장 시

        public PlayerData_Serializable()
        {
            inventorySlots = new List<InventorySlot_Serializable>();
        }
    }

    [System.Serializable]
    public class InventorySlot_Serializable
    {
        public string itemID; // ItemData의 이름 또는 고유 ID
        public int quantity;
        public bool isEmpty;

        public InventorySlot_Serializable(string id, int qty, bool empty)
        {
            itemID = id;
            quantity = qty;
            isEmpty = empty;
        }
    }
}