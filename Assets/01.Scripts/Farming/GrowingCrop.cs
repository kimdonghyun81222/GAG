using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _01.Scripts.Core.Inventory;
using _01.Scripts.Data;
using _01.Scripts.Farming.Mutations;
using UnityEngine;

// ItemDatabase, MutationData 등

namespace _01.Scripts.Farming
{
    public class GrowingCrop : MonoBehaviour
    {
        private CropData _baseCropData;
        private FarmPlot _farmPlot;
        private int _currentGrowthStageIndex = -1;
        private float _currentGrowthTimer = 0f;
        private GameObject _currentStageVisual;

        private bool _isWatered = false;
        private bool _isDriedOut = false;

        private PlantMutator _plantMutator;
        public PlantMutator Mutator => _plantMutator;

        private float _actualGrowthSpeedMultiplier = 1f;
        private string _currentVisualOverrideKey = null;

        private void Awake()
        {
            _plantMutator = GetComponent<PlantMutator>();
            if (_plantMutator == null)
            {
                _plantMutator = gameObject.AddComponent<PlantMutator>();
            }
        }

        public void StartGrowing(CropData data, FarmPlot plot)
        {
            _baseCropData = data;
            _farmPlot = plot;
            _currentGrowthStageIndex = -1; // 새 작물은 -1에서 시작하여 ProgressGrowthStage에서 0으로 증가
            _currentGrowthTimer = 0f;
            _isWatered = false;
            _isDriedOut = false;
            _actualGrowthSpeedMultiplier = 1f;
            _currentVisualOverrideKey = null;

            if (_baseCropData == null) {
                Debug.LogError("Cannot start growing, CropData is null!", this);
                Destroy(gameObject); // 오류 방지
                return;
            }

            _plantMutator.Initialize(this, _baseCropData);
            _plantMutator.AttemptMutationRoll(_baseCropData.potentialMutations); // RecalculateModifiedStats 내부 호출

            UpdateVisuals(); // 돌연변이로 인한 외형 변경 즉시 반영 시도
            StartCoroutine(ProgressGrowthStage());
        }

        public void LoadFromSerializableData(CropData baseData, FarmPlot plot, GrowingCropData_Serializable savedCropData)
        {
            _baseCropData = baseData;
            _farmPlot = plot;

            if (_baseCropData == null) {
                Debug.LogError($"Cannot load crop, Base CropData (ID: {savedCropData?.cropDataID}) is null or not found!", this);
                Destroy(gameObject); // 오류 방지
                _farmPlot?.NotifyLoadFailed(this); // FarmPlot에 로드 실패 알림 (선택적)
                return;
            }


            _currentGrowthStageIndex = savedCropData.currentGrowthStageIndex;
            _currentGrowthTimer = savedCropData.currentGrowthTimer;
            _isWatered = savedCropData.isWatered;
            _isDriedOut = savedCropData.isDriedOut;
            _actualGrowthSpeedMultiplier = 1f;
            _currentVisualOverrideKey = null;

            _plantMutator.Initialize(this, _baseCropData);

            if (savedCropData.activeMutationIDs != null && ItemDatabase.Instance != null)
            {
                foreach (string mutationId in savedCropData.activeMutationIDs)
                {
                    MutationData mutationSO = ItemDatabase.Instance.GetMutationByID(mutationId);
                    if (mutationSO != null)
                    {
                        // ApplyMutation은 ActiveMutations에 추가만 함. Recalculate는 별도 호출.
                        _plantMutator.ActiveMutations.Add(mutationSO); // 직접 추가 (ApplyMutation 중복 방지)
                    }
                    else Debug.LogWarning($"GrowingCrop ({_baseCropData.cropName}): Could not find MutationData with ID '{mutationId}' during load.", this);
                }
                _plantMutator.RecalculateModifiedStats(); // 모든 저장된 돌연변이 적용 후 한 번만 재계산
            }
            else if (savedCropData.activeMutationIDs != null && savedCropData.activeMutationIDs.Count > 0)
            {
                Debug.LogWarning($"GrowingCrop ({_baseCropData.cropName}): Could not load mutations as ItemDatabase.Instance is missing or no mutations to load.", this);
            }

            UpdateStatsFromMutator(); // Mutator 값 적용 (비주얼 포함)
            UpdateVisuals(); // 최종 비주얼 업데이트

            if (_currentGrowthStageIndex >= _baseCropData.growthStages.Count) // 이미 다 자란 상태로 로드
            {
                _farmPlot.NotifyCropFullyGrown(this);
            }
            else
            {
                StartCoroutine(ProgressGrowthStage());
            }
        }

        public GrowingCropData_Serializable GetSerializableData()
        {
            if (_baseCropData == null) {
                Debug.LogError("Cannot get serializable data, _baseCropData is null!", this);
                return null; // 또는 빈 데이터 반환
            }
            return new GrowingCropData_Serializable
            {
                cropDataID = _baseCropData.cropID,
                currentGrowthStageIndex = this._currentGrowthStageIndex,
                currentGrowthTimer = this._currentGrowthTimer,
                isWatered = this._isWatered,
                isDriedOut = this._isDriedOut,
                activeMutationIDs = _plantMutator.ActiveMutations.Select(m => m.mutationID).ToList()
            };
        }

        public void UpdateStatsFromMutator()
        {
            if (_plantMutator == null) return;
            _actualGrowthSpeedMultiplier = _plantMutator.GrowthSpeedMultiplier;
            if (_currentVisualOverrideKey != _plantMutator.VisualOverrideKey)
            {
                _currentVisualOverrideKey = _plantMutator.VisualOverrideKey;
                UpdateVisuals();
            }
        }

        private IEnumerator ProgressGrowthStage()
        {
            if (_baseCropData == null || _baseCropData.growthStages == null || _baseCropData.growthStages.Count == 0)
            {
                Debug.LogError("Cannot progress growth, CropData or growthStages are invalid.", this);
                _farmPlot.NotifyCropFullyGrown(this); // 문제 발생 시 즉시 완료 처리 시도
                yield break;
            }

            // 현재 성장 단계가 이미 마지막 단계를 넘어서고, 타이머도 해당 단계의 지속시간을 넘었다면, 이미 다 자란 것.
            if (_currentGrowthStageIndex >= _baseCropData.growthStages.Count -1 )
            {
                if (_currentGrowthStageIndex == _baseCropData.growthStages.Count -1 && _currentGrowthTimer >= _baseCropData.growthStages[_currentGrowthStageIndex].growthDuration)
                {
                    _farmPlot.NotifyCropFullyGrown(this);
                    yield break;
                }
                else if (_currentGrowthStageIndex >= _baseCropData.growthStages.Count) // 인덱스가 이미 초과
                {
                    _farmPlot.NotifyCropFullyGrown(this);
                    yield break;
                }
            }


            // 새 작물이거나, 이전 단계가 막 완료된 경우 (타이머=0) 다음 단계로 인덱스 증가
            if (_currentGrowthTimer == 0f)
            {
                if (_currentGrowthStageIndex < _baseCropData.growthStages.Count - 1)
                {
                    _currentGrowthStageIndex++;
                }
                else if (_currentGrowthStageIndex == _baseCropData.growthStages.Count - 1) // 이미 마지막 단계, 더 이상 증가 안함
                {
                    // 이 경우는 타이머가 돌고 수확 가능해짐
                }
                else // _currentGrowthStageIndex가 -1일때 (최초 심기)
                {
                    _currentGrowthStageIndex = 0;
                }
            }
        
            // 유효한 성장 단계인지 최종 확인
            if (_currentGrowthStageIndex < 0 || _currentGrowthStageIndex >= _baseCropData.growthStages.Count)
            {
                _farmPlot.NotifyCropFullyGrown(this); // 잘못된 상태면 수확 가능으로 처리
                yield break;
            }

            UpdateVisuals();

            CropGrowthStage currentStageInfo = _baseCropData.growthStages[_currentGrowthStageIndex];
            float timeToNextStage = currentStageInfo.growthDuration;

            while (_currentGrowthTimer < timeToNextStage)
            {
                float currentFrameGrowthRate = _actualGrowthSpeedMultiplier;
                if (_isWatered && !_isDriedOut) currentFrameGrowthRate *= 1.5f;
                else if (_isDriedOut) currentFrameGrowthRate *= 0.5f;
                _currentGrowthTimer += Time.deltaTime * currentFrameGrowthRate;
                yield return null;
            }

            // 현재 단계 완료
            if (_currentGrowthStageIndex >= _baseCropData.growthStages.Count - 1) // 마지막 단계였으면
            {
                _farmPlot.NotifyCropFullyGrown(this);
            }
            else // 다음 단계로
            {
                _currentGrowthTimer = 0f; // 다음 단계를 위해 타이머 리셋
                _isWatered = false;
                _isDriedOut = false;
                StartCoroutine(ProgressGrowthStage());
            }
        }

        private void UpdateVisuals()
        {
            if (_currentStageVisual != null) Destroy(_currentStageVisual);
            _currentStageVisual = null;

            if (_baseCropData == null || _currentGrowthStageIndex < 0 || _currentGrowthStageIndex >= _baseCropData.growthStages.Count)
            {
                return;
            }

            GameObject prefabToInstantiate = null;
            if (!string.IsNullOrEmpty(_currentVisualOverrideKey))
            {
                GameObject overridePrefab = Resources.Load<GameObject>($"MutationVisuals/{_currentVisualOverrideKey}");
                if (overridePrefab != null) prefabToInstantiate = overridePrefab;
                else Debug.LogWarning($"VisualOverrideKey '{_currentVisualOverrideKey}' not found in Resources/MutationVisuals/.");
            }
            if (prefabToInstantiate == null)
            {
                prefabToInstantiate = _baseCropData.growthStages[_currentGrowthStageIndex].stagePrefab;
            }

            if (prefabToInstantiate != null)
            {
                _currentStageVisual = Instantiate(prefabToInstantiate, transform.position, transform.rotation, transform);
                if (_currentStageVisual != null) _currentStageVisual.transform.localPosition = Vector3.zero;
                else Debug.LogError($"Failed to instantiate visual prefab for {_baseCropData.cropName}", this);
            }
        }

        public void NotifyWatered() { _isWatered = true; _isDriedOut = false; }
        public void NotifyDried() { _isWatered = false; _isDriedOut = true; }

        public int GetFinalHarvestAmount()
        {
            if (_baseCropData == null) return 0;
            int baseAmount = _baseCropData.GetRandomHarvestAmount();
            if (_plantMutator == null) return baseAmount;
            return Mathf.Max(0, Mathf.RoundToInt((baseAmount + _plantMutator.AdditionalYield) * _plantMutator.YieldMultiplier));
        }

        public int GetFinalSellPrice(int baseItemSellPrice)
        {
            if (_plantMutator == null) return baseItemSellPrice;
            return Mathf.Max(0, Mathf.RoundToInt((baseItemSellPrice + _plantMutator.AdditionalSellPrice) * _plantMutator.SellPriceMultiplier));
        }

        public List<KeyValuePair<ItemData, int>> GetBonusDrops() => _plantMutator?.BonusItemDrops ?? new List<KeyValuePair<ItemData, int>>();

        private void OnDestroy()
        {
            if (_currentStageVisual != null) Destroy(_currentStageVisual);
            StopAllCoroutines();
        }
    }
}