using System.Collections.Generic;
using System.Linq;
using _01.Scripts.Core;
using _01.Scripts.Farming.Mutations;
using UnityEngine;

namespace _01.Scripts.Farming
{
    public class PlantMutator : MonoBehaviour
    {
        private GrowingCrop _growingCrop; // 이 Mutator가 부착된 작물
        private CropData _baseCropData;   // 원본 작물 데이터

        // 현재 작물에 적용된 돌연변이 목록
        public List<MutationData> ActiveMutations { get; private set; } = new List<MutationData>();

        // 돌연변이로 인해 변경된 최종 수치들 (계산된 값)
        public float GrowthSpeedMultiplier { get; private set; } = 1f;
        public int AdditionalYield { get; private set; } = 0;
        public float YieldMultiplier { get; private set; } = 1f;
        public int AdditionalSellPrice { get; private set; } = 0;
        public float SellPriceMultiplier { get; private set; } = 1f;
        public float WaterConsumptionModifier { get; private set; } = 1f;
        public string VisualOverrideKey { get; private set; } = null;
        public List<KeyValuePair<ItemData, int>> BonusItemDrops { get; private set; } = new List<KeyValuePair<ItemData, int>>();


        public void Initialize(GrowingCrop crop, CropData baseData)
        {
            _growingCrop = crop;
            _baseCropData = baseData;
            ActiveMutations.Clear();
            ResetCalculatedValues();
            // TODO: 저장된 게임 로드 시, 여기서 저장된 돌연변이 불러와서 적용하는 로직 필요
        }

        // 작물 심을 때 또는 특정 조건에서 돌연변이 발생 시도
        public void AttemptMutationRoll(List<MutationData> possibleMutations)
        {
            if (possibleMutations == null || possibleMutations.Count == 0) return;

            foreach (MutationData mutation in possibleMutations)
            {
                if (mutation == null) continue;

                // 이미 적용된 돌연변이는 다시 시도하지 않음 (선택적 정책)
                if (ActiveMutations.Any(am => am.mutationID == mutation.mutationID)) continue;

                float randomChance = Random.Range(0f, 1f);
                if (randomChance < mutation.baseOccurrenceChance) // 기본 발생 확률
                {
                    // TODO: 추가 조건 확인 로직 (예: 특정 작물, 계절 등)
                    // if (IsMutationApplicable(mutation))
                    // {
                    ApplyMutation(mutation);
                    // }
                }
            }
            // 돌연변이 적용 후 최종 수치 재계산
            RecalculateModifiedStats();
        }

        private void ApplyMutation(MutationData mutation)
        {
            if (mutation == null || ActiveMutations.Any(am => am.mutationID == mutation.mutationID)) return;

            ActiveMutations.Add(mutation);
            Debug.Log($"Mutation Applied to {_baseCropData.cropName}: {mutation.mutationName}!");
            UIManager.Instance?.ShowNotification($"{_baseCropData.cropName}에 [{mutation.mutationName}] 돌연변이 발생!");

            // TODO: 돌연변이 적용 시 시각적/청각적 피드백 (파티클, 사운드 등)
        }

        // 적용된 모든 돌연변이를 기반으로 최종 수치 계산
        public void RecalculateModifiedStats()
        {
            ResetCalculatedValues();

            foreach (MutationData mutation in ActiveMutations)
            {
                foreach (MutationEffect effect in mutation.positiveEffects.Concat(mutation.negativeEffects))
                {
                    ApplySingleEffect(effect);
                }
            }
            // 최종 계산된 값들을 GrowingCrop에 알리거나, GrowingCrop이 직접 이 값들을 참조
            _growingCrop?.UpdateStatsFromMutator();
        }

        private void ResetCalculatedValues()
        {
            GrowthSpeedMultiplier = 1f;
            AdditionalYield = 0;
            YieldMultiplier = 1f;
            AdditionalSellPrice = 0;
            SellPriceMultiplier = 1f;
            WaterConsumptionModifier = 1f;
            VisualOverrideKey = null;
            BonusItemDrops.Clear();
        }

        private void ApplySingleEffect(MutationEffect effect)
        {
            switch (effect.effectType)
            {
                case MutationEffectType.YieldIncreaseAbsolute:
                    AdditionalYield += (int)effect.value;
                    break;
                case MutationEffectType.YieldDecreaseAbsolute:
                    AdditionalYield -= (int)effect.value; // 실제로는 양수로 저장하고 빼는게 나을수도
                    break;
                case MutationEffectType.YieldIncreasePercentage:
                    YieldMultiplier += effect.value; // 0.1f for +10%
                    break;
                case MutationEffectType.YieldDecreasePercentage:
                    YieldMultiplier -= effect.value;
                    break;
                case MutationEffectType.GrowthSpeedIncrease:
                    GrowthSpeedMultiplier *= effect.value; // 1.5f for 1.5x speed
                    break;
                case MutationEffectType.GrowthSpeedDecrease:
                    GrowthSpeedMultiplier *= effect.value; // 0.8f for 0.8x speed
                    break;
                case MutationEffectType.SellPriceIncreaseAbsolute:
                    AdditionalSellPrice += (int)effect.value;
                    break;
                case MutationEffectType.SellPriceDecreaseAbsolute:
                    AdditionalSellPrice -= (int)effect.value;
                    break;
                case MutationEffectType.SellPriceIncreasePercentage:
                    SellPriceMultiplier += effect.value;
                    break;
                case MutationEffectType.SellPriceDecreasePercentage:
                    SellPriceMultiplier -= effect.value;
                    break;
                case MutationEffectType.WaterConsumptionIncrease:
                    WaterConsumptionModifier *= effect.value; // 예: 1.2 (20% 더 소모)
                    break;
                case MutationEffectType.WaterConsumptionDecrease:
                    WaterConsumptionModifier *= effect.value; // 예: 0.8 (20% 덜 소모)
                    break;
                case MutationEffectType.VisualOverride:
                    VisualOverrideKey = effect.stringParameter; // 가장 마지막에 적용된 VisualOverride 사용
                    break;
                case MutationEffectType.BonusItemDrop:
                    if (effect.bonusItemData != null && effect.value > 0)
                    {
                        BonusItemDrops.Add(new KeyValuePair<ItemData, int>(effect.bonusItemData, (int)effect.value));
                    }
                    break;
                // TODO: 다른 효과들 구현
            }
        }

        // (선택적) 특정 돌연변이가 이 작물에 적용될 수 있는지 확인하는 로직
        // private bool IsMutationApplicable(MutationData mutation)
        // {
        //    if (mutation.applicableCrops != null && mutation.applicableCrops.Count > 0)
        //    {
        //        if (!mutation.applicableCrops.Contains(_baseCropData)) return false;
        //    }
        //    // ... 기타 조건 (계절, 환경 등)
        //    return true;
        // }
    }
}