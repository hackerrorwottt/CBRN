using System;
using System.Collections.Generic;
using UnityEngine;
using CBRN.AI;
using CBRN.PPE;
using CBRN.Hazards;

namespace CBRN.Rescue
{
    /// <summary>
    /// Manages search, triage, and extraction of casualties in the hazard area.
    /// Evaluates whether trainee secured the leak or donned proper PPE before entering.
    /// Subscribes cleanly to victim events and handles proper cleanup.
    /// </summary>
    public class RescueSystem : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private PlayerPPEState playerPPE;
        [SerializeField] private HazardSystem hazardSystem;

        // Events
        public event Action<VictimAI> OnVictimTriaged;
        public event Action<string, int> OnRescueError; // message, penalty

        private readonly List<VictimAI> _registeredVictims = new List<VictimAI>();

        private void Start()
        {
            if (playerPPE == null) playerPPE = FindObjectOfType<PlayerPPEState>();
            if (hazardSystem == null) hazardSystem = FindObjectOfType<HazardSystem>();

            // Register with all victims in the scene
            VictimAI[] victims = FindObjectsOfType<VictimAI>();
            foreach (var victim in victims)
            {
                victim.OnVictimTriaged += HandleVictimTriaged;
                _registeredVictims.Add(victim);
            }
        }

        private void OnDestroy()
        {
            foreach (var victim in _registeredVictims)
            {
                if (victim != null)
                {
                    victim.OnVictimTriaged -= HandleVictimTriaged;
                }
            }
            _registeredVictims.Clear();
        }

        private void HandleVictimTriaged(VictimAI victim)
        {
            // Protocol check: Attempting rescue while gas is spreading and without Level A PPE
            if (hazardSystem != null && hazardSystem.IsActive)
            {
                if (playerPPE == null || !playerPPE.HasAdequateProtection(PPELevel.LevelA_FullyEncapsulated))
                {
                    OnRescueError?.Invoke("Dangerous Rescue: Attempted casualty extraction inside an uncontained chemical plume without Level A PPE!", 20);
                }
            }

            Debug.Log($"[RescueSystem] Casualty {victim.name} triaged and following responder.");
            OnVictimTriaged?.Invoke(victim);
        }
    }
}
