using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.VFX
{
    public class VFXInstance : MonoBehaviour
    {
        [Header("VFX Settings")]
        [SerializeField] private float duration = 2f;
        [SerializeField] private bool autoDestroy = true;
        [SerializeField] private bool followTarget = false;
        [SerializeField] private Transform target;
        
        // State
        private float _currentTime = 0f;
        private bool _isPlaying = false;
        private ParticleSystem[] _particleSystems;
        private AudioSource _audioSource;
        
        // Properties
        public float Duration => duration;
        public bool IsPlaying => _isPlaying;
        public bool IsCompleted => _currentTime >= duration;
        public float Progress => duration > 0 ? Mathf.Clamp01(_currentTime / duration) : 1f;
        
        // Events
        public System.Action<VFXInstance> OnCompleted;
        public System.Action<VFXInstance> OnStarted;

        private void Awake()
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            
            _currentTime += Time.deltaTime;
            
            // Follow target if enabled
            if (followTarget && target != null)
            {
                transform.position = target.position;
            }
            
            // Check completion
            if (_currentTime >= duration)
            {
                Complete();
            }
        }

        public void Play()
        {
            _isPlaying = true;
            _currentTime = 0f;
            
            // Play particle systems
            foreach (var ps in _particleSystems)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }
            
            // Play audio
            if (_audioSource != null && _audioSource.clip != null)
            {
                _audioSource.Play();
            }
            
            OnStarted?.Invoke(this);
        }

        public void Stop()
        {
            _isPlaying = false;
            
            // Stop particle systems
            foreach (var ps in _particleSystems)
            {
                if (ps != null)
                {
                    ps.Stop();
                }
            }
            
            // Stop audio
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }

        public void Complete()
        {
            _isPlaying = false;
            _currentTime = duration;
            
            OnCompleted?.Invoke(this);
            
            if (autoDestroy)
            {
                DestroyVFX();
            }
        }

        public void SetDuration(float newDuration)
        {
            duration = Mathf.Max(0f, newDuration);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetFollowTarget(bool follow)
        {
            followTarget = follow;
        }

        public void SetAutoDestroy(bool destroy)
        {
            autoDestroy = destroy;
        }

        public void DestroyVFX()
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        // Static creation method
        public static VFXInstance Create(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null) return null;
            
            GameObject instance = Instantiate(prefab, position, rotation, parent);
            VFXInstance vfxInstance = instance.GetComponent<VFXInstance>();
            
            if (vfxInstance == null)
            {
                vfxInstance = instance.AddComponent<VFXInstance>();
            }
            
            return vfxInstance;
        }
    }
}