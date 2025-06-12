using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GrowAGarden.Core._01.Scripts.Core.Dependencies
{
    [Provide]
    public class DependencyInjector : MonoBehaviour, IDependencyProvider
    {
        [Header("Injection Settings")]
        [SerializeField] private bool injectOnAwake = true;
        [SerializeField] private bool injectOnSceneLoad = true;
        [SerializeField] private bool debugMode = false;
        
        // Service registry
        private static Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static Dictionary<Type, Func<object>> _serviceFactories = new Dictionary<Type, Func<object>>();
        private static List<WeakReference> _injectedObjects = new List<WeakReference>();
        
        // Singleton instance
        private static DependencyInjector _instance;
        public static DependencyInjector Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (injectOnAwake)
                {
                    RegisterProvidersInScene();
                    InjectAllInScene();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (injectOnSceneLoad)
            {
                RegisterProvidersInScene();
                InjectAllInScene();
            }
        }

        [Provide]
        public DependencyInjector ProvideDependencyInjector() => this;

        // Service registration
        public static void RegisterService<T>(T service) where T : class
        {
            RegisterService(typeof(T), service);
        }

        public static void RegisterService(Type serviceType, object service)
        {
            if (service == null)
            {
                Debug.LogWarning($"Attempted to register null service for type {serviceType.Name}");
                return;
            }
            
            _services[serviceType] = service;
            
            if (_instance != null && _instance.debugMode)
            {
                Debug.Log($"Registered service: {serviceType.Name}");
            }
        }

        public static void RegisterServiceFactory<T>(Func<T> factory) where T : class
        {
            _serviceFactories[typeof(T)] = () => factory();
        }

        // Service resolution
        public static T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }

        public static object GetService(Type serviceType)
        {
            // Check registered services first
            if (_services.TryGetValue(serviceType, out object service))
            {
                return service;
            }
            
            // Check service factories
            if (_serviceFactories.TryGetValue(serviceType, out Func<object> factory))
            {
                object factoryService = factory();
                _services[serviceType] = factoryService; // Cache it
                return factoryService;
            }
            
            // Try to find provider in scene
            var providers = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb is IDependencyProvider)
                .ToArray();
            
            foreach (var provider in providers)
            {
                var methods = provider.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<ProvideAttribute>() != null)
                    .ToArray();
                
                foreach (var method in methods)
                {
                    var provideAttr = method.GetCustomAttribute<ProvideAttribute>();
                    Type providedType = provideAttr.ServiceType ?? method.ReturnType;
                    
                    if (serviceType.IsAssignableFrom(providedType))
                    {
                        object providedService = method.Invoke(provider, null);
                        if (provideAttr.Singleton)
                        {
                            _services[serviceType] = providedService;
                        }
                        return providedService;
                    }
                }
            }
            
            return null;
        }

        // Injection methods
        public static void InjectInto(object target)
        {
            if (target == null) return;
            
            Type targetType = target.GetType();
            
            // Inject fields
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<InjectAttribute>() != null)
                .ToArray();
            
            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                object service = GetService(field.FieldType);
                
                if (service != null)
                {
                    field.SetValue(target, service);
                    
                    if (_instance != null && _instance.debugMode)
                    {
                        Debug.Log($"Injected {field.FieldType.Name} into {targetType.Name}.{field.Name}");
                    }
                }
                else if (!injectAttr.Optional)
                {
                    Debug.LogWarning($"Failed to inject required dependency {field.FieldType.Name} into {targetType.Name}.{field.Name}");
                }
            }
            
            // Inject properties
            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<InjectAttribute>() != null && p.CanWrite)
                .ToArray();
            
            foreach (var property in properties)
            {
                var injectAttr = property.GetCustomAttribute<InjectAttribute>();
                object service = GetService(property.PropertyType);
                
                if (service != null)
                {
                    property.SetValue(target, service);
                    
                    if (_instance != null && _instance.debugMode)
                    {
                        Debug.Log($"Injected {property.PropertyType.Name} into {targetType.Name}.{property.Name}");
                    }
                }
                else if (!injectAttr.Optional)
                {
                    Debug.LogWarning($"Failed to inject required dependency {property.PropertyType.Name} into {targetType.Name}.{property.Name}");
                }
            }
            
            // Track injected object
            _injectedObjects.Add(new WeakReference(target));
        }

        public static void InjectAllInScene()
        {
            // Find all MonoBehaviours that need injection
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var mb in allMonoBehaviours)
            {
                if (HasInjectableMembers(mb.GetType()))
                {
                    InjectInto(mb);
                }
            }
        }

        private static bool HasInjectableMembers(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(f => f.GetCustomAttribute<InjectAttribute>() != null);
            
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(p => p.GetCustomAttribute<InjectAttribute>() != null);
            
            return fields || properties;
        }

        public void RegisterProvidersInScene()
        {
            var providers = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb is IDependencyProvider)
                .ToArray();
            
            foreach (var provider in providers)
            {
                RegisterProvider(provider);
            }
        }

        private void RegisterProvider(MonoBehaviour provider)
        {
            var methods = provider.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<ProvideAttribute>() != null)
                .ToArray();
            
            foreach (var method in methods)
            {
                var provideAttr = method.GetCustomAttribute<ProvideAttribute>();
                Type serviceType = provideAttr.ServiceType ?? method.ReturnType;
                
                if (provideAttr.Singleton)
                {
                    // Register as singleton
                    object service = method.Invoke(provider, null);
                    RegisterService(serviceType, service);
                }
                else
                {
                    // Register as factory
                    _serviceFactories[serviceType] = () => method.Invoke(provider, null);
                }
            }
        }

        // Cleanup
        public static void ClearServices()
        {
            _services.Clear();
            _serviceFactories.Clear();
            
            // Clean up weak references
            _injectedObjects.RemoveAll(wr => !wr.IsAlive);
        }

        // Re-inject all tracked objects (useful after service changes)
        public static void ReInjectAll()
        {
            var aliveObjects = _injectedObjects.Where(wr => wr.IsAlive).Select(wr => wr.Target).ToArray();
            _injectedObjects.Clear();
            
            foreach (var obj in aliveObjects)
            {
                InjectInto(obj);
            }
        }

        // Debug methods
        public void LogRegisteredServices()
        {
            Debug.Log("=== Registered Services ===");
            foreach (var kvp in _services)
            {
                Debug.Log($"{kvp.Key.Name} -> {kvp.Value.GetType().Name}");
            }
            
            Debug.Log("=== Service Factories ===");
            foreach (var kvp in _serviceFactories)
            {
                Debug.Log($"{kvp.Key.Name} -> Factory");
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ClearServices();
                _instance = null;
            }
        }
    }
}