using System;
using System.Collections.Generic;
using UnityEngine;
using CBRN.AI;

namespace CBRN.Rescue
{
    /// <summary>
    /// Decontamination and medical triage handover point in the Warm/Cold transition zone.
    /// Detects when casualties are successfully escorted into the safety perimeter.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EvacuationSystem : MonoBehaviour
    {
        [Header("Evacuation Goal")]
        [Tooltip("Total victims required to evacuate to complete Step 6.")]
        [SerializeField] private int requiredEvacuations = 1;

        // Events
        public event Action<VictimAI> OnVictimSafelyHandedOver;
        public event Action OnAllCasualtiesEvacuated;

        private readonly List<VictimAI> _evacuatedVictims = new List<VictimAI>();

        public int EvacuatedCount => _evacuatedVictims.Count;
        public bool IsEvacuationGoalMet => _evacuatedVictims.Count >= requiredEvacuations;

        private void OnTriggerEnter(Collider other)
        {
            VictimAI victim = other.GetComponent<VictimAI>();
            if (victim == null)
            {
                victim = other.GetComponentInParent<VictimAI>();
            }

            if (victim != null && victim.CurrentState == VictimState.Following_Hero)
            {
                if (!_evacuatedVictims.Contains(victim))
                {
                    _evacuatedVictims.Add(victim);
                    victim.MarkSafe();

                    Debug.Log($"[EvacuationSystem] Casualty {victim.name} safely received at Triage ({_evacuatedVictims.Count}/{requiredEvacuations})");
                    OnVictimSafelyHandedOver?.Invoke(victim);

                    if (IsEvacuationGoalMet)
                    {
                        Debug.Log("[EvacuationSystem] All casualties successfully evacuated!");
                        OnAllCasualtiesEvacuated?.Invoke();
                    }
                }
            }
        }
    }
}

