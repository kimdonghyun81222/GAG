using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _01.Scripts.Networking.Authentication;
using _01.Scripts.Networking.CloudSave;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Farming._01.Scripts.Farming.Economy;
using Unity.Services.CloudSave;
using UnityEngine;

namespace GrowAGarden.Networking._01.Scripts.Networking.CloudSave
{
    [Provide]
    public class CloudSaveManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Save Settings")]
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private bool saveOnApplicationPause = true;
        
        // Dependencies
        [Inject] private AuthenticationManager _authManager;
        [Inject] private EconomyManager _economyManager;
        
        // Save State
        private bool _isSaving = false;
        private bool _isLoading = false;
        private float _lastSaveTime = 0f;
        private PlayerSaveData _currentSaveData;
        
        // Properties
        public bool IsSaving => _isSaving;
        public bool IsLoading => _isLoading;
        public DateTime LastSaveTime { get; private set; }
        
        // Events
        public event Action<PlayerSaveData> OnGameLoaded;
        public event Action<PlayerSaveData> OnGameSaved;
        public event Action<string> OnSaveError;
        public event Action<string> OnLoadError;

        private void Start()
        {
            if (_authManager != null)
            {
                _authManager.OnSignedIn += HandlePlayerSignedIn;
                _authManager.OnSignedOut += HandlePlayerSignedOut;
            }
            
            if (enableAutoSave)
            {
                InvokeRepeating(nameof(AutoSave), autoSaveInterval, autoSaveInterval);
            }
        }
        
        [Provide]
        public CloudSaveManager ProvideCloudSaveManager() => this;

        private async void HandlePlayerSignedIn()
        {
            await LoadGameData();
        }

        private void HandlePlayerSignedOut()
        {
            // Save before signing out
            if (_currentSaveData != null)
            {
                _ = SaveGameData();
            }
        }

        public async Task<bool> SaveGameData()
        {
            if (_isSaving || !_authManager.IsSignedIn)
                return false;
            
            _isSaving = true;
            
            try
            {
                // Collect current game state
                PlayerSaveData saveData = CollectGameData();
                
                // Prepare data for cloud save - 수정된 부분
                var cloudData = new Dictionary<string, object>
                {
                    { "playerProgress", saveData.ToCloudData() },
                    { "saveVersion", saveData.saveVersion },
                    { "lastSaveTime", DateTime.UtcNow.ToBinary() }
                };
                
                // Save to cloud - 수정된 부분
                await CloudSaveService.Instance.Data.Player.SaveAsync(cloudData);
                
                _currentSaveData = saveData;
                LastSaveTime = DateTime.UtcNow;
                _lastSaveTime = Time.time;
                
                Debug.Log($"Game saved successfully at {LastSaveTime:yyyy-MM-dd HH:mm:ss}");
                OnGameSaved?.Invoke(saveData);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game: {ex.Message}");
                OnSaveError?.Invoke(ex.Message);
                return false;
            }
            finally
            {
                _isSaving = false;
            }
        }

        public async Task<bool> LoadGameData()
        {
            if (_isLoading || !_authManager.IsSignedIn)
                return false;
            
            _isLoading = true;
            
            try
            {
                // Load data from cloud - 수정된 부분
                var keys = new HashSet<string> { "playerProgress", "saveVersion", "lastSaveTime" };
                var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
                
                if (playerData.TryGetValue("playerProgress", out var progressData))
                {
                    // Parse save data
                    var cloudDataDict = progressData.Value.GetAs<Dictionary<string, object>>();
                    PlayerSaveData saveData = PlayerSaveData.FromCloudData(cloudDataDict);
                    
                    // Get save metadata
                    if (playerData.TryGetValue("lastSaveTime", out var saveTimeData))
                    {
                        long saveTimeBinary = Convert.ToInt64(saveTimeData.Value.GetAsString());
                        LastSaveTime = DateTime.FromBinary(saveTimeBinary);
                    }
                    
                    // Apply loaded data to game
                    ApplyLoadedData(saveData);
                    _currentSaveData = saveData;
                    
                    Debug.Log($"Game loaded successfully. Last save: {LastSaveTime:yyyy-MM-dd HH:mm:ss}");
                    OnGameLoaded?.Invoke(saveData);
                    
                    return true;
                }
                else
                {
                    // No save data found, create new game
                    Debug.Log("No save data found, starting new game");
                    CreateNewGameData();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game: {ex.Message}");
                OnLoadError?.Invoke(ex.Message);
                
                // Fallback to new game
                CreateNewGameData();
                return false;
            }
            finally
            {
                _isLoading = false;
            }
        }

        public async Task<bool> DeleteSaveData()
        {
            if (!_authManager.IsSignedIn)
                return false;
            
            try
            {
                // 수정된 부분 - 개별 키들을 삭제
                var keysToDelete = new List<string> { "playerProgress", "saveVersion", "lastSaveTime" };
                
                foreach (string key in keysToDelete)
                {
                    try
                    {
                        await CloudSaveService.Instance.Data.Player.DeleteAsync(key);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to delete key {key}: {ex.Message}");
                    }
                }
                
                Debug.Log("Save data deleted successfully");
                CreateNewGameData();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save data: {ex.Message}");
                OnSaveError?.Invoke(ex.Message);
                return false;
            }
        }

        private PlayerSaveData CollectGameData()
        {
            return new PlayerSaveData
            {
                saveVersion = PlayerSaveData.CURRENT_VERSION,
                playerId = _authManager.PlayerId,
                playerName = _authManager.PlayerName,
                
                // Economy data
                playerMoney = _economyManager?.GetPlayerMoney() ?? 0f,
                
                // Farm data - this would be collected from farm manager
                farmLevel = 1,
                unlockedAreas = new List<string> { "starter_field" },
                
                // Tools data - this would be collected from tool manager
                ownedTools = new List<ToolSaveData>(),
                
                // Crops data - this would be collected from field manager
                fieldPlots = new List<FieldPlotSaveData>(),
                
                // Statistics
                totalHarvests = 0,
                totalEarnings = 0,
                totalTimePlayed = Time.time - _lastSaveTime,
                
                // Achievements and unlocks
                achievements = new List<string>(),
                unlockedCrops = new List<string> { "basic_carrot", "basic_wheat" },
                
                // Settings
                gameSettings = new GameSettingsSaveData
                {
                    masterVolume = 1f,
                    musicVolume = 0.8f,
                    sfxVolume = 1f,
                    mouseSensitivity = 2f
                }
            };
        }

        private void ApplyLoadedData(PlayerSaveData saveData)
        {
            // Apply economy data
            if (_economyManager != null)
            {
                _economyManager.SetPlayerMoney(saveData.playerMoney);
            }
            
            // Apply tool data
            // This would integrate with ToolManager
            
            // Apply farm data
            // This would integrate with FarmManager
            
            // Apply crop data
            // This would integrate with FieldManager
            
            Debug.Log($"Applied save data for player: {saveData.playerName}");
        }

        private void CreateNewGameData()
        {
            _currentSaveData = new PlayerSaveData
            {
                saveVersion = PlayerSaveData.CURRENT_VERSION,
                playerId = _authManager.PlayerId,
                playerName = _authManager.PlayerName,
                playerMoney = 100f, // Starting money
                farmLevel = 1,
                unlockedAreas = new List<string> { "starter_field" },
                unlockedCrops = new List<string> { "basic_carrot", "basic_wheat" },
                gameSettings = new GameSettingsSaveData()
            };
            
            ApplyLoadedData(_currentSaveData);
            OnGameLoaded?.Invoke(_currentSaveData);
        }

        private async void AutoSave()
        {
            if (_authManager.IsSignedIn && !_isSaving && Time.time - _lastSaveTime > autoSaveInterval)
            {
                await SaveGameData();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (saveOnApplicationPause && pauseStatus && _authManager.IsSignedIn)
            {
                _ = SaveGameData();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (saveOnApplicationPause && !hasFocus && _authManager.IsSignedIn)
            {
                _ = SaveGameData();
            }
        }

        private void OnDestroy()
        {
            if (_authManager != null)
            {
                _authManager.OnSignedIn -= HandlePlayerSignedIn;
                _authManager.OnSignedOut -= HandlePlayerSignedOut;
            }
        }

        // Manual save/load triggers
        public void ManualSave()
        {
            _ = SaveGameData();
        }

        public void ManualLoad()
        {
            _ = LoadGameData();
        }
    }
}