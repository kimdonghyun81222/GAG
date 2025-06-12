using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public class AnimationSequence : IAnimationSequence
    {
        private List<IAnimationSequence> _animations = new List<IAnimationSequence>();
        private List<IAnimationSequence> _parallelAnimations = new List<IAnimationSequence>();
        private AnimationManager _manager;
        private int _currentIndex = 0;
        private bool _isPlaying = false;
        private bool _isPaused = false;
        private bool _isCompleted = false;
        
        // Properties
        public bool IsPlaying => _isPlaying;
        public bool IsPaused => _isPaused;
        public bool IsCompleted => _isCompleted;
        public float Progress => CalculateProgress();
        
        // Events
        public System.Action OnCompleted;
        public System.Action OnStarted;

        public AnimationSequence(AnimationManager manager)
        {
            _manager = manager;
        }

        // Sequence building methods
        public AnimationSequence Then(IAnimationSequence animation)
        {
            _animations.Add(animation);
            return this;
        }

        public AnimationSequence Join(IAnimationSequence animation)
        {
            _parallelAnimations.Add(animation);
            return this;
        }

        public AnimationSequence Wait(float duration)
        {
            var waitAnimation = new WaitAnimation(duration);
            _animations.Add(waitAnimation);
            return this;
        }

        public AnimationSequence Callback(System.Action callback)
        {
            var callbackAnimation = new CallbackAnimation(callback);
            _animations.Add(callbackAnimation);
            return this;
        }

        // Playback control
        public void Play()
        {
            if (_isPlaying) return;
            
            _isPlaying = true;
            _isPaused = false;
            _isCompleted = false;
            _currentIndex = 0;
            
            _manager.RegisterAnimation(this);
            OnStarted?.Invoke();
        }

        public void Pause()
        {
            _isPaused = true;
            
            // Pause current animations
            if (_currentIndex < _animations.Count)
            {
                _animations[_currentIndex]?.Pause();
            }
            
            foreach (var parallel in _parallelAnimations)
            {
                parallel?.Pause();
            }
        }

        public void Resume()
        {
            _isPaused = false;
            
            // Resume current animations
            if (_currentIndex < _animations.Count)
            {
                _animations[_currentIndex]?.Resume();
            }
            
            foreach (var parallel in _parallelAnimations)
            {
                parallel?.Resume();
            }
        }

        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;
            
            // Stop all animations
            foreach (var animation in _animations)
            {
                animation?.Stop();
            }
            
            foreach (var parallel in _parallelAnimations)
            {
                parallel?.Stop();
            }
        }

        public void Complete()
        {
            // Complete all animations instantly
            foreach (var animation in _animations)
            {
                animation?.Complete();
            }
            
            foreach (var parallel in _parallelAnimations)
            {
                parallel?.Complete();
            }
            
            _isCompleted = true;
            _isPlaying = false;
            OnCompleted?.Invoke();
            _manager.CompleteAnimation(this);
        }

        public void UpdateAnimation(float deltaTime)
        {
            if (!_isPlaying || _isPaused || _isCompleted) return;
            
            // Update parallel animations
            UpdateParallelAnimations(deltaTime);
            
            // Update sequential animations
            if (_currentIndex < _animations.Count)
            {
                var currentAnimation = _animations[_currentIndex];
                currentAnimation.UpdateAnimation(deltaTime);
                
                if (currentAnimation.IsCompleted)
                {
                    _currentIndex++;
                }
            }
            
            // Check if sequence is completed
            if (_currentIndex >= _animations.Count && AllParallelAnimationsCompleted())
            {
                Complete();
            }
        }

        private void UpdateParallelAnimations(float deltaTime)
        {
            foreach (var parallel in _parallelAnimations)
            {
                if (!parallel.IsCompleted)
                {
                    parallel.UpdateAnimation(deltaTime);
                }
            }
        }

        private bool AllParallelAnimationsCompleted()
        {
            foreach (var parallel in _parallelAnimations)
            {
                if (!parallel.IsCompleted) return false;
            }
            return true;
        }

        private float CalculateProgress()
        {
            if (_isCompleted) return 1f;
            if (!_isPlaying) return 0f;
            
            int totalAnimations = _animations.Count + _parallelAnimations.Count;
            if (totalAnimations == 0) return 1f;
            
            float sequentialProgress = _animations.Count > 0 ? (float)_currentIndex / _animations.Count : 1f;
            
            // Add current animation progress
            if (_currentIndex < _animations.Count)
            {
                sequentialProgress += _animations[_currentIndex].Progress / _animations.Count;
            }
            
            return Mathf.Clamp01(sequentialProgress);
        }
    }
}