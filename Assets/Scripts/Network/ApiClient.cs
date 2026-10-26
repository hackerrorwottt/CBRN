using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using CBRN.Assessment;
using CBRN.Hazards;

namespace CBRN.Network
{
    /// <summary>
    /// Communicates with the Node.js 20 + Express + MySQL 8 REST backend.
    /// Handles JWT authentication, session saving with atomic step logs,
    /// dynamic adaptive difficulty synchronization, and offline retry queue.
    /// </summary>
    public class ApiClient : MonoBehaviour
    {
        [Header("Backend Connection")]
        [Tooltip("Base REST API URL (use computer local IP e.g. http://192.168.1.50:3000/api when testing on mobile phone over Wi-Fi).")]
        [SerializeField] private string baseUrl = "http://127.0.0.1:3000/api";

        [Header("Active Session Profile")]
        [SerializeField] private int currentUserId = 1;
        [SerializeField] private int currentScenarioId = 1;

        [Header("Dependencies")]
        [SerializeField] private OfflineQueue offlineQueue;
        [SerializeField] private AssessmentManager assessmentManager;
        [SerializeField] private HazardSystem hazardSystem;

        private string _jwtToken = string.Empty;

        public bool IsAuthenticated => !string.IsNullOrEmpty(_jwtToken);

        private void Awake()
        {
            if (offlineQueue == null) offlineQueue = GetComponent<OfflineQueue>() ?? gameObject.AddComponent<OfflineQueue>();
            if (assessmentManager == null) assessmentManager = FindObjectOfType<AssessmentManager>();
            if (hazardSystem == null) hazardSystem = FindObjectOfType<HazardSystem>();
        }

        private void Start()
        {
            if (assessmentManager != null)
            {
                assessmentManager.OnAssessmentFinalized += HandleAssessmentFinalized;
            }

            // Sync adaptive difficulty on scenario launch
            StartCoroutine(FetchAdaptiveDifficultyRoutine(currentUserId));

            // Flush any offline cached sessions if connection is up
            StartCoroutine(FlushOfflineQueueRoutine());
        }

        private void OnDestroy()
        {
            if (assessmentManager != null)
            {
                assessmentManager.OnAssessmentFinalized -= HandleAssessmentFinalized;
            }
        }

        public void SetAuthToken(string token)
        {
            _jwtToken = token;
        }

        /// <summary>
        /// Authenticates trainee or instructor against /api/auth/login.
        /// </summary>
        public IEnumerator LoginRoutine(string email, string password, Action<bool, string> onComplete)
        {
            string url = $"{baseUrl}/auth/login";
            LoginRequest req = new LoginRequest { email = email, password = password };
            string bodyJson = JsonUtility.ToJson(req);

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    LoginResponse res = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
                    _jwtToken = res.token;
                    currentUserId = res.user.id;
                    Debug.Log($"[ApiClient] Authenticated as {res.user.username} ({res.user.role})");
                    onComplete?.Invoke(true, res.token);
                }
                else
                {
                    Debug.LogWarning($"[ApiClient] Login failed: {www.error}");
                    onComplete?.Invoke(false, www.error);
                }
            }
        }

        /// <summary>
        /// Queries the trainee's historical performance (last 5 sessions) to tune simulation hazard speed.
        /// </summary>
        public IEnumerator FetchAdaptiveDifficultyRoutine(int userId)
        {
            string url = $"{baseUrl}/sessions/adaptive-difficulty/{userId}";

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(_jwtToken))
                {
                    www.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
                }

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    DifficultyResponse res = JsonUtility.FromJson<DifficultyResponse>(www.downloadHandler.text);
                    Debug.Log($"[ApiClient] Adaptive difficulty retrieved: {res.adaptive_difficulty_multiplier:F2}x (Avg mistakes: {res.average_mistakes_last_5:F1})");

                    if (hazardSystem != null)
                    {
                        hazardSystem.SetDifficultyMultiplier(res.adaptive_difficulty_multiplier);
                    }
                }
                else
                {
                    Debug.Log($"[ApiClient] Could not fetch adaptive difficulty (using baseline 1.0x). Reason: {www.error}");
                }
            }
        }

        private void HandleAssessmentFinalized(int finalScore, bool passed, string summary)
        {
            if (assessmentManager == null) return;

            // Assemble session payload
            SessionUploadPayload payload = new SessionUploadPayload
            {
                user_id = currentUserId,
                scenario_id = currentScenarioId,
                score = finalScore,
                total_duration_seconds = Mathf.Round(assessmentManager.SessionDuration * 10f) / 10f,
                total_mistakes = assessmentManager.MistakeCount,
                status = passed ? "COMPLETED" : "FAILED",
                step_logs = new System.Collections.Generic.List<StepLogPayload>()
            };

            foreach (var step in assessmentManager.StepLogs)
            {
                payload.step_logs.Add(new StepLogPayload
                {
                    step_number = step.step_number,
                    step_name = step.step_name,
                    duration_seconds = step.duration_seconds,
                    mistakes_count = step.mistakes_count,
                    status = step.status
                });
            }

            StartCoroutine(SubmitSessionRoutine(payload));
        }

        public IEnumerator SubmitSessionRoutine(SessionUploadPayload payload)
        {
            string url = $"{baseUrl}/sessions";
            string bodyJson = JsonUtility.ToJson(payload);

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(_jwtToken))
                {
                    www.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
                }

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[ApiClient] Session successfully saved to MySQL database! Response: {www.downloadHandler.text}");
                }
                else
                {
                    Debug.LogWarning($"[ApiClient] Server unreachable ({www.error}). Saving session locally to OfflineQueue.");
                    offlineQueue?.Enqueue(payload);
                }
            }
        }

        private IEnumerator FlushOfflineQueueRoutine()
        {
            if (offlineQueue == null) yield break;

            var pending = offlineQueue.GetPendingPayloads();
            if (pending.Count == 0) yield break;

            Debug.Log($"[ApiClient] Attempting to flush {pending.Count} offline sessions to server...");

            for (int i = 0; i < pending.Count; i++)
            {
                yield return SubmitSessionRoutine(pending[i]);
            }

            offlineQueue.ClearPending();
        }
    }
}
