using System.Collections.Generic;
using UnityEngine;

namespace GrowAGarden.UI._01.Scripts.UI.Menus.InGame.Dialog
{
    [System.Serializable]
    public class DialogData
    {
        [Header("Dialog Identity")]
        public string dialogId;
        public string dialogName;
        
        [Header("Content")]
        public string speakerName;
        [TextArea(3, 6)]
        public string dialogText;
        
        [Header("Choices")]
        public List<DialogChoice> choices = new List<DialogChoice>();
        
        [Header("Flow Control")]
        public string nextDialogId; // If no choices, auto-continue to this
        public bool isEndDialog = false;
        
        [Header("Visual")]
        public Sprite speakerPortrait;
        public AudioClip voiceClip;
        
        [Header("Timing")]
        public float textSpeed = 30f; // Characters per second
        public float autoAdvanceDelay = 0f; // 0 = wait for input
        
        // Helper methods
        public bool HasChoices()
        {
            return choices != null && choices.Count > 0;
        }
        
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(dialogId)) return false;
            if (string.IsNullOrEmpty(dialogText)) return false;
            if (!HasChoices() && !isEndDialog && string.IsNullOrEmpty(nextDialogId)) return false;
            
            return true;
        }
        
        public DialogChoice GetChoice(int index)
        {
            if (choices == null || index < 0 || index >= choices.Count)
                return null;
            
            return choices[index];
        }
    }

    [System.Serializable]
    public class DialogChoice
    {
        [TextArea(2, 4)]
        public string choiceText;
        public string targetDialogId;
        public bool isEnabled = true;
        
        // Simple conditions
        public bool requiresItem = false;
        public string requiredItemId;
        public int requiredItemCount = 1;
        
        public bool IsAvailable()
        {
            if (!isEnabled) return false;
            
            // Check item requirement
            if (requiresItem)
            {
                // TODO: Implement item checking
                // return PlayerInventory.HasItem(requiredItemId, requiredItemCount);
            }
            
            return true;
        }
    }
    
    // Container for multiple dialogs
    [CreateAssetMenu(fileName = "New Dialog Collection", menuName = "Grow A Garden/Dialog/Dialog Collection")]
    public class DialogCollection : ScriptableObject
    {
        [Header("Collection Info")]
        public string collectionName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Dialogs")]
        public List<DialogData> dialogs = new List<DialogData>();
        
        public DialogData GetDialog(string dialogId)
        {
            if (string.IsNullOrEmpty(dialogId)) return null;
            
            foreach (var dialog in dialogs)
            {
                if (dialog.dialogId == dialogId)
                    return dialog;
            }
            
            Debug.LogWarning($"Dialog not found: {dialogId} in collection: {collectionName}");
            return null;
        }
        
        public bool HasDialog(string dialogId)
        {
            return GetDialog(dialogId) != null;
        }
        
        public void AddDialog(DialogData dialog)
        {
            if (dialog != null && !string.IsNullOrEmpty(dialog.dialogId))
            {
                // Check for duplicate IDs
                if (!HasDialog(dialog.dialogId))
                {
                    dialogs.Add(dialog);
                }
                else
                {
                    Debug.LogWarning($"Dialog ID already exists: {dialog.dialogId}");
                }
            }
        }
        
        public void RemoveDialog(string dialogId)
        {
            for (int i = dialogs.Count - 1; i >= 0; i--)
            {
                if (dialogs[i].dialogId == dialogId)
                {
                    dialogs.RemoveAt(i);
                    break;
                }
            }
        }
        
        public List<string> GetAllDialogIds()
        {
            var ids = new List<string>();
            foreach (var dialog in dialogs)
            {
                if (!string.IsNullOrEmpty(dialog.dialogId))
                {
                    ids.Add(dialog.dialogId);
                }
            }
            return ids;
        }
        
        public bool ValidateCollection()
        {
            bool isValid = true;
            
            // Check for duplicate IDs
            var usedIds = new HashSet<string>();
            foreach (var dialog in dialogs)
            {
                if (string.IsNullOrEmpty(dialog.dialogId))
                {
                    Debug.LogError($"Dialog with empty ID found in collection: {collectionName}");
                    isValid = false;
                    continue;
                }
                
                if (usedIds.Contains(dialog.dialogId))
                {
                    Debug.LogError($"Duplicate dialog ID: {dialog.dialogId} in collection: {collectionName}");
                    isValid = false;
                }
                else
                {
                    usedIds.Add(dialog.dialogId);
                }
                
                // Validate individual dialog
                if (!dialog.IsValid())
                {
                    Debug.LogError($"Invalid dialog: {dialog.dialogId} in collection: {collectionName}");
                    isValid = false;
                }
            }
            
            // Check for broken references
            foreach (var dialog in dialogs)
            {
                // Check next dialog reference
                if (!string.IsNullOrEmpty(dialog.nextDialogId) && !HasDialog(dialog.nextDialogId))
                {
                    Debug.LogWarning($"Broken reference in dialog {dialog.dialogId}: nextDialogId '{dialog.nextDialogId}' not found");
                }
                
                // Check choice references
                if (dialog.choices != null)
                {
                    foreach (var choice in dialog.choices)
                    {
                        if (!string.IsNullOrEmpty(choice.targetDialogId) && !HasDialog(choice.targetDialogId))
                        {
                            Debug.LogWarning($"Broken reference in dialog {dialog.dialogId}: choice target '{choice.targetDialogId}' not found");
                        }
                    }
                }
            }
            
            return isValid;
        }
        
#if UNITY_EDITOR
        [ContextMenu("Validate Collection")]
        private void ValidateInEditor()
        {
            if (ValidateCollection())
            {
                Debug.Log($"Dialog collection '{collectionName}' is valid!");
            }
            else
            {
                Debug.LogError($"Dialog collection '{collectionName}' has validation errors!");
            }
        }
        
        [ContextMenu("Generate Dialog IDs")]
        private void GenerateDialogIds()
        {
            for (int i = 0; i < dialogs.Count; i++)
            {
                var dialog = dialogs[i];
                if (string.IsNullOrEmpty(dialog.dialogId))
                {
                    dialog.dialogId = $"{collectionName.Replace(" ", "_").ToLower()}_dialog_{i:D3}";
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
            
            Debug.Log($"Generated dialog IDs for collection: {collectionName}");
        }
        
        [ContextMenu("Clean Up Empty Dialogs")]
        private void CleanUpEmptyDialogs()
        {
            int removedCount = 0;
            for (int i = dialogs.Count - 1; i >= 0; i--)
            {
                var dialog = dialogs[i];
                if (string.IsNullOrEmpty(dialog.dialogText) && string.IsNullOrEmpty(dialog.dialogId))
                {
                    dialogs.RemoveAt(i);
                    removedCount++;
                }
            }
            
            if (removedCount > 0)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"Removed {removedCount} empty dialogs from collection: {collectionName}");
            }
            else
            {
                Debug.Log($"No empty dialogs found in collection: {collectionName}");
            }
        }
#endif
    }
}