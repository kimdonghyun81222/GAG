using _01.Scripts.Player;

namespace _01.Scripts.Core
{
    public interface IInteractable
    {
        void Interact(PlayerController player);
        string GetInteractionPrompt();
    }
}