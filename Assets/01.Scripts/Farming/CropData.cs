using System.Collections.Generic;
using _01.Scripts.Farming.Mutations;
using UnityEngine;

namespace _01.Scripts.Farming
{
    // ItemData, MutationData 네임스페이스

// CropData 네임스페이스 (선택적)
// namespace GrowingAGarden.SO.Crops

    [CreateAssetMenu(fileName = "NewCropData", menuName = "Grow A Garden/Crop Data", order = 0)]
    public class CropData : ScriptableObject
    {
        [Header("Basic Info")]
        public string cropID; // 저장/로드 및 데이터베이스 조회용 고유 ID
        public string cropName = "New Crop";
        [TextArea]
        public string description = "Description of the crop.";
        public ItemData harvestedItem; // 수확 시 얻는 아이템 (ItemData ScriptableObject)
        public int minHarvestAmount = 1;
        public int maxHarvestAmount = 3;

        [Header("Growth Stages")]
        public List<CropGrowthStage> growthStages = new List<CropGrowthStage>();

        [Header("Mutations")]
        [Tooltip("List of mutations that can potentially occur on this crop.")]
        public List<MutationData> potentialMutations = new List<MutationData>();

        public int GetRandomHarvestAmount()
        {
            if (minHarvestAmount > maxHarvestAmount) return minHarvestAmount;
            return Random.Range(minHarvestAmount, maxHarvestAmount + 1);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(cropID))
            {
                if (!string.IsNullOrEmpty(cropName) && cropName != "New Crop")
                {
                    cropID = cropName.ToLower().Replace(" ", "_") + "_crop";
                }
                else if (name != "NewCropData" && !string.IsNullOrEmpty(name))
                {
                    cropID = name.ToLower().Replace(" ", "_") + "_crop_asset";
                }
                else
                {
                    // 정말 임시 ID, 에셋 생성 직후 이름이 없을 때만
                    cropID = "temp_cropid_" + System.Guid.NewGuid().ToString().Substring(0,8);
                }
            }
        }
    }

    [System.Serializable]
    public class CropGrowthStage
    {
        public string stageName = "Seedling";
        public GameObject stagePrefab;
        public float growthDuration = 10f;
    }
}