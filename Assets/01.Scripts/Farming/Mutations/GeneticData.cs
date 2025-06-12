using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Mutations
{
    [CreateAssetMenu(fileName = "GeneticData", menuName = "GrowAGarden/Farming/Genetic Data")]
    public class GeneticData : ScriptableObject
    {
        [Header("Genetic Information")]
        public string geneticId;
        public string geneticName = "Basic Genetics";
        public string description = "Basic genetic traits";
        
        [Header("Trait Modifiers")]
        public List<GeneticTrait> traits = new List<GeneticTrait>();
        
        [Header("Inheritance")]
        [Range(0f, 1f)] public float inheritanceChance = 0.5f;
        [Range(0f, 1f)] public float dominanceStrength = 0.5f;
        public bool isRecessive = false;
        public bool isRare = false;
        
        [Header("Mutation Settings")]
        [Range(0f, 1f)] public float mutationChance = 0.01f;
        public List<GeneticData> possibleMutations = new List<GeneticData>();
        
        [Header("Compatibility")]
        public List<string> compatibleCrops = new List<string>();
        public List<GeneticData> incompatibleGenes = new List<GeneticData>();
        
        [Header("Visual")]
        public Color traitColor = Color.white;
        public Sprite traitIcon;
        
        // Validation
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(geneticId))
                geneticId = geneticName?.Replace(" ", "_").ToLower() ?? "unknown_gene";
            
            inheritanceChance = Mathf.Clamp01(inheritanceChance);
            dominanceStrength = Mathf.Clamp01(dominanceStrength);
            mutationChance = Mathf.Clamp01(mutationChance);
            
            // Validate traits
            foreach (var trait in traits)
            {
                if (trait != null)
                {
                    trait.Validate();
                }
            }
        }
        
        // Utility methods
        public bool IsCompatibleWith(string cropId)
        {
            return compatibleCrops.Count == 0 || compatibleCrops.Contains(cropId);
        }
        
        public bool CanCoexistWith(GeneticData otherGene)
        {
            return otherGene != null && !incompatibleGenes.Contains(otherGene);
        }
        
        public float GetTraitValue(TraitType traitType)
        {
            foreach (var trait in traits)
            {
                if (trait.traitType == traitType)
                {
                    return trait.value;
                }
            }
            return 0f;
        }
        
        public bool HasTrait(TraitType traitType)
        {
            return traits.Exists(t => t.traitType == traitType);
        }
        
        public GeneticTrait GetTrait(TraitType traitType)
        {
            return traits.Find(t => t.traitType == traitType);
        }
        
        public string GetGeneticDescription()
        {
            string desc = description + "\n\n";
            desc += $"Inheritance: {inheritanceChance:P0}\n";
            desc += $"Dominance: {dominanceStrength:P0}\n";
            
            if (isRecessive)
                desc += "Recessive Gene\n";
            
            if (isRare)
                desc += "Rare Gene\n";
            
            desc += "\nTraits:\n";
            foreach (var trait in traits)
            {
                desc += $"• {trait.GetDescription()}\n";
            }
            
            return desc;
        }
    }
    
    [System.Serializable]
    public class GeneticTrait
    {
        [Header("Trait Information")]
        public TraitType traitType;
        public string traitName;
        public string description;
        
        [Header("Trait Values")]
        public float value;
        public float minValue = -1f;
        public float maxValue = 1f;
        public bool isPercentage = false;
        
        [Header("Expression")]
        public TraitExpression expressionType = TraitExpression.Additive;
        [Range(0f, 1f)] public float expressionStrength = 1f;
        
        public void Validate()
        {
            value = Mathf.Clamp(value, minValue, maxValue);
            expressionStrength = Mathf.Clamp01(expressionStrength);
            
            if (string.IsNullOrEmpty(traitName))
                traitName = traitType.ToString();
        }
        
        public string GetDescription()
        {
            string valueText = isPercentage ? $"{value:P0}" : $"{value:F2}";
            return $"{traitName}: {valueText}";
        }
        
        public float GetEffectiveValue(float dominance = 1f)
        {
            return value * expressionStrength * dominance;
        }
    }
    
    public enum TraitType
    {
        // Growth traits
        GrowthSpeed,
        YieldBonus,
        QualityBonus,
        SizeModifier,
        
        // Resistance traits
        DiseaseResistance,
        PestResistance,
        DroughtResistance,
        FrostResistance,
        HeatResistance,
        
        // Environmental traits
        SoilAdaptability,
        WaterEfficiency,
        LightEfficiency,
        TemperatureTolerance,
        
        // Special traits
        NutrientDensity,
        StorageLife,
        FlavorIntensity,
        ColorVariation,
        ShapeVariation,
        
        // Economic traits
        MarketValue,
        ProcessingYield,
        
        // Breeding traits
        FertilityBonus,
        MutationRate,
        SeedProduction
    }
    
    public enum TraitExpression
    {
        Additive,      // Values add together
        Multiplicative, // Values multiply
        Dominant,      // Strongest value wins
        Average,       // Average of all values
        Minimum,       // Minimum value
        Maximum        // Maximum value
    }
}