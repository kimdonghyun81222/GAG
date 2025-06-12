using System.Collections.Generic;

namespace _01.Scripts.Data
{
    [System.Serializable]
    public class GameSaveData
    {
        public string saveVersion = "0.1.0"; // 저장 파일 버전 관리
        public string lastSavedSceneName;     // 마지막으로 저장된 씬 이름

        public PlayerData_Serializable playerData;
        public List<FarmPlotData_Serializable> farmPlotDataList;
        // public WorldData_Serializable worldData; // 월드 시간, 계절 등

        public GameSaveData()
        {
            playerData = new PlayerData_Serializable();
            farmPlotDataList = new List<FarmPlotData_Serializable>();
            // worldData = new WorldData_Serializable();
        }
    }
}