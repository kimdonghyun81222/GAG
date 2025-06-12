using UnityEngine;

namespace _01.Scripts.Farming.Mutations
{
    [System.Serializable]
    public class MutationEffect
    {
        public MutationEffectType effectType;

        [Tooltip("Effect magnitude. For percentage, 0.1 = 10%. For visual override, this might be unused or an index. For BonusItemDrop, this could be quantity.")]
        public float value;

        [Tooltip("For BonusItemDrop, this is the ItemData to drop.")]
        public ItemData bonusItemData; // ItemData ScriptableObject 참조

        [Tooltip("Optional string parameter, e.g., for VisualOverride, the name/path of the prefab.")]
        public string stringParameter;

        // 예시: 효과를 설명하는 문자열 생성 (UI 표시용)
        public string GetDescription()
        {
            string desc = effectType.ToString() + ": ";
            switch (effectType)
            {
                case MutationEffectType.YieldIncreaseAbsolute:
                case MutationEffectType.YieldDecreaseAbsolute:
                case MutationEffectType.SellPriceIncreaseAbsolute:
                case MutationEffectType.SellPriceDecreaseAbsolute:
                    desc += (value > 0 ? "+" : "") + value.ToString("F0");
                    break;
                case MutationEffectType.YieldIncreasePercentage:
                case MutationEffectType.YieldDecreasePercentage:
                case MutationEffectType.SellPriceIncreasePercentage:
                case MutationEffectType.SellPriceDecreasePercentage:
                    desc += (value > 0 ? "+" : "") + (value * 100).ToString("F0") + "%";
                    break;
                case MutationEffectType.GrowthSpeedIncrease:
                case MutationEffectType.GrowthSpeedDecrease:
                    desc += "x" + value.ToString("F2");
                    break;
                case MutationEffectType.VisualOverride:
                    desc += string.IsNullOrEmpty(stringParameter) ? "Visual Change" : $"New Look ({stringParameter})";
                    break;
                case MutationEffectType.BonusItemDrop:
                    desc += $"Drops {value.ToString("F0")} {(bonusItemData != null ? bonusItemData.itemName : "Unknown Item")}";
                    break;
                default:
                    desc += value.ToString("F2");
                    break;
            }
            return desc;
        }
    }
}