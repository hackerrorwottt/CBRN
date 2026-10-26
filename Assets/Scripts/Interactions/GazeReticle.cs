using UnityEngine;
using UnityEngine.UI;

namespace CBRN.Interactions
{
    /// <summary>
    /// Visual reticle cursor anchored to the trainee's line of sight.
    /// Dynamically positions itself on the hit surface to prevent stereoscopic eye strain (diplopia)
    /// and visualizes the dwell-timer progress.
    /// </summary>
    public class GazeReticle : MonoBehaviour
    {
        [Header("Distance Settings")]
        [Tooltip("Default distance in front of the camera when not looking at an object.")]
        [SerializeField] private float defaultDistance = 2.5f;

        [Tooltip("Minimum distance to prevent clipping into near objects.")]
        [SerializeField] private float minDistance = 0.5f;

        [Header("UI Visuals")]
        [Tooltip("Optional radial fill image that fills from 0 to 1 as dwell timer progresses.")]
        [SerializeField] private Image progressFillImage;

        [Tooltip("Base scale of the reticle at 1 meter distance.")]
        [SerializeField] private Vector3 baseScale = new Vector3(0.02f, 0.02f, 0.02f);

        private Transform _cameraTransform;

        private void Awake()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void Start()
        {
            ResetProgress();
        }

        /// <summary>
        /// Updates the reticle position and scale based on raycast hit.
        /// </summary>
        /// <param name="hitPoint">World position of raycast hit, or Vector3.zero if no hit.</param>
        /// <param name="isHitting">Whether the raycast currently strikes a collider.</param>
        public void UpdatePosition(Vector3 hitPoint, bool isHitting)
        {
            if (_cameraTransform == null)
            {
                if (Camera.main != null) _cameraTransform = Camera.main.transform;
                else return;
            }

            float targetDistance = defaultDistance;

            if (isHitting)
            {
                float calculatedDist = Vector3.Distance(_cameraTransform.position, hitPoint) - 0.05f; // Slight offset toward camera
                targetDistance = Mathf.Max(minDistance, calculatedDist);
            }

            // Position reticle along camera forward vector
            transform.position = _cameraTransform.position + (_cameraTransform.forward * targetDistance);

            // Orient reticle to face camera flat
            transform.rotation = _cameraTransform.rotation;

            // Scale proportionally with distance so the reticle maintains consistent perceived angular size
            transform.localScale = baseScale * targetDistance;
        }

        /// <summary>
        /// Updates the dwell progress visual (0.0 to 1.0).
        /// </summary>
        public void SetProgress(float normalizedProgress)
        {
            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = Mathf.Clamp01(normalizedProgress);
            }
        }

        /// <summary>
        /// Resets the dwell progress visual to 0.
        /// </summary>
        public void ResetProgress()
        {
            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = 0f;
            }
        }
    }
}

