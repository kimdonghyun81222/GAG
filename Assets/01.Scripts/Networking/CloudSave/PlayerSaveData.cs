using System;
using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.Networking.CloudSave
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public const int CURRENT_VERSION = 1;
        
        [Header("Basic Info")]
        public int saveVersion = CURRENT_VERSION;
        public string playerId;
        public string playerName;
        public DateTime saveTime;
        
        [Header("Economy")]
        public float playerMoney;
        public Dictionary<string, float> marketPrices = new Dictionary<string, float>();
        
        [Header("Farm Progress")]
        public int farmLevel;
        public List<string> unlockedAreas = new List<string>();
        public List<FieldPlotSaveData> fieldPlots = new List<FieldPlotSaveData>();
        
        [Header("Tools & Equipment")]
        public List<ToolSaveData> ownedTools = new List<ToolSaveData>();
        public string currentToolType;
        
        [Header("Crops & Seeds")]
        public List<string> unlockedCrops = new List<string>();
        public Dictionary<string, int> seedInventory = new Dictionary<string, int>();
        
        [Header("Statistics")]
        public int totalHarvests;
        public float totalEarnings;
        public float totalTimePlayed;
        public Dictionary<string, int> cropHarvestCounts = new Dictionary<string, int>();
        
        [Header("Achievements")]
        public List<string> achievements = new List<string>();
        public Dictionary<string, object> achievementProgress = new Dictionary<string, object>();
        
        [Header("Settings")]
        public GameSettingsSaveData gameSettings = new GameSettingsSaveData();

        public Dictionary<string, object> ToCloudData()
        {
            return new Dictionary<string, object>
            {
                ["saveVersion"] = saveVersion,
                ["playerId"] = playerId,
                ["playerName"] = playerName,
                ["saveTime"] = DateTime.UtcNow.ToBinary(),
                ["playerMoney"] = playerMoney,
                ["marketPrices"] = marketPrices,
                ["farmLevel"] = farmLevel,
                ["unlockedAreas"] = unlockedAreas,
                ["fieldPlots"] = ConvertFieldPlotsToDict(),
                ["ownedTools"] = ConvertToolsToDict(),
                ["currentToolType"] = currentToolType ?? "",
                ["unlockedCrops"] = unlockedCrops,
                ["seedInventory"] = seedInventory,
                ["totalHarvests"] = totalHarvests,
                ["totalEarnings"] = totalEarnings,
                ["totalTimePlayed"] = totalTimePlayed,
                ["cropHarvestCounts"] = cropHarvestCounts,
                ["achievements"] = achievements,
                ["achievementProgress"] = achievementProgress,
                ["gameSettings"] = gameSettings.ToDict()
            };
        }

        public static PlayerSaveData FromCloudData(Dictionary<string, object> data)
        {
            var saveData = new PlayerSaveData();
            
            if (data.TryGetValue("saveVersion", out var version))
                saveData.saveVersion = Convert.ToInt32(version);
            
            if (data.TryGetValue("playerId", out var playerId))
                saveData.playerId = playerId.ToString();
            
            if (data.TryGetValue("playerName", out var playerName))
                saveData.playerName = playerName.ToString();
            
            if (data.TryGetValue("saveTime", out var saveTime))
                saveData.saveTime = DateTime.FromBinary(Convert.ToInt64(saveTime));
            
            if (data.TryGetValue("playerMoney", out var money))
                saveData.playerMoney = Convert.ToSingle(money);
            
            if (data.TryGetValue("farmLevel", out var farmLevel))
                saveData.farmLevel = Convert.ToInt32(farmLevel);
            
            // Handle collections safely
            if (data.TryGetValue("unlockedAreas", out var areas) && areas is List<object> areasList)
                saveData.unlockedAreas = areasList.ConvertAll(x => x.ToString());
            
            if (data.TryGetValue("unlockedCrops", out var crops) && crops is List<object> cropsList)
                saveData.unlockedCrops = cropsList.ConvertAll(x => x.ToString());
            
            if (data.TryGetValue("achievements", out var achievements) && achievements is List<object> achievementsList)
                saveData.achievements = achievementsList.ConvertAll(x => x.ToString());
            
            // Handle dictionaries
            if (data.TryGetValue("marketPrices", out var marketPrices) && marketPrices is Dictionary<string, object> pricesDict)
            {
                saveData.marketPrices = new Dictionary<string, float>();
                foreach (var kvp in pricesDict)
                    saveData.marketPrices[kvp.Key] = Convert.ToSingle(kvp.Value);
            }
            
            if (data.TryGetValue("seedInventory", out var seedInventory) && seedInventory is Dictionary<string, object> seedDict)
            {
                saveData.seedInventory = new Dictionary<string, int>();
                foreach (var kvp in seedDict)
                    saveData.seedInventory[kvp.Key] = Convert.ToInt32(kvp.Value);
            }
            
            // Statistics
            if (data.TryGetValue("totalHarvests", out var totalHarvests))
                saveData.totalHarvests = Convert.ToInt32(totalHarvests);
            
            if (data.TryGetValue("totalEarnings", out var totalEarnings))
                saveData.totalEarnings = Convert.ToSingle(totalEarnings);
            
            if (data.TryGetValue("totalTimePlayed", out var totalTimePlayed))
                saveData.totalTimePlayed = Convert.ToSingle(totalTimePlayed);
            
            // Game settings
            if (data.TryGetValue("gameSettings", out var settings) && settings is Dictionary<string, object> settingsDict)
                saveData.gameSettings = GameSettingsSaveData.FromDict(settingsDict);
            
            return saveData;
        }

        private List<Dictionary<string, object>> ConvertFieldPlotsToDict()
        {
            var result = new List<Dictionary<string, object>>();
            foreach (var plot in fieldPlots)
            {
                result.Add(plot.ToDict());
            }
            return result;
        }

        private List<Dictionary<string, object>> ConvertToolsToDict()
        {
            var result = new List<Dictionary<string, object>>();
            foreach (var tool in ownedTools)
            {
                result.Add(tool.ToDict());
            }
            return result;
        }
    }

    [System.Serializable]
    public class FieldPlotSaveData
    {
        public Vector3 position;
        public bool isEmpty;
        public string cropType;
        public int growthStage;
        public float growthProgress;
        public float waterLevel;
        public float fertilityLevel;
        public float soilQuality;
        
        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>
            {
                ["positionX"] = position.x,
                ["positionY"] = position.y,
                ["positionZ"] = position.z,
                ["isEmpty"] = isEmpty,
                ["cropType"] = cropType ?? "",
                ["growthStage"] = growthStage,
                ["growthProgress"] = growthProgress,
                ["waterLevel"] = waterLevel,
                ["fertilityLevel"] = fertilityLevel,
                ["soilQuality"] = soilQuality
            };
        }
        
        public static FieldPlotSaveData FromDict(Dictionary<string, object> data)
        {
            var plot = new FieldPlotSaveData();
            
            if (data.TryGetValue("positionX", out var x) && 
                data.TryGetValue("positionY", out var y) && 
                data.TryGetValue("positionZ", out var z))
            {
                plot.position = new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), Convert.ToSingle(z));
            }
            
            if (data.TryGetValue("isEmpty", out var isEmpty))
                plot.isEmpty = Convert.ToBoolean(isEmpty);
            
            if (data.TryGetValue("cropType", out var cropType))
                plot.cropType = cropType.ToString();
            
            if (data.TryGetValue("growthStage", out var growthStage))
                plot.growthStage = Convert.ToInt32(growthStage);
            
            if (data.TryGetValue("growthProgress", out var growthProgress))
                plot.growthProgress = Convert.ToSingle(growthProgress);
            
            if (data.TryGetValue("waterLevel", out var waterLevel))
                plot.waterLevel = Convert.ToSingle(waterLevel);
            
            if (data.TryGetValue("fertilityLevel", out var fertilityLevel))
                plot.fertilityLevel = Convert.ToSingle(fertilityLevel);
            
            if (data.TryGetValue("soilQuality", out var soilQuality))
                plot.soilQuality = Convert.ToSingle(soilQuality);
            
            return plot;
        }
    }

    [System.Serializable]
    public class ToolSaveData
    {
        public string toolType;
        public int toolLevel;
        public float durability;
        public float maxDurability;
        public bool isEquipped;
        
        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>
            {
                ["toolType"] = toolType,
                ["toolLevel"] = toolLevel,
                ["durability"] = durability,
                ["maxDurability"] = maxDurability,
                ["isEquipped"] = isEquipped
            };
        }
        
        public static ToolSaveData FromDict(Dictionary<string, object> data)
        {
            var tool = new ToolSaveData();
            
            if (data.TryGetValue("toolType", out var toolType))
                tool.toolType = toolType.ToString();
            
            if (data.TryGetValue("toolLevel", out var toolLevel))
                tool.toolLevel = Convert.ToInt32(toolLevel);
            
            if (data.TryGetValue("durability", out var durability))
                tool.durability = Convert.ToSingle(durability);
            
            if (data.TryGetValue("maxDurability", out var maxDurability))
                tool.maxDurability = Convert.ToSingle(maxDurability);
            
            if (data.TryGetValue("isEquipped", out var isEquipped))
                tool.isEquipped = Convert.ToBoolean(isEquipped);
            
            return tool;
        }
    }

    [System.Serializable]
    public class GameSettingsSaveData
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public float mouseSensitivity = 2f;
        public bool invertY = false;
        public int qualityLevel = 2;
        public bool fullscreen = true;
        public int resolutionIndex = 0;
        
        public Dictionary<string, object> ToDict()
        {
            return new Dictionary<string, object>
            {
                ["masterVolume"] = masterVolume,
                ["musicVolume"] = musicVolume,
                ["sfxVolume"] = sfxVolume,
                ["mouseSensitivity"] = mouseSensitivity,
                ["invertY"] = invertY,
                ["qualityLevel"] = qualityLevel,
                ["fullscreen"] = fullscreen,
                ["resolutionIndex"] = resolutionIndex
            };
        }
        
        public static GameSettingsSaveData FromDict(Dictionary<string, object> data)
        {
            var settings = new GameSettingsSaveData();
            
            if (data.TryGetValue("masterVolume", out var masterVolume))
                settings.masterVolume = Convert.ToSingle(masterVolume);
            
            if (data.TryGetValue("musicVolume", out var musicVolume))
                settings.musicVolume = Convert.ToSingle(musicVolume);
            
            if (data.TryGetValue("sfxVolume", out var sfxVolume))
                settings.sfxVolume = Convert.ToSingle(sfxVolume);
            
            if (data.TryGetValue("mouseSensitivity", out var mouseSensitivity))
                settings.mouseSensitivity = Convert.ToSingle(mouseSensitivity);
            
            if (data.TryGetValue("invertY", out var invertY))
                settings.invertY = Convert.ToBoolean(invertY);
            
            if (data.TryGetValue("qualityLevel", out var qualityLevel))
                settings.qualityLevel = Convert.ToInt32(qualityLevel);
            
            if (data.TryGetValue("fullscreen", out var fullscreen))
                settings.fullscreen = Convert.ToBoolean(fullscreen);
            
            if (data.TryGetValue("resolutionIndex", out var resolutionIndex))
                settings.resolutionIndex = Convert.ToInt32(resolutionIndex);
            
            return settings;
        }
    }
}