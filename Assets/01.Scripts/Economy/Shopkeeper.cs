using _01.Scripts.Core;
using _01.Scripts.Player;
using UnityEngine;

namespace _01.Scripts.Economy
{
    public class Shopkeeper : MonoBehaviour, IInteractable
    {
        [SerializeField] private ShopData assignedShopData; // 이 상점 주인이 열어줄 상점 데이터
        [SerializeField] private string shopkeeperName = "상점 주인";

        public void Interact(PlayerController player)
        {
            if (assignedShopData == null)
            {
                Debug.LogError($"Shopkeeper {shopkeeperName} has no ShopData assigned!");
                UIManager.Instance?.ShowNotification("상점 정보를 불러올 수 없습니다.");
                return;
            }

            if (ShopSystem.Instance != null)
            {
                ShopSystem.Instance.OpenShop(assignedShopData, this);
            }
            else
            {
                Debug.LogError("ShopSystem instance not found!");
            }
        }

        public string GetInteractionPrompt()
        {
            if (assignedShopData != null)
            {
                return $"{shopkeeperName}에게 말 걸기 (E)"; //  ({assignedShopData.shopName})
            }
            return "상호작용 (E)";
        }

        private void OnValidate()
        {
            if(string.IsNullOrEmpty(shopkeeperName) && gameObject.name != "GameObject")
            {
                shopkeeperName = gameObject.name;
            }
        }
    }
}