using System;
using System.Collections.Generic;
using GrowAGarden.Farming._01.Scripts.Farming.Crops;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Mutations
{
    [System.Serializable]
    public class CropMutation
    {
        [Header("Mutation Information")]
        public string mutationId;
        public string mutationName = "Unknown Mutation";
        public string description = "A mysterious crop mutation";
        public MutationType mutationType = MutationType.Beneficial;
        
        [Header("Genetic Composition")]
        public List<GeneticData> activeGenes = new List<GeneticData>();
        public List<GeneticData> recessiveGenes = new List<GeneticData>();
        
        [Header("Mutation Effects")]
        public List<MutationEffect> effects = new List<MutationEffect>();
        
        [Header("Stability")]
        [Range(0f, 1f)] public float stability = 0.8f;
        [Range(0f, 1f)] public float breedingStability = 0.6f;
        public int generationStability = 3; // How many generations it remains stable
        
        [Header("Discovery")]
        public bool isDiscovered = false;
        public DateTime discoveryDate;
        public int discoveryGeneration = 0;
        
        [Header("Rarity")]
        public MutationRarity rarity = MutationRarity.Common;
        [Range(0f, 1f)] public float naturalOccurrence = 0.05f;
        
        // Properties
        public bool IsStable => stability >= 0.7f;
        public bool CanBreed => breedingStability >= 0.5f;
        public int TotalGenes => activeGenes.Count + recessiveGenes.Count;
        
        // Constructor
        public CropMutation()
        {
            mutationId = Guid.NewGuid().ToString();
            discoveryDate = DateTime.Now;
        }
        
        public CropMutation(string name, List<GeneticData> genes)
        {
            mutationId = Guid.NewGuid().ToString();
            mutationName = name;
            activeGenes = new List<GeneticData>(genes);
            discoveryDate = DateTime.Now;
            GenerateEffectsFromGenes();
        }
        
        // Effect calculation
        public void GenerateEffectsFromGenes()
        {
            effects.Clear();
            
            // Calculate combined effects from all active genes
            var traitTotals = new Dictionary<TraitType, float>();
            var traitCounts = new Dictionary<TraitType, int>();
            var traitExpressions = new Dictionary<TraitType, TraitExpression>();
            
            foreach (var gene in activeGenes)
            {
                if (gene?.traits == null) continue;
                
                foreach (var trait in gene.traits)
                {
                    if (!traitTotals.ContainsKey(trait.traitType))
                    {
                        traitTotals[trait.traitType] = 0f;
                        traitCounts[trait.traitType] = 0;
                        traitExpressions[trait.traitType] = trait.expressionType;
                    }
                    
                    float effectiveValue = trait.GetEffectiveValue(gene.dominanceStrength);
                    
                    switch (trait.expressionType)
                    {
                        case TraitExpression.Additive:
                            traitTotals[trait.traitType] += effectiveValue;
                            break;
                            
                        case TraitExpression.Multiplicative:
                            traitTotals[trait.traitType] = traitTotals[trait.traitType] == 0f ? 
                                effectiveValue : traitTotals[trait.traitType] * effectiveValue;
                            break;
                            
                        case TraitExpression.Dominant:
                            traitTotals[trait.traitType] = Mathf.Max(traitTotals[trait.traitType], effectiveValue);
                            break;
                            
                        case TraitExpression.Average:
                            traitTotals[trait.traitType] += effectiveValue;
                            break;
                            
                        case TraitExpression.Minimum:
                            traitTotals[trait.traitType] = traitTotals[trait.traitType] == 0f ? 
                                effectiveValue : Mathf.Min(traitTotals[trait.traitType], effectiveValue);
                            break;
                            
                        case TraitExpression.Maximum:
                            traitTotals[trait.traitType] = Mathf.Max(traitTotals[trait.traitType], effectiveValue);
                            break;
                    }
                    
                    traitCounts[trait.traitType]++;
                }
            }
            
            // Create mutation effects from calculated traits
            foreach (var kvp in traitTotals)
            {
                TraitType traitType = kvp.Key;
                float value = kvp.Value;
                
                // Apply averaging for average expression type
                if (traitExpressions[traitType] == TraitExpression.Average && traitCounts[traitType] > 1)
                {
                    value /= traitCounts[traitType];
                }
                
                if (Mathf.Abs(value) > 0.01f) // Only add significant effects
                {
                    effects.Add(new MutationEffect
                    {
                        effectType = ConvertTraitToEffect(traitType),
                        value = value,
                        isPercentage = IsTraitPercentage(traitType),
                        description = GetEffectDescription(traitType, value)
                    });
                }
            }
            
            // Determine mutation type based on overall effect
            DetermineMutationType();
        }
        
        private MutationEffectType ConvertTraitToEffect(TraitType traitType)
        {
            return traitType switch
            {
                TraitType.GrowthSpeed => MutationEffectType.GrowthSpeedModifier,
                TraitType.YieldBonus => MutationEffectType.YieldModifier,
                TraitType.QualityBonus => MutationEffectType.QualityModifier,
                TraitType.SizeModifier => MutationEffectType.SizeModifier,
                TraitType.DiseaseResistance => MutationEffectType.DiseaseResistance,
                TraitType.DroughtResistance => MutationEffectType.DroughtResistance,
                TraitType.WaterEfficiency => MutationEffectType.WaterEfficiency,
                TraitType.MarketValue => MutationEffectType.ValueModifier,
                _ => MutationEffectType.Special
            };
        }
        
        private bool IsTraitPercentage(TraitType traitType)
        {
            return traitType switch
            {
                TraitType.GrowthSpeed => true,
                TraitType.YieldBonus => true,
                TraitType.QualityBonus => true,
                TraitType.DiseaseResistance => true,
                TraitType.DroughtResistance => true,
                TraitType.WaterEfficiency => true,
                _ => false
            };
        }
        
        private string GetEffectDescription(TraitType traitType, float value)
        {
            string prefix = value > 0 ? "+" : "";
            string suffix = IsTraitPercentage(traitType) ? "%" : "";
            
            return traitType switch
            {
                TraitType.GrowthSpeed => $"{prefix}{value * 100:F0}{suffix} Growth Speed",
                TraitType.YieldBonus => $"{prefix}{value * 100:F0}{suffix} Yield",
                TraitType.QualityBonus => $"{prefix}{value * 100:F0}{suffix} Quality",
                TraitType.SizeModifier => $"{prefix}{value:F1} Size",
                TraitType.DiseaseResistance => $"{prefix}{value * 100:F0}{suffix} Disease Resistance",
                TraitType.DroughtResistance => $"{prefix}{value * 100:F0}{suffix} Drought Resistance",
                TraitType.WaterEfficiency => $"{prefix}{value * 100:F0}{suffix} Water Efficiency",
                TraitType.MarketValue => $"{prefix}{value * 100:F0}{suffix} Market Value",
                _ => $"{traitType}: {prefix}{value:F2}"
            };
        }
        
        private void DetermineMutationType()
        {
            float totalBenefit = 0f;
            int beneficialEffects = 0;
            int detrimentalEffects = 0;
            
            foreach (var effect in effects)
            {
                if (IsBeneficialEffect(effect.effectType))
                {
                    if (effect.value > 0)
                    {
                        beneficialEffects++;
                        totalBenefit += effect.value;
                    }
                    else
                    {
                        detrimentalEffects++;
                        totalBenefit += effect.value;
                    }
                }
                else
                {
                    // For resistance effects, higher values are better
                    if (effect.value > 0)
                    {
                        beneficialEffects++;
                        totalBenefit += effect.value;
                    }
                }
            }
            
            if (totalBenefit > 0.2f && beneficialEffects > detrimentalEffects)
            {
                mutationType = MutationType.Beneficial;
            }
            else if (totalBenefit < -0.2f || detrimentalEffects > beneficialEffects)
            {
                mutationType = MutationType.Detrimental;
            }
            else
            {
                mutationType = MutationType.Neutral;
            }
        }
        
        private bool IsBeneficialEffect(MutationEffectType effectType)
        {
            return effectType switch
            {
                MutationEffectType.GrowthSpeedModifier => true,
                MutationEffectType.YieldModifier => true,
                MutationEffectType.QualityModifier => true,
                MutationEffectType.ValueModifier => true,
                _ => false
            };
        }
        
        // Application methods
        public void ApplyToCrop(CropEntity crop)
        {
            if (crop?.CropData == null) return;
            
            foreach (var effect in effects)
            {
                ApplyEffect(crop, effect);
            }
        }
        
        private void ApplyEffect(CropEntity crop, MutationEffect effect)
        {
            // This would apply the mutation effect to the crop
            // Implementation depends on how CropEntity exposes its modifiable properties
            
            switch (effect.effectType)
            {
                case MutationEffectType.GrowthSpeedModifier:
                    // Apply growth speed modification
                    break;
                    
                case MutationEffectType.YieldModifier:
                    // Apply yield modification
                    break;
                    
                case MutationEffectType.QualityModifier:
                    // Apply quality modification
                    break;
                    
                case MutationEffectType.SizeModifier:
                    // Apply size modification
                    break;
                    
                case MutationEffectType.DiseaseResistance:
                    // Apply disease resistance
                    break;
                    
                case MutationEffectType.DroughtResistance:
                    // Apply drought resistance
                    break;
                    
                case MutationEffectType.WaterEfficiency:
                    // Apply water efficiency
                    break;
                    
                case MutationEffectType.ValueModifier:
                    // Apply value modification
                    break;
            }
        }
        
        // Breeding methods
        public static CropMutation CrossBreed(CropMutation parent1, CropMutation parent2)
        {
            if (parent1 == null || parent2 == null) return null;
            
            var offspring = new CropMutation();
            offspring.mutationName = $"{parent1.mutationName} × {parent2.mutationName}";
            offspring.discoveryGeneration = Mathf.Max(parent1.discoveryGeneration, parent2.discoveryGeneration) + 1;
            
            // Inherit genes from both parents
            offspring.activeGenes = InheritGenes(parent1.activeGenes, parent2.activeGenes);
            offspring.recessiveGenes = InheritGenes(parent1.recessiveGenes, parent2.recessiveGenes);
            
            // Calculate stability based on parents
            offspring.stability = (parent1.stability + parent2.stability) * 0.5f * 0.9f; // Slight stability loss
            offspring.breedingStability = (parent1.breedingStability + parent2.breedingStability) * 0.5f;
            
            // Generate effects from inherited genes
            offspring.GenerateEffectsFromGenes();
            
            return offspring;
        }
        
        private static List<GeneticData> InheritGenes(List<GeneticData> parent1Genes, List<GeneticData> parent2Genes)
        {
            var inheritedGenes = new List<GeneticData>();
            var allGenes = new HashSet<GeneticData>();
            
            // Collect all unique genes from both parents
            allGenes.UnionWith(parent1Genes);
            allGenes.UnionWith(parent2Genes);
            
            foreach (var gene in allGenes)
            {
                // Check inheritance chance
                float inheritanceChance = gene.inheritanceChance;
                
                // Bonus chance if gene is present in both parents
                if (parent1Genes.Contains(gene) && parent2Genes.Contains(gene))
                {
                    inheritanceChance = Mathf.Min(1f, inheritanceChance * 1.5f);
                }
                
                if (UnityEngine.Random.value <= inheritanceChance)
                {
                    inheritedGenes.Add(gene);
                }
            }
            
            return inheritedGenes;
        }
        
        // Mutation methods
        public bool TryMutate()
        {
            if (activeGenes.Count == 0) return false;
            
            foreach (var gene in activeGenes)
            {
                if (gene?.possibleMutations == null || gene.possibleMutations.Count == 0) continue;
                
                if (UnityEngine.Random.value <= gene.mutationChance)
                {
                    // Select random mutation
                    var mutationTarget = gene.possibleMutations[UnityEngine.Random.Range(0, gene.possibleMutations.Count)];
                    
                    if (mutationTarget != null && !activeGenes.Contains(mutationTarget))
                    {
                        activeGenes.Add(mutationTarget);
                        GenerateEffectsFromGenes();
                        
                        // Reduce stability due to mutation
                        stability = Mathf.Max(0.1f, stability * 0.8f);
                        
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        // Utility methods
        public float GetEffectValue(MutationEffectType effectType)
        {
            foreach (var effect in effects)
            {
                if (effect.effectType == effectType)
                {
                    return effect.value;
                }
            }
            return 0f;
        }
        
        public bool HasEffect(MutationEffectType effectType)
        {
            return effects.Exists(e => e.effectType == effectType);
        }
        
        public void Discover()
        {
            isDiscovered = true;
            discoveryDate = DateTime.Now;
        }
        
        public string GetMutationSummary()
        {
            string summary = $"{mutationName} ({mutationType})\n";
            summary += $"Stability: {stability:P0}\n";
            summary += $"Generation: {discoveryGeneration}\n\n";
            
            summary += "Effects:\n";
            foreach (var effect in effects)
            {
                summary += $"• {effect.description}\n";
            }
            
            return summary;
        }
        
        public Color GetMutationColor()
        {
            return mutationType switch
            {
                MutationType.Beneficial => Color.green,
                MutationType.Detrimental => Color.red,
                MutationType.Neutral => Color.yellow,
                MutationType.Special => Color.magenta,
                _ => Color.white
            };
        }
    }
    
    [System.Serializable]
    public class MutationEffect
    {
        public MutationEffectType effectType;
        public float value;
        public bool isPercentage;
        public string description;
        public bool isPermanent = true;
        public float duration = 0f;
    }
    
    public enum MutationType
    {
        Beneficial,
        Detrimental,
        Neutral,
        Special
    }
    
    public enum MutationRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical
    }
    
    public enum MutationEffectType
    {
        GrowthSpeedModifier,
        YieldModifier,
        QualityModifier,
        SizeModifier,
        DiseaseResistance,
        PestResistance,
        DroughtResistance,
        FrostResistance,
        HeatResistance,
        WaterEfficiency,
        LightEfficiency,
        NutrientEfficiency,
        ValueModifier,
        StorageModifier,
        FlavorModifier,
        ColorChange,
        ShapeChange,
        Special
    }
}