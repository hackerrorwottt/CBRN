using UnityEngine;
using CBRN.PPE;

namespace CBRN.Hazards
{
    /// <summary>
    /// Configurable data container defining an environmental chemical hazard profile.
    /// ScriptableObject ensures hazard spread speeds, toxicity, and PPE requirements are never hardcoded.
    /// Educational training prototype only: parameters reflect generic responder protocols.
    /// </summary>
    [CreateAssetMenu(fileName = "HazardProfile_", menuName = "CBRN/Data/Hazard Profile", order = 2)]
    public class HazardProfileData : ScriptableObject
    {
        [Header("Hazard Identification")]
        [Tooltip("Procedural training name (e.g. Industrial Toxic Vapor Analogue).")]
        public string hazardName = "Industrial Toxic Vapor (Training Analogue)";

        [Tooltip("Placard UN code / Hazchem reference for identification step.")]
        public string placardCode = "UN 1005 (Ammonia Analogue) / Class 2.3";

        [Tooltip("Procedural summary displayed on briefing/placard.")]
        [TextArea(2, 4)]
        public string hazardDescription = "Toxic, corrosive vapor cloud. Inhalation hazard. Keep upwind and isolate perimeter.";

        [Header("PPE Requirements")]
        [Tooltip("Minimum certified PPE ensemble required to safely enter the hazard area.")]
        public PPELevel minimumRequiredPPE = PPELevel.LevelA_FullyEncapsulated;

        [Header("Spread Dynamics")]
        [Tooltip("Initial radius in meters when the chemical leak begins.")]
        public float initialRadius = 2.0f;

        [Tooltip("Base expansion rate in meters per second.")]
        public float baseSpreadSpeed = 0.35f;

        [Tooltip("Maximum gas expansion radius if uncontained.")]
        public float maxSpreadRadius = 16.0f;

        [Tooltip("Dissipation speed in meters per second once the leak valve is closed.")]
        public float dissipationSpeed = 1.0f;

        [Header("Toxicity & Penalties")]
        [Tooltip("Health / exposure damage per second if player is inside gas zone without required PPE.")]
        public float toxicityRatePerSecond = 15.0f;

        [Tooltip("Penalty points deducted for entering the hazard zone without Level A PPE.")]
        public int incorrectEntryPenalty = 25;
    }
}
