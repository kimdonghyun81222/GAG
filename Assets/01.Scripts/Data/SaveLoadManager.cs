using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _01.Scripts.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _01.Scripts.Data
{
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        [SerializeField] private string defaultSaveFileName = "MyGameSave";
        private List<ISaveable> _saveableObjects;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _saveableObjects = new List<ISaveable>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 씬 로드 시점에 ISaveable 객체들을 다시 수집할 필요가 있다면 여기서 처리
            // 현재는 각 ISaveable 객체가 스스로 Register/Unregister 하도록 유도
        }

        public void RegisterSaveable(ISaveable saveable)
        {
            if (saveable == null) return;
            if (!_saveableObjects.Contains(saveable))
            {
                _saveableObjects.Add(saveable);
                // Debug.Log($"Registered ISaveable: {saveable.GetType().Name} on {((MonoBehaviour)saveable).gameObject.name}");
            }
        }

        public void UnregisterSaveable(ISaveable saveable)
        {
            if (saveable == null) return;
            if (_saveableObjects.Contains(saveable))
            {
                _saveableObjects.Remove(saveable);
                // Debug.Log($"Unregistered ISaveable: {saveable.GetType().Name} on {((MonoBehaviour)saveable).gameObject.name}");
            }
        }
        
        // Helper to check if a MonoBehaviour is part of DontDestroyOnLoad
        private bool IsDontDestroyOnLoad(MonoBehaviour mb)
        {
            if (mb == null || mb.gameObject == null) return false;
            return mb.gameObject.scene.buildIndex == -1;
        }


        public void TriggerSaveGame(string saveFileName = null)
        {
            if (string.IsNullOrEmpty(saveFileName)) saveFileName = defaultSaveFileName;

            GameSaveData saveData = new GameSaveData
            {
                lastSavedSceneName = SceneManager.GetActiveScene().name
            };

            // 필터링된 ISaveable 객체들에 대해 PopulateSaveData 호출
            // DontDestroyOnLoad 객체들 먼저 처리
            foreach (ISaveable saveable in _saveableObjects.Where(obj => obj != null && IsDontDestroyOnLoad((MonoBehaviour)obj)))
            {
                try
                {
                    saveable.PopulateSaveData(saveData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error populating save data for DontDestroyOnLoad object {saveable.GetType()} on {((MonoBehaviour)saveable).name}: {e.Message}\n{e.StackTrace}");
                }
            }

            // 현재 활성 씬에 있는 객체들 처리
            foreach (ISaveable saveable in _saveableObjects.Where(obj => obj != null && !IsDontDestroyOnLoad((MonoBehaviour)obj) && ((MonoBehaviour)obj).gameObject.scene == SceneManager.GetActiveScene()))
            {
                 try
                {
                    saveable.PopulateSaveData(saveData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error populating save data for scene object {saveable.GetType()} on {((MonoBehaviour)saveable).name}: {e.Message}\n{e.StackTrace}");
                }
            }

            SaveSystem.SaveGame(saveData, saveFileName);
        }

        public void TriggerLoadGame(string saveFileName = null)
        {
            if (string.IsNullOrEmpty(saveFileName)) saveFileName = defaultSaveFileName;

            GameSaveData saveData = SaveSystem.LoadGame(saveFileName);
            if (saveData == null)
            {
                Debug.LogWarning("Failed to load game data. Aborting load process.");
                return;
            }

            if (SceneManager.GetActiveScene().name != saveData.lastSavedSceneName)
            {
                StartCoroutine(LoadSceneAndThenApplyData(saveData));
            }
            else
            {
                ApplyLoadedData(saveData);
            }
        }

        private IEnumerator LoadSceneAndThenApplyData(GameSaveData saveData) // IEnumerator<WaitForSeconds> -> IEnumerator
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(saveData.lastSavedSceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            yield return null; // 씬 로드 후 한 프레임 더 대기 (Start 함수들 실행 보장)
            ApplyLoadedData(saveData);
        }

        private void ApplyLoadedData(GameSaveData saveData)
        {
            // DontDestroyOnLoad 객체들 먼저 로드
            foreach (ISaveable saveable in _saveableObjects.Where(obj => obj != null && IsDontDestroyOnLoad((MonoBehaviour)obj)))
            {
                 try
                {
                    saveable.LoadFromSaveData(saveData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading save data for DontDestroyOnLoad object {saveable.GetType()} on {((MonoBehaviour)saveable).name}: {e.Message}\n{e.StackTrace}");
                }
            }

            // 현재 활성 씬에 있는 객체들 로드
            // 이 시점에는 새 씬의 객체들이 RegisterSaveable을 통해 _saveableObjects에 등록되었어야 함.
            foreach (ISaveable saveable in _saveableObjects.Where(obj => obj != null && !IsDontDestroyOnLoad((MonoBehaviour)obj) && ((MonoBehaviour)obj).gameObject.scene == SceneManager.GetActiveScene()))
            {
                try
                {
                    saveable.LoadFromSaveData(saveData);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error loading save data for scene object {saveable.GetType()} on {((MonoBehaviour)saveable).name}: {e.Message}\n{e.StackTrace}");
                }
            }
            Debug.Log("Game data applied to saveable objects.");
            UIManager.Instance?.ShowNotification("게임 데이터를 적용했습니다.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                TriggerSaveGame();
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                TriggerLoadGame();
            }
        }
    }
}