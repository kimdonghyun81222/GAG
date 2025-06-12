using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Farming._01.Scripts.Farming.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.HUD
{
    public class ToolHUD : MonoBehaviour
    {
        [Header("Tool Hotbar")]
        [SerializeField] private Transform hotbarContainer;
        [SerializeField] private GameObject toolSlotPrefab;
        [SerializeField] private int maxHotbarSlots = 8;
        
        [Header("Tool Information")]
        [SerializeField] private GameObject toolInfoPanel;
        [SerializeField] private TextMeshProUGUI toolNameText;
        [SerializeField] private TextMeshProUGUI toolDescriptionText;
        [SerializeField] private Image toolIconLarge;
        [SerializeField] private Slider toolDurabilityBar;
        [SerializeField] private TextMeshProUGUI durabilityText;
        
        [Header("Tool Stats")]
        [SerializeField] private Transform toolStatsContainer;
        [SerializeField] private GameObject statItemPrefab;
        
        [Header("Quick Actions")]
        [SerializeField] private Transform quickActionsContainer;
        [SerializeField] private Button repairButton;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button dropButton;
        
        [Header("Visual Settings")]
        [SerializeField] public Color selectedSlotColor = Color.yellow;
        [SerializeField] public Color normalSlotColor = Color.white;
        [SerializeField] public Color emptySlotColor = Color.gray;
        [SerializeField] public Color brokenToolColor = Color.red;
        
        [Header("Animation")]
        [SerializeField] private bool enableSlotAnimation = true;
        [SerializeField] private float selectionAnimationDuration = 0.2f;
        [SerializeField] private AnimationCurve selectionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Auto Hide")]
        [SerializeField] private bool autoHideToolInfo = true;
        [SerializeField] private float toolInfoHideDelay = 3f;
        
        // Dependencies
        [Inject] private ToolManager toolManager;
        
        // Tool slot management
        private List<ToolSlotHUD> _toolSlots = new List<ToolSlotHUD>();
        private int _currentSelectedSlot = 0;
        private FarmingTool _currentTool;
        
        // Tool info management
        private bool _toolInfoVisible = false;
        private Coroutine _hideToolInfoCoroutine;
        
        // Visual effects
        private Dictionary<int, Coroutine> _slotAnimations = new Dictionary<int, Coroutine>();

        private void Awake()
        {
            InitializeToolHUD();
        }

        private void Start()
        {
            CreateToolSlots();
            SetupQuickActions();
            RefreshToolSlots();
            HideToolInfo();
        }

        private void Update()
        {
            HandleToolInput();
            UpdateCurrentToolInfo();
        }

        private void InitializeToolHUD()
        {
            // Auto-find components if not assigned
            if (hotbarContainer == null)
            {
                var hotbarObj = transform.Find("Hotbar");
                if (hotbarObj != null)
                    hotbarContainer = hotbarObj;
            }
            
            if (toolInfoPanel == null)
            {
                var infoObj = transform.Find("ToolInfo");
                if (infoObj != null)
                    toolInfoPanel = infoObj.gameObject;
            }
            
            // Create default slot prefab if none exists
            if (toolSlotPrefab == null)
            {
                CreateDefaultSlotPrefab();
            }
            
            // Subscribe to tool manager events
            if (toolManager != null)
            {
                toolManager.OnActiveSlotChanged += OnActiveSlotChanged;
                toolManager.OnToolEquipped += OnToolEquipped;
                toolManager.OnToolUnequipped += OnToolUnequipped;
            }
        }

        private void CreateDefaultSlotPrefab()
        {
            var slotObj = new GameObject("ToolSlot");
            var rectTransform = slotObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(60f, 60f);
            
            // Background
            var background = slotObj.AddComponent<Image>();
            background.color = normalSlotColor;
            background.sprite = CreateSlotSprite();
            
            // Tool icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform, false);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(5f, 5f);
            iconRect.offsetMax = new Vector2(-5f, -5f);
            
            var icon = iconObj.AddComponent<Image>();
            icon.color = Color.white;
            
            // Slot number text
            var numberObj = new GameObject("Number");
            numberObj.transform.SetParent(slotObj.transform, false);
            var numberRect = numberObj.AddComponent<RectTransform>();
            numberRect.anchorMin = new Vector2(0f, 0f);
            numberRect.anchorMax = new Vector2(0f, 0f);
            numberRect.anchoredPosition = new Vector2(5f, 5f);
            numberRect.sizeDelta = new Vector2(20f, 20f);
            
            var numberText = numberObj.AddComponent<TextMeshProUGUI>();
            numberText.text = "1";
            numberText.fontSize = 12f;
            numberText.color = Color.white;
            numberText.alignment = TextAlignmentOptions.Center;
            
            // Durability bar
            var durabilityObj = new GameObject("Durability");
            durabilityObj.transform.SetParent(slotObj.transform, false);
            var durabilityRect = durabilityObj.AddComponent<RectTransform>();
            durabilityRect.anchorMin = new Vector2(0f, 0f);
            durabilityRect.anchorMax = new Vector2(1f, 0f);
            durabilityRect.anchoredPosition = new Vector2(0f, 3f);
            durabilityRect.sizeDelta = new Vector2(-6f, 4f);
            
            var durabilityBar = durabilityObj.AddComponent<Slider>();
            durabilityBar.value = 1f;
            
            // Add ToolSlotHUD component
            slotObj.AddComponent<ToolSlotHUD>();
            
            toolSlotPrefab = slotObj;
            toolSlotPrefab.SetActive(false);
        }

        private Sprite CreateSlotSprite()
        {
            // Create a simple square sprite for the slot background
            var texture = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        private void CreateToolSlots()
        {
            if (hotbarContainer == null) return;
            
            // Clear existing slots
            foreach (Transform child in hotbarContainer)
            {
                DestroyImmediate(child.gameObject);
            }
            _toolSlots.Clear();
            
            // Create new slots
            for (int i = 0; i < maxHotbarSlots; i++)
            {
                var slotObj = Instantiate(toolSlotPrefab, hotbarContainer);
                slotObj.SetActive(true);
                
                var toolSlot = slotObj.GetComponent<ToolSlotHUD>();
                if (toolSlot == null)
                {
                    toolSlot = slotObj.AddComponent<ToolSlotHUD>();
                }
                
                toolSlot.Initialize(i, this);
                _toolSlots.Add(toolSlot);
                
                // Set slot number
                var numberText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
                if (numberText != null)
                {
                    numberText.text = (i + 1).ToString();
                }
            }
        }

        private void SetupQuickActions()
        {
            if (repairButton != null)
            {
                repairButton.onClick.RemoveAllListeners();
                repairButton.onClick.AddListener(RepairCurrentTool);
            }
            
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(UpgradeCurrentTool);
            }
            
            if (dropButton != null)
            {
                dropButton.onClick.RemoveAllListeners();
                dropButton.onClick.AddListener(DropCurrentTool);
            }
        }

        private void HandleToolInput()
        {
            // Number key input for tool selection
            for (int i = 0; i < maxHotbarSlots && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectToolSlot(i);
                }
            }
            
            // Mouse wheel for tool cycling
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.1f)
            {
                int direction = scroll > 0 ? 1 : -1;
                int newSlot = (_currentSelectedSlot + direction + maxHotbarSlots) % maxHotbarSlots;
                SelectToolSlot(newSlot);
            }
            
            // Tool info toggle
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleToolInfo();
            }
        }

        private void UpdateCurrentToolInfo()
        {
            if (_currentTool != null && _toolInfoVisible)
            {
                UpdateToolInfoDisplay(_currentTool);
            }
        }

        // Tool slot management
        public void RefreshToolSlots()
        {
            if (toolManager == null) return;
            
            var equippedTools = toolManager.GetAllEquippedTools();
            
            for (int i = 0; i < _toolSlots.Count; i++)
            {
                var tool = i < equippedTools.Length ? equippedTools[i] : null;
                _toolSlots[i].SetTool(tool);
                _toolSlots[i].SetSelected(i == _currentSelectedSlot);
            }
            
            // Update current tool reference
            if (_currentSelectedSlot < equippedTools.Length)
            {
                _currentTool = equippedTools[_currentSelectedSlot];
            }
            else
            {
                _currentTool = null;
            }
        }

        public void SelectToolSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _toolSlots.Count) return;
            
            int previousSlot = _currentSelectedSlot;
            _currentSelectedSlot = slotIndex;
            
            // Update visual selection
            for (int i = 0; i < _toolSlots.Count; i++)
            {
                _toolSlots[i].SetSelected(i == slotIndex);
            }
            
            // Animate slot selection
            if (enableSlotAnimation)
            {
                AnimateSlotSelection(previousSlot, slotIndex);
            }
            
            // Update tool manager
            if (toolManager != null)
            {
                toolManager.SwitchToSlot(slotIndex);
            }
            
            // Show tool info
            if (_currentTool != null)
            {
                ShowToolInfo();
            }
            else
            {
                HideToolInfo();
            }
        }

        private void AnimateSlotSelection(int fromSlot, int toSlot)
        {
            // Stop previous animations
            if (_slotAnimations.ContainsKey(fromSlot))
            {
                StopCoroutine(_slotAnimations[fromSlot]);
                _slotAnimations.Remove(fromSlot);
            }
            
            if (_slotAnimations.ContainsKey(toSlot))
            {
                StopCoroutine(_slotAnimations[toSlot]);
                _slotAnimations.Remove(toSlot);
            }
            
            // Start new animations
            if (fromSlot < _toolSlots.Count)
            {
                _slotAnimations[fromSlot] = StartCoroutine(AnimateSlot(_toolSlots[fromSlot], false));
            }
            
            if (toSlot < _toolSlots.Count)
            {
                _slotAnimations[toSlot] = StartCoroutine(AnimateSlot(_toolSlots[toSlot], true));
            }
        }

        private System.Collections.IEnumerator AnimateSlot(ToolSlotHUD slot, bool selected)
        {
            var rectTransform = slot.GetComponent<RectTransform>();
            if (rectTransform == null) yield break;
            
            Vector3 startScale = rectTransform.localScale;
            Vector3 targetScale = selected ? Vector3.one * 1.1f : Vector3.one;
            
            float elapsedTime = 0f;
            while (elapsedTime < selectionAnimationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / selectionAnimationDuration;
                float curveValue = selectionCurve.Evaluate(progress);
                
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
                
                yield return null;
            }
            
            rectTransform.localScale = targetScale;
        }

        // Tool info display
        private void ShowToolInfo()
        {
            if (toolInfoPanel == null || _currentTool == null) return;
            
            _toolInfoVisible = true;
            toolInfoPanel.SetActive(true);
            UpdateToolInfoDisplay(_currentTool);
            
            // Cancel auto-hide coroutine
            if (_hideToolInfoCoroutine != null)
            {
                StopCoroutine(_hideToolInfoCoroutine);
            }
            
            // Start auto-hide timer
            if (autoHideToolInfo)
            {
                _hideToolInfoCoroutine = StartCoroutine(AutoHideToolInfo());
            }
        }

        private void HideToolInfo()
        {
            if (toolInfoPanel == null) return;
            
            _toolInfoVisible = false;
            toolInfoPanel.SetActive(false);
            
            if (_hideToolInfoCoroutine != null)
            {
                StopCoroutine(_hideToolInfoCoroutine);
                _hideToolInfoCoroutine = null;
            }
        }

        private void ToggleToolInfo()
        {
            if (_toolInfoVisible)
            {
                HideToolInfo();
            }
            else if (_currentTool != null)
            {
                ShowToolInfo();
            }
        }

        private System.Collections.IEnumerator AutoHideToolInfo()
        {
            yield return new WaitForSeconds(toolInfoHideDelay);
            HideToolInfo();
        }

        private void UpdateToolInfoDisplay(FarmingTool tool)
        {
            if (tool?.ToolData == null) return;
            
            // Update basic info
            if (toolNameText != null)
                toolNameText.text = tool.ToolData.toolName;
            
            if (toolDescriptionText != null)
                toolDescriptionText.text = tool.ToolData.description;
            
            if (toolIconLarge != null)
                toolIconLarge.sprite = tool.ToolData.toolIcon;
            
            // 🔧 수정: CurrentDurability와 MaxDurability를 안전하게 처리
            if (toolDurabilityBar != null)
            {
                float currentDurability = GetToolCurrentDurability(tool);
                float maxDurability = GetToolMaxDurability(tool);
                float durabilityPercent = maxDurability > 0 ? currentDurability / maxDurability : 1f;
                toolDurabilityBar.value = durabilityPercent;
            }
            
            if (durabilityText != null)
            {
                float currentDurability = GetToolCurrentDurability(tool);
                float maxDurability = GetToolMaxDurability(tool);
                durabilityText.text = $"{currentDurability:F0}/{maxDurability:F0}";
            }
            
            // Update tool stats
            UpdateToolStats(tool);
            
            // Update quick action buttons
            UpdateQuickActionButtons(tool);
        }

        // 🔧 수정: 안전한 durability 접근 메서드들
        private float GetToolCurrentDurability(FarmingTool tool)
        {
            // FarmingTool에 CurrentDurability 속성이 없으므로 임시값 반환
            return 100f; // 나중에 실제 속성으로 교체
        }

        private float GetToolMaxDurability(FarmingTool tool)
        {
            // FarmingTool에 MaxDurability 속성이 없으므로 임시값 반환
            return 100f; // 나중에 실제 속성으로 교체
        }

        private bool IsToolBroken(FarmingTool tool)
        {
            // FarmingTool에 IsBroken 속성이 없으므로 임시값 반환
            return GetToolCurrentDurability(tool) <= 0f;
        }

        private void UpdateToolStats(FarmingTool tool)
        {
            if (toolStatsContainer == null) return;
            
            // Clear existing stats
            foreach (Transform child in toolStatsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Add tool stats
            if (tool.ToolData != null)
            {
                CreateStatItem("Efficiency", $"{tool.ToolData.efficiency:F1}");
                CreateStatItem("Speed", $"{tool.ToolData.useSpeed:F1}");
                CreateStatItem("Energy Cost", $"{tool.ToolData.energyCost:F0}");
                
                // 🔧 수정: 존재하지 않는 속성들은 주석 처리
                // CreateStatItem("Durability Loss", $"{tool.ToolData.durabilityLossPerUse:F1}");
                // CreateStatItem("Damage", $"{tool.ToolData.damage:F0}");
                
                // 임시 스탯들
                CreateStatItem("Durability Loss", "1.0");
                CreateStatItem("Damage", "10");
                
                if (tool.ToolData.range > 0)
                {
                    CreateStatItem("Range", $"{tool.ToolData.range:F1}m");
                }
            }
        }

        private void CreateStatItem(string statName, string statValue)
        {
            GameObject statObj;
            
            if (statItemPrefab != null)
            {
                statObj = Instantiate(statItemPrefab, toolStatsContainer);
            }
            else
            {
                statObj = new GameObject($"Stat_{statName}");
                statObj.transform.SetParent(toolStatsContainer, false);
                
                var layout = statObj.AddComponent<HorizontalLayoutGroup>();
                layout.childForceExpandWidth = true;
                
                // Stat name
                var nameObj = new GameObject("Name");
                nameObj.transform.SetParent(statObj.transform, false);
                var nameText = nameObj.AddComponent<TextMeshProUGUI>();
                nameText.text = statName + ":";
                nameText.fontSize = 12f;
                
                // Stat value
                var valueObj = new GameObject("Value");
                valueObj.transform.SetParent(statObj.transform, false);
                var valueText = valueObj.AddComponent<TextMeshProUGUI>();
                valueText.text = statValue;
                valueText.fontSize = 12f;
                valueText.alignment = TextAlignmentOptions.Right;
            }
        }

        private void UpdateQuickActionButtons(FarmingTool tool)
        {
            if (repairButton != null)
            {
                float currentDurability = GetToolCurrentDurability(tool);
                float maxDurability = GetToolMaxDurability(tool);
                repairButton.interactable = currentDurability < maxDurability;
            }
            
            if (upgradeButton != null)
            {
                // 🔧 수정: CanBeUpgraded 메서드가 없으므로 임시 처리
                upgradeButton.interactable = true; // 나중에 실제 로직으로 교체
            }
            
            if (dropButton != null)
            {
                dropButton.interactable = true;
            }
        }

        // Quick actions
        private void RepairCurrentTool()
        {
            if (_currentTool != null && toolManager != null)
            {
                // 🔧 수정: Repair 메서드가 없으므로 임시 처리
                // _currentTool.Repair(100f);
                Debug.Log($"Repairing tool: {_currentTool.ToolData?.toolName}");
                ShowToolInfo(); // Refresh display
            }
        }

        private void UpgradeCurrentTool()
        {
            if (_currentTool != null && toolManager != null)
            {
                // This would connect to an upgrade system
                Debug.Log($"Upgrading tool: {_currentTool.ToolData?.toolName}");
            }
        }

        private void DropCurrentTool()
        {
            if (_currentTool != null && toolManager != null)
            {
                toolManager.UnequipTool(_currentTool);
                HideToolInfo();
            }
        }

        // Event handlers
        private void OnActiveSlotChanged(int slotIndex)
        {
            if (slotIndex != _currentSelectedSlot)
            {
                SelectToolSlot(slotIndex);
            }
        }

        private void OnToolEquipped(FarmingTool tool)
        {
            RefreshToolSlots();
        }

        private void OnToolUnequipped(FarmingTool tool)
        {
            RefreshToolSlots();
            
            if (_currentTool == tool)
            {
                HideToolInfo();
            }
        }

        // Public interface
        public void SetSelectedSlot(int slotIndex)
        {
            SelectToolSlot(slotIndex);
        }

        public int GetSelectedSlot()
        {
            return _currentSelectedSlot;
        }

        public FarmingTool GetCurrentTool()
        {
            return _currentTool;
        }

        public bool IsToolInfoVisible()
        {
            return _toolInfoVisible;
        }

        public void SetToolInfoVisible(bool visible)
        {
            if (visible && _currentTool != null)
            {
                ShowToolInfo();
            }
            else
            {
                HideToolInfo();
            }
        }

        // Settings
        public void SetMaxHotbarSlots(int maxSlots)
        {
            maxHotbarSlots = Mathf.Clamp(maxSlots, 1, 12);
            CreateToolSlots();
            RefreshToolSlots();
        }

        public void SetAutoHideToolInfo(bool autoHide)
        {
            autoHideToolInfo = autoHide;
        }

        public void SetToolInfoHideDelay(float delay)
        {
            toolInfoHideDelay = Mathf.Max(0.5f, delay);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (toolManager != null)
            {
                toolManager.OnActiveSlotChanged -= OnActiveSlotChanged;
                toolManager.OnToolEquipped -= OnToolEquipped;
                toolManager.OnToolUnequipped -= OnToolUnequipped;
            }
            
            // Stop all coroutines
            StopAllCoroutines();
        }
    }

    // Helper component for individual tool slots
    public class ToolSlotHUD : MonoBehaviour
    {
        [Header("Slot Components")]
        [SerializeField] private Image background;
        [SerializeField] private Image toolIcon;
        [SerializeField] private TextMeshProUGUI slotNumber;
        [SerializeField] private Slider durabilityBar;
        [SerializeField] private Image selectionBorder;
        
        private int _slotIndex;
        private FarmingTool _tool;
        private ToolHUD _parentHUD;
        private bool _isSelected;

        private void Awake()
        {
            // Auto-find components
            if (background == null)
                background = GetComponent<Image>();
            
            if (toolIcon == null)
            {
                var icons = GetComponentsInChildren<Image>();
                foreach (var icon in icons)
                {
                    if (icon != background && icon.name.Contains("Icon"))
                    {
                        toolIcon = icon;
                        break;
                    }
                }
            }
            
            if (slotNumber == null)
                slotNumber = GetComponentInChildren<TextMeshProUGUI>();
            
            if (durabilityBar == null)
                durabilityBar = GetComponentInChildren<Slider>();
        }

        public void Initialize(int index, ToolHUD parentHUD)
        {
            _slotIndex = index;
            _parentHUD = parentHUD;
            
            if (slotNumber != null)
            {
                slotNumber.text = (index + 1).ToString();
            }
            
            // Add click handler
            var button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _parentHUD?.SelectToolSlot(_slotIndex));
        }

        public void SetTool(FarmingTool tool)
        {
            _tool = tool;
            UpdateVisuals();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // Update tool icon
            if (toolIcon != null)
            {
                if (_tool?.ToolData?.toolIcon != null)
                {
                    toolIcon.sprite = _tool.ToolData.toolIcon;
                    toolIcon.color = Color.white;
                    toolIcon.gameObject.SetActive(true);
                }
                else
                {
                    toolIcon.gameObject.SetActive(false);
                }
            }
            
            // Update background color
            if (background != null && _parentHUD != null)
            {
                if (_tool == null)
                {
                    background.color = _parentHUD.emptySlotColor;
                }
                else if (IsToolBroken(_tool))
                {
                    background.color = _parentHUD.brokenToolColor;
                }
                else if (_isSelected)
                {
                    background.color = _parentHUD.selectedSlotColor;
                }
                else
                {
                    background.color = _parentHUD.normalSlotColor;
                }
            }
            
            // Update durability bar
            if (durabilityBar != null)
            {
                if (_tool != null)
                {
                    float currentDurability = GetToolCurrentDurability(_tool);
                    float maxDurability = GetToolMaxDurability(_tool);
                    float durabilityPercent = maxDurability > 0 ? currentDurability / maxDurability : 1f;
                    durabilityBar.value = durabilityPercent;
                    durabilityBar.gameObject.SetActive(true);
                }
                else
                {
                    durabilityBar.gameObject.SetActive(false);
                }
            }
            
            // Update selection border
            if (selectionBorder != null)
            {
                selectionBorder.gameObject.SetActive(_isSelected);
            }
        }

        // 🔧 수정: 안전한 durability 접근 메서드들
        private float GetToolCurrentDurability(FarmingTool tool)
        {
            return 100f; // 임시값
        }

        private float GetToolMaxDurability(FarmingTool tool)
        {
            return 100f; // 임시값
        }

        private bool IsToolBroken(FarmingTool tool)
        {
            return GetToolCurrentDurability(tool) <= 0f;
        }

        public int SlotIndex => _slotIndex;
        public FarmingTool Tool => _tool;
        public bool IsSelected => _isSelected;
        public bool HasTool => _tool != null;
    }
}