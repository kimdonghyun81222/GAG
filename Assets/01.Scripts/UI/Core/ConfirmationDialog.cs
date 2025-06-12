using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrowAGarden.UI._01.Scripts.UI.Core
{
    public class ConfirmationDialog : UIPanel
    {
        [Header("Confirmation Dialog")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;
        
        [Header("Default Texts")]
        [SerializeField] private string defaultConfirmText = "Confirm";
        [SerializeField] private string defaultCancelText = "Cancel";
        [SerializeField] private string defaultMessage = "Are you sure?";
        
        // Current confirmation data
        private Action _onConfirm;
        private Action _onCancel;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Auto-find components if not assigned
            if (messageText == null)
                messageText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (confirmButton == null)
            {
                var buttons = GetComponentsInChildren<Button>();
                if (buttons.Length > 0) confirmButton = buttons[0];
                if (buttons.Length > 1) cancelButton = buttons[1];
            }
            
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmClicked);
                
                if (confirmButtonText == null)
                    confirmButtonText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelClicked);
                
                if (cancelButtonText == null)
                    cancelButtonText = cancelButton.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Set default texts
            if (confirmButtonText != null)
                confirmButtonText.text = defaultConfirmText;
            
            if (cancelButtonText != null)
                cancelButtonText.text = defaultCancelText;
            
            if (messageText != null)
                messageText.text = defaultMessage;
        }

        // Show confirmation with callbacks
        public void ShowConfirmation(string message, Action onConfirm, Action onCancel = null)
        {
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            
            if (messageText != null)
                messageText.text = message;
            
            Open();
        }

        public void ShowConfirmation(string message, string confirmText, string cancelText, Action onConfirm, Action onCancel = null)
        {
            if (confirmButtonText != null)
                confirmButtonText.text = confirmText;
            
            if (cancelButtonText != null)
                cancelButtonText.text = cancelText;
            
            ShowConfirmation(message, onConfirm, onCancel);
        }

        // Button handlers
        private void OnConfirmClicked()
        {
            _onConfirm?.Invoke();
            Close();
            ClearCallbacks();
        }

        private void OnCancelClicked()
        {
            _onCancel?.Invoke();
            Close();
            ClearCallbacks();
        }

        protected override void OnClose()
        {
            base.OnClose();
            ClearCallbacks();
        }

        private void ClearCallbacks()
        {
            _onConfirm = null;
            _onCancel = null;
        }

        // Utility methods for common confirmations
        public void ShowDeleteConfirmation(string itemName, Action onConfirm)
        {
            ShowConfirmation(
                $"Are you sure you want to delete '{itemName}'? This action cannot be undone.",
                "Delete",
                "Cancel",
                onConfirm
            );
        }

        public void ShowExitConfirmation(Action onConfirm)
        {
            ShowConfirmation(
                "Are you sure you want to exit? Any unsaved progress will be lost.",
                "Exit",
                "Cancel",
                onConfirm
            );
        }

        public void ShowResetConfirmation(Action onConfirm)
        {
            ShowConfirmation(
                "Are you sure you want to reset? All progress will be lost.",
                "Reset",
                "Cancel",
                onConfirm
            );
        }

        public void ShowOverwriteConfirmation(string fileName, Action onConfirm)
        {
            ShowConfirmation(
                $"'{fileName}' already exists. Do you want to overwrite it?",
                "Overwrite",
                "Cancel",
                onConfirm
            );
        }

        // Static convenience methods
        public static void Show(string message, Action onConfirm, Action onCancel = null)
        {
            var dialog = FindPanel<ConfirmationDialog>("ConfirmationDialog");
            dialog?.ShowConfirmation(message, onConfirm, onCancel);
        }

        public static void ShowDelete(string itemName, Action onConfirm)
        {
            var dialog = FindPanel<ConfirmationDialog>("ConfirmationDialog");
            dialog?.ShowDeleteConfirmation(itemName, onConfirm);
        }

        public static void ShowExit(Action onConfirm)
        {
            var dialog = FindPanel<ConfirmationDialog>("ConfirmationDialog");
            dialog?.ShowExitConfirmation(onConfirm);
        }
    }
}