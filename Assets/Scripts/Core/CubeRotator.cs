using UnityEngine;

namespace CBRN.Core
{
    /// <summary>
    /// Rotates a GameObject smoothly across frames.
    /// Used in Milestone 1 to verify continuous rendering, 3D stereo depth,
    /// and framerate stability on the mobile VR headset.
    /// </summary>
    public class CubeRotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Degrees to rotate per second along each axis (X, Y, Z).")]
        [SerializeField] private Vector3 rotationSpeed = new Vector3(15f, 30f, 45f);

        private void Update()
        {
            // Time.deltaTime ensures smooth, uniform motion regardless of whether
            // the phone runs at 30, 60, or 90 frames per second.
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}

