using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Player;
using SurvivalGame.Interaction;

namespace SurvivalGame.UI
{
    /// <summary>
    /// IMGUI-based debug overlay. Shows FPS, player position, speed,
    /// current interact target, and a scrolling debug log.
    /// Toggle with F1 key.
    /// </summary>
    public class DebugUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _showOnStart = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;

        private bool _visible;
        private PlayerController _player;
        private InteractSystem _interactSystem;

        // FPS calculation
        private float _fpsTimer;
        private int _fpsFrameCount;
        private float _currentFPS;

        // Debug log
        private string _lastDebugMessage = "";
        private float _debugMessageTimer;

        // Interact prompt
        private string _currentPrompt = "";

        private void Awake()
        {
            _visible = _showOnStart;
        }

        private void OnEnable()
        {
            GameEvents.OnDebugMessage += HandleDebugMessage;
            GameEvents.OnInteractableFound += HandleInteractableFound;
            GameEvents.OnInteractableLost += HandleInteractableLost;
        }

        private void OnDisable()
        {
            GameEvents.OnDebugMessage -= HandleDebugMessage;
            GameEvents.OnInteractableFound -= HandleInteractableFound;
            GameEvents.OnInteractableLost -= HandleInteractableLost;
        }

        private void Start()
        {
            // Cache references
            _player = FindFirstObjectByType<PlayerController>();
            _interactSystem = FindFirstObjectByType<InteractSystem>();
        }

        private void Update()
        {
            // Toggle visibility
            if (Input.GetKeyDown(_toggleKey))
            {
                _visible = !_visible;
            }

            // FPS counter
            _fpsTimer += Time.unscaledDeltaTime;
            _fpsFrameCount++;
            if (_fpsTimer >= 0.5f)
            {
                _currentFPS = _fpsFrameCount / _fpsTimer;
                _fpsFrameCount = 0;
                _fpsTimer = 0;
            }

            // Fade out debug message
            if (_debugMessageTimer > 0)
            {
                _debugMessageTimer -= Time.deltaTime;
                if (_debugMessageTimer <= 0)
                    _lastDebugMessage = "";
            }
        }

        private void OnGUI()
        {
            // Always show interaction prompt (center screen)
            DrawInteractionPrompt();

            // Always show crosshair
            DrawCrosshair();

            if (!_visible) return;

            // Debug panel (top-left)
            DrawDebugPanel();

            // Debug messages (bottom-left)
            if (!string.IsNullOrEmpty(_lastDebugMessage))
            {
                DrawDebugMessage();
            }
        }

        private void DrawCrosshair()
        {
            float size = 6f;
            float thickness = 2f;
            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;

            Color crosshairColor = _interactSystem != null && _interactSystem.CurrentTarget != null
                ? Color.green
                : Color.white;

            GUI.color = crosshairColor;

            // Horizontal line
            GUI.DrawTexture(new Rect(cx - size, cy - thickness / 2, size * 2, thickness), Texture2D.whiteTexture);
            // Vertical line
            GUI.DrawTexture(new Rect(cx - thickness / 2, cy - size, thickness, size * 2), Texture2D.whiteTexture);

            GUI.color = Color.white;
        }

        private void DrawInteractionPrompt()
        {
            if (string.IsNullOrEmpty(_currentPrompt)) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.white;

            float w = 300f;
            float h = 30f;
            Rect rect = new Rect(
                (Screen.width - w) / 2f,
                Screen.height / 2f + 40f,
                w, h
            );

            // Shadow
            GUI.color = new Color(0, 0, 0, 0.7f);
            Rect shadowRect = new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height);
            GUI.Label(shadowRect, _currentPrompt, style);

            GUI.color = Color.white;
            GUI.Label(rect, _currentPrompt, style);
        }

        private void DrawDebugPanel()
        {
            GUILayout.BeginArea(new Rect(10, 10, 320, 250));
            GUILayout.BeginVertical("box");

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            GUILayout.Label("=== DEBUG (F1) ===", headerStyle);
            GUILayout.Label($"FPS: {_currentFPS:F0}");

            if (_player != null)
            {
                Vector3 pos = _player.transform.position;
                GUILayout.Label($"Pos: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
                GUILayout.Label($"Speed: {_player.CurrentSpeed:F1} m/s");
                GUILayout.Label($"Grounded: {_player.IsGrounded} | Sprint: {_player.IsSprinting}");
            }
            else
            {
                GUILayout.Label("Player: NOT FOUND");
            }

            GUILayout.Space(5);

            if (_interactSystem != null)
            {
                var target = _interactSystem.CurrentTarget;
                string targetName = target != null ? target.InteractionPrompt : "None";
                GUILayout.Label($"Looking At: {targetName}");
                GUILayout.Label($"Interact Range: {_interactSystem.InteractRange}m");
            }
            else
            {
                GUILayout.Label("InteractSystem: NOT FOUND");
            }

            GUILayout.Space(5);
            GUILayout.Label("ESC = Toggle Cursor | F1 = Toggle Debug");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawDebugMessage()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Italic
            };
            style.normal.textColor = Color.yellow;

            float alpha = Mathf.Clamp01(_debugMessageTimer);
            GUI.color = new Color(1, 1, 0, alpha);
            GUI.Label(new Rect(10, Screen.height - 40, 500, 30), _lastDebugMessage, style);
            GUI.color = Color.white;
        }

        // ── Event Handlers ──

        private void HandleDebugMessage(string msg)
        {
            _lastDebugMessage = msg;
            _debugMessageTimer = 3f;
        }

        private void HandleInteractableFound(GameObject target)
        {
            var interactable = target.GetComponentInParent<IInteractable>();
            _currentPrompt = interactable?.InteractionPrompt ?? "";
        }

        private void HandleInteractableLost()
        {
            _currentPrompt = "";
        }
    }
}
