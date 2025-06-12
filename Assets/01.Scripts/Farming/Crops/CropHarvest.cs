using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Crops
{
    public class CropHarvest : MonoBehaviour
    {
        [Header("Harvest Settings")]
        [SerializeField] private CropDataSO harvestedCrop;
        [SerializeField] private CropQuality harvestQuality = CropQuality.Average;
        [SerializeField] private int harvestAmount = 1;
        [SerializeField] private int experienceGained = 10;
        [SerializeField] private int goldValue = 50;
        
        [Header("Visual")]
        [SerializeField] private GameObject harvestPrefab;
        [SerializeField] private ParticleSystem harvestParticles;
        [SerializeField] private Color qualityColor = Color.white;
        
        [Header("Audio")]
        [SerializeField] private AudioClip harvestSound;
        [SerializeField] private float soundVolume = 0.7f;
        
        // Properties
        public CropDataSO HarvestedCrop => harvestedCrop;
        public CropQuality HarvestQuality => harvestQuality;
        public int HarvestAmount => harvestAmount;
        public int ExperienceGained => experienceGained;
        public int GoldValue => goldValue;
        
        // Events
        public System.Action<CropHarvest> OnHarvested;

        private void Start()
        {
            ApplyQualityVisuals();
        }

        private void ApplyQualityVisuals()
        {
            // Set quality color
            qualityColor = harvestQuality switch
            {
                CropQuality.Perfect => Color.green,
                CropQuality.Good => Color.yellow,
                CropQuality.Average => Color.white,
                CropQuality.Poor => Color.red,
                _ => Color.white
            };
            
            // Apply to renderers
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.material != null)
                {
                    renderer.material.color = qualityColor;
                }
            }
        }

        public void Harvest()
        {
            PlayHarvestEffects();
            OnHarvested?.Invoke(this);
            
            // Create harvest result
            var result = new CropHarvestResult
            {
                cropData = harvestedCrop,
                quality = harvestQuality,
                amount = harvestAmount,
                value = goldValue,
                experience = experienceGained
            };
            
            // TODO: Add to player inventory
            
            // Destroy harvest object
            Destroy(gameObject);
        }

        private void PlayHarvestEffects()
        {
            // Play harvest particles
            if (harvestParticles != null)
            {
                harvestParticles.Play();
            }
            
            // Play harvest sound
            if (harvestSound != null)
            {
                AudioSource.PlayClipAtPoint(harvestSound, transform.position, soundVolume);
            }
        }

        public void SetHarvestData(CropHarvestResult result)
        {
            if (result == null) return;
            
            harvestedCrop = result.cropData;
            harvestQuality = result.quality;
            harvestAmount = result.amount;
            experienceGained = result.experience;
            goldValue = result.value;
            
            ApplyQualityVisuals();
        }

        public static GameObject CreateHarvestObject(CropHarvestResult result, Vector3 position, Transform parent = null)
        {
            if (result?.cropData == null) return null;
            
            // Create basic harvest object
            var harvestObj = new GameObject($"{result.cropData.cropName} Harvest");
            harvestObj.transform.position = position;
            harvestObj.transform.SetParent(parent);
            
            // Add CropHarvest component
            var cropHarvest = harvestObj.AddComponent<CropHarvest>();
            cropHarvest.SetHarvestData(result);
            
            // Add visual representation
            if (result.cropData.cropIcon != null)
            {
                var spriteRenderer = harvestObj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = result.cropData.cropIcon;
            }
            
            // Add collider for interaction
            var collider = harvestObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            
            return harvestObj;
        }
    }
}