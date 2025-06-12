using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GrowAGarden.UI._01.Scripts.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame
{
    public class MapPanel : UIPanel, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
    {
        [Header("Map Display")]
        [SerializeField] private RectTransform mapContainer;
        [SerializeField] private Image mapImage;
        [SerializeField] private RectTransform mapContent;
        [SerializeField] private ScrollRect mapScrollRect;
        [SerializeField] private Button resetViewButton;
        
        [Header("Map Layers")]
        [SerializeField] private Toggle terrainLayerToggle;
        [SerializeField] private Toggle buildingsLayerToggle;
        [SerializeField] private Toggle npcsLayerToggle;
        [SerializeField] private Toggle questsLayerToggle;
        [SerializeField] private Toggle resourcesLayerToggle;
        [SerializeField] private Toggle fogOfWarToggle;
        
        [Header("Map Controls")]
        [SerializeField] private Slider zoomSlider;
        [SerializeField] private TextMeshProUGUI zoomText;
        [SerializeField] private Button zoomInButton;
        [SerializeField] private Button zoomOutButton;
        [SerializeField] private Button centerOnPlayerButton;
        
        [Header("Map Markers")]
        [SerializeField] private Transform markersContainer;
        [SerializeField] private GameObject playerMarkerPrefab;
        [SerializeField] private GameObject questMarkerPrefab;
        [SerializeField] private GameObject npcMarkerPrefab;
        [SerializeField] private GameObject resourceMarkerPrefab;
        [SerializeField] private GameObject waypointMarkerPrefab;
        [SerializeField] private GameObject customMarkerPrefab;
        
        [Header("Minimap")]
        [SerializeField] private GameObject minimapPanel;
        [SerializeField] private Image minimapImage;
        [SerializeField] private RectTransform minimapPlayerMarker;
        [SerializeField] private Button toggleMinimapButton;
        [SerializeField] private Slider minimapSizeSlider;
        
        [Header("Location Info")]
        [SerializeField] private GameObject locationInfoPanel;
        [SerializeField] private TextMeshProUGUI locationNameText;
        [SerializeField] private TextMeshProUGUI locationDescriptionText;
        [SerializeField] private TextMeshProUGUI coordinatesText;
        [SerializeField] private Image locationIcon;
        [SerializeField] private Button visitLocationButton;
        
        [Header("Map Navigation")]
        [SerializeField] private Transform quickLocationsContainer;
        [SerializeField] private GameObject quickLocationButtonPrefab;
        [SerializeField] private TMP_InputField searchLocationField;
        [SerializeField] private Button clearSearchButton;
        
        [Header("Custom Markers")]
        [SerializeField] private GameObject markerCreationPanel;
        [SerializeField] private TMP_InputField markerNameInput;
        [SerializeField] private TMP_InputField markerDescriptionInput;
        [SerializeField] private TMP_Dropdown markerTypeDropdown;
        [SerializeField] private Button createMarkerButton;
        [SerializeField] private Button cancelMarkerButton;
        
        [Header("Map Settings")]
        [SerializeField] private GameObject mapSettingsPanel;
        [SerializeField] private Toggle showGridToggle;
        [SerializeField] private Toggle showCoordinatesToggle;
        [SerializeField] private Toggle autoFollowPlayerToggle;
        [SerializeField] private Slider mapOpacitySlider;
        [SerializeField] private Button mapSettingsButton;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem mapTransitionEffect;
        [SerializeField] private GameObject locationPulseEffect;
        [SerializeField] private LineRenderer pathRenderer;
        [SerializeField] private Material pathMaterial;
        
        [Header("Audio")]
        [SerializeField] private AudioClip mapOpenSound;
        [SerializeField] private AudioClip mapCloseSound;
        [SerializeField] private AudioClip markerPlaceSound;
        [SerializeField] private AudioClip locationSelectSound;
        [SerializeField] private AudioClip buttonClickSound;
        
        [Header("Animation")]
        [SerializeField] private bool enableMapAnimations = true;
        [SerializeField] private float markerAnimationDelay = 0.02f;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Map management
        private MapData _currentMapData;
        private List<MapMarker> _mapMarkers = new List<MapMarker>();
        private List<MapLocation> _mapLocations = new List<MapLocation>();
        private List<CustomMarker> _customMarkers = new List<CustomMarker>();
        
        // Player and world state
        private Vector2 _playerWorldPosition;
        private Vector2 _selectedWorldPosition;
        private MapLocation _selectedLocation;
        private bool _isCreatingMarker = false;
        
        // Map interaction
        private float _currentZoom = 1f;
        private Vector2 _mapOffset = Vector2.zero;
        private bool _isDragging = false;
        private Vector2 _lastDragPosition;
        private bool _autoFollowPlayer = true;
        
        // Map layers
        private Dictionary<MapLayer, bool> _layerVisibility = new Dictionary<MapLayer, bool>();
        
        // Quick locations
        private readonly MapLocation[] _quickLocations = new MapLocation[]
        {
            new MapLocation { name = "Farm", position = new Vector2(0, 0), type = LocationType.Farm },
            new MapLocation { name = "Town", position = new Vector2(100, 50), type = LocationType.Town },
            new MapLocation { name = "Forest", position = new Vector2(-50, 100), type = LocationType.Forest },
            new MapLocation { name = "Mountain", position = new Vector2(150, 150), type = LocationType.Mountain },
            new MapLocation { name = "Beach", position = new Vector2(200, 0), type = LocationType.Beach },
            new MapLocation { name = "Desert", position = new Vector2(300, 100), type = LocationType.Desert }
        };
        
        // Search and filter
        private string _searchQuery = "";
        
        // Map bounds
        private readonly Vector2 _mapSize = new Vector2(1000f, 1000f);
        private readonly float _minZoom = 0.5f;
        private readonly float _maxZoom = 5f;

        protected override void Awake()
        {
            base.Awake();
            InitializeMap();
        }

        protected override void Start()
        {
            base.Start();
            SetupMapPanel();
            LoadMapData();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Initially hide this panel
            gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleMapInput();
            UpdatePlayerPosition();
            UpdateMinimap();
            
            if (_autoFollowPlayer)
            {
                CenterMapOnPlayer();
            }
        }

        private void InitializeMap()
        {
            // Initialize layer visibility
            _layerVisibility[MapLayer.Terrain] = true;
            _layerVisibility[MapLayer.Buildings] = true;
            _layerVisibility[MapLayer.NPCs] = true;
            _layerVisibility[MapLayer.Quests] = true;
            _layerVisibility[MapLayer.Resources] = false;
            _layerVisibility[MapLayer.FogOfWar] = true;
            
            // Create default prefabs if none exist
            CreateDefaultMarkerPrefabs();
            
            // Setup path renderer
            if (pathRenderer != null)
            {
                pathRenderer.material = pathMaterial;
                pathRenderer.startWidth = 2f;
                pathRenderer.endWidth = 2f;
                pathRenderer.positionCount = 0;
            }
        }

        private void SetupMapPanel()
        {
            // Setup map controls
            if (zoomSlider != null)
            {
                zoomSlider.minValue = _minZoom;
                zoomSlider.maxValue = _maxZoom;
                zoomSlider.value = _currentZoom;
                zoomSlider.onValueChanged.AddListener(OnZoomChanged);
            }
            
            if (zoomInButton != null)
                zoomInButton.onClick.AddListener(ZoomIn);
            
            if (zoomOutButton != null)
                zoomOutButton.onClick.AddListener(ZoomOut);
            
            if (centerOnPlayerButton != null)
                centerOnPlayerButton.onClick.AddListener(CenterOnPlayer);
            
            if (resetViewButton != null)
                resetViewButton.onClick.AddListener(ResetMapView);
            
            // Setup layer toggles
            if (terrainLayerToggle != null)
            {
                terrainLayerToggle.isOn = _layerVisibility[MapLayer.Terrain];
                terrainLayerToggle.onValueChanged.AddListener(value => SetLayerVisibility(MapLayer.Terrain, value));
            }
            
            if (buildingsLayerToggle != null)
            {
                buildingsLayerToggle.isOn = _layerVisibility[MapLayer.Buildings];
                buildingsLayerToggle.onValueChanged.AddListener(value => SetLayerVisibility(MapLayer.Buildings, value));
            }
            
            if (npcsLayerToggle != null)
            {
                npcsLayerToggle.isOn = _layerVisibility[MapLayer.NPCs];
                npcsLayerToggle.onValueChanged.AddListener(value => SetLayerVisibility(MapLayer.NPCs, value));
            }
            
            if (questsLayerToggle != null)
            {
                questsLayerToggle.isOn = _layerVisibility[MapLayer.Quests];
                questsLayerToggle.onValueChanged.AddListener(value => SetLayerVisibility(MapLayer.Quests, value));
            }
            
            if (resourcesLayerToggle != null)
            {
                resourcesLayerToggle.isOn = _layerVisibility[MapLayer.Resources];
                resourcesLayerToggle.onValueChanged.AddListener(value => SetLayerVisibility(MapLayer.Resources, value));
            }
            
            if (fogOfWarToggle != null)
            {
                fogOfWarToggle.isOn = _layerVisibility[MapLayer.FogOfWar];
                fogOfWarToggle.onValueChanged.AddListener(value => SetLayerVisibility(MapLayer.FogOfWar, value));
            }
            
            // Setup minimap
            if (toggleMinimapButton != null)
                toggleMinimapButton.onClick.AddListener(ToggleMinimap);
            
            if (minimapSizeSlider != null)
                minimapSizeSlider.onValueChanged.AddListener(OnMinimapSizeChanged);
            
            // Setup location search
            if (searchLocationField != null)
                searchLocationField.onValueChanged.AddListener(OnLocationSearchChanged);
            
            if (clearSearchButton != null)
                clearSearchButton.onClick.AddListener(ClearLocationSearch);
            
            // Setup marker creation
            if (createMarkerButton != null)
                createMarkerButton.onClick.AddListener(CreateCustomMarker);
            
            if (cancelMarkerButton != null)
                cancelMarkerButton.onClick.AddListener(CancelMarkerCreation);
            
            // Setup map settings
            if (mapSettingsButton != null)
                mapSettingsButton.onClick.AddListener(ToggleMapSettings);
            
            if (showGridToggle != null)
                showGridToggle.onValueChanged.AddListener(OnShowGridChanged);
            
            if (showCoordinatesToggle != null)
                showCoordinatesToggle.onValueChanged.AddListener(OnShowCoordinatesChanged);
            
            if (autoFollowPlayerToggle != null)
            {
                autoFollowPlayerToggle.isOn = _autoFollowPlayer;
                autoFollowPlayerToggle.onValueChanged.AddListener(OnAutoFollowPlayerChanged);
            }
            
            if (mapOpacitySlider != null)
                mapOpacitySlider.onValueChanged.AddListener(OnMapOpacityChanged);
            
            if (visitLocationButton != null)
                visitLocationButton.onClick.AddListener(VisitSelectedLocation);
            
            // Initialize displays
            HideLocationInfo();
            HideMarkerCreation();
            HideMapSettings();
            UpdateZoomDisplay();
            CreateQuickLocationButtons();
        }

        private void CreateDefaultMarkerPrefabs()
        {
            if (playerMarkerPrefab == null)
            {
                playerMarkerPrefab = CreateMarkerPrefab("PlayerMarker", Color.blue, 20f);
            }
            
            if (questMarkerPrefab == null)
            {
                questMarkerPrefab = CreateMarkerPrefab("QuestMarker", Color.yellow, 15f);
            }
            
            if (npcMarkerPrefab == null)
            {
                npcMarkerPrefab = CreateMarkerPrefab("NPCMarker", Color.green, 12f);
            }
            
            if (resourceMarkerPrefab == null)
            {
                resourceMarkerPrefab = CreateMarkerPrefab("ResourceMarker", Color.cyan, 10f);
            }
            
            if (waypointMarkerPrefab == null)
            {
                waypointMarkerPrefab = CreateMarkerPrefab("WaypointMarker", Color.red, 15f);
            }
            
            if (customMarkerPrefab == null)
            {
                customMarkerPrefab = CreateMarkerPrefab("CustomMarker", Color.white, 12f);
            }
        }

        private GameObject CreateMarkerPrefab(string name, Color color, float size)
        {
            var markerObj = new GameObject(name);
            markerObj.AddComponent<RectTransform>();
            
            // Marker background
            var background = markerObj.AddComponent<Image>();
            background.color = color;
            background.sprite = CreateCircleSprite();
            
            var rect = markerObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);
            
            // Add button for interaction
            var button = markerObj.AddComponent<Button>();
            button.targetGraphic = background;
            
            // Add marker component
            var marker = markerObj.AddComponent<MapMarkerUI>();
            
            markerObj.SetActive(false);
            return markerObj;
        }

        private Sprite CreateCircleSprite()
        {
            // Create a simple circle texture
            int size = 32;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * size + x] = distance <= radius ? Color.white : Color.clear;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        // Input handling
        private void HandleMapInput()
        {
            // ESC to close map
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMap();
            }
            
            // M key to toggle map
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (gameObject.activeInHierarchy)
                    CloseMap();
                else
                    OpenMap();
            }
            
            // Mouse wheel for zooming
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f && IsMouseOverMap())
            {
                float newZoom = _currentZoom + scroll * 0.5f;
                SetZoom(newZoom);
            }
            
            // Right click to create marker
            if (Input.GetMouseButtonDown(1) && IsMouseOverMap())
            {
                Vector2 worldPos = ScreenToWorldPosition(Input.mousePosition);
                StartMarkerCreation(worldPos);
            }
            
            // Keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.C))
                CenterOnPlayer();
            
            if (Input.GetKeyDown(KeyCode.R))
                ResetMapView();
            
            if (Input.GetKeyDown(KeyCode.Tab))
                ToggleMinimap();
        }

        private bool IsMouseOverMap()
        {
            return mapContainer != null && 
                   RectTransformUtility.RectangleContainsScreenPoint(mapContainer, Input.mousePosition);
        }

        // Map data management
        private void LoadMapData()
        {
            // This would normally load from MapManager
            CreateMockMapData();
            RefreshMapDisplay();
        }

        private void CreateMockMapData()
        {
            _currentMapData = new MapData
            {
                mapName = "Stardew Valley",
                mapSize = _mapSize,
                playerSpawnPosition = Vector2.zero
            };
            
            // Load map locations
            _mapLocations.Clear();
            _mapLocations.AddRange(_quickLocations);
            
            // Add additional locations
            var additionalLocations = new MapLocation[]
            {
                new MapLocation { name = "Community Center", position = new Vector2(80, 60), type = LocationType.Building, description = "A place where the community gathers for events." },
                new MapLocation { name = "Blacksmith", position = new Vector2(110, 45), type = LocationType.Shop, description = "Clint's blacksmith shop for tool upgrades." },
                new MapLocation { name = "General Store", position = new Vector2(90, 40), type = LocationType.Shop, description = "Pierre's shop for seeds and supplies." },
                new MapLocation { name = "Saloon", position = new Vector2(85, 35), type = LocationType.Building, description = "The Stardrop Saloon where townspeople gather." },
                new MapLocation { name = "Library", position = new Vector2(95, 55), type = LocationType.Building, description = "Museum and library with ancient artifacts." },
                new MapLocation { name = "Secret Woods", position = new Vector2(-80, 120), type = LocationType.Secret, description = "A hidden area in the forest with rare resources." },
                new MapLocation { name = "Quarry", position = new Vector2(180, 180), type = LocationType.Mining, description = "A large quarry with stone and mineral deposits." }
            };
            
            _mapLocations.AddRange(additionalLocations);
            
            // Create map markers
            CreateMapMarkers();
            
            // Set initial player position
            _playerWorldPosition = _currentMapData.playerSpawnPosition;
        }

        private void CreateMapMarkers()
        {
            _mapMarkers.Clear();
            
            // Create player marker
            var playerMarker = new MapMarker
            {
                id = "player",
                name = "Player",
                worldPosition = _playerWorldPosition,
                type = MarkerType.Player,
                layer = MapLayer.Player,
                isVisible = true,
                prefab = playerMarkerPrefab
            };
            _mapMarkers.Add(playerMarker);
            
            // Create location markers
            foreach (var location in _mapLocations)
            {
                var marker = new MapMarker
                {
                    id = $"location_{location.name}",
                    name = location.name,
                    worldPosition = location.position,
                    type = GetMarkerTypeFromLocation(location.type),
                    layer = MapLayer.Buildings,
                    isVisible = true,
                    prefab = GetMarkerPrefabFromType(GetMarkerTypeFromLocation(location.type)),
                    data = location
                };
                _mapMarkers.Add(marker);
            }
            
            // Create quest markers (mock data)
            var questMarkers = new MapMarker[]
            {
                new MapMarker { id = "quest_1", name = "Deliver to Lewis", worldPosition = new Vector2(120, 70), type = MarkerType.Quest, layer = MapLayer.Quests, isVisible = true, prefab = questMarkerPrefab },
                new MapMarker { id = "quest_2", name = "Find Lost Axe", worldPosition = new Vector2(-30, 80), type = MarkerType.Quest, layer = MapLayer.Quests, isVisible = true, prefab = questMarkerPrefab }
            };
            _mapMarkers.AddRange(questMarkers);
            
            // Create NPC markers (mock data)
            var npcMarkers = new MapMarker[]
            {
                new MapMarker { id = "npc_lewis", name = "Mayor Lewis", worldPosition = new Vector2(120, 70), type = MarkerType.NPC, layer = MapLayer.NPCs, isVisible = true, prefab = npcMarkerPrefab },
                new MapMarker { id = "npc_pierre", name = "Pierre", worldPosition = new Vector2(90, 40), type = MarkerType.NPC, layer = MapLayer.NPCs, isVisible = true, prefab = npcMarkerPrefab },
                new MapMarker { id = "npc_clint", name = "Clint", worldPosition = new Vector2(110, 45), type = MarkerType.NPC, layer = MapLayer.NPCs, isVisible = true, prefab = npcMarkerPrefab }
            };
            _mapMarkers.AddRange(npcMarkers);
        }

        private MarkerType GetMarkerTypeFromLocation(LocationType locationType)
        {
            return locationType switch
            {
                LocationType.Shop => MarkerType.Shop,
                LocationType.Building => MarkerType.Building,
                LocationType.Mining => MarkerType.Resource,
                LocationType.Secret => MarkerType.Custom,
                _ => MarkerType.Waypoint
            };
        }

        private GameObject GetMarkerPrefabFromType(MarkerType markerType)
        {
            return markerType switch
            {
                MarkerType.Player => playerMarkerPrefab,
                MarkerType.Quest => questMarkerPrefab,
                MarkerType.NPC => npcMarkerPrefab,
                MarkerType.Resource => resourceMarkerPrefab,
                MarkerType.Waypoint => waypointMarkerPrefab,
                MarkerType.Custom => customMarkerPrefab,
                _ => customMarkerPrefab
            };
        }

        private void RefreshMapDisplay()
        {
            // Clear existing markers
            ClearMapMarkers();
            
            // Create marker displays
            StartCoroutine(CreateMarkerDisplays());
            
            // Update map image
            UpdateMapImage();
            
            // Center on player initially
            if (_autoFollowPlayer)
            {
                CenterMapOnPlayer();
            }
        }

        private void ClearMapMarkers()
        {
            if (markersContainer == null) return;
            
            foreach (Transform child in markersContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private IEnumerator CreateMarkerDisplays()
        {
            if (markersContainer == null) yield break;
            
            for (int i = 0; i < _mapMarkers.Count; i++)
            {
                var marker = _mapMarkers[i];
                
                if (marker.prefab != null && ShouldShowMarker(marker))
                {
                    var markerObj = Instantiate(marker.prefab, markersContainer);
                    markerObj.SetActive(true);
                    
                    // Position marker
                    var rectTransform = markerObj.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = WorldToMapPosition(marker.worldPosition);
                    
                    // Setup marker UI component
                    var markerUI = markerObj.GetComponent<MapMarkerUI>();
                    if (markerUI != null)
                    {
                        markerUI.Initialize(marker, this);
                    }
                    
                    // Setup button interaction
                    var button = markerObj.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => SelectMarker(marker));
                    }
                    
                    // Animate marker in
                    if (enableMapAnimations)
                    {
                        StartCoroutine(AnimateMarkerIn(markerObj.transform, i * markerAnimationDelay));
                    }
                }
                
                yield return null;
            }
        }

        private bool ShouldShowMarker(MapMarker marker)
        {
            // Check layer visibility
            if (!_layerVisibility.ContainsKey(marker.layer) || !_layerVisibility[marker.layer])
            {
                return false;
            }
            
            // Check marker visibility
            if (!marker.isVisible)
            {
                return false;
            }
            
            // Check search filter
            if (!string.IsNullOrEmpty(_searchQuery))
            {
                return marker.name.ToLower().Contains(_searchQuery.ToLower());
            }
            
            return true;
        }

        private IEnumerator AnimateMarkerIn(Transform markerTransform, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            var startScale = markerTransform.localScale;
            markerTransform.localScale = Vector3.zero;
            
            float elapsedTime = 0f;
            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / slideInDuration;
                
                markerTransform.localScale = Vector3.Lerp(Vector3.zero, startScale, slideInCurve.Evaluate(progress));
                
                yield return null;
            }
            
            markerTransform.localScale = startScale;
        }

        // Map positioning and coordinate conversion
        private Vector2 WorldToMapPosition(Vector2 worldPosition)
        {
            if (mapContent == null) return Vector2.zero;
            
            // Convert world position to map position
            Vector2 normalizedPos = new Vector2(
                (worldPosition.x + _mapSize.x / 2f) / _mapSize.x,
                (worldPosition.y + _mapSize.y / 2f) / _mapSize.y
            );
            
            Vector2 mapSize = mapContent.rect.size;
            return new Vector2(
                normalizedPos.x * mapSize.x - mapSize.x / 2f,
                normalizedPos.y * mapSize.y - mapSize.y / 2f
            );
        }

        private Vector2 MapToWorldPosition(Vector2 mapPosition)
        {
            if (mapContent == null) return Vector2.zero;
            
            Vector2 mapSize = mapContent.rect.size;
            Vector2 normalizedPos = new Vector2(
                (mapPosition.x + mapSize.x / 2f) / mapSize.x,
                (mapPosition.y + mapSize.y / 2f) / mapSize.y
            );
            
            return new Vector2(
                normalizedPos.x * _mapSize.x - _mapSize.x / 2f,
                normalizedPos.y * _mapSize.y - _mapSize.y / 2f
            );
        }

        private Vector2 ScreenToWorldPosition(Vector2 screenPosition)
        {
            if (mapContainer == null) return Vector2.zero;
            
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapContainer, screenPosition, null, out localPoint);
            
            return MapToWorldPosition(localPoint);
        }

        // Map controls and interaction
        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            _lastDragPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            Vector2 dragDelta = eventData.position - _lastDragPosition;
            _lastDragPosition = eventData.position;
            
            // Apply drag to map offset
            _mapOffset += dragDelta / _currentZoom;
            ApplyMapTransform();
            
            // Disable auto-follow when manually dragging
            if (_autoFollowPlayer)
            {
                _autoFollowPlayer = false;
                if (autoFollowPlayerToggle != null)
                    autoFollowPlayerToggle.isOn = false;
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            float zoomDelta = eventData.scrollDelta.y * 0.1f;
            float newZoom = _currentZoom + zoomDelta;
            SetZoom(newZoom);
        }

        private void OnZoomChanged(float zoom)
        {
            SetZoom(zoom);
        }

        private void SetZoom(float zoom)
        {
            _currentZoom = Mathf.Clamp(zoom, _minZoom, _maxZoom);
            
            if (zoomSlider != null)
                zoomSlider.value = _currentZoom;
            
            ApplyMapTransform();
            UpdateZoomDisplay();
        }

        private void ZoomIn()
        {
            PlayButtonClickSound();
            SetZoom(_currentZoom + 0.25f);
        }

        private void ZoomOut()
        {
            PlayButtonClickSound();
            SetZoom(_currentZoom - 0.25f);
        }

        private void ApplyMapTransform()
        {
            if (mapContent == null) return;
            
            mapContent.localScale = Vector3.one * _currentZoom;
            mapContent.anchoredPosition = _mapOffset;
        }

        private void UpdateZoomDisplay()
        {
            if (zoomText != null)
            {
                zoomText.text = $"Zoom: {_currentZoom:F1}x";
            }
        }

        private void CenterOnPlayer()
        {
            PlayButtonClickSound();
            CenterMapOnPlayer();
        }

        private void CenterMapOnPlayer()
        {
            Vector2 playerMapPos = WorldToMapPosition(_playerWorldPosition);
            _mapOffset = -playerMapPos * _currentZoom;
            ApplyMapTransform();
        }

        private void ResetMapView()
        {
            PlayButtonClickSound();
            
            _currentZoom = 1f;
            _mapOffset = Vector2.zero;
            
            if (zoomSlider != null)
                zoomSlider.value = _currentZoom;
            
            ApplyMapTransform();
            UpdateZoomDisplay();
        }

        // Layer management
        private void SetLayerVisibility(MapLayer layer, bool visible)
        {
            _layerVisibility[layer] = visible;
            RefreshMarkerVisibility();
        }

        private void RefreshMarkerVisibility()
        {
            if (markersContainer == null) return;
            
            for (int i = 0; i < markersContainer.childCount; i++)
            {
                var markerObj = markersContainer.GetChild(i);
                var markerUI = markerObj.GetComponent<MapMarkerUI>();
                
                if (markerUI != null)
                {
                    bool shouldShow = ShouldShowMarker(markerUI.MarkerData);
                    markerObj.gameObject.SetActive(shouldShow);
                }
            }
        }

        // Marker selection and interaction
        public void SelectMarker(MapMarker marker)
        {
            PlayLocationSelectSound();
            
            if (marker.data is MapLocation location)
            {
                _selectedLocation = location;
                ShowLocationInfo(location);
            }
            
            // Center map on selected marker
            Vector2 markerMapPos = WorldToMapPosition(marker.worldPosition);
            _mapOffset = -markerMapPos * _currentZoom;
            ApplyMapTransform();
            
            // Show pulse effect
            if (locationPulseEffect != null)
            {
                var pulse = Instantiate(locationPulseEffect, markersContainer);
                var pulseRect = pulse.GetComponent<RectTransform>();
                pulseRect.anchoredPosition = WorldToMapPosition(marker.worldPosition);
                
                StartCoroutine(DestroyEffectAfterDelay(pulse, 2f));
            }
        }

        private void ShowLocationInfo(MapLocation location)
        {
            if (locationInfoPanel == null) return;
            
            locationInfoPanel.SetActive(true);
            
            if (locationNameText != null)
                locationNameText.text = location.name;
            
            if (locationDescriptionText != null)
                locationDescriptionText.text = location.description ?? "No description available.";
            
            if (coordinatesText != null)
                coordinatesText.text = $"Coordinates: ({location.position.x:F0}, {location.position.y:F0})";
            
            if (locationIcon != null)
                locationIcon.sprite = GetLocationIcon(location.type);
            
            if (visitLocationButton != null)
                visitLocationButton.gameObject.SetActive(location.type != LocationType.Secret);
        }

        private void HideLocationInfo()
        {
            if (locationInfoPanel != null)
            {
                locationInfoPanel.SetActive(false);
            }
            
            _selectedLocation = null;
        }

        private Sprite GetLocationIcon(LocationType type)
        {
            // This would return appropriate icons for different location types
            return null; // Placeholder
        }

        private void VisitSelectedLocation()
        {
            if (_selectedLocation == null) return;
            
            PlayButtonClickSound();
            
            // This would trigger travel to the selected location
            Debug.Log($"Traveling to {_selectedLocation.name}...");
            
            // Show travel animation
            if (mapTransitionEffect != null)
            {
                mapTransitionEffect.Play();
            }
            
            // In a real game, this would load the new scene or area
        }

        // Quick locations
        private void CreateQuickLocationButtons()
        {
            if (quickLocationsContainer == null || quickLocationButtonPrefab == null) return;
            
            foreach (var location in _quickLocations)
            {
                var buttonObj = CreateQuickLocationButton(location);
                if (buttonObj != null)
                {
                    buttonObj.transform.SetParent(quickLocationsContainer, false);
                }
            }
        }

        private GameObject CreateQuickLocationButton(MapLocation location)
        {
            var buttonObj = new GameObject($"QuickLocation_{location.name}");
            buttonObj.AddComponent<RectTransform>();
            
            // Layout element
            var layoutElement = buttonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 30f;
            layoutElement.flexibleWidth = 1f;
            
            // Button background
            var background = buttonObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Button component
            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(() => TeleportToLocation(location));
            
            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = location.name;
            text.fontSize = 12f;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            return buttonObj;
        }

        private void TeleportToLocation(MapLocation location)
        {
            PlayLocationSelectSound();
            
            // Center map on location
            Vector2 locationMapPos = WorldToMapPosition(location.position);
            _mapOffset = -locationMapPos * _currentZoom;
            ApplyMapTransform();
            
            // Show location info
            _selectedLocation = location;
            ShowLocationInfo(location);
        }

        // Location search
        private void OnLocationSearchChanged(string searchQuery)
        {
            _searchQuery = searchQuery;
            RefreshMarkerVisibility();
        }

        private void ClearLocationSearch()
        {
            PlayButtonClickSound();
            
            if (searchLocationField != null)
                searchLocationField.text = "";
            
            _searchQuery = "";
            RefreshMarkerVisibility();
        }

        // Custom marker creation
        private void StartMarkerCreation(Vector2 worldPosition)
        {
            _selectedWorldPosition = worldPosition;
            _isCreatingMarker = true;
            ShowMarkerCreation();
        }

        private void ShowMarkerCreation()
        {
            if (markerCreationPanel == null) return;
            
            markerCreationPanel.SetActive(true);
            
            // Clear input fields
            if (markerNameInput != null)
                markerNameInput.text = "";
            
            if (markerDescriptionInput != null)
                markerDescriptionInput.text = "";
            
            if (markerTypeDropdown != null)
                markerTypeDropdown.value = 0;
        }

        private void HideMarkerCreation()
        {
            if (markerCreationPanel != null)
            {
                markerCreationPanel.SetActive(false);
            }
            
            _isCreatingMarker = false;
        }

        private void CreateCustomMarker()
        {
            if (!_isCreatingMarker) return;
            
            PlayMarkerPlaceSound();
            
            string markerName = markerNameInput?.text ?? "Custom Marker";
            string markerDescription = markerDescriptionInput?.text ?? "";
            
            var customMarker = new CustomMarker
            {
                id = System.Guid.NewGuid().ToString(),
                name = markerName,
                description = markerDescription,
                worldPosition = _selectedWorldPosition,
                dateCreated = System.DateTime.Now
            };
            
            _customMarkers.Add(customMarker);
            
            // Create map marker
            var mapMarker = new MapMarker
            {
                id = customMarker.id,
                name = customMarker.name,
                worldPosition = customMarker.worldPosition,
                type = MarkerType.Custom,
                layer = MapLayer.Custom,
                isVisible = true,
                prefab = customMarkerPrefab,
                data = customMarker
            };
            
            _mapMarkers.Add(mapMarker);
            
            // Refresh display
            RefreshMapDisplay();
            
            HideMarkerCreation();
        }

        private void CancelMarkerCreation()
        {
            PlayButtonClickSound();
            HideMarkerCreation();
        }

        // Minimap management
        private void UpdateMinimap()
        {
            if (minimapImage == null || minimapPlayerMarker == null) return;
            
            // Update minimap based on current map view
            // This would typically show a zoomed-out version of the current area
            
            // Update player marker position on minimap
            Vector2 playerNormalizedPos = new Vector2(
                (_playerWorldPosition.x + _mapSize.x / 2f) / _mapSize.x,
                (_playerWorldPosition.y + _mapSize.y / 2f) / _mapSize.y
            );
            
            if (minimapImage.rectTransform != null)
            {
                Vector2 minimapSize = minimapImage.rectTransform.rect.size;
                Vector2 playerMinimapPos = new Vector2(
                    playerNormalizedPos.x * minimapSize.x - minimapSize.x / 2f,
                    playerNormalizedPos.y * minimapSize.y - minimapSize.y / 2f
                );
                
                minimapPlayerMarker.anchoredPosition = playerMinimapPos;
            }
        }

        private void ToggleMinimap()
        {
            PlayButtonClickSound();
            
            if (minimapPanel != null)
            {
                minimapPanel.SetActive(!minimapPanel.activeInHierarchy);
            }
        }

        private void OnMinimapSizeChanged(float size)
        {
            if (minimapPanel != null)
            {
                var rectTransform = minimapPanel.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = Vector2.one * (100f + size * 100f);
                }
            }
        }

        // Map settings
        private void ToggleMapSettings()
        {
            PlayButtonClickSound();
            
            if (mapSettingsPanel != null)
            {
                mapSettingsPanel.SetActive(!mapSettingsPanel.activeInHierarchy);
            }
        }

        private void HideMapSettings()
        {
            if (mapSettingsPanel != null)
            {
                mapSettingsPanel.SetActive(false);
            }
        }

        private void OnShowGridChanged(bool showGrid)
        {
            // Implementation for grid overlay
            Debug.Log($"Grid visibility: {showGrid}");
        }

        private void OnShowCoordinatesChanged(bool showCoordinates)
        {
            // Implementation for coordinate display
            Debug.Log($"Coordinates visibility: {showCoordinates}");
        }

        private void OnAutoFollowPlayerChanged(bool autoFollow)
        {
            _autoFollowPlayer = autoFollow;
        }

        private void OnMapOpacityChanged(float opacity)
        {
            if (mapImage != null)
            {
                var color = mapImage.color;
                color.a = opacity;
                mapImage.color = color;
            }
        }

        // Player position management
        private void UpdatePlayerPosition()
        {
            // This would normally get the player's current world position from PlayerManager
            // For now, we'll simulate player movement
            
            // Update player marker position
            var playerMarker = _mapMarkers.FirstOrDefault(m => m.type == MarkerType.Player);
            if (playerMarker != null)
            {
                playerMarker.worldPosition = _playerWorldPosition;
                
                // Update marker display position
                if (markersContainer != null)
                {
                    for (int i = 0; i < markersContainer.childCount; i++)
                    {
                        var markerObj = markersContainer.GetChild(i);
                        var markerUI = markerObj.GetComponent<MapMarkerUI>();
                        
                        if (markerUI != null && markerUI.MarkerData.id == playerMarker.id)
                        {
                            var rectTransform = markerObj.GetComponent<RectTransform>();
                            rectTransform.anchoredPosition = WorldToMapPosition(_playerWorldPosition);
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateMapImage()
        {
            // This would load the appropriate map texture based on current area
            // For now, we'll use a placeholder
        }

        // Utility methods
        private IEnumerator DestroyEffectAfterDelay(GameObject effect, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            
            if (effect != null)
            {
                Destroy(effect);
            }
        }

        // Navigation
        private void OpenMap()
        {
            gameObject.SetActive(true);
            PlayMapOpenSound();
            
            if (mapTransitionEffect != null)
            {
                mapTransitionEffect.Play();
            }
        }

        private void CloseMap()
        {
            PlayMapCloseSound();
            gameObject.SetActive(false);
            
            // Return to pause menu if it was open
            var pauseMenu = FindObjectOfType<PauseMenuPanel>();
            if (pauseMenu != null && pauseMenu.IsPaused)
            {
                // pauseMenu.ReturnToMainPauseMenu(); // Would implement this
            }
        }

        // Audio methods
        private void PlayMapOpenSound()
        {
            Debug.Log("Map open sound would play here");
        }

        private void PlayMapCloseSound()
        {
            Debug.Log("Map close sound would play here");
        }

        private void PlayMarkerPlaceSound()
        {
            Debug.Log("Marker place sound would play here");
        }

        private void PlayLocationSelectSound()
        {
            Debug.Log("Location select sound would play here");
        }

        private void PlayButtonClickSound()
        {
            Debug.Log("Button click sound would play here");
        }

        // Public interface
        public Vector2 PlayerWorldPosition 
        { 
            get => _playerWorldPosition; 
            set => _playerWorldPosition = value; 
        }
        
        public float CurrentZoom => _currentZoom;
        public bool AutoFollowPlayer => _autoFollowPlayer;
        public List<CustomMarker> CustomMarkers => _customMarkers;
        
        public void AddCustomMarker(CustomMarker marker)
        {
            _customMarkers.Add(marker);
            
            var mapMarker = new MapMarker
            {
                id = marker.id,
                name = marker.name,
                worldPosition = marker.worldPosition,
                type = MarkerType.Custom,
                layer = MapLayer.Custom,
                isVisible = true,
                prefab = customMarkerPrefab,
                data = marker
            };
            
            _mapMarkers.Add(mapMarker);
            RefreshMapDisplay();
        }
        
        public void RemoveCustomMarker(string markerId)
        {
            _customMarkers.RemoveAll(m => m.id == markerId);
            _mapMarkers.RemoveAll(m => m.id == markerId);
            RefreshMapDisplay();
        }
        
        public void SetPlayerPosition(Vector2 worldPosition)
        {
            _playerWorldPosition = worldPosition;
        }
        
        public void CenterOnLocation(string locationName)
        {
            var location = _mapLocations.FirstOrDefault(l => l.name == locationName);
            if (location != null)
            {
                TeleportToLocation(location);
            }
        }
    }

    // Helper component for map markers
    public class MapMarkerUI : MonoBehaviour
    {
        private MapMarker _markerData;
        private MapPanel _mapPanel;
        
        public void Initialize(MapMarker markerData, MapPanel mapPanel)
        {
            _markerData = markerData;
            _mapPanel = mapPanel;
        }
        
        public MapMarker MarkerData => _markerData;
    }

    // Data structures and enums
    [System.Serializable]
    public class MapData
    {
        public string mapName;
        public Vector2 mapSize;
        public Vector2 playerSpawnPosition;
        public Sprite mapTexture;
    }

    [System.Serializable]
    public class MapMarker
    {
        public string id;
        public string name;
        public Vector2 worldPosition;
        public MarkerType type;
        public MapLayer layer;
        public bool isVisible;
        public GameObject prefab;
        public object data;
    }

    [System.Serializable]
    public class MapLocation
    {
        public string name;
        public string description;
        public Vector2 position;
        public LocationType type;
        public bool isUnlocked = true;
        public Sprite icon;
    }

    [System.Serializable]
    public class CustomMarker
    {
        public string id;
        public string name;
        public string description;
        public Vector2 worldPosition;
        public System.DateTime dateCreated;
        public Color color = Color.white;
    }

    public enum MapLayer
    {
        Terrain,
        Buildings,
        NPCs,
        Quests,
        Resources,
        FogOfWar,
        Player,
        Custom
    }

    public enum MarkerType
    {
        Player,
        Quest,
        NPC,
        Resource,
        Waypoint,
        Custom,
        Shop,
        Building
    }

    public enum LocationType
    {
        Farm,
        Town,
        Forest,
        Mountain,
        Beach,
        Desert,
        Shop,
        Building,
        Mining,
        Secret
    }
}