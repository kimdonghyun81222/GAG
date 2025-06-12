using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public abstract class BaseAnimation : IAnimationSequence
    {
        protected float duration;
        protected float currentTime;
        protected bool isPlaying;
        protected bool isPaused;
        protected bool isCompleted;
        protected AnimationCurve easingCurve;
        
        // Properties
        public bool IsPlaying => isPlaying;
        public bool IsPaused => isPaused;
        public bool IsCompleted => isCompleted;
        public float Progress => duration > 0 ? Mathf.Clamp01(currentTime / duration) : 1f;
        public float NormalizedTime => easingCurve != null ? easingCurve.Evaluate(Progress) : Progress;
        
        // Events
        public System.Action OnCompleted;
        public System.Action OnStarted;

        protected BaseAnimation(float duration)
        {
            this.duration = duration;
            this.currentTime = 0f;
            this.isPlaying = false;
            this.isPaused = false;
            this.isCompleted = false;
            this.easingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }

        public virtual void Play()
        {
            isPlaying = true;
            isPaused = false;
            isCompleted = false;
            currentTime = 0f;
            OnStarted?.Invoke();
        }

        public virtual void Pause()
        {
            isPaused = true;
        }

        public virtual void Resume()
        {
            isPaused = false;
        }

        public virtual void Stop()
        {
            isPlaying = false;
            isPaused = false;
            currentTime = 0f;
        }

        public virtual void Complete()
        {
            currentTime = duration;
            isCompleted = true;
            isPlaying = false;
            UpdateAnimation(0f); // Apply final state
            OnCompleted?.Invoke();
        }

        public virtual void UpdateAnimation(float deltaTime)
        {
            if (!isPlaying || isPaused || isCompleted) return;
            
            currentTime += deltaTime;
            
            if (currentTime >= duration)
            {
                Complete();
            }
            else
            {
                ApplyAnimation();
            }
        }

        public BaseAnimation SetEasing(AnimationCurve curve)
        {
            easingCurve = curve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
            return this;
        }

        public BaseAnimation SetDuration(float newDuration)
        {
            duration = Mathf.Max(0f, newDuration);
            return this;
        }

        public BaseAnimation OnComplete(System.Action callback)
        {
            OnCompleted += callback;
            return this;
        }

        public BaseAnimation OnStart(System.Action callback)
        {
            OnStarted += callback;
            return this;
        }

        protected abstract void ApplyAnimation();
    }
}