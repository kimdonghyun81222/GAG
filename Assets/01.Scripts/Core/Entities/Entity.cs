using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.Core._01.Scripts.Core.Entities
{
    public class Entity : MonoBehaviour
    {
        [Header("Entity Settings")]
        [SerializeField] private string entityId;
        [SerializeField] private string entityName;
        [SerializeField] private bool initializeOnAwake = true;
        
        // Components
        private List<IEntityComponent> _components = new List<IEntityComponent>();
        private Dictionary<Type, IEntityComponent> _componentCache = new Dictionary<Type, IEntityComponent>();
        private bool _isInitialized = false;
        
        // Properties
        public string EntityId => entityId;
        public string EntityName => entityName;
        public bool IsInitialized => _isInitialized;
        
        // Events
        public event Action<Entity> OnEntityInitialized;
        public event Action<Entity> OnEntityDestroyed;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(entityId))
            {
                entityId = Guid.NewGuid().ToString();
            }
            
            if (string.IsNullOrEmpty(entityName))
            {
                entityName = gameObject.name;
            }
            
            // Find and cache components
            CacheEntityComponents();
            
            if (initializeOnAwake)
            {
                Initialize();
            }
        }

        protected virtual void Start()
        {
            // Initialize components that need to run after Start
            InitializeAfterComponents();
        }

        public virtual void Initialize()
        {
            if (_isInitialized) return;
            
            // Initialize all components
            foreach (var component in _components)
            {
                try
                {
                    component.Initialize(this);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize component {component.GetType().Name} on entity {entityName}: {ex.Message}");
                }
            }
            
            _isInitialized = true;
            OnEntityInitialized?.Invoke(this);
            
            Debug.Log($"Entity initialized: {entityName} ({entityId})");
        }

        private void CacheEntityComponents()
        {
            _components.Clear();
            _componentCache.Clear();
            
            // Find all IEntityComponent implementations
            var allComponents = GetComponents<MonoBehaviour>();
            
            foreach (var component in allComponents)
            {
                if (component is IEntityComponent entityComponent)
                {
                    _components.Add(entityComponent);
                    _componentCache[component.GetType()] = entityComponent;
                }
            }
        }

        private void InitializeAfterComponents()
        {
            foreach (var component in _components)
            {
                if (component is IAfterInitialize afterInit)
                {
                    try
                    {
                        afterInit.AfterInitialize();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to run AfterInitialize on component {component.GetType().Name}: {ex.Message}");
                    }
                }
            }
        }

        // Component access methods
        public T GetEntityComponent<T>() where T : class, IEntityComponent
        {
            if (_componentCache.TryGetValue(typeof(T), out IEntityComponent component))
            {
                return component as T;
            }
            
            // Try to find by interface
            foreach (var comp in _components)
            {
                if (comp is T result)
                {
                    _componentCache[typeof(T)] = comp;
                    return result;
                }
            }
            
            return null;
        }

        public bool HasEntityComponent<T>() where T : class, IEntityComponent
        {
            return GetEntityComponent<T>() != null;
        }

        public IEntityComponent[] GetAllEntityComponents()
        {
            return _components.ToArray();
        }

        // Entity lifecycle
        public virtual void DestroyEntity()
        {
            OnEntityDestroyed?.Invoke(this);
            
            // Cleanup components
            foreach (var component in _components)
            {
                if (component is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            Destroy(gameObject);
        }

        // Utility methods
        public void SetEntityId(string newId)
        {
            if (!_isInitialized)
            {
                entityId = newId;
            }
            else
            {
                Debug.LogWarning("Cannot change entity ID after initialization");
            }
        }

        public void SetEntityName(string newName)
        {
            entityName = newName;
            gameObject.name = newName;
        }

        protected virtual void OnDestroy()
        {
            OnEntityDestroyed?.Invoke(this);
        }
    }
}