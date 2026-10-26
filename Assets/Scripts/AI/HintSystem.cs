using System;
using UnityEngine;
using CBRN.Core;
using CBRN.Assessment;

namespace CBRN.AI
{
    /// <summary>
    /// Virtual Instructor AI: Provides dynamic context-aware guidance
    /// driven by the active scenario step and the trainee's most recent mistake.
    /// </summary>
    public class HintSystem : MonoBehaviour
    {
        [Header("System Linkage")]
        [SerializeField] private AssessmentManager assessmentManager;

        [Header("Active Guidance")]
        [SerializeField] private string currentHintText = "Proceed to the Briefing Board in the Cold Zone to begin.";
        [SerializeField] private bool isUrgentCorrection = false;

        // Events
        public event Action<string, bool> OnHintUpdated; // text, isUrgent

        public string CurrentHint => currentHintText;
        public bool IsUrgent => isUrgentCorrection;

        private void Start()
        {
            if (assessmentManager == null) assessmentManager = FindObjectOfType<AssessmentManager>();

            if (assessmentManager != null)
            {
                assessmentManager.OnMistakeRecorded += HandleMistakeGuidance;
            }

            OnHintUpdated?.Invoke(currentHintText, false);
        }

        private void OnDestroy()
        {
            if (assessmentManager != null)
            {
                assessmentManager.OnMistakeRecorded -= HandleMistakeGuidance;
            }
        }

        public void SetStepGuidance(ScenarioStepData stepData)
        {
            if (stepData == null) return;

            isUrgentCorrection = false;
            currentHintText = $"[Objective]: {stepData.objectiveText}\n[Instructor Hint]: {stepData.standardHint}";

            Debug.Log($"[HintSystem] Step Guidance: {currentHintText}");
            OnHintUpdated?.Invoke(currentHintText, false);
        }

        private void HandleMistakeGuidance(string mistakeDesc, int penalty)
        {
            isUrgentCorrection = true;

            // Formulate tailored instructional corrective action
            if (mistakeDesc.Contains("PPE") || mistakeDesc.Contains("unprotected"))
            {
                currentHintText = "CORRECTIVE ADVICE: Evacuate to Cold Zone immediately! Locate the PPE locker and equip the Level A Encapsulating Suit before re-entering.";
            }
            else if (mistakeDesc.Contains("placard") || mistakeDesc.Contains("identifying"))
            {
                currentHintText = "CORRECTIVE ADVICE: Inspect the UN Hazmat placard on the chemical tanks to verify the substance before entering danger zone.";
            }
            else if (mistakeDesc.Contains("Rescue") || mistakeDesc.Contains("uncontained"))
            {
                currentHintText = "CORRECTIVE ADVICE: Contain the leak at the ruptured valve first, or ensure certified vapor protection before approaching casualty.";
            }
            else
            {
                currentHintText = $"CORRECTIVE ADVICE: Avoid safety violation: {mistakeDesc}";
            }

            Debug.LogWarning($"[HintSystem] Urgent Instructor Correction: {currentHintText}");
            OnHintUpdated?.Invoke(currentHintText, true);
        }
    }
}

