using UnityEngine;

namespace CBRN.Assessment
{
    /// <summary>
    /// Configurable data container defining the scoring weights and penalty thresholds.
    /// Educational training prototype only: scores reflect simulation adherence, not real-world certification.
    /// </summary>
    [CreateAssetMenu(fileName = "ScoringRubric_", menuName = "CBRN/Data/Scoring Rubric", order = 4)]
    public class ScoringRubricData : ScriptableObject
    {
        [Header("Base Scores")]
        [Tooltip("Maximum achievable score in the simulation.")]
        public int maxPossibleScore = 100;

        [Tooltip("Minimum passing score threshold.")]
        public int passingScore = 70;

        [Header("Penalties")]
        [Tooltip("Points deducted for entering Hot Zone with inadequate or missing PPE.")]
        public int wrongPPEPenalty = 30;

        [Tooltip("Points deducted for attempting rescue before shutting off or containing the leak.")]
        public int prematureRescuePenalty = 15;

        [Tooltip("Points deducted per safety zone boundary violation.")]
        public int zoneViolationPenalty = 10;

        [Tooltip("Points deducted per 10 seconds exceeded beyond target scenario duration.")]
        public int timeOverrunPenaltyPerBlock = 5;

        [Tooltip("Points deducted per hint requested by trainee.")]
        public int hintPenalty = 3;

        [Header("Mistake Limits")]
        [Tooltip("Maximum allowed critical mistakes before session is flagged as failed.")]
        public int maxAllowedCriticalMistakes = 3;
    }
}

