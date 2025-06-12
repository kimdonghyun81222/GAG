using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Fields
{
    [CreateAssetMenu(fileName = "FieldData", menuName = "GrowAGarden/Farming/Field Data")]
    public class FieldData : ScriptableObject
    {
        [Header("Field Information")]
        public string fieldName = "Farm Field";
        public string description = "A basic farming field";
        public Sprite fieldIcon;
        public FieldType fieldType = FieldType.Regular;
        
        [Header("Field Dimensions")]
        public int width = 10;
        public int height = 10;
        public float tileSize = 1f;
        public float tileSpacing = 0.1f;
        
        [Header("Soil Properties")]
        public SoilQuality defaultSoilQuality = SoilQuality.Average;
        public float defaultMoisture = 0.5f;
        public float defaultFertility = 0.7f;
        public float defaultPH = 7f; // Neutral pH
        
        [Header("Field Bonuses")]
        public float growthSpeedMultiplier = 1f;
        public float yieldMultiplier = 1f;
        public float qualityBonus = 0f;
        public bool allowsIrrigation = true;
        public bool allowsFertilizer = true;
        
        [Header("Unlocking")]
        public int requiredPlayerLevel = 1;
        public int unlockCost = 0;
        public List<string> requiredUpgrades = new List<string>();
        
        [Header("Visual")]
        public Material fieldMaterial;
        public Material tilledMaterial;
        public Material wateredMaterial;
        public GameObject fieldPrefab;
        
        // Validation
        private void OnValidate()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            tileSize = Mathf.Max(0.1f, tileSize);
            tileSpacing = Mathf.Max(0f, tileSpacing);
            
            defaultMoisture = Mathf.Clamp01(defaultMoisture);
            defaultFertility = Mathf.Clamp01(defaultFertility);
            defaultPH = Mathf.Clamp(defaultPH, 0f, 14f);
            
            growthSpeedMultiplier = Mathf.Max(0.1f, growthSpeedMultiplier);
            yieldMultiplier = Mathf.Max(0.1f, yieldMultiplier);
            qualityBonus = Mathf.Clamp(qualityBonus, -1f, 1f);
        }
        
        // Utility methods
        public int GetTotalTiles()
        {
            return width * height;
        }
        
        public Vector2 GetFieldSize()
        {
            return new Vector2(
                width * (tileSize + tileSpacing) - tileSpacing,
                height * (tileSize + tileSpacing) - tileSpacing
            );
        }
        
        public Vector3 GetTileWorldPosition(int x, int y, Vector3 fieldOrigin)
        {
            float worldX = fieldOrigin.x + x * (tileSize + tileSpacing);
            float worldZ = fieldOrigin.z + y * (tileSize + tileSpacing);
            
            return new Vector3(worldX, fieldOrigin.y, worldZ);
        }
        
        public Vector2Int GetTileCoordinates(Vector3 worldPosition, Vector3 fieldOrigin)
        {
            Vector3 localPos = worldPosition - fieldOrigin;
            
            int x = Mathf.RoundToInt(localPos.x / (tileSize + tileSpacing));
            int y = Mathf.RoundToInt(localPos.z / (tileSize + tileSpacing));
            
            return new Vector2Int(x, y);
        }
        
        public bool IsValidTilePosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
        
        public bool IsValidTilePosition(Vector2Int position)
        {
            return IsValidTilePosition(position.x, position.y);
        }
    }
    
    public enum FieldType
    {
        Regular,
        Greenhouse,
        Hydroponic,
        Raised,
        Terraced,
        Indoor
    }
    
    public enum SoilQuality
    {
        Poor,
        Below_Average,
        Average,
        Good,
        Excellent
    }
}