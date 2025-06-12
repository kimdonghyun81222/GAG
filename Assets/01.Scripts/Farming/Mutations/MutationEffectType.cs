namespace _01.Scripts.Farming.Mutations
{
    public enum MutationEffectType
    {
        // 수확량 관련
        YieldIncreaseAbsolute,    // 수확량 절대값 증가 (예: +2개)
        YieldIncreasePercentage,  // 수확량 퍼센트 증가 (예: +20%)
        YieldDecreaseAbsolute,    // 수확량 절대값 감소
        YieldDecreasePercentage,  // 수확량 퍼센트 감소

        // 성장 속도 관련
        GrowthSpeedIncrease,      // 성장 속도 증가 (예: 1.5배)
        GrowthSpeedDecrease,      // 성장 속도 감소 (예: 0.8배)
        GrowthDurationReduction,  // 총 성장 시간 감소 (절대값, 예: -30초)
        GrowthDurationIncrease,   // 총 성장 시간 증가 (절대값, 예: +30초)

        // 판매 가격 관련
        SellPriceIncreaseAbsolute,  // 판매 가격 절대값 증가 (예: +10골드)
        SellPriceIncreasePercentage,// 판매 가격 퍼센트 증가 (예: +15%)
        SellPriceDecreaseAbsolute,
        SellPriceDecreasePercentage,

        // 물 필요량 관련
        WaterConsumptionIncrease, // 물 소비량 증가 (더 자주 물 줘야 함)
        WaterConsumptionDecrease, // 물 소비량 감소 (덜 자주 물 줘도 됨)
        DroughtResistance,        // 물 마름 효과에 대한 저항력 증가

        // 외형 변경 (특정 프리팹으로 교체)
        VisualOverride,

        // 특정 아이템 추가 드랍
        BonusItemDrop,

        // 품질 향상/저하 (별도 품질 시스템 필요 시)
        QualityIncrease,
        QualityDecrease,

        // 기타 특수 효과
        PestResistance,           // 해충 저항
        DiseaseResistance,        // 질병 저항
        IncreasedFertilizerEffect // 비료 효과 증폭
    }
}