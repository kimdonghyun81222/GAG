using System.IO;
using _01.Scripts.Core;
using UnityEngine;

namespace _01.Scripts.Data
{
    public static class SaveSystem
    {
        private static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Saves/";
        private const string SAVE_FILE_EXTENSION = ".json";
        
        private static void EnsureSaveFolderExists()
        {
            if (!Directory.Exists(SAVE_FOLDER))
            {
                Directory.CreateDirectory(SAVE_FOLDER);
            }
        }

        public static void SaveGame(GameSaveData data, string saveFileName)
        {
            EnsureSaveFolderExists();
            string fullPath = SAVE_FOLDER + saveFileName + SAVE_FILE_EXTENSION;

            try
            {
                string json = JsonUtility.ToJson(data, true); // 'true' for pretty print
                File.WriteAllText(fullPath, json);
                Debug.Log($"Game saved to: {fullPath}");
                UIManager.Instance?.ShowNotification($"게임이 '{saveFileName}'에 저장되었습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save game to {fullPath}: {e.Message}");
                UIManager.Instance?.ShowNotification("게임 저장에 실패했습니다.");
            }
        }

        public static GameSaveData LoadGame(string saveFileName)
        {
            string fullPath = SAVE_FOLDER + saveFileName + SAVE_FILE_EXTENSION;

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"Save file not found at: {fullPath}");
                UIManager.Instance?.ShowNotification($"저장 파일 '{saveFileName}'을(를) 찾을 수 없습니다.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log($"Game loaded from: {fullPath}");
                UIManager.Instance?.ShowNotification($"'{saveFileName}'에서 게임을 불러왔습니다.");
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game from {fullPath}: {e.Message}");
                UIManager.Instance?.ShowNotification("게임 불러오기에 실패했습니다.");
                return null;
            }
        }

        public static bool DoesSaveFileExist(string saveFileName)
        {
            return File.Exists(SAVE_FOLDER + saveFileName + SAVE_FILE_EXTENSION);
        }

        public static void DeleteSaveFile(string saveFileName)
        {
            string fullPath = SAVE_FOLDER + saveFileName + SAVE_FILE_EXTENSION;
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                    Debug.Log($"Save file deleted: {fullPath}");
                    UIManager.Instance?.ShowNotification($"저장 파일 '{saveFileName}'이(가) 삭제되었습니다.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to delete save file {fullPath}: {e.Message}");
                    UIManager.Instance?.ShowNotification("저장 파일 삭제에 실패했습니다.");
                }
            }
            else
            {
                Debug.LogWarning($"Could not delete save file, not found at: {fullPath}");
            }
        }

        public static string[] GetSaveFileNames()
        {
            EnsureSaveFolderExists();
            DirectoryInfo directoryInfo = new DirectoryInfo(SAVE_FOLDER);
            FileInfo[] saveFiles = directoryInfo.GetFiles("*" + SAVE_FILE_EXTENSION);
            string[] names = new string[saveFiles.Length];
            for(int i=0; i < saveFiles.Length; i++)
            {
                names[i] = Path.GetFileNameWithoutExtension(saveFiles[i].Name);
            }
            return names;
        }
    }
}