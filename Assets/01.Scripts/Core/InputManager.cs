using UnityEngine;

namespace _01.Scripts.Core
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        private Controls _playerInputActions;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool InteractTriggered { get; private set; }
        public bool OpenInventoryTriggered { get; private set; }
        public bool PauseTriggered { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _playerInputActions = new Controls();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            _playerInputActions.Gameplay.Enable();
            _playerInputActions.Gameplay.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
            _playerInputActions.Gameplay.Move.canceled += ctx => MoveInput = Vector2.zero;
            _playerInputActions.Gameplay.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
            _playerInputActions.Gameplay.Look.canceled += ctx => LookInput = Vector2.zero;
            _playerInputActions.Gameplay.Jump.performed += ctx => JumpTriggered = true;
            _playerInputActions.Gameplay.Interact.performed += ctx => InteractTriggered = true;
            _playerInputActions.Gameplay.OpenInventory.performed += ctx => OpenInventoryTriggered = true;
            _playerInputActions.Gameplay.Pause.performed += ctx => PauseTriggered = true;
        }

        private void OnDisable()
        {
            if (_playerInputActions != null)
            {
                _playerInputActions.Gameplay.Disable();
                // It's good practice to unsubscribe, especially if the object could be enabled/disabled multiple times.
                _playerInputActions.Gameplay.Move.performed -= ctx => MoveInput = ctx.ReadValue<Vector2>();
                _playerInputActions.Gameplay.Move.canceled -= ctx => MoveInput = Vector2.zero;
                _playerInputActions.Gameplay.Look.performed -= ctx => LookInput = ctx.ReadValue<Vector2>();
                _playerInputActions.Gameplay.Look.canceled -= ctx => LookInput = Vector2.zero;
                _playerInputActions.Gameplay.Jump.performed -= ctx => JumpTriggered = true;
                _playerInputActions.Gameplay.Interact.performed -= ctx => InteractTriggered = true;
                _playerInputActions.Gameplay.OpenInventory.performed -= ctx => OpenInventoryTriggered = true;
                _playerInputActions.Gameplay.Pause.performed -= ctx => PauseTriggered = true;
            }
        }

        private void LateUpdate() // Reset one-frame triggers
        {
            JumpTriggered = false;
            InteractTriggered = false;
            OpenInventoryTriggered = false;
            PauseTriggered = false;
        }

        public void EnableGameplayInput() => _playerInputActions.Gameplay.Enable();
        public void DisableGameplayInput() => _playerInputActions.Gameplay.Disable();
    }
}