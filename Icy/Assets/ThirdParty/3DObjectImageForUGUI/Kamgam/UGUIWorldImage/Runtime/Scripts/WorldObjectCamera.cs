using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIWorldImage
{
    /// <summary>
    /// A temporary camera wrapper object. Used by the WorldImage to generate a texture for the image.<br />
    /// This manages the camera that is used to render into a render texture.<br />
    /// It applies all the camera properties to the actual camera object.<br />
    /// It also manages the differences between Built-In, URP and HDRP cameras.<br />
    /// This camera object will not be saved in the scene. It is regenerated on demand and self-destroys after deserialization.
    /// </summary>
    public partial class WorldObjectCamera : MonoBehaviour, ISerializationCallbackReceiver
    {
        public enum ClearType { Color, Sky, Depth }

        public event System.Action<WorldObjectCamera> OnPreDestroy;

        [System.NonSerialized]
        public WorldImage Image;

        [System.NonSerialized]
        public Camera Camera;

        [System.NonSerialized]
        public ClearType CameraClearType;

        [System.NonSerialized]
        public bool IsScheduledForDestruction = false;

        public static WorldObjectCamera CreateForImage(WorldImage image)
        {
            var go = new GameObject("[Temp] Camera (for World Image " + image.GetInstanceID() + ")");
            go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.NotEditable;

            var woCamera = go.AddComponent<WorldObjectCamera>();
            woCamera.Image = image;
            woCamera.Camera = go.AddComponent<Camera>();
            var cam = woCamera.Camera;

            // Copy properties from current rendering camera
            var currentCamera = getCurrentCamera();
            if (currentCamera != null)
            {
                cam.CopyFrom(currentCamera);
            }

            // Set camera up for render texture
            cam.nearClipPlane = image.CameraNearClipPlane;
            cam.farClipPlane = image.CameraFarClipPlane;
            cam.depth = image.CameraDepth;
            cam.fieldOfView = image.CameraFieldOfView;
            cam.cullingMask = image.CameraCullingMask;
            cam.orthographic = image.CameraOrthographic;
            cam.orthographicSize = image.CameraOrthographicSize;

            woCamera.SetCameraClearType(image.CameraClearType);
            woCamera.SetUseRenderTexture(image.UseRenderTexture);
            woCamera.SetBackgroundColor(image.CameraBackgroundColor);

            go.SetActive(image.gameObject.activeSelf);

            return woCamera;
        }

#if KAMGAM_RENDER_PIPELINE_HDRP
        protected UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData m_hdAdditionalCameraData;
        public UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData HDAdditionalCameraData
        {
            get
            {
                if (m_hdAdditionalCameraData == null)
                {
                    if (Camera != null)
                    {
                        m_hdAdditionalCameraData = Camera.gameObject.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();

                        // We add the data component because by default Unity adds this
                        // only on-demand, which in most cases it too late to set any values.
                        if (m_hdAdditionalCameraData == null)
                        {
                            m_hdAdditionalCameraData = Camera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        }
                    }
                }
                return m_hdAdditionalCameraData;
            }
        }
#endif

        public void SetCameraClearType(ClearType clearType)
        {
            if (clearType == ClearType.Color)
                Camera.clearFlags = CameraClearFlags.SolidColor;
            else if (clearType == ClearType.Sky)
                Camera.clearFlags = CameraClearFlags.Skybox;
            else
                Camera.clearFlags = CameraClearFlags.Depth;

#if KAMGAM_RENDER_PIPELINE_HDRP
            // Check if data is available because the user could have HDRP installed but not use it (have seen it happen) .
            if (HDAdditionalCameraData != null)
            {
                if (clearType == ClearType.Color)
                {
                    HDAdditionalCameraData.clearColorMode = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.ClearColorMode.Color;
                }
                else if (clearType == ClearType.Sky)
                {
                    HDAdditionalCameraData.clearColorMode = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.ClearColorMode.Sky;
                }
                else
                {
                    HDAdditionalCameraData.clearColorMode = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.ClearColorMode.None;
                }

                HDAdditionalCameraData.backgroundColorHDR = Image.CameraBackgroundColor;
                HDAdditionalCameraData.clearDepth = true;
            }
#endif
        }

        public void SetBackgroundColor(Color color)
        {
            Camera.backgroundColor = color;

#if KAMGAM_RENDER_PIPELINE_HDRP
            // Check if data is available because the user could have HDRP installed but not use it (have seen it happen) .
            if (HDAdditionalCameraData != null)
            {
                HDAdditionalCameraData.backgroundColorHDR = color;
                HDAdditionalCameraData.clearDepth = true;
            }
#endif
        }

        public void UpdateCameraClippingFromBounds()
        {
            if (Image.CameraUseBoundsToClip)
            {
                var bounds = Image.GetWorldObjectsBounds();
                if (bounds.HasValue)
                {
                    var center = getCenter();

                    Vector3 camPosition;
                    Vector3 lookAtPosition;
                    if (Image.CameraOrigin == null)
                    {
                        camPosition = center + Image.CameraLookAtPosition + Image.CameraOffset;
                        lookAtPosition = center + Image.CameraLookAtPosition;
                    }
                    else
                    {
                        camPosition = Image.CameraOrigin.position + Image.CameraLookAtPosition + Image.CameraOffset;
                        lookAtPosition = Image.CameraOrigin.position + Image.CameraLookAtPosition;
                    }
                    var camToBoundsCenter = bounds.Value.center - camPosition;

                    var extents = bounds.Value.extents;
                    var extentsManitude = extents.magnitude;
                    var distanceToBoundingSphere = camToBoundsCenter.magnitude - extentsManitude;
                    var camToBoundingSphereEdge = distanceToBoundingSphere * camToBoundsCenter.normalized;
                    var lookDirection = lookAtPosition - camPosition;
                    var camToBoundingSphereEdgeInLookDirection = Vector3.Project(camToBoundingSphereEdge, lookDirection);

                    if (distanceToBoundingSphere > 0f)
                    {
                        Camera.nearClipPlane = camToBoundingSphereEdgeInLookDirection.magnitude;
                        Camera.farClipPlane = camToBoundingSphereEdgeInLookDirection.magnitude + extents.magnitude * 2f;
                    }
                    else
                    {
                        Camera.nearClipPlane = 0.01f;
                        Camera.farClipPlane = extentsManitude * 2f;
                    }
                }
            }
            else
            {
                Camera.nearClipPlane = Image.CameraNearClipPlane;
                Camera.farClipPlane = Image.CameraFarClipPlane;
            }
        }

        private Vector3 getCenter()
        {
            Vector3 center;

            var bounds = Image.GetWorldObjectsBounds();
            if (Image.CameraFollowBoundsCenter && bounds.HasValue)
            {
                center = bounds.Value.center;
            }
            else if (Image.HasActiveWorldObjects())
            {
                center = Vector3.zero;
                var firstActive = Image.GetFirstActiveWorldObject();
                center = firstActive.position;
            }
            else
            {
                // We use the Camera Look At Offset as the default position for the camera
                // in case no world object is set.
                center = Image.CameraLookAtPosition;
            }

            return center;
        }

        static Camera getCurrentCamera()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (Camera.main != null)
                    return Camera.main;
                else
                    return Camera.current;
            }
            else
            {
                return Camera.main;
            }
#else
            if (Camera.main != null)
                return Camera.main;
            else
                return Camera.current;
#endif
        }

        public void SetUseRenderTexture(bool useRenderTexture)
        {
            if (useRenderTexture)
            {
                SetCameraClearType(Image.CameraClearType);
                Camera.rect = new Rect(0, 0, 1f, 1f);
            }
            else
            {
                SetCameraClearType(ClearType.Depth);

                // Match the overlay camera to the image rect in screen space.
                matchOverlayCameraToImageRect(Image);
            }
        }

        protected void matchOverlayCameraToImageRect(WorldImage image)
        {
            var cam = image.canvas.worldCamera;

            if (cam == null)
            {
                Camera.rect = new Rect(0, 0, 1f, 1f);
                return;
            }

            Camera.rect = rectToViewport(image.rectTransform, cam);
        }

        // The returned array of 4 vertices is clockwise.
        // It starts bottom left and rotates to top left, then top right,
        // and finally bottom right.
        protected Vector3[] m_worldCornersOfImageRectBuffer = new Vector3[4];

        protected Rect rectToViewport(RectTransform rectTransform, Camera cam)
        {
            rectTransform.GetWorldCorners(m_worldCornersOfImageRectBuffer);

            var bottomLeft = cam.WorldToViewportPoint(m_worldCornersOfImageRectBuffer[0]);
            var topRight = cam.WorldToViewportPoint(m_worldCornersOfImageRectBuffer[2]);

            var screenPos = new Rect(
                bottomLeft.x,
                bottomLeft.y,
                topRight.x - bottomLeft.x,
                topRight.y - bottomLeft.y);

            return screenPos;
        }

        public void UpdateTransform(bool cameraFollowTransform)
        {
            Vector3 center;
            if (Image.CameraOrigin == null)
                center = getCenter();
            else
                center = Image.CameraOrigin.position;

            if (Camera.orthographic != Image.CameraOrthographic)
                Camera.orthographic = Image.CameraOrthographic;

            if (Camera.orthographic)
                Camera.orthographicSize = Image.CameraOrthographicSize;

            var deltaToCenter = Image.CameraLookAtPosition + Image.CameraOffset;
            var lookAtPosition = Image.CameraLookAtPosition;
            var forward = Vector3.forward;
            var up = Vector3.up;

            // Take first first WorldObject rotation and scal into account
            if (cameraFollowTransform && Image.HasActiveWorldObjects() && Image.CameraOrigin == null)
            {
                var space = Image.GetFirstActiveWorldObject();
                deltaToCenter = space.TransformVector(deltaToCenter);
                lookAtPosition = space.TransformVector(lookAtPosition);
                forward = space.TransformVector(Vector3.forward);
                up = space.TransformVector(Vector3.up);
            }

            var localPosition = center + deltaToCenter;
            var localLookAtPosition = center + lookAtPosition;
            var localRoll = Image.CameraRoll;
            if(Image.CameraOffset.z > 0f)
            {
                localRoll += 180f;
            }

            transform.position = localPosition;
            var upRotation = Quaternion.FromToRotation(forward, localLookAtPosition - localPosition) * Quaternion.Euler(0f, 0f, -localRoll);
            transform.LookAt(localLookAtPosition, upRotation * up);

            if (!Image.UseRenderTexture)
                matchOverlayCameraToImageRect(Image);
        }

        public void OnDestroy()
        {
            OnPreDestroy?.Invoke(this);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            // Destroy camera if no longer needed. 
#if UNITY_EDITOR
            IsScheduledForDestruction = true;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && gameObject != null)
                {
                    OnPreDestroy?.Invoke(this);
                    GameObject.DestroyImmediate(gameObject);
                }
            };
#endif
        }
    }
}