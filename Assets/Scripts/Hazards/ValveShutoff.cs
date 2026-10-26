using System;
using UnityEngine;
using CBRN.Interactions;

namespace CBRN.Hazards
{
    /// <summary>
    /// Interactive valve wheel at the pipe rupture site.
    /// Trainee interacts via gaze-dwell or tap to shut off the chemical flow.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ValveShutoff : MonoBehaviour, IGazeInteractable
    {
        [Header("System Linkage")]
        [Tooltip("The hazard system to contain when closed.")]
        [SerializeField] private HazardSystem targetHazard;

        [Header("Visual Feedback")]
        [SerializeField] private Renderer valveRenderer;
        [SerializeField] private Color openColor = Color.red;
        [SerializeField] private Color closedColor = Color.green;

        // Events
        public static event Action OnValveShutoff;

        private bool _isClosed = false;
        private Material _instancedMat;

        public bool IsClosed => _isClosed;

        private void Awake()
        {
            if (targetHazard == null) targetHazard = FindObjectOfType<HazardSystem>();

            if (valveRenderer == null) valveRenderer = GetComponent<Renderer>();
            if (valveRenderer != null)
            {
                _instancedMat = valveRenderer.material;
                _instancedMat.color = openColor;
            }
        }

        public void OnGazeEnter()
        {
            if (!_isClosed && _instancedMat != null)
            {
                _instancedMat.color = Color.yellow;
            }
        }

        public void OnGazeStay(float normalizedProgress)
        {
            // Spin the valve slightly as dwell timer progresses
            if (!_isClosed)
            {
                transform.Rotate(Vector3.forward * (300f * Time.deltaTime));
            }
        }

        public void OnGazeExit()
        {
            if (_instancedMat != null)
            {
                _instancedMat.color = _isClosed ? closedColor : openColor;
            }
        }

        public void OnGazeTrigger()
        {
            if (_isClosed) return;

            _isClosed = true;
            if (_instancedMat != null) _instancedMat.color = closedColor;

            // Trigger containment on the hazard simulation
            if (targetHazard != null)
            {
                targetHazard.ContainLeak();
            }

            Debug.Log("[ValveShutoff] Trainee isolated the chemical leak!");
            OnValveShutoff?.Invoke();
        }

        private void OnDestroy()
        {
            if (_instancedMat != null) Destroy(_instancedMat);
        }
    }
}
