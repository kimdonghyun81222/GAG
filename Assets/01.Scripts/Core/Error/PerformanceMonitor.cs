using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.Core._01.Scripts.Core.Error
{
    [Provide]
    public class PerformanceMonitor : MonoBehaviour, IDependencyProvider
    {
        [Header("Performance Settings")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private float updateInterval = 1f;
        [SerializeField] private int maxSamples = 60;
        [SerializeField] private float lowFPSThreshold = 30f;
        [SerializeField] private float criticalFPSThreshold = 15f;
        
        // Performance data
        private Queue<float> _fpsHistory = new Queue<float>();
        private Queue<float> _memoryHistory = new Queue<float>();
        private float _lastUpdateTime;
        private float _deltaTime;
        
        // Properties
        public float CurrentFPS { get; private set; }
        public float AverageFPS { get; private set; }
        public float CurrentMemoryUsage { get; private set; }
        public bool IsPerformanceLow => CurrentFPS < lowFPSThreshold;
        public bool IsPerformanceCritical => CurrentFPS < criticalFPSThreshold;
        
        // Events
        public System.Action<float> OnFPSUpdated;
        public System.Action<float> OnMemoryUpdated;
        public System.Action OnLowPerformance;
        public System.Action OnCriticalPerformance;

        private void Start()
        {
            _lastUpdateTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            if (!enableMonitoring) return;
            
            UpdatePerformanceMetrics();
            
            if (Time.realtimeSinceStartup - _lastUpdateTime >= updateInterval)
            {
                UpdatePerformanceHistory();
                CheckPerformanceThresholds();
                _lastUpdateTime = Time.realtimeSinceStartup;
            }
        }
        
        [Provide]
        public PerformanceMonitor ProvidePerformanceMonitor() => this;

        private void UpdatePerformanceMetrics()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            CurrentFPS = 1.0f / _deltaTime;
            
            // Memory usage in MB
            CurrentMemoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        }

        private void UpdatePerformanceHistory()
        {
            // Update FPS history
            _fpsHistory.Enqueue(CurrentFPS);
            if (_fpsHistory.Count > maxSamples)
            {
                _fpsHistory.Dequeue();
            }
            
            // Update memory history
            _memoryHistory.Enqueue(CurrentMemoryUsage);
            if (_memoryHistory.Count > maxSamples)
            {
                _memoryHistory.Dequeue();
            }
            
            // Calculate average FPS
            float totalFPS = 0f;
            foreach (float fps in _fpsHistory)
            {
                totalFPS += fps;
            }
            AverageFPS = totalFPS / _fpsHistory.Count;
            
            // Trigger events
            OnFPSUpdated?.Invoke(CurrentFPS);
            OnMemoryUpdated?.Invoke(CurrentMemoryUsage);
        }

        private void CheckPerformanceThresholds()
        {
            if (CurrentFPS < criticalFPSThreshold)
            {
                OnCriticalPerformance?.Invoke();
                GameErrorHandler.LogWarning($"Critical performance detected: {CurrentFPS:F1} FPS");
            }
            else if (CurrentFPS < lowFPSThreshold)
            {
                OnLowPerformance?.Invoke();
                GameErrorHandler.LogWarning($"Low performance detected: {CurrentFPS:F1} FPS");
            }
        }

        public PerformanceSnapshot GetPerformanceSnapshot()
        {
            return new PerformanceSnapshot
            {
                currentFPS = CurrentFPS,
                averageFPS = AverageFPS,
                minFPS = GetMinFPS(),
                maxFPS = GetMaxFPS(),
                currentMemory = CurrentMemoryUsage,
                averageMemory = GetAverageMemory(),
                timestamp = System.DateTime.UtcNow
            };
        }

        private float GetMinFPS()
        {
            if (_fpsHistory.Count == 0) return 0f;
            
            float min = float.MaxValue;
            foreach (float fps in _fpsHistory)
            {
                if (fps < min) min = fps;
            }
            return min;
        }

        private float GetMaxFPS()
        {
            if (_fpsHistory.Count == 0) return 0f;
            
            float max = 0f;
            foreach (float fps in _fpsHistory)
            {
                if (fps > max) max = fps;
            }
            return max;
        }

        private float GetAverageMemory()
        {
            if (_memoryHistory.Count == 0) return 0f;
            
            float total = 0f;
            foreach (float memory in _memoryHistory)
            {
                total += memory;
            }
            return total / _memoryHistory.Count;
        }

        public void SetLowFPSThreshold(float threshold)
        {
            lowFPSThreshold = Mathf.Max(1f, threshold);
        }

        public void SetCriticalFPSThreshold(float threshold)
        {
            criticalFPSThreshold = Mathf.Max(1f, threshold);
        }

        public void ClearHistory()
        {
            _fpsHistory.Clear();
            _memoryHistory.Clear();
        }
    }

    [System.Serializable]
    public class PerformanceSnapshot
    {
        public float currentFPS;
        public float averageFPS;
        public float minFPS;
        public float maxFPS;
        public float currentMemory;
        public float averageMemory;
        public System.DateTime timestamp;
        
        public override string ToString()
        {
            return $"FPS: {currentFPS:F1} (Avg: {averageFPS:F1}, Min: {minFPS:F1}, Max: {maxFPS:F1}) | Memory: {currentMemory:F1}MB";
        }
    }
}