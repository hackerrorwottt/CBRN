using UnityEngine;
using CBRN.Assessment;
using CBRN.PPE;
using CBRN.Interactions;

namespace CBRN.Environment
{
    /// <summary>
    /// Identifies and visually marks operational zone boundaries in the Scene view using Gizmos.
    /// Detects when the trainee enters this zone volume and notifies the SafetyZoneMonitor.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ZoneIdentifier : MonoBehaviour
    {
        [Header("Zone Configuration")]
        [Tooltip("The CBRN operational classification for this area.")]
        [SerializeField] private ZoneType zoneType = ZoneType.ColdZone_Staging;

        [Header("Gizmo Visualization")]
        [Tooltip("Alpha transparency of the editor debug volume.")]
        [Range(0.1f, 0.8f)]
        [SerializeField] private float debugAlpha = 0.25f;

        public ZoneType CurrentZoneType => zoneType;

        private void OnTriggerEnter(Collider other)
        {
            // Verify if the entering entity is the trainee
            if (other.GetComponentInParent<PlayerPPEState>() != null ||
                other.GetComponentInParent<GazeInteractor>() != null ||
                other.CompareTag("MainCamera"))
            {
                if (SafetyZoneMonitor.Instance != null)
                {
                    SafetyZoneMonitor.Instance.OnZoneBoundaryCrossed(zoneType);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col == null) return;

            // Assign distinct color codes according to standard disaster response protocols
            Color gizmoColor = zoneType switch
            {
                ZoneType.ColdZone_Staging => Color.blue,
                ZoneType.WarmZone_Perimeter => Color.yellow,
                ZoneType.HotZone_ChemicalLeak => Color.red,
                ZoneType.Evacuation_TriagePoint => Color.green,
                _ => Color.white
            };

            gizmoColor.a = debugAlpha;
            Gizmos.color = gizmoColor;

            // Draw wireframe and solid bounding box in Scene view
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
                gizmoColor.a = 0.8f;
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
    }
}
