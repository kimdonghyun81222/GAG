using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _01.Scripts.Networking.Authentication;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Farming._01.Scripts.Farming.Economy;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace GrowAGarden.Networking._01.Scripts.Networking.Leaderboards
{
    [Provide]
    public class LeaderboardsManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Leaderboard Settings")]
        [SerializeField] private string moneyLeaderboardId = "money_leaderboard";
        [SerializeField] private string harvestLeaderboardId = "harvest_leaderboard";
        [SerializeField] private string levelLeaderboardId = "level_leaderboard";
        [SerializeField] private int entriesPerPage = 10;
        [SerializeField] private bool enableAutoSubmit = true;
        [SerializeField] private float submitCooldown = 30f;
        
        // Dependencies
        [Inject] private AuthenticationManager _authManager;
        [Inject] private EconomyManager _economyManager;
        
        // Leaderboard State
        private Dictionary<string, LeaderboardData> _leaderboardCache = new Dictionary<string, LeaderboardData>();
        private Dictionary<string, float> _lastSubmitTime = new Dictionary<string, float>();
        private bool _isInitialized = false;
        
        // Properties
        public bool IsInitialized => _isInitialized;
        
        // Events - 올바른 Unity API 타입 사용
        public event Action<string, LeaderboardScoresPage> OnLeaderboardLoaded;
        public event Action<string, LeaderboardEntry> OnScoreSubmitted;
        public event Action<string, string> OnLeaderboardError;

        private void Start()
        {
            if (_authManager != null)
            {
                _authManager.OnSignedIn += HandlePlayerSignedIn;
                _authManager.OnSignedOut += HandlePlayerSignedOut;
            }
            
            if (_economyManager != null && enableAutoSubmit)
            {
                _economyManager.OnMoneyChanged += HandleMoneyChanged;
            }
        }
        
        [Provide]
        public LeaderboardsManager ProvideLeaderboardsManager() => this;

        private async void HandlePlayerSignedIn()
        {
            await InitializeLeaderboards();
        }

        private void HandlePlayerSignedOut()
        {
            _isInitialized = false;
            _leaderboardCache.Clear();
        }

        private async Task InitializeLeaderboards()
        {
            if (_isInitialized || !_authManager.IsSignedIn)
                return;
            
            try
            {
                // Validate leaderboards exist by attempting to load them
                await LoadLeaderboard(moneyLeaderboardId, 1, 1);
                
                _isInitialized = true;
                Debug.Log("Leaderboards initialized successfully");
                
                // Submit initial scores
                if (enableAutoSubmit)
                {
                    await SubmitInitialScores();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize leaderboards: {ex.Message}");
                OnLeaderboardError?.Invoke("initialization", ex.Message);
            }
        }

        // 수정된 부분: Unity API가 실제로 반환하는 타입 사용
        public async Task<LeaderboardScoresPage> LoadLeaderboard(string leaderboardId, int offset = 0, int limit = -1)
        {
            if (!_authManager.IsSignedIn)
            {
                OnLeaderboardError?.Invoke(leaderboardId, "Not signed in");
                return null;
            }
            
            if (limit < 0) limit = entriesPerPage;
            
            try
            {
                var options = new GetScoresOptions
                {
                    Offset = offset,
                    Limit = limit,
                    IncludeMetadata = true
                };
                
                // Unity API: GetScoresAsync는 실제로 LeaderboardScoresPage를 반환
                var scoresPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, options);
                
                // Cache the results
                _leaderboardCache[leaderboardId] = new LeaderboardData
                {
                    leaderboardId = leaderboardId,
                    scoresPage = scoresPage, // LeaderboardScoresPage 저장
                    lastUpdated = DateTime.UtcNow
                };
                
                OnLeaderboardLoaded?.Invoke(leaderboardId, scoresPage);
                Debug.Log($"Loaded leaderboard {leaderboardId}: {scoresPage.Results.Count} entries");
                
                return scoresPage;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load leaderboard {leaderboardId}: {ex.Message}");
                OnLeaderboardError?.Invoke(leaderboardId, ex.Message);
                return null;
            }
        }

        public async Task<LeaderboardEntry> SubmitScore(string leaderboardId, double score, Dictionary<string, string> metadata = null)
        {
            if (!_authManager.IsSignedIn)
            {
                OnLeaderboardError?.Invoke(leaderboardId, "Not signed in");
                return null;
            }
            
            // Check cooldown
            if (_lastSubmitTime.TryGetValue(leaderboardId, out float lastSubmit))
            {
                if (Time.time - lastSubmit < submitCooldown)
                {
                    Debug.LogWarning($"Submit cooldown active for {leaderboardId}");
                    return null;
                }
            }
            
            try
            {
                var options = new AddPlayerScoreOptions();
                if (metadata != null && metadata.Count > 0)
                {
                    options.Metadata = metadata;
                }
                
                var entry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score, options);
                
                _lastSubmitTime[leaderboardId] = Time.time;
                
                OnScoreSubmitted?.Invoke(leaderboardId, entry);
                Debug.Log($"Submitted score {score} to {leaderboardId}. Rank: {entry.Rank + 1}");
                
                // Invalidate cache for this leaderboard
                if (_leaderboardCache.ContainsKey(leaderboardId))
                {
                    _leaderboardCache.Remove(leaderboardId);
                }
                
                return entry;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to submit score to {leaderboardId}: {ex.Message}");
                OnLeaderboardError?.Invoke(leaderboardId, ex.Message);
                return null;
            }
        }

        public async Task<LeaderboardEntry> GetPlayerScore(string leaderboardId)
        {
            if (!_authManager.IsSignedIn)
                return null;
            
            try
            {
                var entry = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
                return entry;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not get player score from {leaderboardId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<LeaderboardEntry>> GetPlayerScores(List<string> leaderboardIds)
        {
            if (!_authManager.IsSignedIn)
                return new List<LeaderboardEntry>();
            
            var scores = new List<LeaderboardEntry>();
            
            foreach (string leaderboardId in leaderboardIds)
            {
                var score = await GetPlayerScore(leaderboardId);
                if (score != null)
                {
                    scores.Add(score);
                }
            }
            
            return scores;
        }

        // 수정된 부분: Unity API가 실제로 반환하는 타입 사용
        public async Task<LeaderboardScores> GetPlayerRange(string leaderboardId, int rangeLimit = 5)
        {
            if (!_authManager.IsSignedIn)
                return null;
            
            try
            {
                var options = new GetPlayerRangeOptions
                {
                    RangeLimit = rangeLimit,
                    IncludeMetadata = true
                };
                
                // Unity API: GetPlayerRangeAsync는 실제로 LeaderboardScoresPage를 반환
                var scoresPage = await LeaderboardsService.Instance.GetPlayerRangeAsync(leaderboardId, options);
                return scoresPage;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get player range from {leaderboardId}: {ex.Message}");
                OnLeaderboardError?.Invoke(leaderboardId, ex.Message);
                return null;
            }
        }

        // Auto-submit handlers
        private async void HandleMoneyChanged(float oldAmount, float newAmount)
        {
            if (enableAutoSubmit && _isInitialized && newAmount > oldAmount)
            {
                await SubmitMoneyScore(newAmount);
            }
        }

        public async Task SubmitMoneyScore(float moneyAmount)
        {
            var metadata = new Dictionary<string, string>
            {
                { "playerName", _authManager.PlayerName },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            
            await SubmitScore(moneyLeaderboardId, moneyAmount, metadata);
        }

        public async Task SubmitHarvestScore(int harvestCount)
        {
            var metadata = new Dictionary<string, string>
            {
                { "playerName", _authManager.PlayerName },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            
            await SubmitScore(harvestLeaderboardId, harvestCount, metadata);
        }

        public async Task SubmitLevelScore(int playerLevel)
        {
            var metadata = new Dictionary<string, string>
            {
                { "playerName", _authManager.PlayerName },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };
            
            await SubmitScore(levelLeaderboardId, playerLevel, metadata);
        }

        private async Task SubmitInitialScores()
        {
            // Submit current money score
            if (_economyManager != null)
            {
                await SubmitMoneyScore(_economyManager.GetPlayerMoney());
            }
        }

        public LeaderboardData GetCachedLeaderboard(string leaderboardId)
        {
            return _leaderboardCache.GetValueOrDefault(leaderboardId);
        }

        public bool IsCacheValid(string leaderboardId, TimeSpan maxAge)
        {
            if (!_leaderboardCache.TryGetValue(leaderboardId, out LeaderboardData data))
                return false;
            
            return DateTime.UtcNow - data.lastUpdated < maxAge;
        }

        // Utility methods for UI
        public async Task<List<LeaderboardSummary>> GetAllLeaderboardSummaries()
        {
            var summaries = new List<LeaderboardSummary>();
            
            var leaderboardIds = new[] { moneyLeaderboardId, harvestLeaderboardId, levelLeaderboardId };
            
            foreach (string id in leaderboardIds)
            {
                var playerScore = await GetPlayerScore(id);
                var topScores = await LoadLeaderboard(id, 0, 3);
                
                summaries.Add(new LeaderboardSummary
                {
                    leaderboardId = id,
                    leaderboardName = GetLeaderboardDisplayName(id),
                    playerRank = playerScore?.Rank + 1 ?? 0,
                    playerScore = playerScore?.Score ?? 0,
                    topEntries = topScores?.Results ?? new List<LeaderboardEntry>()
                });
            }
            
            return summaries;
        }

        private string GetLeaderboardDisplayName(string leaderboardId)
        {
            return leaderboardId switch
            {
                var id when id == moneyLeaderboardId => "Richest Farmers",
                var id when id == harvestLeaderboardId => "Most Harvests",
                var id when id == levelLeaderboardId => "Highest Level",
                _ => leaderboardId
            };
        }

        // 편의 메서드: LeaderboardScoresPage에서 Results에 쉽게 접근
        public List<LeaderboardEntry> GetResultsFromPage(LeaderboardScoresPage scoresPage)
        {
            return scoresPage?.Results ?? new List<LeaderboardEntry>();
        }

        // 편의 메서드: 캐시된 리더보드의 Results 가져오기
        public List<LeaderboardEntry> GetCachedResults(string leaderboardId)
        {
            var cached = GetCachedLeaderboard(leaderboardId);
            return cached?.scoresPage?.Results ?? new List<LeaderboardEntry>();
        }

        private void OnDestroy()
        {
            if (_authManager != null)
            {
                _authManager.OnSignedIn -= HandlePlayerSignedIn;
                _authManager.OnSignedOut -= HandlePlayerSignedOut;
            }
            
            if (_economyManager != null)
            {
                _economyManager.OnMoneyChanged -= HandleMoneyChanged;
            }
        }
    }

    // 수정된 부분: Unity API에 맞는 정확한 타입 사용
    [System.Serializable]
    public class LeaderboardData
    {
        public string leaderboardId;
        public LeaderboardScoresPage scoresPage; // LeaderboardScoresPage 타입 사용
        public DateTime lastUpdated;
    }

    [System.Serializable]
    public class LeaderboardSummary
    {
        public string leaderboardId;
        public string leaderboardName;
        public long playerRank;
        public double playerScore;
        public List<LeaderboardEntry> topEntries;
    }
}