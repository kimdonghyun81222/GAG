using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.Economy
{
    [CreateAssetMenu(fileName = "NewShopData", menuName = "Grow A Garden/Shop Data", order = 52)]
    public class ShopData : ScriptableObject
    {
        public string shopName = "General Store";
        public List<ShopItemEntry> itemsToSell = new List<ShopItemEntry>();

        // 상점 데이터 초기화 시 각 아이템의 재고도 초기화
        public void InitializeShopStock()
        {
            foreach (var entry in itemsToSell)
            {
                entry.InitializeStock();
            }
        }
    }
}