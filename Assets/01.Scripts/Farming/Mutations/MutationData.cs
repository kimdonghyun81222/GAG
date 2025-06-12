using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts.Farming.Mutations
{
    public enum MutationRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(fileName = "NewMutationData", menuName = "Grow A Garden/Mutation Data", order = 100)]
    public class MutationData : ScriptableObject
    {
        [Header("Basic Info")]
        public string mutationID; // 중복되지 않는 고유 ID
        public string mutationName = "New Mutation";
        [TextArea(3, 5)]
        public string description = "A special trait affecting the plant.";
        public MutationRarity rarity = MutationRarity.Common;
        public Sprite mutationIcon; // UI 표시용 아이콘 (선택적)

        [Header("Effects")]
        public List<MutationEffect> positiveEffects = new List<MutationEffect>();
        public List<MutationEffect> negativeEffects = new List<MutationEffect>(); // 돌연변이는 단점도 가질 수 있음

        [Header("Application Rules")]
        [Range(0f, 1f)]
        [Tooltip("Base chance for this mutation to occur (0 to 1).")]
        public float baseOccurrenceChance = 0.1f; // 10%

        // [Tooltip("Prefab to use for visual override if one of the effects is VisualOverride.")]
        // public GameObject visualOverridePrefab; // MutationEffect의 stringParameter로 대체하거나 여기서 직접 관리

        // TODO: 추가 조건들 (예: 특정 작물에만 발생, 특정 환경 조건 등)
        // public List<CropData> applicableCrops; (특정 작물 전용 돌연변이)
        // public Season requiredSeason; (특정 계절에만 발생)

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(mutationID))
            {
                mutationID = System.Guid.NewGuid().ToString(); // 간단한 고유 ID 자동 생성
            }
            if (string.IsNullOrEmpty(mutationName) && name != "NewMutationData")
            {
                mutationName = name; // 에셋 이름으로 자동 설정
            }
        }
    }
}