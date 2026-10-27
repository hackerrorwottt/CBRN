using System;
using UnityEngine;
using CBRN.Environment;
using CBRN.PPE;
using CBRN.Hazards;

namespace CBRN.Assessment
{
    /// <summary>
    /// Tracks trainee movement across CBRN operational zones (Cold, Warm, Hot, Evacuation).
    /// Enforces disaster response safety protocols: checks PPE compliance and hazard identification
    /// before permitting safe operations in the Hot Zone.
    /// Supports both direct trigger detection and notifications from ZoneIdentifier.
    /// </summary>
    public class SafetyZoneMonitor : MonoBehaviour
    {
        public static SafetyZoneMonitor Instance { get; private set; }

        [Header("References")]
        [Tooltip("Player PPE state tracker.")]
        [SerializeField] private PlayerPPEState playerPPE;

        [Header("State (Read-Only)")]
        [SerializeField] private ZoneType currentZone = ZoneType.ColdZone_Staging;
        [SerializeField] private bool hasIdentifiedHazard = false;

        // Events
        public event Action<ZoneType> OnZoneEntered;
        public event Action<string, int> OnSafetyViolation; // message, penalty points

        public ZoneType CurrentZone => currentZone;

        private void Awake()
        {
            Instance = this;

            if (playerPPE == null) playerPPE = FindObjectOfType<PlayerPPEState>();
            HazardPlacard.OnHazardIdentified += HandleHazardIdentified;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            HazardPlacard.OnHazardIdentified -= HandleHazardIdentified;
        }

        private void HandleHazardIdentified(HazardProfileData profile)
        {
            hasIdentifiedHazard = true;
        }

        /// <summary>
        /// Called when the trainee crosses into a new operational zone.
        /// </summary>
        public void OnZoneBoundaryCrossed(ZoneType enteredZone)
        {
            if (enteredZone == currentZone) return;

            currentZone = enteredZone;
            Debug.Log($"[SafetyZoneMonitor] Trainee crossed boundary into: {currentZone}");
            OnZoneEntered?.Invoke(currentZone);

            EvaluateSafetyCompliance(enteredZone);
        }

        private void OnTriggerEnter(Collider other)
        {
            ZoneIdentifier zone = other.GetComponent<ZoneIdentifier>();
            if (zone != null)
            {
                OnZoneBoundaryCrossed(zone.CurrentZoneType);
            }
        }

        private void EvaluateSafetyCompliance(ZoneType zone)
        {
            if (zone == ZoneType.HotZone_ChemicalLeak)
            {
                // Protocol check 1: Has trainee identified the hazard?
                if (!hasIdentifiedHazard)
                {
                    OnSafetyViolation?.Invoke("Safety Breach: Entered Hot Zone prior to identifying the hazard via UN placard!", 15);
                }

                // Protocol check 2: Is trainee wearing Level A vapor protective gear?
                if (playerPPE == null || !playerPPE.HasAdequateProtection(PPELevel.LevelA_FullyEncapsulated))
                {
                    OnSafetyViolation?.Invoke("Critical Safety Breach: Entered Hot Zone without certified Level A vapor-protective ensemble!", 30);
                }
            }
        }
    }
}
