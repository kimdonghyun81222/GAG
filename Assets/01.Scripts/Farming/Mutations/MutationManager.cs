using System;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Farming._01.Scripts.Farming.Crops;
using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Mutations
{
    [Provide]
    public class MutationManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Mutation Settings")]
        [SerializeField] private List<GeneticData> availableGenes = new List<GeneticData>();
        [SerializeField] private List<CropMutation> knownMutations = new List<CropMutation>();
        [SerializeField] private float globalMutationRate = 0.01f;
        [SerializeField] private bool enableNaturalMutations = true;
        [SerializeField] private bool debugMode = false;
        
        [Header("Discovery Settings")]
        [SerializeField] private int maxMutationsPerCrop = 3;
        [SerializeField] private float discoveryExperienceBonus = 50f;
        [SerializeField] private bool autoDiscoverMutations = true;
        
        [Header("Breeding Settings")]
        [SerializeField] private float breedingSuccessRate = 0.7f;
        [SerializeField] private int maxBreedingAttempts = 10;
        [SerializeField] private bool allowInbreeding = false;
        
        // Mutation tracking
        private Dictionary<string, List<CropMutation>> _cropMutations = new Dictionary<string, List<CropMutation>>();
        private Dictionary<string, CropMutation> _discoveredMutations = new Dictionary<string, CropMutation>();
        private List<CropMutation> _activeMutations = new List<CropMutation>();
        
        // Statistics
        private int _totalMutationsDiscovered = 0;
        private int _totalBreedingAttempts = 0;
        private int _successfulBreedings = 0;
        
        // Properties
        public List<GeneticData> AvailableGenes => new List<GeneticData>(availableGenes);
        public List<CropMutation> KnownMutations => new List<CropMutation>(knownMutations);
        public List<CropMutation> DiscoveredMutations => new List<CropMutation>(_discoveredMutations.Values);
        public int TotalMutationsDiscovered => _totalMutationsDiscovered;
        public float BreedingSuccessRate => _totalBreedingAttempts > 0 ? (float)_successfulBreedings / _totalBreedingAttempts : 0f;
        public bool MutationsEnabled => enableNaturalMutations;
        
        // Events
        public event Action<CropMutation> OnMutationDiscovered;
        public event Action<CropMutation, CropEntity> OnMutationApplied;
        public event Action<CropMutation, CropMutation, CropMutation> OnSuccessfulBreeding; // parent1, parent2, offspring
        public event Action<string> OnBreedingFailed;
        public event Action<CropEntity> OnNaturalMutation;

        private void Awake()
        {
            InitializeManager();
        }

        private void Start()
        {
            LoadKnownMutations();
        }
        
        [Provide]
        public MutationManager ProvideMutationManager() => this;

        private void InitializeManager()
        {
            // Initialize crop mutation dictionary with available genes
            foreach (var gene in availableGenes)
            {
                if (gene?.compatibleCrops != null)
                {
                    foreach (var cropId in gene.compatibleCrops)
                    {
                        if (!_cropMutations.ContainsKey(cropId))
                        {
                            _cropMutations[cropId] = new List<CropMutation>();
                        }
                    }
                }
            }
        }

        private void LoadKnownMutations()
        {
            foreach (var mutation in knownMutations)
            {
                if (mutation != null)
                {
                    RegisterMutation(mutation);
                }
            }
        }

        // Mutation discovery and registration
        public void RegisterMutation(CropMutation mutation)
        {
            if (mutation?.mutationId == null) return;
            
            if (!_discoveredMutations.ContainsKey(mutation.mutationId))
            {
                _discoveredMutations[mutation.mutationId] = mutation;
                _activeMutations.Add(mutation);
                
                if (autoDiscoverMutations && !mutation.isDiscovered)
                {
                    mutation.Discover();
                    _totalMutationsDiscovered++;
                    OnMutationDiscovered?.Invoke(mutation);
                    
                    if (debugMode)
                    {
                        Debug.Log($"Discovered new mutation: {mutation.mutationName}");
                    }
                }
            }
        }

        public CropMutation CreateMutation(string name, List<GeneticData> genes)
        {
            if (genes == null || genes.Count == 0) return null;
            
            var mutation = new CropMutation(name, genes);
            RegisterMutation(mutation);
            
            return mutation;
        }

        public CropMutation CreateRandomMutation(string cropId)
        {
            var compatibleGenes = availableGenes.Where(g => g.IsCompatibleWith(cropId)).ToList();
            
            if (compatibleGenes.Count == 0) return null;
            
            // Select 1-3 random compatible genes
            int geneCount = UnityEngine.Random.Range(1, 4);
            var selectedGenes = new List<GeneticData>();
            
            for (int i = 0; i < geneCount && compatibleGenes.Count > 0; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, compatibleGenes.Count);
                var selectedGene = compatibleGenes[randomIndex];
                
                selectedGenes.Add(selectedGene);
                compatibleGenes.Remove(selectedGene);
            }
            
            string mutationName = $"Natural {cropId} Variant {UnityEngine.Random.Range(1000, 9999)}";
            return CreateMutation(mutationName, selectedGenes);
        }

        // Natural mutation system
        public bool TryNaturalMutation(CropEntity crop)
        {
            if (!enableNaturalMutations || crop?.CropData == null) return false;
            
            float mutationChance = globalMutationRate;
            
            // Increase chance based on environmental stress or special conditions
            mutationChance *= CalculateEnvironmentalMutationModifier(crop);
            
            if (UnityEngine.Random.value <= mutationChance)
            {
                return ApplyNaturalMutation(crop);
            }
            
            return false;
        }

        private float CalculateEnvironmentalMutationModifier(CropEntity crop)
        {
            float modifier = 1f;
            
            // Low water increases mutation chance
            if (crop.WaterLevel < 0.3f)
                modifier *= 1.5f;
            
            // Poor soil health increases mutation chance
            if (crop.SoilHealth < 0.5f)
                modifier *= 1.3f;
            
            // Extreme temperatures increase mutation chance
            float temp = crop.CurrentTemperature;
            if (temp < 5f || temp > 35f)
                modifier *= 1.4f;
            
            return modifier;
        }

        private bool ApplyNaturalMutation(CropEntity crop)
        {
            string cropId = crop.CropData.cropName;
            var mutation = CreateRandomMutation(cropId);
            
            if (mutation != null)
            {
                ApplyMutationToCrop(mutation, crop);
                OnNaturalMutation?.Invoke(crop);
                
                if (debugMode)
                {
                    Debug.Log($"Natural mutation occurred on {cropId}: {mutation.mutationName}");
                }
                
                return true;
            }
            
            return false;
        }

        // Mutation application
        public void ApplyMutationToCrop(CropMutation mutation, CropEntity crop)
        {
            if (mutation == null || crop?.CropData == null) return;
            
            // Apply mutation effects
            mutation.ApplyToCrop(crop);
            
            // Track applied mutation
            string cropId = crop.CropData.cropName;
            if (!_cropMutations.ContainsKey(cropId))
            {
                _cropMutations[cropId] = new List<CropMutation>();
            }
            
            if (!_cropMutations[cropId].Contains(mutation))
            {
                _cropMutations[cropId].Add(mutation);
            }
            
            OnMutationApplied?.Invoke(mutation, crop);
            
            if (debugMode)
            {
                Debug.Log($"Applied mutation {mutation.mutationName} to {cropId}");
            }
        }

        public void RemoveMutationFromCrop(CropMutation mutation, CropEntity crop)
        {
            if (mutation == null || crop?.CropData == null) return;
            
            string cropId = crop.CropData.cropName;
            if (_cropMutations.ContainsKey(cropId))
            {
                _cropMutations[cropId].Remove(mutation);
            }
            
            // Would need to implement mutation removal effects
            if (debugMode)
            {
                Debug.Log($"Removed mutation {mutation.mutationName} from {cropId}");
            }
        }

        // Breeding system
        public CropMutation AttemptBreeding(CropMutation parent1, CropMutation parent2)
        {
            _totalBreedingAttempts++;
            
            if (!CanBreed(parent1, parent2))
            {
                OnBreedingFailed?.Invoke("Parents are not compatible for breeding");
                return null;
            }
            
            float successChance = CalculateBreedingSuccess(parent1, parent2);
            
            if (UnityEngine.Random.value <= successChance)
            {
                var offspring = CropMutation.CrossBreed(parent1, parent2);
                
                if (offspring != null)
                {
                    RegisterMutation(offspring);
                    _successfulBreedings++;
                    
                    OnSuccessfulBreeding?.Invoke(parent1, parent2, offspring);
                    
                    if (debugMode)
                    {
                        Debug.Log($"Successful breeding: {parent1.mutationName} × {parent2.mutationName} = {offspring.mutationName}");
                    }
                    
                    return offspring;
                }
            }
            
            OnBreedingFailed?.Invoke("Breeding attempt failed due to genetic incompatibility");
            return null;
        }

        private bool CanBreed(CropMutation parent1, CropMutation parent2)
        {
            if (parent1 == null || parent2 == null) return false;
            if (!parent1.CanBreed || !parent2.CanBreed) return false;
            if (!allowInbreeding && parent1 == parent2) return false;
            
            // Check for genetic compatibility
            foreach (var gene1 in parent1.activeGenes)
            {
                foreach (var gene2 in parent2.activeGenes)
                {
                    if (!gene1.CanCoexistWith(gene2))
                        return false;
                }
            }
            
            return true;
        }

        private float CalculateBreedingSuccess(CropMutation parent1, CropMutation parent2)
        {
            float baseChance = breedingSuccessRate;
            
            // Stability affects breeding success
            float avgStability = (parent1.breedingStability + parent2.breedingStability) * 0.5f;
            baseChance *= avgStability;
            
            // Similar mutations breed more easily
            float similarity = CalculateGeneticSimilarity(parent1, parent2);
            baseChance *= Mathf.Lerp(0.7f, 1.3f, similarity);
            
            return Mathf.Clamp01(baseChance);
        }

        private float CalculateGeneticSimilarity(CropMutation mutation1, CropMutation mutation2)
        {
            if (mutation1.TotalGenes == 0 || mutation2.TotalGenes == 0) return 0f;
            
            int sharedGenes = 0;
            foreach (var gene1 in mutation1.activeGenes)
            {
                if (mutation2.activeGenes.Contains(gene1))
                    sharedGenes++;
            }
            
            int totalUniqueGenes = mutation1.activeGenes.Union(mutation2.activeGenes).Count();
            return totalUniqueGenes > 0 ? (float)sharedGenes / totalUniqueGenes : 0f;
        }

        // Query methods
        public List<CropMutation> GetMutationsForCrop(string cropId)
        {
            if (_cropMutations.TryGetValue(cropId, out List<CropMutation> mutations))
            {
                return new List<CropMutation>(mutations);
            }
            return new List<CropMutation>();
        }

        public List<CropMutation> GetMutationsByType(MutationType type)
        {
            return _activeMutations.Where(m => m.mutationType == type).ToList();
        }

        public List<CropMutation> GetMutationsByRarity(MutationRarity rarity)
        {
            return _activeMutations.Where(m => m.rarity == rarity).ToList();
        }

        public CropMutation GetMutationById(string mutationId)
        {
            _discoveredMutations.TryGetValue(mutationId, out CropMutation mutation);
            return mutation;
        }

        public List<GeneticData> GetCompatibleGenes(string cropId)
        {
            return availableGenes.Where(g => g.IsCompatibleWith(cropId)).ToList();
        }

        // Advanced breeding
        public List<CropMutation> GetBreedingCandidates(CropMutation targetMutation)
        {
            return _activeMutations.Where(m => CanBreed(targetMutation, m)).ToList();
        }

        public CropMutation PredictOffspring(CropMutation parent1, CropMutation parent2)
        {
            // Create a theoretical offspring without actually breeding
            return CropMutation.CrossBreed(parent1, parent2);
        }

        // Settings and configuration
        public void SetGlobalMutationRate(float rate)
        {
            globalMutationRate = Mathf.Clamp01(rate);
        }

        public void SetMutationsEnabled(bool enabled)
        {
            enableNaturalMutations = enabled;
        }

        public void SetBreedingSuccessRate(float rate)
        {
            breedingSuccessRate = Mathf.Clamp01(rate);
        }

        public void AddGeneticData(GeneticData gene)
        {
            if (gene != null && !availableGenes.Contains(gene))
            {
                availableGenes.Add(gene);
                
                // Update crop mutation dictionary
                foreach (var cropId in gene.compatibleCrops)
                {
                    if (!_cropMutations.ContainsKey(cropId))
                    {
                        _cropMutations[cropId] = new List<CropMutation>();
                    }
                }
            }
        }

        // Statistics and analytics
        public MutationStatistics GetStatistics()
        {
            return new MutationStatistics
            {
                totalMutationsDiscovered = _totalMutationsDiscovered,
                totalBreedingAttempts = _totalBreedingAttempts,
                successfulBreedings = _successfulBreedings,
                breedingSuccessRate = BreedingSuccessRate,
                activeMutations = _activeMutations.Count,
                mutationsByType = GetMutationsByTypeCount(),
                mutationsByRarity = GetMutationsByRarityCount()
            };
        }

        private Dictionary<MutationType, int> GetMutationsByTypeCount()
        {
            var counts = new Dictionary<MutationType, int>();
            foreach (MutationType type in Enum.GetValues(typeof(MutationType)))
            {
                counts[type] = _activeMutations.Count(m => m.mutationType == type);
            }
            return counts;
        }

        private Dictionary<MutationRarity, int> GetMutationsByRarityCount()
        {
            var counts = new Dictionary<MutationRarity, int>();
            foreach (MutationRarity rarity in Enum.GetValues(typeof(MutationRarity)))
            {
                counts[rarity] = _activeMutations.Count(m => m.rarity == rarity);
            }
            return counts;
        }

        // Debug methods
        public void DEBUG_CreateRandomMutations(int count)
        {
            if (!debugMode) return;
            
            for (int i = 0; i < count; i++)
            {
                string randomCropId = "TestCrop";
                CreateRandomMutation(randomCropId);
            }
        }

        public void DEBUG_ForceNaturalMutation(CropEntity crop)
        {
            if (!debugMode) return;
            ApplyNaturalMutation(crop);
        }

        public void DEBUG_PrintStatistics()
        {
            if (!debugMode) return;
            
            var stats = GetStatistics();
            Debug.Log($"Mutation Statistics:\n" +
                     $"Discovered: {stats.totalMutationsDiscovered}\n" +
                     $"Breeding Success: {stats.breedingSuccessRate:P1}\n" +
                     $"Active Mutations: {stats.activeMutations}");
        }
    }

    [System.Serializable]
    public class MutationStatistics
    {
        public int totalMutationsDiscovered;
        public int totalBreedingAttempts;
        public int successfulBreedings;
        public float breedingSuccessRate;
        public int activeMutations;
        public Dictionary<MutationType, int> mutationsByType;
        public Dictionary<MutationRarity, int> mutationsByRarity;
    }
}