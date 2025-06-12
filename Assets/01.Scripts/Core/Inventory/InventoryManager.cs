using System.Collections.Generic;
using _01.Scripts.Data;
using _01.Scripts.Farming;
using UnityEngine;

namespace _01.Scripts.Core.Inventory
{
    public class InventoryManager : MonoBehaviour, ISaveable
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Inventory Settings")]
        [SerializeField] private int inventorySize = 20;
        public List<InventorySlot> inventorySlots;

        // ItemDatabase 참조는 이제 ItemDatabase.Instance를 통해 직접 접근
        // [SerializeField] private ItemDatabase itemDatabase; // 이 필드는 더 이상 필요 없을 수 있음

        public delegate void OnInventoryChanged();
        public event OnInventoryChanged onInventoryChangedCallback;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                inventorySlots = new List<InventorySlot>(inventorySize);
                for (int i = 0; i < inventorySize; i++)
                {
                    inventorySlots.Add(new InventorySlot());
                }
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // ItemDatabase의 초기화는 GameManager 또는 로딩 시퀀스에서 ItemDatabase.Instance.InitializeAllDatabases()를 통해 수행되어야 함.
            // InventoryManager는 ItemDatabase.Instance가 준비되었다고 가정하고 사용.
            if (ItemDatabase.Instance == null)
            {
                Debug.LogError("InventoryManager: ItemDatabase.Instance is null! Make sure ItemDatabase is loaded and initialized before InventoryManager starts.", this);
            }
            // else if (!ItemDatabase.Instance.IsFullyInitialized()) // 수정됨: IsFullyInitialized 사용
            // {
            //     Debug.LogWarning("InventoryManager: ItemDatabase is not fully initialized yet. Item lookups might fail until it is.", this);
            //     // ItemDatabase.Instance.InitializeAllDatabases(); // 여기서 강제 초기화는 동시성 문제 유발 가능
            // }
        }

        private void OnEnable()
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.RegisterSaveable(this);
            }
            else
            {
                Invoke(nameof(RegisterSelfToSaveLoadManager), 0.01f);
            }
        }
        private void RegisterSelfToSaveLoadManager()
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.RegisterSaveable(this);
            }
            else
            {
                Debug.LogWarning("InventoryManager: SaveLoadManager not found to register.", this);
            }
        }

        private void OnDisable()
        {
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.UnregisterSaveable(this);
            }
        }

        public bool AddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;
            // ... (이전 AddItem 로직과 동일하게 유지, ItemDatabase.Instance.GetItemByID 사용 등은 이미 반영됨) ...
            // 내부적으로 InventorySlot의 AddItem을 호출하므로, ItemData의 isStackable, maxStackSize를 직접 참조하지 않음.
            int originalQuantity = quantity;
            int quantitySuccessfullyAdded = 0;

            if (item.isStackable)
            {
                foreach (InventorySlot slot in inventorySlots)
                {
                    if (!slot.IsEmpty() && slot.itemData.itemID == item.itemID && slot.quantity < item.maxStackSize)
                    {
                        int spaceLeftInStack = item.maxStackSize - slot.quantity;
                        int amountToStack = Mathf.Min(quantity, spaceLeftInStack);
                        slot.AddItem(item, amountToStack);
                        quantity -= amountToStack;
                        quantitySuccessfullyAdded += amountToStack;
                        if (quantity <= 0) break;
                    }
                }
            }
            if (quantity > 0)
            {
                foreach (InventorySlot slot in inventorySlots)
                {
                    if (slot.IsEmpty())
                    {
                        if (item.isStackable)
                        {
                            int amountToAdd = Mathf.Min(quantity, item.maxStackSize);
                            slot.AddItem(item, amountToAdd);
                            quantity -= amountToAdd;
                            quantitySuccessfullyAdded += amountToAdd;
                        }
                        else
                        {
                            slot.AddItem(item, 1);
                            quantity -= 1;
                            quantitySuccessfullyAdded += 1;
                        }
                        if (quantity <= 0) break;
                    }
                }
            }
            if (quantitySuccessfullyAdded < originalQuantity && quantitySuccessfullyAdded > 0)
                Debug.LogWarning($"Inventory is full or item not stackable. Could not add all {originalQuantity} of {item.itemName}. Added: {quantitySuccessfullyAdded}");
            else if (quantitySuccessfullyAdded == 0 && originalQuantity > 0)
                Debug.LogWarning($"Inventory is full. Could not add any {item.itemName}.");

            if (quantitySuccessfullyAdded > 0) onInventoryChangedCallback?.Invoke();
            return quantity == 0 && quantitySuccessfullyAdded == originalQuantity; // 모든 요청 수량이 성공적으로 추가되었을 때만 true
        }

        public bool RemoveItem(ItemData item, int quantity = 1)
        {
            // ... (이전 RemoveItem 로직과 동일하게 유지) ...
            if (item == null || quantity <= 0) return false;
            int quantityToRemove = quantity;
            for (int i = inventorySlots.Count - 1; i >= 0; i--)
            {
                InventorySlot slot = inventorySlots[i];
                if (!slot.IsEmpty() && slot.itemData.itemID == item.itemID)
                {
                    if (slot.quantity >= quantityToRemove)
                    {
                        slot.RemoveQuantity(quantityToRemove);
                        quantityToRemove = 0;
                        break;
                    }
                    else
                    {
                        quantityToRemove -= slot.quantity;
                        slot.ClearSlot();
                    }
                }
            }
            if (quantityToRemove > 0)
            {
                Debug.LogWarning($"Could not remove all {item.itemName}. Requested: {quantity}, Actual Removed: {quantity - quantityToRemove}");
                onInventoryChangedCallback?.Invoke(); // 일부라도 변경되었을 수 있으므로 콜백
                return false;
            }
            onInventoryChangedCallback?.Invoke();
            return true;
        }

        public bool HasItem(ItemData item, int quantity = 1)
        {
            // ... (이전 HasItem 로직과 동일하게 유지) ...
            if (item == null || quantity <= 0) return false;
            int count = 0;
            foreach (InventorySlot slot in inventorySlots)
            {
                if (!slot.IsEmpty() && slot.itemData.itemID == item.itemID)
                {
                    count += slot.quantity;
                }
            }
            return count >= quantity;
        }

        public ItemData GetEquippedItem()
        {
            // ... (이전 GetEquippedItem 로직과 동일하게 유지) ...
            if (inventorySlots.Count > 0 && !inventorySlots[0].IsEmpty())
            {
                return inventorySlots[0].itemData;
            }
            return null;
        }

        public void PopulateSaveData(GameSaveData saveData)
        {
            if (saveData.playerData == null) saveData.playerData = new PlayerData_Serializable();
            saveData.playerData.inventorySlots = new List<InventorySlot_Serializable>();
            foreach (InventorySlot slot in this.inventorySlots)
            {
                if (slot.IsEmpty()) saveData.playerData.inventorySlots.Add(new InventorySlot_Serializable(null, 0, true));
                else saveData.playerData.inventorySlots.Add(new InventorySlot_Serializable(slot.itemData.itemID, slot.quantity, false));
            }
        }

        public void LoadFromSaveData(GameSaveData saveData)
        {
            if (ItemDatabase.Instance == null) // 수정됨: ItemDatabase.Instance 직접 사용
            {
                Debug.LogError("InventoryManager: ItemDatabase.Instance is null. Cannot load inventory items by ID.", this);
                for (int i = 0; i < inventorySlots.Count; i++) inventorySlots[i].ClearSlot();
                onInventoryChangedCallback?.Invoke();
                return;
            }
            // ItemDatabase.Instance.IsFullyInitialized()로 초기화 상태 확인
            if (!ItemDatabase.Instance.IsFullyInitialized()) // 수정됨: IsFullyInitialized 사용 및 Instance 직접 사용
            {
                Debug.LogWarning("InventoryManager: ItemDatabase is not fully initialized. Attempting to load items, but some might fail.", this);
                // 여기서 ItemDatabase.Instance.InitializeAllDatabases()를 호출하는 것은 동시성 문제를 일으킬 수 있으므로,
                // 게임 시작 로직에서 미리 초기화되도록 보장하는 것이 중요합니다.
            }

            if (saveData.playerData != null && saveData.playerData.inventorySlots != null)
            {
                int countToLoad = Mathf.Min(this.inventorySlots.Count, saveData.playerData.inventorySlots.Count);
                for (int i = 0; i < countToLoad; i++)
                {
                    InventorySlot_Serializable savedSlot = saveData.playerData.inventorySlots[i];
                    if (savedSlot.isEmpty || string.IsNullOrEmpty(savedSlot.itemID))
                    {
                        this.inventorySlots[i].ClearSlot();
                    }
                    else
                    {
                        ItemData item = ItemDatabase.Instance.GetItemByID(savedSlot.itemID); // 수정됨: Instance 직접 사용
                        if (item != null) this.inventorySlots[i].AddItem(item, savedSlot.quantity);
                        else
                        {
                            Debug.LogWarning($"InventoryManager: Could not find item with ID '{savedSlot.itemID}'. Slot {i} will be empty.", this);
                            this.inventorySlots[i].ClearSlot();
                        }
                    }
                }
                for (int i = countToLoad; i < this.inventorySlots.Count; i++) this.inventorySlots[i].ClearSlot();
                onInventoryChangedCallback?.Invoke();
            }
            else
            {
                Debug.LogWarning("InventoryManager: No inventory data found in save file to load.", this);
                for (int i = 0; i < inventorySlots.Count; i++) inventorySlots[i].ClearSlot();
                onInventoryChangedCallback?.Invoke();
            }
        }
    }
}