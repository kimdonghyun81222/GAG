using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Crops
{
    public class CropStageComponent : MonoBehaviour
    {
        [Header("Stage Settings")]
        [SerializeField] private string stageName;
        [SerializeField] private CropStageComponentType stageType;
        [SerializeField] private bool canHarvest = false;
        [SerializeField] private bool canWater = true;
        
        [Header("Visual")]
        [SerializeField] private GameObject stageModel;
        [SerializeField] private ParticleSystem growthParticles;
        [SerializeField] private Color stageColor = Color.green;
        
        [Header("Audio")]
        [SerializeField] private AudioClip stageSound;
        [SerializeField] private float soundVolume = 0.5f;
        
        // Properties
        public string StageName => stageName;
        public CropStageComponentType StageType => stageType;
        public bool CanHarvest => canHarvest;
        public bool CanWater => canWater;
        public Color StageColor => stageColor;
        
        // Components
        private Renderer[] _renderers;
        private AudioSource _audioSource;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _audioSource = GetComponent<AudioSource>();
            
            if (_audioSource == null && stageSound != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.volume = soundVolume;
            }
        }

        private void Start()
        {
            ApplyStageVisuals();
            PlayStageEffects();
        }

        private void ApplyStageVisuals()
        {
            // Apply stage color to renderers
            foreach (var renderer in _renderers)
            {
                if (renderer.material != null)
                {
                    renderer.material.color = stageColor;
                }
            }
            
            // Activate stage model
            if (stageModel != null)
            {
                stageModel.SetActive(true);
            }
        }

        private void PlayStageEffects()
        {
            // Play growth particles
            if (growthParticles != null)
            {
                growthParticles.Play();
            }
            
            // Play stage sound
            if (_audioSource != null && stageSound != null)
            {
                _audioSource.clip = stageSound;
                _audioSource.Play();
            }
        }

        public void SetStageColor(Color color)
        {
            stageColor = color;
            ApplyStageVisuals();
        }

        public void PlayGrowthEffect()
        {
            if (growthParticles != null)
            {
                growthParticles.Play();
            }
        }

        public void StopGrowthEffect()
        {
            if (growthParticles != null)
            {
                growthParticles.Stop();
            }
        }
    }
    
    public enum CropStageComponentType
    {
        Seed,
        Sprout,
        Seedling,
        Vegetative,
        Flowering,
        Fruiting,
        Mature,
        Dying
    }
}