using System;
using UnityEngine;
using UnityEngine.AI;
using CBRN.Interactions;

namespace CBRN.AI
{
    /// <summary>
    /// NavMesh-based AI State Machine for disaster victims.
    /// States: Panic -> Following (when triaged by trainee) -> Safe (when evacuated to triage zone).
    /// Robust against unbaked NavMesh and ensures kinematic collision detection.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
    public class VictimAI : MonoBehaviour, IGazeInteractable
    {
        [Header("State Machine")]
        [Tooltip("Current operational state of this victim.")]
        [SerializeField] private VictimState currentState = VictimState.Panic_Agitated;

        [Header("Navigation Parameters")]
        [Tooltip("Follow distance to maintain behind trainee during evacuation.")]
        [SerializeField] private float followStoppingDistance = 2.0f;

        [Tooltip("Panic wander radius around initial position.")]
        [SerializeField] private float panicRadius = 3.5f;

        [Tooltip("Interval in seconds between random wander steps in panic state.")]
        [SerializeField] private float panicStepInterval = 2.5f;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer victimRenderer;
        [SerializeField] private Color panicColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color followingColor = new Color(0.9f, 0.7f, 0.1f);
        [SerializeField] private Color safeColor = new Color(0.2f, 0.8f, 0.3f);

        // Events
        public event Action<VictimAI> OnVictimTriaged;
        public event Action<VictimAI> OnVictimEvacuated;

        private NavMeshAgent _agent;
        private Transform _playerTransform;
        private Vector3 _initialPosition;
        private float _panicTimer = 0f;
        private Material _instancedMat;

        public VictimState CurrentState => currentState;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _initialPosition = transform.position;

            // Ensure a kinematic Rigidbody exists so trigger collisions with EvacuationSystem fire reliably
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;

            if (victimRenderer == null) victimRenderer = GetComponent<Renderer>();
            if (victimRenderer != null)
            {
                _instancedMat = victimRenderer.material;
            }

            if (Camera.main != null)
            {
                _playerTransform = Camera.main.transform.parent != null ? Camera.main.transform.parent : Camera.main.transform;
            }
        }

        private void Start()
        {
            SetState(currentState);
        }

        private void Update()
        {
            switch (currentState)
            {
                case VictimState.Panic_Agitated:
                    UpdatePanicState();
                    break;
                case VictimState.Following_Hero:
                    UpdateFollowingState();
                    break;
                case VictimState.Safe_Evacuated:
                    // Stationed safely at triage
                    break;
            }
        }

        private void UpdatePanicState()
        {
            if (_agent == null || !_agent.isOnNavMesh) return;

            _panicTimer += Time.deltaTime;
            if (_panicTimer >= panicStepInterval)
            {
                _panicTimer = 0f;
                Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * panicRadius;
                randomDirection += _initialPosition;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, panicRadius, NavMesh.AllAreas))
                {
                    _agent.speed = 1.8f;
                    _agent.SetDestination(hit.position);
                }
            }
        }

        private void UpdateFollowingState()
        {
            if (_playerTransform == null && Camera.main != null)
            {
                _playerTransform = Camera.main.transform.parent != null ? Camera.main.transform.parent : Camera.main.transform;
            }

            if (_playerTransform != null && _agent != null && _agent.isOnNavMesh)
            {
                _agent.stoppingDistance = followStoppingDistance;
                _agent.speed = 2.4f;
                _agent.SetDestination(_playerTransform.position);
            }
        }

        public void SetState(VictimState newState)
        {
            currentState = newState;

            Color targetColor = currentState switch
            {
                VictimState.Panic_Agitated => panicColor,
                VictimState.Following_Hero => followingColor,
                VictimState.Safe_Evacuated => safeColor,
                _ => Color.white
            };

            if (_instancedMat != null)
            {
                _instancedMat.color = targetColor;
            }

            if (currentState == VictimState.Safe_Evacuated)
            {
                if (_agent != null && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                    _agent.ResetPath();
                }
                OnVictimEvacuated?.Invoke(this);
            }
        }

        public void MarkSafe()
        {
            if (currentState != VictimState.Safe_Evacuated)
            {
                Debug.Log($"[VictimAI] {gameObject.name} successfully evacuated to triage safe point!");
                SetState(VictimState.Safe_Evacuated);
            }
        }

        // IGazeInteractable Implementation
        public void OnGazeEnter()
        {
            if (currentState == VictimState.Panic_Agitated && _instancedMat != null)
            {
                _instancedMat.color = Color.yellow;
            }
        }

        public void OnGazeStay(float normalizedProgress) { }

        public void OnGazeExit()
        {
            SetState(currentState); // Restore state color
        }

        public void OnGazeTrigger()
        {
            if (currentState == VictimState.Panic_Agitated || currentState == VictimState.Idle_Injured)
            {
                Debug.Log($"[VictimAI] Trainee triaged and calmed {gameObject.name}! Transitioning to Following.");
                SetState(VictimState.Following_Hero);
                OnVictimTriaged?.Invoke(this);
            }
        }

        private void OnDestroy()
        {
            if (_instancedMat != null) Destroy(_instancedMat);
        }
    }
}
