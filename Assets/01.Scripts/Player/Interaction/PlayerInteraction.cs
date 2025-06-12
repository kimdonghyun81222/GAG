using System.Collections.Generic;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Core._01.Scripts.Core.Input;
using UnityEngine;

namespace GrowAGarden.Player._01.Scripts.Player.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactionLayerMask = -1;
        [SerializeField] private float interactionCheckRate = 10f; // Checks per second
        
        [Header("Raycast Settings")]
        [SerializeField] private Transform raycastOrigin;
        [SerializeField] private bool useSphereCast = false;
        [SerializeField] private float sphereCastRadius = 0.5f;
        
        [Header("UI")]
        [SerializeField] private bool showInteractionPrompt = true;
        [SerializeField] private string interactionPromptText = "Press E to interact";
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool showDebugRays = false;
        
        // Dependencies
        [Inject] private InputManager inputManager;
        
        // Components
        private Camera playerCamera;
        
        // Interaction state
        private IInteractable currentInteractable;
        private IInteractable lastInteractable;
        private List<IInteractable> nearbyInteractables = new List<IInteractable>();
        private float lastInteractionCheckTime;
        
        // Properties
        public IInteractable CurrentInteractable => currentInteractable;
        public bool HasInteractable => currentInteractable != null;
        public float InteractionRange => interactionRange;
        public bool IsInteracting => currentInteractable != null && currentInteractable.IsBeingInteracted;
        
        // Events
        public System.Action<IInteractable> OnInteractableFound;
        public System.Action<IInteractable> OnInteractableLost;
        public System.Action<IInteractable> OnInteractionStarted;
        public System.Action<IInteractable> OnInteractionCompleted;
        public System.Action<IInteractable> OnInteractionCancelled;

        private void Awake()
        {
            if (raycastOrigin == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                raycastOrigin = playerCamera != null ? playerCamera.transform : transform;
            }
        }

        private void Start()
        {
            if (inputManager == null)
            {
                inputManager = FindObjectOfType<InputManager>();
                if (inputManager == null && debugMode)
                {
                    Debug.LogWarning("InputManager not found! Interaction will not work.");
                }
            }
        }

        private void Update()
        {
            CheckForInteractables();
            HandleInput();
        }

        private void CheckForInteractables()
        {
            // Limit interaction checks to improve performance
            if (Time.time - lastInteractionCheckTime < 1f / interactionCheckRate)
                return;
            
            lastInteractionCheckTime = Time.time;
            
            IInteractable foundInteractable = FindInteractable();
            
            // Handle interactable changes
            if (foundInteractable != currentInteractable)
            {
                // Lost previous interactable
                if (currentInteractable != null)
                {
                    currentInteractable.OnLookExit();
                    OnInteractableLost?.Invoke(currentInteractable);
                    
                    if (debugMode)
                    {
                        Debug.Log($"Lost interactable: {currentInteractable.GetInteractionText()}");
                    }
                }
                
                // Found new interactable
                if (foundInteractable != null)
                {
                    foundInteractable.OnLookEnter();
                    OnInteractableFound?.Invoke(foundInteractable);
                    
                    if (debugMode)
                    {
                        Debug.Log($"Found interactable: {foundInteractable.GetInteractionText()}");
                    }
                }
                
                lastInteractable = currentInteractable;
                currentInteractable = foundInteractable;
            }
        }

        private IInteractable FindInteractable()
        {
            if (raycastOrigin == null) return null;
            
            Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
            RaycastHit hit;
            bool hitSomething = false;
            
            if (useSphereCast)
            {
                hitSomething = Physics.SphereCast(ray, sphereCastRadius, out hit, interactionRange, interactionLayerMask);
            }
            else
            {
                hitSomething = Physics.Raycast(ray, out hit, interactionRange, interactionLayerMask);
            }
            
            if (showDebugRays)
            {
                Color rayColor = hitSomething ? Color.green : Color.red;
                Debug.DrawRay(ray.origin, ray.direction * interactionRange, rayColor);
            }
            
            if (hitSomething)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                
                if (interactable != null && interactable.CanInteract())
                {
                    return interactable;
                }
                
                // Check parent objects for interactables
                Transform parent = hit.collider.transform.parent;
                while (parent != null)
                {
                    interactable = parent.GetComponent<IInteractable>();
                    if (interactable != null && interactable.CanInteract())
                    {
                        return interactable;
                    }
                    parent = parent.parent;
                }
            }
            
            return null;
        }

        private void HandleInput()
        {
            // Manual input check as fallback
            if (inputManager != null && inputManager.InteractPressed)
            {
                HandleInteractionInput();
            }
        }

        private void HandleInteractionInput()
        {
            if (currentInteractable == null || !currentInteractable.CanInteract())
                return;
            
            StartInteraction(currentInteractable);
        }

        // Public interaction methods
        public void StartInteraction(IInteractable interactable)
        {
            if (interactable == null || !interactable.CanInteract())
            {
                if (debugMode)
                {
                    Debug.LogWarning("Cannot start interaction - interactable is null or cannot be interacted with");
                }
                return;
            }
            
            OnInteractionStarted?.Invoke(interactable);
            
            bool interactionSuccess = interactable.Interact();
            
            if (interactionSuccess)
            {
                OnInteractionCompleted?.Invoke(interactable);
                
                if (debugMode)
                {
                    Debug.Log($"Interaction completed: {interactable.GetInteractionText()}");
                }
            }
            else
            {
                OnInteractionCancelled?.Invoke(interactable);
                
                if (debugMode)
                {
                    Debug.Log($"Interaction cancelled: {interactable.GetInteractionText()}");
                }
            }
        }

        public void CancelCurrentInteraction()
        {
            if (currentInteractable != null && currentInteractable.IsBeingInteracted)
            {
                currentInteractable.CancelInteraction();
                OnInteractionCancelled?.Invoke(currentInteractable);
                
                if (debugMode)
                {
                    Debug.Log($"Cancelled interaction: {currentInteractable.GetInteractionText()}");
                }
            }
        }

        // Rest of the methods remain the same...
        public void RegisterNearbyInteractable(IInteractable interactable)
        {
            if (interactable != null && !nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Add(interactable);
            }
        }

        public void UnregisterNearbyInteractable(IInteractable interactable)
        {
            nearbyInteractables.Remove(interactable);
        }

        public List<IInteractable> GetNearbyInteractables()
        {
            return new List<IInteractable>(nearbyInteractables);
        }

        public IInteractable GetClosestInteractable()
        {
            if (nearbyInteractables.Count == 0) return null;
            
            IInteractable closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (var interactable in nearbyInteractables)
            {
                if (interactable == null || !interactable.CanInteract()) continue;
                
                float distance = Vector3.Distance(transform.position, interactable.GetPosition());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }
            
            return closest;
        }

        // Settings methods remain the same...
        public void SetInteractionRange(float range)
        {
            interactionRange = Mathf.Max(0.1f, range);
        }

        public string GetCurrentInteractionText()
        {
            if (currentInteractable == null) return "";
            
            return showInteractionPrompt ? 
                $"{interactionPromptText}: {currentInteractable.GetInteractionText()}" : 
                currentInteractable.GetInteractionText();
        }

        public bool ShouldShowInteractionUI()
        {
            return showInteractionPrompt && currentInteractable != null && currentInteractable.CanInteract();
        }

        private void OnDrawGizmosSelected()
        {
            if (raycastOrigin == null) return;
            
            Gizmos.color = HasInteractable ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(raycastOrigin.position, interactionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactionRange);
            
            if (useSphereCast)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(raycastOrigin.position + raycastOrigin.forward * interactionRange, sphereCastRadius);
            }
        }
    }
}