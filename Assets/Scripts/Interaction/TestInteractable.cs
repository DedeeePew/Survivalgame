using UnityEngine;
using SurvivalGame.Core;

namespace SurvivalGame.Interaction
{
    /// <summary>
    /// Simple test interactable for M1 validation.
    /// Place on any collider in the scene. Logs a message on interact.
    /// Will be replaced by real interactables (chests, nodes, etc.) later.
    /// </summary>
    public class TestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _prompt = "Test Object";
        [SerializeField] private Color _highlightColor = Color.yellow;

        private Renderer _renderer;
        private Color _originalColor;
        private int _interactCount;

        public string InteractionPrompt => $"{_prompt} [E]";

        public bool CanInteract(GameObject interactor)
        {
            return true; // Always interactable for testing
        }

        public void Interact(GameObject interactor)
        {
            _interactCount++;
            Debug.Log($"[TestInteractable] '{_prompt}' interacted! Count: {_interactCount}");

            // Visual feedback: flash color
            if (_renderer != null)
            {
                _renderer.material.color = _highlightColor;
                Invoke(nameof(ResetColor), 0.3f);
            }
        }

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                _originalColor = _renderer.material.color;
            }
        }

        private void ResetColor()
        {
            if (_renderer != null)
            {
                _renderer.material.color = _originalColor;
            }
        }
    }
}
