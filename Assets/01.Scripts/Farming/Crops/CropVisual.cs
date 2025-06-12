using UnityEngine;

namespace GrowAGarden.Farming._01.Scripts.Farming.Crops
{
    public class CropVisual : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private bool animateGrowth = true;
        [SerializeField] private float growthAnimationSpeed = 1f;
        [SerializeField] private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Quality Indication")]
        [SerializeField] private bool showQualityColors = true;
        [SerializeField] private Color perfectColor = Color.green;
        [SerializeField] private Color goodColor = Color.yellow;
        [SerializeField] private Color averageColor = Color.white;
        [SerializeField] private Color poorColor = Color.red;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem growthParticles;
        [SerializeField] private ParticleSystem waterParticles;
        [SerializeField] private Light cropLight;
        
        // Components
        private CropEntity _cropEntity;
        private Renderer[] _renderers;
        private Vector3 _originalScale;
        private float _growthAnimationTime = 0f;
        
        // Properties
        public bool AnimateGrowth => animateGrowth;
        public float GrowthAnimationSpeed => growthAnimationSpeed;

        private void Awake()
        {
            _cropEntity = GetComponentInParent<CropEntity>();
            _renderers = GetComponentsInChildren<Renderer>();
            _originalScale = transform.localScale;
        }

        private void Start()
        {
            if (_cropEntity != null)
            {
                _cropEntity.OnStageChanged += OnCropStageChanged;
                _cropEntity.OnWatered += OnCropWatered;
                _cropEntity.OnCropDied += OnCropDied;
            }
            
            UpdateVisuals();
        }

        private void Update()
        {
            if (animateGrowth && _cropEntity != null && _cropEntity.IsGrowing)
            {
                UpdateGrowthAnimation();
            }
            
            UpdateQualityVisuals();
        }

        private void UpdateGrowthAnimation()
        {
            if (_cropEntity == null) return;
            
            float targetProgress = _cropEntity.StageProgress;
            _growthAnimationTime = Mathf.MoveTowards(_growthAnimationTime, targetProgress, 
                Time.deltaTime * growthAnimationSpeed);
            
            float animatedProgress = growthCurve.Evaluate(_growthAnimationTime);
            
            // Scale based on growth progress
            Vector3 targetScale = _originalScale * Mathf.Lerp(0.1f, 1f, animatedProgress);
            transform.localScale = targetScale;
            
            // Adjust light intensity
            if (cropLight != null)
            {
                cropLight.intensity = Mathf.Lerp(0.1f, 1f, animatedProgress);
            }
        }

        private void UpdateQualityVisuals()
        {
            if (!showQualityColors || _cropEntity == null) return;
            
            Color qualityColor = _cropEntity.Quality switch
            {
                CropQuality.Perfect => perfectColor,
                CropQuality.Good => goodColor,
                CropQuality.Average => averageColor,
                CropQuality.Poor => poorColor,
                _ => averageColor
            };
            
            ApplyColorToRenderers(qualityColor);
        }

        private void ApplyColorToRenderers(Color color)
        {
            foreach (var renderer in _renderers)
            {
                if (renderer.material != null)
                {
                    renderer.material.color = color;
                }
            }
        }

        private void OnCropStageChanged(CropEntity crop)
        {
            UpdateVisuals();
            PlayGrowthEffect();
        }

        private void OnCropWatered(CropEntity crop, float amount)
        {
            PlayWaterEffect();
        }

        private void OnCropDied(CropEntity crop)
        {
            // Show dead crop visuals
            ApplyColorToRenderers(Color.gray);
            
            if (growthParticles != null)
            {
                growthParticles.Stop();
            }
            
            if (cropLight != null)
            {
                cropLight.enabled = false;
            }
        }

        private void UpdateVisuals()
        {
            if (_cropEntity == null) return;
            
            // Reset animation time when stage changes
            _growthAnimationTime = 0f;
            
            // Update particle effects
            if (growthParticles != null && _cropEntity.IsGrowing)
            {
                if (!growthParticles.isPlaying)
                {
                    growthParticles.Play();
                }
            }
        }

        private void PlayGrowthEffect()
        {
            if (growthParticles != null)
            {
                growthParticles.Emit(10);
            }
        }

        private void PlayWaterEffect()
        {
            if (waterParticles != null)
            {
                waterParticles.Emit(5);
            }
        }

        // Public methods
        public void SetGrowthAnimationSpeed(float speed)
        {
            growthAnimationSpeed = Mathf.Max(0f, speed);
        }

        public void SetShowQualityColors(bool show)
        {
            showQualityColors = show;
            if (!show)
            {
                ApplyColorToRenderers(Color.white);
            }
        }

        public void PlayCustomEffect(ParticleSystem particles, int count = 10)
        {
            if (particles != null)
            {
                particles.Emit(count);
            }
        }

        private void OnDestroy()
        {
            if (_cropEntity != null)
            {
                _cropEntity.OnStageChanged -= OnCropStageChanged;
                _cropEntity.OnWatered -= OnCropWatered;
                _cropEntity.OnCropDied -= OnCropDied;
            }
        }
    }
}