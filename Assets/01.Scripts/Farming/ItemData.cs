using UnityEngine;

namespace _01.Scripts.Farming
{
    public enum ItemType
    {
        Generic,
        Seed,
        Crop, // 수확물
        Tool,
        Consumable,
        Equipment,
        QuestItem,
        MutationModifier // 돌연변이 유발/제거 아이템 등
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "Grow A Garden/Item Data", order = 1)]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemID; // 저장/로드 및 데이터베이스 조회용 고유 ID
        public string itemName = "New Item";
        [TextArea]
        public string description = "Item Description";
        public Sprite itemIcon;
        public ItemType itemType = ItemType.Generic;

        [Header("Stacking")]
        public bool isStackable = true; // 이 아이템을 여러 개 겹칠 수 있는지 여부
        public int maxStackSize = 99;   // 겹칠 수 있는 최대 개수 (isStackable이 true일 때만 의미 있음)

        [Header("Gameplay Values")]
        public int buyPrice = 10;  // 상점에서 구매 시 가격
        public int sellPrice = 5;  // 상점에 판매 시 가격

        [Header("Specific Type Data")]
        [Tooltip("If ItemType is Seed, assign the CropData to grow here.")]
        public CropData cropToGrow; // ItemType.Seed일 경우, 심을 작물 데이터

        // 여기에 아이템 타입에 따른 추가 데이터 필드들을 넣을 수 있습니다.
        // 예: public int damage; // ItemType.Tool 또는 Equipment일 경우
        // 예: public int healAmount; // ItemType.Consumable일 경우

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(itemID))
            {
                if (!string.IsNullOrEmpty(itemName))
                {
                    itemID = itemName.ToLower().Replace(" ", "_") + "_item";
                }
                else if (name != "NewItemData")
                {
                    itemID = name.ToLower().Replace(" ", "_") + "_item_asset";
                }
                else
                {
                    itemID = System.Guid.NewGuid().ToString().Substring(0,8); // 임시 ID
                }
            }
            if (!isStackable)
            {
                maxStackSize = 1; // 스택 불가능하면 최대 스택 크기는 1
            }
        }
    }
}