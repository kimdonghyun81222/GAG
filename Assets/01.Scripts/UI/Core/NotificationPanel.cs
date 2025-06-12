using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Core
{
    public class NotificationPanel : UIPanel
    {
        [Header("Notification Settings")]
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private Image notificationBackground;
        [SerializeField] private float defaultDuration = 3f;
        [SerializeField] private int maxNotifications = 5;
        
        [Header("Notification Types")]
        [SerializeField] private NotificationStyle infoStyle = new NotificationStyle { backgroundColor = Color.blue, textColor = Color.white };
        [SerializeField] private NotificationStyle successStyle = new NotificationStyle { backgroundColor = Color.green, textColor = Color.white };
        [SerializeField] private NotificationStyle warningStyle = new NotificationStyle { backgroundColor = Color.yellow, textColor = Color.black };
        [SerializeField] private NotificationStyle errorStyle = new NotificationStyle { backgroundColor = Color.red, textColor = Color.white };
        
        [Header("Animation")]
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private float slideOutDuration = 0.3f;
        // 🔧 26-27번째 줄 수정: EaseOut/EaseIn → 직접 생성
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve slideOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        
        // Notification queue
        private Queue<NotificationData> _notificationQueue = new Queue<NotificationData>();
        private List<GameObject> _activeNotifications = new List<GameObject>();
        private bool _isShowingNotification = false;
        
        // Prefab for multiple notifications
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationContainer;

        protected override void Awake()
        {
            base.Awake();
            
            // 🔧 AnimationCurve 초기화 (Awake에서 설정)
            InitializeAnimationCurves();
            
            // Auto-find components
            if (notificationText == null)
                notificationText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (notificationBackground == null)
                notificationBackground = GetComponent<Image>();
            
            if (notificationContainer == null)
                notificationContainer = transform;
            
            // Create notification prefab if none exists
            if (notificationPrefab == null)
            {
                CreateDefaultNotificationPrefab();
            }
        }

        // 🔧 새로운 메서드: AnimationCurve 초기화
        private void InitializeAnimationCurves()
        {
            // EaseOut 곡선 생성 (빠르게 시작, 천천히 끝남)
            slideInCurve = new AnimationCurve();
            slideInCurve.AddKey(0f, 0f);
            slideInCurve.AddKey(1f, 1f);
            
            // 키프레임에 EaseOut 스타일 적용
            for (int i = 0; i < slideInCurve.keys.Length; i++)
            {
                Keyframe key = slideInCurve.keys[i];
                key.inTangent = 0f;
                key.outTangent = 0f;
                slideInCurve.MoveKey(i, key);
            }
            
            // EaseIn 곡선 생성 (천천히 시작, 빠르게 끝남)
            slideOutCurve = new AnimationCurve();
            slideOutCurve.AddKey(0f, 0f);
            slideOutCurve.AddKey(1f, 1f);
            
            // 키프레임에 EaseIn 스타일 적용
            for (int i = 0; i < slideOutCurve.keys.Length; i++)
            {
                Keyframe key = slideOutCurve.keys[i];
                key.inTangent = 0f;
                key.outTangent = 0f;
                slideOutCurve.MoveKey(i, key);
            }
        }

        private void CreateDefaultNotificationPrefab()
        {
            var prefabObject = new GameObject("NotificationItem");
            var rectTransform = prefabObject.AddComponent<RectTransform>();
            var canvasGroup = prefabObject.AddComponent<CanvasGroup>();
            
            // Background
            var bg = prefabObject.AddComponent<Image>();
            bg.color = infoStyle.backgroundColor;
            
            // Text
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(prefabObject.transform, false);
            var textRect = textObject.AddComponent<RectTransform>();
            var text = textObject.AddComponent<TextMeshProUGUI>();
            
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
            
            text.text = "Notification";
            text.color = infoStyle.textColor;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            
            notificationPrefab = prefabObject;
            notificationPrefab.SetActive(false);
        }

        // 나머지 메서드들은 동일...
        // Public notification methods
        public void ShowNotification(string message, float duration = 0f)
        {
            ShowNotification(message, NotificationType.Info, duration > 0 ? duration : defaultDuration);
        }

        public void ShowNotification(string message, NotificationType type, float duration = 0f)
        {
            var notification = new NotificationData
            {
                message = message,
                type = type,
                duration = duration > 0 ? duration : defaultDuration,
                timestamp = Time.time
            };
            
            _notificationQueue.Enqueue(notification);
            ProcessNotificationQueue();
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
        private void ProcessNotificationQueue()
        {
            if (_notificationQueue.Count == 0 || _isShowingNotification) return;
            
            var notification = _notificationQueue.Dequeue();
            StartCoroutine(DisplayNotification(notification));
        }

        private IEnumerator DisplayNotification(NotificationData notification)
        {
            _isShowingNotification = true;
            
            // Create notification UI
            var notificationObj = CreateNotificationUI(notification);
            _activeNotifications.Add(notificationObj);
            
            // Remove old notifications if too many
            while (_activeNotifications.Count > maxNotifications)
            {
                var oldNotification = _activeNotifications[0];
                _activeNotifications.RemoveAt(0);
                StartCoroutine(HideNotification(oldNotification));
            }
            
            // Show notification
            yield return StartCoroutine(ShowNotificationAnimation(notificationObj));
            
            // Wait for duration
            yield return new WaitForSeconds(notification.duration);
            
            // Hide notification
            yield return StartCoroutine(HideNotification(notificationObj));
            
            // Remove from active list
            _activeNotifications.Remove(notificationObj);
            Destroy(notificationObj);
            
            _isShowingNotification = false;
            
            // Process next notification
            ProcessNotificationQueue();
        }

        private GameObject CreateNotificationUI(NotificationData notification)
        {
            var notificationObj = Instantiate(notificationPrefab, notificationContainer);
            notificationObj.SetActive(true);
            
            // Get components
            var image = notificationObj.GetComponent<Image>();
            var text = notificationObj.GetComponentInChildren<TextMeshProUGUI>();
            var canvasGroup = notificationObj.GetComponent<CanvasGroup>();
            
            // Apply style
            var style = GetNotificationStyle(notification.type);
            if (image != null) image.color = style.backgroundColor;
            if (text != null)
            {
                text.text = notification.message;
                text.color = style.textColor;
            }
            
            // Position notification
            var rect = notificationObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0, -50 - (_activeNotifications.Count * 60));
                rect.sizeDelta = new Vector2(300, 50);
            }
            
            // Start invisible
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            
            return notificationObj;
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

        private IEnumerator ShowNotificationAnimation(GameObject notificationObj)
        {
            var canvasGroup = notificationObj.GetComponent<CanvasGroup>();
            var rectTransform = notificationObj.GetComponent<RectTransform>();
            
            if (canvasGroup == null) yield break;
            
            float elapsedTime = 0f;
            Vector2 startPos = rectTransform.anchoredPosition + Vector2.right * 350f;
            Vector2 endPos = rectTransform.anchoredPosition;
            
            rectTransform.anchoredPosition = startPos;
            
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                float curveValue = slideInCurve.Evaluate(progress);
                
                canvasGroup.alpha = curveValue;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = endPos;
        }

        private IEnumerator HideNotification(GameObject notificationObj)
        {
            var canvasGroup = notificationObj.GetComponent<CanvasGroup>();
            var rectTransform = notificationObj.GetComponent<RectTransform>();
            
            if (canvasGroup == null) yield break;
            
            float elapsedTime = 0f;
            Vector2 startPos = rectTransform.anchoredPosition;
            Vector2 endPos = startPos + Vector2.right * 350f;
            
            while (elapsedTime < slideOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideOutDuration;
                float curveValue = slideOutCurve.Evaluate(progress);
                
                canvasGroup.alpha = 1f - curveValue;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, curveValue);
                
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }

        // Clear all notifications
        public void ClearAllNotifications()
        {
            StopAllCoroutines();
            _notificationQueue.Clear();
            
            foreach (var notification in _activeNotifications)
            {
                if (notification != null)
                {
                    Destroy(notification);
                }
            }
            
            _activeNotifications.Clear();
            _isShowingNotification = false;
        }

        // Static convenience methods
        public static void Show(string message, float duration = 3f)
        {
            var panel = FindPanel<NotificationPanel>("NotificationPanel");
            panel?.ShowNotification(message, duration);
        }

        public static void Info(string message, float duration = 3f)
        {
            var panel = FindPanel<NotificationPanel>("NotificationPanel");
            panel?.ShowInfo(message, duration);
        }

        public static void Success(string message, float duration = 3f)
        {
            var panel = FindPanel<NotificationPanel>("NotificationPanel");
            panel?.ShowSuccess(message, duration);
        }

        public static void Warning(string message, float duration = 3f)
        {
            var panel = FindPanel<NotificationPanel>("NotificationPanel");
            panel?.ShowWarning(message, duration);
        }

        public static void Error(string message, float duration = 3f)
        {
            var panel = FindPanel<NotificationPanel>("NotificationPanel");
            panel?.ShowError(message, duration);
        }
    }

    [System.Serializable]
    public class NotificationStyle
    {
        public Color backgroundColor = Color.blue;
        public Color textColor = Color.white;
        public Font font;
        public int fontSize = 14;
    }

    [System.Serializable]
    public class NotificationData
    {
        public string message;
        public NotificationType type;
        public float duration;
        public float timestamp;
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}