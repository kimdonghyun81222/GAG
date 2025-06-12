using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame
{
    public class CraftingPanel : UIPanel
    {
        [Header("Crafting Categories")]
        [SerializeField] private Transform categoryContainer;
        [SerializeField] private Button allCategoryButton;
        [SerializeField] private Button toolsCategoryButton;
        [SerializeField] private Button equipmentCategoryButton;
        [SerializeField] private Button consumablesCategoryButton;
        [SerializeField] private Button buildingsCategoryButton;
        [SerializeField] private Button decorationsCategoryButton;
        
        [Header("Recipe List")]
        [SerializeField] private Transform recipeListContainer;
        [SerializeField] private GameObject recipeItemPrefab;
        [SerializeField] private ScrollRect recipeScrollRect;
        [SerializeField] private int maxVisibleRecipes = 8;
        
        [Header("Crafting Station")]
        [SerializeField] private GameObject craftingStationPanel;
        [SerializeField] private Image resultItemIcon;
        [SerializeField] private TextMeshProUGUI resultItemName;
        [SerializeField] private TextMeshProUGUI resultItemDescription;
        [SerializeField] private TextMeshProUGUI resultQuantityText;
        
        [Header("Materials Required")]
        [SerializeField] private Transform materialsContainer;
        [SerializeField] private GameObject materialSlotPrefab;
        [SerializeField] private ScrollRect materialsScrollRect;
        
        [Header("Crafting Progress")]
        [SerializeField] private GameObject craftingProgressPanel;
        [SerializeField] private Slider craftingProgressBar;
        [SerializeField] private TextMeshProUGUI craftingProgressText;
        [SerializeField] private Button cancelCraftingButton;
        
        [Header("Crafting Controls")]
        [SerializeField] private Slider quantitySlider;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button decreaseQuantityButton;
        [SerializeField] private Button increaseQuantityButton;
        [SerializeField] private Button craftButton;
        [SerializeField] private Button craftAllButton;
        [SerializeField] private Button queueCraftButton;
        
        [Header("Player Inventory")]
        [SerializeField] private Transform playerInventoryContainer;
        [SerializeField] private GameObject inventorySlotPrefab;
        [SerializeField] private ScrollRect inventoryScrollRect;
        [SerializeField] private GridLayoutGroup inventoryGridLayout;
        
        [Header("Crafting Queue")]
        [SerializeField] private GameObject craftingQueuePanel;
        [SerializeField] private Transform queueContainer;
        [SerializeField] private GameObject queueItemPrefab;
        [SerializeField] private TextMeshProUGUI queueStatusText;
        [SerializeField] private Button clearQueueButton;
        
        [Header("Search and Filter")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown difficultyFilter;
        [SerializeField] private TMP_Dropdown unlockStatusFilter;
        [SerializeField] private Button resetFiltersButton;
        
        [Header("Player Skills")]
        [SerializeField] private GameObject skillsPanel;
        [SerializeField] private TextMeshProUGUI craftingLevelText;
        [SerializeField] private Slider craftingExperienceBar;
        [SerializeField] private TextMeshProUGUI experienceText;
        [SerializeField] private Button skillTreeButton;
        
        [Header("Recipe Details")]
        [SerializeField] private GameObject recipeDetailsPanel;
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private TextMeshProUGUI recipeDescriptionText;
        [SerializeField] private TextMeshProUGUI craftingTimeText;
        [SerializeField] private TextMeshProUGUI experienceRewardText;
        [SerializeField] private TextMeshProUGUI unlockRequirementsText;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem craftingEffect;
        [SerializeField] private ParticleSystem successEffect;
        [SerializeField] private GameObject sparkleEffect;
        [SerializeField] private Animator craftingStationAnimator;
        
        [Header("Audio")]
        [SerializeField] private AudioClip craftingStartSound;
        [SerializeField] private AudioClip craftingCompleteSound;
        [SerializeField] private AudioClip craftingFailSound;
        [SerializeField] private AudioClip hammerSound;
        [SerializeField] private AudioClip buttonClickSound;
        
        [Header("Animation")]
        [SerializeField] private bool enableCraftingAnimations = true;
        [SerializeField] private float recipeAnimationDelay = 0.04f;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Crafting system
        private List<CraftingRecipe> _allRecipes = new List<CraftingRecipe>();
        private List<RecipeItemUI> _recipeItems = new List<RecipeItemUI>();
        private CraftingCategory _currentCategory = CraftingCategory.All;
        private CraftingRecipe _selectedRecipe;
        private int _craftingQuantity = 1;
        
        // Player data
        private List<ItemData> _playerInventory = new List<ItemData>();
        private List<InventorySlot> _inventorySlots = new List<InventorySlot>();
        private CraftingSkillData _craftingSkill;
        
        // Crafting queue and progress
        private List<CraftingJob> _craftingQueue = new List<CraftingJob>();
        private CraftingJob _currentCraftingJob;
        private bool _isCrafting = false;
        
        // Search and filter
        private string _searchQuery = "";
        private CraftingDifficulty _difficultyFilter = CraftingDifficulty.All;
        private UnlockStatus _unlockFilter = UnlockStatus.All;
        
        // Filter options
        private readonly string[] _difficultyOptions = { "All", "Beginner", "Intermediate", "Advanced", "Master" };
        private readonly string[] _unlockOptions = { "All", "Available", "Locked", "Learned" };

        protected override void Awake()
        {
            base.Awake();
            InitializeCraftingPanel();
        }

        protected override void Start()
        {
            base.Start();
            SetupCraftingPanel();
            LoadCraftingData();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initially hide this panel
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleCraftingInput();
            UpdateCraftingProgress();
            ProcessCraftingQueue();
        }

        private void InitializeCraftingPanel()
        {
            // Create default prefabs if none exist
            if (recipeItemPrefab == null)
            {
                CreateDefaultRecipeItemPrefab();
            }
            
            if (materialSlotPrefab == null)
            {
                CreateDefaultMaterialSlotPrefab();
            }
            
            if (queueItemPrefab == null)
            {
                CreateDefaultQueueItemPrefab();
            }
            
            if (inventorySlotPrefab == null)
            {
                CreateDefaultInventorySlotPrefab();
            }
            
            // Initialize skill data
            _craftingSkill = new CraftingSkillData
            {
                level = 1,
                experience = 0,
                experienceToNext = 100
            };
        }

        private void SetupCraftingPanel()
        {
            // Setup category buttons
            if (allCategoryButton != null)
            {
                allCategoryButton.onClick.AddListener(() => SetCategory(CraftingCategory.All));
                SetCategoryButtonActive(allCategoryButton, true);
            }
            
            if (toolsCategoryButton != null)
                toolsCategoryButton.onClick.AddListener(() => SetCategory(CraftingCategory.Tools));
            
            if (equipmentCategoryButton != null)
                equipmentCategoryButton.onClick.AddListener(() => SetCategory(CraftingCategory.Equipment));
            
            if (consumablesCategoryButton != null)
                consumablesCategoryButton.onClick.AddListener(() => SetCategory(CraftingCategory.Consumables));
            
            if (buildingsCategoryButton != null)
                buildingsCategoryButton.onClick.AddListener(() => SetCategory(CraftingCategory.Buildings));
            
            if (decorationsCategoryButton != null)
                decorationsCategoryButton.onClick.AddListener(() => SetCategory(CraftingCategory.Decorations));
            
            // Setup crafting controls
            if (quantitySlider != null)
            {
                quantitySlider.onValueChanged.AddListener(OnQuantitySliderChanged);
                quantitySlider.minValue = 1;
                quantitySlider.value = 1;
            }
            
            if (quantityInput != null)
                quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);
            
            if (decreaseQuantityButton != null)
                decreaseQuantityButton.onClick.AddListener(DecreaseQuantity);
            
            if (increaseQuantityButton != null)
                increaseQuantityButton.onClick.AddListener(IncreaseQuantity);
            
            if (craftButton != null)
                craftButton.onClick.AddListener(StartCrafting);
            
            if (craftAllButton != null)
                craftAllButton.onClick.AddListener(CraftMaxPossible);
            
            if (queueCraftButton != null)
                queueCraftButton.onClick.AddListener(QueueCrafting);
            
            if (cancelCraftingButton != null)
                cancelCraftingButton.onClick.AddListener(CancelCurrentCrafting);
            
            // Setup search and filters
            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);
            
            if (difficultyFilter != null)
            {
                difficultyFilter.ClearOptions();
                difficultyFilter.AddOptions(_difficultyOptions.ToList());
                difficultyFilter.onValueChanged.AddListener(OnDifficultyFilterChanged);
            }
            
            if (unlockStatusFilter != null)
            {
                unlockStatusFilter.ClearOptions();
                unlockStatusFilter.AddOptions(_unlockOptions.ToList());
                unlockStatusFilter.onValueChanged.AddListener(OnUnlockFilterChanged);
            }
            
            if (resetFiltersButton != null)
                resetFiltersButton.onClick.AddListener(ResetFilters);
            
            // Setup queue management
            if (clearQueueButton != null)
                clearQueueButton.onClick.AddListener(ClearCraftingQueue);
            
            if (skillTreeButton != null)
                skillTreeButton.onClick.AddListener(OpenSkillTree);
            
            // Initialize displays
            HideRecipeDetails();
            HideCraftingProgress();
            UpdateSkillDisplay();
            UpdateQueueDisplay();
        }

        private void CreateDefaultRecipeItemPrefab()
        {
            var recipeObj = new GameObject("RecipeItem");
            recipeObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = recipeObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 100f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = recipeObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Button component
            var button = recipeObj.AddComponent<Button>();
            button.targetGraphic = background;
            
            // Content container
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(recipeObj.transform, false);
            var contentLayout = contentObj.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 15f;
            contentLayout.childControlWidth = false;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandHeight = true;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Recipe icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(contentObj.transform, false);
            var icon = iconObj.AddComponent<Image>();
            icon.color = Color.white;
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(80f, 80f);
            
            // Recipe info container
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(contentObj.transform, false);
            var infoLayout = infoObj.AddComponent<VerticalLayoutGroup>();
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.spacing = 5f;
            
            var infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.sizeDelta = new Vector2(250f, 80f);
            
            // Recipe name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(infoObj.transform, false);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Recipe Name";
            nameText.fontSize = 16f;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;
            
            // Recipe description
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(infoObj.transform, false);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Recipe description...";
            descText.fontSize = 12f;
            descText.color = Color.gray;
            
            // Requirements container
            var reqObj = new GameObject("Requirements");
            reqObj.transform.SetParent(contentObj.transform, false);
            var reqLayout = reqObj.AddComponent<VerticalLayoutGroup>();
            reqLayout.childControlWidth = true;
            reqLayout.childControlHeight = false;
            
            var reqRect = reqObj.GetComponent<RectTransform>();
            reqRect.sizeDelta = new Vector2(120f, 80f);
            
            // Crafting time
            var timeObj = new GameObject("Time");
            timeObj.transform.SetParent(reqObj.transform, false);
            var timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = "Time: 5s";
            timeText.fontSize = 11f;
            timeText.color = Color.cyan;
            
            // Skill requirement
            var skillObj = new GameObject("Skill");
            skillObj.transform.SetParent(reqObj.transform, false);
            var skillText = skillObj.AddComponent<TextMeshProUGUI>();
            skillText.text = "Level: 1";
            skillText.fontSize = 11f;
            skillText.color = Color.yellow;
            
            // Status indicator
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(reqObj.transform, false);
            var statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Available";
            statusText.fontSize = 10f;
            statusText.color = Color.green;
            
            // Add RecipeItemUI component
            var recipeItemUI = recipeObj.AddComponent<RecipeItemUI>();
            
            recipeItemPrefab = recipeObj;
            recipeItemPrefab.SetActive(false);
        }

        private void CreateDefaultMaterialSlotPrefab()
        {
            var materialObj = new GameObject("MaterialSlot");
            materialObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = materialObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60f;
            layoutElement.flexibleWidth = 1f;
            
            // Horizontal layout
            var layout = materialObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 10f;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            
            // Material icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(materialObj.transform, false);
            var icon = iconObj.AddComponent<Image>();
            icon.color = Color.white;
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(50f, 50f);
            
            // Material info
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(materialObj.transform, false);
            var infoLayout = infoObj.AddComponent<VerticalLayoutGroup>();
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            
            // Material name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(infoObj.transform, false);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Material Name";
            nameText.fontSize = 12f;
            nameText.color = Color.white;
            
            // Quantity required
            var quantityObj = new GameObject("Quantity");
            quantityObj.transform.SetParent(infoObj.transform, false);
            var quantityText = quantityObj.AddComponent<TextMeshProUGUI>();
            quantityText.text = "Need: 5 | Have: 3";
            quantityText.fontSize = 10f;
            quantityText.color = Color.yellow;
            
            materialSlotPrefab = materialObj;
            materialSlotPrefab.SetActive(false);
        }

        private void CreateDefaultQueueItemPrefab()
        {
            var queueObj = new GameObject("QueueItem");
            queueObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = queueObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 40f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = queueObj.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Horizontal layout
            var layout = queueObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 10f;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            
            // Queue icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(queueObj.transform, false);
            var icon = iconObj.AddComponent<Image>();
            icon.color = Color.white;
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(30f, 30f);
            
            // Queue info
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(queueObj.transform, false);
            var infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.text = "Item Name x5";
            infoText.fontSize = 12f;
            infoText.color = Color.white;
            
            // Progress bar
            var progressObj = new GameObject("Progress");
            progressObj.transform.SetParent(queueObj.transform, false);
            var progress = progressObj.AddComponent<Slider>();
            progress.value = 0.5f;
            var progressRect = progressObj.GetComponent<RectTransform>();
            progressRect.sizeDelta = new Vector2(100f, 20f);
            
            // Remove button
            var removeObj = new GameObject("Remove");
            removeObj.transform.SetParent(queueObj.transform, false);
            var removeButton = removeObj.AddComponent<Button>();
            var removeText = removeObj.AddComponent<TextMeshProUGUI>();
            removeText.text = "X";
            removeText.fontSize = 12f;
            removeText.color = Color.red;
            removeText.alignment = TextAlignmentOptions.Center;
            var removeRect = removeObj.GetComponent<RectTransform>();
            removeRect.sizeDelta = new Vector2(25f, 25f);
            
            queueItemPrefab = queueObj;
            queueItemPrefab.SetActive(false);
        }

        private void CreateDefaultInventorySlotPrefab()
        {
            var slotObj = new GameObject("InventorySlot");
            var rect = slotObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60f, 60f);
            
            // Background
            var background = slotObj.AddComponent<Image>();
            background.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            
            // Item icon
            var iconObj = new GameObject("ItemIcon");
            iconObj.transform.SetParent(slotObj.transform, false);
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.one * 3f;
            iconRect.offsetMax = Vector2.one * -3f;
            
            var iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            
            // Quantity text
            var quantityObj = new GameObject("Quantity");
            quantityObj.transform.SetParent(slotObj.transform, false);
            var quantityRect = quantityObj.GetComponent<RectTransform>();
            quantityRect.anchorMin = new Vector2(1f, 0f);
            quantityRect.anchorMax = new Vector2(1f, 0f);
            quantityRect.anchoredPosition = new Vector2(-3f, 3f);
            quantityRect.sizeDelta = new Vector2(15f, 12f);
            
            var quantityText = quantityObj.AddComponent<TextMeshProUGUI>();
            quantityText.text = "";
            quantityText.fontSize = 8f;
            quantityText.color = Color.white;
            quantityText.alignment = TextAlignmentOptions.BottomRight;
            
            inventorySlotPrefab = slotObj;
            inventorySlotPrefab.SetActive(false);
        }

        // Input handling
        private void HandleCraftingInput()
        {
            // ESC to close crafting panel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseCraftingPanel();
            }
            
            // Enter key to start crafting
            if (Input.GetKeyDown(KeyCode.Return) && _selectedRecipe != null)
            {
                StartCrafting();
            }
            
            // Space key to cancel current crafting
            if (Input.GetKeyDown(KeyCode.Space) && _isCrafting)
            {
                CancelCurrentCrafting();
            }
            
            // Number keys for quantity selection
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    SetCraftingQuantity(i);
                }
            }
        }

        // Crafting data management
        private void LoadCraftingData()
        {
            // This would normally load from CraftingManager
            CreateMockCraftingData();
            LoadPlayerInventory();
            RefreshRecipeDisplay();
        }

        private void CreateMockCraftingData()
        {
            _allRecipes.Clear();
            
            var sampleRecipes = new CraftingRecipe[]
            {
                new CraftingRecipe
                {
                    id = 1,
                    name = "Wooden Tool Handle",
                    description = "A sturdy handle for basic tools.",
                    category = CraftingCategory.Tools,
                    difficulty = CraftingDifficulty.Beginner,
                    craftingTime = 5f,
                    experienceReward = 10,
                    levelRequired = 1,
                    isUnlocked = true,
                    resultItem = new ItemData { id = 101, name = "Tool Handle", type = ItemType.Material, value = 25 },
                    resultQuantity = 1,
                    materials = new List<CraftingMaterial>
                    {
                        new CraftingMaterial { itemId = 11, itemName = "Wood", quantityRequired = 3 },
                        new CraftingMaterial { itemId = 12, itemName = "Stone", quantityRequired = 1 }
                    }
                },
                new CraftingRecipe
                {
                    id = 2,
                    name = "Basic Fertilizer",
                    description = "Improves soil quality for better crop growth.",
                    category = CraftingCategory.Consumables,
                    difficulty = CraftingDifficulty.Beginner,
                    craftingTime = 3f,
                    experienceReward = 5,
                    levelRequired = 1,
                    isUnlocked = true,
                    resultItem = new ItemData { id = 102, name = "Basic Fertilizer", type = ItemType.Consumable, value = 100 },
                    resultQuantity = 5,
                    materials = new List<CraftingMaterial>
                    {
                        new CraftingMaterial { itemId = 11, itemName = "Wood", quantityRequired = 2 },
                        new CraftingMaterial { itemId = 13, itemName = "Sap", quantityRequired = 1 }
                    }
                },
                new CraftingRecipe
                {
                    id = 3,
                    name = "Copper Watering Can",
                    description = "An upgraded watering can that holds more water.",
                    category = CraftingCategory.Tools,
                    difficulty = CraftingDifficulty.Intermediate,
                    craftingTime = 15f,
                    experienceReward = 50,
                    levelRequired = 3,
                    isUnlocked = true,
                    resultItem = new ItemData { id = 103, name = "Copper Watering Can", type = ItemType.Tool, value = 2000 },
                    resultQuantity = 1,
                    materials = new List<CraftingMaterial>
                    {
                        new CraftingMaterial { itemId = 101, itemName = "Tool Handle", quantityRequired = 1 },
                        new CraftingMaterial { itemId = 14, itemName = "Copper Bar", quantityRequired = 5 },
                        new CraftingMaterial { itemId = 15, itemName = "Coal", quantityRequired = 2 }
                    }
                },
                new CraftingRecipe
                {
                    id = 4,
                    name = "Quality Sprinkler",
                    description = "Automatically waters crops in a 3x3 area.",
                    category = CraftingCategory.Buildings,
                    difficulty = CraftingDifficulty.Advanced,
                    craftingTime = 30f,
                    experienceReward = 100,
                    levelRequired = 6,
                    isUnlocked = false,
                    resultItem = new ItemData { id = 104, name = "Quality Sprinkler", type = ItemType.Tool, value = 5000 },
                    resultQuantity = 1,
                    materials = new List<CraftingMaterial>
                    {
                        new CraftingMaterial { itemId = 16, itemName = "Iron Bar", quantityRequired = 1 },
                        new CraftingMaterial { itemId = 17, itemName = "Gold Bar", quantityRequired = 1 },
                        new CraftingMaterial { itemId = 18, itemName = "Refined Quartz", quantityRequired = 1 }
                    }
                }
            };
            
            foreach (var recipe in sampleRecipes)
            {
                _allRecipes.Add(recipe);
            }
        }

        private void LoadPlayerInventory()
        {
            // This would normally load from InventoryManager
            _playerInventory.Clear();
            
            var sampleInventory = new ItemData[]
            {
                new ItemData { id = 11, name = "Wood", type = ItemType.Material, quantity = 50, value = 2 },
                new ItemData { id = 12, name = "Stone", type = ItemType.Material, quantity = 25, value = 2 },
                new ItemData { id = 13, name = "Sap", type = ItemType.Material, quantity = 10, value = 3 },
                new ItemData { id = 14, name = "Copper Bar", type = ItemType.Material, quantity = 8, value = 15 },
                new ItemData { id = 15, name = "Coal", type = ItemType.Material, quantity = 5, value = 10 },
                new ItemData { id = 101, name = "Tool Handle", type = ItemType.Material, quantity = 2, value = 25 }
            };
            
            foreach (var item in sampleInventory)
            {
                _playerInventory.Add(item);
            }
            
            RefreshInventoryDisplay();
        }

        private void RefreshRecipeDisplay()
        {
            // Filter recipes based on current settings
            var filteredRecipes = FilterRecipes(_allRecipes);
            
            // Clear existing recipes
            ClearRecipeItems();
            
            // Create new recipe displays
            StartCoroutine(CreateRecipeItems(filteredRecipes));
        }

        private List<CraftingRecipe> FilterRecipes(List<CraftingRecipe> recipes)
        {
            var filtered = recipes.AsEnumerable();
            
            // Filter by category
            if (_currentCategory != CraftingCategory.All)
            {
                filtered = filtered.Where(recipe => recipe.category == _currentCategory);
            }
            
            // Filter by search query
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                filtered = filtered.Where(recipe => 
                    recipe.name.ToLower().Contains(_searchQuery.ToLower()) ||
                    recipe.description.ToLower().Contains(_searchQuery.ToLower()));
            }
            
            // Filter by difficulty
            if (_difficultyFilter != CraftingDifficulty.All)
            {
                filtered = filtered.Where(recipe => recipe.difficulty == _difficultyFilter);
            }
            
            // Filter by unlock status
            filtered = _unlockFilter switch
            {
                UnlockStatus.Available => filtered.Where(recipe => recipe.isUnlocked && CanCraftRecipe(recipe)),
                UnlockStatus.Locked => filtered.Where(recipe => !recipe.isUnlocked),
                UnlockStatus.Learned => filtered.Where(recipe => recipe.isUnlocked),
                _ => filtered
            };
            
            // Sort by difficulty then by name
            return filtered.OrderBy(recipe => recipe.difficulty)
                          .ThenBy(recipe => recipe.name)
                          .ToList();
        }

        private IEnumerator CreateRecipeItems(List<CraftingRecipe> recipes)
        {
            for (int i = 0; i < recipes.Count; i++)
            {
                var recipe = recipes[i];
                var itemObj = Instantiate(recipeItemPrefab, recipeListContainer);
                itemObj.SetActive(true);
                
                var recipeItem = itemObj.GetComponent<RecipeItemUI>();
                if (recipeItem != null)
                {
                    recipeItem.Initialize(recipe, this);
                    _recipeItems.Add(recipeItem);
                }
                
                // Animate item in if enabled
                if (enableCraftingAnimations)
                {
                    StartCoroutine(AnimateRecipeItemIn(itemObj.transform, i * recipeAnimationDelay));
                }
                
                yield return null;
            }
        }

        private IEnumerator AnimateRecipeItemIn(Transform itemTransform, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            var startPos = itemTransform.localPosition + Vector3.right * 300f;
            var endPos = itemTransform.localPosition;
            
            itemTransform.localPosition = startPos;
            
            var canvasGroup = itemTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = itemTransform.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                float curveValue = slideInCurve.Evaluate(progress);
                
                itemTransform.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
                canvasGroup.alpha = curveValue;
                
                yield return null;
            }
            
            itemTransform.localPosition = endPos;
            canvasGroup.alpha = 1f;
        }

        private void ClearRecipeItems()
        {
            foreach (var item in _recipeItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            
            _recipeItems.Clear();
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
            
            // Create inventory slots
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

        // Category and filter management
        private void SetCategory(CraftingCategory category)
        {
            _currentCategory = category;
            RefreshRecipeDisplay();
            
            // Update category button states
            SetCategoryButtonActive(allCategoryButton, category == CraftingCategory.All);
            SetCategoryButtonActive(toolsCategoryButton, category == CraftingCategory.Tools);
            SetCategoryButtonActive(equipmentCategoryButton, category == CraftingCategory.Equipment);
            SetCategoryButtonActive(consumablesCategoryButton, category == CraftingCategory.Consumables);
            SetCategoryButtonActive(buildingsCategoryButton, category == CraftingCategory.Buildings);
            SetCategoryButtonActive(decorationsCategoryButton, category == CraftingCategory.Decorations);
        }

        private void SetCategoryButtonActive(Button button, bool active)
        {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = active ? Color.yellow : Color.white;
            button.colors = colors;
        }

        // Search and filter
        private void OnSearchChanged(string searchQuery)
        {
            _searchQuery = searchQuery;
            RefreshRecipeDisplay();
        }

        private void OnDifficultyFilterChanged(int index)
        {
            _difficultyFilter = (CraftingDifficulty)index;
            RefreshRecipeDisplay();
        }

        private void OnUnlockFilterChanged(int index)
        {
            _unlockFilter = (UnlockStatus)index;
            RefreshRecipeDisplay();
        }

        private void ResetFilters()
        {
            _searchQuery = "";
            _difficultyFilter = CraftingDifficulty.All;
            _unlockFilter = UnlockStatus.All;
            
            if (searchField != null) searchField.text = "";
            if (difficultyFilter != null) difficultyFilter.value = 0;
            if (unlockStatusFilter != null) unlockStatusFilter.value = 0;
            
            RefreshRecipeDisplay();
        }

        // Recipe selection and details
        public void SelectRecipe(CraftingRecipe recipe)
        {
            _selectedRecipe = recipe;
            _craftingQuantity = 1;
            
            ShowRecipeDetails(recipe);
            UpdateCraftingControls();
        }

        private void ShowRecipeDetails(CraftingRecipe recipe)
        {
            if (craftingStationPanel == null || recipe == null) return;
            
            craftingStationPanel.SetActive(true);
            
            // Update result item display
            if (resultItemIcon != null)
                resultItemIcon.sprite = recipe.resultItem.icon;
            
            if (resultItemName != null)
                resultItemName.text = recipe.name;
            
            if (resultItemDescription != null)
                resultItemDescription.text = recipe.description;
            
            if (resultQuantityText != null)
                resultQuantityText.text = $"x{recipe.resultQuantity}";
            
            // Update recipe details
            if (recipeDetailsPanel != null)
            {
                recipeDetailsPanel.SetActive(true);
                
                if (recipeNameText != null)
                    recipeNameText.text = recipe.name;
                
                if (recipeDescriptionText != null)
                    recipeDescriptionText.text = recipe.description;
                
                if (craftingTimeText != null)
                    craftingTimeText.text = $"Crafting Time: {recipe.craftingTime:F1}s";
                
                if (experienceRewardText != null)
                    experienceRewardText.text = $"Experience: +{recipe.experienceReward}";
                
                if (unlockRequirementsText != null)
                {
                    if (recipe.isUnlocked)
                    {
                        unlockRequirementsText.text = "✓ Recipe Unlocked";
                        unlockRequirementsText.color = Color.green;
                    }
                    else
                    {
                        unlockRequirementsText.text = $"Requires Level {recipe.levelRequired}";
                        unlockRequirementsText.color = Color.red;
                    }
                }
            }
            
            // Update materials display
            UpdateMaterialsDisplay(recipe);
        }

        private void HideRecipeDetails()
        {
            if (craftingStationPanel != null)
            {
                craftingStationPanel.SetActive(false);
            }
            
            if (recipeDetailsPanel != null)
            {
                recipeDetailsPanel.SetActive(false);
            }
        }

        private void UpdateMaterialsDisplay(CraftingRecipe recipe)
        {
            if (materialsContainer == null) return;
            
            // Clear existing materials
            foreach (Transform child in materialsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create material slots
            foreach (var material in recipe.materials)
            {
                CreateMaterialSlot(material);
            }
        }

        private void CreateMaterialSlot(CraftingMaterial material)
        {
            if (materialSlotPrefab == null) return;
            
            var materialObj = Instantiate(materialSlotPrefab, materialsContainer);
            materialObj.SetActive(true);
            
            // Find player inventory item
            var playerItem = _playerInventory.FirstOrDefault(item => item.id == material.itemId);
            int playerQuantity = playerItem?.quantity ?? 0;
            
            // Update display
            var texts = materialObj.GetComponentsInChildren<TextMeshProUGUI>();
            var images = materialObj.GetComponentsInChildren<Image>();
            
            if (texts.Length >= 2)
            {
                texts[0].text = material.itemName;
                texts[1].text = $"Need: {material.quantityRequired} | Have: {playerQuantity}";
                
                // Color code based on availability
                texts[1].color = playerQuantity >= material.quantityRequired ? Color.green : Color.red;
            }
            
            // Update icon if available
            if (images.Length > 0 && playerItem?.icon != null)
            {
                images[0].sprite = playerItem.icon;
            }
        }

        // Quantity management
        private void OnQuantitySliderChanged(float value)
        {
            _craftingQuantity = Mathf.RoundToInt(value);
            
            if (quantityInput != null)
                quantityInput.text = _craftingQuantity.ToString();
            
            UpdateCraftingControls();
        }

        private void OnQuantityInputChanged(string value)
        {
            if (int.TryParse(value, out int quantity))
            {
                _craftingQuantity = Mathf.Max(1, quantity);
                
                if (quantitySlider != null)
                    quantitySlider.value = _craftingQuantity;
                
                UpdateCraftingControls();
            }
        }

        private void IncreaseQuantity()
        {
            if (_selectedRecipe != null)
            {
                int maxQuantity = CalculateMaxCraftableQuantity(_selectedRecipe);
                if (_craftingQuantity < maxQuantity)
                {
                    _craftingQuantity++;
                    UpdateQuantityDisplay();
                    UpdateCraftingControls();
                }
            }
        }

        private void DecreaseQuantity()
        {
            if (_craftingQuantity > 1)
            {
                _craftingQuantity--;
                UpdateQuantityDisplay();
                UpdateCraftingControls();
            }
        }

        private void SetCraftingQuantity(int quantity)
        {
            if (_selectedRecipe != null)
            {
                int maxQuantity = CalculateMaxCraftableQuantity(_selectedRecipe);
                _craftingQuantity = Mathf.Clamp(quantity, 1, maxQuantity);
                UpdateQuantityDisplay();
                UpdateCraftingControls();
            }
        }

        private void UpdateQuantityDisplay()
        {
            if (quantitySlider != null)
                quantitySlider.value = _craftingQuantity;
            
            if (quantityInput != null)
                quantityInput.text = _craftingQuantity.ToString();
        }

        private void UpdateCraftingControls()
        {
            if (_selectedRecipe == null) return;
            
            bool canCraft = CanCraftRecipe(_selectedRecipe, _craftingQuantity);
            int maxQuantity = CalculateMaxCraftableQuantity(_selectedRecipe);
            
            // Update quantity slider
            if (quantitySlider != null)
            {
                quantitySlider.maxValue = Mathf.Max(1, maxQuantity);
            }
            
            // Update buttons
            if (craftButton != null)
                craftButton.interactable = canCraft && !_isCrafting;
            
            if (craftAllButton != null)
                craftAllButton.interactable = maxQuantity > 0 && !_isCrafting;
            
            if (queueCraftButton != null)
                queueCraftButton.interactable = canCraft;
        }

        // Crafting logic
        public bool CanCraftRecipe(CraftingRecipe recipe, int quantity = 1)
        {
            if (!recipe.isUnlocked) return false;
            if (_craftingSkill.level < recipe.levelRequired) return false;
            
            foreach (var material in recipe.materials)
            {
                var playerItem = _playerInventory.FirstOrDefault(item => item.id == material.itemId);
                int available = playerItem?.quantity ?? 0;
                
                if (available < material.quantityRequired * quantity)
                {
                    return false;
                }
            }
            
            return true;
        }

        private int CalculateMaxCraftableQuantity(CraftingRecipe recipe)
        {
            if (!recipe.isUnlocked || _craftingSkill.level < recipe.levelRequired)
                return 0;
            
            int maxQuantity = int.MaxValue;
            
            foreach (var material in recipe.materials)
            {
                var playerItem = _playerInventory.FirstOrDefault(item => item.id == material.itemId);
                int available = playerItem?.quantity ?? 0;
                int possibleCrafts = available / material.quantityRequired;
                
                maxQuantity = Mathf.Min(maxQuantity, possibleCrafts);
            }
            
            return Mathf.Max(0, maxQuantity == int.MaxValue ? 0 : maxQuantity);
        }

        private void StartCrafting()
        {
            if (_selectedRecipe == null || !CanCraftRecipe(_selectedRecipe, _craftingQuantity))
                return;
            
            PlayButtonClickSound();
            
            // Create crafting job
            var craftingJob = new CraftingJob
            {
                recipe = _selectedRecipe,
                quantity = _craftingQuantity,
                totalTime = _selectedRecipe.craftingTime * _craftingQuantity,
                remainingTime = _selectedRecipe.craftingTime * _craftingQuantity,
                isCurrentJob = true
            };
            
            // Consume materials
            ConsumeMaterials(_selectedRecipe, _craftingQuantity);
            
            // Start crafting
            _currentCraftingJob = craftingJob;
            _isCrafting = true;
            
            ShowCraftingProgress();
            PlayCraftingStartSound();
            
            if (craftingEffect != null)
                craftingEffect.Play();
            
            UpdateCraftingControls();
            RefreshInventoryDisplay();
        }

        private void CraftMaxPossible()
        {
            if (_selectedRecipe == null) return;
            
            int maxQuantity = CalculateMaxCraftableQuantity(_selectedRecipe);
            if (maxQuantity > 0)
            {
                _craftingQuantity = maxQuantity;
                UpdateQuantityDisplay();
                StartCrafting();
            }
        }

        private void QueueCrafting()
        {
            if (_selectedRecipe == null || !CanCraftRecipe(_selectedRecipe, _craftingQuantity))
                return;
            
            PlayButtonClickSound();
            
            // Create crafting job for queue
            var craftingJob = new CraftingJob
            {
                recipe = _selectedRecipe,
                quantity = _craftingQuantity,
                totalTime = _selectedRecipe.craftingTime * _craftingQuantity,
                remainingTime = _selectedRecipe.craftingTime * _craftingQuantity,
                isCurrentJob = false
            };
            
            _craftingQueue.Add(craftingJob);
            UpdateQueueDisplay();
        }

        private void ConsumeMaterials(CraftingRecipe recipe, int quantity)
        {
            foreach (var material in recipe.materials)
            {
                var playerItem = _playerInventory.FirstOrDefault(item => item.id == material.itemId);
                if (playerItem != null)
                {
                    int requiredAmount = material.quantityRequired * quantity;
                    playerItem.quantity -= requiredAmount;
                    
                    if (playerItem.quantity <= 0)
                    {
                        _playerInventory.Remove(playerItem);
                    }
                }
            }
        }

        private void CompleteCrafting(CraftingJob job)
        {
            // Add result items to inventory
            AddItemToInventory(job.recipe.resultItem, job.recipe.resultQuantity * job.quantity);
            
            // Add experience
            int expGained = job.recipe.experienceReward * job.quantity;
            AddCraftingExperience(expGained);
            
            PlayCraftingCompleteSound();
            
            if (successEffect != null)
                successEffect.Play();
            
            // Show completion message
            Debug.Log($"Crafted {job.recipe.name} x{job.quantity}! Gained {expGained} experience.");
        }

        private void AddItemToInventory(ItemData item, int quantity)
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
        }

        private void AddCraftingExperience(int experience)
        {
            _craftingSkill.experience += experience;
            
            // Check for level up
            while (_craftingSkill.experience >= _craftingSkill.experienceToNext)
            {
                _craftingSkill.experience -= _craftingSkill.experienceToNext;
                _craftingSkill.level++;
                _craftingSkill.experienceToNext = CalculateExperienceToNext(_craftingSkill.level);
                
                // Unlock new recipes
                UnlockRecipesForLevel(_craftingSkill.level);
                
                Debug.Log($"Crafting level up! Now level {_craftingSkill.level}");
            }
            
            UpdateSkillDisplay();
        }

        private int CalculateExperienceToNext(int level)
        {
            return 100 + (level - 1) * 50; // Progressive experience requirement
        }

        private void UnlockRecipesForLevel(int level)
        {
            foreach (var recipe in _allRecipes)
            {
                if (!recipe.isUnlocked && recipe.levelRequired <= level)
                {
                    recipe.isUnlocked = true;
                    Debug.Log($"Recipe unlocked: {recipe.name}");
                }
            }
            
            RefreshRecipeDisplay();
        }

        // Crafting progress and queue
        private void UpdateCraftingProgress()
        {
            if (!_isCrafting || _currentCraftingJob == null) return;
            
            _currentCraftingJob.remainingTime -= Time.unscaledDeltaTime;
            
            // Update progress bar
            if (craftingProgressBar != null)
            {
                float progress = 1f - (_currentCraftingJob.remainingTime / _currentCraftingJob.totalTime);
                craftingProgressBar.value = progress;
            }
            
            if (craftingProgressText != null)
            {
                craftingProgressText.text = $"Crafting: {_currentCraftingJob.remainingTime:F1}s";
            }
            
            // Check if crafting is complete
            if (_currentCraftingJob.remainingTime <= 0f)
            {
                CompleteCrafting(_currentCraftingJob);
                _currentCraftingJob = null;
                _isCrafting = false;
                
                HideCraftingProgress();
                UpdateCraftingControls();
                RefreshInventoryDisplay();
                
                if (craftingEffect != null)
                    craftingEffect.Stop();
            }
        }

        private void ProcessCraftingQueue()
        {
            if (!_isCrafting && _craftingQueue.Count > 0)
            {
                // Start next job in queue
                var nextJob = _craftingQueue[0];
                _craftingQueue.RemoveAt(0);
                
                if (CanCraftRecipe(nextJob.recipe, nextJob.quantity))
                {
                    ConsumeMaterials(nextJob.recipe, nextJob.quantity);
                    _currentCraftingJob = nextJob;
                    _currentCraftingJob.isCurrentJob = true;
                    _isCrafting = true;
                    
                    ShowCraftingProgress();
                    PlayCraftingStartSound();
                    
                    if (craftingEffect != null)
                        craftingEffect.Play();
                }
                
                UpdateQueueDisplay();
                RefreshInventoryDisplay();
            }
        }

        private void ShowCraftingProgress()
        {
            if (craftingProgressPanel != null)
            {
                craftingProgressPanel.SetActive(true);
            }
        }

        private void HideCraftingProgress()
        {
            if (craftingProgressPanel != null)
            {
                craftingProgressPanel.SetActive(false);
            }
        }

        private void CancelCurrentCrafting()
        {
            if (!_isCrafting || _currentCraftingJob == null) return;
            
            PlayButtonClickSound();
            
            // Return materials (with some loss)
            ReturnMaterials(_currentCraftingJob.recipe, _currentCraftingJob.quantity, 0.5f);
            
            _currentCraftingJob = null;
            _isCrafting = false;
            
            HideCraftingProgress();
            UpdateCraftingControls();
            RefreshInventoryDisplay();
            
            if (craftingEffect != null)
                craftingEffect.Stop();
        }

        private void ReturnMaterials(CraftingRecipe recipe, int quantity, float returnRate)
        {
            foreach (var material in recipe.materials)
            {
                int returnAmount = Mathf.RoundToInt(material.quantityRequired * quantity * returnRate);
                if (returnAmount > 0)
                {
                    var existingItem = _playerInventory.FirstOrDefault(item => item.id == material.itemId);
                    if (existingItem != null)
                    {
                        existingItem.quantity += returnAmount;
                    }
                    else
                    {
                        var newItem = new ItemData
                        {
                            id = material.itemId,
                            name = material.itemName,
                            quantity = returnAmount,
                            type = ItemType.Material
                        };
                        _playerInventory.Add(newItem);
                    }
                }
            }
        }

        private void UpdateQueueDisplay()
        {
            if (queueContainer == null) return;
            
            // Clear existing queue items
            foreach (Transform child in queueContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create queue item displays
            for (int i = 0; i < _craftingQueue.Count; i++)
            {
                CreateQueueItem(_craftingQueue[i], i);
            }
            
            // Update queue status
            if (queueStatusText != null)
            {
                queueStatusText.text = _craftingQueue.Count > 0 ? 
                    $"Queue: {_craftingQueue.Count} items" : 
                    "Queue: Empty";
            }
            
            // Show/hide queue panel
            if (craftingQueuePanel != null)
            {
                craftingQueuePanel.SetActive(_craftingQueue.Count > 0 || _isCrafting);
            }
        }

        private void CreateQueueItem(CraftingJob job, int index)
        {
            if (queueItemPrefab == null) return;
            
            var queueObj = Instantiate(queueItemPrefab, queueContainer);
            queueObj.SetActive(true);
            
            // Update display
            var texts = queueObj.GetComponentsInChildren<TextMeshProUGUI>();
            var images = queueObj.GetComponentsInChildren<Image>();
            var buttons = queueObj.GetComponentsInChildren<Button>();
            
            if (texts.Length > 0)
            {
                texts[0].text = $"{job.recipe.name} x{job.quantity}";
            }
            
            if (images.Length > 0 && job.recipe.resultItem.icon != null)
            {
                images[0].sprite = job.recipe.resultItem.icon;
            }
            
            // Setup remove button
            if (buttons.Length > 0)
            {
                buttons[0].onClick.AddListener(() => RemoveFromQueue(index));
            }
        }

        private void RemoveFromQueue(int index)
        {
            if (index >= 0 && index < _craftingQueue.Count)
            {
                var job = _craftingQueue[index];
                _craftingQueue.RemoveAt(index);
                
                // Return materials
                ReturnMaterials(job.recipe, job.quantity, 1f);
                
                UpdateQueueDisplay();
                RefreshInventoryDisplay();
            }
        }

        private void ClearCraftingQueue()
        {
            PlayButtonClickSound();
            
            // Return all materials
            foreach (var job in _craftingQueue)
            {
                ReturnMaterials(job.recipe, job.quantity, 1f);
            }
            
            _craftingQueue.Clear();
            UpdateQueueDisplay();
            RefreshInventoryDisplay();
        }

        // UI updates
        private void UpdateSkillDisplay()
        {
            if (craftingLevelText != null)
                craftingLevelText.text = $"Level: {_craftingSkill.level}";
            
            if (craftingExperienceBar != null)
                craftingExperienceBar.value = (float)_craftingSkill.experience / _craftingSkill.experienceToNext;
            
            if (experienceText != null)
                experienceText.text = $"{_craftingSkill.experience}/{_craftingSkill.experienceToNext} XP";
        }

        // Navigation and special features
        private void OpenSkillTree()
        {
            PlayButtonClickSound();
            // Implementation for skill tree
            Debug.Log("Opening crafting skill tree...");
        }

        private void CloseCraftingPanel()
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
        private void PlayCraftingStartSound()
        {
            Debug.Log("Crafting start sound would play here");
        }

        private void PlayCraftingCompleteSound()
        {
            Debug.Log("Crafting complete sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        // Public interface
        public CraftingRecipe SelectedRecipe => _selectedRecipe;
        public bool IsCrafting => _isCrafting;
        public CraftingSkillData CraftingSkill => _craftingSkill;
        public List<CraftingJob> CraftingQueue => _craftingQueue;
    }

    // Helper component for recipe items
    public class RecipeItemUI : MonoBehaviour
    {
        private CraftingRecipe _recipe;
        private CraftingPanel _craftingPanel;
        private Button _button;
        private bool _isSelected = false;
        
        // UI components
        private Image _background;
        private Image _icon;
        private TextMeshProUGUI _nameText;
        private TextMeshProUGUI _descriptionText;
        private TextMeshProUGUI _timeText;
        private TextMeshProUGUI _skillText;
        private TextMeshProUGUI _statusText;

        public void Initialize(CraftingRecipe recipe, CraftingPanel craftingPanel)
        {
            _recipe = recipe;
            _craftingPanel = craftingPanel;
            
            // Get components
            _button = GetComponent<Button>();
            if (_button == null)
                _button = gameObject.AddComponent<Button>();
            
            _background = GetComponent<Image>();
            
            // Find child components
            _icon = transform.Find("Content/Icon")?.GetComponent<Image>();
            _nameText = transform.Find("Content/Info/Name")?.GetComponent<TextMeshProUGUI>();
            _descriptionText = transform.Find("Content/Info/Description")?.GetComponent<TextMeshProUGUI>();
            _timeText = transform.Find("Content/Requirements/Time")?.GetComponent<TextMeshProUGUI>();
            _skillText = transform.Find("Content/Requirements/Skill")?.GetComponent<TextMeshProUGUI>();
            _statusText = transform.Find("Content/Requirements/Status")?.GetComponent<TextMeshProUGUI>();
            
            // Setup button click
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _craftingPanel.SelectRecipe(_recipe));
            
            UpdateDisplay();
        }

                public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_recipe == null) return;
            
            // Update text components
            if (_nameText != null)
                _nameText.text = _recipe.name;
            
            if (_descriptionText != null)
                _descriptionText.text = _recipe.description;
            
            if (_timeText != null)
                _timeText.text = $"Time: {_recipe.craftingTime:F1}s";
            
            if (_skillText != null)
                _skillText.text = $"Level: {_recipe.levelRequired}";
            
            // Update status
            if (_statusText != null)
            {
                if (!_recipe.isUnlocked)
                {
                    _statusText.text = "Locked";
                    _statusText.color = Color.red;
                }
                else if (_craftingPanel.CraftingSkill.level < _recipe.levelRequired)
                {
                    _statusText.text = "Level Too Low";
                    _statusText.color = Color.red;
                }
                else if (_craftingPanel.CanCraftRecipe(_recipe))
                {
                    _statusText.text = "Available";
                    _statusText.color = Color.green;
                }
                else
                {
                    _statusText.text = "Missing Materials";
                    _statusText.color = Color.yellow;
                }
            }
            
            // Update icon
            if (_icon != null && _recipe.resultItem.icon != null)
                _icon.sprite = _recipe.resultItem.icon;
            
            // Update background color based on state
            if (_background != null)
            {
                Color backgroundColor;
                
                if (_isSelected)
                {
                    backgroundColor = Color.yellow;
                }
                else if (!_recipe.isUnlocked)
                {
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                }
                else if (_craftingPanel.CanCraftRecipe(_recipe))
                {
                    backgroundColor = new Color(0.2f, 0.4f, 0.2f, 0.9f);
                }
                else
                {
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
                }
                
                _background.color = backgroundColor;
            }
            
            // Enable/disable button
            if (_button != null)
            {
                _button.interactable = _recipe.isUnlocked;
            }
        }

        public CraftingRecipe Recipe => _recipe;
    }

    // Data structures and enums
    [System.Serializable]
    public class CraftingRecipe
    {
        public int id;
        public string name;
        public string description;
        public CraftingCategory category;
        public CraftingDifficulty difficulty;
        public float craftingTime;
        public int experienceReward;
        public int levelRequired;
        public bool isUnlocked;
        public ItemData resultItem;
        public int resultQuantity;
        public List<CraftingMaterial> materials = new List<CraftingMaterial>();
        public Sprite icon;
        public List<string> unlockConditions = new List<string>();
    }

    [System.Serializable]
    public class CraftingMaterial
    {
        public int itemId;
        public string itemName;
        public int quantityRequired;
        public Sprite icon;
    }

    [System.Serializable]
    public class CraftingJob
    {
        public CraftingRecipe recipe;
        public int quantity;
        public float totalTime;
        public float remainingTime;
        public bool isCurrentJob;
        public System.DateTime startTime;
    }

    [System.Serializable]
    public class CraftingSkillData
    {
        public int level;
        public int experience;
        public int experienceToNext;
        public List<string> unlockedRecipes = new List<string>();
        public List<string> specializations = new List<string>();
    }

    public enum CraftingCategory
    {
        All,
        Tools,
        Equipment,
        Consumables,
        Buildings,
        Decorations
    }

    public enum CraftingDifficulty
    {
        All = 0,
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Master = 4
    }

    public enum UnlockStatus
    {
        All,
        Available,
        Locked,
        Learned
    }
}