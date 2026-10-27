using UnityEngine;

namespace CBRN.Interactions
{
    /// <summary>
    /// Performs raycasting from the VR camera forward, detecting gaze targets,
    /// tracking dwell duration, handling Cardboard single taps, and notifying interactables.
    /// Uses trigger-filtering so invisible zone trigger volumes do not block interaction with physical targets.
    /// </summary>
    public class GazeInteractor : MonoBehaviour
    {
        [Header("Gaze Settings")]
        [Tooltip("Maximum distance in meters the trainee can interact with objects.")]
        [SerializeField] private float maxRayDistance = 25f;

        [Tooltip("LayerMask containing interactable objects (default everything).")]
        [SerializeField] private LayerMask interactableLayers = ~0;

        [Tooltip("Seconds required to look at an interactable before triggering automatically.")]
        [SerializeField] private float dwellDuration = 1.5f;

        [Header("Reticle Reference")]
        [Tooltip("Reference to the GazeReticle component.")]
        [SerializeField] private GazeReticle reticle;

        // Current state
        private IGazeInteractable _currentInteractable;
        private float _dwellTimer = 0f;
        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;

            if (reticle == null)
            {
                reticle = FindObjectOfType<GazeReticle>();
            }
        }

        private void Update()
        {
            HandleGazeRaycast();
            HandleTapInput();
        }

        private void HandleGazeRaycast()
        {
            if (_cam == null)
            {
                if (Camera.main != null) _cam = Camera.main;
                else return;
            }

            Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

            // RaycastAll allows us to look through invisible zone trigger volumes
            // (e.g. ZoneIdentifier boundary triggers) and find the actual interactive object behind them
            RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance, interactableLayers, QueryTriggerInteraction.Collide);

            RaycastHit bestHit = default;
            IGazeInteractable targetInteractable = null;
            float closestDistance = float.MaxValue;
            bool foundHit = false;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                // Check if this object or its parent is interactable
                IGazeInteractable interactable = hit.collider.GetComponentInParent<IGazeInteractable>();

                if (interactable != null)
                {
                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        bestHit = hit;
                        targetInteractable = interactable;
                        foundHit = true;
                    }
                }
                else if (!hit.collider.isTrigger)
                {
                    // Solid non-trigger obstacle (e.g. wall, floor, tank) that doesn't have an interactable
                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        bestHit = hit;
                        targetInteractable = null;
                        foundHit = true;
                    }
                }
            }

            // Update reticle spatial location to prevent stereoscopic eye strain
            if (reticle != null)
            {
                reticle.UpdatePosition(bestHit.point, foundHit);
            }

            if (targetInteractable != null)
            {
                if (targetInteractable == _currentInteractable)
                {
                    // Ongoing gaze: accumulate dwell time
                    _dwellTimer += Time.deltaTime;
                    float progress = Mathf.Clamp01(_dwellTimer / dwellDuration);

                    _currentInteractable.OnGazeStay(progress);
                    if (reticle != null) reticle.SetProgress(progress);

                    // Trigger when dwell completes
                    if (_dwellTimer >= dwellDuration)
                    {
                        TriggerCurrent();
                    }
                }
                else
                {
                    // Switched to a new interactable
                    ExitCurrent();
                    _currentInteractable = targetInteractable;
                    _dwellTimer = 0f;
                    _currentInteractable.OnGazeEnter();
                    if (reticle != null) reticle.ResetProgress();
                }
                return;
            }

            // Looking at empty space or non-interactable object
            ExitCurrent();
        }

        private void HandleTapInput()
        {
            // Detect Cardboard button click, screen tap, or left mouse click
            bool isTapped = Input.GetMouseButtonDown(0);

            if (!isTapped && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    isTapped = true;
                }
            }

            if (isTapped && _currentInteractable != null)
            {
                TriggerCurrent();
            }
        }

        private void TriggerCurrent()
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnGazeTrigger();
                _dwellTimer = 0f;
                if (reticle != null) reticle.ResetProgress();
            }
        }

        private void ExitCurrent()
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnGazeExit();
                _currentInteractable = null;
            }
            _dwellTimer = 0f;
            if (reticle != null) reticle.ResetProgress();
        }
    }
}
