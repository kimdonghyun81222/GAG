using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.VFX
{
    [Provide]
    public class VFXManager : MonoBehaviour, IDependencyProvider
    {
        [Header("VFX Settings")]
        [SerializeField] private VFXDatabase vfxDatabase;
        [SerializeField] private bool enableVFX = true;
        [SerializeField] private int maxActiveVFX = 50;
        [SerializeField] private bool debugMode = false;
        
        // VFX tracking
        private List<VFXInstance> _activeVFX = new List<VFXInstance>();
        private Queue<VFXInstance> _vfxPool = new Queue<VFXInstance>();
        
        // Properties
        public bool VFXEnabled => enableVFX;
        public int ActiveVFXCount => _activeVFX.Count;
        public int MaxActiveVFX => maxActiveVFX;

        private void Awake()
        {
            if (vfxDatabase != null)
            {
                vfxDatabase.Initialize();
            }
        }

        private void Update()
        {
            UpdateActiveVFX();
        }
        
        [Provide]
        public VFXManager ProvideVFXManager() => this;

        private void UpdateActiveVFX()
        {
            for (int i = _activeVFX.Count - 1; i >= 0; i--)
            {
                var vfx = _activeVFX[i];
                
                if (vfx == null || vfx.IsCompleted)
                {
                    _activeVFX.RemoveAt(i);
                }
            }
        }

        // VFX playing methods
        public VFXInstance PlayVFX(string vfxName, Vector3 position, Quaternion rotation = default)
        {
            if (!enableVFX || vfxDatabase == null) return null;
            
            var entry = vfxDatabase.GetVFXEntry(vfxName);
            if (entry == null || entry.prefab == null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"VFX '{vfxName}' not found in database");
                }
                return null;
            }
            
            return PlayVFX(entry, position, rotation);
        }

        public VFXInstance PlayVFX(VFXEntry entry, Vector3 position, Quaternion rotation = default)
        {
            if (!enableVFX || entry?.prefab == null) return null;
            
            // Check max active VFX limit
            if (_activeVFX.Count >= maxActiveVFX)
            {
                // Remove oldest VFX
                var oldest = _activeVFX[0];
                if (oldest != null)
                {
                    oldest.Complete();
                }
            }
            
            // Create VFX instance
            if (rotation == default) rotation = Quaternion.identity;
            
            var vfxInstance = VFXInstance.Create(entry.prefab, position, rotation);
            if (vfxInstance == null) return null;
            
            // Configure VFX
            vfxInstance.SetDuration(entry.defaultDuration);
            vfxInstance.SetAutoDestroy(entry.autoDestroy);
            vfxInstance.transform.localScale = entry.defaultScale;
            
            // Setup audio
            if (entry.audioClip != null)
            {
                var audioSource = vfxInstance.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = vfxInstance.gameObject.AddComponent<AudioSource>();
                }
                
                audioSource.clip = entry.audioClip;
                audioSource.volume = entry.audioVolume;
                audioSource.playOnAwake = false;
            }
            
            // Setup completion callback
            vfxInstance.OnCompleted += OnVFXCompleted;
            
            // Add to active list and play
            _activeVFX.Add(vfxInstance);
            vfxInstance.Play();
            
            if (debugMode)
            {
                Debug.Log($"Playing VFX: {entry.name} at {position}");
            }
            
            return vfxInstance;
        }

        public VFXInstance PlayVFXAtTarget(string vfxName, Transform target, bool followTarget = false, Vector3 offset = default)
        {
            if (target == null) return null;
            
            var vfx = PlayVFX(vfxName, target.position + offset, target.rotation);
            if (vfx != null && followTarget)
            {
                vfx.SetTarget(target);
                vfx.SetFollowTarget(true);
            }
            
            return vfx;
        }

        private void OnVFXCompleted(VFXInstance vfx)
        {
            if (vfx != null)
            {
                vfx.OnCompleted -= OnVFXCompleted;
                _activeVFX.Remove(vfx);
            }
        }

        // VFX management
        public void StopAllVFX()
        {
            foreach (var vfx in _activeVFX)
            {
                if (vfx != null)
                {
                    vfx.Stop();
                }
            }
            
            _activeVFX.Clear();
            
            if (debugMode)
            {
                Debug.Log("All VFX stopped");
            }
        }

        public void StopVFXByName(string vfxName)
        {
            for (int i = _activeVFX.Count - 1; i >= 0; i--)
            {
                var vfx = _activeVFX[i];
                if (vfx != null && vfx.name.Contains(vfxName))
                {
                    vfx.Stop();
                    _activeVFX.RemoveAt(i);
                }
            }
        }

        public List<VFXInstance> GetActiveVFXByName(string vfxName)
        {
            var result = new List<VFXInstance>();
            
            foreach (var vfx in _activeVFX)
            {
                if (vfx != null && vfx.name.Contains(vfxName))
                {
                    result.Add(vfx);
                }
            }
            
            return result;
        }

        // Settings
        public void SetVFXEnabled(bool enabled)
        {
            enableVFX = enabled;
            
            if (!enabled)
            {
                StopAllVFX();
            }
        }

        public void SetMaxActiveVFX(int max)
        {
            maxActiveVFX = Mathf.Max(1, max);
        }

        public void SetVFXDatabase(VFXDatabase database)
        {
            vfxDatabase = database;
            if (database != null)
            {
                database.Initialize();
            }
        }

        // Utility methods
        public bool HasVFX(string vfxName)
        {
            return vfxDatabase != null && vfxDatabase.GetVFXEntry(vfxName) != null;
        }

        public List<string> GetAvailableVFXNames()
        {
            return vfxDatabase?.GetAllVFXNames() ?? new List<string>();
        }
    }
}