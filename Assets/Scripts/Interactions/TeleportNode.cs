using UnityEngine;

namespace CBRN.Interactions
{
    /// <summary>
    /// Gaze-activated waypoint for comfortable VR navigation without motion sickness.
    /// The trainee looks at the node for the dwell duration (or taps), and is teleported to this location.
    /// </summary>
    public class TeleportNode : MonoBehaviour, IGazeInteractable
    {
        [Header("Teleport Target")]
        [Tooltip("The player rig or camera root GameObject to reposition.")]
        [SerializeField] private Transform playerTransform;

        [Tooltip("Target height offset for player eye-level (standard ~1.6m).")]
        [SerializeField] private float eyeLevelHeight = 1.6f;

        [Header("Visual Feedback")]
        [Tooltip("Renderer of the teleport marker.")]
        [SerializeField] private Renderer nodeRenderer;

        [SerializeField] private Color normalColor = new Color(0.2f, 0.6f, 1.0f, 0.6f);
        [SerializeField] private Color hoverColor = new Color(1.0f, 0.8f, 0.2f, 0.9f);
        [SerializeField] private Color activatedColor = new Color(0.2f, 1.0f, 0.4f, 1.0f);

        private Material _instancedMaterial;

        private void Awake()
        {
            if (nodeRenderer == null) nodeRenderer = GetComponent<Renderer>();
            if (nodeRenderer != null)
            {
                _instancedMaterial = nodeRenderer.material;
                _instancedMaterial.color = normalColor;
            }

            if (playerTransform == null && Camera.main != null)
            {
                // If camera has a parent (e.g. XR Rig or Player root), use parent; else use camera
                playerTransform = Camera.main.transform.parent != null ? Camera.main.transform.parent : Camera.main.transform;
            }
        }

        public void OnGazeEnter()
        {
            if (_instancedMaterial != null) _instancedMaterial.color = hoverColor;
            transform.localScale *= 1.15f; // Gentle hover pop
        }

        public void OnGazeStay(float normalizedProgress)
        {
            // Visual feedback: lerp between hover color and activated color as dwell nears 100%
            if (_instancedMaterial != null)
            {
                _instancedMaterial.color = Color.Lerp(hoverColor, activatedColor, normalizedProgress);
            }
        }

        public void OnGazeExit()
        {
            if (_instancedMaterial != null) _instancedMaterial.color = normalColor;
            transform.localScale = Vector3.one; // Reset scale
        }

        public void OnGazeTrigger()
        {
            if (playerTransform == null)
            {
                if (Camera.main != null)
                {
                    playerTransform = Camera.main.transform.parent != null ? Camera.main.transform.parent : Camera.main.transform;
                }
            }

            if (playerTransform != null)
            {
                // Teleport player directly above node at eye level
                Vector3 destination = transform.position;
                destination.y += eyeLevelHeight;
                playerTransform.position = destination;

                Debug.Log($"[TeleportNode] Trainee teleported to waypoint: {gameObject.name} at {destination}");
            }

            OnGazeExit();
        }

        private void OnDestroy()
        {
            if (_instancedMaterial != null)
            {
                Destroy(_instancedMaterial);
            }
        }
    }
}
