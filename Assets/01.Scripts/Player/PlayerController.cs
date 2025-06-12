using _01.Scripts.Core;
using TMPro;
using UnityEngine;

namespace _01.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float jumpHeight = 1.8f;
        [SerializeField] private float gravityValue = -19.62f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.2f;
        [SerializeField] private LayerMask groundMask;

        [Header("Interaction")]
        [SerializeField] private float interactionDistance = 2.5f;
        [SerializeField] private LayerMask interactionLayer;
        public TextMeshProUGUI interactionPromptText_TMP; // TextMeshPro UI 요소

        private CharacterController _characterController;
        private Vector3 _playerVelocity;
        private bool _isGrounded;
        private InputManager _inputManager;
        private Transform _cameraForRaycast;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            _inputManager = InputManager.Instance;
            if (_inputManager == null) Debug.LogError("InputManager not found!");

            if (Camera.main != null)
            {
                _cameraForRaycast = Camera.main.transform;
            }
            else
            {
                Debug.LogError("Main Camera not found or not tagged 'MainCamera'! Interaction and movement might not work as expected.");
                if (_cameraForRaycast == null) _cameraForRaycast = transform;
            }

            if (interactionPromptText_TMP != null)
            {
                interactionPromptText_TMP.gameObject.SetActive(false); // 시작 시 프롬프트 숨김
            }
        }

        private void Update()
        {
            if (_inputManager == null || _characterController == null || groundCheck == null || _cameraForRaycast == null)
            {
                if(enabled)
                    DebugExtensions.LogWarningOnce("PlayerController is missing one or more critical references. Update loop will not run.", this);
                return;
            }

            HandleGroundCheck();
            HandleMovement();
            HandleJump();
            ApplyGravity();
            HandleInteraction();
        }

        private void HandleGroundCheck()
        {
            _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        private void HandleMovement()
        {
            Vector2 moveInput = _inputManager.MoveInput;
            Vector3 forward = _cameraForRaycast.forward;
            Vector3 right = _cameraForRaycast.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 desiredMoveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            _characterController.Move(desiredMoveDirection * moveSpeed * Time.deltaTime);
        }

        private void HandleJump()
        {
            if (_isGrounded && _playerVelocity.y < 0)
            {
                _playerVelocity.y = -2f;
            }

            if (_inputManager.JumpTriggered && _isGrounded)
            {
                _playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            }
        }

        private void ApplyGravity()
        {
            _playerVelocity.y += gravityValue * Time.deltaTime;
            _characterController.Move(_playerVelocity * Time.deltaTime);
        }

        private void HandleInteraction()
        {
            RaycastHit hit;
            IInteractable interactable = null;

            if (Physics.Raycast(_cameraForRaycast.position, _cameraForRaycast.forward, out hit, interactionDistance, interactionLayer))
            {
                interactable = hit.collider.GetComponent<IInteractable>();
            }

            if (interactable != null)
            {
                if (interactionPromptText_TMP != null)
                {
                    interactionPromptText_TMP.text = interactable.GetInteractionPrompt();
                    interactionPromptText_TMP.gameObject.SetActive(true);
                }

                if (_inputManager.InteractTriggered)
                {
                    interactable.Interact(this);
                }
            }
            else
            {
                if (interactionPromptText_TMP != null)
                {
                    interactionPromptText_TMP.gameObject.SetActive(false);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);

            if (_cameraForRaycast != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(_cameraForRaycast.position, _cameraForRaycast.forward * interactionDistance);
            }
        }
    }

// Helper for logging only once (can be in a separate file)
    public static class DebugExtensions
    {
        private static readonly System.Collections.Generic.HashSet<string> loggedMessages = new System.Collections.Generic.HashSet<string>();

        public static void LogWarningOnce(string message, Object context = null)
        {
            string key = message + (context != null ? context.GetInstanceID().ToString() : "");
            if (!loggedMessages.Contains(key))
            {
                Debug.LogWarning(message, context);
                loggedMessages.Add(key);
            }
        }
    }
}