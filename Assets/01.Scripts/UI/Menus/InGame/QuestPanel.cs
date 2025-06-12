using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame
{
    public class QuestPanel : UIPanel
    {
        [Header("Quest Categories")]
        [SerializeField] private Transform categoryContainer;
        [SerializeField] private Button activeQuestsButton;
        [SerializeField] private Button completedQuestsButton;
        [SerializeField] private Button failedQuestsButton;
        [SerializeField] private Button allQuestsButton;
        
        [Header("Quest List")]
        [SerializeField] private Transform questListContainer;
        [SerializeField] private GameObject questItemPrefab;
        [SerializeField] private ScrollRect questListScrollRect;
        [SerializeField] private int maxVisibleQuests = 10;
        
        [Header("Quest Details")]
        [SerializeField] private GameObject questDetailsPanel;
        [SerializeField] private TextMeshProUGUI questTitleText;
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private TextMeshProUGUI questGiverText;
        [SerializeField] private TextMeshProUGUI questStatusText;
        [SerializeField] private TextMeshProUGUI questExperienceText;
        [SerializeField] private TextMeshProUGUI questRewardText;
        [SerializeField] private Image questIcon;
        
        [Header("Quest Objectives")]
        [SerializeField] private Transform objectivesContainer;
        [SerializeField] private GameObject objectiveItemPrefab;
        [SerializeField] private ScrollRect objectivesScrollRect;
        
        [Header("Quest Progress")]
        [SerializeField] private GameObject progressContainer;
        [SerializeField] private Slider overallProgressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI timeRemainingText;
        
        [Header("Action Buttons")]
        [SerializeField] private Button trackQuestButton;
        [SerializeField] private Button abandonQuestButton;
        [SerializeField] private Button claimRewardButton;
        [SerializeField] private Button closeButton;
        
        [Header("Search and Filter")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown difficultyFilter;
        [SerializeField] private TMP_Dropdown typeFilter;
        [SerializeField] private Button resetFiltersButton;
        
        [Header("Quest Statistics")]
        [SerializeField] private GameObject statsContainer;
        [SerializeField] private TextMeshProUGUI totalQuestsText;
        [SerializeField] private TextMeshProUGUI completedQuestsText;
        [SerializeField] private TextMeshProUGUI activeQuestsText;
        [SerializeField] private TextMeshProUGUI completionRateText;
        
        [Header("Notification")]
        [SerializeField] private GameObject questNotificationPrefab;
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private float notificationDuration = 3f;
        
        [Header("Visual Settings")]
        [SerializeField] public Color activeQuestColor = Color.white;
        [SerializeField] public Color completedQuestColor = Color.green;
        [SerializeField] public Color failedQuestColor = Color.red;
        [SerializeField] public Color trackedQuestColor = Color.yellow;
        
        [Header("Animation")]
        [SerializeField] private bool enableQuestAnimations = true;
        [SerializeField] private float questItemAnimationDelay = 0.05f;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip questAcceptSound;
        [SerializeField] private AudioClip questCompleteSound;
        [SerializeField] private AudioClip questFailSound;
        [SerializeField] private AudioClip buttonClickSound;
        
        // Quest management
        private List<QuestData> _allQuests = new List<QuestData>();
        private List<QuestItemUI> _questItems = new List<QuestItemUI>();
        private QuestCategory _currentCategory = QuestCategory.Active;
        private QuestData _selectedQuest;
        private QuestItemUI _selectedQuestItem;
        
        // Filtering and search
        private string _searchQuery = "";
        private QuestDifficulty _difficultyFilter = QuestDifficulty.All;
        private QuestType _typeFilter = QuestType.All;
        
        // Tracked quests (shown in HUD)
        private List<QuestData> _trackedQuests = new List<QuestData>();
        private int _maxTrackedQuests = 3;
        
        // Quest statistics
        private QuestStatistics _questStats;
        
        // Filter options
        private readonly string[] _difficultyOptions = { "All", "Easy", "Normal", "Hard", "Expert" };
        private readonly string[] _typeOptions = { "All", "Main Story", "Side Quest", "Daily", "Repeatable", "Collection", "Delivery" };

        protected override void Awake()
        {
            base.Awake();
            InitializeQuestPanel();
        }

        protected override void Start()
        {
            base.Start();
            SetupQuestPanel();
            LoadQuestData();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initially hide this panel
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleQuestInput();
            UpdateQuestTimers();
        }

        private void InitializeQuestPanel()
        {
            // Create default prefabs if none exist
            if (questItemPrefab == null)
            {
                CreateDefaultQuestItemPrefab();
            }
            
            if (objectiveItemPrefab == null)
            {
                CreateDefaultObjectiveItemPrefab();
            }
            
            if (questNotificationPrefab == null)
            {
                CreateDefaultNotificationPrefab();
            }
            
            // Initialize quest statistics
            _questStats = new QuestStatistics();
        }

        private void SetupQuestPanel()
        {
            // Setup category buttons
            if (activeQuestsButton != null)
            {
                activeQuestsButton.onClick.AddListener(() => SetCategory(QuestCategory.Active));
                SetCategoryButtonActive(activeQuestsButton, true);
            }
            
            if (completedQuestsButton != null)
                completedQuestsButton.onClick.AddListener(() => SetCategory(QuestCategory.Completed));
            
            if (failedQuestsButton != null)
                failedQuestsButton.onClick.AddListener(() => SetCategory(QuestCategory.Failed));
            
            if (allQuestsButton != null)
                allQuestsButton.onClick.AddListener(() => SetCategory(QuestCategory.All));
            
            // Setup action buttons
            if (trackQuestButton != null)
            {
                trackQuestButton.onClick.AddListener(ToggleTrackQuest);
                trackQuestButton.gameObject.SetActive(false);
            }
            
            if (abandonQuestButton != null)
            {
                abandonQuestButton.onClick.AddListener(AbandonSelectedQuest);
                abandonQuestButton.gameObject.SetActive(false);
            }
            
            if (claimRewardButton != null)
            {
                claimRewardButton.onClick.AddListener(ClaimQuestReward);
                claimRewardButton.gameObject.SetActive(false);
            }
            
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseQuestPanel);
            
            // Setup search and filters
            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);
            
            if (difficultyFilter != null)
            {
                difficultyFilter.ClearOptions();
                difficultyFilter.AddOptions(_difficultyOptions.ToList());
                difficultyFilter.onValueChanged.AddListener(OnDifficultyFilterChanged);
            }
            
            if (typeFilter != null)
            {
                typeFilter.ClearOptions();
                typeFilter.AddOptions(_typeOptions.ToList());
                typeFilter.onValueChanged.AddListener(OnTypeFilterChanged);
            }
            
            if (resetFiltersButton != null)
                resetFiltersButton.onClick.AddListener(ResetFilters);
            
            // Hide details panel initially
            HideQuestDetails();
            UpdateQuestStatistics();
        }

        private void CreateDefaultQuestItemPrefab()
        {
            var questObj = new GameObject("QuestItem");
            questObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = questObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = questObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Button component
            var button = questObj.AddComponent<Button>();
            button.targetGraphic = background;
            
            // Content container
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(questObj.transform, false);
            var contentLayout = contentObj.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 15f;
            contentLayout.childControlWidth = false;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = true;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Quest icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(contentObj.transform, false);
            var icon = iconObj.AddComponent<Image>();
            icon.color = Color.white;
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(60f, 60f);
            
            // Quest info container
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(contentObj.transform, false);
            var infoLayout = infoObj.AddComponent<VerticalLayoutGroup>();
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            
            var infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.sizeDelta = new Vector2(300f, 60f);
            
            // Quest title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(infoObj.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Quest Title";
            titleText.fontSize = 16f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            
            // Quest description
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(infoObj.transform, false);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Quest description goes here...";
            descText.fontSize = 12f;
            descText.color = Color.gray;
            descText.overflowMode = TextOverflowModes.Ellipsis;
            
            // Progress container
            var progressObj = new GameObject("Progress");
            progressObj.transform.SetParent(contentObj.transform, false);
            var progressLayout = progressObj.AddComponent<VerticalLayoutGroup>();
            progressLayout.childControlWidth = true;
            progressLayout.childControlHeight = false;
            
            var progressRect = progressObj.GetComponent<RectTransform>();
            progressRect.sizeDelta = new Vector2(100f, 60f);
            
            // Progress bar
            var progressBarObj = new GameObject("ProgressBar");
            progressBarObj.transform.SetParent(progressObj.transform, false);
            var progressBar = progressBarObj.AddComponent<Slider>();
            progressBar.value = 0.5f;
            
            // Progress text
            var progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(progressObj.transform, false);
            var progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "50%";
            progressText.fontSize = 10f;
            progressText.color = Color.white;
            progressText.alignment = TextAlignmentOptions.Center;
            
            // Add QuestItemUI component
            var questItemUI = questObj.AddComponent<QuestItemUI>();
            
            questItemPrefab = questObj;
            questItemPrefab.SetActive(false);
        }

        private void CreateDefaultObjectiveItemPrefab()
        {
            var objObj = new GameObject("ObjectiveItem");
            objObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = objObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30f;
            layoutElement.flexibleWidth = 1f;
            
            // Horizontal layout
            var layout = objObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.spacing = 10f;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            
            // Checkbox
            var checkboxObj = new GameObject("Checkbox");
            checkboxObj.transform.SetParent(objObj.transform, false);
            var checkbox = checkboxObj.AddComponent<Image>();
            checkbox.color = Color.gray;
            var checkboxRect = checkboxObj.GetComponent<RectTransform>();
            checkboxRect.sizeDelta = new Vector2(20f, 20f);
            
            // Objective text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(objObj.transform, false);
            var objectiveText = textObj.AddComponent<TextMeshProUGUI>();
            objectiveText.text = "Complete objective";
            objectiveText.fontSize = 12f;
            objectiveText.color = Color.white;
            
            // Progress indicator
            var progressObj = new GameObject("Progress");
            progressObj.transform.SetParent(objObj.transform, false);
            var progressText = progressObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0/10";
            progressText.fontSize = 11f;
            progressText.color = Color.yellow;
            progressText.alignment = TextAlignmentOptions.Right;
            var progressRect = progressObj.GetComponent<RectTransform>();
            progressRect.sizeDelta = new Vector2(50f, 20f);
            
            objectiveItemPrefab = objObj;
            objectiveItemPrefab.SetActive(false);
        }

        private void CreateDefaultNotificationPrefab()
        {
            var notifObj = new GameObject("QuestNotification");
            notifObj.AddComponent<RectTransform>();
            
            // Background
            var background = notifObj.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.8f);
            
            // Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(notifObj.transform, false);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.one * 10f;
            textRect.offsetMax = Vector2.one * -10f;
            
            var notifText = textObj.AddComponent<TextMeshProUGUI>();
            notifText.text = "Quest notification";
            notifText.fontSize = 14f;
            notifText.color = Color.white;
            notifText.alignment = TextAlignmentOptions.Center;
            
            questNotificationPrefab = notifObj;
            questNotificationPrefab.SetActive(false);
        }

        // Input handling
        private void HandleQuestInput()
        {
            // ESC to close quest panel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseQuestPanel();
            }
            
            // Number keys to track/untrack quests
            for (int i = 1; i <= 3; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    ToggleTrackQuestByIndex(i - 1);
                }
            }
        }

        private void UpdateQuestTimers()
        {
            // Update time remaining for timed quests
            foreach (var quest in _allQuests)
            {
                if (quest.isTimeLimited && quest.status == QuestStatus.Active)
                {
                    quest.timeRemaining -= Time.unscaledDeltaTime;
                    
                    if (quest.timeRemaining <= 0f)
                    {
                        FailQuest(quest);
                    }
                }
            }
            
            // Update selected quest display
            if (_selectedQuest != null && _selectedQuest.isTimeLimited)
            {
                UpdateTimeRemainingDisplay();
            }
        }

        // Quest data management
        private void LoadQuestData()
        {
            // This would normally load from QuestManager
            CreateMockQuestData();
            RefreshQuestDisplay();
        }

        private void CreateMockQuestData()
        {
            _allQuests.Clear();
            
            var sampleQuests = new QuestData[]
            {
                new QuestData
                {
                    id = 1,
                    title = "Welcome to Farming",
                    description = "Learn the basics of farming by planting your first crops.",
                    questGiver = "Mayor Lewis",
                    type = QuestType.MainStory,
                    difficulty = QuestDifficulty.Easy,
                    status = QuestStatus.Active,
                    experienceReward = 100,
                    moneyReward = 250,
                    isTimeLimited = false,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Plant 5 parsnip seeds", currentProgress = 3, targetProgress = 5, isCompleted = false },
                        new QuestObjective { description = "Water crops for 3 days", currentProgress = 1, targetProgress = 3, isCompleted = false }
                    }
                },
                new QuestData
                {
                    id = 2,
                    title = "Community Center Bundle",
                    description = "Collect items for the Spring Bundle at the Community Center.",
                    questGiver = "Junimos",
                    type = QuestType.Collection,
                    difficulty = QuestDifficulty.Normal,
                    status = QuestStatus.Active,
                    experienceReward = 200,
                    moneyReward = 500,
                    isTimeLimited = false,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Collect 5 Spring Onions", currentProgress = 5, targetProgress = 5, isCompleted = true },
                        new QuestObjective { description = "Collect 1 Leek", currentProgress = 0, targetProgress = 1, isCompleted = false },
                        new QuestObjective { description = "Collect 1 Dandelion", currentProgress = 1, targetProgress = 1, isCompleted = true }
                    }
                },
                new QuestData
                {
                    id = 3,
                    title = "Delivery for Pierre",
                    description = "Deliver fresh vegetables to Pierre's General Store.",
                    questGiver = "Pierre",
                    type = QuestType.Delivery,
                    difficulty = QuestDifficulty.Easy,
                    status = QuestStatus.Completed,
                    experienceReward = 50,
                    moneyReward = 150,
                    isTimeLimited = false,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Deliver 10 Parsnips", currentProgress = 10, targetProgress = 10, isCompleted = true }
                    }
                },
                new QuestData
                {
                    id = 4,
                    title = "Daily: Feed the Chickens",
                    description = "Make sure all chickens are fed today.",
                    questGiver = "Marnie",
                    type = QuestType.Daily,
                    difficulty = QuestDifficulty.Easy,
                    status = QuestStatus.Active,
                    experienceReward = 25,
                    moneyReward = 100,
                    isTimeLimited = true,
                    timeRemaining = 14400f, // 4 hours
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Feed 6 chickens", currentProgress = 4, targetProgress = 6, isCompleted = false }
                    }
                }
            };
            
            foreach (var quest in sampleQuests)
            {
                _allQuests.Add(quest);
                
                // Add some quests to tracked list
                if (quest.status == QuestStatus.Active && _trackedQuests.Count < _maxTrackedQuests)
                {
                    _trackedQuests.Add(quest);
                }
            }
            
            UpdateQuestStatistics();
        }

        private void RefreshQuestDisplay()
        {
            // Filter quests based on current category and search
            var filteredQuests = FilterQuests(_allQuests);
            
            // Clear existing quest items
            ClearQuestItems();
            
            // Create quest items
            StartCoroutine(CreateQuestItems(filteredQuests));
            
            // Update statistics
            UpdateQuestStatistics();
        }

        private List<QuestData> FilterQuests(List<QuestData> quests)
        {
            var filtered = quests.AsEnumerable();
            
            // Filter by category
            filtered = _currentCategory switch
            {
                QuestCategory.Active => filtered.Where(q => q.status == QuestStatus.Active),
                QuestCategory.Completed => filtered.Where(q => q.status == QuestStatus.Completed),
                QuestCategory.Failed => filtered.Where(q => q.status == QuestStatus.Failed),
                QuestCategory.All => filtered,
                _ => filtered
            };
            
            // Filter by search query
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                filtered = filtered.Where(q => 
                    q.title.ToLower().Contains(_searchQuery.ToLower()) ||
                    q.description.ToLower().Contains(_searchQuery.ToLower()) ||
                    q.questGiver.ToLower().Contains(_searchQuery.ToLower()));
            }
            
            // Filter by difficulty
            if (_difficultyFilter != QuestDifficulty.All)
            {
                filtered = filtered.Where(q => q.difficulty == _difficultyFilter);
            }
            
            // Filter by type
            if (_typeFilter != QuestType.All)
            {
                filtered = filtered.Where(q => q.type == _typeFilter);
            }
            
            // Sort by priority (tracked quests first, then by status)
            return filtered.OrderBy(q => !_trackedQuests.Contains(q))
                          .ThenBy(q => q.status)
                          .ThenBy(q => q.difficulty)
                          .ToList();
        }

        private IEnumerator CreateQuestItems(List<QuestData> quests)
        {
            for (int i = 0; i < quests.Count; i++)
            {
                var quest = quests[i];
                var itemObj = Instantiate(questItemPrefab, questListContainer);
                itemObj.SetActive(true);
                
                var questItem = itemObj.GetComponent<QuestItemUI>();
                if (questItem != null)
                {
                    questItem.Initialize(quest, this);
                    _questItems.Add(questItem);
                }
                
                // Animate item in if enabled
                if (enableQuestAnimations)
                {
                    StartCoroutine(AnimateQuestItemIn(itemObj.transform, i * questItemAnimationDelay));
                }
                
                yield return null;
            }
        }

        private IEnumerator AnimateQuestItemIn(Transform itemTransform, float delay)
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

        private void ClearQuestItems()
        {
            foreach (var item in _questItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            
            _questItems.Clear();
            _selectedQuest = null;
            _selectedQuestItem = null;
            
            HideQuestDetails();
        }

        // Category management
        private void SetCategory(QuestCategory category)
        {
            _currentCategory = category;
            RefreshQuestDisplay();
            
            // Update category button states
            SetCategoryButtonActive(activeQuestsButton, category == QuestCategory.Active);
            SetCategoryButtonActive(completedQuestsButton, category == QuestCategory.Completed);
            SetCategoryButtonActive(failedQuestsButton, category == QuestCategory.Failed);
            SetCategoryButtonActive(allQuestsButton, category == QuestCategory.All);
        }

        private void SetCategoryButtonActive(Button button, bool active)
        {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = active ? Color.yellow : Color.white;
            button.colors = colors;
        }

        // Search and filtering
        private void OnSearchChanged(string searchQuery)
        {
            _searchQuery = searchQuery;
            RefreshQuestDisplay();
        }

        private void OnDifficultyFilterChanged(int index)
        {
            _difficultyFilter = (QuestDifficulty)index;
            RefreshQuestDisplay();
        }

        private void OnTypeFilterChanged(int index)
        {
            _typeFilter = (QuestType)index;
            RefreshQuestDisplay();
        }

        private void ResetFilters()
        {
            _searchQuery = "";
            _difficultyFilter = QuestDifficulty.All;
            _typeFilter = QuestType.All;
            
            if (searchField != null) searchField.text = "";
            if (difficultyFilter != null) difficultyFilter.value = 0;
            if (typeFilter != null) typeFilter.value = 0;
            
            RefreshQuestDisplay();
        }

        // Quest selection and details
        public void SelectQuest(QuestData quest, QuestItemUI questItem)
        {
            // Deselect previous quest
            if (_selectedQuestItem != null)
            {
                _selectedQuestItem.SetSelected(false);
            }
            
            // Select new quest
            _selectedQuest = quest;
            _selectedQuestItem = questItem;
            
            if (_selectedQuestItem != null)
            {
                _selectedQuestItem.SetSelected(true);
                ShowQuestDetails(quest);
            }
            else
            {
                HideQuestDetails();
            }
        }

        private void ShowQuestDetails(QuestData quest)
        {
            if (questDetailsPanel == null || quest == null) return;
            
            questDetailsPanel.SetActive(true);
            
            // Update quest info
            if (questTitleText != null)
                questTitleText.text = quest.title;
            
            if (questDescriptionText != null)
                questDescriptionText.text = quest.description;
            
            if (questGiverText != null)
                questGiverText.text = $"Quest Giver: {quest.questGiver}";
            
            if (questStatusText != null)
            {
                questStatusText.text = $"Status: {quest.status}";
                questStatusText.color = GetStatusColor(quest.status);
            }
            
            if (questExperienceText != null)
                questExperienceText.text = $"Experience: {quest.experienceReward} XP";
            
            if (questRewardText != null)
                questRewardText.text = $"Reward: ${quest.moneyReward}";
            
            if (questIcon != null)
                questIcon.sprite = quest.icon;
            
            // Update objectives
            UpdateObjectivesDisplay(quest);
            
            // Update progress
            UpdateProgressDisplay(quest);
            
            // Update time remaining
            if (quest.isTimeLimited)
            {
                UpdateTimeRemainingDisplay();
            }
            else if (timeRemainingText != null)
            {
                timeRemainingText.gameObject.SetActive(false);
            }
            
            // Update action buttons
            UpdateActionButtons(quest);
        }

        private void HideQuestDetails()
        {
            if (questDetailsPanel != null)
            {
                questDetailsPanel.SetActive(false);
            }
            
            // Hide action buttons
            if (trackQuestButton != null) trackQuestButton.gameObject.SetActive(false);
            if (abandonQuestButton != null) abandonQuestButton.gameObject.SetActive(false);
            if (claimRewardButton != null) claimRewardButton.gameObject.SetActive(false);
        }

        private void UpdateObjectivesDisplay(QuestData quest)
        {
            if (objectivesContainer == null) return;
            
            // Clear existing objectives
            foreach (Transform child in objectivesContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create objective items
            foreach (var objective in quest.objectives)
            {
                CreateObjectiveItem(objective);
            }
        }

        private void CreateObjectiveItem(QuestObjective objective)
        {
            if (objectiveItemPrefab == null) return;
            
            var objItem = Instantiate(objectiveItemPrefab, objectivesContainer);
            objItem.SetActive(true);
            
            var texts = objItem.GetComponentsInChildren<TextMeshProUGUI>();
            var images = objItem.GetComponentsInChildren<Image>();
            
            // Update objective text
            if (texts.Length > 0)
            {
                texts[0].text = objective.description;
                texts[0].color = objective.isCompleted ? Color.gray : Color.white;
                
                // Strike through if completed
                if (objective.isCompleted)
                {
                    texts[0].fontStyle |= FontStyles.Strikethrough;
                }
            }
            
            // Update progress text
            if (texts.Length > 1)
            {
                texts[1].text = $"{objective.currentProgress}/{objective.targetProgress}";
                texts[1].color = objective.isCompleted ? Color.green : Color.yellow;
            }
            
            // Update checkbox
            if (images.Length > 0)
            {
                images[0].color = objective.isCompleted ? Color.green : Color.gray;
            }
        }

        private void UpdateProgressDisplay(QuestData quest)
        {
            if (progressContainer == null) return;
            
            progressContainer.SetActive(true);
            
            // Calculate overall progress
            float totalProgress = 0f;
            if (quest.objectives.Count > 0)
            {
                foreach (var objective in quest.objectives)
                {
                    float objProgress = objective.targetProgress > 0 ? 
                        (float)objective.currentProgress / objective.targetProgress : 0f;
                    totalProgress += objProgress;
                }
                totalProgress /= quest.objectives.Count;
            }
            
            // Update progress bar
            if (overallProgressBar != null)
                overallProgressBar.value = totalProgress;
            
            // Update progress text
            if (progressText != null)
                progressText.text = $"{totalProgress * 100f:F0}% Complete";
        }

        private void UpdateTimeRemainingDisplay()
        {
            if (_selectedQuest == null || !_selectedQuest.isTimeLimited || timeRemainingText == null) return;
            
            timeRemainingText.gameObject.SetActive(true);
            
            float timeRemaining = _selectedQuest.timeRemaining;
            if (timeRemaining > 0f)
            {
                int hours = Mathf.FloorToInt(timeRemaining / 3600f);
                int minutes = Mathf.FloorToInt((timeRemaining % 3600f) / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                
                timeRemainingText.text = $"Time Remaining: {hours:D2}:{minutes:D2}:{seconds:D2}";
                
                // Change color based on time remaining
                if (timeRemaining < 3600f) // Less than 1 hour
                    timeRemainingText.color = Color.red;
                else if (timeRemaining < 7200f) // Less than 2 hours
                    timeRemainingText.color = Color.yellow;
                else
                    timeRemainingText.color = Color.white;
            }
            else
            {
                timeRemainingText.text = "Time Expired";
                timeRemainingText.color = Color.red;
            }
        }

        private void UpdateActionButtons(QuestData quest)
        {
            if (quest == null) return;
            
            // Track/Untrack button
            if (trackQuestButton != null)
            {
                bool isTracked = _trackedQuests.Contains(quest);
                bool canTrack = quest.status == QuestStatus.Active;
                
                trackQuestButton.gameObject.SetActive(canTrack);
                
                if (canTrack)
                {
                    var buttonText = trackQuestButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = isTracked ? "Untrack" : "Track";
                    }
                }
            }
            
            // Abandon button
            if (abandonQuestButton != null)
            {
                bool canAbandon = quest.status == QuestStatus.Active && quest.type != QuestType.MainStory;
                abandonQuestButton.gameObject.SetActive(canAbandon);
            }
            
            // Claim reward button
            if (claimRewardButton != null)
            {
                bool canClaim = quest.status == QuestStatus.Completed && !quest.rewardClaimed;
                claimRewardButton.gameObject.SetActive(canClaim);
            }
        }

        private Color GetStatusColor(QuestStatus status)
        {
            return status switch
            {
                QuestStatus.Active => activeQuestColor,
                QuestStatus.Completed => completedQuestColor,
                QuestStatus.Failed => failedQuestColor,
                _ => Color.white
            };
        }

        // Quest actions
        private void ToggleTrackQuest()
        {
            if (_selectedQuest == null) return;
            
            PlayButtonClickSound();
            
            if (_trackedQuests.Contains(_selectedQuest))
            {
                UntrackQuest(_selectedQuest);
            }
            else
            {
                TrackQuest(_selectedQuest);
            }
            
            UpdateActionButtons(_selectedQuest);
            RefreshQuestDisplay();
        }

        private void TrackQuest(QuestData quest)
        {
            if (_trackedQuests.Count >= _maxTrackedQuests)
            {
                ShowNotification($"Maximum {_maxTrackedQuests} quests can be tracked!");
                return;
            }
            
            if (!_trackedQuests.Contains(quest))
            {
                _trackedQuests.Add(quest);
                ShowNotification($"Now tracking: {quest.title}");
            }
        }

        private void UntrackQuest(QuestData quest)
        {
            if (_trackedQuests.Remove(quest))
            {
                ShowNotification($"Stopped tracking: {quest.title}");
            }
        }

        private void ToggleTrackQuestByIndex(int index)
        {
            if (index >= 0 && index < _trackedQuests.Count)
            {
                UntrackQuest(_trackedQuests[index]);
            }
        }

        private void AbandonSelectedQuest()
        {
            if (_selectedQuest == null) return;
            
            PlayButtonClickSound();
            
            // Show confirmation dialog
            // For now, directly abandon
            AbandonQuest(_selectedQuest);
        }

        private void AbandonQuest(QuestData quest)
        {
            quest.status = QuestStatus.Failed;
            _trackedQuests.Remove(quest);
            
            ShowNotification($"Abandoned quest: {quest.title}");
            PlayQuestFailSound();
            
            RefreshQuestDisplay();
            UpdateQuestStatistics();
        }

        private void ClaimQuestReward()
        {
            if (_selectedQuest == null || _selectedQuest.status != QuestStatus.Completed) return;
            
            PlayButtonClickSound();
            
            // Claim rewards
            _selectedQuest.rewardClaimed = true;
            
            // This would add rewards to player inventory/stats
            Debug.Log($"Claimed reward: {_selectedQuest.experienceReward} XP, ${_selectedQuest.moneyReward}");
            
            ShowNotification($"Claimed reward for: {_selectedQuest.title}");
            PlayQuestCompleteSound();
            
            UpdateActionButtons(_selectedQuest);
        }

        private void FailQuest(QuestData quest)
        {
            quest.status = QuestStatus.Failed;
            _trackedQuests.Remove(quest);
            
            ShowNotification($"Quest failed: {quest.title}");
            PlayQuestFailSound();
            
            RefreshQuestDisplay();
            UpdateQuestStatistics();
        }

        // Statistics
        private void UpdateQuestStatistics()
        {
            _questStats.totalQuests = _allQuests.Count;
            _questStats.activeQuests = _allQuests.Count(q => q.status == QuestStatus.Active);
            _questStats.completedQuests = _allQuests.Count(q => q.status == QuestStatus.Completed);
            _questStats.failedQuests = _allQuests.Count(q => q.status == QuestStatus.Failed);
            
            _questStats.completionRate = _questStats.totalQuests > 0 ? 
                (float)_questStats.completedQuests / _questStats.totalQuests * 100f : 0f;
            
            // Update UI
            if (totalQuestsText != null)
                totalQuestsText.text = $"Total: {_questStats.totalQuests}";
            
            if (activeQuestsText != null)
                activeQuestsText.text = $"Active: {_questStats.activeQuests}";
            
            if (completedQuestsText != null)
                completedQuestsText.text = $"Completed: {_questStats.completedQuests}";
            
            if (completionRateText != null)
                completionRateText.text = $"Completion: {_questStats.completionRate:F1}%";
        }

        // Notifications
        private void ShowNotification(string message)
        {
            if (questNotificationPrefab == null || notificationContainer == null) return;
            
            var notification = Instantiate(questNotificationPrefab, notificationContainer);
            notification.SetActive(true);
            
            var notifText = notification.GetComponentInChildren<TextMeshProUGUI>();
            if (notifText != null)
            {
                notifText.text = message;
            }
            
            // Auto-destroy after duration
            StartCoroutine(DestroyNotificationAfterDelay(notification, notificationDuration));
        }

        private IEnumerator DestroyNotificationAfterDelay(GameObject notification, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            if (notification != null)
            {
                Destroy(notification);
            }
        }

        // Navigation
        private void CloseQuestPanel()
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
        private void PlayQuestAcceptSound()
        {
            Debug.Log("Quest accept sound would play here");
        }

        private void PlayQuestCompleteSound()
        {
            Debug.Log("Quest complete sound would play here");
        }

        private void PlayQuestFailSound()
        {
            Debug.Log("Quest fail sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        // Public interface
        public List<QuestData> TrackedQuests => _trackedQuests;
        public QuestData SelectedQuest => _selectedQuest;
        public QuestStatistics Statistics => _questStats;
        
        public void AddQuest(QuestData quest)
        {
            if (!_allQuests.Contains(quest))
            {
                _allQuests.Add(quest);
                RefreshQuestDisplay();
                UpdateQuestStatistics();
                
                ShowNotification($"New quest: {quest.title}");
                PlayQuestAcceptSound();
            }
        }
        
        public void CompleteQuest(int questId)
        {
            var quest = _allQuests.FirstOrDefault(q => q.id == questId);
            if (quest != null && quest.status == QuestStatus.Active)
            {
                quest.status = QuestStatus.Completed;
                
                ShowNotification($"Quest completed: {quest.title}");
                PlayQuestCompleteSound();
                
                RefreshQuestDisplay();
                UpdateQuestStatistics();
            }
        }
    }

    // Helper component for individual quest items
    public class QuestItemUI : MonoBehaviour
    {
        private QuestData _quest;
        private QuestPanel _questPanel;
        private Button _button;
        private bool _isSelected = false;
        
        // UI components
        private Image _background;
        private Image _icon;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _descriptionText;
        private Slider _progressBar;
        private TextMeshProUGUI _progressText;

        public void Initialize(QuestData quest, QuestPanel questPanel)
        {
            _quest = quest;
            _questPanel = questPanel;
            
            // Get components
            _button = GetComponent<Button>();
            if (_button == null)
                _button = gameObject.AddComponent<Button>();
            
            _background = GetComponent<Image>();
            
            // Find child components
            _icon = transform.Find("Content/Icon")?.GetComponent<Image>();
            _titleText = transform.Find("Content/Info/Title")?.GetComponent<TextMeshProUGUI>();
            _descriptionText = transform.Find("Content/Info/Description")?.GetComponent<TextMeshProUGUI>();
            _progressBar = transform.Find("Content/Progress/ProgressBar")?.GetComponent<Slider>();
            _progressText = transform.Find("Content/Progress/ProgressText")?.GetComponent<TextMeshProUGUI>();
            
            // Setup button click
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _questPanel.SelectQuest(_quest, this));
            
            UpdateDisplay();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_quest == null) return;
            
            // Update text components
            if (_titleText != null)
                _titleText.text = _quest.title;
            
            if (_descriptionText != null)
                _descriptionText.text = _quest.description;
            
            // Update progress
            if (_progressBar != null && _progressText != null)
            {
                float progress = CalculateProgress();
                _progressBar.value = progress;
                _progressText.text = $"{progress * 100f:F0}%";
            }
            
            // Update icon
            if (_icon != null && _quest.icon != null)
                _icon.sprite = _quest.icon;
            
            // Update background color based on state
            if (_background != null)
            {
                Color backgroundColor = GetBackgroundColor();
                _background.color = backgroundColor;
            }
        }

        private float CalculateProgress()
        {
            if (_quest.objectives.Count == 0) return 0f;
            
            float totalProgress = 0f;
            foreach (var objective in _quest.objectives)
            {
                float objProgress = objective.targetProgress > 0 ? 
                    (float)objective.currentProgress / objective.targetProgress : 0f;
                totalProgress += Mathf.Clamp01(objProgress);
            }
            
            return totalProgress / _quest.objectives.Count;
        }

        private Color GetBackgroundColor()
        {
            // Base color based on quest status
            Color baseColor = _quest.status switch
            {
                QuestStatus.Active => _questPanel.activeQuestColor,
                QuestStatus.Completed => _questPanel.completedQuestColor,
                QuestStatus.Failed => _questPanel.failedQuestColor,
                _ => Color.gray
            };
            
            // Modify based on selection and tracking
            if (_isSelected)
            {
                baseColor = Color.Lerp(baseColor, Color.white, 0.3f);
            }
            
            if (_questPanel.TrackedQuests.Contains(_quest))
            {
                baseColor = Color.Lerp(baseColor, _questPanel.trackedQuestColor, 0.2f);
            }
            
            // Reduce alpha for non-active background
            baseColor.a = 0.8f;
            
            return baseColor;
        }

        public QuestData Quest => _quest;
    }

    // Data structures and enums
    [System.Serializable]
    public class QuestData
    {
        public int id;
        public string title;
        public string description;
        public string questGiver;
        public QuestType type;
        public QuestDifficulty difficulty;
        public QuestStatus status;
        public List<QuestObjective> objectives = new List<QuestObjective>();
        public int experienceReward;
        public int moneyReward;
        public bool rewardClaimed;
        public Sprite icon;
        public bool isTimeLimited;
        public float timeRemaining;
        public System.DateTime acceptedDate;
        public System.DateTime? completedDate;
    }

    [System.Serializable]
    public class QuestObjective
    {
        public string description;
        public int currentProgress;
        public int targetProgress;
        public bool isCompleted;
        public string objectiveType; // "collect", "kill", "deliver", etc.
    }

    [System.Serializable]
    public class QuestStatistics
    {
        public int totalQuests;
        public int activeQuests;
        public int completedQuests;
        public int failedQuests;
        public float completionRate;
    }

    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    public enum QuestType
    {
        All = 0,
        MainStory = 1,
        SideQuest = 2,
        Daily = 3,
        Repeatable = 4,
        Collection = 5,
        Delivery = 6
    }

    public enum QuestDifficulty
    {
        All = 0,
        Easy = 1,
        Normal = 2,
        Hard = 3,
        Expert = 4
    }

    public enum QuestCategory
    {
        Active,
        Completed,
        Failed,
        All
    }
}