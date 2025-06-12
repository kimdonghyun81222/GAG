using System.Collections.Generic;

namespace _01.Scripts.Data
{
    [System.Serializable]
    public class FarmPlotData_Serializable
    {
        public string uniquePlotID; // 농경지의 고유 식별자 (예: Scene 내 경로 또는 할당된 ID)
        // public Vector3 position; // 농경지 위치 (동적 생성 시 필요할 수 있음)

        public int farmPlotStateValue; // FarmPlotState enum 값을 int로 저장
        public float currentWateredTimeRemaining;

        public bool hasCrop;
        public GrowingCropData_Serializable cropData; // 심겨진 작물 데이터

        public FarmPlotData_Serializable()
        {
            cropData = new GrowingCropData_Serializable();
        }
    }

    [System.Serializable]
    public class GrowingCropData_Serializable
    {
        public string cropDataID; // 원본 CropData의 이름 또는 고유 ID
        public int currentGrowthStageIndex;
        public float currentGrowthTimer;
        public bool isWatered;
        public bool isDriedOut;

        public List<string> activeMutationIDs; // 적용된 MutationData의 ID 목록

        public GrowingCropData_Serializable()
        {
            activeMutationIDs = new List<string>();
        }
    }
}