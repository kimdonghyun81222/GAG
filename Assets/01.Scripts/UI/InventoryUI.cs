using System.Collections.Generic;
using _01.Scripts.Core;
using _01.Scripts.Core.Inventory;
using UnityEngine;

namespace _01.Scripts.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform slotsParent;
        [SerializeField] private GameObject slotPrefab; // This is a GameObject prefab

        private InventoryManager _inventoryManager;
        private List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>();

        private InputManager _inputManager;
        private bool _isInventoryOpen = false;

        void Start()
        {
            _inventoryManager = InventoryManager.Instance;
            if (_inventoryManager == null)
            {
                Debug.LogError("InventoryManager instance not found!");
                enabled = false;
                return;
            }

            _inputManager = InputManager.Instance;
            if (_inputManager == null)
            {
                Debug.LogWarning("InputManager instance not found by InventoryUI. Hotkey toggle might not work.");
            }

            _inventoryManager.onInventoryChangedCallback += UpdateUI;

            InitializeInventoryUI();
            ToggleInventoryPanel(false);
        }

        void Update()
        {
            if (_inputManager != null && _inputManager.OpenInventoryTriggered)
            {
                _isInventoryOpen = !_isInventoryOpen;
                ToggleInventoryPanel(_isInventoryOpen);

                if (GameManager.Instance != null)
                {
                    if (_isInventoryOpen)
                    {
                        if(GameManager.Instance.CurrentState != GameManager.GameState.InventoryMenu)
                            GameManager.Instance.ChangeState(GameManager.GameState.InventoryMenu);
                    }
                    else
                    {
                        if(GameManager.Instance.CurrentState == GameManager.GameState.InventoryMenu)
                            GameManager.Instance.ChangeState(GameManager.Instance.PreviousState);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_inventoryManager != null)
            {
                _inventoryManager.onInventoryChangedCallback -= UpdateUI;
            }
        }

        void InitializeInventoryUI()
        {
            if (slotsParent == null || slotPrefab == null)
            {
                Debug.LogError("Slots Parent or Slot Prefab not assigned in InventoryUI!");
                return;
            }

            foreach (Transform child in slotsParent)
            {
                Destroy(child.gameObject);
            }
            _slotUIs.Clear();

            for (int i = 0; i < _inventoryManager.inventorySlots.Capacity; i++)
            {
                // Explicitly cast the result of Instantiate to GameObject
                GameObject slotObj = Instantiate(slotPrefab, slotsParent) as GameObject;
                if (slotObj == null)
                {
                    Debug.LogError($"Failed to instantiate slotPrefab or it's not a GameObject. Prefab: {slotPrefab.name}", this);
                    continue;
                }

                InventorySlotUI slotUIComponent = slotObj.GetComponent<InventorySlotUI>();
                if (slotUIComponent != null)
                {
                    slotUIComponent.InitializeSlot(i); // InitializeSlot에 인덱스 전달
                    _slotUIs.Add(slotUIComponent);
                }
                else
                {
                    Debug.LogError($"Slot Prefab is missing InventorySlotUI component! Prefab: {slotPrefab.name}", slotPrefab);
                }
            }
            UpdateUI();
        }

        void UpdateUI()
        {
            if (_inventoryManager == null || _slotUIs.Count != _inventoryManager.inventorySlots.Count)
            {
                if (_inventoryManager != null && _inventoryManager.inventorySlots.Count != _slotUIs.Count && _slotUIs.Capacity >= _inventoryManager.inventorySlots.Count)
                {
                    Debug.LogWarning("Inventory slot count mismatch, re-initializing UI.");
                    InitializeInventoryUI();
                    return;
                }
                if(_inventoryManager == null) return;
            }

            for (int i = 0; i < _slotUIs.Count; i++)
            {
                if (i < _inventoryManager.inventorySlots.Count && _inventoryManager.inventorySlots[i] != null)
                {
                    _slotUIs[i].UpdateSlot(_inventoryManager.inventorySlots[i]);
                }
                else if (i < _slotUIs.Count)
                {
                    _slotUIs[i].ClearSlotDisplay();
                }
            }
        }

        public void ToggleInventoryPanel(bool show)
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(show);
                _isInventoryOpen = show;
            }
        }
    }
}

// InventorySlotUI class (ensure this is defined, e.g., in InventorySlotUI.cs)
// public class InventorySlotUI : MonoBehaviour { ... public void InitializeSlot(int slotIndex) {...} ... }