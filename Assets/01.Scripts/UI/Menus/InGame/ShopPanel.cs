using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame
{
    public class ShopPanel : UIPanel
    {
        [Header("Shop Tabs")]
        [SerializeField] private Transform tabContainer;
        [SerializeField] private Button buyTabButton;
        [SerializeField] private Button sellTabButton;
        [SerializeField] private Button buybackTabButton;
        
        [Header("Shop Categories")]
        [SerializeField] private Transform categoryContainer;
        [SerializeField] private Button allCategoryButton;
        [SerializeField] private Button seedsCategoryButton;
        [SerializeField] private Button toolsCategoryButton;
        [SerializeField] private Button equipmentCategoryButton;
        [SerializeField] private Button materialsCategoryButton;
        [SerializeField] private Button consumablesCategoryButton;
        
        [Header("Shop Items Grid")]
        [SerializeField] private Transform shopItemsContainer;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private ScrollRect shopScrollRect;
        [SerializeField] private GridLayoutGroup shopGridLayout;
        
        [Header("Player Inventory")]
        [SerializeField] private Transform playerInventoryContainer;
        [SerializeField] private GameObject inventorySlotPrefab;
        [SerializeField] private ScrollRect inventoryScrollRect;
        [SerializeField] private GridLayoutGroup inventoryGridLayout;
        
        [Header("Transaction Panel")]
        [SerializeField] private GameObject transactionPanel;
        [SerializeField] private Image transactionItemIcon;
        [SerializeField] private TextMeshProUGUI transactionItemName;
        [SerializeField] private TextMeshProUGUI transactionItemDescription;
        [SerializeField] private TextMeshProUGUI transactionItemPrice;
        [SerializeField] private TextMeshProUGUI transactionTotalPrice;
        [SerializeField] private Slider quantitySlider;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button decreaseQuantityButton;
        [SerializeField] private Button increaseQuantityButton;
        [SerializeField] private Button confirmTransactionButton;
        [SerializeField] private Button cancelTransactionButton;
        
        [Header("Shop Info")]
        [SerializeField] private TextMeshProUGUI shopNameText;
        [SerializeField] private TextMeshProUGUI shopDescriptionText;
        [SerializeField] private Image shopKeeperPortrait;
        [SerializeField] private TextMeshProUGUI shopKeeperGreeting;
        
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerMoneyText;
        [SerializeField] private TextMeshProUGUI inventorySpaceText;
        [SerializeField] private Slider inventoryCapacityBar;
        
        [Header("Search and Sort")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Button sortOrderButton;
        [SerializeField] private TextMeshProUGUI sortOrderText;
        
        [Header("Shop Features")]
        [SerializeField] private Button refreshShopButton;
        [SerializeField] private Button favoriteItemsButton;
        [SerializeField] private Button shopHistoryButton;
        [SerializeField] private Button closeShopButton;
        
        [Header("Price Comparison")]
        [SerializeField] private GameObject priceComparisonPanel;
        [SerializeField] private TextMeshProUGUI basePriceText;
        [SerializeField] private TextMeshProUGUI discountText;
        [SerializeField] private TextMeshProUGUI finalPriceText;
        [SerializeField] private TextMeshProUGUI pricePerUnitText;
        
        [Header("Shop Effects")]
        [SerializeField] private ParticleSystem coinEffect;
        [SerializeField] private ParticleSystem purchaseEffect;
        [SerializeField] private GameObject transactionSuccessEffect;
        
        [Header("Audio")]
        [SerializeField] private AudioClip shopOpenSound;
        [SerializeField] private AudioClip shopCloseSound;
        [SerializeField] private AudioClip purchaseSound;
        [SerializeField] private AudioClip sellSound;
        [SerializeField] private AudioClip coinSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip buttonClickSound;
        
        [Header("Animation")]
        [SerializeField] private bool enableShopAnimations = true;
        [SerializeField] private float itemAnimationDelay = 0.03f;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Shop management
        private ShopData _currentShop;
        private ShopTab _currentTab = ShopTab.Buy;
        private ItemCategory _currentCategory = ItemCategory.All;
        private List<ShopItemData> _shopItems = new List<ShopItemData>();
        private List<ItemData> _playerInventory = new List<ItemData>();
        private List<ShopItemData> _buybackItems = new List<ShopItemData>();
        
        // UI management
        private List<ShopItemUI> _shopItemUIs = new List<ShopItemUI>();
        private List<InventorySlot> _inventorySlots = new List<InventorySlot>();
        private ShopItemData _selectedItem;
        private int _transactionQuantity = 1;
        
        // Search and sort
        private string _searchQuery = "";
        private ItemSortType _sortType = ItemSortType.Name;
        private bool _sortAscending = true;
        
        // Player data
        private int _playerMoney = 5000;
        private int _inventoryCapacity = 48;
        private int _currentInventoryCount = 0;
        
        // Shop features
        private List<ShopItemData> _favoriteItems = new List<ShopItemData>();
        private List<TransactionRecord> _transactionHistory = new List<TransactionRecord>();
        
        // Sort options
        private readonly string[] _sortOptions = { "Name", "Price", "Type", "Quality", "Stock" };

        protected override void Awake()
        {
            base.Awake();
            InitializeShop();
        }

        protected override void Start()
        {
            base.Start();
            SetupShopPanel();
            LoadShopData();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initially hide this panel
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleShopInput();
        }

        private void InitializeShop()
        {
            // Create default prefabs if none exist
            if (shopItemPrefab == null)
            {
                CreateDefaultShopItemPrefab();
            }
            
            if (inventorySlotPrefab == null)
            {
                CreateDefaultInventorySlotPrefab();
            }
            
            // Setup grid layouts
            SetupGridLayouts();
        }

        private void SetupShopPanel()
        {
            // Setup tab buttons
            if (buyTabButton != null)
            {
                buyTabButton.onClick.AddListener(() => SetTab(ShopTab.Buy));
                SetTabButtonActive(buyTabButton, true);
            }
            
            if (sellTabButton != null)
                sellTabButton.onClick.AddListener(() => SetTab(ShopTab.Sell));
            
            if (buybackTabButton != null)
                buybackTabButton.onClick.AddListener(() => SetTab(ShopTab.Buyback));
            
            // Setup category buttons
            if (allCategoryButton != null)
            {
                allCategoryButton.onClick.AddListener(() => SetCategory(ItemCategory.All));
                SetCategoryButtonActive(allCategoryButton, true);
            }
            
            if (seedsCategoryButton != null)
                seedsCategoryButton.onClick.AddListener(() => SetCategory(ItemCategory.Seeds));
            
            if (toolsCategoryButton != null)
                toolsCategoryButton.onClick.AddListener(() => SetCategory(ItemCategory.Tools));
            
            if (equipmentCategoryButton != null)
                equipmentCategoryButton.onClick.AddListener(() => SetCategory(ItemCategory.Equipment));
            
            if (materialsCategoryButton != null)
                materialsCategoryButton.onClick.AddListener(() => SetCategory(ItemCategory.Materials));
            
            if (consumablesCategoryButton != null)
                consumablesCategoryButton.onClick.AddListener(() => SetCategory(ItemCategory.Miscellaneous));
            
            // Setup transaction panel
            if (quantitySlider != null)
            {
                quantitySlider.onValueChanged.AddListener(OnQuantitySliderChanged);
                quantitySlider.minValue = 1;
            }
            
            if (quantityInput != null)
                quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);
            
            if (decreaseQuantityButton != null)
                decreaseQuantityButton.onClick.AddListener(DecreaseQuantity);
            
            if (increaseQuantityButton != null)
                increaseQuantityButton.onClick.AddListener(IncreaseQuantity);
            
            if (confirmTransactionButton != null)
                confirmTransactionButton.onClick.AddListener(ConfirmTransaction);
            
            if (cancelTransactionButton != null)
                cancelTransactionButton.onClick.AddListener(CancelTransaction);
            
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
            
            // Setup other buttons
            if (refreshShopButton != null)
                refreshShopButton.onClick.AddListener(RefreshShop);
            
            if (favoriteItemsButton != null)
                favoriteItemsButton.onClick.AddListener(ShowFavoriteItems);
            
            if (shopHistoryButton != null)
                shopHistoryButton.onClick.AddListener(ShowTransactionHistory);
            
            if (closeShopButton != null)
                closeShopButton.onClick.AddListener(CloseShop);
            
            // Initialize displays
            UpdateSortOrderDisplay();
            HideTransactionPanel();
            UpdatePlayerInfo();
        }

        private void SetupGridLayouts()
        {
            // Setup shop items grid
            if (shopGridLayout != null)
            {
                shopGridLayout.cellSize = new Vector2(100f, 120f);
                shopGridLayout.spacing = new Vector2(5f, 5f);
                shopGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                shopGridLayout.constraintCount = 6;
            }
            
            // Setup inventory grid
            if (inventoryGridLayout != null)
            {
                inventoryGridLayout.cellSize = new Vector2(80f, 80f);
                inventoryGridLayout.spacing = new Vector2(3f, 3f);
                inventoryGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                inventoryGridLayout.constraintCount = 8;
            }
        }

        private void CreateDefaultShopItemPrefab()
        {
            var itemObj = new GameObject("ShopItem");
            itemObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = itemObj.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 100f;
            layoutElement.preferredHeight = 120f;
            
            // Background
            var background = itemObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Button component
            var button = itemObj.AddComponent<Button>();
            button.targetGraphic = background;
            
            // Content container
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(itemObj.transform, false);
            var contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(5, 5, 5, 5);
            contentLayout.spacing = 3f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Item icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(contentObj.transform, false);
            var icon = iconObj.AddComponent<Image>();
            icon.color = Color.white;
            icon.preserveAspect = true;
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(60f, 60f);
            
            // Item name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(contentObj.transform, false);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Item Name";
            nameText.fontSize = 10f;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.overflowMode = TextOverflowModes.Ellipsis;
            
            // Price container
            var priceObj = new GameObject("Price");
            priceObj.transform.SetParent(contentObj.transform, false);
            var priceLayout = priceObj.AddComponent<HorizontalLayoutGroup>();
            priceLayout.childControlWidth = true;
            priceLayout.childControlHeight = true;
            priceLayout.childForceExpandWidth = true;
            priceLayout.childForceExpandHeight = false;
            
            // Price text
            var priceTextObj = new GameObject("PriceText");
            priceTextObj.transform.SetParent(priceObj.transform, false);
            var priceText = priceTextObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "$100";
            priceText.fontSize = 9f;
            priceText.color = Color.yellow;
            priceText.alignment = TextAlignmentOptions.Center;
            
            // Stock text
            var stockObj = new GameObject("Stock");
            stockObj.transform.SetParent(contentObj.transform, false);
            var stockText = stockObj.AddComponent<TextMeshProUGUI>();
            stockText.text = "Stock: 10";
            stockText.fontSize = 8f;
            stockText.color = Color.gray;
            stockText.alignment = TextAlignmentOptions.Center;
            
            // Add ShopItemUI component
            var shopItemUI = itemObj.AddComponent<ShopItemUI>();
            
            shopItemPrefab = itemObj;
            shopItemPrefab.SetActive(false);
        }

        private void CreateDefaultInventorySlotPrefab()
        {
            var slotObj = new GameObject("InventorySlot");
            var rect = slotObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80f, 80f);
            
            // Background
            var background = slotObj.AddComponent<Image>();
            background.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            
            // Button
            var button = slotObj.AddComponent<Button>();
            button.targetGraphic = background;
            
            // Item icon
            var iconObj = new GameObject("ItemIcon");
            iconObj.transform.SetParent(slotObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
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
            var quantityRect = quantityObj.GetComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(1f, 0f);
            quantityRect.anchorMax = new Vector2(1f, 0f);
            quantityRect.anchoredPosition = new Vector2(-5f, 5f);
            quantityRect.sizeDelta = new Vector2(20f, 15f);
            
            var quantityText = quantityObj.AddComponent<TextMeshProUGUI>();
            quantityText.text = "";
            quantityText.fontSize = 9f;
            quantityText.color = Color.white;
            quantityText.alignment = TextAlignmentOptions.BottomRight;
            
            inventorySlotPrefab = slotObj;
            inventorySlotPrefab.SetActive(false);
        }

        // Input handling
        private void HandleShopInput()
        {
            // ESC to close shop
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (transactionPanel != null && transactionPanel.activeInHierarchy)
                {
                    CancelTransaction();
                }
                else
                {
                    CloseShop();
                }
            }
            
            // Tab keys for quick tab switching
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetTab(ShopTab.Buy);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SetTab(ShopTab.Sell);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SetTab(ShopTab.Buyback);
            
            // R key to refresh shop
            if (Input.GetKeyDown(KeyCode.R))
                RefreshShop();
        }

        // Shop data management
        private void LoadShopData()
        {
            // This would normally load from ShopManager
            CreateMockShopData();
            RefreshShopDisplay();
        }

        private void CreateMockShopData()
        {
            // Create mock shop data
            _currentShop = new ShopData
            {
                shopName = "Pierre's General Store",
                shopDescription = "Your one-stop shop for all farming needs!",
                shopKeeperName = "Pierre",
                shopKeeperGreeting = "Welcome to my store! What can I get for you today?"
            };
            
            // Create mock shop items
            _shopItems.Clear();
            var sampleItems = new ShopItemData[]
            {
                new ShopItemData
                {
                    item = new ItemData { id = 1, name = "Parsnip Seeds", type = ItemType.Seeds, category = ItemCategory.Seeds, value = 20, quality = ItemQuality.Normal },
                    price = 20, basePrice = 20, stock = 50, maxStock = 50, restockRate = 10, isUnlimited = true
                },
                new ShopItemData
                {
                    item = new ItemData { id = 2, name = "Cauliflower Seeds", type = ItemType.Seeds, category = ItemCategory.Seeds, value = 80, quality = ItemQuality.Normal },
                    price = 80, basePrice = 80, stock = 25, maxStock = 25, restockRate = 5, isUnlimited = false
                },
                new ShopItemData
                {
                    item = new ItemData { id = 3, name = "Copper Watering Can", type = ItemType.Tool, category = ItemCategory.Tools, value = 2000, quality = ItemQuality.Normal },
                    price = 2000, basePrice = 2000, stock = 1, maxStock = 1, restockRate = 0, isUnlimited = false
                },
                new ShopItemData
                {
                    item = new ItemData { id = 4, name = "Basic Fertilizer", type = ItemType.Material, category = ItemCategory.Materials, value = 100, quality = ItemQuality.Normal },
                    price = 100, basePrice = 100, stock = 999, maxStock = 999, restockRate = 50, isUnlimited = true
                },
                new ShopItemData
                {
                    item = new ItemData { id = 5, name = "Energy Bar", type = ItemType.Consumable, category = ItemCategory.Miscellaneous, value = 50, quality = ItemQuality.Normal },
                    price = 50, basePrice = 50, stock = 20, maxStock = 20, restockRate = 10, isUnlimited = false
                }
            };
            
            foreach (var shopItem in sampleItems)
            {
                _shopItems.Add(shopItem);
            }
            
            // Create mock player inventory
            LoadPlayerInventory();
            
            // Update shop info
            UpdateShopInfo();
        }

        private void LoadPlayerInventory()
        {
            // This would normally load from InventoryManager
            _playerInventory.Clear();
            
            var sampleInventory = new ItemData[]
            {
                new ItemData { id = 10, name = "Parsnip", type = ItemType.Crop, category = ItemCategory.Crops, quantity = 15, value = 35, quality = ItemQuality.Normal },
                new ItemData { id = 11, name = "Wood", type = ItemType.Material, category = ItemCategory.Materials, quantity = 64, value = 2, quality = ItemQuality.Normal },
                new ItemData { id = 12, name = "Stone", type = ItemType.Material, category = ItemCategory.Materials, quantity = 32, value = 2, quality = ItemQuality.Normal },
                new ItemData { id = 13, name = "Copper Ore", type = ItemType.Material, category = ItemCategory.Materials, quantity = 8, value = 5, quality = ItemQuality.Normal }
            };
            
            foreach (var item in sampleInventory)
            {
                _playerInventory.Add(item);
            }
            
            _currentInventoryCount = _playerInventory.Sum(item => item.quantity);
        }

        private void RefreshShopDisplay()
        {
            // Filter and sort items based on current settings
            var filteredItems = FilterShopItems();
            
            // Clear existing items
            ClearShopItems();
            
            // Create new item displays
            StartCoroutine(CreateShopItems(filteredItems));
            
            // Update inventory display if in sell mode
            if (_currentTab == ShopTab.Sell)
            {
                RefreshInventoryDisplay();
            }
            
            // Update UI
            UpdatePlayerInfo();
        }

        private List<ShopItemData> FilterShopItems()
        {
            List<ShopItemData> items = _currentTab switch
            {
                ShopTab.Buy => _shopItems,
                ShopTab.Sell => ConvertInventoryToShopItems(),
                ShopTab.Buyback => _buybackItems,
                _ => _shopItems
            };
            
            var filtered = items.AsEnumerable();
            
            // Filter by category
            if (_currentCategory != ItemCategory.All)
            {
                filtered = filtered.Where(item => item.item.category == _currentCategory);
            }
            
            // Filter by search query
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                filtered = filtered.Where(item => 
                    item.item.name.ToLower().Contains(_searchQuery.ToLower()) ||
                    item.item.type.ToString().ToLower().Contains(_searchQuery.ToLower()));
            }
            
            // Sort items
            return SortShopItems(filtered.ToList());
        }

        private List<ShopItemData> ConvertInventoryToShopItems()
        {
            return _playerInventory.Select(item => new ShopItemData
            {
                item = item,
                price = Mathf.RoundToInt(item.value * 0.8f), // 80% of base value when selling
                basePrice = item.value,
                stock = item.quantity,
                maxStock = item.quantity,
                isUnlimited = false
            }).ToList();
        }

        private List<ShopItemData> SortShopItems(List<ShopItemData> items)
        {
            var sorted = items.AsEnumerable();
            
            sorted = _sortType switch
            {
                ItemSortType.Name => _sortAscending ? 
                    sorted.OrderBy(item => item.item.name) : 
                    sorted.OrderByDescending(item => item.item.name),
                ItemSortType.Value => _sortAscending ? 
                    sorted.OrderBy(item => item.price) : 
                    sorted.OrderByDescending(item => item.price),
                ItemSortType.Type => _sortAscending ? 
                    sorted.OrderBy(item => item.item.type) : 
                    sorted.OrderByDescending(item => item.item.type),
                ItemSortType.Quality => _sortAscending ? 
                    sorted.OrderBy(item => item.item.quality) : 
                    sorted.OrderByDescending(item => item.item.quality),
                _ => sorted.OrderBy(item => item.item.name)
            };
            
            return sorted.ToList();
        }

        private IEnumerator CreateShopItems(List<ShopItemData> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var shopItem = items[i];
                var itemObj = Instantiate(shopItemPrefab, shopItemsContainer);
                itemObj.SetActive(true);
                
                var shopItemUI = itemObj.GetComponent<ShopItemUI>();
                if (shopItemUI != null)
                {
                    shopItemUI.Initialize(shopItem, this, _currentTab);
                    _shopItemUIs.Add(shopItemUI);
                }
                
                // Animate item in if enabled
                if (enableShopAnimations)
                {
                    StartCoroutine(AnimateItemIn(itemObj.transform, i * itemAnimationDelay));
                }
                
                yield return null;
            }
        }

        private IEnumerator AnimateItemIn(Transform itemTransform, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            var startScale = itemTransform.localScale;
            itemTransform.localScale = Vector3.zero;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                
                itemTransform.localScale = Vector3.Lerp(Vector3.zero, startScale, slideInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            itemTransform.localScale = startScale;
        }

        private void ClearShopItems()
        {
            foreach (var item in _shopItemUIs)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            
            _shopItemUIs.Clear();
        }

        private void RefreshInventoryDisplay()
        {
            // Clear existing slots
            foreach (var slot in _inventorySlots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            _inventorySlots.Clear();
            
            // Create inventory slots for selling
            foreach (var item in _playerInventory)
            {
                var slotObj = Instantiate(inventorySlotPrefab, playerInventoryContainer);
                slotObj.SetActive(true);
                
                var slot = slotObj.GetComponent<InventorySlot>();
                if (slot == null)
                {
                    slot = slotObj.AddComponent<InventorySlot>();
                }
                
                slot.SetItem(item);
                _inventorySlots.Add(slot);
            }
        }

        // Tab and category management
        private void SetTab(ShopTab tab)
        {
            _currentTab = tab;
            RefreshShopDisplay();
            
            // Update tab button states
            SetTabButtonActive(buyTabButton, tab == ShopTab.Buy);
            SetTabButtonActive(sellTabButton, tab == ShopTab.Sell);
            SetTabButtonActive(buybackTabButton, tab == ShopTab.Buyback);
            
            // Show/hide inventory based on tab
            if (playerInventoryContainer != null)
            {
                playerInventoryContainer.gameObject.SetActive(tab == ShopTab.Sell);
            }
        }

        private void SetTabButtonActive(Button button, bool active)
        {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = active ? Color.yellow : Color.white;
            button.colors = colors;
        }

        private void SetCategory(ItemCategory category)
        {
            _currentCategory = category;
            RefreshShopDisplay();
            
            // Update category button states
            SetCategoryButtonActive(allCategoryButton, category == ItemCategory.All);
            SetCategoryButtonActive(seedsCategoryButton, category == ItemCategory.Seeds);
            SetCategoryButtonActive(toolsCategoryButton, category == ItemCategory.Tools);
            SetCategoryButtonActive(equipmentCategoryButton, category == ItemCategory.Equipment);
            SetCategoryButtonActive(materialsCategoryButton, category == ItemCategory.Materials);
            SetCategoryButtonActive(consumablesCategoryButton, category == ItemCategory.Miscellaneous);
        }

        private void SetCategoryButtonActive(Button button, bool active)
        {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = active ? Color.green : Color.white;
            button.colors = colors;
        }

        // Search and sort
        private void OnSearchChanged(string searchQuery)
        {
            _searchQuery = searchQuery;
            RefreshShopDisplay();
        }

        private void OnSortTypeChanged(int index)
        {
            _sortType = (ItemSortType)index;
            RefreshShopDisplay();
        }

        private void ToggleSortOrder()
        {
            _sortAscending = !_sortAscending;
            UpdateSortOrderDisplay();
            RefreshShopDisplay();
        }

        private void UpdateSortOrderDisplay()
        {
            if (sortOrderText != null)
            {
                sortOrderText.text = _sortAscending ? "↑" : "↓";
            }
        }

        // Item selection and transaction
        public void SelectShopItem(ShopItemData shopItem)
        {
            _selectedItem = shopItem;
            ShowTransactionPanel(shopItem);
        }

        private void ShowTransactionPanel(ShopItemData shopItem)
        {
            if (transactionPanel == null) return;
            
            transactionPanel.SetActive(true);
            
            // Update item info
            if (transactionItemIcon != null)
                transactionItemIcon.sprite = shopItem.item.icon;
            
            if (transactionItemName != null)
                transactionItemName.text = shopItem.item.name;
            
            if (transactionItemDescription != null)
                transactionItemDescription.text = shopItem.item.description;
            
            if (transactionItemPrice != null)
                transactionItemPrice.text = $"${shopItem.price}";
            
            // Setup quantity controls
            _transactionQuantity = 1;
            int maxQuantity = CalculateMaxTransactionQuantity(shopItem);
            
            if (quantitySlider != null)
            {
                quantitySlider.maxValue = maxQuantity;
                quantitySlider.value = 1;
            }
            
            if (quantityInput != null)
                quantityInput.text = "1";
            
            // Update transaction buttons
            string actionText = _currentTab switch
            {
                ShopTab.Buy => "Buy",
                ShopTab.Sell => "Sell",
                ShopTab.Buyback => "Buyback",
                _ => "Confirm"
            };
            
            var confirmButtonText = confirmTransactionButton?.GetComponentInChildren<TextMeshProUGUI>();
            if (confirmButtonText != null)
                confirmButtonText.text = actionText;
            
            // Update price display
            UpdateTransactionPrice();
            UpdatePriceComparison(shopItem);
        }

        private void HideTransactionPanel()
        {
            if (transactionPanel != null)
            {
                transactionPanel.SetActive(false);
            }
            
            _selectedItem = null;
            _transactionQuantity = 1;
        }

        private int CalculateMaxTransactionQuantity(ShopItemData shopItem)
        {
            if (_currentTab == ShopTab.Buy)
            {
                // Limited by stock and player money
                int maxByMoney = _playerMoney / shopItem.price;
                int maxByStock = shopItem.isUnlimited ? 999 : shopItem.stock;
                int maxByInventorySpace = _inventoryCapacity - _currentInventoryCount;
                
                return Mathf.Min(maxByMoney, maxByStock, maxByInventorySpace);
            }
            else if (_currentTab == ShopTab.Sell)
            {
                // Limited by owned quantity
                return shopItem.stock;
            }
            else // Buyback
            {
                // Limited by player money
                return _playerMoney / shopItem.price;
            }
        }

        // Quantity management
        private void OnQuantitySliderChanged(float value)
        {
            _transactionQuantity = Mathf.RoundToInt(value);
            
            if (quantityInput != null)
                quantityInput.text = _transactionQuantity.ToString();
            
            UpdateTransactionPrice();
        }

        private void OnQuantityInputChanged(string value)
        {
            if (int.TryParse(value, out int quantity))
            {
                _transactionQuantity = Mathf.Clamp(quantity, 1, (int)quantitySlider.maxValue);
                
                if (quantitySlider != null)
                    quantitySlider.value = _transactionQuantity;
                
                UpdateTransactionPrice();
            }
        }

        private void IncreaseQuantity()
        {
            if (_transactionQuantity < quantitySlider.maxValue)
            {
                _transactionQuantity++;
                
                if (quantitySlider != null)
                    quantitySlider.value = _transactionQuantity;
                
                if (quantityInput != null)
                    quantityInput.text = _transactionQuantity.ToString();
                
                UpdateTransactionPrice();
            }
        }

        private void DecreaseQuantity()
        {
            if (_transactionQuantity > 1)
            {
                _transactionQuantity--;
                
                if (quantitySlider != null)
                    quantitySlider.value = _transactionQuantity;
                
                if (quantityInput != null)
                    quantityInput.text = _transactionQuantity.ToString();
                
                UpdateTransactionPrice();
            }
        }

        private void UpdateTransactionPrice()
        {
            if (_selectedItem == null || transactionTotalPrice == null) return;
            
            int totalPrice = _selectedItem.price * _transactionQuantity;
            transactionTotalPrice.text = $"Total: ${totalPrice}";
            
            // Enable/disable confirm button based on affordability
            if (confirmTransactionButton != null)
            {
                bool canAfford = _currentTab == ShopTab.Sell || totalPrice <= _playerMoney;
                confirmTransactionButton.interactable = canAfford;
            }
        }

        private void UpdatePriceComparison(ShopItemData shopItem)
        {
            if (priceComparisonPanel == null) return;
            
            priceComparisonPanel.SetActive(true);
            
            if (basePriceText != null)
                basePriceText.text = $"Base: ${shopItem.basePrice}";
            
            if (finalPriceText != null)
                finalPriceText.text = $"Price: ${shopItem.price}";
            
            if (pricePerUnitText != null)
                pricePerUnitText.text = $"Per Unit: ${shopItem.price}";
            
            // Calculate discount
            float discountPercent = shopItem.basePrice > 0 ? 
                ((float)(shopItem.basePrice - shopItem.price) / shopItem.basePrice) * 100f : 0f;
            
            if (discountText != null)
            {
                if (discountPercent > 0)
                {
                    discountText.text = $"-{discountPercent:F0}%";
                    discountText.color = Color.green;
                }
                else if (discountPercent < 0)
                {
                    discountText.text = $"+{Mathf.Abs(discountPercent):F0}%";
                    discountText.color = Color.red;
                }
                else
                {
                    discountText.text = "No Change";
                    discountText.color = Color.gray;
                }
            }
        }

        // Transaction processing
        private void ConfirmTransaction()
        {
            if (_selectedItem == null) return;
            
            PlayButtonClickSound();
            
            bool success = _currentTab switch
            {
                ShopTab.Buy => ProcessPurchase(_selectedItem, _transactionQuantity),
                ShopTab.Sell => ProcessSale(_selectedItem, _transactionQuantity),
                ShopTab.Buyback => ProcessBuyback(_selectedItem, _transactionQuantity),
                _ => false
            };
            
            if (success)
            {
                // Play success effects
                PlayTransactionSuccessEffects();
                
                // Record transaction
                RecordTransaction(_selectedItem, _transactionQuantity, _currentTab);
                
                // Update displays
                RefreshShopDisplay();
                UpdatePlayerInfo();
                
                // Hide transaction panel
                HideTransactionPanel();
            }
            else
            {
                PlayErrorSound();
            }
        }

        private void CancelTransaction()
        {
            PlayButtonClickSound();
            HideTransactionPanel();
        }

        private bool ProcessPurchase(ShopItemData shopItem, int quantity)
        {
            int totalCost = shopItem.price * quantity;
            
            // Check if player can afford
            if (_playerMoney < totalCost)
            {
                Debug.Log("Not enough money!");
                return false;
            }
            
            // Check inventory space
            if (_currentInventoryCount + quantity > _inventoryCapacity)
            {
                Debug.Log("Not enough inventory space!");
                return false;
            }
            
            // Check stock
            if (!shopItem.isUnlimited && shopItem.stock < quantity)
            {
                Debug.Log("Not enough stock!");
                return false;
            }
            
            // Process purchase
            _playerMoney -= totalCost;
            
            // Add to player inventory
            AddToPlayerInventory(shopItem.item, quantity);
            
            // Update shop stock
            if (!shopItem.isUnlimited)
            {
                shopItem.stock -= quantity;
            }
            
            PlayPurchaseSound();
            return true;
        }

        private bool ProcessSale(ShopItemData shopItem, int quantity)
        {
            // Check if player has enough items
            var playerItem = _playerInventory.FirstOrDefault(item => item.id == shopItem.item.id);
            if (playerItem == null || playerItem.quantity < quantity)
            {
                Debug.Log("Not enough items to sell!");
                return false;
            }
            
            // Process sale
            int totalValue = shopItem.price * quantity;
            _playerMoney += totalValue;
            
            // Remove from player inventory
            RemoveFromPlayerInventory(shopItem.item, quantity);
            
            // Add to buyback items
            AddToBuyback(shopItem, quantity);
            
            PlaySellSound();
            return true;
        }

        private bool ProcessBuyback(ShopItemData shopItem, int quantity)
        {
            int totalCost = shopItem.price * quantity;
            
            // Check if player can afford
            if (_playerMoney < totalCost)
            {
                Debug.Log("Not enough money for buyback!");
                return false;
            }
            
            // Process buyback
            _playerMoney -= totalCost;
            
            // Add to player inventory
            AddToPlayerInventory(shopItem.item, quantity);
            
            // Remove from buyback
            RemoveFromBuyback(shopItem, quantity);
            
            PlayPurchaseSound();
            return true;
        }

        // Inventory management
        private void AddToPlayerInventory(ItemData item, int quantity)
        {
            var existingItem = _playerInventory.FirstOrDefault(i => i.id == item.id);
            if (existingItem != null)
            {
                existingItem.quantity += quantity;
            }
            else
            {
                var newItem = new ItemData
                {
                    id = item.id,
                    name = item.name,
                    description = item.description,
                    type = item.type,
                    category = item.category,
                    quantity = quantity,
                    value = item.value,
                    quality = item.quality,
                    icon = item.icon
                };
                _playerInventory.Add(newItem);
            }
            
            _currentInventoryCount += quantity;
        }

        private void RemoveFromPlayerInventory(ItemData item, int quantity)
        {
            var playerItem = _playerInventory.FirstOrDefault(i => i.id == item.id);
            if (playerItem != null)
            {
                playerItem.quantity -= quantity;
                _currentInventoryCount -= quantity;
                
                if (playerItem.quantity <= 0)
                {
                    _playerInventory.Remove(playerItem);
                }
            }
        }

        private void AddToBuyback(ShopItemData shopItem, int quantity)
        {
            var existingBuyback = _buybackItems.FirstOrDefault(item => item.item.id == shopItem.item.id);
            if (existingBuyback != null)
            {
                existingBuyback.stock += quantity;
            }
            else
            {
                var buybackItem = new ShopItemData
                {
                    item = shopItem.item,
                    price = Mathf.RoundToInt(shopItem.price * 1.2f), // 20% markup for buyback
                    basePrice = shopItem.basePrice,
                    stock = quantity,
                    maxStock = quantity,
                    isUnlimited = false
                };
                _buybackItems.Add(buybackItem);
            }
        }

        private void RemoveFromBuyback(ShopItemData shopItem, int quantity)
        {
            var buybackItem = _buybackItems.FirstOrDefault(item => item.item.id == shopItem.item.id);
            if (buybackItem != null)
            {
                buybackItem.stock -= quantity;
                
                if (buybackItem.stock <= 0)
                {
                    _buybackItems.Remove(buybackItem);
                }
            }
        }

        // UI updates
        private void UpdateShopInfo()
        {
            if (_currentShop == null) return;
            
            if (shopNameText != null)
                shopNameText.text = _currentShop.shopName;
            
            if (shopDescriptionText != null)
                shopDescriptionText.text = _currentShop.shopDescription;
            
            if (shopKeeperGreeting != null)
                shopKeeperGreeting.text = _currentShop.shopKeeperGreeting;
            
            if (shopKeeperPortrait != null)
                shopKeeperPortrait.sprite = _currentShop.shopKeeperPortrait;
        }

        private void UpdatePlayerInfo()
        {
            if (playerMoneyText != null)
                playerMoneyText.text = $"${_playerMoney:N0}";
            
            if (inventorySpaceText != null)
                inventorySpaceText.text = $"{_currentInventoryCount}/{_inventoryCapacity}";
            
            if (inventoryCapacityBar != null)
                inventoryCapacityBar.value = (float)_currentInventoryCount / _inventoryCapacity;
        }

        // Transaction history and favorites
        private void RecordTransaction(ShopItemData shopItem, int quantity, ShopTab transactionType)
        {
            var record = new TransactionRecord
            {
                itemName = shopItem.item.name,
                quantity = quantity,
                pricePerUnit = shopItem.price,
                totalPrice = shopItem.price * quantity,
                transactionType = transactionType,
                timestamp = System.DateTime.Now
            };
            
            _transactionHistory.Add(record);
            
            // Keep only last 50 transactions
            if (_transactionHistory.Count > 50)
            {
                _transactionHistory.RemoveAt(0);
            }
        }

        private void ShowFavoriteItems()
        {
            // Implementation for showing favorite items
            Debug.Log("Showing favorite items...");
        }

        private void ShowTransactionHistory()
        {
            // Implementation for showing transaction history
            Debug.Log("Showing transaction history...");
        }

        // Shop features
        private void RefreshShop()
        {
            PlayButtonClickSound();
            
            // Restock items
            foreach (var shopItem in _shopItems)
            {
                if (!shopItem.isUnlimited)
                {
                    shopItem.stock = Mathf.Min(shopItem.stock + shopItem.restockRate, shopItem.maxStock);
                }
            }
            
            RefreshShopDisplay();
            
            if (coinEffect != null)
                coinEffect.Play();
        }

        // Effects and audio
        private void PlayTransactionSuccessEffects()
        {
            if (purchaseEffect != null)
                purchaseEffect.Play();
            
            if (transactionSuccessEffect != null)
            {
                transactionSuccessEffect.SetActive(true);
                StartCoroutine(HideEffectAfterDelay(transactionSuccessEffect, 2f));
            }
            
            if (coinEffect != null)
                coinEffect.Play();
        }

        private IEnumerator HideEffectAfterDelay(GameObject effect, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            effect.SetActive(false);
        }

        private void PlayPurchaseSound()
        {
            Debug.Log("Purchase sound would play here");
        }

        private void PlaySellSound()
        {
            Debug.Log("Sell sound would play here");
        }

        private void PlayErrorSound()
        {
            Debug.Log("Error sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        // Navigation
        private void CloseShop()
        {
            PlayButtonClickSound();
            gameObject.SetActive(false);
            
            // Return to game or pause menu
            var pauseMenu = FindObjectOfType<PauseMenuPanel>();
            if (pauseMenu != null && pauseMenu.IsPaused)
            {
                // pauseMenu.ReturnToMainPauseMenu(); // Would implement this
            }
        }

        // Public interface
        public ShopData CurrentShop => _currentShop;
        public ShopTab CurrentTab => _currentTab;
        public int PlayerMoney => _playerMoney;
        public List<TransactionRecord> TransactionHistory => _transactionHistory;
    }

    // Helper component for shop items
    public class ShopItemUI : MonoBehaviour
    {
        private ShopItemData _shopItem;
        private ShopPanel _shopPanel;
        private ShopTab _shopTab;
        private Button _button;
        
        // UI components
        private Image _background;
        private Image _icon;
        private TextMeshProUGUI _nameText;
        private TextMeshProUGUI _priceText;
        private TextMeshProUGUI _stockText;

        public void Initialize(ShopItemData shopItem, ShopPanel shopPanel, ShopTab shopTab)
        {
            _shopItem = shopItem;
            _shopPanel = shopPanel;
            _shopTab = shopTab;
            
            // Get components
            _button = GetComponent<Button>();
            if (_button == null)
                _button = gameObject.AddComponent<Button>();
            
            _background = GetComponent<Image>();
            
            // Find child components
            _icon = transform.Find("Content/Icon")?.GetComponent<Image>();
            _nameText = transform.Find("Content/Name")?.GetComponent<TextMeshProUGUI>();
            _priceText = transform.Find("Content/Price/PriceText")?.GetComponent<TextMeshProUGUI>();
            _stockText = transform.Find("Content/Stock")?.GetComponent<TextMeshProUGUI>();
            
            // Setup button click
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _shopPanel.SelectShopItem(_shopItem));
            
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_shopItem?.item == null) return;
            
            // Update icon
            if (_icon != null && _shopItem.item.icon != null)
                _icon.sprite = _shopItem.item.icon;
            
            // Update name
            if (_nameText != null)
                _nameText.text = _shopItem.item.name;
            
            // Update price
            if (_priceText != null)
            {
                string pricePrefix = _shopTab == ShopTab.Sell ? "Sell: " : "";
                _priceText.text = $"{pricePrefix}${_shopItem.price}";
            }
            
            // Update stock
            if (_stockText != null)
            {
                if (_shopItem.isUnlimited)
                {
                    _stockText.text = "Unlimited";
                }
                else
                {
                    _stockText.text = $"Stock: {_shopItem.stock}";
                    
                    // Change color based on stock level
                    if (_shopItem.stock == 0)
                        _stockText.color = Color.red;
                    else if (_shopItem.stock < _shopItem.maxStock * 0.3f)
                        _stockText.color = Color.yellow;
                    else
                        _stockText.color = Color.white;
                }
            }
            
            // Update background based on availability
            if (_background != null)
            {
                bool isAvailable = _shopItem.stock > 0 || _shopItem.isUnlimited;
                _background.color = isAvailable ? 
                    new Color(0.2f, 0.2f, 0.2f, 0.9f) : 
                    new Color(0.1f, 0.1f, 0.1f, 0.9f);
            }
            
            // Enable/disable button
            if (_button != null)
            {
                _button.interactable = (_shopItem.stock > 0 || _shopItem.isUnlimited);
            }
        }

        public ShopItemData ShopItem => _shopItem;
    }

    // Data structures and enums
    [System.Serializable]
    public class ShopData
    {
        public string shopName;
        public string shopDescription;
        public string shopKeeperName;
        public string shopKeeperGreeting;
        public Sprite shopKeeperPortrait;
        public ShopType shopType;
        public float priceMultiplier = 1f;
        public bool hasSpecialOffers;
    }

    [System.Serializable]
    public class ShopItemData
    {
        public ItemData item;
        public int price;
        public int basePrice;
        public int stock;
        public int maxStock;
        public int restockRate;
        public bool isUnlimited;
        public bool isOnSale;
        public float saleMultiplier = 1f;
        public System.DateTime lastRestockTime;
    }

    [System.Serializable]
    public class TransactionRecord
    {
        public string itemName;
        public int quantity;
        public int pricePerUnit;
        public int totalPrice;
        public ShopTab transactionType;
        public System.DateTime timestamp;
    }

    public enum ShopTab
    {
        Buy,
        Sell,
        Buyback
    }

    public enum ShopType
    {
        General,
        Seeds,
        Tools,
        Blacksmith,
        Clinic,
        Tavern
    }
}