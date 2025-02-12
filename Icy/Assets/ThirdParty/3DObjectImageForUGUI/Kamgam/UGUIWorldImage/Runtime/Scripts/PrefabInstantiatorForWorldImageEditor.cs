#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.UGUIWorldImage
{
    [UnityEditor.CustomEditor(typeof(PrefabInstantiatorForWorldImage))]
    public class PrefabInstantiatorForWorldImageEditor : UnityEditor.Editor
    {
        PrefabInstantiatorForWorldImage m_instantiator;
        bool m_createButtonsFoldout = true;

        public void OnEnable()
        {
            m_instantiator = target as PrefabInstantiatorForWorldImage;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create or Update ALL Instances"))
            {
                m_instantiator.CreateAndAddAllInstancesToImage(m_instantiator.WorldImage);
            }

            if (GUILayout.Button("Destroy ALL Instances"))
            {
                m_instantiator.RemoveAllFromImageAndDestroyInstances(m_instantiator.WorldImage);
            }

            m_createButtonsFoldout = EditorGUILayout.Foldout(m_createButtonsFoldout, new GUIContent("Toggle Instances"));
            if (m_createButtonsFoldout)
            {
                foreach (var prefabHandle in m_instantiator.Prefabs)
                {
                    if (prefabHandle.Prefab == null)
                        continue;

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Toggle " + prefabHandle.Prefab.name))
                    {
                        m_instantiator.ToogleOrCreate(prefabHandle, destroyOnDisable: true);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
#endif