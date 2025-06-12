using System;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GrowAGarden.Core._01.Scripts.Core.Input
{
    [Provide]
    public class InputManager : MonoBehaviour, IDependencyProvider, Controls.IPlayerActions
    {
        [Header("Input Settings")]
        [SerializeField] private bool enableInput = true;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private bool invertY = false;
        
        // Input System
        private Controls _controls;
        
        // Input State
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _runInput;
        private bool _jumpPressed;
        private bool _interactPressed;
        private bool _crouchInput;
        private bool _inventoryPressed;
        private bool _usePressed;
        
        // Properties
        public bool InputEnabled => enableInput;
        public Vector2 MoveInput => _moveInput;
        public Vector2 LookInput => _lookInput * mouseSensitivity * (invertY ? new Vector2(1, -1) : Vector2.one);
        public bool RunInput => _runInput;
        public bool JumpPressed => _jumpPressed;
        public bool InteractPressed => _interactPressed;
        public bool CrouchInput => _crouchInput;
        public bool InventoryPressed => _inventoryPressed;
        public bool UsePressed => _usePressed;
        
        // Events
        public event Action OnJumpPerformed;
        public event Action OnInteractPerformed;
        public event Action OnInventoryPerformed;
        public event Action OnUsePerformed;

        private void Awake()
        {
            _controls = new Controls();
            _controls.Player.AddCallbacks(this);
            
            // Reset input state
            ResetInputState();
        }

        private void OnEnable()
        {
            _controls?.Player.Enable();
        }

        private void OnDisable()
        {
            _controls?.Player.Disable();
        }

        private void LateUpdate()
        {
            // Reset one-frame inputs
            _jumpPressed = false;
            _interactPressed = false;
            _inventoryPressed = false;
            _usePressed = false;
        }
        
        [Provide]
        public InputManager ProvideInputManager() => this;

        private void ResetInputState()
        {
            _moveInput = Vector2.zero;
            _lookInput = Vector2.zero;
            _runInput = false;
            _jumpPressed = false;
            _interactPressed = false;
            _crouchInput = false;
            _inventoryPressed = false;
            _usePressed = false;
        }

        // Input System Callbacks
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!enableInput) return;
            _moveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!enableInput) return;
            _lookInput = context.ReadValue<Vector2>();
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            if (!enableInput) return;
            _runInput = context.ReadValueAsButton();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (!enableInput) return;
            if (context.performed)
            {
                _jumpPressed = true;
                OnJumpPerformed?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!enableInput) return;
            if (context.performed)
            {
                _interactPressed = true;
                OnInteractPerformed?.Invoke();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (!enableInput) return;
            _crouchInput = context.ReadValueAsButton();
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (!enableInput) return;
            if (context.performed)
            {
                _inventoryPressed = true;
                OnInventoryPerformed?.Invoke();
            }
        }

        public void OnUse(InputAction.CallbackContext context)
        {
            if (!enableInput) return;
            if (context.performed)
            {
                _usePressed = true;
                OnUsePerformed?.Invoke();
            }
        }

        // Public Methods
        public void SetInputEnabled(bool enabled)
        {
            enableInput = enabled;
            if (!enabled)
            {
                ResetInputState();
            }
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = Mathf.Max(0.1f, sensitivity);
        }

        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }

        private void OnDestroy()
        {
            _controls?.Player.RemoveCallbacks(this);
            _controls?.Dispose();
        }
    }
}