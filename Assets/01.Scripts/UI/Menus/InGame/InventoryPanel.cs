using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame
{
    public class InventoryPanel : UIPanel
    {
        [Header("Inventory Grid")]
        [SerializeField] private Transform inventoryGrid;
        [SerializeField] private GameObject inventorySlotPrefab;
        [SerializeField] private int inventoryRows = 6;
        [SerializeField] private int inventoryColumns = 8;
        [SerializeField] private float slotSize = 80f;
        [SerializeField] private float slotSpacing = 5f;
        
        [Header("Equipment Slots")]
        [SerializeField] private InventorySlot hatSlot;
        [SerializeField] private InventorySlot shirtSlot;
        [SerializeField] private InventorySlot pantsSlot;
        [SerializeField] private InventorySlot bootsSlot;
        [SerializeField] private InventorySlot toolSlot;
        [SerializeField] private InventorySlot accessorySlot;
        
        [Header("Categories")]
        [SerializeField] private Transform categoryContainer;
        [SerializeField] private Button allItemsButton;
        [SerializeField] private Button toolsButton;
        [SerializeField] private Button seedsButton;
        [SerializeField] private Button cropsButton;
        [SerializeField] private Button materialsButton;
        [SerializeField] private Button equipmentButton;
        [SerializeField] private Button miscButton;
        
        [Header("Item Details")]
        [SerializeField] private GameObject itemDetailsPanel;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private TextMeshProUGUI itemValueText;
        [SerializeField] private TextMeshProUGUI itemQuantityText;
        [SerializeField] private TextMeshProUGUI itemQualityText;
        
        [Header("Action Buttons")]
        [SerializeField] private Button useItemButton;
        [SerializeField] private Button dropItemButton;
        [SerializeField] private Button equipItemButton;
        [SerializeField] private Button sellItemButton;
        [SerializeField] private Button closeButton;
        
        [Header("Search and Sort")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Button sortOrderButton;
        [SerializeField] private TextMeshProUGUI sortOrderText;
        
        [Header("Inventory Info")]
        [SerializeField] private TextMeshProUGUI inventoryTitleText;
        [SerializeField] private TextMeshProUGUI capacityText;
        [SerializeField] private Slider capacityBar;
        [SerializeField] private TextMeshProUGUI totalValueText;
        
        [Header("Drag and Drop")]
        [SerializeField] private GameObject dragPreviewPrefab;
        [SerializeField] private Canvas dragCanvas;
        [SerializeField] private Color validDropColor = Color.green;
        [SerializeField] private Color invalidDropColor = Color.red;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem itemPickupEffect;
        [SerializeField] private AudioClip itemPickupSound;
        [SerializeField] private AudioClip itemDropSound;
        [SerializeField] private AudioClip itemEquipSound;
        [SerializeField] private AudioClip itemUseSound;
        
        [Header("Animation")]
        [SerializeField] private bool enableInventoryAnimations = true;
        [SerializeField] private float slotAnimationDelay = 0.02f;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Inventory management
        private List<InventorySlot> _inventorySlots = new List<InventorySlot>();
        private List<ItemData> _inventoryItems = new List<ItemData>();
        private ItemCategory _currentCategory = ItemCategory.All;
        private string _searchQuery = "";
        private ItemSortType _sortType = ItemSortType.Name;
        private bool _sortAscending = true;
        
        // Selection and interaction
        private InventorySlot _selectedSlot;
        private ItemData _selectedItem;
        private bool _isDragging = false;
        private GameObject _dragPreview;
        private InventorySlot _dragSourceSlot;
        
        // Capacity management
        private int _maxCapacity = 48; // 6x8 grid
        private int _currentItemCount = 0;
        
        // Sort options
        private readonly string[] _sortOptions = { "Name", "Type", "Value", "Quantity", "Quality", "Recent" };

        protected override void Awake()
        {
            base.Awake();
            InitializeInventory();
        }

        protected override void Start()
        {
            base.Start();
            SetupInventoryPanel();
            CreateInventoryGrid();
            LoadInventoryData();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initially hide this panel
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleInventoryInput();
            UpdateDragPreview();
        }

        private void InitializeInventory()
        {
            // Create drag canvas if not assigned
            if (dragCanvas == null)
            {
                var canvasObj = new GameObject("DragCanvas");
                dragCanvas = canvasObj.AddComponent<Canvas>();
                dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                dragCanvas.sortingOrder = 1000;
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create default drag preview if none exists
            if (dragPreviewPrefab == null)
            {
                CreateDefaultDragPreview();
            }
            
            // Create default slot prefab if none exists
            if (inventorySlotPrefab == null)
            {
                CreateDefaultSlotPrefab();
            }
        }

        private void SetupInventoryPanel()
        {
            // Setup category buttons
            if (allItemsButton != null)
            {
                allItemsButton.onClick.AddListener(() => SetCategory(ItemCategory.All));
                SetCategoryButtonActive(allItemsButton, true);
            }
            
            if (toolsButton != null)
                toolsButton.onClick.AddListener(() => SetCategory(ItemCategory.Tools));
            
            if (seedsButton != null)
                seedsButton.onClick.AddListener(() => SetCategory(ItemCategory.Seeds));
            
            if (cropsButton != null)
                cropsButton.onClick.AddListener(() => SetCategory(ItemCategory.Crops));
            
            if (materialsButton != null)
                materialsButton.onClick.AddListener(() => SetCategory(ItemCategory.Materials));
            
            if (equipmentButton != null)
                equipmentButton.onClick.AddListener(() => SetCategory(ItemCategory.Equipment));
            
            if (miscButton != null)
                miscButton.onClick.AddListener(() => SetCategory(ItemCategory.Miscellaneous));
            
            // Setup action buttons
            if (useItemButton != null)
            {
                useItemButton.onClick.AddListener(UseSelectedItem);
                useItemButton.gameObject.SetActive(false);
            }
            
            if (dropItemButton != null)
            {
                dropItemButton.onClick.AddListener(DropSelectedItem);
                dropItemButton.gameObject.SetActive(false);
            }
            
            if (equipItemButton != null)
            {
                equipItemButton.onClick.AddListener(EquipSelectedItem);
                equipItemButton.gameObject.SetActive(false);
            }
            
            if (sellItemButton != null)
            {
                sellItemButton.onClick.AddListener(SellSelectedItem);
                sellItemButton.gameObject.SetActive(false);
            }
            
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseInventory);
            
            // Setup search and sort
            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);
            
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(_sortOptions.ToList());
                sortDropdown.onValueChanged.AddListener(OnSortTypeChanged);
            }
            
            if (sortOrderButton != null)
                sortOrderButton.onClick.AddListener(ToggleSortOrder);
            
            // Initialize UI
            UpdateSortOrderDisplay();
            HideItemDetails();
            UpdateInventoryInfo();
        }

        private void CreateInventoryGrid()
        {
            if (inventoryGrid == null || inventorySlotPrefab == null) return;
            
            // Clear existing slots
            foreach (Transform child in inventoryGrid)
            {
                Destroy(child.gameObject);
            }
            _inventorySlots.Clear();
            
            // Setup grid layout
            var gridLayout = inventoryGrid.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = inventoryGrid.gameObject.AddComponent<GridLayoutGroup>();
            }
            
            gridLayout.cellSize = new Vector2(slotSize, slotSize);
            gridLayout.spacing = new Vector2(slotSpacing, slotSpacing);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = inventoryColumns;
            
            // Create inventory slots
            for (int i = 0; i < _maxCapacity; i++)
            {
                var slotObj = Instantiate(inventorySlotPrefab, inventoryGrid);
                var slot = slotObj.GetComponent<InventorySlot>();
                
                if (slot == null)
                {
                    slot = slotObj.AddComponent<InventorySlot>();
                }
                
                slot.Initialize(i, this);
                _inventorySlots.Add(slot);
                
                // Animate slot in if enabled
                if (enableInventoryAnimations)
                {
                    StartCoroutine(AnimateSlotIn(slotObj.transform, i * slotAnimationDelay));
                }
            }
        }

        private IEnumerator AnimateSlotIn(Transform slotTransform, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            var startScale = slotTransform.localScale;
            slotTransform.localScale = Vector3.zero;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                
                slotTransform.localScale = Vector3.Lerp(Vector3.zero, startScale, slideInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            slotTransform.localScale = startScale;
        }

        private void CreateDefaultSlotPrefab()
        {
            var slotObj = new GameObject("InventorySlot");
            var rect = slotObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(slotSize, slotSize);
            
            // Background image
            var background = slotObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Add border
            var border = new GameObject("Border");
            border.transform.SetParent(slotObj.transform, false);
            var borderRect = border.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            
            var borderImage = border.AddComponent<Image>();
            borderImage.color = Color.gray;
            borderImage.type = Image.Type.Sliced;
            
            // Item icon
            var iconObj = new GameObject("ItemIcon");
            iconObj.transform.SetParent(slotObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.one * 5f;
            iconRect.offsetMax = Vector2.one * -5f;
            
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            
            // Quantity text
            var quantityObj = new GameObject("Quantity");
            quantityObj.transform.SetParent(slotObj.transform, false);
            var quantityRect = quantityObj.AddComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(1f, 0f);
            quantityRect.anchorMax = new Vector2(1f, 0f);
            quantityRect.anchoredPosition = new Vector2(-5f, 5f);
            quantityRect.sizeDelta = new Vector2(20f, 15f);
            
            var quantityText = quantityObj.AddComponent<TextMeshProUGUI>();
            quantityText.text = "";
            quantityText.fontSize = 10f;
            quantityText.color = Color.white;
            quantityText.alignment = TextAlignmentOptions.BottomRight;
            
            // Add button for interaction
            var button = slotObj.AddComponent<Button>();
            button.targetGraphic = background;
            
            inventorySlotPrefab = slotObj;
            inventorySlotPrefab.SetActive(false);
        }

        private void CreateDefaultDragPreview()
        {
            var previewObj = new GameObject("DragPreview");
            var rect = previewObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(slotSize * 0.8f, slotSize * 0.8f);
            
            // Semi-transparent background
            var background = previewObj.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.7f);
            
            // Item icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(previewObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;
            
            // Disable raycast to prevent interference
            var canvasGroup = previewObj.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            
            dragPreviewPrefab = previewObj;
            dragPreviewPrefab.SetActive(false);
        }

        // Input handling
        private void HandleInventoryInput()
        {
            // ESC to close inventory
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseInventory();
            }
            
            // Number keys for quick selection
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    SelectSlotByIndex(i - 1);
                }
            }
            
            // Delete key to drop selected item
            if (Input.GetKeyDown(KeyCode.Delete) && _selectedItem != null)
            {
                DropSelectedItem();
            }
            
            // Right click for context menu (future feature)
            if (Input.GetMouseButtonDown(1) && _selectedSlot != null)
            {
                // ShowContextMenu();
            }
        }

        // Item management
        private void LoadInventoryData()
        {
            // This would normally load from InventoryManager
            // For now, create some mock data
            CreateMockInventoryData();
            RefreshInventoryDisplay();
        }

        private void CreateMockInventoryData()
        {
            _inventoryItems.Clear();
            
            // Create some sample items
            var sampleItems = new ItemData[]
            {
                new ItemData { id = 1, name = "Parsnip Seeds", type = ItemType.Seeds, category = ItemCategory.Seeds, quantity = 15, value = 20, quality = ItemQuality.Normal, icon = null },
                new ItemData { id = 2, name = "Copper Axe", type = ItemType.Tool, category = ItemCategory.Tools, quantity = 1, value = 200, quality = ItemQuality.Normal, icon = null },
                new ItemData { id = 3, name = "Parsnip", type = ItemType.Crop, category = ItemCategory.Crops, quantity = 8, value = 35, quality = ItemQuality.Silver, icon = null },
                new ItemData { id = 4, name = "Wood", type = ItemType.Material, category = ItemCategory.Materials, quantity = 64, value = 2, quality = ItemQuality.Normal, icon = null },
                new ItemData { id = 5, name = "Stone", type = ItemType.Material, category = ItemCategory.Materials, quantity = 32, value = 2, quality = ItemQuality.Normal, icon = null },
                new ItemData { id = 6, name = "Farmer Hat", type = ItemType.Equipment, category = ItemCategory.Equipment, quantity = 1, value = 150, quality = ItemQuality.Gold, icon = null },
                new ItemData { id = 7, name = "Energy Bar", type = ItemType.Consumable, category = ItemCategory.Miscellaneous, quantity = 5, value = 50, quality = ItemQuality.Normal, icon = null }
            };
            
            foreach (var item in sampleItems)
            {
                _inventoryItems.Add(item);
            }
            
            _currentItemCount = _inventoryItems.Sum(item => item.quantity);
        }

        private void RefreshInventoryDisplay()
        {
            // Filter items based on current category and search
            var filteredItems = FilterItems(_inventoryItems);
            
            // Sort items
            var sortedItems = SortItems(filteredItems);
            
            // Clear all slots
            foreach (var slot in _inventorySlots)
            {
                slot.ClearSlot();
            }
            
            // Place items in slots
            for (int i = 0; i < sortedItems.Count && i < _inventorySlots.Count; i++)
            {
                _inventorySlots[i].SetItem(sortedItems[i]);
            }
            
            // Update inventory info
            UpdateInventoryInfo();
        }

        private List<ItemData> FilterItems(List<ItemData> items)
        {
            var filtered = items.AsEnumerable();
            
            // Filter by category
            if (_currentCategory != ItemCategory.All)
            {
                filtered = filtered.Where(item => item.category == _currentCategory);
            }
            
            // Filter by search query
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                filtered = filtered.Where(item => 
                    item.name.ToLower().Contains(_searchQuery.ToLower()) ||
                    item.type.ToString().ToLower().Contains(_searchQuery.ToLower()));
            }
            
            return filtered.ToList();
        }

        private List<ItemData> SortItems(List<ItemData> items)
        {
            var sorted = items.AsEnumerable();
            
            sorted = _sortType switch
            {
                ItemSortType.Name => _sortAscending ? 
                    sorted.OrderBy(item => item.name) : 
                    sorted.OrderByDescending(item => item.name),
                ItemSortType.Type => _sortAscending ? 
                    sorted.OrderBy(item => item.type) : 
                    sorted.OrderByDescending(item => item.type),
                ItemSortType.Value => _sortAscending ? 
                    sorted.OrderBy(item => item.value) : 
                    sorted.OrderByDescending(item => item.value),
                ItemSortType.Quantity => _sortAscending ? 
                    sorted.OrderBy(item => item.quantity) : 
                    sorted.OrderByDescending(item => item.quantity),
                ItemSortType.Quality => _sortAscending ? 
                    sorted.OrderBy(item => item.quality) : 
                    sorted.OrderByDescending(item => item.quality),
                ItemSortType.Recent => _sortAscending ? 
                    sorted.OrderBy(item => item.lastUsed) : 
                    sorted.OrderByDescending(item => item.lastUsed),
                _ => sorted.OrderBy(item => item.name)
            };
            
            return sorted.ToList();
        }

        // Category management
        private void SetCategory(ItemCategory category)
        {
            _currentCategory = category;
            RefreshInventoryDisplay();
            
            // Update category button states
            SetCategoryButtonActive(allItemsButton, category == ItemCategory.All);
            SetCategoryButtonActive(toolsButton, category == ItemCategory.Tools);
            SetCategoryButtonActive(seedsButton, category == ItemCategory.Seeds);
            SetCategoryButtonActive(cropsButton, category == ItemCategory.Crops);
            SetCategoryButtonActive(materialsButton, category == ItemCategory.Materials);
            SetCategoryButtonActive(equipmentButton, category == ItemCategory.Equipment);
            SetCategoryButtonActive(miscButton, category == ItemCategory.Miscellaneous);
        }

        private void SetCategoryButtonActive(Button button, bool active)
        {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = active ? Color.yellow : Color.white;
            button.colors = colors;
        }

        // Search and sort
        private void OnSearchChanged(string searchQuery)
        {
            _searchQuery = searchQuery;
            RefreshInventoryDisplay();
        }

        private void OnSortTypeChanged(int index)
        {
            _sortType = (ItemSortType)index;
            RefreshInventoryDisplay();
        }

        private void ToggleSortOrder()
        {
            _sortAscending = !_sortAscending;
            UpdateSortOrderDisplay();
            RefreshInventoryDisplay();
        }

        private void UpdateSortOrderDisplay()
        {
            if (sortOrderText != null)
            {
                sortOrderText.text = _sortAscending ? "↑" : "↓";
            }
        }

        // Item selection and details
        public void SelectSlot(InventorySlot slot)
        {
            // Deselect previous slot
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
            }
            
            // Select new slot
            _selectedSlot = slot;
            _selectedItem = slot?.GetItem();
            
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(true);
                ShowItemDetails(_selectedItem);
            }
            else
            {
                HideItemDetails();
            }
        }

        private void SelectSlotByIndex(int index)
        {
            if (index >= 0 && index < _inventorySlots.Count)
            {
                SelectSlot(_inventorySlots[index]);
            }
        }

        private void ShowItemDetails(ItemData item)
        {
            if (itemDetailsPanel == null || item == null) return;
            
            itemDetailsPanel.SetActive(true);
            
            if (itemIcon != null)
                itemIcon.sprite = item.icon;
            
            if (itemNameText != null)
                itemNameText.text = item.name;
            
            if (itemDescriptionText != null)
                itemDescriptionText.text = item.description;
            
            if (itemTypeText != null)
                itemTypeText.text = $"Type: {item.type}";
            
            if (itemValueText != null)
                itemValueText.text = $"Value: ${item.value}";
            
            if (itemQuantityText != null)
                itemQuantityText.text = $"Quantity: {item.quantity}";
            
            if (itemQualityText != null)
            {
                itemQualityText.text = $"Quality: {item.quality}";
                itemQualityText.color = GetQualityColor(item.quality);
            }
            
            // Show/hide action buttons based on item type
            UpdateActionButtons(item);
        }

        private void HideItemDetails()
        {
            if (itemDetailsPanel != null)
            {
                itemDetailsPanel.SetActive(false);
            }
            
            // Hide all action buttons
            if (useItemButton != null) useItemButton.gameObject.SetActive(false);
            if (dropItemButton != null) dropItemButton.gameObject.SetActive(false);
            if (equipItemButton != null) equipItemButton.gameObject.SetActive(false);
            if (sellItemButton != null) sellItemButton.gameObject.SetActive(false);
        }

        private void UpdateActionButtons(ItemData item)
        {
            if (item == null) return;
            
            // Use button for consumables
            if (useItemButton != null)
                useItemButton.gameObject.SetActive(item.type == ItemType.Consumable);
            
            // Drop button for all items
            if (dropItemButton != null)
                dropItemButton.gameObject.SetActive(true);
            
            // Equip button for equipment and tools
            if (equipItemButton != null)
                equipItemButton.gameObject.SetActive(item.type == ItemType.Equipment || item.type == ItemType.Tool);
            
            // Sell button for sellable items
            if (sellItemButton != null)
                sellItemButton.gameObject.SetActive(item.value > 0);
        }

        private Color GetQualityColor(ItemQuality quality)
        {
            return quality switch
            {
                ItemQuality.Poor => Color.gray,
                ItemQuality.Normal => Color.white,
                ItemQuality.Silver => Color.cyan,
                ItemQuality.Gold => Color.yellow,
                ItemQuality.Iridium => Color.magenta,
                _ => Color.white
            };
        }

        // Item actions
        private void UseSelectedItem()
        {
            if (_selectedItem == null) return;
            
            PlayItemUseSound();
            
            // This would call the item's use function
            Debug.Log($"Used {_selectedItem.name}");
            
            // Decrease quantity
            _selectedItem.quantity--;
            if (_selectedItem.quantity <= 0)
            {
                RemoveItem(_selectedItem);
            }
            
            RefreshInventoryDisplay();
        }

        private void DropSelectedItem()
        {
            if (_selectedItem == null) return;
            
            PlayItemDropSound();
            
            // This would drop the item in the world
            Debug.Log($"Dropped {_selectedItem.name}");
            
            RemoveItem(_selectedItem);
            RefreshInventoryDisplay();
            HideItemDetails();
        }

        private void EquipSelectedItem()
        {
            if (_selectedItem == null) return;
            
            PlayItemEquipSound();
            
            // This would equip the item
            Debug.Log($"Equipped {_selectedItem.name}");
            
            // Move to appropriate equipment slot
            EquipToSlot(_selectedItem);
        }

        private void SellSelectedItem()
        {
            if (_selectedItem == null) return;
            
            // This would sell the item
            Debug.Log($"Sold {_selectedItem.name} for ${_selectedItem.value}");
            
            RemoveItem(_selectedItem);
            RefreshInventoryDisplay();
            HideItemDetails();
        }

        private void EquipToSlot(ItemData item)
        {
            InventorySlot targetSlot = item.type switch
            {
                ItemType.Tool => toolSlot,
                ItemType.Equipment when item.name.Contains("Hat") => hatSlot,
                ItemType.Equipment when item.name.Contains("Shirt") => shirtSlot,
                ItemType.Equipment when item.name.Contains("Pants") => pantsSlot,
                ItemType.Equipment when item.name.Contains("Boots") => bootsSlot,
                _ => accessorySlot
            };
            
            if (targetSlot != null)
            {
                // Unequip current item if any
                var currentItem = targetSlot.GetItem();
                if (currentItem != null)
                {
                    AddItem(currentItem);
                }
                
                // Equip new item
                targetSlot.SetItem(item);
                RemoveItem(item);
                RefreshInventoryDisplay();
            }
        }

        // Inventory operations
        public bool AddItem(ItemData item)
        {
            // Check if we can stack with existing item
            var existingItem = _inventoryItems.FirstOrDefault(i => i.id == item.id && i.CanStackWith(item));
            if (existingItem != null)
            {
                existingItem.quantity += item.quantity;
                RefreshInventoryDisplay();
                return true;
            }
            
            // Check if we have space
            if (_inventoryItems.Count >= _maxCapacity)
            {
                Debug.Log("Inventory is full!");
                return false;
            }
            
            // Add new item
            _inventoryItems.Add(item);
            _currentItemCount += item.quantity;
            
            PlayItemPickupSound();
            RefreshInventoryDisplay();
            return true;
        }

        public bool RemoveItem(ItemData item)
        {
            if (_inventoryItems.Remove(item))
            {
                _currentItemCount -= item.quantity;
                RefreshInventoryDisplay();
                return true;
            }
            return false;
        }

        // Drag and drop system
        public void StartDrag(InventorySlot sourceSlot)
        {
            if (sourceSlot.GetItem() == null) return;
            
            _isDragging = true;
            _dragSourceSlot = sourceSlot;
            
            // Create drag preview
            _dragPreview = Instantiate(dragPreviewPrefab, dragCanvas.transform);
            _dragPreview.SetActive(true);
            
            // Set preview icon
            var previewIcon = _dragPreview.GetComponentInChildren<Image>();
            if (previewIcon != null && sourceSlot.GetItem().icon != null)
            {
                previewIcon.sprite = sourceSlot.GetItem().icon;
            }
        }

        public void EndDrag(InventorySlot targetSlot)
        {
            if (!_isDragging) return;
            
            _isDragging = false;
            
            // Destroy drag preview
            if (_dragPreview != null)
            {
                Destroy(_dragPreview);
                _dragPreview = null;
            }
            
            // Handle item swap/move
            if (targetSlot != null && targetSlot != _dragSourceSlot)
            {
                SwapItems(_dragSourceSlot, targetSlot);
            }
            
            _dragSourceSlot = null;
        }

        private void SwapItems(InventorySlot sourceSlot, InventorySlot targetSlot)
        {
            var sourceItem = sourceSlot.GetItem();
            var targetItem = targetSlot.GetItem();
            
            // Simple swap
            sourceSlot.SetItem(targetItem);
            targetSlot.SetItem(sourceItem);
            
            PlayItemDropSound();
        }

        private void UpdateDragPreview()
        {
            if (_isDragging && _dragPreview != null)
            {
                Vector2 mousePosition = Input.mousePosition;
                _dragPreview.transform.position = mousePosition;
            }
        }

        // UI management
        private void UpdateInventoryInfo()
        {
            if (inventoryTitleText != null)
                inventoryTitleText.text = "Inventory";
            
            if (capacityText != null)
                capacityText.text = $"{_currentItemCount}/{_maxCapacity}";
            
            if (capacityBar != null)
                capacityBar.value = (float)_currentItemCount / _maxCapacity;
            
            if (totalValueText != null)
            {
                int totalValue = _inventoryItems.Sum(item => item.value * item.quantity);
                totalValueText.text = $"Total Value: ${totalValue:N0}";
            }
        }

        private void CloseInventory()
        {
            gameObject.SetActive(false);
            
            // Return to pause menu if it was open
            var pauseMenu = FindObjectOfType<PauseMenuPanel>();
            if (pauseMenu != null && pauseMenu.IsPaused)
            {
                // pauseMenu.ReturnToMainPauseMenu(); // Would implement this
            }
        }

        // Audio methods
        private void PlayItemPickupSound()
        {
            Debug.Log("Item pickup sound would play here");
        }

        private void PlayItemDropSound()
        {
            Debug.Log("Item drop sound would play here");
        }

        private void PlayItemEquipSound()
        {
            Debug.Log("Item equip sound would play here");
        }

        private void PlayItemUseSound()
        {
            Debug.Log("Item use sound would play here");
        }

        // Public interface
        public int CurrentItemCount => _currentItemCount;
        public int MaxCapacity => _maxCapacity;
        public bool IsFull => _currentItemCount >= _maxCapacity;
        public ItemData SelectedItem => _selectedItem;
    }

    // Helper component for individual inventory slots
    public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("Slot Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image qualityFrame;
        
        private ItemData _item;
        private int _slotIndex;
        private InventoryPanel _inventoryPanel;
        private bool _isSelected = false;
        private bool _isHovered = false;

        public void Initialize(int slotIndex, InventoryPanel inventoryPanel)
        {
            _slotIndex = slotIndex;
            _inventoryPanel = inventoryPanel;
            
            // Auto-find components if not assigned
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (itemIcon == null)
                itemIcon = GetComponentInChildren<Image>();
            
            if (quantityText == null)
                quantityText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void SetItem(ItemData item)
        {
            _item = item;
            UpdateSlotDisplay();
        }

        public void ClearSlot()
        {
            _item = null;
            UpdateSlotDisplay();
        }

        public ItemData GetItem()
        {
            return _item;
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateSlotDisplay();
        }

        private void UpdateSlotDisplay()
        {
            // Update item icon
            if (itemIcon != null)
            {
                if (_item != null && _item.icon != null)
                {
                    itemIcon.sprite = _item.icon;
                    itemIcon.color = Color.white;
                }
                else
                {
                    itemIcon.sprite = null;
                    itemIcon.color = Color.clear;
                }
            }
            
            // Update quantity text
            if (quantityText != null)
            {
                if (_item != null && _item.quantity > 1)
                {
                    quantityText.text = _item.quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
            
            // Update background color based on state
            if (backgroundImage != null)
            {
                Color backgroundColor = Color.gray;
                
                if (_isSelected)
                    backgroundColor = Color.yellow;
                else if (_isHovered)
                    backgroundColor = Color.white;
                else if (_item != null)
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                else
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                
                backgroundImage.color = backgroundColor;
            }
            
            // Update quality frame
            if (qualityFrame != null && _item != null)
            {
                qualityFrame.gameObject.SetActive(true);
                qualityFrame.color = GetQualityColor(_item.quality);
            }
            else if (qualityFrame != null)
            {
                qualityFrame.gameObject.SetActive(false);
            }
        }

        private Color GetQualityColor(ItemQuality quality)
        {
            return quality switch
            {
                ItemQuality.Poor => Color.gray,
                ItemQuality.Normal => Color.white,
                ItemQuality.Silver => Color.cyan,
                ItemQuality.Gold => Color.yellow,
                ItemQuality.Iridium => Color.magenta,
                _ => Color.white
            };
        }

        // Event handlers
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateSlotDisplay();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateSlotDisplay();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _inventoryPanel?.SelectSlot(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_item != null)
            {
                _inventoryPanel?.StartDrag(this);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Drag preview is handled in InventoryPanel
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Find the slot we're dropping on
            var targetSlot = eventData.pointerCurrentRaycast.gameObject?.GetComponent<InventorySlot>();
            _inventoryPanel?.EndDrag(targetSlot);
        }

        public void OnDrop(PointerEventData eventData)
        {
            // This is called on the target slot
        }
    }

    // Data structures and enums
    [System.Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public string description;
        public ItemType type;
        public ItemCategory category;
        public int quantity;
        public int value;
        public ItemQuality quality;
        public Sprite icon;
        public System.DateTime lastUsed;
        
        public bool CanStackWith(ItemData other)
        {
            return other != null && 
                   id == other.id && 
                   quality == other.quality &&
                   type != ItemType.Equipment && 
                   type != ItemType.Tool;
        }
    }

    public enum ItemType
    {
        Seeds,
        Crop,
        Tool,
        Equipment,
        Material,
        Consumable,
        Misc
    }

    public enum ItemCategory
    {
        All,
        Seeds,
        Crops,
        Tools,
        Equipment,
        Materials,
        Miscellaneous
    }

    public enum ItemQuality
    {
        Poor,
        Normal,
        Silver,
        Gold,
        Iridium
    }

    public enum ItemSortType
    {
        Name = 0,
        Type = 1,
        Value = 2,
        Quantity = 3,
        Quality = 4,
        Recent = 5
    }
}