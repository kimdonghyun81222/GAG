using System.Linq;
using _01.Scripts.Core;
using _01.Scripts.Core.Inventory;
using _01.Scripts.Data;
using _01.Scripts.Player;
using UnityEngine;

namespace _01.Scripts.Farming
{
    public enum FarmPlotState
    {
        Empty,
        Seeded,
        Watered,
        Grown,
        Withered
    }

    public class FarmPlot : MonoBehaviour, IInteractable, ISaveable
    {
        [Header("State")]
        public FarmPlotState currentState = FarmPlotState.Empty;

        [Header("Crop References")]
        [SerializeField] private Transform cropSpawnPoint;
        private GrowingCrop currentCropInstance;

        [Header("Visuals")]
        [SerializeField] private Renderer plotRenderer;
        [SerializeField] private Material dryMaterial;
        [SerializeField] private Material wetMaterial;

        [Header("Settings")]
        [SerializeField] private float wateredDurationSeconds = 300f;
        private float _currentWateredTimeRemaining = 0f;

        [Header("Save/Load Settings")]
        [SerializeField] private string _uniquePlotID;
        public string UniqueID { get => _uniquePlotID; set => _uniquePlotID = value; }

        private void Awake()
        {
            if (string.IsNullOrEmpty(_uniquePlotID))
            {
                _uniquePlotID = $"FarmPlot_{gameObject.name}_{transform.GetSiblingIndex()}_{GetInstanceID()}";
#if UNITY_EDITOR
                if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
                Debug.LogWarning($"FarmPlot '{gameObject.name}' had no UniqueID, generated: {_uniquePlotID}. Assign a persistent unique ID in editor.", this);
            }
        }

        private void OnEnable()
        {
            if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.RegisterSaveable(this);
            else Invoke(nameof(RegisterSelfToSaveLoadManager), 0.01f);
        }
        private void RegisterSelfToSaveLoadManager()
        {
            if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.RegisterSaveable(this);
            else Debug.LogWarning($"FarmPlot ({UniqueID}): SaveLoadManager not found to register.", this);
        }
        private void OnDisable()
        {
            if (SaveLoadManager.Instance != null) SaveLoadManager.Instance.UnregisterSaveable(this);
        }

        private void Start()
        {
            if (plotRenderer == null) plotRenderer = GetComponent<Renderer>();
            if (cropSpawnPoint == null) cropSpawnPoint = transform;
            UpdatePlotVisuals();

            // 로드된 게임이 아니고, 초기 상태가 물 준 상태라면 타이머 시작 (주로 디버깅용)
            if (currentState == FarmPlotState.Watered && _currentWateredTimeRemaining == 0 && wateredDurationSeconds > 0)
            {
                _currentWateredTimeRemaining = wateredDurationSeconds;
            }
        }

        private void Update()
        {
            if (currentState == FarmPlotState.Watered && wateredDurationSeconds > 0 && currentCropInstance != null) // 작물이 있을 때만 마름
            {
                _currentWateredTimeRemaining -= Time.deltaTime;
                if (_currentWateredTimeRemaining <= 0)
                {
                    currentState = FarmPlotState.Seeded;
                    _currentWateredTimeRemaining = 0;
                    UpdatePlotVisuals();
                    currentCropInstance?.NotifyDried();
                }
            }
        }

        public void Interact(PlayerController player)
        {
            ItemData heldItem = InventoryManager.Instance?.GetEquippedItem();
            switch (currentState)
            {
                case FarmPlotState.Empty:
                    if (heldItem?.itemType == ItemType.Seed && heldItem.cropToGrow != null)
                    {
                        if (InventoryManager.Instance.HasItem(heldItem, 1)) PlantSeed(heldItem.cropToGrow, player, heldItem);
                        else UIManager.Instance?.ShowNotification($"{heldItem.itemName}이(가) 부족합니다.");
                    }
                    else UIManager.Instance?.ShowNotification("씨앗을 손에 들어주세요.");
                    break;
                case FarmPlotState.Seeded:
                    if (heldItem?.itemType == ItemType.Tool && heldItem.itemName.Contains("Watering Can")) WaterPlot();
                    else UIManager.Instance?.ShowNotification("작물이 자라고 있습니다. 물을 주세요.");
                    break;
                case FarmPlotState.Watered:
                    UIManager.Instance?.ShowNotification("작물이 잘 자라고 있습니다.");
                    break;
                case FarmPlotState.Grown:
                    HarvestCrop(player);
                    break;
                case FarmPlotState.Withered:
                    ClearWitheredCrop(player);
                    break;
            }
        }

        public string GetInteractionPrompt()
        {
            ItemData heldItem = InventoryManager.Instance?.GetEquippedItem();
            switch (currentState)
            {
                case FarmPlotState.Empty:
                    return (heldItem?.itemType == ItemType.Seed) ? $"[{heldItem.itemName}] 심기 (E)" : "씨앗 선택 후 사용 (E)";
                case FarmPlotState.Seeded:
                    return (heldItem?.itemType == ItemType.Tool && heldItem.itemName.Contains("Watering Can")) ? "물 주기 (E)" : "성장 중 (물을 주세요)";
                case FarmPlotState.Watered: return "성장 중...";
                case FarmPlotState.Grown:
                    string cropNamePrompt = _cropDataFromCurrentInstance()?.harvestedItem?.itemName ?? "작물";
                    if(currentCropInstance?.Mutator.ActiveMutations.Any() == true)
                    {
                        string mutationNames = string.Join(", ", currentCropInstance.Mutator.ActiveMutations.Select(m => m.mutationName));
                        return $"[돌연변이! {mutationNames}] {cropNamePrompt} 수확하기 (E)";
                    }
                    return $"[{cropNamePrompt}] 수확하기 (E)";
                case FarmPlotState.Withered: return "마른 작물 제거 (E)";
                default: return "";
            }
        }
    
        private CropData _getBaseCropDataFromInstance(GrowingCrop crop)
        {
            if (crop == null) return null;
            // GrowingCrop에 BaseCropData를 직접 접근할 수 있는 public getter를 만드는 것이 더 좋음
            // 예: public CropData BaseCropData => _baseCropData;
            // 여기서는 리플렉션 대신, GrowingCrop이 초기화될 때 _baseCropData를 가지고 있다고 가정.
            // PlantMutator._baseCropData를 참조하는 것은 PlantMutator의 내부 구현에 의존적이므로 변경.
            // GrowingCrop.cs에 public CropData GetBaseCropData() { return _baseCropData; } 추가하고 호출 권장.
            // 임시로 PlantMutator를 통해 접근하던 코드는 제거. GrowingCrop이 직접 baseData를 알아야 함.
            var baseDataField = typeof(GrowingCrop).GetField("_baseCropData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return baseDataField?.GetValue(crop) as CropData;
        }
        private CropData _cropDataFromCurrentInstance() => _getBaseCropDataFromInstance(currentCropInstance);


        public void PlantSeed(CropData cropDataToPlant, PlayerController planter, ItemData seedItemUsed)
        {
            if (currentState != FarmPlotState.Empty) return;
            if (InventoryManager.Instance.RemoveItem(seedItemUsed, 1))
            {
                currentState = FarmPlotState.Seeded;
                GameObject cropInstanceObj = new GameObject(cropDataToPlant.cropName + "_Instance");
                cropInstanceObj.transform.SetParent(cropSpawnPoint);
                cropInstanceObj.transform.localPosition = Vector3.zero;
                currentCropInstance = cropInstanceObj.AddComponent<GrowingCrop>();
                currentCropInstance.StartGrowing(cropDataToPlant, this);
                UIManager.Instance?.ShowNotification($"{cropDataToPlant.cropName} 씨앗을 심었습니다.");
                UpdatePlotVisuals();
            }
            else UIManager.Instance?.ShowNotification($"{seedItemUsed.itemName}이(가) 부족합니다.");
        }

        public void WaterPlot()
        {
            if (currentState == FarmPlotState.Seeded || currentState == FarmPlotState.Watered)
            {
                currentState = FarmPlotState.Watered;
                _currentWateredTimeRemaining = wateredDurationSeconds;
                currentCropInstance?.NotifyWatered();
                UIManager.Instance?.ShowNotification("물을 주었습니다.");
                UpdatePlotVisuals();
            }
        }

        public void NotifyCropFullyGrown(GrowingCrop grownCrop)
        {
            if (currentCropInstance == grownCrop)
            {
                currentState = FarmPlotState.Grown;
                CropData baseCropData = _getBaseCropDataFromInstance(grownCrop);
                string cropDisplayName = baseCropData?.cropName ?? "작물";
                if (grownCrop.Mutator.ActiveMutations.Any())
                {
                    string mutationNames = string.Join(", ", grownCrop.Mutator.ActiveMutations.Select(m => m.mutationName));
                    UIManager.Instance?.ShowNotification($"[돌연변이!] {cropDisplayName}이(가) 다 자랐습니다: {mutationNames}");
                }
                else UIManager.Instance?.ShowNotification($"{cropDisplayName}이(가) 다 자랐습니다!");
                UpdatePlotVisuals();
            }
        }
        public void NotifyLoadFailed(GrowingCrop failedCrop) // GrowingCrop에서 로드 실패 시 호출
        {
            if (currentCropInstance == failedCrop)
            {
                Debug.LogError($"FarmPlot ({UniqueID}): Crop instance reported load failure. Resetting plot.", this);
                if (currentCropInstance != null) Destroy(currentCropInstance.gameObject);
                currentCropInstance = null;
                currentState = FarmPlotState.Empty;
                _currentWateredTimeRemaining = 0f;
                UpdatePlotVisuals();
            }
        }

        public void HarvestCrop(PlayerController harvester)
        {
            if (currentState != FarmPlotState.Grown || currentCropInstance == null) return;
            CropData baseCropData = _getBaseCropDataFromInstance(currentCropInstance);
            if (baseCropData == null)
            {
                Debug.LogError($"Could not retrieve CropData for harvesting on plot {UniqueID}!", this);
                return;
            }
            int amountHarvested = currentCropInstance.GetFinalHarvestAmount();
            ItemData harvestedItemData = baseCropData.harvestedItem;
            if (amountHarvested > 0 && harvestedItemData != null)
            {
                if (!InventoryManager.Instance.AddItem(harvestedItemData, amountHarvested))
                    UIManager.Instance?.ShowNotification($"인벤토리가 가득 차서 {harvestedItemData.itemName}을(를) 얻지 못했습니다.");
                else UIManager.Instance?.ShowNotification($"{harvestedItemData.itemName} {amountHarvested}개를 수확했습니다.");
            }
            var bonusDrops = currentCropInstance.GetBonusDrops();
            if (bonusDrops != null) bonusDrops.ForEach(drop => { /* ... 이전과 동일한 보너스 드랍 로직 ... */ });

            Destroy(currentCropInstance.gameObject);
            currentCropInstance = null;
            currentState = FarmPlotState.Empty;
            _currentWateredTimeRemaining = 0f;
            UpdatePlotVisuals();
        }

        public void ClearWitheredCrop(PlayerController clearer) { /* ... 이전과 동일 ... */ }
        private void UpdatePlotVisuals() { /* ... 이전과 동일 ... */ }
        private void OnValidate() { /* ... 이전과 동일, UniqueID 자동생성 로직은 Awake로 이동 ... */ }

        public void PopulateSaveData(GameSaveData saveData)
        {
            FarmPlotData_Serializable plotSaveData = new FarmPlotData_Serializable
            {
                uniquePlotID = this.UniqueID,
                farmPlotStateValue = (int)this.currentState,
                currentWateredTimeRemaining = this._currentWateredTimeRemaining,
                hasCrop = this.currentCropInstance != null
            };
            if (this.currentCropInstance != null)
            {
                plotSaveData.cropData = this.currentCropInstance.GetSerializableData();
                if(plotSaveData.cropData == null) // GrowingCrop에서 데이터 생성 실패 시
                {
                    plotSaveData.hasCrop = false; // 작물 없는 것으로 저장
                    Debug.LogWarning($"FarmPlot ({UniqueID}): GrowingCrop failed to provide serializable data. Saving as no crop.", this);
                }
            }
            int existingIndex = saveData.farmPlotDataList.FindIndex(p => p.uniquePlotID == this.UniqueID);
            if (existingIndex != -1) saveData.farmPlotDataList[existingIndex] = plotSaveData;
            else saveData.farmPlotDataList.Add(plotSaveData);
        }

        public void LoadFromSaveData(GameSaveData saveData)
        {
            FarmPlotData_Serializable plotSaveData = saveData.farmPlotDataList.FirstOrDefault(p => p.uniquePlotID == this.UniqueID);
            if (plotSaveData == null)
            {
                currentState = FarmPlotState.Empty; // 저장 데이터 없으면 초기화
                _currentWateredTimeRemaining = 0f;
                if (currentCropInstance != null) Destroy(currentCropInstance.gameObject);
                currentCropInstance = null;
                UpdatePlotVisuals();
                return;
            }

            this.currentState = (FarmPlotState)plotSaveData.farmPlotStateValue;
            this._currentWateredTimeRemaining = plotSaveData.currentWateredTimeRemaining;

            if (this.currentCropInstance != null) Destroy(this.currentCropInstance.gameObject);
            this.currentCropInstance = null;

            if (plotSaveData.hasCrop && plotSaveData.cropData != null)
            {
                if (ItemDatabase.Instance == null || !ItemDatabase.Instance.IsFullyInitialized())
                {
                    Debug.LogError($"FarmPlot ({UniqueID}): ItemDatabase not ready. Cannot load crop (ID: {plotSaveData.cropData.cropDataID}).", this);
                    this.currentState = FarmPlotState.Empty; // DB 미준비 시 작물 로드 실패
                }
                else
                {
                    CropData cropSO = ItemDatabase.Instance.GetCropDataByID(plotSaveData.cropData.cropDataID);
                    if (cropSO != null)
                    {
                        GameObject cropInstanceObj = new GameObject(cropSO.cropName + "_Instance_Loaded");
                        cropInstanceObj.transform.SetParent(cropSpawnPoint);
                        cropInstanceObj.transform.localPosition = Vector3.zero;
                        this.currentCropInstance = cropInstanceObj.AddComponent<GrowingCrop>();
                        this.currentCropInstance.LoadFromSerializableData(cropSO, this, plotSaveData.cropData);
                    }
                    else
                    {
                        Debug.LogError($"FarmPlot ({UniqueID}): Could not find CropData with ID '{plotSaveData.cropData.cropDataID}'. Cannot load crop.", this);
                        this.currentState = FarmPlotState.Empty;
                    }
                }
            }
            UpdatePlotVisuals();
        }
    }
}