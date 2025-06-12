using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Core._01.Scripts.Core.Input;
using GrowAGarden.Player._01.Scripts.Player.Stats;
using UnityEngine;

namespace GrowAGarden.Player._01.Scripts.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2.5f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -20f;
        
        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask = 1;
        
        [Header("Movement Modifiers")]
        [SerializeField] private float airControlMultiplier = 0.5f;
        [SerializeField] private float accelerationTime = 0.1f;
        [SerializeField] private float decelerationTime = 0.1f;
        
        [Header("Stamina")]
        [SerializeField] private bool useStamina = true;
        [SerializeField] private float staminaDrainRate = 10f;
        [SerializeField] private float staminaRegenRate = 15f;
        [SerializeField] private float maxStamina = 100f;
        
        [Header("Audio")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioClip[] walkFootsteps;
        [SerializeField] private AudioClip[] runFootsteps;
        [SerializeField] private float footstepInterval = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        // Dependencies
        [Inject] private InputManager inputManager;
        
        // Components
        private CharacterController characterController;
        private PlayerStats playerStats;
        
        // Movement state
        private Vector3 velocity;
        private Vector3 moveDirection;
        private Vector3 smoothMoveVelocity;
        private bool isGrounded;
        private bool isRunning;
        private bool isCrouching;
        private bool isJumping;
        
        // Stamina
        private float currentStamina;
        
        // Audio
        private float lastFootstepTime;
        
        // Properties
        public bool IsGrounded => isGrounded;
        public bool IsMoving => moveDirection.magnitude > 0.1f;
        public bool IsRunning => isRunning && IsMoving;
        public bool IsCrouching => isCrouching;
        public bool IsJumping => isJumping;
        public float CurrentSpeed => characterController.velocity.magnitude;
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public Vector3 MoveDirection => moveDirection;
        public Vector3 Velocity => velocity;
        
        // Events
        public System.Action OnJumped;
        public System.Action OnLanded;
        public System.Action OnStartedRunning;
        public System.Action OnStoppedRunning;
        public System.Action OnStartedCrouching;
        public System.Action OnStoppedCrouching;
        public System.Action OnStaminaDepleted;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerStats = GetComponent<PlayerStats>();
            
            if (groundCheck == null)
            {
                // Create ground check if not assigned
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.SetParent(transform);
                groundCheckObj.transform.localPosition = new Vector3(0, -characterController.height * 0.5f - 0.1f, 0);
                groundCheck = groundCheckObj.transform;
            }
            
            currentStamina = maxStamina;
        }

        private void Start()
        {
            if (inputManager == null)
            {
                inputManager = FindObjectOfType<InputManager>();
                if (inputManager == null && debugMode)
                {
                    Debug.LogWarning("InputManager not found! Creating fallback input handling.");
                }
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateGroundCheck();
            UpdateMovement();
            UpdateStamina();
            UpdateAudio();
        }

        private void HandleInput()
        {
            if (inputManager == null) return;
            
            // Get movement input
            Vector2 moveInput = inputManager.MoveInput;
            
            // Calculate move direction relative to transform
            Vector3 inputDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            
            // Smooth the movement
            moveDirection = Vector3.SmoothDamp(moveDirection, inputDirection, ref smoothMoveVelocity, 
                inputDirection.magnitude > 0 ? accelerationTime : decelerationTime);
            
            // Handle running
            bool wantsToRun = inputManager.RunInput && moveDirection.magnitude > 0.1f;
            UpdateRunningState(wantsToRun);
            
            // Handle crouching
            bool wantsToCrouch = inputManager.CrouchInput;
            UpdateCrouchingState(wantsToCrouch);
            
            // Handle jumping
            if (inputManager.JumpPressed && CanJump())
            {
                Jump();
            }
        }

        private void UpdateGroundCheck()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            
            // Landing detection
            if (!wasGrounded && isGrounded)
            {
                isJumping = false;
                OnLanded?.Invoke();
                
                if (debugMode)
                {
                    Debug.Log("Player landed");
                }
            }
        }

        private void UpdateMovement()
        {
            // Calculate movement speed
            float targetSpeed = CalculateTargetSpeed();
            
            // Apply movement
            Vector3 move = moveDirection * targetSpeed;
            
            // Apply air control when not grounded
            if (!isGrounded)
            {
                move *= airControlMultiplier;
            }
            
            characterController.Move(move * Time.deltaTime);
            
            // Apply gravity
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Keep player grounded
            }
            
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        private float CalculateTargetSpeed()
        {
            if (isCrouching)
                return crouchSpeed;
            
            if (isRunning && HasStaminaForRunning())
                return runSpeed;
            
            return walkSpeed;
        }

        private void UpdateRunningState(bool wantsToRun)
        {
            bool wasRunning = isRunning;
            
            // Can only run if we have stamina and are moving
            isRunning = wantsToRun && HasStaminaForRunning() && !isCrouching;
            
            // Fire events
            if (!wasRunning && isRunning)
            {
                OnStartedRunning?.Invoke();
            }
            else if (wasRunning && !isRunning)
            {
                OnStoppedRunning?.Invoke();
            }
        }

        private void UpdateCrouchingState(bool wantsToCrouch)
        {
            bool wasCrouching = isCrouching;
            isCrouching = wantsToCrouch;
            
            // Adjust character controller height
            if (isCrouching && !wasCrouching)
            {
                characterController.height *= 0.5f;
                characterController.center = new Vector3(0, characterController.height * 0.5f, 0);
                OnStartedCrouching?.Invoke();
            }
            else if (!isCrouching && wasCrouching)
            {
                // Check if we have room to stand up
                if (CanStandUp())
                {
                    characterController.height *= 2f;
                    characterController.center = new Vector3(0, characterController.height * 0.5f, 0);
                    OnStoppedCrouching?.Invoke();
                }
                else
                {
                    isCrouching = true; // Stay crouched
                }
            }
        }

        private bool CanJump()
        {
            return isGrounded && !isCrouching;
        }

        private void Jump()
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            isJumping = true;
            OnJumped?.Invoke();
            
            if (debugMode)
            {
                Debug.Log($"Player jumped with velocity: {velocity.y}");
            }
        }

        private bool CanStandUp()
        {
            // Check if there's room above the player to stand up
            Vector3 capsuleTop = transform.position + Vector3.up * (characterController.height * 2f);
            return !Physics.CheckSphere(capsuleTop, characterController.radius, groundMask);
        }

        private void UpdateStamina()
        {
            if (!useStamina) return;
            
            float staminaChange = 0f;
            
            if (isRunning && IsMoving)
            {
                // Drain stamina while running
                staminaChange = -staminaDrainRate * Time.deltaTime;
            }
            else
            {
                // Regenerate stamina
                staminaChange = staminaRegenRate * Time.deltaTime;
            }
            
            float oldStamina = currentStamina;
            currentStamina = Mathf.Clamp(currentStamina + staminaChange, 0f, maxStamina);
            
            // Check for stamina depletion
            if (oldStamina > 0f && currentStamina <= 0f)
            {
                OnStaminaDepleted?.Invoke();
                
                if (debugMode)
                {
                    Debug.Log("Stamina depleted!");
                }
            }
        }

        private bool HasStaminaForRunning()
        {
            return !useStamina || currentStamina > 0f;
        }

        private void UpdateAudio()
        {
            if (!IsMoving || footstepAudioSource == null) return;
            
            float currentInterval = isRunning ? footstepInterval * 0.7f : footstepInterval;
            
            if (Time.time - lastFootstepTime >= currentInterval)
            {
                PlayFootstepSound();
                lastFootstepTime = Time.time;
            }
        }

        private void PlayFootstepSound()
        {
            if (footstepAudioSource == null) return;
            
            AudioClip[] footsteps = isRunning ? runFootsteps : walkFootsteps;
            
            if (footsteps != null && footsteps.Length > 0)
            {
                AudioClip clip = footsteps[Random.Range(0, footsteps.Length)];
                if (clip != null)
                {
                    footstepAudioSource.PlayOneShot(clip);
                }
            }
        }

        // Public methods
        public void SetMovementEnabled(bool enabled)
        {
            this.enabled = enabled;
            
            if (!enabled)
            {
                moveDirection = Vector3.zero;
                velocity.x = 0f;
                velocity.z = 0f;
            }
        }

        public void AddForce(Vector3 force)
        {
            velocity += force;
        }

        public void SetPosition(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }

        public void Teleport(Vector3 position)
        {
            SetPosition(position);
            velocity = Vector3.zero;
            moveDirection = Vector3.zero;
        }

        // Stamina management
        public void AddStamina(float amount)
        {
            currentStamina = Mathf.Clamp(currentStamina + amount, 0f, maxStamina);
        }

        public void DrainStamina(float amount)
        {
            currentStamina = Mathf.Clamp(currentStamina - amount, 0f, maxStamina);
        }

        public void SetMaxStamina(float newMaxStamina)
        {
            float ratio = currentStamina / maxStamina;
            maxStamina = Mathf.Max(1f, newMaxStamina);
            currentStamina = maxStamina * ratio;
        }

        // Speed modifiers
        public void SetSpeedMultiplier(float multiplier)
        {
            walkSpeed *= multiplier;
            runSpeed *= multiplier;
            crouchSpeed *= multiplier;
        }

        public void ResetSpeeds()
        {
            // This would need to store original values
            // For now, just ensure minimum values
            walkSpeed = Mathf.Max(1f, walkSpeed);
            runSpeed = Mathf.Max(walkSpeed, runSpeed);
            crouchSpeed = Mathf.Max(0.5f, crouchSpeed);
        }

        // Debug methods
        public void DEBUG_ToggleNoClip()
        {
            if (!debugMode) return;
            
            // Toggle character controller for no-clip mode
            characterController.enabled = !characterController.enabled;
            Debug.Log($"No-clip mode: {!characterController.enabled}");
        }

        public void DEBUG_AddStamina(float amount)
        {
            if (!debugMode) return;
            AddStamina(amount);
        }

        public void DEBUG_SetGravity(float newGravity)
        {
            if (!debugMode) return;
            gravity = newGravity;
        }

        // Gizmos
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
            
            // Draw movement direction
            if (Application.isPlaying && moveDirection.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, moveDirection.normalized * 2f);
            }
        }
    }
}