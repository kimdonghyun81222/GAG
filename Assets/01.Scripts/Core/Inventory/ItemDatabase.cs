using System.Collections.Generic;
using _01.Scripts.Farming;
using _01.Scripts.Farming.Mutations;
using UnityEngine;
// 네임스페이스 확인 필요: CropData, MutationData가 있는 실제 네임스페이스로 변경
// 예시: using GrowingAGarden.SO.Crops;
// 예시: using GrowingAGarden.SO.Mutations;
// 만약 SO 네임스페이스를 사용한다면:


namespace _01.Scripts.Core.Inventory // ScriptableObject 네임스페이스 (또는 다른 적절한 네임스페이스)
{
    [CreateAssetMenu(fileName = "GlobalItemDatabase", menuName = "Grow A Garden/Databases/Item Database", order = 200)]
    public class ItemDatabase : ScriptableObject
    {
        public static ItemDatabase Instance { get; private set; } // 싱글톤 인스턴스

        [Header("Item Database")]
        public List<ItemData> allItems = new List<ItemData>();
        private Dictionary<string, ItemData> _itemDictionary;
        private bool _isItemDbInitialized = false;

        [Header("Crop Database")]
        public List<CropData> allCrops = new List<CropData>();
        private Dictionary<string, CropData> _cropDictionary;
        private bool _isCropDbInitialized = false;

        [Header("Mutation Database")]
        public List<MutationData> allMutations = new List<MutationData>();
        private Dictionary<string, MutationData> _mutationDictionary;
        private bool _isMutationDbInitialized = false;


        // ScriptableObject가 로드될 때 Instance 설정 (게임 시작 시점에 로드되도록 보장 필요)
        // 또는 GameManager 등에서 명시적으로 Instance를 할당하고 InitializeAllDatabases 호출
        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(this); // SO는 DontDestroyOnLoad 직접 사용 불가. 로드 방식에 따라 관리.
                // InitializeAllDatabases(); // OnEnable에서 바로 초기화할 수도 있지만, 시점 문제 발생 가능성
            }
            else if (Instance != this)
            {
                // 중복 인스턴스 경고 또는 처리 (보통 SO는 하나만 로드됨)
                // Debug.LogWarning("Another instance of ItemDatabase already exists. Ignoring this one.", this);
            }
        }
        public bool IsFullyInitialized() => _isItemDbInitialized && _isCropDbInitialized && _isMutationDbInitialized;


        public void InitializeAllDatabases()
        {
            if (Instance == null) Instance = this; // 호출 시점에 인스턴스 재확인

            if (!_isItemDbInitialized) InitializeItemDatabaseInternal();
            if (!_isCropDbInitialized) InitializeCropDatabaseInternal();
            if (!_isMutationDbInitialized) InitializeMutationDatabaseInternal();

            if (IsFullyInitialized())
            {
                Debug.Log("All databases in ItemDatabase initialized successfully.");
            }
        }

        private void InitializeItemDatabaseInternal()
        {
            _itemDictionary = new Dictionary<string, ItemData>();
            foreach (ItemData item in allItems)
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.itemID))
                {
                    Debug.LogWarning($"Item '{item.itemName}' (asset: {item.name}) has no ItemID.", item);
                    continue;
                }
                if (!_itemDictionary.ContainsKey(item.itemID)) _itemDictionary.Add(item.itemID, item);
                else Debug.LogWarning($"Duplicate ItemID '{item.itemID}' for item '{item.itemName}'.", item);
            }
            _isItemDbInitialized = true;
            Debug.Log($"Item Database Initialized with {_itemDictionary.Count} items.");
        }

        private void InitializeCropDatabaseInternal()
        {
            _cropDictionary = new Dictionary<string, CropData>();
            foreach (CropData crop in allCrops)
            {
                if (crop == null) continue;
                if (string.IsNullOrEmpty(crop.cropID)) // CropData의 고유 ID 필드 사용
                {
                     Debug.LogWarning($"CropData asset '{crop.name}' has no cropID and will not be added.", crop);
                    continue;
                }
                if (!_cropDictionary.ContainsKey(crop.cropID)) _cropDictionary.Add(crop.cropID, crop);
                else Debug.LogWarning($"Duplicate CropID '{crop.cropID}' for crop '{crop.cropName}'.", crop);
            }
            _isCropDbInitialized = true;
            Debug.Log($"Crop Database Initialized with {_cropDictionary.Count} crops.");
        }

        private void InitializeMutationDatabaseInternal()
        {
            _mutationDictionary = new Dictionary<string, MutationData>();
            foreach (MutationData mutation in allMutations)
            {
                if (mutation == null) continue;
                if (string.IsNullOrEmpty(mutation.mutationID))
                {
                     Debug.LogWarning($"MutationData asset '{mutation.name}' has no mutationID.", mutation);
                    continue;
                }
                if (!_mutationDictionary.ContainsKey(mutation.mutationID)) _mutationDictionary.Add(mutation.mutationID, mutation);
                else Debug.LogWarning($"Duplicate MutationID '{mutation.mutationID}' for mutation '{mutation.mutationName}'.", mutation);
            }
            _isMutationDbInitialized = true;
            Debug.Log($"Mutation Database Initialized with {_mutationDictionary.Count} mutations.");
        }

        public ItemData GetItemByID(string itemID)
        {
            if (!_isItemDbInitialized) { /*Debug.LogWarning("ItemDB not ready for GetItemByID.");*/ return null; }
            if (string.IsNullOrEmpty(itemID)) return null;
            _itemDictionary.TryGetValue(itemID, out ItemData item);
            // if(item == null) Debug.LogWarning($"Item with ID '{itemID}' not found in ItemDatabase.");
            return item;
        }

        public CropData GetCropDataByID(string cropID)
        {
            if (!_isCropDbInitialized) { /*Debug.LogWarning("CropDB not ready for GetCropDataByID.");*/ return null; }
            if (string.IsNullOrEmpty(cropID)) return null;
            _cropDictionary.TryGetValue(cropID, out CropData crop);
            // if(crop == null) Debug.LogWarning($"CropData with ID '{cropID}' not found in ItemDatabase (CropDB).");
            return crop;
        }

        public MutationData GetMutationByID(string mutationID)
        {
            if (!_isMutationDbInitialized) { /*Debug.LogWarning("MutationDB not ready for GetMutationByID.");*/ return null; }
            if (string.IsNullOrEmpty(mutationID)) return null;
            _mutationDictionary.TryGetValue(mutationID, out MutationData mutation);
            // if(mutation == null) Debug.LogWarning($"MutationData with ID '{mutationID}' not found in ItemDatabase (MutationDB).");
            return mutation;
        }

        [ContextMenu("Force Initialize All Databases (Editor)")]
        private void ForceInitializeAllEditor()
        {
            _isItemDbInitialized = false;
            _isCropDbInitialized = false;
            _isMutationDbInitialized = false;
            InitializeAllDatabases();
            Debug.Log("All Databases Manually Initialized via Context Menu.");
        }
    }
}