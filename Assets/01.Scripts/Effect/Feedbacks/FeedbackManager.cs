using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Feedbacks
{
    [Provide]
    public class FeedbackManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Feedback Settings")]
        [SerializeField] private bool enableFeedbacks = true;
        [SerializeField] private float globalIntensity = 1f;
        [SerializeField] private bool debugMode = false;
        
        // Dependencies
        [Inject] private Camera playerCamera;
        
        // Feedback tracking
        private List<Feedback> _activeFeedbacks = new List<Feedback>();
        private Dictionary<string, FeedbackProfile> _feedbackProfiles = new Dictionary<string, FeedbackProfile>();
        
        // Camera shake
        private Vector3 _originalCameraPosition;
        private bool _isCameraShaking = false;
        
        // Properties
        public bool FeedbacksEnabled => enableFeedbacks;
        public float GlobalIntensity => globalIntensity;
        public int ActiveFeedbackCount => _activeFeedbacks.Count;

        private void Awake()
        {
            LoadDefaultFeedbackProfiles();
        }

        private void Start()
        {
            if (playerCamera != null)
            {
                _originalCameraPosition = playerCamera.transform.localPosition;
            }
        }

        private void Update()
        {
            UpdateActiveFeedbacks();
        }

        private void LateUpdate()
        {
            // Reset camera position if not shaking
            if (!_isCameraShaking && playerCamera != null)
            {
                playerCamera.transform.localPosition = _originalCameraPosition;
            }
        }
        
        [Provide]
        public FeedbackManager ProvideFeedbackManager() => this;

        private void LoadDefaultFeedbackProfiles()
        {
            // Create default feedback profiles
            var impactProfile = new FeedbackProfile
            {
                name = "Impact",
                cameraShake = new CameraShakeFeedback { intensity = 0.5f, duration = 0.2f },
                screenFlash = new ScreenFlashFeedback { color = Color.white, intensity = 0.3f, duration = 0.1f }
            };
            
            var explosionProfile = new FeedbackProfile
            {
                name = "Explosion",
                cameraShake = new CameraShakeFeedback { intensity = 1.0f, duration = 0.5f },
                screenFlash = new ScreenFlashFeedback { color = Color.red, intensity = 0.5f, duration = 0.2f }
            };
            
            _feedbackProfiles["Impact"] = impactProfile;
            _feedbackProfiles["Explosion"] = explosionProfile;
        }

        private void UpdateActiveFeedbacks()
        {
            _isCameraShaking = false;
            
            for (int i = _activeFeedbacks.Count - 1; i >= 0; i--)
            {
                var feedback = _activeFeedbacks[i];
                
                if (feedback.IsCompleted)
                {
                    _activeFeedbacks.RemoveAt(i);
                    continue;
                }
                
                feedback.Update(Time.deltaTime);
                
                // Apply camera shake
                if (feedback is CameraShakeFeedback shake && shake.IsActive)
                {
                    ApplyCameraShake(shake);
                    _isCameraShaking = true;
                }
            }
        }

        // Public feedback methods
        public void PlayFeedback(string profileName, float intensityMultiplier = 1f)
        {
            if (!enableFeedbacks) return;
            
            if (_feedbackProfiles.TryGetValue(profileName, out FeedbackProfile profile))
            {
                PlayFeedbackProfile(profile, intensityMultiplier);
            }
            else if (debugMode)
            {
                Debug.LogWarning($"Feedback profile '{profileName}' not found");
            }
        }

        public void PlayCameraShake(float intensity, float duration)
        {
            if (!enableFeedbacks) return;
            
            var shake = new CameraShakeFeedback
            {
                intensity = intensity * globalIntensity,
                duration = duration
            };
            
            shake.Play();
            _activeFeedbacks.Add(shake);
            
            if (debugMode)
            {
                Debug.Log($"Camera shake: intensity={intensity}, duration={duration}");
            }
        }

        public void PlayScreenFlash(Color color, float intensity, float duration)
        {
            if (!enableFeedbacks) return;
            
            var flash = new ScreenFlashFeedback
            {
                color = color,
                intensity = intensity * globalIntensity,
                duration = duration
            };
            
            flash.Play();
            _activeFeedbacks.Add(flash);
            
            if (debugMode)
            {
                Debug.Log($"Screen flash: color={color}, intensity={intensity}, duration={duration}");
            }
        }

        private void PlayFeedbackProfile(FeedbackProfile profile, float intensityMultiplier)
        {
            float finalIntensity = globalIntensity * intensityMultiplier;
            
            // Play camera shake
            if (profile.cameraShake != null)
            {
                var shake = new CameraShakeFeedback
                {
                    intensity = profile.cameraShake.intensity * finalIntensity,
                    duration = profile.cameraShake.duration
                };
                shake.Play();
                _activeFeedbacks.Add(shake);
            }
            
            // Play screen flash
            if (profile.screenFlash != null)
            {
                var flash = new ScreenFlashFeedback
                {
                    color = profile.screenFlash.color,
                    intensity = profile.screenFlash.intensity * finalIntensity,
                    duration = profile.screenFlash.duration
                };
                flash.Play();
                _activeFeedbacks.Add(flash);
            }
            
            if (debugMode)
            {
                Debug.Log($"Played feedback profile: {profile.name}");
            }
        }

        private void ApplyCameraShake(CameraShakeFeedback shake)
        {
            if (playerCamera == null) return;
            
            Vector3 shakeOffset = Random.insideUnitSphere * shake.CurrentIntensity;
            shakeOffset.z = 0f; // Don't shake forward/backward
            
            playerCamera.transform.localPosition = _originalCameraPosition + shakeOffset;
        }

        // Profile management
        public void RegisterFeedbackProfile(string name, FeedbackProfile profile)
        {
            _feedbackProfiles[name] = profile;
        }

        public void RemoveFeedbackProfile(string name)
        {
            _feedbackProfiles.Remove(name);
        }

        public bool HasFeedbackProfile(string name)
        {
            return _feedbackProfiles.ContainsKey(name);
        }

        // Settings
        public void SetGlobalIntensity(float intensity)
        {
            globalIntensity = Mathf.Clamp01(intensity);
        }

        public void SetFeedbacksEnabled(bool enabled)
        {
            enableFeedbacks = enabled;
            
            if (!enabled)
            {
                StopAllFeedbacks();
            }
        }

        public void StopAllFeedbacks()
        {
            foreach (var feedback in _activeFeedbacks)
            {
                feedback.Stop();
            }
            
            _activeFeedbacks.Clear();
            _isCameraShaking = false;
            
            if (playerCamera != null)
            {
                playerCamera.transform.localPosition = _originalCameraPosition;
            }
        }
    }

    [System.Serializable]
    public class FeedbackProfile
    {
        public string name;
        public CameraShakeFeedback cameraShake;
        public ScreenFlashFeedback screenFlash;
    }

    public abstract class Feedback
    {
        public float duration;
        public float currentTime;
        public bool isActive;
        
        public bool IsCompleted => currentTime >= duration;
        public bool IsActive => isActive && !IsCompleted;
        public float Progress => duration > 0 ? Mathf.Clamp01(currentTime / duration) : 1f;
        
        public virtual void Play()
        {
            isActive = true;
            currentTime = 0f;
        }
        
        public virtual void Stop()
        {
            isActive = false;
        }
        
        public virtual void Update(float deltaTime)
        {
            if (isActive)
            {
                currentTime += deltaTime;
            }
        }
    }

    [System.Serializable]
    public class CameraShakeFeedback : Feedback
    {
        public float intensity;
        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        public float CurrentIntensity => intensity * intensityCurve.Evaluate(Progress);
    }

    [System.Serializable]
    public class ScreenFlashFeedback : Feedback
    {
        public Color color = Color.white;
        public float intensity;
        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        public float CurrentIntensity => intensity * intensityCurve.Evaluate(Progress);
        public Color CurrentColor => new Color(color.r, color.g, color.b, CurrentIntensity);
    }
}