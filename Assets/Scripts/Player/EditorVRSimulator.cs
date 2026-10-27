using UnityEngine;

namespace CBRN.Player
{
    /// <summary>
    /// Editor Test Mode: Simulates VR head rotation (mouse look) and trainee translation (WASD)
    /// in the Unity Editor, enabling complete scenario testing without a Cardboard headset or Android build.
    /// Automatically disables itself on mobile Android devices.
    /// </summary>
    public class EditorVRSimulator : MonoBehaviour
    {
        [Header("Mouse Look Settings")]
        [Tooltip("Mouse sensitivity for simulated head rotation.")]
        [SerializeField] private float mouseSensitivity = 2.0f;

        [Tooltip("Minimum and maximum vertical pitch angles to simulate neck constraints.")]
        [SerializeField] private float minPitchAngle = -80f;
        [SerializeField] private float maxPitchAngle = 80f;

        [Header("Editor Navigation")]
        [Tooltip("Root rig transform to translate with WASD (defaults to parent PlayerRig if attached to camera).")]
        [SerializeField] private Transform rigTransform;

        [Tooltip("Comfortable walking speed for testing the compound in the Editor.")]
        [SerializeField] private float moveSpeed = 4.0f;

        [Tooltip("Multiplier applied when holding Left Shift.")]
        [SerializeField] private float sprintMultiplier = 2.0f;

        private float _yaw = 0f;
        private float _pitch = 0f;
        private bool _isCursorLocked = false;

        private void Start()
        {
#if !UNITY_EDITOR
            // Automatically destroy/disable simulator when running on real Android Cardboard device
            enabled = false;
            return;
#endif

            if (rigTransform == null)
            {
                rigTransform = transform.parent != null ? transform.parent : transform;
            }

            Vector3 currentEuler = transform.localEulerAngles;
            _yaw = currentEuler.y;
            _pitch = currentEuler.x > 180f ? currentEuler.x - 360f : currentEuler.x;
        }

        private void Update()
        {
#if UNITY_EDITOR
            HandleCursorLock();
            HandleMouseLook();
            HandleMovement();
#endif
        }

        private void HandleCursorLock()
        {
            // Toggle cursor lock with Escape or Right Click
            if (Input.GetMouseButtonDown(1)) // Right click to capture
            {
                SetCursorLock(true);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLock(false);
            }
        }

        private void SetCursorLock(bool locked)
        {
            _isCursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void HandleMouseLook()
        {
            // Only look around if cursor is locked OR right mouse button is held
            if (!_isCursorLocked && !Input.GetMouseButton(1)) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _yaw += mouseX;
            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, minPitchAngle, maxPitchAngle);

            transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal"); // A/D
            float vertical = Input.GetAxis("Vertical");     // W/S

            if (Mathf.Abs(horizontal) < 0.01f && Mathf.Abs(vertical) < 0.01f) return;

            Transform moveTarget = rigTransform != null ? rigTransform : transform;
            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1.0f);

            // Move relative to camera facing direction, flattened on ground plane
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
            moveTarget.position += moveDirection * (speed * Time.deltaTime);
        }
    }
}

