using System;
using System.Collections.Generic;
using UnityEngine;
using CBRN.PPE;
using CBRN.Hazards;
using CBRN.Rescue;
using CBRN.Assessment;
using CBRN.AI;
using CBRN.Environment;

namespace CBRN.Core
{
    /// <summary>
    /// Master orchestrator for the 7-step chemical disaster scenario.
    /// Manages state transitions, listens to environmental domain events,
    /// coordinates scoring with AssessmentManager, and updates the HintSystem.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        [Header("Scenario Configuration")]
        [Tooltip("ScriptableObject definitions for steps 1 through 7.")]
        [SerializeField] private List<ScenarioStepData> scenarioSteps = new List<ScenarioStepData>();

        [Header("System Dependencies")]
        [SerializeField] private AssessmentManager assessmentManager;
        [SerializeField] private HintSystem hintSystem;
        [SerializeField] private SafetyZoneMonitor safetyZoneMonitor;
        [SerializeField] private PPESelectionStation ppeStation;
        [SerializeField] private EvacuationSystem evacuationSystem;

        [Header("State (Read-Only)")]
        [SerializeField] private MissionStep currentStep = MissionStep.Step1_Briefing;

        // Events
        public event Action<MissionStep, ScenarioStepData> OnStepTransitioned;
        public event Action OnMissionComplete;

        public MissionStep CurrentStep => currentStep;

        private void Start()
        {
            ResolveDependencies();
            SubscribeToEvents();

            // Initialize Step 1: Briefing
            TransitionToStep(MissionStep.Step1_Briefing);
        }

        private void ResolveDependencies()
        {
            if (assessmentManager == null) assessmentManager = FindObjectOfType<AssessmentManager>();
            if (hintSystem == null) hintSystem = FindObjectOfType<HintSystem>();
            if (safetyZoneMonitor == null) safetyZoneMonitor = FindObjectOfType<SafetyZoneMonitor>();
            if (ppeStation == null) ppeStation = FindObjectOfType<PPESelectionStation>();
            if (evacuationSystem == null) evacuationSystem = FindObjectOfType<EvacuationSystem>();
        }

        private void SubscribeToEvents()
        {
            // Step 2 Trigger: Hazard Identification
            HazardPlacard.OnHazardIdentified += HandleHazardIdentified;

            // Step 3 Trigger: PPE Selection
            if (ppeStation != null)
            {
                ppeStation.OnPPEChanged += HandlePPEChanged;
            }

            // Step 4 Trigger: Zone Boundary Assessment
            if (safetyZoneMonitor != null)
            {
                safetyZoneMonitor.OnZoneEntered += HandleZoneEntered;
                safetyZoneMonitor.OnSafetyViolation += HandleSafetyViolation;
            }

            // Step 5 & 6 Trigger: Rescue & Evacuation
            if (evacuationSystem != null)
            {
                evacuationSystem.OnAllCasualtiesEvacuated += HandleAllCasualtiesEvacuated;
            }
        }

        private void OnDestroy()
        {
            HazardPlacard.OnHazardIdentified -= HandleHazardIdentified;
            if (ppeStation != null) ppeStation.OnPPEChanged -= HandlePPEChanged;
            if (safetyZoneMonitor != null)
            {
                safetyZoneMonitor.OnZoneEntered -= HandleZoneEntered;
                safetyZoneMonitor.OnSafetyViolation -= HandleSafetyViolation;
            }
            if (evacuationSystem != null)
            {
                evacuationSystem.OnAllCasualtiesEvacuated -= HandleAllCasualtiesEvacuated;
            }
        }

        public void TransitionToStep(MissionStep nextStep)
        {
            // Log previous step completion in assessment
            if (currentStep != nextStep)
            {
                int prevIndex = (int)currentStep;
                string prevName = currentStep.ToString();
                assessmentManager?.FinalizeStepLog(prevIndex, prevName, true);
            }

            currentStep = nextStep;
            int stepIndex = (int)currentStep;

            ScenarioStepData stepData = scenarioSteps.Find(s => s.stepIndex == stepIndex);

            Debug.Log($"[MissionManager] === ADVANCING TO STEP {stepIndex}: {currentStep} ===");

            if (hintSystem != null && stepData != null)
            {
                hintSystem.SetStepGuidance(stepData);
            }

            OnStepTransitioned?.Invoke(currentStep, stepData);

            if (currentStep == MissionStep.Step7_Debrief)
            {
                assessmentManager?.FinalizeAssessment();
                OnMissionComplete?.Invoke();
            }
        }

        // Domain event callbacks driving procedural flow:
        public void CompleteBriefing()
        {
            if (currentStep == MissionStep.Step1_Briefing)
            {
                TransitionToStep(MissionStep.Step2_HazardIdentification);
            }
        }

        private void HandleHazardIdentified(HazardProfileData profile)
        {
            if (currentStep <= MissionStep.Step2_HazardIdentification)
            {
                TransitionToStep(MissionStep.Step3_PPESelection);
            }
        }

        private void HandlePPEChanged(PPEItemData equippedItem)
        {
            if (currentStep == MissionStep.Step3_PPESelection)
            {
                if (equippedItem.protectionLevel == PPELevel.LevelA_FullyEncapsulated)
                {
                    TransitionToStep(MissionStep.Step4_SafetyAssessment);
                }
                else
                {
                    assessmentManager?.RecordMistake($"Inadequate PPE Selected: {equippedItem.itemName} does not protect against toxic vapor!", equippedItem.incorrectSelectionPenalty);
                }
            }
        }

        private void HandleZoneEntered(ZoneType zone)
        {
            if (currentStep == MissionStep.Step4_SafetyAssessment)
            {
                if (zone == ZoneType.WarmZone_Perimeter || zone == ZoneType.HotZone_ChemicalLeak)
                {
                    TransitionToStep(MissionStep.Step5_VictimRescue);
                }
            }
        }

        private void HandleSafetyViolation(string message, int penalty)
        {
            assessmentManager?.RecordMistake(message, penalty);
        }

        private void HandleAllCasualtiesEvacuated()
        {
            if (currentStep == MissionStep.Step5_VictimRescue || currentStep == MissionStep.Step6_Evacuation)
            {
                TransitionToStep(MissionStep.Step7_Debrief);
            }
        }
    }
}
