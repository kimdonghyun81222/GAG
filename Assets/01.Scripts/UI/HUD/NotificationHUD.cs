using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.HUD
{
    public class NotificationHUD : MonoBehaviour
    {
        [Header("Notification Container")]
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private int maxNotifications = 5;
        
        [Header("Default Settings")]
        [SerializeField] private float defaultDuration = 3f;
        [SerializeField] private Vector2 notificationSize = new Vector2(300f, 60f);
        [SerializeField] private float notificationSpacing = 10f;
        
        [Header("Animation")]
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private float slideOutDuration = 0.3f;
        [SerializeField] private Vector2 slideDirection = Vector2.right;
        
        [Header("Notification Styles")]
        [SerializeField] private NotificationStyle infoStyle = new NotificationStyle 
        { 
            backgroundColor = new Color(0.2f, 0.6f, 1f, 0.9f), 
            textColor = Color.white 
        };
        [SerializeField] private NotificationStyle successStyle = new NotificationStyle 
        { 
            backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.9f), 
            textColor = Color.white 
        };
        [SerializeField] private NotificationStyle warningStyle = new NotificationStyle 
        { 
            backgroundColor = new Color(1f, 0.8f, 0.2f, 0.9f), 
            textColor = Color.black 
        };
        [SerializeField] private NotificationStyle errorStyle = new NotificationStyle 
        { 
            backgroundColor = new Color(0.9f, 0.2f, 0.2f, 0.9f), 
            textColor = Color.white 
        };
        
        // Notification queue and management
        private Queue<NotificationData> _notificationQueue = new Queue<NotificationData>();
        private List<NotificationInstance> _activeNotifications = new List<NotificationInstance>();
        private bool _isProcessingQueue = false;

        private void Awake()
        {
            InitializeContainer();
            CreateDefaultPrefab();
        }

        private void InitializeContainer()
        {
            if (notificationContainer == null)
            {
                var containerObj = new GameObject("NotificationContainer");
                containerObj.transform.SetParent(transform, false);
                
                var rectTransform = containerObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(1f, 1f);
                rectTransform.anchorMax = new Vector2(1f, 1f);
                rectTransform.anchoredPosition = new Vector2(-20f, -20f);
                rectTransform.sizeDelta = new Vector2(320f, 400f);
                
                notificationContainer = containerObj.transform;
            }
        }

        private void CreateDefaultPrefab()
        {
            if (notificationPrefab != null) return;
            
            var prefabObj = new GameObject("NotificationPrefab");
            
            // RectTransform
            var rectTransform = prefabObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = notificationSize;
            
            // Background Image
            var background = prefabObj.AddComponent<Image>();
            background.color = infoStyle.backgroundColor;
            
            // CanvasGroup for fading
            prefabObj.AddComponent<CanvasGroup>();
            
            // Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(prefabObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 5f);
            textRect.offsetMax = new Vector2(-10f, -5f);
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Notification";
            text.color = infoStyle.textColor;
            text.fontSize = 14f;
            text.alignment = TextAlignmentOptions.Left;   
            text.fontStyle = FontStyles.Normal;
            
            notificationPrefab = prefabObj;
            notificationPrefab.SetActive(false);
        }

        // Public notification methods
        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 0f)
        {
            var notification = new NotificationData
            {
                message = message,
                type = type,
                duration = duration > 0 ? duration : defaultDuration,
                timestamp = Time.time
            };
            
            _notificationQueue.Enqueue(notification);
            
            if (!_isProcessingQueue)
            {
                StartCoroutine(ProcessNotificationQueue());
            }
        }

        public void ShowInfo(string message, float duration = 0f)
        {
            ShowNotification(message, NotificationType.Info, duration);
        }

        public void ShowSuccess(string message, float duration = 0f)
        {
            ShowNotification(message, NotificationType.Success, duration);
        }

        public void ShowWarning(string message, float duration = 0f)
        {
            ShowNotification(message, NotificationType.Warning, duration);
        }

        public void ShowError(string message, float duration = 0f)
        {
            ShowNotification(message, NotificationType.Error, duration);
        }

        // Queue processing
        private IEnumerator ProcessNotificationQueue()
        {
            _isProcessingQueue = true;
            
            while (_notificationQueue.Count > 0)
            {
                var notification = _notificationQueue.Dequeue();
                yield return StartCoroutine(DisplayNotification(notification));
                
                // Small delay between notifications
                yield return new WaitForSeconds(0.1f);
            }
            
            _isProcessingQueue = false;
        }

        private IEnumerator DisplayNotification(NotificationData notification)
        {
            // Remove oldest notification if at max capacity
            if (_activeNotifications.Count >= maxNotifications)
            {
                var oldestNotification = _activeNotifications[0];
                _activeNotifications.RemoveAt(0);
                StartCoroutine(HideNotification(oldestNotification));
            }
            
            // Create notification instance
            var instance = CreateNotificationInstance(notification);
            _activeNotifications.Add(instance);
            
            // Update positions of all notifications
            UpdateNotificationPositions();
            
            // Show animation
            yield return StartCoroutine(ShowNotificationAnimation(instance));
            
            // Wait for duration
            yield return new WaitForSeconds(notification.duration);
            
            // Hide animation
            yield return StartCoroutine(HideNotification(instance));
            
            // Remove from active list
            _activeNotifications.Remove(instance);
            
            // Clean up
            if (instance.gameObject != null)
            {
                Destroy(instance.gameObject);
            }
            
            // Update positions after removal
            UpdateNotificationPositions();
        }

        private NotificationInstance CreateNotificationInstance(NotificationData notification)
        {
            var notificationObj = Instantiate(notificationPrefab, notificationContainer);
            notificationObj.SetActive(true);
            
            // Get components
            var rectTransform = notificationObj.GetComponent<RectTransform>();
            var background = notificationObj.GetComponent<Image>();
            var canvasGroup = notificationObj.GetComponent<CanvasGroup>();
            var text = notificationObj.GetComponentInChildren<TextMeshProUGUI>();
            
            // Apply style
            var style = GetNotificationStyle(notification.type);
            if (background != null) background.color = style.backgroundColor;
            if (text != null)
            {
                text.text = notification.message;
                text.color = style.textColor;
            }
            
            // Set initial state (invisible and off-screen)
            canvasGroup.alpha = 0f;
            Vector2 offScreenPos = new Vector2(notificationSize.x + 50f, 0f);
            rectTransform.anchoredPosition = offScreenPos;
            
            return new NotificationInstance
            {
                gameObject = notificationObj,
                rectTransform = rectTransform,
                canvasGroup = canvasGroup,
                data = notification
            };
        }

        private NotificationStyle GetNotificationStyle(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => infoStyle,
                NotificationType.Success => successStyle,
                NotificationType.Warning => warningStyle,
                NotificationType.Error => errorStyle,
                _ => infoStyle
            };
        }

        private void UpdateNotificationPositions()
        {
            for (int i = 0; i < _activeNotifications.Count; i++)
            {
                var notification = _activeNotifications[i];
                if (notification.rectTransform != null)
                {
                    float yPosition = -(i * (notificationSize.y + notificationSpacing));
                    Vector2 targetPos = new Vector2(0f, yPosition);
                    
                    StartCoroutine(MoveNotificationToPosition(notification, targetPos));
                }
            }
        }

        private IEnumerator MoveNotificationToPosition(NotificationInstance notification, Vector2 targetPosition)
        {
            if (notification.rectTransform == null) yield break;
            
            Vector2 startPos = notification.rectTransform.anchoredPosition;
            float elapsedTime = 0f;
            float moveDuration = 0.3f;
            
            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / moveDuration;
                
                if (notification.rectTransform != null)
                {
                    notification.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPosition, progress);
                }
                
                yield return null;
            }
            
            if (notification.rectTransform != null)
            {
                notification.rectTransform.anchoredPosition = targetPosition;
            }
        }

        private IEnumerator ShowNotificationAnimation(NotificationInstance notification)
        {
            if (notification.canvasGroup == null || notification.rectTransform == null) yield break;
            
            Vector2 startPos = notification.rectTransform.anchoredPosition;
            Vector2 endPos = new Vector2(0f, startPos.y);
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
                
                notification.canvasGroup.alpha = easeProgress;
                notification.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, easeProgress);
                
                yield return null;
            }
            
            notification.canvasGroup.alpha = 1f;
            notification.rectTransform.anchoredPosition = endPos;
        }

        private IEnumerator HideNotification(NotificationInstance notification)
        {
            if (notification.canvasGroup == null || notification.rectTransform == null) yield break;
            
            Vector2 startPos = notification.rectTransform.anchoredPosition;
            Vector2 endPos = startPos + slideDirection * (notificationSize.x + 50f);
            
            float elapsedTime = 0f;
            while (elapsedTime < slideOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideOutDuration;
                float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
                
                notification.canvasGroup.alpha = 1f - easeProgress;
                notification.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, easeProgress);
                
                yield return null;
            }
            
            notification.canvasGroup.alpha = 0f;
        }

        // Utility methods
        public void ClearAllNotifications()
        {
            StopAllCoroutines();
            _notificationQueue.Clear();
            
            foreach (var notification in _activeNotifications)
            {
                if (notification.gameObject != null)
                {
                    Destroy(notification.gameObject);
                }
            }
            
            _activeNotifications.Clear();
            _isProcessingQueue = false;
        }

        public int GetActiveNotificationCount()
        {
            return _activeNotifications.Count;
        }

        public int GetQueuedNotificationCount()
        {
            return _notificationQueue.Count;
        }

        // Static convenience methods
        public static void Show(string message, NotificationType type = NotificationType.Info)
        {
            var notificationHUD = FindObjectOfType<NotificationHUD>();
            notificationHUD?.ShowNotification(message, type);
        }

        public static void Info(string message)
        {
            Show(message, NotificationType.Info);
        }

        public static void Success(string message)
        {
            Show(message, NotificationType.Success);
        }

        public static void Warning(string message)
        {
            Show(message, NotificationType.Warning);
        }

        public static void Error(string message)
        {
            Show(message, NotificationType.Error);
        }
    }

    [System.Serializable]
    public class NotificationInstance
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public CanvasGroup canvasGroup;
        public NotificationData data;
    }

    [System.Serializable]
    public class NotificationData
    {
        public string message;
        public NotificationType type;
        public float duration;
        public float timestamp;
    }

    [System.Serializable]
    public class NotificationStyle
    {
        public Color backgroundColor = Color.blue;
        public Color textColor = Color.white;
        public Font font;
        public int fontSize = 14;
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}