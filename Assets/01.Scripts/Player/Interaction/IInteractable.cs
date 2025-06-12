using UnityEngine;

namespace GrowAGarden.Player._01.Scripts.Player.Interaction
{
    public interface IInteractable
    {
        // Basic interaction
        bool CanInteract();
        bool Interact();
        void CancelInteraction();
        
        // Interaction text
        string GetInteractionText();
        
        // Look events
        void OnLookEnter();
        void OnLookExit();
        
        // State
        bool IsBeingInteracted { get; }
        
        // Position
        Vector3 GetPosition();
        
        // Optional: Interaction range override
        float GetInteractionRange() => 3f;
        
        // Optional: Can this be interacted with by specific player
        bool CanInteractWith(GameObject player) => true;
    }
}