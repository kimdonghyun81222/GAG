using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame
{
    public class JournalPanel : UIPanel
    {
        [Header("Journal Tabs")]
        [SerializeField] private Transform tabContainer;
        [SerializeField] private Button notesTabButton;
        [SerializeField] private Button achievementsTabButton;
        [SerializeField] private Button collectionsTabButton;
        [SerializeField] private Button statisticsTabButton;
        [SerializeField] private Button settingsTabButton;
        
        [Header("Notes Section")]
        [SerializeField] private GameObject notesPanel;
        [SerializeField] private Transform notesList;
        [SerializeField] private GameObject noteItemPrefab;
        [SerializeField] private Button addNoteButton;
        [SerializeField] private TMP_InputField noteSearchField;
        
        [Header("Note Editor")]
        [SerializeField] private GameObject noteEditorPanel;
        [SerializeField] private TMP_InputField noteTitleInput;
        [SerializeField] private TMP_InputField noteContentInput;
        [SerializeField] private Button saveNoteButton;
        [SerializeField] private Button cancelNoteButton;
        [SerializeField] private Button deleteNoteButton;
        [SerializeField] private TMP_Dropdown noteCategoryDropdown;
        
        [Header("Achievements Section")]
        [SerializeField] private GameObject achievementsPanel;
        [SerializeField] private Transform achievementsList;
        [SerializeField] private GameObject achievementItemPrefab;
        [SerializeField] private TMP_Dropdown achievementFilterDropdown;
        [SerializeField] private TextMeshProUGUI achievementProgressText;
        
        [Header("Collections Section")]
        [SerializeField] private GameObject collectionsPanel;
        [SerializeField] private Transform collectionsList;
        [SerializeField] private GameObject collectionItemPrefab;
        [SerializeField] private TMP_Dropdown collectionCategoryDropdown;
        [SerializeField] private TextMeshProUGUI collectionProgressText;
        
        [Header("Statistics Section")]
        [SerializeField] private GameObject statisticsPanel;
        [SerializeField] private Transform statisticsList;
        [SerializeField] private GameObject statisticItemPrefab;
        [SerializeField] private Button exportStatsButton;
        
        [Header("Settings Section")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Toggle autoSaveToggle;
        [SerializeField] private Slider autoSaveIntervalSlider;
        [SerializeField] private TextMeshProUGUI autoSaveIntervalText;
        [SerializeField] private Button backupDataButton;
        [SerializeField] private Button restoreDataButton;
        
        [Header("Navigation")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button helpButton;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem achievementUnlockEffect;
        [SerializeField] private AudioClip achievementUnlockSound;
        [SerializeField] private AudioClip buttonClickSound;
        
        [Header("Animation")]
        [SerializeField] private bool enableJournalAnimations = true;
        [SerializeField] private float itemAnimationDelay = 0.03f;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Journal management
        private JournalTab _currentTab = JournalTab.Notes;
        private List<JournalNote> _playerNotes = new List<JournalNote>();
        private List<Achievement> _achievements = new List<Achievement>();
        private List<CollectionItem> _collections = new List<CollectionItem>();
        private List<GameStatistic> _statistics = new List<GameStatistic>();
        
        // Current editing
        private JournalNote _editingNote;
        private bool _isCreatingNewNote = false;
        
        // Filter options
        private readonly string[] _noteCategoryOptions = { "All", "General", "Farming", "Mining", "Fishing", "Combat", "Social" };
        private readonly string[] _achievementFilterOptions = { "All", "Unlocked", "Locked", "Recent" };
        private readonly string[] _collectionCategoryOptions = { "All", "Crops", "Fish", "Minerals", "Artifacts", "Cooking" };

        protected override void Awake()
        {
            base.Awake();
            InitializeJournal();
        }

        protected override void Start()
        {
            base.Start();
            SetupJournalPanel();
            LoadJournalData();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initially hide this panel
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleJournalInput();
        }

        private void InitializeJournal()
        {
            // Create default prefabs if none exist
            if (noteItemPrefab == null)
            {
                CreateDefaultNoteItemPrefab();
            }
            
            if (achievementItemPrefab == null)
            {
                CreateDefaultAchievementItemPrefab();
            }
            
            if (collectionItemPrefab == null)
            {
                CreateDefaultCollectionItemPrefab();
            }
            
            if (statisticItemPrefab == null)
            {
                CreateDefaultStatisticItemPrefab();
            }
        }

        private void SetupJournalPanel()
        {
            // Setup tab buttons
            if (notesTabButton != null)
            {
                notesTabButton.onClick.AddListener(() => SetTab(JournalTab.Notes));
                SetTabButtonActive(notesTabButton, true);
            }
            
            if (achievementsTabButton != null)
                achievementsTabButton.onClick.AddListener(() => SetTab(JournalTab.Achievements));
            
            if (collectionsTabButton != null)
                collectionsTabButton.onClick.AddListener(() => SetTab(JournalTab.Collections));
            
            if (statisticsTabButton != null)
                statisticsTabButton.onClick.AddListener(() => SetTab(JournalTab.Statistics));
            
            if (settingsTabButton != null)
                settingsTabButton.onClick.AddListener(() => SetTab(JournalTab.Settings));
            
            // Setup note editor
            if (addNoteButton != null)
                addNoteButton.onClick.AddListener(StartCreatingNewNote);
            
            if (saveNoteButton != null)
                saveNoteButton.onClick.AddListener(SaveCurrentNote);
            
            if (cancelNoteButton != null)
                cancelNoteButton.onClick.AddListener(CancelNoteEditing);
            
            if (deleteNoteButton != null)
                deleteNoteButton.onClick.AddListener(DeleteCurrentNote);
            
            if (noteSearchField != null)
                noteSearchField.onValueChanged.AddListener(OnNoteSearchChanged);
            
            // Setup dropdowns
            if (noteCategoryDropdown != null)
            {
                noteCategoryDropdown.ClearOptions();
                noteCategoryDropdown.AddOptions(_noteCategoryOptions.ToList());
            }
            
            if (achievementFilterDropdown != null)
            {
                achievementFilterDropdown.ClearOptions();
                achievementFilterDropdown.AddOptions(_achievementFilterOptions.ToList());
                achievementFilterDropdown.onValueChanged.AddListener(OnAchievementFilterChanged);
            }
            
            if (collectionCategoryDropdown != null)
            {
                collectionCategoryDropdown.ClearOptions();
                collectionCategoryDropdown.AddOptions(_collectionCategoryOptions.ToList());
                collectionCategoryDropdown.onValueChanged.AddListener(OnCollectionCategoryChanged);
            }
            
            // Setup settings
            if (autoSaveToggle != null)
                autoSaveToggle.onValueChanged.AddListener(OnAutoSaveToggleChanged);
            
            if (autoSaveIntervalSlider != null)
                autoSaveIntervalSlider.onValueChanged.AddListener(OnAutoSaveIntervalChanged);
            
            if (backupDataButton != null)
                backupDataButton.onClick.AddListener(BackupData);
            
            if (restoreDataButton != null)
                restoreDataButton.onClick.AddListener(RestoreData);
            
            if (exportStatsButton != null)
                exportStatsButton.onClick.AddListener(ExportStatistics);
            
            // Setup navigation
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseJournal);
            
            if (helpButton != null)
                helpButton.onClick.AddListener(ShowHelp);
            
            // Initialize displays
            HideNoteEditor();
            UpdateAutoSaveDisplay();
        }

        private void CreateDefaultNoteItemPrefab()
        {
            var noteObj = new GameObject("NoteItem");
            noteObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = noteObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 80f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = noteObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Button component
            var button = noteObj.AddComponent<Button>();
            button.targetGraphic = background;
            
            // Content layout
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(noteObj.transform, false);
            var contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 5f;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Note title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(contentObj.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Note Title";
            titleText.fontSize = 14f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            
            // Note preview
            var previewObj = new GameObject("Preview");
            previewObj.transform.SetParent(contentObj.transform, false);
            var previewText = previewObj.AddComponent<TextMeshProUGUI>();
            previewText.text = "Note content preview...";
            previewText.fontSize = 11f;
            previewText.color = Color.gray;
            previewText.overflowMode = TextOverflowModes.Ellipsis;
            
            // Note date
            var dateObj = new GameObject("Date");
            dateObj.transform.SetParent(contentObj.transform, false);
            var dateText = dateObj.AddComponent<TextMeshProUGUI>();
            dateText.text = "Created: Today";
            dateText.fontSize = 9f;
            dateText.color = Color.yellow;
            dateText.alignment = TextAlignmentOptions.TopRight;
            
            noteItemPrefab = noteObj;
            noteItemPrefab.SetActive(false);
        }

        private void CreateDefaultAchievementItemPrefab()
        {
            var achObj = new GameObject("AchievementItem");
            achObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = achObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 100f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = achObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Content layout
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(achObj.transform, false);
            var contentLayout = contentObj.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 15f;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Achievement icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(contentObj.transform, false);
            var icon = iconObj.AddComponent<Image>();
            icon.color = Color.white;
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(80f, 80f);
            
            // Achievement info
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(contentObj.transform, false);
            var infoLayout = infoObj.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 5f;
            
            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(infoObj.transform, false);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Achievement Title";
            titleText.fontSize = 16f;
            titleText.fontStyle = FontStyles.Bold;
            
            // Description
            var descObj = new GameObject("Description");
            descObj.transform.SetParent(infoObj.transform, false);
            var descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Achievement description";
            descText.fontSize = 12f;
            descText.color = Color.gray;
            
            // Progress
            var progressObj = new GameObject("Progress");
            progressObj.transform.SetParent(infoObj.transform, false);
            var progressText = progressObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "Progress: 50/100";
            progressText.fontSize = 11f;
            progressText.color = Color.yellow;
            
            achievementItemPrefab = achObj;
            achievementItemPrefab.SetActive(false);
        }

        private void CreateDefaultCollectionItemPrefab()
        {
            var collObj = new GameObject("CollectionItem");
            collObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = collObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 60f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = collObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            // Content layout
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(collObj.transform, false);
            var contentLayout = contentObj.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 10f;
            
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
            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(40f, 40f);
            
            // Item name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(contentObj.transform, false);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Item Name";
            nameText.fontSize = 14f;
            nameText.color = Color.white;
            
            // Collection status
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(contentObj.transform, false);
            var statusText = statusObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "Collected";
            statusText.fontSize = 12f;
            statusText.color = Color.green;
            statusText.alignment = TextAlignmentOptions.Right;
            
            collectionItemPrefab = collObj;
            collectionItemPrefab.SetActive(false);
        }

        private void CreateDefaultStatisticItemPrefab()
        {
            var statObj = new GameObject("StatisticItem");
            statObj.AddComponent<RectTransform>();
            
            // Layout component
            var layoutElement = statObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 40f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = statObj.AddComponent<Image>();
            background.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            
            // Content layout
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(statObj.transform, false);
            var contentLayout = contentObj.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(15, 15, 10, 10);
            contentLayout.spacing = 10f;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = true;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Stat name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(contentObj.transform, false);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Statistic Name";
            nameText.fontSize = 12f;
            nameText.color = Color.white;
            
            // Stat value
            var valueObj = new GameObject("Value");
            valueObj.transform.SetParent(contentObj.transform, false);
            var valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = "0";
            valueText.fontSize = 12f;
            valueText.color = Color.yellow;
            valueText.alignment = TextAlignmentOptions.Right;
            
            statisticItemPrefab = statObj;
            statisticItemPrefab.SetActive(false);
        }

        // Input handling
        private void HandleJournalInput()
        {
            // ESC to close journal
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (noteEditorPanel != null && noteEditorPanel.activeInHierarchy)
                {
                    CancelNoteEditing();
                }
                else
                {
                    CloseJournal();
                }
            }
            
            // Tab keys for quick tab switching
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetTab(JournalTab.Notes);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SetTab(JournalTab.Achievements);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SetTab(JournalTab.Collections);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                SetTab(JournalTab.Statistics);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                SetTab(JournalTab.Settings);
        }

        // Data loading and management
        private void LoadJournalData()
        {
            // This would normally load from various managers
            LoadPlayerNotes();
            LoadAchievements();
            LoadCollections();
            LoadStatistics();
            
            RefreshCurrentTabDisplay();
        }

        private void LoadPlayerNotes()
        {
            // Mock data for demonstration
            _playerNotes.Clear();
            
            var sampleNotes = new JournalNote[]
            {
                new JournalNote
                {
                    id = 1,
                    title = "First Day on the Farm",
                    content = "Today I inherited grandfather's old farm. It's in rough shape but I can see the potential. Need to clear the land and plant some parsnips to start.",
                    category = NoteCategory.General,
                    dateCreated = System.DateTime.Now.AddDays(-10),
                    dateModified = System.DateTime.Now.AddDays(-10)
                },
                new JournalNote
                {
                    id = 2,
                    title = "Crop Rotation Tips",
                    content = "Lewis mentioned that rotating crops can improve soil quality. Spring: parsnips, cauliflower. Summer: blueberries, melons. Fall: pumpkins, cranberries.",
                    category = NoteCategory.Farming,
                    dateCreated = System.DateTime.Now.AddDays(-7),
                    dateModified = System.DateTime.Now.AddDays(-5)
                },
                new JournalNote
                {
                    id = 3,
                    title = "Mining Strategy",
                    content = "The mines go deep. Bring lots of food and upgrade pickaxe when possible. Copper on floors 21-39, Iron on 41-79, Gold on 81-119.",
                    category = NoteCategory.Mining,
                    dateCreated = System.DateTime.Now.AddDays(-3),
                    dateModified = System.DateTime.Now.AddDays(-1)
                }
            };
            
            foreach (var note in sampleNotes)
            {
                _playerNotes.Add(note);
            }
        }

        private void LoadAchievements()
        {
            // Mock achievements data
            _achievements.Clear();
            
            var sampleAchievements = new Achievement[]
            {
                new Achievement
                {
                    id = 1,
                    title = "Greenhorn",
                    description = "Earn 15,000g",
                    category = AchievementCategory.Money,
                    isUnlocked = true,
                    currentProgress = 15000,
                    maxProgress = 15000,
                    rewardExperience = 100,
                    dateUnlocked = System.DateTime.Now.AddDays(-5)
                },
                new Achievement
                {
                    id = 2,
                    title = "Crop Master",
                    description = "Ship 300 crops",
                    category = AchievementCategory.Farming,
                    isUnlocked = false,
                    currentProgress = 187,
                    maxProgress = 300,
                    rewardExperience = 200
                },
                new Achievement
                {
                    id = 3,
                    title = "Treasure Hunter",
                    description = "Find 40 artifacts",
                    category = AchievementCategory.Exploration,
                    isUnlocked = false,
                    currentProgress = 12,
                    maxProgress = 40,
                    rewardExperience = 150
                }
            };
            
            foreach (var achievement in sampleAchievements)
            {
                _achievements.Add(achievement);
            }
        }

        private void LoadCollections()
        {
            // Mock collections data
            _collections.Clear();
            
            var sampleCollections = new CollectionItem[]
            {
                new CollectionItem { name = "Parsnip", category = CollectionCategory.Crops, isCollected = true, dateCollected = System.DateTime.Now.AddDays(-8) },
                new CollectionItem { name = "Cauliflower", category = CollectionCategory.Crops, isCollected = true, dateCollected = System.DateTime.Now.AddDays(-6) },
                new CollectionItem { name = "Potato", category = CollectionCategory.Crops, isCollected = false },
                new CollectionItem { name = "Carp", category = CollectionCategory.Fish, isCollected = true, dateCollected = System.DateTime.Now.AddDays(-4) },
                new CollectionItem { name = "Sardine", category = CollectionCategory.Fish, isCollected = false },
                new CollectionItem { name = "Copper Ore", category = CollectionCategory.Minerals, isCollected = true, dateCollected = System.DateTime.Now.AddDays(-7) },
                new CollectionItem { name = "Iron Ore", category = CollectionCategory.Minerals, isCollected = true, dateCollected = System.DateTime.Now.AddDays(-3) },
                new CollectionItem { name = "Gold Ore", category = CollectionCategory.Minerals, isCollected = false }
            };
            
            foreach (var item in sampleCollections)
            {
                _collections.Add(item);
            }
        }

        private void LoadStatistics()
        {
            // Mock statistics data
            _statistics.Clear();
            
            var sampleStats = new GameStatistic[]
            {
                new GameStatistic { name = "Days Played", value = "25", category = "General" },
                new GameStatistic { name = "Total Money Earned", value = "$47,350", category = "Economy" },
                new GameStatistic { name = "Crops Harvested", value = "387", category = "Farming" },
                new GameStatistic { name = "Fish Caught", value = "56", category = "Fishing" },
                new GameStatistic { name = "Monsters Defeated", value = "23", category = "Combat" },
                new GameStatistic { name = "Floors Reached", value = "65", category = "Mining" },
                new GameStatistic { name = "Recipes Learned", value = "12", category = "Cooking" },
                new GameStatistic { name = "Friends Made", value = "8", category = "Social" }
            };
            
            foreach (var stat in sampleStats)
            {
                _statistics.Add(stat);
            }
        }

        // Tab management
        private void SetTab(JournalTab tab)
        {
            _currentTab = tab;
            
            // Hide all panels
            HideAllPanels();
            
            // Show selected panel
            switch (tab)
            {
                case JournalTab.Notes:
                    if (notesPanel != null) notesPanel.SetActive(true);
                    break;
                case JournalTab.Achievements:
                    if (achievementsPanel != null) achievementsPanel.SetActive(true);
                    break;
                case JournalTab.Collections:
                    if (collectionsPanel != null) collectionsPanel.SetActive(true);
                    break;
                case JournalTab.Statistics:
                    if (statisticsPanel != null) statisticsPanel.SetActive(true);
                    break;
                case JournalTab.Settings:
                    if (settingsPanel != null) settingsPanel.SetActive(true);
                    break;
            }
            
            // Update tab button states
            SetTabButtonActive(notesTabButton, tab == JournalTab.Notes);
            SetTabButtonActive(achievementsTabButton, tab == JournalTab.Achievements);
            SetTabButtonActive(collectionsTabButton, tab == JournalTab.Collections);
            SetTabButtonActive(statisticsTabButton, tab == JournalTab.Statistics);
            SetTabButtonActive(settingsTabButton, tab == JournalTab.Settings);
            
            // Refresh display for current tab
            RefreshCurrentTabDisplay();
        }

        private void SetTabButtonActive(Button button, bool active)
        {
            if (button == null) return;
            
            var colors = button.colors;
            colors.normalColor = active ? Color.yellow : Color.white;
            button.colors = colors;
        }

        private void HideAllPanels()
        {
            if (notesPanel != null) notesPanel.SetActive(false);
            if (achievementsPanel != null) achievementsPanel.SetActive(false);
            if (collectionsPanel != null) collectionsPanel.SetActive(false);
            if (statisticsPanel != null) statisticsPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void RefreshCurrentTabDisplay()
        {
            switch (_currentTab)
            {
                case JournalTab.Notes:
                    RefreshNotesDisplay();
                    break;
                case JournalTab.Achievements:
                    RefreshAchievementsDisplay();
                    break;
                case JournalTab.Collections:
                    RefreshCollectionsDisplay();
                    break;
                case JournalTab.Statistics:
                    RefreshStatisticsDisplay();
                    break;
                case JournalTab.Settings:
                    RefreshSettingsDisplay();
                    break;
            }
        }

        // Notes management
        private void RefreshNotesDisplay()
        {
            if (notesList == null) return;
            
            // Clear existing notes
            foreach (Transform child in notesList)
            {
                Destroy(child.gameObject);
            }
            
            // Filter notes based on search
            var filteredNotes = _playerNotes.AsEnumerable();
            
            if (noteSearchField != null && !string.IsNullOrEmpty(noteSearchField.text))
            {
                string searchQuery = noteSearchField.text.ToLower();
                filteredNotes = filteredNotes.Where(note => 
                    note.title.ToLower().Contains(searchQuery) ||
                    note.content.ToLower().Contains(searchQuery));
            }
            
            // Sort by date modified (most recent first)
            var sortedNotes = filteredNotes.OrderByDescending(note => note.dateModified).ToList();
            
            // Create note items
            StartCoroutine(CreateNoteItems(sortedNotes));
        }

        private IEnumerator CreateNoteItems(List<JournalNote> notes)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                var noteObj = Instantiate(noteItemPrefab, notesList);
                noteObj.SetActive(true);
                
                // Update note display
                var texts = noteObj.GetComponentsInChildren<TextMeshProUGUI>();
                var button = noteObj.GetComponent<Button>();
                
                if (texts.Length >= 3)
                {
                    texts[0].text = note.title;
                    texts[1].text = note.content.Length > 100 ? 
                        note.content.Substring(0, 100) + "..." : 
                        note.content;
                    texts[2].text = $"Modified: {note.dateModified:MM/dd/yyyy}";
                }
                
                // Setup button click
                if (button != null)
                {
                    button.onClick.AddListener(() => EditNote(note));
                }
                
                // Animate item in
                if (enableJournalAnimations)
                {
                    StartCoroutine(AnimateItemIn(noteObj.transform, i * itemAnimationDelay));
                }
                
                yield return null;
            }
        }

        private IEnumerator AnimateItemIn(Transform itemTransform, float delay)
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

        private void OnNoteSearchChanged(string searchQuery)
        {
            RefreshNotesDisplay();
        }

        private void StartCreatingNewNote()
        {
            PlayButtonClickSound();
            
            _isCreatingNewNote = true;
            _editingNote = new JournalNote
            {
                id = GetNextNoteId(),
                title = "",
                content = "",
                category = NoteCategory.General,
                dateCreated = System.DateTime.Now,
                dateModified = System.DateTime.Now
            };
            
            ShowNoteEditor();
        }

        private void EditNote(JournalNote note)
        {
            PlayButtonClickSound();
            
            _isCreatingNewNote = false;
            _editingNote = note;
            
            ShowNoteEditor();
        }

        private void ShowNoteEditor()
        {
            if (noteEditorPanel == null) return;
            
            noteEditorPanel.SetActive(true);
            
            // Populate editor fields
            if (noteTitleInput != null)
                noteTitleInput.text = _editingNote.title;
            
            if (noteContentInput != null)
                noteContentInput.text = _editingNote.content;
            
            if (noteCategoryDropdown != null)
                noteCategoryDropdown.value = (int)_editingNote.category;
            
            // Show/hide delete button
            if (deleteNoteButton != null)
                deleteNoteButton.gameObject.SetActive(!_isCreatingNewNote);
        }

        private void HideNoteEditor()
        {
            if (noteEditorPanel != null)
            {
                noteEditorPanel.SetActive(false);
            }
            
            _editingNote = null;
            _isCreatingNewNote = false;
        }

        private void SaveCurrentNote()
        {
            if (_editingNote == null) return;
            
            PlayButtonClickSound();
            
            // Update note data
            _editingNote.title = noteTitleInput?.text ?? "";
            _editingNote.content = noteContentInput?.text ?? "";
            _editingNote.category = noteCategoryDropdown != null ? 
                (NoteCategory)noteCategoryDropdown.value : NoteCategory.General;
            _editingNote.dateModified = System.DateTime.Now;
            
            // Validate note
            if (string.IsNullOrWhiteSpace(_editingNote.title))
            {
                Debug.Log("Note title cannot be empty!");
                return;
            }
            
            // Add to list if creating new
            if (_isCreatingNewNote)
            {
                _playerNotes.Add(_editingNote);
            }
            
            // Save to persistent storage here
            // SaveManager.SavePlayerNotes(_playerNotes);
            
            HideNoteEditor();
            RefreshNotesDisplay();
        }

        private void CancelNoteEditing()
        {
            PlayButtonClickSound();
            HideNoteEditor();
        }

        private void DeleteCurrentNote()
        {
            if (_editingNote == null || _isCreatingNewNote) return;
            
            PlayButtonClickSound();
            
            // Remove from list
            _playerNotes.Remove(_editingNote);
            
            // Save to persistent storage here
            // SaveManager.SavePlayerNotes(_playerNotes);
            
            HideNoteEditor();
            RefreshNotesDisplay();
        }

        private int GetNextNoteId()
        {
            return _playerNotes.Count > 0 ? _playerNotes.Max(note => note.id) + 1 : 1;
        }

        // Achievements display
        private void RefreshAchievementsDisplay()
        {
            if (achievementsList == null) return;
            
            // Clear existing achievements
            foreach (Transform child in achievementsList)
            {
                Destroy(child.gameObject);
            }
            
            // Filter achievements
            var filteredAchievements = FilterAchievements();
            
            // Create achievement items
            StartCoroutine(CreateAchievementItems(filteredAchievements));
            
            // Update progress text
            UpdateAchievementProgress();
        }

        private List<Achievement> FilterAchievements()
        {
            var filtered = _achievements.AsEnumerable();
            
            if (achievementFilterDropdown != null)
            {
                int filterIndex = achievementFilterDropdown.value;
                filtered = filterIndex switch
                {
                    1 => filtered.Where(a => a.isUnlocked), // Unlocked
                    2 => filtered.Where(a => !a.isUnlocked), // Locked
                    3 => filtered.Where(a => a.dateUnlocked.HasValue && 
                        (System.DateTime.Now - a.dateUnlocked.Value).TotalDays <= 7), // Recent (last 7 days)
                    _ => filtered // All
                };
            }
            
            // Sort by unlock status, then by progress
            return filtered.OrderBy(a => !a.isUnlocked)
                          .ThenByDescending(a => (float)a.currentProgress / a.maxProgress)
                          .ToList();
        }

        private IEnumerator CreateAchievementItems(List<Achievement> achievements)
        {
            for (int i = 0; i < achievements.Count; i++)
            {
                var achievement = achievements[i];
                var achObj = Instantiate(achievementItemPrefab, achievementsList);
                achObj.SetActive(true);
                
                // Update achievement display
                var texts = achObj.GetComponentsInChildren<TextMeshProUGUI>();
                var images = achObj.GetComponentsInChildren<Image>();
                
                if (texts.Length >= 3)
                {
                    texts[0].text = achievement.title;
                    texts[1].text = achievement.description;
                    texts[2].text = $"Progress: {achievement.currentProgress}/{achievement.maxProgress}";
                    
                    // Color based on completion
                    if (achievement.isUnlocked)
                    {
                        texts[0].color = Color.yellow;
                        texts[2].color = Color.green;
                        texts[2].text += " ✓";
                    }
                    else
                    {
                        texts[0].color = Color.white;
                        texts[2].color = Color.yellow;
                    }
                }
                
                // Update background color
                if (images.Length > 0)
                {
                    images[0].color = achievement.isUnlocked ? 
                        new Color(0.3f, 0.4f, 0.2f, 0.9f) : 
                        new Color(0.2f, 0.2f, 0.2f, 0.9f);
                }
                
                // Animate item in
                if (enableJournalAnimations)
                {
                    StartCoroutine(AnimateItemIn(achObj.transform, i * itemAnimationDelay));
                }
                
                yield return null;
            }
        }

        private void UpdateAchievementProgress()
        {
            if (achievementProgressText == null) return;
            
            int unlockedCount = _achievements.Count(a => a.isUnlocked);
            int totalCount = _achievements.Count;
            float percentage = totalCount > 0 ? (float)unlockedCount / totalCount * 100f : 0f;
            
            achievementProgressText.text = $"Achievements: {unlockedCount}/{totalCount} ({percentage:F1}%)";
        }

        private void OnAchievementFilterChanged(int filterIndex)
        {
            RefreshAchievementsDisplay();
        }

        // Collections display
        private void RefreshCollectionsDisplay()
        {
            if (collectionsList == null) return;
            
            // Clear existing collections
            foreach (Transform child in collectionsList)
            {
                Destroy(child.gameObject);
            }
            
            // Filter collections
            var filteredCollections = FilterCollections();
            
            // Create collection items
            StartCoroutine(CreateCollectionItems(filteredCollections));
            
            // Update progress text
            UpdateCollectionProgress();
        }

        private List<CollectionItem> FilterCollections()
        {
            var filtered = _collections.AsEnumerable();
            
            if (collectionCategoryDropdown != null)
            {
                int categoryIndex = collectionCategoryDropdown.value;
                if (categoryIndex > 0) // Not "All"
                {
                    var category = (CollectionCategory)(categoryIndex - 1);
                    filtered = filtered.Where(c => c.category == category);
                }
            }
            
            // Sort by category, then by collection status, then by name
            return filtered.OrderBy(c => c.category)
                          .ThenBy(c => !c.isCollected)
                          .ThenBy(c => c.name)
                          .ToList();
        }

        private IEnumerator CreateCollectionItems(List<CollectionItem> collections)
        {
            for (int i = 0; i < collections.Count; i++)
            {
                var collection = collections[i];
                var collObj = Instantiate(collectionItemPrefab, collectionsList);
                collObj.SetActive(true);
                
                // Update collection display
                var texts = collObj.GetComponentsInChildren<TextMeshProUGUI>();
                var images = collObj.GetComponentsInChildren<Image>();
                
                if (texts.Length >= 2)
                {
                    texts[0].text = collection.name;
                    
                    if (collection.isCollected)
                    {
                        texts[1].text = $"✓ {collection.dateCollected:MM/dd/yyyy}";
                        texts[1].color = Color.green;
                    }
                    else
                    {
                        texts[1].text = "Not Collected";
                        texts[1].color = Color.gray;
                    }
                }
                
                // Update background color
                if (images.Length > 0)
                {
                    images[0].color = collection.isCollected ? 
                        new Color(0.2f, 0.4f, 0.2f, 0.9f) : 
                        new Color(0.1f, 0.1f, 0.1f, 0.9f);
                }
                
                // Animate item in
                if (enableJournalAnimations)
                {
                    StartCoroutine(AnimateItemIn(collObj.transform, i * itemAnimationDelay));
                }
                
                yield return null;
            }
        }

        private void UpdateCollectionProgress()
        {
            if (collectionProgressText == null) return;
            
            // Calculate progress by category
            var categoryProgress = new Dictionary<CollectionCategory, (int collected, int total)>();
            
            foreach (var category in System.Enum.GetValues(typeof(CollectionCategory)).Cast<CollectionCategory>())
            {
                var categoryItems = _collections.Where(c => c.category == category).ToList();
                int collected = categoryItems.Count(c => c.isCollected);
                int total = categoryItems.Count;
                categoryProgress[category] = (collected, total);
            }
            
            // Display overall progress
            int totalCollected = _collections.Count(c => c.isCollected);
            int totalItems = _collections.Count;
            float percentage = totalItems > 0 ? (float)totalCollected / totalItems * 100f : 0f;
            
            collectionProgressText.text = $"Collections: {totalCollected}/{totalItems} ({percentage:F1}%)";
        }

        private void OnCollectionCategoryChanged(int categoryIndex)
        {
            RefreshCollectionsDisplay();
        }

        // Statistics display
        private void RefreshStatisticsDisplay()
        {
            if (statisticsList == null) return;
            
            // Clear existing statistics
            foreach (Transform child in statisticsList)
            {
                Destroy(child.gameObject);
            }
            
            // Group statistics by category
            var groupedStats = _statistics.GroupBy(s => s.category).OrderBy(g => g.Key);
            
            StartCoroutine(CreateStatisticItems(groupedStats));
        }

        private IEnumerator CreateStatisticItems(IEnumerable<IGrouping<string, GameStatistic>> groupedStats)
        {
            int itemIndex = 0;
            
            foreach (var group in groupedStats)
            {
                // Create category header
                var headerObj = new GameObject("CategoryHeader");
                headerObj.transform.SetParent(statisticsList, false);
                
                var headerRect = headerObj.AddComponent<RectTransform>();
                var headerLayout = headerObj.AddComponent<LayoutElement>();
                headerLayout.preferredHeight = 30f;
                headerLayout.flexibleWidth = 1f;
                
                var headerText = headerObj.AddComponent<TextMeshProUGUI>();
                headerText.text = group.Key;
                headerText.fontSize = 14f;
                headerText.fontStyle = FontStyles.Bold;
                headerText.color = Color.cyan;
                headerText.alignment = TextAlignmentOptions.Left;
                
                // Create statistics for this category
                foreach (var stat in group)
                {
                    var statObj = Instantiate(statisticItemPrefab, statisticsList);
                    statObj.SetActive(true);
                    
                    // Update statistic display
                    var texts = statObj.GetComponentsInChildren<TextMeshProUGUI>();
                    
                    if (texts.Length >= 2)
                    {
                        texts[0].text = stat.name;
                        texts[1].text = stat.value;
                    }
                    
                    // Animate item in
                    if (enableJournalAnimations)
                    {
                        StartCoroutine(AnimateItemIn(statObj.transform, itemIndex * itemAnimationDelay));
                    }
                    
                    itemIndex++;
                    yield return null;
                }
                
                yield return null;
            }
        }

        private void ExportStatistics()
        {
            PlayButtonClickSound();
            
            // This would export statistics to a file
            Debug.Log("Exporting statistics to file...");
            
            // Mock implementation
            string statisticsData = "Game Statistics Export\n";
            statisticsData += $"Generated: {System.DateTime.Now}\n\n";
            
            foreach (var group in _statistics.GroupBy(s => s.category))
            {
                statisticsData += $"{group.Key}:\n";
                foreach (var stat in group)
                {
                    statisticsData += $"  {stat.name}: {stat.value}\n";
                }
                statisticsData += "\n";
            }
            
            // In a real implementation, this would save to file
            Debug.Log(statisticsData);
        }

        // Settings management
        private void RefreshSettingsDisplay()
        {
            // Update settings display with current values
            UpdateAutoSaveDisplay();
        }

        private void OnAutoSaveToggleChanged(bool enabled)
        {
            // Save auto-save preference
            PlayerPrefs.SetInt("AutoSaveEnabled", enabled ? 1 : 0);
            UpdateAutoSaveDisplay();
        }

        private void OnAutoSaveIntervalChanged(float interval)
        {
            // Save auto-save interval
            PlayerPrefs.SetFloat("AutoSaveInterval", interval);
            UpdateAutoSaveDisplay();
        }

        private void UpdateAutoSaveDisplay()
        {
            if (autoSaveIntervalText != null && autoSaveIntervalSlider != null)
            {
                float interval = autoSaveIntervalSlider.value;
                autoSaveIntervalText.text = $"Auto-save every {interval:F0} minutes";
            }
        }

        private void BackupData()
        {
            PlayButtonClickSound();
            
            // This would create a backup of save data
            Debug.Log("Creating data backup...");
            
            // Mock implementation
            string backupPath = $"Backup_{System.DateTime.Now:yyyyMMdd_HHmmss}.save";
            Debug.Log($"Backup created: {backupPath}");
        }

        private void RestoreData()
        {
            PlayButtonClickSound();
            
            // This would restore from backup
            Debug.Log("Restoring data from backup...");
        }

        // Navigation and utility
        private void CloseJournal()
        {
            gameObject.SetActive(false);
            
            // Return to pause menu if it was open
            var pauseMenu = FindObjectOfType<PauseMenuPanel>();
            if (pauseMenu != null && pauseMenu.IsPaused)
            {
                // pauseMenu.ReturnToMainPauseMenu(); // Would implement this
            }
        }

        private void ShowHelp()
        {
            PlayButtonClickSound();
            Debug.Log("Showing journal help...");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        // Public interface
        public JournalTab CurrentTab => _currentTab;
        public List<JournalNote> PlayerNotes => _playerNotes;
        public List<Achievement> Achievements => _achievements;
        
        public void UnlockAchievement(int achievementId)
        {
            var achievement = _achievements.FirstOrDefault(a => a.id == achievementId);
            if (achievement != null && !achievement.isUnlocked)
            {
                achievement.isUnlocked = true;
                achievement.dateUnlocked = System.DateTime.Now;
                
                // Play unlock effect
                if (achievementUnlockEffect != null)
                    achievementUnlockEffect.Play();
                
                Debug.Log($"Achievement unlocked: {achievement.title}");
                
                if (_currentTab == JournalTab.Achievements)
                {
                    RefreshAchievementsDisplay();
                }
            }
        }
        
        public void UpdateAchievementProgress(int achievementId, int progress)
        {
            var achievement = _achievements.FirstOrDefault(a => a.id == achievementId);
            if (achievement != null)
            {
                achievement.currentProgress = progress;
                
                // Check if achievement should be unlocked
                if (!achievement.isUnlocked && achievement.currentProgress >= achievement.maxProgress)
                {
                    UnlockAchievement(achievementId);
                }
                
                if (_currentTab == JournalTab.Achievements)
                {
                    RefreshAchievementsDisplay();
                }
            }
        }
        
        public void CollectItem(string itemName, CollectionCategory category)
        {
            var item = _collections.FirstOrDefault(c => c.name == itemName && c.category == category);
            if (item != null && !item.isCollected)
            {
                item.isCollected = true;
                item.dateCollected = System.DateTime.Now;
                
                Debug.Log($"Collection item found: {itemName}");
                
                if (_currentTab == JournalTab.Collections)
                {
                    RefreshCollectionsDisplay();
                }
            }
        }
    }

    // Data structures and enums
    [System.Serializable]
    public class JournalNote
    {
        public int id;
        public string title;
        public string content;
        public NoteCategory category;
        public System.DateTime dateCreated;
        public System.DateTime dateModified;
    }

    [System.Serializable]
    public class Achievement
    {
        public int id;
        public string title;
        public string description;
        public AchievementCategory category;
        public bool isUnlocked;
        public int currentProgress;
        public int maxProgress;
        public int rewardExperience;
        public Sprite icon;
        public System.DateTime? dateUnlocked;
    }

    [System.Serializable]
    public class CollectionItem
    {
        public string name;
        public CollectionCategory category;
        public bool isCollected;
        public System.DateTime? dateCollected;
        public Sprite icon;
    }

    [System.Serializable]
    public class GameStatistic
    {
        public string name;
        public string value;
        public string category;
    }

    public enum JournalTab
    {
        Notes,
        Achievements,
        Collections,
        Statistics,
        Settings
    }

    public enum NoteCategory
    {
        General = 0,
        Farming = 1,
        Mining = 2,
        Fishing = 3,
        Combat = 4,
        Social = 5
    }

    public enum AchievementCategory
    {
        General,
        Farming,
        Mining,
        Fishing,
        Combat,
        Social,
        Money,
        Exploration
    }

    public enum CollectionCategory
    {
        Crops,
        Fish,
        Minerals,
        Artifacts,
        Cooking
    }
}