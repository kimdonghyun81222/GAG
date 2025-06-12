using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.MainMenu
{
    public class LoadGamePanel : UIPanel
    {
        [Header("Save Slot Container")]
        [SerializeField] private Transform saveSlotContainer;
        [SerializeField] private GameObject saveSlotPrefab;
        [SerializeField] private ScrollRect saveSlotScrollRect;
        [SerializeField] private int maxVisibleSlots = 6;
        
        [Header("Save Slot Details")]
        [SerializeField] private GameObject saveDetailsPanel;
        [SerializeField] private Image saveScreenshot;
        [SerializeField] private TextMeshProUGUI savePlayerName;
        [SerializeField] private TextMeshProUGUI saveFarmName;
        [SerializeField] private TextMeshProUGUI savePlayTime;
        [SerializeField] private TextMeshProUGUI saveDate;
        [SerializeField] private TextMeshProUGUI saveLevel;
        [SerializeField] private TextMeshProUGUI saveMoney;
        [SerializeField] private TextMeshProUGUI saveSeason;
        [SerializeField] private TextMeshProUGUI saveDay;
        
        [Header("Action Buttons")]
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button deleteGameButton;
        [SerializeField] private Button duplicateGameButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button refreshButton;
        
        [Header("Sorting Options")]
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private Button sortOrderButton;
        [SerializeField] private TextMeshProUGUI sortOrderText;
        [SerializeField] private TMP_InputField searchField;
        
        [Header("Empty State")]
        [SerializeField] private GameObject emptyStatePanel;
        [SerializeField] private TextMeshProUGUI emptyStateText;
        [SerializeField] private Button createNewGameButton;
        
        [Header("Loading State")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private Slider loadingProgressBar;
        [SerializeField] private TextMeshProUGUI loadingText;
        
        [Header("Confirmation Dialogs")]
        [SerializeField] private GameObject deleteConfirmationPanel;
        [SerializeField] private TextMeshProUGUI deleteConfirmationText;
        [SerializeField] private Button confirmDeleteButton;
        [SerializeField] private Button cancelDeleteButton;
        
        [Header("Visual Settings")]
        // 🔧 수정: private 필드를 public으로 변경하여 SaveSlotUI에서 접근 가능하게 함
        public Color selectedSlotColor = Color.yellow;
        public Color normalSlotColor = Color.white;
        public Color corruptedSlotColor = Color.red;
        [SerializeField] private Sprite defaultScreenshot;
        
        [Header("Animation")]
        [SerializeField] private bool enableSlotAnimations = true;
        [SerializeField] private float slotAnimationDelay = 0.05f;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip slotSelectSound;
        [SerializeField] private AudioClip deleteSound;
        [SerializeField] private AudioClip loadGameSound;
        
        // 🔧 수정: Dependencies를 임시로 주석 처리 (나중에 구현)
        // [Inject] private SaveManager saveManager;
        // [Inject] private AudioManager audioManager;
        
        // Save slot management
        private List<SaveSlotUI> _saveSlots = new List<SaveSlotUI>();
        private List<SaveFileInfo> _saveFiles = new List<SaveFileInfo>();
        private SaveSlotUI _selectedSlot;
        private SaveFileInfo _selectedSave;
        
        // Sorting and filtering
        private SaveSortType _currentSortType = SaveSortType.DateModified;
        private bool _sortAscending = false;
        private string _searchQuery = "";
        
        // State management
        private bool _isLoading = false;
        
        // Sort types
        private readonly string[] _sortOptions = { "Date Modified", "Date Created", "Player Name", "Farm Name", "Play Time", "Level" };

        protected override void Awake()
        {
            base.Awake();
            InitializeLoadGamePanel();
        }

        protected override void Start()
        {
            base.Start();
            SetupLoadGamePanel();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 🔧 수정: 초기에 이 패널을 숨김
            gameObject.SetActive(false);
        }

        private void InitializeLoadGamePanel()
        {
            // Create default save slot prefab if none exists
            if (saveSlotPrefab == null)
            {
                CreateDefaultSaveSlotPrefab();
            }
            
            // Initialize sort dropdown
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(_sortOptions.ToList());
                sortDropdown.value = (int)_currentSortType;
            }
        }

        private void SetupLoadGamePanel()
        {
            // Setup button listeners
            if (loadGameButton != null)
            {
                loadGameButton.onClick.AddListener(LoadSelectedGame);
                loadGameButton.interactable = false;
            }
            
            if (deleteGameButton != null)
            {
                deleteGameButton.onClick.AddListener(ShowDeleteConfirmation);
                deleteGameButton.interactable = false;
            }
            
            if (duplicateGameButton != null)
            {
                duplicateGameButton.onClick.AddListener(DuplicateSelectedGame);
                duplicateGameButton.interactable = false;
            }
            
            if (backButton != null)
                backButton.onClick.AddListener(ReturnToMainMenu);
            
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshSaveList);
            
            if (createNewGameButton != null)
                createNewGameButton.onClick.AddListener(CreateNewGame);
            
            // Setup confirmation dialog
            if (confirmDeleteButton != null)
                confirmDeleteButton.onClick.AddListener(ConfirmDeleteGame);
            
            if (cancelDeleteButton != null)
                cancelDeleteButton.onClick.AddListener(CancelDeleteGame);
            
            // Setup sorting
            if (sortDropdown != null)
                sortDropdown.onValueChanged.AddListener(OnSortTypeChanged);
            
            if (sortOrderButton != null)
                sortOrderButton.onClick.AddListener(ToggleSortOrder);
            
            // Setup search
            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);
            
            // Initialize displays
            UpdateSortOrderDisplay();
            ShowLoadingState(false);
            ShowEmptyState(false);
            ShowDeleteConfirmation(false);
            
            // Load save files
            RefreshSaveList();
        }

        private void CreateDefaultSaveSlotPrefab()
        {
            var slotObj = new GameObject("SaveSlot");
            slotObj.AddComponent<RectTransform>();
            
            // Add layout components
            var layoutElement = slotObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 100f;
            layoutElement.flexibleWidth = 1f;
            
            // Background
            var background = slotObj.AddComponent<Image>();
            background.color = normalSlotColor;
            
            // Button component
            var button = slotObj.AddComponent<Button>();
            
            // Content container
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(slotObj.transform, false);
            var contentLayout = contentObj.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 10f;
            contentLayout.childControlWidth = false;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = false;
            
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Screenshot
            var screenshotObj = new GameObject("Screenshot");
            screenshotObj.transform.SetParent(contentObj.transform, false);
            var screenshot = screenshotObj.AddComponent<Image>();
            screenshot.sprite = defaultScreenshot;
            var screenshotRect = screenshotObj.GetComponent<RectTransform>();
            screenshotRect.sizeDelta = new Vector2(80f, 80f);
            
            // Info container
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(contentObj.transform, false);
            var infoLayout = infoObj.AddComponent<VerticalLayoutGroup>();
            infoLayout.childControlWidth = false;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            
            var infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.sizeDelta = new Vector2(200f, 80f);
            
            // Player/Farm name
            var nameObj = new GameObject("Name");
            nameObj.transform.SetParent(infoObj.transform, false);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Player Name - Farm Name";
            nameText.fontSize = 14f;
            nameText.fontStyle = FontStyles.Bold;
            
            // Play time and date
            var timeObj = new GameObject("Time");
            timeObj.transform.SetParent(infoObj.transform, false);
            var timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.text = "Play Time: 00:00:00";
            timeText.fontSize = 12f;
            
            var dateObj = new GameObject("Date");
            dateObj.transform.SetParent(infoObj.transform, false);
            var dateText = dateObj.AddComponent<TextMeshProUGUI>();
            dateText.text = "Last Played: Never";
            dateText.fontSize = 10f;
            dateText.color = Color.gray;
            
            // Status container (level, money, season)
            var statusObj = new GameObject("Status");
            statusObj.transform.SetParent(contentObj.transform, false);
            var statusLayout = statusObj.AddComponent<VerticalLayoutGroup>();
            statusLayout.childControlWidth = false;
            statusLayout.childControlHeight = false;
            
            var statusRect = statusObj.GetComponent<RectTransform>();
            statusRect.sizeDelta = new Vector2(100f, 80f);
            
            var levelObj = new GameObject("Level");
            levelObj.transform.SetParent(statusObj.transform, false);
            var levelText = levelObj.AddComponent<TextMeshProUGUI>();
            levelText.text = "Level: 1";
            levelText.fontSize = 11f;
            
            var moneyObj = new GameObject("Money");
            moneyObj.transform.SetParent(statusObj.transform, false);
            var moneyText = moneyObj.AddComponent<TextMeshProUGUI>();
            moneyText.text = "$0";
            moneyText.fontSize = 11f;
            
            var seasonObj = new GameObject("Season");
            seasonObj.transform.SetParent(statusObj.transform, false);
            var seasonText = seasonObj.AddComponent<TextMeshProUGUI>();
            seasonText.text = "Spring, Day 1";
            seasonText.fontSize = 10f;
            
            // Add SaveSlotUI component
            var saveSlotUI = slotObj.AddComponent<SaveSlotUI>();
            
            saveSlotPrefab = slotObj;
            saveSlotPrefab.SetActive(false);
        }

        // Save file management
        private void RefreshSaveList()
        {
            if (_isLoading) return;
            
            StartCoroutine(RefreshSaveListCoroutine());
        }

        private IEnumerator RefreshSaveListCoroutine()
        {
            _isLoading = true;
            ShowLoadingState(true);
            
            // Clear existing slots
            ClearSaveSlots();
            
            // Load save files from SaveManager
            yield return StartCoroutine(LoadSaveFiles());
            
            // Filter and sort
            var filteredSaves = FilterAndSortSaves(_saveFiles);
            
            // Create UI slots
            yield return StartCoroutine(CreateSaveSlots(filteredSaves));
            
            // Update UI state
            ShowLoadingState(false);
            ShowEmptyState(filteredSaves.Count == 0);
            
            _isLoading = false;
        }

        private IEnumerator LoadSaveFiles()
        {
            _saveFiles.Clear();
            
            // 🔧 수정: SaveManager 사용을 임시로 주석 처리
            // if (saveManager != null)
            // {
            //     // This would call SaveManager to get all save files
            //     var saveFiles = saveManager.GetAllSaveFiles();
            //     
            //     foreach (var saveFile in saveFiles)
            //     {
            //         _saveFiles.Add(saveFile);
            //         yield return null; // Spread loading across frames
            //     }
            // }
            // else
            // {
                // Mock data for demonstration
                yield return StartCoroutine(CreateMockSaveData());
            // }
        }

        private IEnumerator CreateMockSaveData()
        {
            // Create some mock save data for demonstration
            var mockSaves = new SaveFileInfo[]
            {
                new SaveFileInfo
                {
                    fileName = "save_001.json",
                    playerName = "Alex",
                    farmName = "Sunny Acres",
                    playTime = "12:34:56",
                    lastPlayed = System.DateTime.Now.AddDays(-1),
                    level = 5,
                    money = 15000,
                    season = "Summer",
                    day = 15,
                    isCorrupted = false
                },
                new SaveFileInfo
                {
                    fileName = "save_002.json",
                    playerName = "Jordan",
                    farmName = "Green Valley",
                    playTime = "08:21:43",
                    lastPlayed = System.DateTime.Now.AddDays(-3),
                    level = 3,
                    money = 8500,
                    season = "Spring",
                    day = 28,
                    isCorrupted = false
                },
                new SaveFileInfo
                {
                    fileName = "save_003.json",
                    playerName = "Casey",
                    farmName = "Mountain View",
                    playTime = "25:17:09",
                    lastPlayed = System.DateTime.Now.AddHours(-2),
                    level = 8,
                    money = 42000,
                    season = "Autumn",
                    day = 7,
                    isCorrupted = false
                }
            };
            
            foreach (var save in mockSaves)
            {
                _saveFiles.Add(save);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        private List<SaveFileInfo> FilterAndSortSaves(List<SaveFileInfo> saves)
        {
            var filtered = saves.AsEnumerable();
            
            // Apply search filter
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                filtered = filtered.Where(s => 
                    s.playerName.ToLower().Contains(_searchQuery.ToLower()) ||
                    s.farmName.ToLower().Contains(_searchQuery.ToLower()));
            }
            
            // Apply sorting
            filtered = _currentSortType switch
            {
                SaveSortType.DateModified => _sortAscending ? 
                    filtered.OrderBy(s => s.lastPlayed) : 
                    filtered.OrderByDescending(s => s.lastPlayed),
                SaveSortType.DateCreated => _sortAscending ? 
                    filtered.OrderBy(s => s.dateCreated) : 
                    filtered.OrderByDescending(s => s.dateCreated),
                SaveSortType.PlayerName => _sortAscending ? 
                    filtered.OrderBy(s => s.playerName) : 
                    filtered.OrderByDescending(s => s.playerName),
                SaveSortType.FarmName => _sortAscending ? 
                    filtered.OrderBy(s => s.farmName) : 
                    filtered.OrderByDescending(s => s.farmName),
                SaveSortType.PlayTime => _sortAscending ? 
                    filtered.OrderBy(s => s.GetPlayTimeSeconds()) : 
                    filtered.OrderByDescending(s => s.GetPlayTimeSeconds()),
                SaveSortType.Level => _sortAscending ? 
                    filtered.OrderBy(s => s.level) : 
                    filtered.OrderByDescending(s => s.level),
                _ => filtered.OrderByDescending(s => s.lastPlayed)
            };
            
            return filtered.ToList();
        }

        private IEnumerator CreateSaveSlots(List<SaveFileInfo> saves)
        {
            for (int i = 0; i < saves.Count; i++)
            {
                var save = saves[i];
                var slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
                slotObj.SetActive(true);
                
                var saveSlot = slotObj.GetComponent<SaveSlotUI>();
                if (saveSlot != null)
                {
                    saveSlot.Initialize(save, this);
                    _saveSlots.Add(saveSlot);
                }
                
                // Animate slot in if enabled
                if (enableSlotAnimations)
                {
                    StartCoroutine(AnimateSlotIn(slotObj.transform, i * slotAnimationDelay));
                }
                
                yield return null;
            }
        }

        private IEnumerator AnimateSlotIn(Transform slotTransform, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            var rectTransform = slotTransform.GetComponent<RectTransform>();
            if (rectTransform == null) yield break;
            
            Vector3 startPos = rectTransform.localPosition + Vector3.right * 300f;
            Vector3 endPos = rectTransform.localPosition;
            
            rectTransform.localPosition = startPos;
            
            var canvasGroup = slotTransform.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = slotTransform.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                float curveValue = slideInCurve.Evaluate(progress);
                
                rectTransform.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
                canvasGroup.alpha = curveValue;
                
                yield return null;
            }
            
            rectTransform.localPosition = endPos;
            canvasGroup.alpha = 1f;
        }

        private void ClearSaveSlots()
        {
            foreach (var slot in _saveSlots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            
            _saveSlots.Clear();
            _selectedSlot = null;
            _selectedSave = null;
            
            UpdateActionButtons();
            ClearSaveDetails();
        }

        // Save slot selection
        public void SelectSaveSlot(SaveSlotUI slot)
        {
            PlaySlotSelectSound();
            
            // Deselect previous slot
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
            }
            
            // Select new slot
            _selectedSlot = slot;
            _selectedSave = slot.SaveInfo;
            _selectedSlot.SetSelected(true);
            
            // Update UI
            UpdateActionButtons();
            UpdateSaveDetails();
        }

        private void UpdateActionButtons()
        {
            bool hasSelection = _selectedSave != null;
            bool isValidSave = hasSelection && !_selectedSave.isCorrupted;
            
            if (loadGameButton != null)
                loadGameButton.interactable = isValidSave;
            
            if (deleteGameButton != null)
                deleteGameButton.interactable = hasSelection;
            
            if (duplicateGameButton != null)
                duplicateGameButton.interactable = isValidSave;
        }

        private void UpdateSaveDetails()
        {
            if (saveDetailsPanel == null || _selectedSave == null)
            {
                ClearSaveDetails();
                return;
            }
            
            saveDetailsPanel.SetActive(true);
            
            // Update save details
            if (savePlayerName != null)
                savePlayerName.text = _selectedSave.playerName;
            
            if (saveFarmName != null)
                saveFarmName.text = _selectedSave.farmName;
            
            if (savePlayTime != null)
                savePlayTime.text = $"Play Time: {_selectedSave.playTime}";
            
            if (saveDate != null)
                saveDate.text = $"Last Played: {_selectedSave.lastPlayed:yyyy-MM-dd HH:mm}";
            
            if (saveLevel != null)
                saveLevel.text = $"Level: {_selectedSave.level}";
            
            if (saveMoney != null)
                saveMoney.text = $"Money: ${_selectedSave.money:N0}";
            
            if (saveSeason != null)
                saveSeason.text = $"Season: {_selectedSave.season}";
            
            if (saveDay != null)
                saveDay.text = $"Day: {_selectedSave.day}";
            
            // Update screenshot
            if (saveScreenshot != null)
            {
                // Load screenshot if available
                if (_selectedSave.screenshot != null)
                {
                    saveScreenshot.sprite = _selectedSave.screenshot;
                }
                else
                {
                    saveScreenshot.sprite = defaultScreenshot;
                }
            }
        }

        private void ClearSaveDetails()
        {
            if (saveDetailsPanel != null)
            {
                saveDetailsPanel.SetActive(false);
            }
        }

        // Sorting and filtering
        private void OnSortTypeChanged(int index)
        {
            _currentSortType = (SaveSortType)index;
            RefreshSaveList();
        }

        private void ToggleSortOrder()
        {
            PlayButtonSound();
            _sortAscending = !_sortAscending;
            UpdateSortOrderDisplay();
            RefreshSaveList();
        }

        private void UpdateSortOrderDisplay()
        {
            if (sortOrderText != null)
            {
                sortOrderText.text = _sortAscending ? "↑" : "↓";
            }
        }

        private void OnSearchChanged(string searchQuery)
        {
            _searchQuery = searchQuery;
            RefreshSaveList();
        }

        // Game actions
        private void LoadSelectedGame()
        {
            if (_selectedSave == null || _selectedSave.isCorrupted) return;
            
            PlayLoadGameSound();
            StartCoroutine(LoadGameCoroutine());
        }

        private IEnumerator LoadGameCoroutine()
        {
            ShowLoadingState(true);
            
            if (loadingText != null)
                loadingText.text = "Loading game...";
            
            // Simulate loading progress
            for (float progress = 0f; progress <= 1f; progress += 0.1f)
            {
                if (loadingProgressBar != null)
                    loadingProgressBar.value = progress;
                
                yield return new WaitForSecondsRealtime(0.1f);
            }
            
            // 🔧 수정: SaveManager 사용을 임시로 주석 처리
            // Load the game
            // if (saveManager != null)
            // {
            //     saveManager.LoadGame(_selectedSave.fileName);
            // }
            
            // Load game scene
            SceneManager.LoadScene("GameScene");
        }

        private void ShowDeleteConfirmation()
        {
            if (_selectedSave == null) return;
            
            PlayButtonSound();
            ShowDeleteConfirmation(true);
            
            if (deleteConfirmationText != null)
            {
                deleteConfirmationText.text = $"Are you sure you want to delete the save file for '{_selectedSave.playerName} - {_selectedSave.farmName}'?\n\nThis action cannot be undone!";
            }
        }

        private void ShowDeleteConfirmation(bool show)
        {
            if (deleteConfirmationPanel != null)
            {
                deleteConfirmationPanel.SetActive(show);
            }
        }

        private void ConfirmDeleteGame()
        {
            if (_selectedSave == null) return;
            
            PlayDeleteSound();
            
            // 🔧 수정: SaveManager 사용을 임시로 주석 처리
            // Delete the save file
            // if (saveManager != null)
            // {
            //     saveManager.DeleteSave(_selectedSave.fileName);
            // }
            
            ShowDeleteConfirmation(false);
            RefreshSaveList();
        }

        private void CancelDeleteGame()
        {
            PlayButtonSound();
            ShowDeleteConfirmation(false);
        }

        private void DuplicateSelectedGame()
        {
            if (_selectedSave == null || _selectedSave.isCorrupted) return;
            
            PlayButtonSound();
            
            // 🔧 수정: SaveManager 사용을 임시로 주석 처리
            // Duplicate the save file
            // if (saveManager != null)
            // {
            //     saveManager.DuplicateSave(_selectedSave.fileName);
            // }
            
            RefreshSaveList();
        }

        private void CreateNewGame()
        {
            PlayButtonSound();
            
            var newGamePanel = FindObjectOfType<NewGamePanel>();
            if (newGamePanel != null)
            {
                // 🔧 수정: Hide() 대신 gameObject.SetActive(false) 사용
                gameObject.SetActive(false);
                newGamePanel.ShowPanel();
            }
        }

        // UI state management
        private void ShowLoadingState(bool show)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(show);
            }
        }

        private void ShowEmptyState(bool show)
        {
            if (emptyStatePanel != null)
            {
                emptyStatePanel.SetActive(show);
            }
        }

        // Navigation
        private void ReturnToMainMenu()
        {
            PlayButtonSound();
            
            var mainMenuPanel = FindObjectOfType<MainMenuPanel>();
            if (mainMenuPanel != null)
            {
                mainMenuPanel.ReturnToMainMenu();
            }
            
            // 🔧 수정: Hide() 대신 gameObject.SetActive(false) 사용
            gameObject.SetActive(false);
        }

        // Animation
        // 🔧 수정: Show() 메서드를 올바르게 구현
        public void ShowPanel()
        {
            gameObject.SetActive(true);
            RefreshSaveList();
        }

        // Audio helpers
        private void PlayButtonSound()
        {
            // 🔧 수정: AudioManager 사용을 임시로 주석 처리
            // if (audioManager != null && buttonClickSound != null)
            // {
            //     audioManager.PlaySFX(buttonClickSound);
            // }
            
            Debug.Log("Button sound would play here");
        }

        private void PlaySlotSelectSound()
        {
            // 🔧 수정: AudioManager 사용을 임시로 주석 처리
            // if (audioManager != null && slotSelectSound != null)
            // {
            //     audioManager.PlaySFX(slotSelectSound);
            // }
            
            Debug.Log("Slot select sound would play here");
        }

        private void PlayDeleteSound()
        {
            // 🔧 수정: AudioManager 사용을 임시로 주석 처리
            // if (audioManager != null && deleteSound != null)
            // {
            //     audioManager.PlaySFX(deleteSound);
            // }
            
            Debug.Log("Delete sound would play here");
        }

        private void PlayLoadGameSound()
        {
            // 🔧 수정: AudioManager 사용을 임시로 주석 처리
            // if (audioManager != null && loadGameSound != null)
            // {
            //     audioManager.PlaySFX(loadGameSound);
            // }
            
            Debug.Log("Load game sound would play here");
        }
    }

    // Helper component for individual save slots
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image background;
        [SerializeField] private Image screenshot;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI seasonText;
        [SerializeField] private GameObject corruptedOverlay;
        
        private SaveFileInfo _saveInfo;
        private LoadGamePanel _parentPanel;
        private Button _button;
        private bool _isSelected = false;

        private void Awake()
        {
            // Auto-find components
            if (background == null)
                background = GetComponent<Image>();
            
            _button = GetComponent<Button>();
            if (_button == null)
                _button = gameObject.AddComponent<Button>();
            
            // Find child components
            var texts = GetComponentsInChildren<TextMeshProUGUI>();
            var images = GetComponentsInChildren<Image>();
            
            // Auto-assign based on names
            foreach (var text in texts)
            {
                switch (text.name.ToLower())
                {
                    case "name": nameText = text; break;
                    case "time": timeText = text; break;
                    case "date": dateText = text; break;
                    case "level": levelText = text; break;
                    case "money": moneyText = text; break;
                    case "season": seasonText = text; break;
                }
            }
            
            foreach (var image in images)
            {
                if (image.name.ToLower().Contains("screenshot"))
                {
                    screenshot = image;
                    break;
                }
            }
        }

        public void Initialize(SaveFileInfo saveInfo, LoadGamePanel parentPanel)
        {
            _saveInfo = saveInfo;
            _parentPanel = parentPanel;
            
            UpdateDisplay();
            
            // Setup button click
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => _parentPanel.SelectSaveSlot(this));
            }
        }

        private void UpdateDisplay()
        {
            if (_saveInfo == null) return;
            
            // Update text components
            if (nameText != null)
                nameText.text = $"{_saveInfo.playerName} - {_saveInfo.farmName}";
            
            if (timeText != null)
                timeText.text = $"Play Time: {_saveInfo.playTime}";
            
            if (dateText != null)
                dateText.text = $"Last Played: {_saveInfo.lastPlayed:MM/dd/yyyy}";
            
            if (levelText != null)
                levelText.text = $"Level: {_saveInfo.level}";
            
            if (moneyText != null)
                moneyText.text = $"${_saveInfo.money:N0}";
            
            if (seasonText != null)
                seasonText.text = $"{_saveInfo.season}, Day {_saveInfo.day}";
            
            // Update screenshot
            if (screenshot != null && _saveInfo.screenshot != null)
            {
                screenshot.sprite = _saveInfo.screenshot;
            }
            
            // Show corrupted overlay if needed
            if (corruptedOverlay != null)
            {
                corruptedOverlay.SetActive(_saveInfo.isCorrupted);
            }
            
            // Update background color
            UpdateBackgroundColor();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateBackgroundColor();
        }

        private void UpdateBackgroundColor()
        {
            if (background == null || _parentPanel == null) return;
            
            Color targetColor;
            
            if (_saveInfo.isCorrupted)
            {
                targetColor = _parentPanel.corruptedSlotColor;
            }
            else if (_isSelected)
            {
                targetColor = _parentPanel.selectedSlotColor;
            }
            else
            {
                targetColor = _parentPanel.normalSlotColor;
            }
            
            background.color = targetColor;
        }

        public SaveFileInfo SaveInfo => _saveInfo;
        public bool IsSelected => _isSelected;
    }

    // Data structures
    [System.Serializable]
    public class SaveFileInfo
    {
        public string fileName;
        public string playerName;
        public string farmName;
        public string playTime;
        public System.DateTime lastPlayed;
        public System.DateTime dateCreated;
        public int level;
        public int money;
        public string season;
        public int day;
        public Sprite screenshot;
        public bool isCorrupted;
        
        public int GetPlayTimeSeconds()
        {
            // Convert playTime string (HH:MM:SS) to total seconds
            if (System.TimeSpan.TryParse(playTime, out var timeSpan))
            {
                return (int)timeSpan.TotalSeconds;
            }
            return 0;
        }
    }

    public enum SaveSortType
    {
        DateModified = 0,
        DateCreated = 1,
        PlayerName = 2,
        FarmName = 3,
        PlayTime = 4,
        Level = 5
    }
}