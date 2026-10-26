using UnityEngine;
using UnityEngine.UI;
using CBRN.Core;
using CBRN.Assessment;
using CBRN.AI;

namespace CBRN.UI
{
    /// <summary>
    /// Manages VR UI displays: Briefing Billboard, Virtual Instructor HUD, and Debrief Scorecard.
    /// Clean, readable presentation optimized for mobile VR Cardboard resolution.
    /// Note: Displays educational training disclaimers prominently.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private AssessmentManager assessmentManager;
        [SerializeField] private HintSystem hintSystem;

        [Header("Briefing Board UI (Cold Zone)")]
        [SerializeField] private Text briefingTitleText;
        [SerializeField] private Text briefingBodyText;

        [Header("Virtual Instructor HUD")]
        [SerializeField] private Text hintText;
        [SerializeField] private Image hintBackground;
        [SerializeField] private Color normalHintColor = new Color(0.1f, 0.2f, 0.4f, 0.85f);
        [SerializeField] private Color urgentHintColor = new Color(0.6f, 0.15f, 0.1f, 0.95f);

        [Header("Debrief Scorecard UI")]
        [SerializeField] private GameObject debriefPanel;
        [SerializeField] private Text debriefScoreText;
        [SerializeField] private Text debriefStatusText;
        [SerializeField] private Text debriefMistakesSummaryText;
        [SerializeField] private Text debriefDisclaimerText;

        private void Start()
        {
            if (missionManager == null) missionManager = FindObjectOfType<MissionManager>();
            if (assessmentManager == null) assessmentManager = FindObjectOfType<AssessmentManager>();
            if (hintSystem == null) hintSystem = FindObjectOfType<HintSystem>();

            if (hintSystem != null)
            {
                hintSystem.OnHintUpdated += UpdateHintUI;
            }

            if (assessmentManager != null)
            {
                assessmentManager.OnAssessmentFinalized += DisplayDebriefScorecard;
            }

            if (missionManager != null)
            {
                missionManager.OnStepTransitioned += HandleStepTransitionUI;
            }

            if (debriefPanel != null)
            {
                debriefPanel.SetActive(false);
            }

            SetupInitialBriefing();
        }

        private void OnDestroy()
        {
            if (hintSystem != null) hintSystem.OnHintUpdated -= UpdateHintUI;
            if (assessmentManager != null) assessmentManager.OnAssessmentFinalized -= DisplayDebriefScorecard;
            if (missionManager != null) missionManager.OnStepTransitioned -= HandleStepTransitionUI;
        }

        private void SetupInitialBriefing()
        {
            if (briefingTitleText != null)
            {
                briefingTitleText.text = "CBRN INCIDENT BRIEFING (PS 269)";
            }

            if (briefingBodyText != null)
            {
                briefingBodyText.text =
                    "INCIDENT: Industrial Chemical Leak at Storage Sector 4.\n" +
                    "HAZARD: Unconfirmed toxic vapor plume expanding from ruptured flange.\n" +
                    "VICTIMS: 1 plant technician trapped in Hot Zone.\n\n" +
                    "ACTION PLAN:\n" +
                    "1. Confirm Hazard Identity via tank UN Placard.\n" +
                    "2. Don appropriate Level A vapor-tight PPE at Staging Locker.\n" +
                    "3. Isolate leak valve or perform emergency casualty extraction to Triage.\n\n" +
                    "Look at this board or tap to acknowledge and begin.";
            }
        }

        private void UpdateHintUI(string message, bool isUrgent)
        {
            if (hintText != null)
            {
                hintText.text = message;
            }

            if (hintBackground != null)
            {
                hintBackground.color = isUrgent ? urgentHintColor : normalHintColor;
            }
        }

        private void HandleStepTransitionUI(MissionStep step, ScenarioStepData data)
        {
            // Update HUD or sound cues
            Debug.Log($"[UIManager] UI updated for step: {step}");
        }

        private void DisplayDebriefScorecard(int score, bool passed, string summary)
        {
            if (debriefPanel != null)
            {
                debriefPanel.SetActive(true);
            }

            if (debriefScoreText != null)
            {
                debriefScoreText.text = $"SCORE: {score}/100";
                debriefScoreText.color = passed ? Color.green : Color.red;
            }

            if (debriefStatusText != null)
            {
                debriefStatusText.text = passed ? "EVALUATION: PASS (Simulation Criteria)" : "EVALUATION: NEEDS RETRAINING";
            }

            if (debriefMistakesSummaryText != null && assessmentManager != null)
            {
                int mistakeCount = assessmentManager.Mistakes.Count;
                string text = $"Total Safety Violations: {mistakeCount}\n";
                for (int i = 0; i < Mathf.Min(3, mistakeCount); i++)
                {
                    text += $"- {assessmentManager.Mistakes[i].description} (-{assessmentManager.Mistakes[i].penaltyPoints} pts)\n";
                }
                debriefMistakesSummaryText.text = text;
            }

            if (debriefDisclaimerText != null)
            {
                debriefDisclaimerText.text =
                    "NOTICE: Educational training simulation prototype for NDRF / MHA Disaster Response (Problem Statement 269).\n" +
                    "Scores do NOT constitute operational certification or proof of real-world qualification.";
            }
        }
    }
}
