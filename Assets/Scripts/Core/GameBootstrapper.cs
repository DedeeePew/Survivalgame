using UnityEngine;

namespace SurvivalGame.Core
{
    /// <summary>
    /// Bootstrapper that runs before anything else.
    /// Registers core services and sets up the game.
    /// Attach to an empty GameObject "GameManager" in the scene.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _lockCursor = true;

        private void Awake()
        {
            // Ensure only one instance
            if (FindObjectsByType<GameBootstrapper>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            Debug.Log("[GameBootstrapper] Initializing game systems...");

            // Lock cursor for FPS controls
            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Debug.Log("[GameBootstrapper] Game systems ready.");
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.Clear();
        }

        private void Update()
        {
            // Toggle cursor lock with Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool isLocked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = isLocked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = isLocked;
            }
        }
    }
}
