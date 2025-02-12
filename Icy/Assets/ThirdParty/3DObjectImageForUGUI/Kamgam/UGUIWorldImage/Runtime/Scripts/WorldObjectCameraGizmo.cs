#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kamgam.UGUIWorldImage
{
	public static class WorldObjectCameraGizmo
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.NotInSelectionHierarchy)]
        static void DrawGizmosSelected(WorldObjectCamera cam, GizmoType type)
		{
            var lookDirection = Camera.current.transform.position - cam.transform.position;
            Gizmos.DrawIcon(cam.transform.position + lookDirection * 0.1f, "Kamgam/UGUIWorldImage/WorldImage gizmo.tiff", true);
        }
    }
}
#endif