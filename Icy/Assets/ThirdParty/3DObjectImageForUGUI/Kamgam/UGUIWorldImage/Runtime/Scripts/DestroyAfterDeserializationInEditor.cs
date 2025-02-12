using System;
using UnityEngine;

namespace Kamgam.UGUIWorldImage
{
    /// <summary>
    /// Will make the object destroy itself after deserialization.<br />
    /// We use this to auto destroy orphaned prefab instances.
    /// </summary>
    public class DestroyAfterDeserializationInEditor : MonoBehaviour
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
#if UNITY_EDITOR
        [System.NonSerialized]
        public bool IsScheduledForDestruction;

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            // Destroy self
            IsScheduledForDestruction = true;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && gameObject != null)
                {
                    GameObject.DestroyImmediate(gameObject);
                }
            };
        }
#endif
    }
}