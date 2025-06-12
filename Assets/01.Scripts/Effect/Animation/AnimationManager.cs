using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    [Provide]
    public class AnimationManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Animation Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private float globalAnimationSpeed = 1f;
        [SerializeField] private AnimationCurve defaultEaseInOut;
        
        // Animation tracking
        private List<IAnimationSequence> _activeAnimations = new List<IAnimationSequence>();
        private Dictionary<string, AnimationSequence> _namedAnimations = new Dictionary<string, AnimationSequence>();
        
        // Properties
        public float GlobalAnimationSpeed => globalAnimationSpeed;
        public int ActiveAnimationCount => _activeAnimations.Count;
        
        // Events
        public System.Action<IAnimationSequence> OnAnimationStarted;
        public System.Action<IAnimationSequence> OnAnimationCompleted;

        private void Awake()
        {
            // Setup default easing curve if not set
            if (defaultEaseInOut == null || defaultEaseInOut.keys.Length == 0)
            {
                defaultEaseInOut = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }
        }

        private void Update()
        {
            UpdateActiveAnimations();
        }
        
        [Provide]
        public AnimationManager ProvideAnimationManager() => this;

        private void UpdateActiveAnimations()
        {
            for (int i = _activeAnimations.Count - 1; i >= 0; i--)
            {
                var animation = _activeAnimations[i];
                
                if (animation == null || animation.IsCompleted)
                {
                    _activeAnimations.RemoveAt(i);
                    continue;
                }
                
                animation.UpdateAnimation(Time.deltaTime * globalAnimationSpeed);
            }
        }

        // Animation creation methods
        public AnimationSequence CreateAnimation(string name = null)
        {
            var sequence = new AnimationSequence(this);
            
            if (!string.IsNullOrEmpty(name))
            {
                _namedAnimations[name] = sequence;
            }
            
            return sequence;
        }

        public PositionAnimation AnimatePosition(Transform target, Vector3 endPosition, float duration)
        {
            var animation = new PositionAnimation(target, target.position, endPosition, duration);
            RegisterAnimation(animation);
            return animation;
        }

        public RotationAnimation AnimateRotation(Transform target, Quaternion endRotation, float duration)
        {
            var animation = new RotationAnimation(target, target.rotation, endRotation, duration);
            RegisterAnimation(animation);
            return animation;
        }

        public ScaleAnimation AnimateScale(Transform target, Vector3 endScale, float duration)
        {
            var animation = new ScaleAnimation(target, target.localScale, endScale, duration);
            RegisterAnimation(animation);
            return animation;
        }

        public ColorAnimation AnimateColor(Renderer target, Color endColor, float duration)
        {
            if (target.material == null) return null;
            
            var animation = new ColorAnimation(target, target.material.color, endColor, duration);
            RegisterAnimation(animation);
            return animation;
        }

        public FloatAnimation AnimateFloat(float startValue, float endValue, float duration, System.Action<float> onUpdate)
        {
            var animation = new FloatAnimation(startValue, endValue, duration, onUpdate);
            RegisterAnimation(animation);
            return animation;
        }

        // Animation management
        public void RegisterAnimation(IAnimationSequence animation)
        {
            if (animation == null) return;
            
            _activeAnimations.Add(animation);
            OnAnimationStarted?.Invoke(animation);
            
            if (debugMode)
            {
                Debug.Log($"Animation started: {animation.GetType().Name}");
            }
        }

        public void StopAnimation(IAnimationSequence animation)
        {
            if (animation == null) return;
            
            animation.Stop();
            _activeAnimations.Remove(animation);
            
            if (debugMode)
            {
                Debug.Log($"Animation stopped: {animation.GetType().Name}");
            }
        }

        public void StopAnimation(string name)
        {
            if (_namedAnimations.TryGetValue(name, out AnimationSequence animation))
            {
                StopAnimation(animation);
                _namedAnimations.Remove(name);
            }
        }

        public void StopAllAnimations()
        {
            foreach (var animation in _activeAnimations)
            {
                animation?.Stop();
            }
            
            _activeAnimations.Clear();
            _namedAnimations.Clear();
            
            if (debugMode)
            {
                Debug.Log("All animations stopped");
            }
        }

        public AnimationSequence GetNamedAnimation(string name)
        {
            _namedAnimations.TryGetValue(name, out AnimationSequence animation);
            return animation;
        }

        public bool IsAnimationPlaying(string name)
        {
            return _namedAnimations.ContainsKey(name) && !_namedAnimations[name].IsCompleted;
        }

        // Global settings
        public void SetGlobalAnimationSpeed(float speed)
        {
            globalAnimationSpeed = Mathf.Max(0f, speed);
        }

        public void PauseAllAnimations()
        {
            foreach (var animation in _activeAnimations)
            {
                animation?.Pause();
            }
        }

        public void ResumeAllAnimations()
        {
            foreach (var animation in _activeAnimations)
            {
                animation?.Resume();
            }
        }

        // Utility methods
        public AnimationCurve GetDefaultEaseInOut()
        {
            return defaultEaseInOut;
        }

        public void CompleteAnimation(IAnimationSequence animation)
        {
            OnAnimationCompleted?.Invoke(animation);
            
            if (debugMode)
            {
                Debug.Log($"Animation completed: {animation.GetType().Name}");
            }
        }

        private void OnDestroy()
        {
            StopAllAnimations();
        }
    }
}