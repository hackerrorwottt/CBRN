using UnityEngine;

namespace CBRN.Core
{
    /// <summary>
    /// Configurable data container defining an individual scenario step.
    /// Used by MissionManager to drive scenario progression without hardcoded step sequences.
    /// </summary>
    [CreateAssetMenu(fileName = "ScenarioStep_", menuName = "CBRN/Data/Scenario Step", order = 3)]
    public class ScenarioStepData : ScriptableObject
    {
        [Header("Step Information")]
        [Tooltip("Sequential index of this step (1 to 7).")]
        public int stepIndex = 1;

        [Tooltip("Short display title.")]
        public string stepTitle = "1. Briefing & Ingestion";

        [Tooltip("Actionable objective shown to trainee on the virtual instructor panel.")]
        [TextArea(2, 4)]
        public string objectiveText = "Review incident parameters at the Briefing Board in the Cold Zone.";

        [Tooltip("Hint provided if the trainee hesitates or commits an error during this step.")]
        [TextArea(2, 3)]
        public string standardHint = "Approach the large blue briefing board and inspect the facility layout.";

        [Header("Scoring & Completion")]
        [Tooltip("Max base score points awarded for executing this step cleanly.")]
        public int basePoints = 15;

        [Tooltip("Time limit in seconds before time penalty applies (0 = untimed).")]
        public float targetTimeSeconds = 60f;
    }
}

