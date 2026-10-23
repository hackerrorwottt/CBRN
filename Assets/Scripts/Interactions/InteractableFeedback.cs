using UnityEngine;
using UnityEngine.Events;

namespace CBRN.Interactions
{
    /// <summary>
    /// Attach to any 3D object to make it react visually to Cardboard gaze and tap triggers.
    /// Used for briefing boards, PPE gear racks, chemical valve shutoffs, and victims.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableFeedback : MonoBehaviour, IGazeInteractable
    {
        [Header("Hover Visuals")]
        [Tooltip("Optional custom renderer to highlight. If null, uses this GameObject's Renderer.")]
        [SerializeField] private Renderer targetRenderer;

        [Tooltip("Color tint applied when gazed upon.")]
        [SerializeField] private Color hoverColor = new Color(1f, 0.9f, 0.4f);

        [Header("Events")]
        [Tooltip("Triggered when dwell completes or single tap occurs.")]
        public UnityEvent onTriggered;

        private Color _originalColor;
        private Material _instancedMat;
        private bool _isHovered = false;

        private void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                _instancedMat = targetRenderer.material;
                _originalColor = _instancedMat.color;
            }
        }

        public void OnGazeEnter()
        {
            _isHovered = true;
            if (_instancedMat != null)
            {
                _instancedMat.color = hoverColor;
            }
        }

        public void OnGazeStay(float normalizedProgress)
        {
            // Optional micro-pulsing or progress response
        }

        public void OnGazeExit()
        {
            _isHovered = false;
            if (_instancedMat != null)
            {
                _instancedMat.color = _originalColor;
            }
        }

        public void OnGazeTrigger()
        {
            Debug.Log($"[InteractableFeedback] {gameObject.name} triggered by gaze/tap!");
            onTriggered?.Invoke();
        }

        private void OnDestroy()
        {
            if (_instancedMat != null)
            {
                Destroy(_instancedMat);
            }
        }
    }
}
