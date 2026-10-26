using System;
using UnityEngine;

namespace CBRN.PPE
{
    /// <summary>
    /// Attached to the trainee's VR camera rig.
    /// Tracks equipped PPE ensemble, evaluates hazard protection thresholds,
    /// and dispatches state change events to assessment and hazard systems.
    /// </summary>
    public class PlayerPPEState : MonoBehaviour
    {
        [Header("Initial Configuration")]
        [Tooltip("The default PPE ensemble worn when entering the simulation (standard work uniform).")]
        [SerializeField] private PPEItemData initialPPE;

        [Header("Current State (Read-Only)")]
        [SerializeField] private PPEItemData currentEquippedPPE;

        // Events for decoupled system communication
        public event Action<PPEItemData> OnPPEEquipped;

        public PPEItemData CurrentPPE => currentEquippedPPE;
        public PPELevel CurrentLevel => currentEquippedPPE != null ? currentEquippedPPE.protectionLevel : PPELevel.LevelD_WorkCoveralls;

        private void Awake()
        {
            if (initialPPE != null)
            {
                EquipPPE(initialPPE);
            }
        }

        /// <summary>
        /// Equips a new PPE ensemble and notifies listening managers.
        /// </summary>
        public void EquipPPE(PPEItemData newPPE)
        {
            if (newPPE == null) return;

            currentEquippedPPE = newPPE;
            Debug.Log($"[PlayerPPEState] Trainee equipped: {newPPE.itemName} ({newPPE.protectionLevel})");

            OnPPEEquipped?.Invoke(currentEquippedPPE);
        }

        /// <summary>
        /// Checks if currently equipped PPE meets or exceeds the required hazard protection level.
        /// </summary>
        public bool HasAdequateProtection(PPELevel requiredLevel)
        {
            if (currentEquippedPPE == null) return false;
            // Higher or equal enum values indicate higher protection levels
            return (int)currentEquippedPPE.protectionLevel >= (int)requiredLevel;
        }

        /// <summary>
        /// Checks if current PPE has active vapor protection certification.
        /// </summary>
        public bool ProtectsAgainstVapors()
        {
            return currentEquippedPPE != null && currentEquippedPPE.protectsAgainstVapors;
        }
    }
}

