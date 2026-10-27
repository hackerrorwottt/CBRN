using System;
using UnityEngine;
using CBRN.Interactions;

namespace CBRN.Hazards
{
    /// <summary>
    /// Interactive Hazmat placard / NFPA 704 diamond mounted near the chemical tanks.
    /// Trainee inspects it to complete Step 2 (Hazard Identification).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HazardPlacard : MonoBehaviour, IGazeInteractable
    {
        [Header("Hazard Configuration")]
        [Tooltip("The hazard profile linked to this facility infrastructure.")]
        [SerializeField] private HazardProfileData hazardProfile;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer placardRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color identifiedColor = Color.cyan;

        // Events
        public static event Action<HazardProfileData> OnHazardIdentified;

        private bool _isIdentified = false;
        private Material _instancedMat;

        public bool IsIdentified => _isIdentified;

        private void Awake()
        {
            if (placardRenderer == null) placardRenderer = GetComponent<Renderer>();
            if (placardRenderer != null)
            {
                _instancedMat = placardRenderer.material;
                _instancedMat.color = normalColor;
            }
        }

        public void OnGazeEnter()
        {
            if (!_isIdentified && _instancedMat != null)
            {
                _instancedMat.color = Color.yellow;
            }
        }

        public void OnGazeStay(float normalizedProgress) { }

        public void OnGazeExit()
        {
            if (_instancedMat != null)
            {
                _instancedMat.color = _isIdentified ? identifiedColor : normalColor;
            }
        }

        public void OnGazeTrigger()
        {
            if (_isIdentified) return;

            if (hazardProfile == null)
            {
                Debug.LogWarning($"[HazardPlacard] Cannot identify hazard on {gameObject.name}: hazardProfile is not assigned.");
                return;
            }

            _isIdentified = true;
            if (_instancedMat != null) _instancedMat.color = identifiedColor;

            Debug.Log($"[HazardPlacard] Trainee identified hazard: {hazardProfile.hazardName} ({hazardProfile.placardCode})");
            OnHazardIdentified?.Invoke(hazardProfile);
        }

        private void OnDestroy()
        {
            if (_instancedMat != null) Destroy(_instancedMat);
        }
    }
}

