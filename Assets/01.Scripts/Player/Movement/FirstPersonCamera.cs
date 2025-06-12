using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Core._01.Scripts.Core.Input;
using UnityEngine;

namespace GrowAGarden.Player._01.Scripts.Player.Movement
{
    public class FirstPersonCamera : MonoBehaviour
    {
        [Header("Mouse Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private bool invertY = false;
        [SerializeField] private float smoothTime = 0.1f;
        
        [Header("Look Constraints")]
        [SerializeField] private float maxLookAngle = 90f;
        [SerializeField] private float minLookAngle = -90f;
        
        [Header("Camera Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float defaultFOV = 60f;
        [SerializeField] private float runFOVMultiplier = 1.1f;
        [SerializeField] private float fovTransitionSpeed = 2f;
        
        [Header("Camera Effects")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float headBobFrequency = 2f;
        [SerializeField] private float headBobAmplitude = 0.1f;
        [SerializeField] private float headBobSmoothness = 10f;
        
        [Header("Camera Shake")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private float shakeDamping = 1f;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        
        // Dependencies
        [Inject] private InputManager inputManager;
        
        // Camera rotation
        private float xRotation = 0f;
        private float yRotation = 0f;
        private Vector2 currentLookInput;
        private Vector2 lookVelocity;
        
        // FOV management
        private float targetFOV;
        private float currentFOV;
        
        // Head bobbing
        private float headBobTimer;
        private Vector3 originalCameraPosition;
        private bool wasMovingLastFrame;
        
        // Camera shake
        private Vector3 shakeOffset;
        private float shakeIntensity;
        private float shakeDuration;
        
        // Components
        private FirstPersonMovement movement;
        
        // Properties
        public Camera Camera => playerCamera;
        public float CurrentFOV => currentFOV;
        public bool InvertY => invertY;
        public float MouseSensitivity => mouseSensitivity;
        public Vector2 LookInput => currentLookInput;
        
        // Events
        public System.Action<float> OnFOVChanged;
        public System.Action<Vector2> OnLookInputChanged;

        private void Awake()
        {
            movement = GetComponentInParent<FirstPersonMovement>();
            
            if (playerCamera == null)
            {
                playerCamera = GetComponent<Camera>();
            }
            
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
            
            originalCameraPosition = transform.localPosition;
            targetFOV = defaultFOV;
            currentFOV = defaultFOV;
            
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = defaultFOV;
            }
        }

        private void Start()
        {
            if (inputManager == null)
            {
                inputManager = FindObjectOfType<InputManager>();
                if (inputManager == null && debugMode)
                {
                    Debug.LogWarning("InputManager not found! Mouse look will not work.");
                }
            }
            
            // Lock cursor
            SetCursorLocked(true);
        }

        private void Update()
        {
            HandleLookInput();
            UpdateFOV();
            UpdateHeadBob();
            UpdateCameraShake();
            ApplyCameraEffects();
        }

        private void HandleLookInput()
        {
            if (inputManager == null) return;
            
            Vector2 lookInput = inputManager.LookInput;
            
            // Apply sensitivity
            lookInput *= mouseSensitivity;
            
            // Invert Y if enabled
            if (invertY)
            {
                lookInput.y = -lookInput.y;
            }
            
            // Smooth the input
            currentLookInput = Vector2.SmoothDamp(currentLookInput, lookInput, ref lookVelocity, smoothTime);
            
            // Apply rotation
            yRotation += currentLookInput.x;
            xRotation -= currentLookInput.y;
            
            // Clamp vertical rotation
            xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
            
            // Apply rotations
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.parent.rotation = Quaternion.Euler(0f, yRotation, 0f);
            
            OnLookInputChanged?.Invoke(currentLookInput);
        }

        private void UpdateFOV()
        {
            if (playerCamera == null) return;
            
            // Determine target FOV based on movement state
            if (movement != null && movement.IsRunning)
            {
                targetFOV = defaultFOV * runFOVMultiplier;
            }
            else
            {
                targetFOV = defaultFOV;
            }
            
            // Smoothly transition FOV
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovTransitionSpeed * Time.deltaTime);
            playerCamera.fieldOfView = currentFOV;
            
            OnFOVChanged?.Invoke(currentFOV);
        }

        private void UpdateHeadBob()
        {
            if (!enableHeadBob || movement == null) return;
            
            bool isMoving = movement.IsMoving && movement.IsGrounded;
            
            if (isMoving)
            {
                headBobTimer += Time.deltaTime * headBobFrequency;
                
                // Reset timer when starting to move
                if (!wasMovingLastFrame)
                {
                    headBobTimer = 0f;
                }
            }
            else
            {
                // Gradually reduce head bob when not moving
                headBobTimer = Mathf.Lerp(headBobTimer, 0f, headBobSmoothness * Time.deltaTime);
            }
            
            wasMovingLastFrame = isMoving;
        }

        private void UpdateCameraShake()
        {
            if (!enableCameraShake) return;
            
            if (shakeDuration > 0f)
            {
                // Generate shake offset
                shakeOffset = Random.insideUnitSphere * shakeIntensity;
                
                // Reduce shake over time
                shakeDuration -= Time.deltaTime;
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, shakeDamping * Time.deltaTime);
            }
            else
            {
                // Smoothly return to center
                shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, shakeDamping * Time.deltaTime);
            }
        }

        private void ApplyCameraEffects()
        {
            if (playerCamera == null) return;
            
            Vector3 finalPosition = originalCameraPosition;
            
            // Apply head bob
            if (enableHeadBob)
            {
                float bobOffsetY = Mathf.Sin(headBobTimer) * headBobAmplitude;
                float bobOffsetX = Mathf.Cos(headBobTimer * 0.5f) * headBobAmplitude * 0.5f;
                
                finalPosition += new Vector3(bobOffsetX, bobOffsetY, 0f);
            }
            
            // Apply camera shake
            if (enableCameraShake)
            {
                finalPosition += shakeOffset;
            }
            
            transform.localPosition = finalPosition;
        }

        // Public methods
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = Mathf.Max(0.1f, sensitivity);
        }

        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }

        public void SetFOV(float fov)
        {
            defaultFOV = Mathf.Clamp(fov, 30f, 120f);
            targetFOV = defaultFOV;
        }

        public void AddCameraShake(float intensity, float duration)
        {
            if (!enableCameraShake) return;
            
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
            shakeDuration = Mathf.Max(shakeDuration, duration);
            
            if (debugMode)
            {
                Debug.Log($"Camera shake added: intensity={intensity}, duration={duration}");
            }
        }

        public void SetCursorLocked(bool locked)
        {
            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void SetHeadBobEnabled(bool enabled)
        {
            enableHeadBob = enabled;
            
            if (!enabled)
            {
                headBobTimer = 0f;
            }
        }

        public void SetCameraShakeEnabled(bool enabled)
        {
            enableCameraShake = enabled;
            
            if (!enabled)
            {
                shakeOffset = Vector3.zero;
                shakeDuration = 0f;
                shakeIntensity = 0f;
            }
        }

        // Look direction utilities
        public Vector3 GetLookDirection()
        {
            return transform.forward;
        }

        public Ray GetCameraRay()
        {
            if (playerCamera == null) return new Ray();
            
            return new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        }

        public Vector3 GetCameraPosition()
        {
            return playerCamera != null ? playerCamera.transform.position : transform.position;
        }

        // Reset methods
        public void ResetRotation()
        {
            xRotation = 0f;
            yRotation = 0f;
            transform.localRotation = Quaternion.identity;
            transform.parent.rotation = Quaternion.identity;
        }

        public void ResetEffects()
        {
            headBobTimer = 0f;
            shakeOffset = Vector3.zero;
            shakeDuration = 0f;
            shakeIntensity = 0f;
            transform.localPosition = originalCameraPosition;
        }

        // Debug methods
        public void DEBUG_AddRandomShake()
        {
            if (!debugMode) return;
            AddCameraShake(Random.Range(0.1f, 0.5f), Random.Range(0.1f, 1f));
        }

        public void DEBUG_ToggleHeadBob()
        {
            if (!debugMode) return;
            SetHeadBobEnabled(!enableHeadBob);
            Debug.Log($"Head bob: {enableHeadBob}");
        }

        public void DEBUG_SetFOV(float fov)
        {
            if (!debugMode) return;
            SetFOV(fov);
            Debug.Log($"FOV set to: {fov}");
        }
    }
}