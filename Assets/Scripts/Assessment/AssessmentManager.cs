using System;
using System.Collections.Generic;
using UnityEngine;
using CBRN.Core;

namespace CBRN.Assessment
{
    [Serializable]
    public class StepAssessmentLog
    {
        public int step_number;
        public string step_name;
        public float duration_seconds;
        public int mistakes_count;
        public string status; // "COMPLETED" or "FAILED"
    }

    [Serializable]
    public class MistakeRecord
    {
        public string description;
        public int penaltyPoints;
        public float timestamp;
    }

    /// <summary>
    /// Tracks trainee mistakes, step timings, and calculates final score against ScriptableObject rubric.
    /// Prepares session payload for the Node.js REST API.
    /// Note: Educational training prototype only. Scores do not represent proof of real-world qualification.
    /// </summary>
    public class AssessmentManager : MonoBehaviour
    {
        [Header("Scoring Rubric")]
        [SerializeField] private ScoringRubricData rubric;

        [Header("Active Session Metrics")]
        [SerializeField] private int totalScore = 100;
        [SerializeField] private List<MistakeRecord> mistakes = new List<MistakeRecord>();
        [SerializeField] private List<StepAssessmentLog> stepLogs = new List<StepAssessmentLog>();

        // Events
        public event Action<string, int> OnMistakeRecorded;
        public event Action<int, bool, string> OnAssessmentFinalized; // finalScore, isPassed, summary

        private float _sessionStartTime;
        private float _currentStepStartTime;
        private int _currentStepMistakeCount;
        private bool _isFinalized = false;

        public int TotalScore => totalScore;
        public int MistakeCount => mistakes.Count;
        public IReadOnlyList<MistakeRecord> Mistakes => mistakes;
        public IReadOnlyList<StepAssessmentLog> StepLogs => stepLogs;
        public float SessionDuration => Time.time - _sessionStartTime;

        private void Awake()
        {
            _sessionStartTime = Time.time;
            _currentStepStartTime = Time.time;

            if (rubric != null)
            {
                totalScore = rubric.maxPossibleScore;
            }
        }

        public void RecordMistake(string description, int penaltyPoints)
        {
            if (_isFinalized) return;

            MistakeRecord mistake = new MistakeRecord
            {
                description = description,
                penaltyPoints = penaltyPoints,
                timestamp = Time.time - _sessionStartTime
            };

            mistakes.Add(mistake);
            _currentStepMistakeCount++;

            totalScore = Mathf.Max(0, totalScore - penaltyPoints);
            Debug.LogWarning($"[AssessmentManager] Mistake Logged: {description} (-{penaltyPoints} pts). Current Score: {totalScore}");

            OnMistakeRecorded?.Invoke(description, penaltyPoints);
        }

        public void FinalizeStepLog(int stepNumber, string stepName, bool isCompleted)
        {
            float stepDuration = Time.time - _currentStepStartTime;

            StepAssessmentLog log = new StepAssessmentLog
            {
                step_number = stepNumber,
                step_name = stepName,
                duration_seconds = Mathf.Round(stepDuration * 10f) / 10f,
                mistakes_count = _currentStepMistakeCount,
                status = isCompleted ? "COMPLETED" : "FAILED"
            };

            stepLogs.Add(log);

            // Reset step tracker
            _currentStepStartTime = Time.time;
            _currentStepMistakeCount = 0;
        }

        public void FinalizeAssessment()
        {
            if (_isFinalized) return;

            // Evaluate total time overrun penalty against target 360s simulation window
            float totalDuration = SessionDuration;
            const float targetScenarioWindow = 360f;
            if (rubric != null && totalDuration > targetScenarioWindow)
            {
                int overrunBlocks = Mathf.FloorToInt((totalDuration - targetScenarioWindow) / 10f);
                if (overrunBlocks > 0)
                {
                    int timePenalty = overrunBlocks * rubric.timeOverrunPenaltyPerBlock;
                    RecordMistake($"Time Overrun: Exceeded 6-minute operational window by {(int)(totalDuration - targetScenarioWindow)}s.", timePenalty);
                }
            }

            _isFinalized = true;

            int passingThreshold = rubric != null ? rubric.passingScore : 70;
            int maxCriticalMistakes = rubric != null ? rubric.maxAllowedCriticalMistakes : 3;

            bool isPassed = (totalScore >= passingThreshold) && (mistakes.Count <= maxCriticalMistakes);

            string summary = $"Final Score: {totalScore}/100. Mistakes: {mistakes.Count}. Status: {(isPassed ? "PASSED (Simulation Criteria)" : "NEEDS RETRAINING")}";
            Debug.Log($"[AssessmentManager] {summary}");

            OnAssessmentFinalized?.Invoke(totalScore, isPassed, summary);
        }
    }
}

