using UnityEngine;

namespace BakedVolumetrics
{
    /// <summary>
    /// Marker component for fog exclusion zones.
    /// Add this to GameObjects to visualize them as exclusion areas in the editor.
    /// The actual exclusion is handled by FogExclusionManager - assign this object's
    /// Transform to the manager's exclusionVolumes array.
    /// </summary>
    [AddComponentMenu("Baked Volumetrics/Fog Exclusion Volume")]
    [ExecuteInEditMode]
    public class FogExclusionVolume : MonoBehaviour
    {
        [Header("Gizmo Settings")]
        [Tooltip("Color used to visualize this exclusion volume in the editor")]
        public Color gizmoColor = new Color(1f, 0.3f, 0.3f, 0.3f);

        [Tooltip("Whether to show the gizmo when the object is not selected")]
        public bool alwaysShowGizmo = true;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (alwaysShowGizmo)
                DrawGizmo(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmo(true);
        }

        private void DrawGizmo(bool selected)
        {
            Gizmos.color = selected ? new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.6f) : gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);

            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
#endif
    }
}
