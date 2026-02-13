using UnityEngine;

namespace SurvivalGame.Core
{
    /// <summary>
    /// Implement this on any GameObject that the player can interact with.
    /// The InteractSystem will detect it via Raycast and call Interact().
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Display name shown in the HUD prompt, e.g. "Open Chest", "Pick up Stone"</summary>
        string InteractionPrompt { get; }

        /// <summary>Can the player currently interact? (e.g. chest already looted → false)</summary>
        bool CanInteract(GameObject interactor);

        /// <summary>Execute the interaction logic.</summary>
        void Interact(GameObject interactor);
    }
}
