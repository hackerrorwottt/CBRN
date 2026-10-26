using System;
using UnityEngine;
using CBRN.PPE;

namespace CBRN.Hazards
{
    /// <summary>
    /// Models the dynamic chemical vapor spread over time.
    /// Growth speed is scaled by an adaptive difficulty multiplier fetched from the backend.
    /// Detects trainee exposure and validates personal protective equipment.
    /// </summary>
    public class HazardSystem : MonoBehaviour
    {
        [Header("Hazard Configuration")]
        [Tooltip("The ScriptableObject defining expansion speed, max radius, and toxicity.")]
        [SerializeField] private HazardProfileData profile;

        [Header("Visual & Collision Bounds")]
        [Tooltip("Visual representation of the chemical vapor cloud.")]
        [SerializeField] private Transform gasCloudVisual;

        [Tooltip("Trigger volume used to detect trainee or victim overlap.")]
        [SerializeField] private SphereCollider gasCollider;

        [Header("Runtime Dynamics")]
        [Tooltip("Difficulty multiplier fetched from trainee history (1.0 = standard, 1.3 = harder/faster, 0.7 = easier/slower).")]
        [Range(0.5f, 2.0f)]
        [SerializeField] private float difficultyMultiplier = 1.0f;

        [SerializeField] private bool isLeakActive = true;
        [SerializeField] private float currentRadius = 2.0f;

        // Decoupled Events
        public event Action<float> OnHazardRadiusChanged;
        public event Action<float> OnTraineeExposed; // Exposure toxicity amount
        public event Action OnLeakContained;

        private PlayerPPEState _traineePPE;
        private bool _isTraineeInside = false;

        public float CurrentRadius => currentRadius;
        public bool IsActive => isLeakActive;
        public HazardProfileData Profile => profile;

        private void Start()
        {
            if (gasCollider == null)
            {
                gasCollider = GetComponent<SphereCollider>();
                if (gasCollider == null) gasCollider = gameObject.AddComponent<SphereCollider>();
            }
            gasCollider.isTrigger = true;

            if (profile != null)
            {
                currentRadius = profile.initialRadius;
            }

            UpdateBounds();
        }

        private void Update()
        {
            if (isLeakActive && profile != null)
            {
                // Chemical gas expansion formula
                float expansionStep = profile.baseSpreadSpeed * difficultyMultiplier * Time.deltaTime;
                if (currentRadius < profile.maxSpreadRadius)
                {
                    currentRadius = Mathf.Min(profile.maxSpreadRadius, currentRadius + expansionStep);
                    UpdateBounds();
                    OnHazardRadiusChanged?.Invoke(currentRadius);
                }
            }
            else if (!isLeakActive && profile != null && currentRadius > 0.1f)
            {
                // Dissipation after valve containment
                currentRadius = Mathf.Max(0f, currentRadius - (profile.dissipationSpeed * Time.deltaTime));
                UpdateBounds();
                OnHazardRadiusChanged?.Invoke(currentRadius);
            }

            // Check exposure if trainee is inside without Level A PPE
            if (_isTraineeInside && profile != null)
            {
                bool isProtected = _traineePPE != null && _traineePPE.HasAdequateProtection(profile.minimumRequiredPPE);
                if (!isProtected)
                {
                    float exposureTick = profile.toxicityRatePerSecond * Time.deltaTime;
                    OnTraineeExposed?.Invoke(exposureTick);
                }
            }
        }

        private void UpdateBounds()
        {
            if (gasCollider != null)
            {
                gasCollider.radius = currentRadius;
            }

            if (gasCloudVisual != null)
            {
                // Scale visual sphere/cylinder to match the radius
                gasCloudVisual.localScale = new Vector3(currentRadius * 2f, currentRadius * 1.2f, currentRadius * 2f);
            }
        }

        /// <summary>
        /// Adjusts the expansion rate dynamically (called when adaptive difficulty is retrieved from backend).
        /// </summary>
        public void SetDifficultyMultiplier(float multiplier)
        {
            difficultyMultiplier = Mathf.Clamp(multiplier, 0.5f, 2.5f);
            Debug.Log($"[HazardSystem] Adaptive difficulty applied to hazard spread: {difficultyMultiplier:F2}x");
        }

        /// <summary>
        /// Neutralizes/isolates the leak source (called by ValveShutoff).
        /// </summary>
        public void ContainLeak()
        {
            if (!isLeakActive) return;

            isLeakActive = false;
            Debug.Log("[HazardSystem] Ruptured valve closed! Chemical vapor spread halted; dissipation initiated.");
            OnLeakContained?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerPPEState ppe = other.GetComponentInParent<PlayerPPEState>();
            if (ppe != null)
            {
                _traineePPE = ppe;
                _isTraineeInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerPPEState ppe = other.GetComponentInParent<PlayerPPEState>();
            if (ppe != null)
            {
                _isTraineeInside = false;
            }
        }
    }
}

