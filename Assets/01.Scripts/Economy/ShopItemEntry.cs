using _01.Scripts.Farming;
using UnityEngine;

namespace _01.Scripts.Economy
{
    [System.Serializable]
    public class ShopItemEntry
    {
        public ItemData itemData; // 판매할 아이템 (ItemData ScriptableObject)

        [Tooltip("If true, uses itemData's buyPrice. If false, uses customPrice below.")]
        public bool useDefaultBuyPrice = true;
        public int customBuyPrice = 10; // 상점에서 플레이어에게 판매하는 가격

        [Tooltip("If true, uses itemData's sellPrice. If false, uses customSellPrice below.")]
        public bool useDefaultSellPrice = true;
        public int customSellPrice = 5; // 상점이 플레이어로부터 매입하는 가격

        [Tooltip("Infinite stock if less than 0. Limited stock otherwise.")]
        public int stock = -1; // -1이면 무제한 재고

        [HideInInspector] public int currentStock; // 실제 현재 재고 (런타임용)

        public int GetBuyPrice()
        {
            return useDefaultBuyPrice ? itemData.buyPrice : customBuyPrice;
        }

        public int GetSellPrice()
        {
            return useDefaultSellPrice ? itemData.sellPrice : customSellPrice;
        }

        public void InitializeStock()
        {
            currentStock = stock; // 초기 재고 설정
        }

        public bool HasInfiniteStock()
        {
            return stock < 0;
        }
    }
}