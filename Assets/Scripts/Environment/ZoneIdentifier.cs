using UnityEngine;

namespace CBRN.Environment
{
    /// <summary>
    /// Identifies and visually marks operational zone boundaries in the Scene view using Gizmos.
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

