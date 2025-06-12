using System;
using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Farming._01.Scripts.Farming.Crops;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Fields
{
    [Provide]
    public class FieldManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Field Settings")]
        [SerializeField] private FieldData fieldData;
        [SerializeField] private Transform fieldOrigin;
        [SerializeField] private bool autoGenerateField = true;
        [SerializeField] private bool debugMode = false;
        
        [Header("Tile Prefab")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Material tileMaterial;
        
        [Header("Environmental")]
        [SerializeField] private float globalTemperature = 20f;
        [SerializeField] private bool enableWeatherEffects = true;
        [SerializeField] private float rainMoistureBonus = 0.3f;
        
        // Field state
        private FieldTile[,] _fieldTiles;
        private Dictionary<Vector2Int, FieldTile> _tileMap = new Dictionary<Vector2Int, FieldTile>();
        private List<FieldTile> _allTiles = new List<FieldTile>();
        
        // Statistics
        private int _tilledTileCount = 0;
        private int _plantedTileCount = 0;
        private int _harvestReadyCount = 0;
        
        // Properties
        public FieldData FieldData => fieldData;
        public int Width => fieldData?.width ?? 0;
        public int Height => fieldData?.height ?? 0;
        public int TotalTiles => Width * Height;
        public int TilledTileCount => _tilledTileCount;
        public int PlantedTileCount => _plantedTileCount;
        public int HarvestReadyCount => _harvestReadyCount;
        public float FieldUtilization => TotalTiles > 0 ? (float)_plantedTileCount / TotalTiles : 0f;
        public List<FieldTile> AllTiles => new List<FieldTile>(_allTiles);
        
        // Events
        public event Action<FieldManager> OnFieldGenerated;
        public event Action<FieldTile> OnTileTilled;
        public event Action<FieldTile> OnTilePlanted;
        public event Action<FieldTile> OnTileHarvested;
        public event Action<FieldManager> OnFieldStatsUpdated;

        private void Awake()
        {
            if (fieldOrigin == null)
                fieldOrigin = transform;
        }

        private void Start()
        {
            if (autoGenerateField && fieldData != null)
            {
                GenerateField();
            }
        }

        private void Update()
        {
            UpdateFieldStatistics();
            UpdateEnvironmentalEffects();
        }
        
        [Provide]
        public FieldManager ProvideFieldManager() => this;

        // Field generation
        public void GenerateField()
        {
            if (fieldData == null)
            {
                if (debugMode)
                {
                    Debug.LogError("Cannot generate field: FieldData is null");
                }
                return;
            }
            
            ClearExistingField();
            CreateFieldTiles();
            OnFieldGenerated?.Invoke(this);
            
            if (debugMode)
            {
                Debug.Log($"Generated field: {Width}x{Height} ({TotalTiles} tiles)");
            }
        }

        private void ClearExistingField()
        {
            if (_fieldTiles != null)
            {
                for (int x = 0; x < _fieldTiles.GetLength(0); x++)
                {
                    for (int y = 0; y < _fieldTiles.GetLength(1); y++)
                    {
                        if (_fieldTiles[x, y] != null)
                        {
                            DestroyImmediate(_fieldTiles[x, y].gameObject);
                        }
                    }
                }
            }
            
            _fieldTiles = null;
            _tileMap.Clear();
            _allTiles.Clear();
        }

        private void CreateFieldTiles()
        {
            _fieldTiles = new FieldTile[Width, Height];
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    CreateTile(x, y);
                }
            }
        }

        private void CreateTile(int x, int y)
        {
            Vector3 tilePosition = fieldData.GetTileWorldPosition(x, y, fieldOrigin.position);
            GameObject tileObj;
            
            if (tilePrefab != null)
            {
                tileObj = Instantiate(tilePrefab, tilePosition, Quaternion.identity, fieldOrigin);
            }
            else
            {
                // Create default tile
                tileObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tileObj.transform.position = tilePosition;
                tileObj.transform.parent = fieldOrigin;
                tileObj.transform.localScale = new Vector3(fieldData.tileSize, 0.1f, fieldData.tileSize);
                
                if (tileMaterial != null)
                {
                    tileObj.GetComponent<Renderer>().material = tileMaterial;
                }
            }
            
            tileObj.name = $"Tile_{x}_{y}";
            
            // Add or get FieldTile component
            FieldTile tile = tileObj.GetComponent<FieldTile>();
            if (tile == null)
            {
                tile = tileObj.AddComponent<FieldTile>();
            }
            
            // Initialize tile
            tile.SetPosition(new Vector2Int(x, y));
            tile.SetFieldManager(this);
            tile.SetTemperature(globalTemperature);
            
            // Subscribe to tile events
            tile.OnStateChanged += OnTileStateChanged;
            tile.OnCropPlanted += OnTileCropPlanted;
            tile.OnCropHarvested += OnTileCropHarvested;
            
            // Store tile references
            _fieldTiles[x, y] = tile;
            _tileMap[new Vector2Int(x, y)] = tile;
            _allTiles.Add(tile);
        }

        // Tile access methods
        public FieldTile GetTile(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return _fieldTiles[x, y];
        }

        public FieldTile GetTile(Vector2Int position)
        {
            return GetTile(position.x, position.y);
        }

        public FieldTile GetTileAt(Vector3 worldPosition)
        {
            Vector2Int tilePos = fieldData.GetTileCoordinates(worldPosition, fieldOrigin.position);
            return GetTile(tilePos);
        }

        public List<FieldTile> GetTilesInRange(Vector2Int center, int range)
        {
            List<FieldTile> tiles = new List<FieldTile>();
            
            for (int x = center.x - range; x <= center.x + range; x++)
            {
                for (int y = center.y - range; y <= center.y + range; y++)
                {
                    FieldTile tile = GetTile(x, y);
                    if (tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }
            
            return tiles;
        }

        public List<FieldTile> GetTilesByState(TileState state)
        {
            List<FieldTile> tiles = new List<FieldTile>();
            
            foreach (var tile in _allTiles)
            {
                if (tile != null && tile.CurrentState == state)
                {
                    tiles.Add(tile);
                }
            }
            
            return tiles;
        }

        public List<FieldTile> GetTilesWithCrops()
        {
            List<FieldTile> tiles = new List<FieldTile>();
            
            foreach (var tile in _allTiles)
            {
                if (tile != null && tile.HasCrop)
                {
                    tiles.Add(tile);
                }
            }
            
            return tiles;
        }

        public List<FieldTile> GetHarvestReadyTiles()
        {
            List<FieldTile> tiles = new List<FieldTile>();
            
            foreach (var tile in _allTiles)
            {
                if (tile != null && tile.CanHarvest)
                {
                    tiles.Add(tile);
                }
            }
            
            return tiles;
        }

        // Field operations
        public int TillArea(Vector2Int startPos, Vector2Int endPos)
        {
            int tilledCount = 0;
            
            int minX = Mathf.Min(startPos.x, endPos.x);
            int maxX = Mathf.Max(startPos.x, endPos.x);
            int minY = Mathf.Min(startPos.y, endPos.y);
            int maxY = Mathf.Max(startPos.y, endPos.y);
            
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    FieldTile tile = GetTile(x, y);
                    if (tile != null && tile.Till())
                    {
                        tilledCount++;
                    }
                }
            }
            
            return tilledCount;
        }

        public int WaterArea(Vector2Int startPos, Vector2Int endPos, float amount = 0.3f)
        {
            int wateredCount = 0;
            
            int minX = Mathf.Min(startPos.x, endPos.x);
            int maxX = Mathf.Max(startPos.x, endPos.x);
            int minY = Mathf.Min(startPos.y, endPos.y);
            int maxY = Mathf.Max(startPos.y, endPos.y);
            
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    FieldTile tile = GetTile(x, y);
                    if (tile != null && tile.Water(amount))
                    {
                        wateredCount++;
                    }
                }
            }
            
            return wateredCount;
        }

        public int PlantArea(Vector2Int startPos, Vector2Int endPos, CropDataSO cropData)
        {
            if (cropData == null) return 0;
            
            int plantedCount = 0;
            
            int minX = Mathf.Min(startPos.x, endPos.x);
            int maxX = Mathf.Max(startPos.x, endPos.x);
            int minY = Mathf.Min(startPos.y, endPos.y);
            int maxY = Mathf.Max(startPos.y, endPos.y);
            
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    FieldTile tile = GetTile(x, y);
                    if (tile != null && tile.PlantCrop(cropData))
                    {
                        plantedCount++;
                    }
                }
            }
            
            return plantedCount;
        }

        public List<CropHarvestResult> HarvestArea(Vector2Int startPos, Vector2Int endPos)
        {
            List<CropHarvestResult> results = new List<CropHarvestResult>();
            
            int minX = Mathf.Min(startPos.x, endPos.x);
            int maxX = Mathf.Max(startPos.x, endPos.x);
            int minY = Mathf.Min(startPos.y, endPos.y);
            int maxY = Mathf.Max(startPos.y, endPos.y);
            
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    FieldTile tile = GetTile(x, y);
                    if (tile != null && tile.CanHarvest)
                    {
                        var result = tile.HarvestCrop();
                        if (result != null)
                        {
                            results.Add(result);
                        }
                    }
                }
            }
            
            return results;
        }

        // Event handlers
        private void OnTileStateChanged(FieldTile tile, TileState newState)
        {
            if (newState == TileState.Tilled)
            {
                OnTileTilled?.Invoke(tile);
            }
        }

        private void OnTileCropPlanted(FieldTile tile, CropEntity crop)
        {
            OnTilePlanted?.Invoke(tile);
        }

        private void OnTileCropHarvested(FieldTile tile, CropEntity crop)
        {
            OnTileHarvested?.Invoke(tile);
        }

        // Statistics and updates
        private void UpdateFieldStatistics()
        {
            int tilledCount = 0;
            int plantedCount = 0;
            int harvestReadyCount = 0;
            
            foreach (var tile in _allTiles)
            {
                if (tile == null) continue;
                
                if (tile.IsTilled) tilledCount++;
                if (tile.HasCrop) plantedCount++;
                if (tile.CanHarvest) harvestReadyCount++;
            }
            
            bool statsChanged = false;
            if (_tilledTileCount != tilledCount)
            {
                _tilledTileCount = tilledCount;
                statsChanged = true;
            }
            if (_plantedTileCount != plantedCount)
            {
                _plantedTileCount = plantedCount;
                statsChanged = true;
            }
            if (_harvestReadyCount != harvestReadyCount)
            {
                _harvestReadyCount = harvestReadyCount;
                statsChanged = true;
            }
            
            if (statsChanged)
            {
                OnFieldStatsUpdated?.Invoke(this);
            }
        }

        private void UpdateEnvironmentalEffects()
        {
            if (!enableWeatherEffects) return;
            
            // Update temperature for all tiles
            foreach (var tile in _allTiles)
            {
                if (tile != null)
                {
                    tile.SetTemperature(globalTemperature);
                }
            }
        }

        // Utility methods
        public bool IsValidPosition(int x, int y)
        {
            return fieldData != null && fieldData.IsValidTilePosition(x, y);
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return IsValidPosition(position.x, position.y);
        }

        public void SetFieldData(FieldData newFieldData)
        {
            fieldData = newFieldData;
            
            if (autoGenerateField)
            {
                GenerateField();
            }
        }

        public void SetGlobalTemperature(float temperature)
        {
            globalTemperature = temperature;
        }

        // Weather effects
        public void ApplyRain(float intensity = 1f)
        {
            float moistureBonus = rainMoistureBonus * intensity;
            
            foreach (var tile in _allTiles)
            {
                if (tile != null && tile.IsTilled)
                {
                    tile.Water(moistureBonus);
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"Rain applied to field with intensity {intensity}");
            }
        }

        // Debug methods
        public void DEBUG_TillAllTiles()
        {
            if (!debugMode) return;
            
            foreach (var tile in _allTiles)
            {
                if (tile != null)
                {
                    tile.Till();
                }
            }
        }

        public void DEBUG_WaterAllTiles()
        {
            if (!debugMode) return;
            
            foreach (var tile in _allTiles)
            {
                if (tile != null)
                {
                    tile.Water(1f);
                }
            }
        }

        public void DEBUG_PlantRandomCrops(CropDataSO[] cropTypes)
        {
            if (!debugMode || cropTypes == null || cropTypes.Length == 0) return;
            
            foreach (var tile in _allTiles)
            {
                if (tile != null && tile.CanPlant)
                {
                    var randomCrop = cropTypes[UnityEngine.Random.Range(0, cropTypes.Length)];
                    tile.PlantCrop(randomCrop);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (fieldData == null || fieldOrigin == null) return;
            
            // Draw field bounds
            Vector2 fieldSize = fieldData.GetFieldSize();
            Vector3 center = fieldOrigin.position + new Vector3(fieldSize.x * 0.5f, 0f, fieldSize.y * 0.5f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, new Vector3(fieldSize.x, 0.1f, fieldSize.y));
            
            // Draw tile grid
            Gizmos.color = Color.yellow;
            for (int x = 0; x <= Width; x++)
            {
                Vector3 start = fieldOrigin.position + new Vector3(x * (fieldData.tileSize + fieldData.tileSpacing), 0f, 0f);
                Vector3 end = start + new Vector3(0f, 0f, fieldSize.y);
                Gizmos.DrawLine(start, end);
            }
            
            for (int y = 0; y <= Height; y++)
            {
                Vector3 start = fieldOrigin.position + new Vector3(0f, 0f, y * (fieldData.tileSize + fieldData.tileSpacing));
                Vector3 end = start + new Vector3(fieldSize.x, 0f, 0f);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}