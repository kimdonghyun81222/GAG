using System;
using GrowAGarden.Farming._01.Scripts.Farming.Crops;
using GrowAGarden.Player._01.Scripts.Player.Interaction;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Fields
{
    public class FieldTile : MonoBehaviour, IInteractable
    {
        [Header("Tile Settings")]
        [SerializeField] private Vector2Int tilePosition;
        [SerializeField] private bool isInteractable = true;
        [SerializeField] private bool debugMode = false;
        
        [Header("Visual Components")]
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] private GameObject tilledVisual;
        [SerializeField] private GameObject wateredVisual;
        [SerializeField] private GameObject fertilizedVisual;
        
        // Tile state
        private TileState _currentState = TileState.Untilled;
        private SoilData _soilData;
        private CropEntity _plantedCrop;
        private FieldManager _fieldManager;
        
        // Soil properties
        private float _moisture = 0.5f;
        private float _fertility = 0.7f;
        private float _pH = 7f;
        private float _temperature = 20f;
        private float _lastWateredTime;
        private float _lastFertilizedTime;
        
        // Visual state
        private bool _isBeingInteracted = false;
        private bool _isHighlighted = false;
        
        // Properties
        public Vector2Int Position => tilePosition;
        public TileState CurrentState => _currentState;
        public bool HasCrop => _plantedCrop != null;
        public CropEntity PlantedCrop => _plantedCrop;
        public SoilData SoilData => _soilData;
        public float Moisture => _moisture;
        public float Fertility => _fertility;
        public float pH => _pH;
        public float Temperature => _temperature;
        public bool IsTilled => _currentState >= TileState.Tilled;
        public bool IsWatered => Time.time - _lastWateredTime < 3600f; // 1 hour
        public bool IsFertilized => Time.time - _lastFertilizedTime < 86400f; // 24 hours
        public bool CanPlant => IsTilled && !HasCrop;
        public bool CanHarvest => HasCrop && _plantedCrop.IsHarvestReady;
        
        // IInteractable implementation
        public bool IsBeingInteracted => _isBeingInteracted;
        
        // Events
        public event Action<FieldTile, TileState> OnStateChanged;
        public event Action<FieldTile, CropEntity> OnCropPlanted;
        public event Action<FieldTile, CropEntity> OnCropHarvested;
        public event Action<FieldTile> OnTileWatered;
        public event Action<FieldTile> OnTileFertilized;

        private void Awake()
        {
            if (tileRenderer == null)
                tileRenderer = GetComponent<Renderer>();
            
            InitializeSoilData();
        }

        private void Start()
        {
            _fieldManager = GetComponentInParent<FieldManager>();
            UpdateVisuals();
        }

        private void Update()
        {
            UpdateSoilConditions();
            UpdateCropConditions();
        }

        private void InitializeSoilData()
        {
            _soilData = new SoilData
            {
                moisture = _moisture,
                fertility = _fertility,
                pH = _pH,
                temperature = _temperature
            };
        }

        private void UpdateSoilConditions()
        {
            // Moisture evaporation
            float evaporationRate = 0.01f * Time.deltaTime; // 1% per second base rate
            
            // Temperature affects evaporation
            if (_temperature > 25f)
            {
                evaporationRate *= 1.5f;
            }
            
            _moisture = Mathf.Max(0f, _moisture - evaporationRate);
            
            // Fertility decay over time
            if (Time.time - _lastFertilizedTime > 86400f) // After 24 hours
            {
                float fertilityDecay = 0.001f * Time.deltaTime; // Very slow decay
                _fertility = Mathf.Max(0.1f, _fertility - fertilityDecay);
            }
            
            // Update soil data
            _soilData.moisture = _moisture;
            _soilData.fertility = _fertility;
            _soilData.pH = _pH;
            _soilData.temperature = _temperature;
        }

        private void UpdateCropConditions()
        {
            if (!HasCrop) return;
            
            // Pass soil conditions to the crop
            _plantedCrop.SetSoilHealth(_fertility);
            
            // Auto-water from soil moisture
            if (_moisture > 0.3f && _plantedCrop.WaterLevel < 0.7f)
            {
                _plantedCrop.Water(_moisture * 0.1f * Time.deltaTime);
            }
        }

        // Tile state management
        public bool Till()
        {
            if (_currentState != TileState.Untilled) return false;
            if (HasCrop) return false;
            
            SetState(TileState.Tilled);
            
            if (debugMode)
            {
                Debug.Log($"Tile {tilePosition} tilled");
            }
            
            return true;
        }

        public bool Water(float amount = 0.3f)
        {
            if (!IsTilled) return false;
            
            float oldMoisture = _moisture;
            _moisture = Mathf.Min(1f, _moisture + amount);
            _lastWateredTime = Time.time;
            
            // If tile is dry, make it watered
            if (_currentState == TileState.Tilled)
            {
                SetState(TileState.Watered);
            }
            
            OnTileWatered?.Invoke(this);
            
            if (debugMode)
            {
                Debug.Log($"Tile {tilePosition} watered: {oldMoisture:F2} → {_moisture:F2}");
            }
            
            return true;
        }

        public bool Fertilize(float amount = 0.2f)
        {
            if (!IsTilled) return false;
            
            float oldFertility = _fertility;
            _fertility = Mathf.Min(1f, _fertility + amount);
            _lastFertilizedTime = Time.time;
            
            SetState(TileState.Fertilized);
            
            OnTileFertilized?.Invoke(this);
            
            if (debugMode)
            {
                Debug.Log($"Tile {tilePosition} fertilized: {oldFertility:F2} → {_fertility:F2}");
            }
            
            return true;
        }

        public bool PlantCrop(CropDataSO cropData)
        {
            if (!CanPlant || cropData == null) return false;
            
            // Create crop entity
            GameObject cropObj = new GameObject($"{cropData.cropName}_Crop");
            cropObj.transform.SetParent(transform);
            cropObj.transform.localPosition = Vector3.zero;
            
            _plantedCrop = cropObj.AddComponent<CropEntity>();
            _plantedCrop.SetCropData(cropData);
            _plantedCrop.Initialize();
            
            // Set initial conditions
            _plantedCrop.SetSoilHealth(_fertility);
            _plantedCrop.SetCurrentSeason((CropSeason)UnityEngine.Random.Range(0, 4)); // Random season for now
            
            // Subscribe to crop events
            _plantedCrop.OnCropHarvested += OnCropHarvestedInternal;
            _plantedCrop.OnCropDied += OnCropDiedInternal;
            
            SetState(TileState.Planted);
            OnCropPlanted?.Invoke(this, _plantedCrop);
            
            if (debugMode)
            {
                Debug.Log($"Planted {cropData.cropName} on tile {tilePosition}");
            }
            
            return true;
        }

        public CropHarvestResult HarvestCrop()
        {
            if (!CanHarvest) return null;
            
            var result = _plantedCrop.Harvest();
            
            if (result != null)
            {
                OnCropHarvested?.Invoke(this, _plantedCrop);
                
                // If crop was destroyed after harvest, clear the tile
                if (_plantedCrop == null || _plantedCrop.gameObject == null)
                {
                    ClearCrop();
                }
                
                if (debugMode)
                {
                    Debug.Log($"Harvested crop on tile {tilePosition}: {result}");
                }
            }
            
            return result;
        }

        public void ClearCrop()
        {
            if (_plantedCrop != null)
            {
                // Unsubscribe from events
                _plantedCrop.OnCropHarvested -= OnCropHarvestedInternal;
                _plantedCrop.OnCropDied -= OnCropDiedInternal;
                
                // Destroy crop object
                if (_plantedCrop.gameObject != null)
                {
                    DestroyImmediate(_plantedCrop.gameObject);
                }
                
                _plantedCrop = null;
            }
            
            // Return to tilled state
            if (IsTilled)
            {
                SetState(TileState.Tilled);
            }
        }

        private void OnCropHarvestedInternal(CropEntity crop)
        {
            // Crop harvested, might still be on tile if renewable
            if (debugMode)
            {
                Debug.Log($"Crop harvested on tile {tilePosition}");
            }
        }

        private void OnCropDiedInternal(CropEntity crop)
        {
            // Crop died, clear the tile
            ClearCrop();
            
            if (debugMode)
            {
                Debug.Log($"Crop died on tile {tilePosition}");
            }
        }

        private void SetState(TileState newState)
        {
            var oldState = _currentState;
            _currentState = newState;
            
            UpdateVisuals();
            OnStateChanged?.Invoke(this, _currentState);
            
            if (debugMode && oldState != newState)
            {
                Debug.Log($"Tile {tilePosition} state: {oldState} → {newState}");
            }
        }

        private void UpdateVisuals()
        {
            if (tileRenderer == null) return;
            
            // Show/hide visual elements based on state
            if (tilledVisual != null)
                tilledVisual.SetActive(IsTilled);
            
            if (wateredVisual != null)
                wateredVisual.SetActive(IsWatered && IsTilled);
            
            if (fertilizedVisual != null)
                fertilizedVisual.SetActive(IsFertilized && IsTilled);
            
            // Update tile color based on state
            Color tileColor = _currentState switch
            {
                TileState.Untilled => Color.gray,
                TileState.Tilled => new Color(0.6f, 0.4f, 0.2f), // Brown
                TileState.Watered => new Color(0.4f, 0.3f, 0.2f), // Dark brown
                TileState.Fertilized => new Color(0.3f, 0.5f, 0.2f), // Green-brown
                TileState.Planted => new Color(0.2f, 0.6f, 0.2f), // Green
                _ => Color.white
            };
            
            if (_isHighlighted)
            {
                tileColor = Color.Lerp(tileColor, Color.yellow, 0.3f);
            }
            
            tileRenderer.material.color = tileColor;
        }

        // IInteractable implementation
        public bool CanInteract()
        {
            return isInteractable && (_currentState == TileState.Untilled || CanHarvest);
        }

        public bool Interact()
        {
            if (!CanInteract()) return false;
            
            _isBeingInteracted = true;
            
            // Default interaction: till if untilled, harvest if ready
            if (_currentState == TileState.Untilled)
            {
                bool success = Till();
                _isBeingInteracted = false;
                return success;
            }
            else if (CanHarvest)
            {
                var result = HarvestCrop();
                _isBeingInteracted = false;
                return result != null;
            }
            
            _isBeingInteracted = false;
            return false;
        }

        public void CancelInteraction()
        {
            _isBeingInteracted = false;
        }

        public string GetInteractionText()
        {
            if (_currentState == TileState.Untilled)
                return "Till Soil";
            else if (CanHarvest)
                return $"Harvest {_plantedCrop.CropData.cropName}";
            else if (HasCrop)
                return $"{_plantedCrop.CropData.cropName} ({_plantedCrop.CurrentStage})";
            else if (IsTilled)
                return "Plant Seeds";
            
            return "Field Tile";
        }

        public void OnLookEnter()
        {
            _isHighlighted = true;
            UpdateVisuals();
        }

        public void OnLookExit()
        {
            _isHighlighted = false;
            UpdateVisuals();
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        // Public setters
        public void SetPosition(Vector2Int position)
        {
            tilePosition = position;
        }

        public void SetTemperature(float temperature)
        {
            _temperature = temperature;
        }

        public void SetFieldManager(FieldManager manager)
        {
            _fieldManager = manager;
        }

        // Info methods
        public string GetTileInfo()
        {
            string info = $"Tile ({tilePosition.x}, {tilePosition.y})\n";
            info += $"State: {_currentState}\n";
            info += $"Moisture: {_moisture:P0}\n";
            info += $"Fertility: {_fertility:P0}\n";
            info += $"pH: {_pH:F1}\n";
            info += $"Temperature: {_temperature:F1}°C";
            
            if (HasCrop)
            {
                info += $"\nCrop: {_plantedCrop.CropData.cropName}";
                info += $"\nGrowth: {_plantedCrop.GrowthProgress:P0}";
                info += $"\nQuality: {_plantedCrop.Quality}";
            }
            
            return info;
        }

        // Debug methods
        public void DEBUG_SetState(TileState state)
        {
            if (!debugMode) return;
            SetState(state);
        }

        public void DEBUG_AddMoisture(float amount)
        {
            if (!debugMode) return;
            Water(amount);
        }

        public void DEBUG_AddFertility(float amount)
        {
            if (!debugMode) return;
            Fertilize(amount);
        }
    }

    [System.Serializable]
    public class SoilData
    {
        public float moisture;
        public float fertility;
        public float pH;
        public float temperature;
        public SoilQuality quality;
        
        public SoilData()
        {
            moisture = 0.5f;
            fertility = 0.7f;
            pH = 7f;
            temperature = 20f;
            quality = SoilQuality.Average;
        }
    }

    public enum TileState
    {
        Untilled = 0,
        Tilled = 1,
        Watered = 2,
        Fertilized = 3,
        Planted = 4
    }
}