using System.Collections.Generic;
using _01.Scripts.Core.Inventory;
using _01.Scripts.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts.UI
{
    public class ShopUI : MonoBehaviour
    {
        [Header("Shop UI Panels")]
        [SerializeField] private GameObject shopPanelObject;
        [SerializeField] private TextMeshProUGUI shopNameText;

        [Header("Shop Item Display")]
        [SerializeField] private Transform shopSlotsParent;
        [SerializeField] private GameObject shopSlotPrefab; // This is a GameObject prefab

        [Header("Player Inventory Display (for Selling)")]
        [SerializeField] private Transform playerInventorySlotsParent_Shop;
        [SerializeField] private GameObject playerInventorySlot_ShopPrefab; // This is a GameObject prefab

        [SerializeField] private Button closeShopButton;

        private ShopSystem _shopSystem;
        private InventoryManager _inventoryManager;

        private List<ShopSlotUI> _activeShopSlots = new List<ShopSlotUI>();
        private List<InventorySlotUI> _activePlayerShopInventorySlots = new List<InventorySlotUI>();

        private void Start()
        {
            _shopSystem = ShopSystem.Instance;
            _inventoryManager = InventoryManager.Instance;

            if (_shopSystem == null) Debug.LogError("ShopSystem not found by ShopUI.");
            if (_inventoryManager == null) Debug.LogError("InventoryManager not found by ShopUI.");

            if (_shopSystem != null)
            {
                _shopSystem.onShopInventoryChangedCallback += RefreshShopUI;
            }
            if(_inventoryManager != null)
            {
                _inventoryManager.onInventoryChangedCallback += RefreshPlayerInventoryInShopUI;
            }

            if (closeShopButton != null)
            {
                closeShopButton.onClick.AddListener(CloseShopPanel);
            }
            shopPanelObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_shopSystem != null)
            {
                _shopSystem.onShopInventoryChangedCallback -= RefreshShopUI;
            }
            if (_inventoryManager != null)
            {
                _inventoryManager.onInventoryChangedCallback -= RefreshPlayerInventoryInShopUI;
            }
        }

        public void OpenShopPanel(ShopData data)
        {
            if (data == null)
            {
                CloseShopPanel();
                return;
            }
            shopNameText.text = data.shopName;
            shopPanelObject.SetActive(true);
            PopulateShopSlots(data);
            PopulatePlayerInventorySlots();
        }

        public void CloseShopPanel()
        {
            shopPanelObject.SetActive(false);
            if(ShopSystem.Instance != null && ShopSystem.Instance.CurrentShopData != null)
            {
                ShopSystem.Instance.CloseShop();
            }
        }

        private void RefreshShopUI(ShopData currentShopData)
        {
            if (currentShopData != null && shopPanelObject.activeSelf)
            {
                OpenShopPanel(currentShopData);
            }
            else if (currentShopData == null && shopPanelObject.activeSelf)
            {
                CloseShopPanel();
            }
            else if (currentShopData != null && !shopPanelObject.activeSelf)
            {
                OpenShopPanel(currentShopData);
            }
        }

        private void PopulateShopSlots(ShopData data)
        {
            foreach (ShopSlotUI slot in _activeShopSlots) if(slot != null) Destroy(slot.gameObject);
            _activeShopSlots.Clear();

            if (data == null || data.itemsToSell == null) return;

            foreach (ShopItemEntry entry in data.itemsToSell)
            {
                // Explicitly cast the result of Instantiate to GameObject
                GameObject slotObj = Instantiate(shopSlotPrefab, shopSlotsParent) as GameObject;
                if (slotObj == null)
                {
                    Debug.LogError($"Failed to instantiate shopSlotPrefab or it's not a GameObject. Prefab: {shopSlotPrefab.name}", this);
                    continue;
                }
                ShopSlotUI slotUI = slotObj.GetComponent<ShopSlotUI>();
                if (slotUI != null)
                {
                    slotUI.InitializeSlot(entry, _shopSystem);
                    _activeShopSlots.Add(slotUI);
                }
            }
        }

        private void PopulatePlayerInventorySlots()
        {
            foreach (InventorySlotUI slot in _activePlayerShopInventorySlots) if(slot != null) Destroy(slot.gameObject);
            _activePlayerShopInventorySlots.Clear();

            if (_inventoryManager == null || playerInventorySlotsParent_Shop == null || playerInventorySlot_ShopPrefab == null) return;

            for (int i = 0; i < _inventoryManager.inventorySlots.Count; i++)
            {
                // Explicitly cast the result of Instantiate to GameObject
                GameObject slotObj = Instantiate(playerInventorySlot_ShopPrefab, playerInventorySlotsParent_Shop) as GameObject;
                if (slotObj == null)
                {
                    Debug.LogError($"Failed to instantiate playerInventorySlot_ShopPrefab or it's not a GameObject. Prefab: {playerInventorySlot_ShopPrefab.name}", this);
                    continue;
                }
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.InitializeSlot(i); // Assuming InventorySlotUI has an InitializeSlot(int index)
                    slotUI.UpdateSlot(_inventoryManager.inventorySlots[i]);

                    Button sellButton = slotObj.GetComponent<Button>(); // Try to get existing button
                    if (sellButton == null) sellButton = slotObj.AddComponent<Button>(); // Add if not exists

                    int slotIndex = i;
                    sellButton.onClick.RemoveAllListeners(); // Clear previous listeners
                    sellButton.onClick.AddListener(() => OnPlayerSellItemClicked(slotIndex));

                    _activePlayerShopInventorySlots.Add(slotUI);
                }
            }
        }

        private void RefreshPlayerInventoryInShopUI()
        {
            if (shopPanelObject.activeSelf && _inventoryManager != null)
            {
                for(int i=0; i < _activePlayerShopInventorySlots.Count && i < _inventoryManager.inventorySlots.Count; i++)
                {
                    if (_activePlayerShopInventorySlots[i] != null) // Null check for the slot UI itself
                        _activePlayerShopInventorySlots[i].UpdateSlot(_inventoryManager.inventorySlots[i]);
                }
            }
        }

        private void OnPlayerSellItemClicked(int inventorySlotIndex)
        {
            if (_shopSystem == null || _inventoryManager == null) return;
            if (inventorySlotIndex < 0 || inventorySlotIndex >= _inventoryManager.inventorySlots.Count) return;

            InventorySlot slotToSell = _inventoryManager.inventorySlots[inventorySlotIndex];
            if (slotToSell != null && !slotToSell.IsEmpty())
            {
                _shopSystem.PlayerSellsItem(slotToSell, 1);
            }
        }
    }
}