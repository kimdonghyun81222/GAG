using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.VFX
{
    [CreateAssetMenu(fileName = "VFXDatabase", menuName = "GrowAGarden/Effects/VFX Database")]
    public class VFXDatabase : ScriptableObject
    {
        [Header("VFX Effects")]
        [SerializeField] private List<VFXEntry> vfxEntries = new List<VFXEntry>();
        
        // Cache for quick lookup
        private Dictionary<string, VFXEntry> _vfxLookup;
        
        public void Initialize()
        {
            _vfxLookup = new Dictionary<string, VFXEntry>();
            
            foreach (var entry in vfxEntries)
            {
                if (!string.IsNullOrEmpty(entry.name) && entry.prefab != null)
                {
                    _vfxLookup[entry.name] = entry;
                }
            }
        }
        
        public VFXEntry GetVFXEntry(string name)
        {
            if (_vfxLookup == null) Initialize();
            
            _vfxLookup.TryGetValue(name, out VFXEntry entry);
            return entry;
        }
        
        public GameObject GetVFXPrefab(string name)
        {
            var entry = GetVFXEntry(name);
            return entry?.prefab;
        }
        
        public List<string> GetAllVFXNames()
        {
            var names = new List<string>();
            foreach (var entry in vfxEntries)
            {
                if (!string.IsNullOrEmpty(entry.name))
                {
                    names.Add(entry.name);
                }
            }
            return names;
        }
        
        public void AddVFXEntry(VFXEntry entry)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.name))
            {
                vfxEntries.Add(entry);
                if (_vfxLookup != null)
                {
                    _vfxLookup[entry.name] = entry;
                }
            }
        }
        
        public void RemoveVFXEntry(string name)
        {
            vfxEntries.RemoveAll(e => e.name == name);
            _vfxLookup?.Remove(name);
        }
    }
    
    [System.Serializable]
    public class VFXEntry
    {
        public string name;
        public GameObject prefab;
        public float defaultDuration = 2f;
        public bool autoDestroy = true;
        public Vector3 defaultScale = Vector3.one;
        public bool followTarget = false;
        
        [Header("Audio")]
        public AudioClip audioClip;
        public float audioVolume = 1f;
        
        [Header("Categories")]
        public VFXCategory category = VFXCategory.General;
        public List<string> tags = new List<string>();
    }
    
    public enum VFXCategory
    {
        General,
        Combat,
        Environment,
        UI,
        Farming,
        Tools,
        Weather
    }
}