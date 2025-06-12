using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _01.Scripts.Networking.Authentication;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Networking._01.Scripts.Networking.Leaderboards;
using UnityEngine;

namespace GrowAGarden.Networking._01.Scripts.Networking.Spectator
{
    [Provide]
    public class SpectatorManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Spectator Settings")]
        [SerializeField] private bool enableSpectatorMode = true;
        [SerializeField] private float updateInterval = 5f;
        [SerializeField] private int maxSpectatorTargets = 10;
        [SerializeField] private float spectatorCameraSpeed = 5f;
        [SerializeField] private float spectatorCameraHeight = 15f;
        
        // Dependencies
        [Inject] private AuthenticationManager _authManager;
        [Inject] private LeaderboardsManager _leaderboardsManager;
        
        // Spectator State
        private bool _isSpectating = false;
        private SpectatorTarget _currentTarget;
        private List<SpectatorTarget> _availableTargets = new List<SpectatorTarget>();
        private Camera _spectatorCamera;
        private Camera _originalPlayerCamera;
        private GameObject _originalPlayerObject;
        private Vector3 _spectatorPosition;
        private List<GameObject> _spectatorFarmObjects = new List<GameObject>();
        
        // Player Component References - 수정된 부분
        private MonoBehaviour _playerMovementComponent;
        private MonoBehaviour _playerCameraComponent;
        private MonoBehaviour _playerInteractionComponent;
        
        // Properties
        public bool IsSpectating => _isSpectating;
        public SpectatorTarget CurrentTarget => _currentTarget;
        public List<SpectatorTarget> AvailableTargets => new List<SpectatorTarget>(_availableTargets);
        
        // Events
        public event Action OnSpectatorModeEnabled;
        public event Action OnSpectatorModeDisabled;
        public event Action<SpectatorTarget> OnTargetChanged;
        public event Action<List<SpectatorTarget>> OnTargetsUpdated;

        private void Start()
        {
            if (_authManager != null)
            {
                _authManager.OnSignedIn += HandlePlayerSignedIn;
            }
            
            SetupSpectatorCamera();
            
            if (enableSpectatorMode)
            {
                InvokeRepeating(nameof(UpdateSpectatorTargets), updateInterval, updateInterval);
            }
        }
        
        [Provide]
        public SpectatorManager ProvideSpectatorManager() => this;

        private async void HandlePlayerSignedIn()
        {
            if (enableSpectatorMode)
            {
                await LoadSpectatorTargets();
            }
        }

        private void SetupSpectatorCamera()
        {
            // Find or create spectator camera
            GameObject spectatorCameraObject = GameObject.FindWithTag("SpectatorCamera");
            if (spectatorCameraObject == null)
            {
                spectatorCameraObject = new GameObject("SpectatorCamera");
                spectatorCameraObject.tag = "SpectatorCamera";
                _spectatorCamera = spectatorCameraObject.AddComponent<Camera>();
            }
            else
            {
                _spectatorCamera = spectatorCameraObject.GetComponent<Camera>();
            }
            
            // Configure camera for spectator mode
            _spectatorCamera.enabled = false;
            _spectatorCamera.fieldOfView = 60f;
            _spectatorCamera.nearClipPlane = 0.1f;
            _spectatorCamera.farClipPlane = 1000f;
            
            // Store original player camera and object
            _originalPlayerCamera = Camera.main;
            _originalPlayerObject = GameObject.FindWithTag("Player");
            
            // 수정된 부분: 플레이어 컴포넌트들을 이름으로 찾기
            CachePlayerComponents();
        }

        private void CachePlayerComponents()
        {
            if (_originalPlayerObject == null) return;
            
            // 컴포넌트들을 이름으로 찾아서 캐시
            var allComponents = _originalPlayerObject.GetComponents<MonoBehaviour>();
            
            foreach (var component in allComponents)
            {
                string componentName = component.GetType().Name;
                
                if (componentName.Contains("Movement") || componentName.Contains("FirstPerson"))
                {
                    _playerMovementComponent = component;
                }
                else if (componentName.Contains("Camera"))
                {
                    _playerCameraComponent = component;
                }
                else if (componentName.Contains("Interaction"))
                {
                    _playerInteractionComponent = component;
                }
            }
        }

        public async Task<bool> EnableSpectatorMode()
        {
            if (_isSpectating || !enableSpectatorMode)
                return false;
            
            try
            {
                await LoadSpectatorTargets();
                
                if (_availableTargets.Count == 0)
                {
                    Debug.LogWarning("No spectator targets available");
                    return false;
                }
                
                _isSpectating = true;
                
                DisablePlayerControls();
                
                if (_originalPlayerCamera != null)
                    _originalPlayerCamera.enabled = false;
                
                _spectatorCamera.enabled = true;
                
                await SelectSpectatorTarget(_availableTargets[0]);
                
                OnSpectatorModeEnabled?.Invoke();
                Debug.Log("Spectator mode enabled");
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to enable spectator mode: {ex.Message}");
                return false;
            }
        }

        public void DisableSpectatorMode()
        {
            if (!_isSpectating)
                return;
            
            _isSpectating = false;
            _currentTarget = null;
            
            ClearSpectatorFarm();
            EnablePlayerControls();
            
            _spectatorCamera.enabled = false;
            if (_originalPlayerCamera != null)
                _originalPlayerCamera.enabled = true;
            
            OnSpectatorModeDisabled?.Invoke();
            Debug.Log("Spectator mode disabled");
        }

        public async Task<bool> SelectSpectatorTarget(SpectatorTarget target)
        {
            if (!_isSpectating || target == null)
                return false;

            try
            {
                SpectatorFarmData farmData = await LoadTargetFarmData(target);

                if (farmData == null)
                {
                    Debug.LogWarning($"Could not load farm data for target: {target.playerName}");
                    return false;
                }

                _currentTarget = target;
                target.farmData = farmData;

                await ApplySpectatorFarmState(farmData);
                PositionSpectatorCamera(farmData);

                OnTargetChanged?.Invoke(target);
                Debug.Log($"Now spectating: {target.playerName}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to select spectator target: {ex.Message}");
                return false;
            }
        }

        private async Task LoadSpectatorTargets()
        {
            if (!_authManager.IsSignedIn || _leaderboardsManager == null)
                return;
    
            try
            {
                var leaderboard = await _leaderboardsManager.LoadLeaderboard("money_leaderboard", 0, maxSpectatorTargets);
        
                if (leaderboard?.Results == null)
                    return;
        
                _availableTargets.Clear();
        
                foreach (var entry in leaderboard.Results)
                {
                    if (entry.PlayerId == _authManager.PlayerId)
                        continue;
            
                    var target = new SpectatorTarget
                    {
                        playerId = entry.PlayerId,
                        playerName = entry.PlayerName,
                        money = (float)entry.Score,
                        rank = entry.Rank + 1,
                        lastUpdated = DateTime.UtcNow
                    };
            
                    _availableTargets.Add(target);
                }
        
                OnTargetsUpdated?.Invoke(_availableTargets);
                Debug.Log($"Loaded {_availableTargets.Count} spectator targets");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load spectator targets: {ex.Message}");
            }
        }

        private async Task<SpectatorFarmData> LoadTargetFarmData(SpectatorTarget target)
        {
            try
            {
                return CreateMockFarmData(target);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load farm data for {target.playerName}: {ex.Message}");
                return null;
            }
        }

        private SpectatorFarmData CreateMockFarmData(SpectatorTarget target)
        {
            int farmLevel = Mathf.Clamp(Mathf.FloorToInt(target.money / 1000f), 1, 10);
            int numPlots = Mathf.Clamp(farmLevel * 5, 5, 50);
            
            var farmData = new SpectatorFarmData
            {
                playerId = target.playerId,
                playerName = target.playerName,
                money = target.money,
                farmLevel = farmLevel,
                plots = new List<SpectatorPlotData>()
            };
            
            for (int i = 0; i < numPlots; i++)
            {
                var plot = new SpectatorPlotData
                {
                    position = new Vector3(
                        (i % 10) * 2f,
                        0f,
                        (i / 10) * 2f
                    ),
                    isEmpty = UnityEngine.Random.value < 0.3f,
                    cropType = GetRandomCropType(),
                    growthStage = UnityEngine.Random.Range(0, 4),
                    waterLevel = UnityEngine.Random.Range(0.3f, 1f),
                    fertilityLevel = UnityEngine.Random.Range(0.5f, 2f)
                };
                
                farmData.plots.Add(plot);
            }
            
            return farmData;
        }

        private string GetRandomCropType()
        {
            var crops = new[] { "carrot", "wheat", "potato", "tomato", "corn", "lettuce" };
            return crops[UnityEngine.Random.Range(0, crops.Length)];
        }

        private async Task ApplySpectatorFarmState(SpectatorFarmData farmData)
        {
            ClearSpectatorFarm();
            await CreateSpectatorFarm(farmData);
        }

        private void ClearSpectatorFarm()
        {
            foreach (var obj in _spectatorFarmObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            _spectatorFarmObjects.Clear();
            
            var farmObjects = GameObject.FindGameObjectsWithTag("Farm");
            foreach (var obj in farmObjects)
            {
                obj.SetActive(false);
            }
        }

        private async Task CreateSpectatorFarm(SpectatorFarmData farmData)
        {
            foreach (var plotData in farmData.plots)
            {
                await CreateSpectatorPlot(plotData);
            }
        }

        private async Task CreateSpectatorPlot(SpectatorPlotData plotData)
        {
            GameObject plotObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plotObject.transform.position = plotData.position;
            plotObject.transform.localScale = new Vector3(1.8f, 0.1f, 1.8f);
            plotObject.tag = "SpectatorFarm";
            
            _spectatorFarmObjects.Add(plotObject);
            
            Renderer renderer = plotObject.GetComponent<Renderer>();
            if (plotData.isEmpty)
            {
                renderer.material.color = new Color(0.6f, 0.4f, 0.2f);
            }
            else
            {
                await CreateSpectatorCrop(plotData);
                renderer.material.color = Color.green;
            }
        }

        private async Task CreateSpectatorCrop(SpectatorPlotData plotData)
        {
            if (plotData.isEmpty) return;
            
            GameObject cropObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cropObject.transform.position = plotData.position + Vector3.up * 0.5f;
            
            float scale = 0.2f + (plotData.growthStage * 0.3f);
            cropObject.transform.localScale = Vector3.one * scale;
            
            Renderer renderer = cropObject.GetComponent<Renderer>();
            renderer.material.color = GetCropColor(plotData.cropType, plotData.growthStage);
            
            cropObject.tag = "SpectatorFarm";
            _spectatorFarmObjects.Add(cropObject);
        }

        private Color GetCropColor(string cropType, int growthStage)
        {
            Color baseColor = cropType switch
            {
                "carrot" => new Color(1f, 0.5f, 0f),
                "wheat" => Color.yellow,
                "potato" => new Color(0.8f, 0.7f, 0.4f),
                "tomato" => Color.red,
                "corn" => Color.yellow,
                "lettuce" => Color.green,
                _ => Color.green
            };
            
            float intensity = 0.4f + (growthStage * 0.2f);
            return baseColor * intensity;
        }

        private void PositionSpectatorCamera(SpectatorFarmData farmData)
        {
            Vector3 center = CalculateFarmCenter(farmData);
            Vector3 size = CalculateFarmSize(farmData);
            
            float distance = Mathf.Max(size.x, size.z) * 0.75f;
            _spectatorPosition = center + new Vector3(distance, spectatorCameraHeight, distance);
            
            _spectatorCamera.transform.position = _spectatorPosition;
            _spectatorCamera.transform.LookAt(center);
        }

        private Vector3 CalculateFarmCenter(SpectatorFarmData farmData)
        {
            if (farmData.plots.Count == 0) return Vector3.zero;
            
            Vector3 sum = Vector3.zero;
            foreach (var plot in farmData.plots)
            {
                sum += plot.position;
            }
            return sum / farmData.plots.Count;
        }

        private Vector3 CalculateFarmSize(SpectatorFarmData farmData)
        {
            if (farmData.plots.Count == 0) return Vector3.one * 10f;
            
            Vector3 min = farmData.plots[0].position;
            Vector3 max = farmData.plots[0].position;
            
            foreach (var plot in farmData.plots)
            {
                min = Vector3.Min(min, plot.position);
                max = Vector3.Max(max, plot.position);
            }
            
            return max - min;
        }

        public void NextSpectatorTarget()
        {
            if (!_isSpectating || _availableTargets.Count == 0)
                return;
            
            int currentIndex = _availableTargets.IndexOf(_currentTarget);
            int nextIndex = (currentIndex + 1) % _availableTargets.Count;
            
            _ = SelectSpectatorTarget(_availableTargets[nextIndex]);
        }

        public void PreviousSpectatorTarget()
        {
            if (!_isSpectating || _availableTargets.Count == 0)
                return;
            
            int currentIndex = _availableTargets.IndexOf(_currentTarget);
            int prevIndex = (currentIndex - 1 + _availableTargets.Count) % _availableTargets.Count;
            
            _ = SelectSpectatorTarget(_availableTargets[prevIndex]);
        }

        private async void UpdateSpectatorTargets()
        {
            if (_authManager.IsSignedIn)
            {
                await LoadSpectatorTargets();
            }
        }

        // 수정된 부분: 캐시된 컴포넌트 참조 사용
        private void DisablePlayerControls()
        {
            if (_playerMovementComponent != null)
                _playerMovementComponent.enabled = false;
            
            if (_playerCameraComponent != null)
                _playerCameraComponent.enabled = false;
            
            if (_playerInteractionComponent != null)
                _playerInteractionComponent.enabled = false;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void EnablePlayerControls()
        {
            if (_playerMovementComponent != null)
                _playerMovementComponent.enabled = true;
            
            if (_playerCameraComponent != null)
                _playerCameraComponent.enabled = true;
            
            if (_playerInteractionComponent != null)
                _playerInteractionComponent.enabled = true;
            
            var farmObjects = GameObject.FindGameObjectsWithTag("Farm");
            foreach (var obj in farmObjects)
            {
                obj.SetActive(true);
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!_isSpectating) return;
            
            HandleSpectatorInput();
        }

        private void HandleSpectatorInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DisableSpectatorMode();
            }
            
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                NextSpectatorTarget();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                PreviousSpectatorTarget();
            }
            
            HandleSpectatorCameraMovement();
        }

        private void HandleSpectatorCameraMovement()
        {
            if (_spectatorCamera == null || _currentTarget?.farmData == null) return;
            
            if (Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X") * spectatorCameraSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * spectatorCameraSpeed;
                
                Vector3 farmCenter = CalculateFarmCenter(_currentTarget.farmData);
                
                _spectatorCamera.transform.RotateAround(farmCenter, Vector3.up, mouseX);
                _spectatorCamera.transform.RotateAround(farmCenter, _spectatorCamera.transform.right, -mouseY);
                
                _spectatorCamera.transform.LookAt(farmCenter);
            }
            
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 farmCenter = CalculateFarmCenter(_currentTarget.farmData);
                Vector3 direction = (_spectatorCamera.transform.position - farmCenter).normalized;
                
                _spectatorCamera.transform.position += direction * scroll * -10f;
                
                float distance = Vector3.Distance(_spectatorCamera.transform.position, farmCenter);
                if (distance < 5f || distance > 50f)
                {
                    _spectatorCamera.transform.position = farmCenter + direction * Mathf.Clamp(distance, 5f, 50f);
                }
            }
        }

        public void SetSpectatorEnabled(bool enabled)
        {
            enableSpectatorMode = enabled;
            
            if (!enabled && _isSpectating)
            {
                DisableSpectatorMode();
            }
        }

        public float GetSpectatorCameraSpeed()
        {
            return spectatorCameraSpeed;
        }

        public void SetSpectatorCameraSpeed(float speed)
        {
            spectatorCameraSpeed = Mathf.Max(0.1f, speed);
        }

        private void OnDestroy()
        {
            if (_authManager != null)
            {
                _authManager.OnSignedIn -= HandlePlayerSignedIn;
            }
            
            ClearSpectatorFarm();
        }
    }

    // Support classes
    [System.Serializable]
    public class SpectatorTarget
    {
        public string playerId;
        public string playerName;
        public float money;
        public long rank;
        public DateTime lastUpdated;
        public SpectatorFarmData farmData;
    }

    [System.Serializable]
    public class SpectatorFarmData
    {
        public string playerId;
        public string playerName;
        public float money;
        public int farmLevel;
        public List<SpectatorPlotData> plots = new List<SpectatorPlotData>();
    }

    [System.Serializable]
    public class SpectatorPlotData
    {
        public Vector3 position;
        public bool isEmpty;
        public string cropType;
        public int growthStage;
        public float waterLevel;
        public float fertilityLevel;
    }
}