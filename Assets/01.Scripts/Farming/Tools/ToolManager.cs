using System;
using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GrowAGarden.Farming._01.Scripts.Farming.Tools
{
    [Provide]
    public class ToolManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Tool Management")]
        [SerializeField] private List<ToolData> availableTools = new List<ToolData>();
        [SerializeField] private int maxEquippedTools = 8;
        [SerializeField] private bool allowToolSwitching = true;
        [SerializeField] private bool debugMode = false;
        
        [Header("Tool Spawning")]
        [SerializeField] private Transform toolSpawnParent;
        [SerializeField] private Vector3 toolSpawnOffset = Vector3.zero;
        
        [Header("Quick Access")]
        [SerializeField] private KeyCode[] quickAccessKeys = {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8
        };
        
        // Tool collections
        private List<FarmingTool> _ownedTools = new List<FarmingTool>();
        private Dictionary<ToolType, List<FarmingTool>> _toolsByType = new Dictionary<ToolType, List<FarmingTool>>();
        private FarmingTool _currentTool;
        private int _currentToolIndex = -1;
        
        // Tool slots (for quick access)
        private FarmingTool[] _equippedTools;
        private int _activeSlot = 0;
        
        // Properties
        public FarmingTool CurrentTool => _currentTool;
        public bool HasCurrentTool => _currentTool != null;
        public int CurrentToolIndex => _currentToolIndex;
        public int MaxEquippedTools => maxEquippedTools;
        public int OwnedToolCount => _ownedTools.Count;
        public int EquippedToolCount => _equippedTools != null ? Array.FindAll(_equippedTools, t => t != null).Length : 0;
        public int ActiveSlot => _activeSlot;
        
        // Events
        public event Action<FarmingTool> OnToolEquipped;
        public event Action<FarmingTool> OnToolUnequipped;
        public event Action<FarmingTool, FarmingTool> OnToolSwitched; // old, new
        public event Action<FarmingTool> OnToolAdded;
        public event Action<FarmingTool> OnToolRemoved;
        public event Action<int> OnActiveSlotChanged;

        private void Awake()
        {
            _equippedTools = new FarmingTool[maxEquippedTools];
            
            if (toolSpawnParent == null)
                toolSpawnParent = transform;
            
            InitializeToolTypeCollections();
        }

        private void Start()
        {
            // Create initial tools from available tool data
            foreach (var toolData in availableTools)
            {
                if (toolData != null)
                {
                    CreateTool(toolData);
                }
            }
        }

        private void Update()
        {
            HandleQuickAccessInput();
        }
        
        [Provide]
        public ToolManager ProvideToolManager() => this;

        private void InitializeToolTypeCollections()
        {
            foreach (ToolType toolType in Enum.GetValues(typeof(ToolType)))
            {
                _toolsByType[toolType] = new List<FarmingTool>();
            }
        }

        private void HandleQuickAccessInput()
        {
            if (!allowToolSwitching) return;
            
            for (int i = 0; i < quickAccessKeys.Length && i < maxEquippedTools; i++)
            {
                if (Input.GetKeyDown(quickAccessKeys[i]))
                {
                    SwitchToSlot(i);
                    break;
                }
            }
            
            // Mouse wheel scrolling
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.1f)
            {
                int direction = scroll > 0 ? 1 : -1;
                int newSlot = (_activeSlot + direction + maxEquippedTools) % maxEquippedTools;
                SwitchToSlot(newSlot);
            }
        }

        // Tool creation and management
        public FarmingTool CreateTool(ToolData toolData)
        {
            if (toolData == null) return null;
            
            GameObject toolObj;
            
            if (toolData.toolPrefab != null)
            {
                Vector3 spawnPos = toolSpawnParent.position + toolSpawnOffset;
                toolObj = Instantiate<GameObject>(toolData.toolPrefab, spawnPos, Quaternion.identity, toolSpawnParent);
            }
            else
            {
                // Create default tool object
                toolObj = new GameObject($"{toolData.toolName}_Tool");
                toolObj.transform.SetParent(toolSpawnParent);
                toolObj.transform.localPosition = toolSpawnOffset;
                
                // Add basic components
                var renderer = toolObj.AddComponent<MeshRenderer>();
                var meshFilter = toolObj.AddComponent<MeshFilter>();
                var collider = toolObj.AddComponent<BoxCollider>();
                
                // Set default mesh (cube for now)
                meshFilter.mesh = CreateDefaultToolMesh();
                
                if (toolData.toolMaterial != null)
                {
                    renderer.material = toolData.toolMaterial;
                }
            }
            
            // Add or get FarmingTool component
            FarmingTool tool = toolObj.GetComponent<FarmingTool>();
            if (tool == null)
            {
                tool = toolObj.AddComponent<FarmingTool>();
            }
            
            // Initialize tool
            tool.SetToolData(toolData);
            tool.Initialize();
            
            // Subscribe to tool events
            tool.OnToolEquipped += OnToolEquippedInternal;
            tool.OnToolUnequipped += OnToolUnequippedInternal;
            tool.OnToolBroken += OnToolBrokenInternal;
            
            // Add to collections
            AddToolToCollections(tool);
            
            OnToolAdded?.Invoke(tool);
            
            if (debugMode)
            {
                Debug.Log($"Created tool: {toolData.toolName}");
            }
            
            return tool;
        }

        private Mesh CreateDefaultToolMesh()
        {
            // Create a simple cube mesh for default tools
            var mesh = new Mesh();
            
            Vector3[] vertices = {
                new Vector3(-0.1f, -0.5f, -0.05f), new Vector3(0.1f, -0.5f, -0.05f),
                new Vector3(0.1f, -0.5f, 0.05f), new Vector3(-0.1f, -0.5f, 0.05f),
                new Vector3(-0.1f, 0.5f, -0.05f), new Vector3(0.1f, 0.5f, -0.05f),
                new Vector3(0.1f, 0.5f, 0.05f), new Vector3(-0.1f, 0.5f, 0.05f)
            };
            
            int[] triangles = {
                0, 1, 2, 0, 2, 3, // bottom
                4, 7, 6, 4, 6, 5, // top
                0, 4, 5, 0, 5, 1, // front
                2, 6, 7, 2, 7, 3, // back
                0, 3, 7, 0, 7, 4, // left
                1, 5, 6, 1, 6, 2  // right
            };
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            
            return mesh;
        }

        private void AddToolToCollections(FarmingTool tool)
        {
            if (tool?.ToolData == null) return;
            
            _ownedTools.Add(tool);
            
            ToolType toolType = tool.ToolData.toolType;
            if (_toolsByType.ContainsKey(toolType))
            {
                _toolsByType[toolType].Add(tool);
            }
        }

        private void RemoveToolFromCollections(FarmingTool tool)
        {
            if (tool?.ToolData == null) return;
            
            _ownedTools.Remove(tool);
            
            ToolType toolType = tool.ToolData.toolType;
            if (_toolsByType.ContainsKey(toolType))
            {
                _toolsByType[toolType].Remove(tool);
            }
            
            // Remove from equipped tools
            for (int i = 0; i < _equippedTools.Length; i++)
            {
                if (_equippedTools[i] == tool)
                {
                    _equippedTools[i] = null;
                    break;
                }
            }
            
            // Clear current tool if it's being removed
            if (_currentTool == tool)
            {
                _currentTool = null;
                _currentToolIndex = -1;
            }
        }

        // Tool equipment management
        public bool EquipToolToSlot(FarmingTool tool, int slot)
        {
            if (tool == null || slot < 0 || slot >= maxEquippedTools) return false;
            
            // Unequip current tool in slot
            if (_equippedTools[slot] != null)
            {
                UnequipTool(_equippedTools[slot]);
            }
            
            // Equip new tool
            _equippedTools[slot] = tool;
            tool.Equip();
            
            // If this is the active slot, set as current tool
            if (slot == _activeSlot)
            {
                SetCurrentTool(tool);
            }
            
            if (debugMode)
            {
                Debug.Log($"Equipped {tool.ToolData.toolName} to slot {slot}");
            }
            
            return true;
        }

        public bool UnequipTool(FarmingTool tool)
        {
            if (tool == null) return false;
            
            // Find tool in equipped slots
            for (int i = 0; i < _equippedTools.Length; i++)
            {
                if (_equippedTools[i] == tool)
                {
                    _equippedTools[i] = null;
                    tool.Unequip();
                    
                    // Clear current tool if it's being unequipped
                    if (_currentTool == tool)
                    {
                        _currentTool = null;
                        _currentToolIndex = -1;
                        
                        // Try to find another tool in the active slot
                        SwitchToSlot(_activeSlot);
                    }
                    
                    if (debugMode)
                    {
                        Debug.Log($"Unequipped {tool.ToolData.toolName} from slot {i}");
                    }
                    
                    return true;
                }
            }
            
            return false;
        }

        public void SwitchToSlot(int slot)
        {
            if (slot < 0 || slot >= maxEquippedTools) return;
            
            int oldSlot = _activeSlot;
            _activeSlot = slot;
            
            FarmingTool newTool = _equippedTools[slot];
            FarmingTool oldTool = _currentTool;
            
            SetCurrentTool(newTool);
            
            if (oldSlot != _activeSlot)
            {
                OnActiveSlotChanged?.Invoke(_activeSlot);
            }
            
            if (oldTool != newTool)
            {
                OnToolSwitched?.Invoke(oldTool, newTool);
                
                if (debugMode)
                {
                    string newToolName = newTool?.ToolData.toolName ?? "None";
                    Debug.Log($"Switched to slot {slot}: {newToolName}");
                }
            }
        }

        public void SwitchToNextTool()
        {
            int nextSlot = (_activeSlot + 1) % maxEquippedTools;
            SwitchToSlot(nextSlot);
        }

        public void SwitchToPreviousTool()
        {
            int prevSlot = (_activeSlot - 1 + maxEquippedTools) % maxEquippedTools;
            SwitchToSlot(prevSlot);
        }

        private void SetCurrentTool(FarmingTool tool)
        {
            _currentTool = tool;
            _currentToolIndex = tool != null ? _ownedTools.IndexOf(tool) : -1;
        }

        // Tool access methods
        public List<FarmingTool> GetOwnedTools()
        {
            return new List<FarmingTool>(_ownedTools);
        }

        public List<FarmingTool> GetToolsByType(ToolType toolType)
        {
            if (_toolsByType.TryGetValue(toolType, out List<FarmingTool> tools))
            {
                return new List<FarmingTool>(tools);
            }
            return new List<FarmingTool>();
        }

        public FarmingTool GetEquippedTool(int slot)
        {
            if (slot >= 0 && slot < maxEquippedTools)
            {
                return _equippedTools[slot];
            }
            return null;
        }

        public FarmingTool[] GetAllEquippedTools()
        {
            return (FarmingTool[])_equippedTools.Clone();
        }

        public FarmingTool GetBestToolForAction(ToolAction action)
        {
            FarmingTool bestTool = null;
            float bestEfficiency = 0f;
            
            foreach (var tool in _ownedTools)
            {
                if (tool?.ToolData != null && tool.ToolData.CanPerformAction(action) && !tool.IsBroken)
                {
                    if (tool.ToolData.efficiency > bestEfficiency)
                    {
                        bestEfficiency = tool.ToolData.efficiency;
                        bestTool = tool;
                    }
                }
            }
            
            return bestTool;
        }

        // Tool removal
        public void RemoveTool(FarmingTool tool)
        {
            if (tool == null) return;
            
            // Unsubscribe from events
            tool.OnToolEquipped -= OnToolEquippedInternal;
            tool.OnToolUnequipped -= OnToolUnequippedInternal;
            tool.OnToolBroken -= OnToolBrokenInternal;
            
            // Remove from collections
            RemoveToolFromCollections(tool);
            
            OnToolRemoved?.Invoke(tool);
            
            // Destroy the tool object - 🔧 이 부분에서 오류 수정
            if (tool != null && tool.gameObject != null)
            {
                GameObject toolGameObject = tool.gameObject;
                Destroy(toolGameObject);
            }
            
            if (debugMode)
            {
                Debug.Log($"Removed tool: {tool.ToolData?.toolName ?? "Unknown"}");
            }
        }

        // Event handlers
        private void OnToolEquippedInternal(FarmingTool tool)
        {
            OnToolEquipped?.Invoke(tool);
        }

        private void OnToolUnequippedInternal(FarmingTool tool)
        {
            OnToolUnequipped?.Invoke(tool);
        }

        private void OnToolBrokenInternal(FarmingTool tool)
        {
            if (debugMode)
            {
                Debug.Log($"Tool broken: {tool.ToolData.toolName}");
            }
            
            // Could auto-unequip broken tools
            if (tool.IsEquipped)
            {
                UnequipTool(tool);
            }
        }

        // Repair and maintenance
        public void RepairAllTools()
        {
            foreach (var tool in _ownedTools)
            {
                if (tool != null)
                {
                    tool.FullRepair();
                }
            }
            
            if (debugMode)
            {
                Debug.Log("Repaired all tools");
            }
        }

        public void RepairToolsOfType(ToolType toolType)
        {
            if (_toolsByType.TryGetValue(toolType, out List<FarmingTool> tools))
            {
                foreach (var tool in tools)
                {
                    if (tool != null)
                    {
                        tool.FullRepair();
                    }
                }
            }
        }

        public List<FarmingTool> GetBrokenTools()
        {
            List<FarmingTool> brokenTools = new List<FarmingTool>();
            
            foreach (var tool in _ownedTools)
            {
                if (tool != null && tool.IsBroken)
                {
                    brokenTools.Add(tool);
                }
            }
            
            return brokenTools;
        }

        // Settings
        public void SetMaxEquippedTools(int maxTools)
        {
            if (maxTools < 1) return;
            
            // If reducing size, unequip tools beyond new limit
            if (maxTools < maxEquippedTools)
            {
                for (int i = maxTools; i < maxEquippedTools; i++)
                {
                    if (_equippedTools[i] != null)
                    {
                        UnequipTool(_equippedTools[i]);
                    }
                }
            }
            
            // Resize array
            var newEquippedTools = new FarmingTool[maxTools];
            int copyCount = Mathf.Min(maxTools, maxEquippedTools);
            
            for (int i = 0; i < copyCount; i++)
            {
                newEquippedTools[i] = _equippedTools[i];
            }
            
            _equippedTools = newEquippedTools;
            maxEquippedTools = maxTools;
            
            // Adjust active slot if necessary
            if (_activeSlot >= maxTools)
            {
                SwitchToSlot(0);
            }
        }

        // Debug methods
        public void DEBUG_CreateAllToolTypes()
        {
            if (!debugMode) return;
            
            foreach (var toolData in availableTools)
            {
                if (toolData != null)
                {
                    CreateTool(toolData);
                }
            }
        }

        public void DEBUG_BreakAllTools()
        {
            if (!debugMode) return;
            
            foreach (var tool in _ownedTools)
            {
                if (tool != null)
                {
                    tool.DEBUG_BreakTool();
                }
            }
        }

        public void DEBUG_RepairAllTools()
        {
            if (!debugMode) return;
            RepairAllTools();
        }

        public void DEBUG_EquipAllTools()
        {
            if (!debugMode) return;
            
            for (int i = 0; i < Mathf.Min(_ownedTools.Count, maxEquippedTools); i++)
            {
                if (_ownedTools[i] != null)
                {
                    EquipToolToSlot(_ownedTools[i], i);
                }
            }
        }
    }
}