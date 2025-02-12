#if UNITY_EDITOR
using UnityEditor;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIWorldImage
{
    [UnityEditor.CustomEditor(typeof(WorldImage))]
    public class WorldImageEditor : UnityEditor.Editor
    {
        WorldImage m_image;

        SerializedProperty m_useRenderTextureProp;
        int m_lastCameraCullingMask = -1;
        bool m_debugFoldout = false;

        public void OnEnable()
        {
			m_useRenderTextureProp = serializedObject.FindProperty("m_useRenderTexture");
            m_image = target as WorldImage;
            Undo.undoRedoPerformed += onUndoRedo;
        }

        public void OnDisable()
        {
            Undo.undoRedoPerformed -= onUndoRedo;
        }

        private void onUndoRedo()
        {
            m_image?.ForceRenderTextureUpdate();
        }

        public override void OnInspectorGUI()
        {
            var oldFieldOfView = m_image.CameraFieldOfView;
            var oldBackgroundColor = m_image.CameraBackgroundColor;
            var oldCameraClearType = m_image.CameraClearType;
            var oldCameraLookAtPosition = m_image.CameraLookAtPosition;
            var oldCameraOffset = m_image.CameraOffset;
            var oldDepth = m_image.CameraDepth;
            var oldResolutionWidth = m_image.ResolutionWidth;
            var oldResolutionHeight = m_image.ResolutionHeight;
            var oldCameraUseBoundsToClip = m_image.CameraUseBoundsToClip;
            var oldUseRenderTexture = m_useRenderTextureProp != null ? m_useRenderTextureProp.boolValue : m_image.UseRenderTexture;
            var oldRenderTextureOverride = m_image.RenderTextureOverride;
            // DO NOT use GetWorldObjectAt() here since that defrags (removes all NULLs) from the list and thus
            // makes the (+) button useless because null entries are instantly removed.
            var oldFirstWorldObject = m_image.GetFirstActiveWorldObject();
            var oldNearClipValue = m_image.CameraNearClipPlane;
            var oldFarClipValue = m_image.CameraFarClipPlane;
            var oldCameraOffsetAndPositionMultiplier = m_image.CameraOffsetAndPositionMultiplier;

            base.OnInspectorGUI();

            bool canDisableRenderTexture = m_image.GetRenderMode() == RenderMode.ScreenSpaceCamera;
            if (!canDisableRenderTexture && !m_image._useRenderTextureRawEditor)
            {
                EditorGUILayout.HelpBox("Disabling render textures only works for SCREEN SPACE CAMERA canvases.", MessageType.Error);
            }

#if KAMGAM_RENDER_PIPELINE_URP || KAMGAM_RENDER_PIPELINE_HDRP
            if (!m_image.UseRenderTexture)
                EditorGUILayout.HelpBox("In URP and HDRP transparent backgrounds are not supported for camera stacking.", MessageType.Info);
#endif

            serializedObject.ApplyModifiedProperties();

            if (!m_image.TryGetComponent<PrefabInstantiatorForWorldImage>(out var instantiator))
            {
                GUILayout.Space(10);
                GUILayout.Label("Prefabs");
                if (GUILayout.Button("Add Prefab Instantiator"))
                {
                    m_image.gameObject.AddComponent<PrefabInstantiatorForWorldImage>();
                }
            }

            // Debugging
            GUILayout.Space(10);
            m_debugFoldout = EditorGUILayout.Foldout(m_debugFoldout, "Debug");
            if (m_debugFoldout)
            {

                if (GUILayout.Button("Force Render Texture Update"))
                {
                    m_image.ForceRenderTextureUpdate();
                }

                if (GUILayout.Button("Update Bounds"))
                {
                    m_image.UpdateWorldObjectBounds();
                }

                if (GUILayout.Button("Apply Cullsing Mask"))
                {
                    m_image.ApplyCameraCullingMask();
                }

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Instance ID: " + m_image.GetInstanceID());
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (m_image.HasTmpRenderTexture)
                    GUILayout.Label("RT: " + m_image.RenderTexture.name);
                else
                    GUILayout.Label("RT: NULL");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (m_image.CameraFollowBoundsCenter && m_image.HasBounds)
                    GUILayout.Label("Following: Bounds center");
                else if (m_image.HasActiveWorldObjects())
                    GUILayout.Label("Following: " + m_image.GetWorldObjectAt(0).name);
                else
                    GUILayout.Label("Following: none (using LookAt as poistion)");
                GUILayout.EndHorizontal();

                GUI.enabled = m_image.UseRenderTexture;
                if (m_image.UseRenderTexture)
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    EditorGUI.DrawPreviewTexture(new Rect(rect.xMin, rect.yMax, EditorGUIUtility.currentViewWidth * 0.5f - 30, EditorGUIUtility.currentViewWidth * 0.5f - 30), m_image.RenderTexture);
					
					EditorGUI.DrawTextureAlpha(new Rect(rect.xMin + EditorGUIUtility.currentViewWidth * 0.5f - 30, rect.yMax, EditorGUIUtility.currentViewWidth * 0.5f - 30, EditorGUIUtility.currentViewWidth * 0.5f - 30), m_image.RenderTexture);
					
                    GUILayout.Space(EditorGUIUtility.currentViewWidth * 0.5f - 20);

                    // Force a redraw
                    Repaint();
                }
                GUI.enabled = true;
            }

            if (oldFieldOfView != m_image.CameraFieldOfView)
            {
                m_image.ApplyCameraFieldOfView();
            }

            if (oldFieldOfView != m_image.CameraFieldOfView)
            {
                m_image.ApplyCameraFieldOfView();
            }

            if (oldBackgroundColor != m_image.CameraBackgroundColor)
            {
                m_image.ApplyCameraBackgroundColor();
            }
            
            if (oldCameraClearType != m_image.CameraClearType)
            {
                m_image.ApplyCameraClearType();
            }

            if (oldCameraLookAtPosition != m_image.CameraLookAtPosition)
            {
                m_image.ApplyCameraLookAtPosition();
            }

            if (oldCameraOffset != m_image.CameraOffset)
            {
                m_image.ApplyCameraOffset();
            }

            if (oldDepth != m_image.CameraDepth)
            {
                m_image.ApplyCameraDepth();
            }

            if (m_lastCameraCullingMask != m_image.CameraCullingMask)
            {
                m_image.ApplyCameraCullingMask();
            }

            if (oldResolutionWidth != m_image.ResolutionWidth)
            {
                m_image.ApplyResolutionWidth();
            }

            if (oldResolutionHeight != m_image.ResolutionHeight)
            {
                m_image.ApplyResolutionHeight();
            }

            if (oldCameraUseBoundsToClip != m_image.CameraUseBoundsToClip)
            {
                m_image.UpdateWorldObjectBounds();
                m_image.ObjectCamera.UpdateCameraClippingFromBounds();
            }

            if (m_useRenderTextureProp != null)
            {
                if (oldUseRenderTexture != m_useRenderTextureProp.boolValue)
                {
                    m_image.ApplyUseRenderTexture();
                }
            }
            
            if (oldRenderTextureOverride != m_image.RenderTextureOverride)
            {
                m_image.ForceRenderTextureUpdate();
            }

            if (oldFirstWorldObject != m_image.GetFirstActiveWorldObject())
            {
                m_image.UpdateWorldObjectBounds();
            }

            if (oldNearClipValue != m_image.CameraNearClipPlane)
            {
                m_image.ApplyCameraNearClipPlane();
            }

            if (oldFarClipValue != m_image.CameraFarClipPlane)
            {
                m_image.ApplyCameraFarClipPlane();
            }

            if (!Mathf.Approximately(oldCameraOffsetAndPositionMultiplier,m_image.CameraOffsetAndPositionMultiplier))
            {
                m_image.ApplyCameraOffsetAndPositionMultiplier(oldCameraOffsetAndPositionMultiplier);
            }

            serializedObject.Update();

            m_lastCameraCullingMask = m_image.CameraCullingMask;
        }
    }
}
#endif