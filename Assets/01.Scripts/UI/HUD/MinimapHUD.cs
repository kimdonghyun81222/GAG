using System.Collections.Generic;
using GrowAGarden.UI._01.Scripts.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.HUD
{
    public class MinimapHUD : UIPanel
    {
        [Header("Minimap Settings")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private Transform playerIndicator;
        [SerializeField] private RectTransform minimapContainer;
        
        [Header("Size & Scale")]
        [SerializeField] private float mapSize = 200f;
        [SerializeField] private float zoomLevel = 1f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 3f;
        
        [Header("Player Tracking")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private bool followPlayer = true;
        [SerializeField] private bool rotateWithPlayer = false;
        [SerializeField] private float followSpeed = 5f;
        
        [Header("Map Icons")]
        [SerializeField] private GameObject iconPrefab;
        [SerializeField] private Transform iconContainer;
        [SerializeField] private List<MinimapIcon> mapIcons = new List<MinimapIcon>();
        
        [Header("UI Controls")]
        [SerializeField] private Button zoomInButton;
        [SerializeField] private Button zoomOutButton;
        [SerializeField] private Button centerButton;
        [SerializeField] private Toggle followToggle;
        
        // Minimap state
        private RenderTexture _minimapTexture;
        private Vector3 _lastPlayerPosition;
        private float _lastPlayerRotation;
        private Dictionary<GameObject, MinimapIconUI> _iconInstances = new Dictionary<GameObject, MinimapIconUI>();

        protected override void Awake()
        {
            base.Awake();
            InitializeMinimap();
        }

        protected override void Start()
        {
            base.Start();
            SetupMinimapCamera();
            CreateMinimapTexture();
            SetupUIControls();
        }

        private void Update()
        {
            if (followPlayer && playerTransform != null)
            {
                UpdatePlayerTracking();
            }
            
            UpdateMapIcons();
            HandleMinimapInput();
        }

        private void InitializeMinimap()
        {
            // Auto-find components if not assigned
            if (minimapImage == null)
                minimapImage = GetComponentInChildren<RawImage>();
            
            if (minimapCamera == null)
            {
                // Create minimap camera if none exists
                var cameraObj = new GameObject("MinimapCamera");
                cameraObj.transform.SetParent(transform);
                minimapCamera = cameraObj.AddComponent<Camera>();
            }
            
            if (playerIndicator == null)
            {
                // Create player indicator
                CreatePlayerIndicator();
            }
            
            if (iconContainer == null)
            {
                var containerObj = new GameObject("IconContainer");
                containerObj.transform.SetParent(minimapContainer ? minimapContainer : transform, false);
                iconContainer = containerObj.transform;
            }
        }

        private void CreatePlayerIndicator()
        {
            var indicatorObj = new GameObject("PlayerIndicator");
            indicatorObj.transform.SetParent(minimapContainer ? minimapContainer : transform, false);
            
            var image = indicatorObj.AddComponent<Image>();
            image.color = Color.red;
            
            var rectTransform = indicatorObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(10f, 10f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            
            playerIndicator = indicatorObj.transform;
        }

        private void SetupMinimapCamera()
        {
            if (minimapCamera == null) return;
            
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = mapSize * zoomLevel;
            minimapCamera.cullingMask = LayerMask.GetMask("Minimap"); // Minimap layer
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = Color.black;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            // Position camera above the world
            if (playerTransform != null)
            {
                var pos = playerTransform.position;
                minimapCamera.transform.position = new Vector3(pos.x, pos.y + 100f, pos.z);
            }
        }

        private void CreateMinimapTexture()
        {
            if (minimapCamera == null || minimapImage == null) return;
            
            int textureSize = Mathf.RoundToInt(mapSize);
            _minimapTexture = new RenderTexture(textureSize, textureSize, 16);
            _minimapTexture.Create();
            
            minimapCamera.targetTexture = _minimapTexture;
            minimapImage.texture = _minimapTexture;
        }

        private void SetupUIControls()
        {
            if (zoomInButton != null)
            {
                zoomInButton.onClick.RemoveAllListeners();
                zoomInButton.onClick.AddListener(() => ZoomIn());
            }
            
            if (zoomOutButton != null)
            {
                zoomOutButton.onClick.RemoveAllListeners();
                zoomOutButton.onClick.AddListener(() => ZoomOut());
            }
            
            if (centerButton != null)
            {
                centerButton.onClick.RemoveAllListeners();
                centerButton.onClick.AddListener(() => CenterOnPlayer());
            }
            
            if (followToggle != null)
            {
                followToggle.onValueChanged.RemoveAllListeners();
                followToggle.onValueChanged.AddListener((value) => SetFollowPlayer(value));
                followToggle.isOn = followPlayer;
            }
        }

        private void UpdatePlayerTracking()
        {
            if (playerTransform == null || minimapCamera == null) return;
            
            Vector3 currentPos = playerTransform.position;
            
            // Update camera position
            if (Vector3.Distance(currentPos, _lastPlayerPosition) > 0.1f)
            {
                Vector3 targetPos = new Vector3(currentPos.x, minimapCamera.transform.position.y, currentPos.z);
                minimapCamera.transform.position = Vector3.Lerp(minimapCamera.transform.position, targetPos, Time.deltaTime * followSpeed);
                _lastPlayerPosition = currentPos;
            }
            
            // Update player indicator position (always centered when following)
            if (playerIndicator != null && followPlayer)
            {
                playerIndicator.localPosition = Vector3.zero;
                
                if (rotateWithPlayer)
                {
                    float rotation = playerTransform.eulerAngles.y;
                    playerIndicator.rotation = Quaternion.Euler(0f, 0f, -rotation);
                }
            }
        }

        private void UpdateMapIcons()
        {
            foreach (var icon in mapIcons)
            {
                if (icon.worldObject == null) continue;
                
                // Create icon UI if it doesn't exist
                if (!_iconInstances.ContainsKey(icon.worldObject))
                {
                    CreateIconUI(icon);
                }
                
                // Update icon position
                var iconUI = _iconInstances[icon.worldObject];
                if (iconUI != null)
                {
                    UpdateIconPosition(icon, iconUI);
                }
            }
            
            // Remove icons for destroyed objects
            var keysToRemove = new List<GameObject>();
            foreach (var kvp in _iconInstances)
            {
                if (kvp.Key == null)
                {
                    keysToRemove.Add(kvp.Key);
                    if (kvp.Value != null && kvp.Value.iconObject != null)
                    {
                        DestroyImmediate(kvp.Value.iconObject);
                    }
                }
            }
            
            foreach (var key in keysToRemove)
            {
                _iconInstances.Remove(key);
            }
        }

        private void CreateIconUI(MinimapIcon icon)
        {
            GameObject iconObj;
            
            if (iconPrefab != null)
            {
                iconObj = Instantiate(iconPrefab, iconContainer);
            }
            else
            {
                iconObj = new GameObject($"Icon_{icon.worldObject.name}");
                iconObj.transform.SetParent(iconContainer, false);
                
                var image = iconObj.AddComponent<Image>();
                image.sprite = icon.iconSprite;
                image.color = icon.iconColor;
                
                var rectTransform = iconObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = Vector2.one * icon.iconSize;
            }
            
            var iconUI = new MinimapIconUI
            {
                iconObject = iconObj,
                rectTransform = iconObj.GetComponent<RectTransform>(),
                image = iconObj.GetComponent<Image>()
            };
            
            _iconInstances[icon.worldObject] = iconUI;
        }

        private void UpdateIconPosition(MinimapIcon icon, MinimapIconUI iconUI)
        {
            if (minimapCamera == null || iconUI.rectTransform == null) return;
            
            // Convert world position to minimap position
            Vector3 worldPos = icon.worldObject.transform.position;
            Vector3 cameraPos = minimapCamera.transform.position;
            
            // Calculate relative position
            Vector2 relativePos = new Vector2(
                worldPos.x - cameraPos.x,
                worldPos.z - cameraPos.z
            );
            
            // Scale to minimap size
            float mapScale = mapSize / minimapCamera.orthographicSize;
            Vector2 minimapPos = relativePos * mapScale;
            
            // Clamp to minimap bounds
            float halfSize = mapSize * 0.5f;
            minimapPos.x = Mathf.Clamp(minimapPos.x, -halfSize, halfSize);
            minimapPos.y = Mathf.Clamp(minimapPos.y, -halfSize, halfSize);
            
            iconUI.rectTransform.anchoredPosition = minimapPos;
            
            // Update visibility based on distance
            bool isVisible = Vector2.Distance(Vector2.zero, minimapPos) <= halfSize;
            iconUI.iconObject.SetActive(isVisible && icon.isVisible);
        }

        private void HandleMinimapInput()
        {
            // Zoom with scroll wheel when mouse is over minimap
            if (RectTransformUtility.RectangleContainsScreenPoint(minimapContainer, Input.mousePosition))
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    ZoomMap(scroll > 0 ? 0.9f : 1.1f);
                }
            }
        }

        // Public methods
        public void ZoomIn()
        {
            ZoomMap(0.8f);
        }

        public void ZoomOut()
        {
            ZoomMap(1.2f);
        }

        public void ZoomMap(float zoomMultiplier)
        {
            zoomLevel = Mathf.Clamp(zoomLevel * zoomMultiplier, minZoom, maxZoom);
            
            if (minimapCamera != null)
            {
                minimapCamera.orthographicSize = mapSize * zoomLevel;
            }
        }

        public void SetZoomLevel(float zoom)
        {
            zoomLevel = Mathf.Clamp(zoom, minZoom, maxZoom);
            
            if (minimapCamera != null)
            {
                minimapCamera.orthographicSize = mapSize * zoomLevel;
            }
        }

        public void CenterOnPlayer()
        {
            if (playerTransform != null && minimapCamera != null)
            {
                var pos = playerTransform.position;
                minimapCamera.transform.position = new Vector3(pos.x, minimapCamera.transform.position.y, pos.z);
            }
        }

        public void SetFollowPlayer(bool follow)
        {
            followPlayer = follow;
            
            if (followToggle != null && followToggle.isOn != follow)
            {
                followToggle.isOn = follow;
            }
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        public void AddMapIcon(GameObject worldObject, Sprite icon, Color color, float size = 8f, bool visible = true)
        {
            var mapIcon = new MinimapIcon
            {
                worldObject = worldObject,
                iconSprite = icon,
                iconColor = color,
                iconSize = size,
                isVisible = visible
            };
            
            mapIcons.Add(mapIcon);
        }

        public void RemoveMapIcon(GameObject worldObject)
        {
            mapIcons.RemoveAll(icon => icon.worldObject == worldObject);
            
            if (_iconInstances.ContainsKey(worldObject))
            {
                var iconUI = _iconInstances[worldObject];
                if (iconUI.iconObject != null)
                {
                    DestroyImmediate(iconUI.iconObject);
                }
                _iconInstances.Remove(worldObject);
            }
        }

        public void SetMapIconVisibility(GameObject worldObject, bool visible)
        {
            var icon = mapIcons.Find(i => i.worldObject == worldObject);
            if (icon != null)
            {
                icon.isVisible = visible;
            }
        }

        // 🔧 수정: MonoBehaviour의 OnDestroy 사용 (override 제거)
        private void OnDestroy()
        {
            if (_minimapTexture != null)
            {
                _minimapTexture.Release();
                DestroyImmediate(_minimapTexture);
            }
        }
    }

    [System.Serializable]
    public class MinimapIcon
    {
        public GameObject worldObject;
        public Sprite iconSprite;
        public Color iconColor = Color.white;
        public float iconSize = 8f;
        public bool isVisible = true;
    }

    [System.Serializable]
    public class MinimapIconUI
    {
        public GameObject iconObject;
        public RectTransform rectTransform;
        public Image image;
    }
}