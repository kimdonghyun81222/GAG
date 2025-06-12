using System;
using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.Core._01.Scripts.Core.Error
{
    [Provide]
    public class GameErrorHandler : MonoBehaviour, IDependencyProvider
    {
        [Header("Error Handling Settings")]
        [SerializeField] private bool logErrors = true;
        [SerializeField] private bool showErrorsInUI = true;
        [SerializeField] private int maxStoredErrors = 100;
        [SerializeField] private string logFilePath = "Logs/game_errors.log";
        
        // Error storage
        private Queue<GameError> _recentErrors = new Queue<GameError>();
        private List<GameError> _criticalErrors = new List<GameError>();
        
        // Properties
        public IReadOnlyCollection<GameError> RecentErrors => _recentErrors.ToArray();
        public IReadOnlyCollection<GameError> CriticalErrors => _criticalErrors.AsReadOnly();
        
        // Events
        public static event Action<GameError> OnErrorOccurred;
        public static event Action<GameError> OnCriticalError;

        private void Awake()
        {
            // Register Unity log callback
            Application.logMessageReceived += HandleUnityLogMessage;
        }
        
        [Provide]
        public GameErrorHandler ProvideGameErrorHandler() => this;

        private void HandleUnityLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                var gameError = new GameError
                {
                    message = condition,
                    stackTrace = stackTrace,
                    timestamp = DateTime.UtcNow,
                    severity = type == LogType.Exception ? ErrorSeverity.Critical : ErrorSeverity.Error,
                    source = "Unity"
                };
                
                HandleError(gameError);
            }
        }

        public static void LogError(string message, Exception exception = null, ErrorSeverity severity = ErrorSeverity.Error)
        {
            var gameError = new GameError
            {
                message = message,
                exception = exception,
                stackTrace = exception?.StackTrace ?? Environment.StackTrace,
                timestamp = DateTime.UtcNow,
                severity = severity,
                source = "Game"
            };
            
            if (Instance != null)
            {
                Instance.HandleError(gameError);
            }
            else
            {
                Debug.LogError($"[GameErrorHandler] {message}");
            }
        }

        public static void LogWarning(string message)
        {
            var gameError = new GameError
            {
                message = message,
                timestamp = DateTime.UtcNow,
                severity = ErrorSeverity.Warning,
                source = "Game"
            };
            
            if (Instance != null)
            {
                Instance.HandleError(gameError);
            }
            else
            {
                Debug.LogWarning($"[GameErrorHandler] {message}");
            }
        }

        public static void LogInfo(string message)
        {
            var gameError = new GameError
            {
                message = message,
                timestamp = DateTime.UtcNow,
                severity = ErrorSeverity.Info,
                source = "Game"
            };
            
            if (Instance != null)
            {
                Instance.HandleError(gameError);
            }
            else
            {
                Debug.Log($"[GameErrorHandler] {message}");
            }
        }

        private void HandleError(GameError error)
        {
            // Store error
            StoreError(error);
            
            // Log to Unity console
            if (logErrors)
            {
                LogToUnityConsole(error);
            }
            
            // Show in UI
            if (showErrorsInUI)
            {
                ShowErrorInUI(error);
            }
            
            // Trigger events
            OnErrorOccurred?.Invoke(error);
            
            if (error.severity == ErrorSeverity.Critical)
            {
                OnCriticalError?.Invoke(error);
            }
        }

        private void StoreError(GameError error)
        {
            // Add to recent errors queue
            _recentErrors.Enqueue(error);
            
            // Maintain max size
            while (_recentErrors.Count > maxStoredErrors)
            {
                _recentErrors.Dequeue();
            }
            
            // Store critical errors separately
            if (error.severity == ErrorSeverity.Critical)
            {
                _criticalErrors.Add(error);
            }
        }

        private void LogToUnityConsole(GameError error)
        {
            string logMessage = $"[{error.severity}] {error.message}";
            
            switch (error.severity)
            {
                case ErrorSeverity.Critical:
                case ErrorSeverity.Error:
                    Debug.LogError(logMessage);
                    break;
                case ErrorSeverity.Warning:
                    Debug.LogWarning(logMessage);
                    break;
                case ErrorSeverity.Info:
                default:
                    Debug.Log(logMessage);
                    break;
            }
        }

        private void ShowErrorInUI(GameError error)
        {
            // This would integrate with UI system
            // For now, just log
            Debug.Log($"UI Error: {error.message}");
        }

        public void ClearErrors()
        {
            _recentErrors.Clear();
            _criticalErrors.Clear();
        }

        public void ClearErrorsOfSeverity(ErrorSeverity severity)
        {
            var recentErrorsArray = _recentErrors.ToArray();
            _recentErrors.Clear();
            
            foreach (var error in recentErrorsArray)
            {
                if (error.severity != severity)
                {
                    _recentErrors.Enqueue(error);
                }
            }
            
            if (severity == ErrorSeverity.Critical)
            {
                _criticalErrors.Clear();
            }
        }

        private static GameErrorHandler _instance;
        public static GameErrorHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameErrorHandler>();
                }
                return _instance;
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleUnityLogMessage;
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }

    [Serializable]
    public class GameError
    {
        public string message;
        public string stackTrace;
        public Exception exception;
        public DateTime timestamp;
        public ErrorSeverity severity;
        public string source;
        
        public override string ToString()
        {
            return $"[{timestamp:yyyy-MM-dd HH:mm:ss}] [{severity}] {message}";
        }
    }

    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
}