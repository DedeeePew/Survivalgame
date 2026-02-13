using UnityEngine;
using SurvivalGame.Core;

namespace SurvivalGame.Interaction
{
    /// <summary>
    /// Raycast-based interaction system.
    /// Attach to the Player GameObject. Uses the main camera for raycasting.
    /// Fires events when interactables are found/lost, and handles E-key interaction.
    /// </summary>
    public class InteractSystem : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float _interactRange = 3f;
        [SerializeField] private LayerMask _interactMask = ~0;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;

        private Camera _cam;
        private IInteractable _currentTarget;
        private GameObject _currentTargetGO;

        // Public accessors for DebugUI
        public IInteractable CurrentTarget => _currentTarget;
        public float InteractRange => _interactRange;

        private void Awake()
        {
            _cam = GetComponentInChildren<Camera>();
            if (_cam == null)
                _cam = Camera.main;

            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<InteractSystem>();
        }

        private void Update()
        {
            PerformRaycast();
            HandleInput();
        }

        private void PerformRaycast()
        {
            if (_cam == null) return;
            Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, _interactRange, _interactMask))
            {
                // Check the hit object and its parents for IInteractable
                var interactable = hit.collider.GetComponentInParent<IInteractable>();

                if (interactable != null && interactable.CanInteract(gameObject))
                {
                    if (interactable != _currentTarget)
                    {
                        // New target found
                        _currentTarget = interactable;
                        _currentTargetGO = hit.collider.gameObject;
                        GameEvents.RaiseInteractableFound(_currentTargetGO);
                    }
                    return;
                }
            }

            // Nothing found or not interactable
            if (_currentTarget != null)
            {
                _currentTarget = null;
                _currentTargetGO = null;
                GameEvents.RaiseInteractableLost();
            }
        }

        private void HandleInput()
        {
            if (_currentTarget == null) return;

            if (Input.GetKeyDown(_interactKey))
            {
                _currentTarget.Interact(gameObject);
                GameEvents.RaiseInteract(_currentTargetGO);
                GameEvents.RaiseDebugMessage($"Interacted with: {_currentTarget.InteractionPrompt}");
            }
        }
    }
}
