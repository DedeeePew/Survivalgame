using UnityEngine;

namespace SurvivalGame.Player
{
    /// <summary>
    /// First-Person Player Controller.
    /// Handles WASD movement, sprint, jump, gravity, and mouse look.
    /// Requires a CharacterController component on the same GameObject.
    /// Camera should be a child object.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _sprintSpeed = 8f;
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private float _gravity = -15f;

        [Header("Mouse Look")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _maxLookAngle = 85f;
        [SerializeField] private Transform _cameraHolder;

        [Header("Ground Check")]
        [SerializeField] private float _groundCheckDistance = 0.3f;
        [SerializeField] private LayerMask _groundMask = ~0;

        private CharacterController _cc;
        private Vector3 _velocity;
        private float _cameraPitch;
        private bool _isGrounded;

        // Public accessors for DebugUI
        public float CurrentSpeed => new Vector3(_cc.velocity.x, 0, _cc.velocity.z).magnitude;
        public bool IsGrounded => _isGrounded;
        public bool IsSprinting { get; private set; }

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();

            if (_cameraHolder == null)
            {
                // Try to find camera in children
                var cam = GetComponentInChildren<Camera>();
                if (cam != null)
                    _cameraHolder = cam.transform;
                else
                    Debug.LogError("[PlayerController] No camera found! Assign CameraHolder.");
            }
        }

        private void Update()
        {
            HandleGroundCheck();
            HandleMovement();
            HandleMouseLook();
        }

        private void HandleGroundCheck()
        {
            // Use CharacterController's built-in ground check + a small raycast
            _isGrounded = _cc.isGrounded;

            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Small downward force to keep grounded
            }
        }

        private void HandleMovement()
        {
            // Input
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            IsSprinting = Input.GetKey(KeyCode.LeftShift) && moveZ > 0;
            float speed = IsSprinting ? _sprintSpeed : _walkSpeed;

            // Direction relative to player facing
            Vector3 moveDir = transform.right * moveX + transform.forward * moveZ;
            moveDir = Vector3.ClampMagnitude(moveDir, 1f); // Normalize diagonal

            _cc.Move(moveDir * speed * Time.deltaTime);

            // Jump
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _velocity.y = _jumpForce;
            }

            // Gravity
            _velocity.y += _gravity * Time.deltaTime;
            _cc.Move(_velocity * Time.deltaTime);
        }

        private void HandleMouseLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

            // Horizontal rotation → rotate player body
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation → rotate camera only
            _cameraPitch -= mouseY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -_maxLookAngle, _maxLookAngle);

            if (_cameraHolder != null)
            {
                _cameraHolder.localRotation = Quaternion.Euler(_cameraPitch, 0, 0);
            }
        }
    }
}
