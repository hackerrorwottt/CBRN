using UnityEngine;
using CBRN.Interactions;
using CBRN.Core;

namespace CBRN.UI
{
    /// <summary>
    /// Interactive Briefing Billboard in the Cold Zone.
    /// Trainee looks at the board or taps to acknowledge the incident parameters and begin Step 2.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BriefingBoard : MonoBehaviour, IGazeInteractable
    {
        [Header("System Linkage")]
        [SerializeField] private MissionManager missionManager;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer boardRenderer;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.45f, 0.75f);
        [SerializeField] private Color hoverColor = new Color(0.35f, 0.6f, 0.9f);
        [SerializeField] private Color acknowledgedColor = new Color(0.2f, 0.75f, 0.4f);

        private Material _instancedMat;
        private bool _isAcknowledged = false;

        private void Awake()
        {
            if (missionManager == null) missionManager = FindObjectOfType<MissionManager>();

            if (boardRenderer == null) boardRenderer = GetComponent<Renderer>();
            if (boardRenderer != null)
            {
                _instancedMat = boardRenderer.material;
                _instancedMat.color = normalColor;
            }
        }

        public void OnGazeEnter()
        {
            if (!_isAcknowledged && _instancedMat != null)
            {
                _instancedMat.color = hoverColor;
            }
        }

        public void OnGazeStay(float normalizedProgress) { }

        public void OnGazeExit()
        {
            if (_instancedMat != null)
            {
                _instancedMat.color = _isAcknowledged ? acknowledgedColor : normalColor;
            }
        }

        public void OnGazeTrigger()
        {
            if (_isAcknowledged) return;

            _isAcknowledged = true;
            if (_instancedMat != null) _instancedMat.color = acknowledgedColor;

            if (missionManager == null) missionManager = FindObjectOfType<MissionManager>();
            if (missionManager != null)
            {
                missionManager.CompleteBriefing();
            }

            Debug.Log("[BriefingBoard] Trainee acknowledged incident briefing! Transitioning to Hazard Identification.");
        }

        private void OnDestroy()
        {
            if (_instancedMat != null) Destroy(_instancedMat);
        }
    }
}

