#if UNITY_EDITOR
using UnityEditor;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIWorldImage
{
    [UnityEditor.CustomEditor(typeof(WorldObjectCamera))]
    public class WorldObjectCameraEditor : UnityEditor.Editor
    {
        WorldObjectCamera m_camera;

        public void OnEnable()
        {
            m_camera = target as WorldObjectCamera;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This camera object is a temporary object.\n\n" +
                "It contains the camera that is used to render the world objects." +
                " You will see one of these for each World Image in your UI.\n\n" +
                "It is not saved in your scene beause it is generated on the fly whenever it is needed." +
                " That's also why you can not edit it since none of your" +
                " changes would persist.\n\n" +
                "If you want to edit the camera properties then please use the OnCameraCreated event on the World Image component.", MessageType.Info);

            // base.OnInspectorGUI();
        }
    }
}
#endif