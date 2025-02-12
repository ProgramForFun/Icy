using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kamgam.UGUIWorldImage
{
    [RequireComponent(typeof(WorldImage))]
    public class PrefabInstantiatorForWorldImage : MonoBehaviour
    {

        [Serializable]
        public class PrefabHandle
        {
            public GameObject Prefab;
            
            [Space(5)]
            public Vector3 Position = Vector3.zero;
            public Quaternion Rotation;
            public Vector3 Scale = Vector3.one;
            
            [Tooltip("Leave empty to spawn at root level.")]
            public Transform Parent;

            [System.NonSerialized]
            public GameObject Instance;

            /// <summary>
            /// Use this in a dynamic setting to add a reference to your custom data.
            /// </summary>
            [System.NonSerialized]
            public object UserData;

            public PrefabHandle(GameObject prefab, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Transform parent, object userData = null)
            {
                Prefab = prefab;
                Position = localPosition;
                Scale = localScale;
                Rotation = localRotation;
                Parent = parent;
                UserData = userData;
            }

            public bool HasPrefab => Prefab != null;
            public bool HasInstance => Instance != null;

            public void CreateOrUpdateInstance(WorldImage image, Transform parentOverride = null, System.Action<PrefabHandle> onCreateEvent = null)
            {
                if (Prefab == null)
                    return;

                bool created = false;
                if (Instance == null)
                {
                    Instance = GameObject.Instantiate(Prefab, parentOverride != null ? parentOverride : Parent);
                    Instance.name = Prefab.name + " (for World Image " + image.GetInstanceID() + ")";
                    Instance.AddComponent<DestroyAfterDeserializationInEditor>();
                    created = true;
                }

                Instance.transform.localPosition = Position;
                Instance.transform.localScale = Scale;
                Instance.transform.localRotation = Rotation;

                if (created)
                    onCreateEvent?.Invoke(this);
            }

            public void DestroyInstance()
            {
                if (Instance == null)
                {
                    return;
                }

                var instance = Instance;
                Instance = null;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    GameObject.DestroyImmediate(instance);
                }
                else
#endif
                {
                    GameObject.Destroy(instance);
                }
            }

            public void EnableInstance()
            {
                if (Instance == null)
                {
                    return;
                }

                Instance.gameObject.SetActive(true);
            }

            public void DisableInstance()
            {
                if (Instance == null)
                {
                    return;
                }

                Instance.gameObject.SetActive(false);
            }

            public override string ToString()
            {
                if (Instance != null)
                    return Instance.name;

                if (Prefab != null)
                    return Prefab.name;

                return base.ToString();
            }
        }



        /// <summary>
        /// A list of all the prefab handles.
        /// To access the prefab object use the ".Prefab" property of a handle.
        /// To access the instance in the scene use the ".Instance" property of a handle.
        /// </summary>
        [SerializeField]
        [ShowIf("m_prefabSourceAsset", null, ShowIfAttribute.DisablingType.ReadOnly)]
        [Tooltip("List of prefabs.\n" +
            "NOTICE: These may be overruled by the PrefabSourceAsset.")]
        protected List<PrefabHandle> m_prefabs = new List<PrefabHandle>();

        /// <summary>
        /// If set then this will take precedence over the default Prefabs list.<br />
        /// This Scriptable Object will have to implement the IPrefabInstantiatorForWorldImagePrefabSource interface or else it will be ignored.
        /// </summary>
        [SerializeField]
        [Tooltip("If set then this will take precedence over the default Prefabs list.\n" +
            "This Scriptable Object will have to implement the IPrefabInstantiatorForWorldImagePrefabSource interface or else it will be ignored.")]
        protected ScriptableObject m_prefabSourceAsset;

        /// <summary>
        /// If set then this will take precendence over both the PrefabSourceAsset and the Prefabs list.
        /// </summary>
        protected IPrefabInstantiatorForWorldImagePrefabSource m_prefabSource;

        /// <summary>
        /// If set then this will be used as the parent for all prefab instances.
        /// </summary>
        [Tooltip("If set then this will be used as the parent for all prefab instances.")]
        public Transform PrefabParentOverride;

        public List<PrefabHandle> Prefabs
        {
            get
            {
                if (m_prefabSource != null)
                {
                    return m_prefabSource.GetPrefabHandles();
                }

                if (m_prefabSourceAsset != null)
                {
                    var sourceAsset = m_prefabSourceAsset as IPrefabInstantiatorForWorldImagePrefabSource;
                    if(sourceAsset != null)
                        return sourceAsset.GetPrefabHandles();
                }

                return m_prefabs;
            }
        }
        

        public void SetPrefabs(List<PrefabHandle> prefabs)
        {
            m_prefabs = prefabs;
        }

        public void SetPrefabSource(IPrefabInstantiatorForWorldImagePrefabSource source)
        {
            m_prefabSource = source;
        }

        [Tooltip("Should instances be marked as do-not-save in the editor?")]
        public bool MarkAsDoNotSave = true;

        [Space(5)]
        [Tooltip("Should the prefabs be instantiated in OnEnable?\n" +
            "Instances will only be generated if not yet instantiated.")]
        public bool InstantiateOnEnable = false;

        [Tooltip("If the image is enabled then the prefab instances will be enabled.")]
        public bool ActivateOnEnable = false;

        [Tooltip("If empty then all prefabs will be instantiated or activated. If set then the indices in the list will be instantiated/activated.\n" +
            "This is used for both 'InstantiateOnEnable' and 'ActivateOnEnable'.\n" +
            "NOTICE: This will do nothing if 'InstantiateOnEnable' and 'ActivateOnEnable' are both off.")]
        public List<int> OnEnableIndices = new List<int>();

        [Tooltip("If the image is disabled then the prefab instances will be disabled.")]
        public bool DeactivateOnDisable = true;

        [Tooltip("If the image is destroyed then all the prefab instances will be destroyed too.")]
        public bool DestroyOnDestroy = true;
        [Space(5)]

        [Tooltip("Should instances be added to the world objects list?\n" +
            "Usually keeping this ON is recommended. Though it may be useful to disable this " +
            "if you have a dedicated prefab instance parent for all your prefabs.")]
        public bool AddToWorldObjectsList = true;

        public event System.Action<PrefabInstantiatorForWorldImage> OnWillEnable;
        public event System.Action<PrefabInstantiatorForWorldImage> OnWillDisable;
        public event System.Action<PrefabInstantiatorForWorldImage> OnWillDestroy;

        /// <summary>
        /// An event you can subscribe to that is called after the creation of a new prefab instance.<br />
        /// Useful for doing setup stuff on your instances.
        /// </summary>
        public event Action<PrefabHandle> OnCreatedInstance;

        [System.NonSerialized]
        protected WorldImage m_worldImage;
        public WorldImage WorldImage
        {
            get
            {
                if (m_worldImage == null || m_worldImage.gameObject == null)
                {
                    m_worldImage = this.GetComponent<WorldImage>();
                    RegisterActiveStateEvents(m_worldImage);
                }
                return m_worldImage;
            }
        }

        public void RegisterActiveStateEvents(WorldImage image)
        {
            if (image != null)
            {
                image.OnWillEnable -= onImageWillEnable;
                image.OnWillEnable += onImageWillEnable;

                image.OnWillDisable -= onImageWillDisable;
                image.OnWillDisable += onImageWillDisable;
            }
        }

        protected void onImageWillEnable(WorldImage image)
        {
            if (InstantiateOnEnable)
            {
                instantiateOnEnable(image);
            }

            if (ActivateOnEnable)
            {
                activateOnEnable();
            }
        }

        private void instantiateOnEnable(WorldImage image)
        {
            if (OnEnableIndices.Count == 0)
            {
                CreateAndAddAllInstancesToImage(image);
            }
            else
            {
                for (int i = 0; i < Prefabs.Count; i++)
                {
                    if (!OnEnableIndices.Contains(i))
                        continue;

                    var handle = Prefabs[i];
                    CreateOrUpdateInstance(image, handle);
                    if (AddToWorldObjectsList)
                    {
                        AddToImage(image, handle);
                    }
                }
            }
        }

        private void activateOnEnable()
        {
            if (OnEnableIndices.Count == 0)
            {
                foreach (var handle in Prefabs)
                {
                    if (handle.Instance != null)
                    {
                        handle.Instance.SetActive(true);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Prefabs.Count; i++)
                {
                    if (!OnEnableIndices.Contains(i))
                        continue;

                    var handle = Prefabs[i];
                    if (handle.Instance != null)
                    {
                        handle.Instance.SetActive(true);
                    }
                }
            }

            WorldImage.UpdateWorldObjectBounds();
        }

        protected void onImageWillDisable(WorldImage image)
        {
            if (DeactivateOnDisable)
            {
                foreach (var handle in Prefabs)
                {
                    if (handle.Instance != null)
                    {
                        handle.Instance.SetActive(false);
                    }
                }
            }
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            foreach (var handle in Prefabs)
            {
                if (handle.Scale.sqrMagnitude < 0.001f && handle.Position.sqrMagnitude < 0.001f)
                {
                    handle.Scale = Vector3.one;
                    UnityEditor.EditorUtility.SetDirty(this);
                }

                if (handle.Prefab != null && UnityEditor.PrefabUtility.GetPrefabAssetType(handle.Prefab) == UnityEditor.PrefabAssetType.NotAPrefab)
                {
                    handle.Prefab = null;
                    Debug.LogError("Prefab Instantiator: The chosen object is not a prefab! Resetting to null.");
                }

                // Update while editing
                if (handle.Instance != null)
                {
                    handle.Instance.transform.localPosition = handle.Position;
                    handle.Instance.transform.localScale = handle.Scale;
                    handle.Instance.transform.localRotation = handle.Rotation;
                }
            }

            WorldImage.UpdateWorldObjectBounds();
        }
#endif

        /// <summary>
        /// An alias for calling CreateOrUpdateInstances(image); and AddToImage(image);
        /// </summary>
        /// <param name="image"></param>
        public void CreateAndAddAllInstancesToImage(WorldImage image)
        {
            CreateOrUpdateAllInstances(image);

            if (AddToWorldObjectsList)
                AddAllToImage(image);
        }

        public void CreateAndAddInstanceToImage(WorldImage image, PrefabHandle handle)
        {
            CreateOrUpdateInstance(image, handle);

            if (AddToWorldObjectsList)
                AddToImage(image, handle);
        }

        public void RemoveAllFromImageAndDestroyInstances(WorldImage image)
        {
            RemoveAllFromImage(image);
            DestroyAllInstances();
        }

        public void RemoveFromImageAndDestroyInstance(WorldImage image, PrefabHandle handle)
        {
            RemoveFromImage(image, handle);
            DestroyInstance(handle);
        }

        public void RemoveFromImageAndDisableInstance(WorldImage image, PrefabHandle handle)
        {
            RemoveFromImage(image, handle);
            DisableInstance(handle);
        }

        public void CreateOrUpdateAllInstances(WorldImage image)
        {
            foreach (var handle in Prefabs)
            {
                CreateOrUpdateInstance(image, handle);
            }
        }

        public void CreateOrUpdateInstance(WorldImage image, PrefabHandle handle)
        {
            handle.CreateOrUpdateInstance(image, PrefabParentOverride, OnCreatedInstance);

            if (handle.Instance != null)
            {
                if (MarkAsDoNotSave)
                {
                    handle.Instance.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.NotEditable;
                    if (!handle.Instance.name.StartsWith("[Temp]")) 
                        handle.Instance.name = "[Temp] " + handle.Instance.name;
                }
                else
                {
                    handle.Instance.hideFlags = HideFlags.None;
                }
            }
        }

        public void ToogleOrCreate(int prefabIndex)
        {
            ToogleOrCreate(prefabIndex, destroyOnDisable: false, disableOthers: true);
        }

        public void ToogleOrCreate(int prefabIndex, bool destroyOnDisable, bool disableOthers)
        {
            if (prefabIndex < 0 || prefabIndex >= Prefabs.Count)
                return;

            var prefabHandle = Prefabs[prefabIndex];
            ToogleOrCreate(prefabHandle, destroyOnDisable, disableOthers);
        }

        public void ToogleOrCreate(PrefabHandle prefabHandle, bool destroyOnDisable = false, bool disableOthers = true)
        {
            bool show;

            if (prefabHandle.Instance == null)
            {
                CreateAndAddInstanceToImage(WorldImage, prefabHandle);
                show = true;
            }
            else
            {
                if (destroyOnDisable)
                {
                    show = false;
                }
                else
                {
                    show = !prefabHandle.Instance.activeInHierarchy;
                }
            }

            if (show)
            {
                if (!destroyOnDisable)
                {
                    prefabHandle.Instance.SetActive(true);
                }

                if (disableOthers)
                {
                    // Destroy/disable out all the others
                    foreach (var handle in Prefabs)
                    {
                        if (handle != prefabHandle && handle.HasInstance)
                        {
                            if (destroyOnDisable)
                            {
                                RemoveFromImageAndDestroyInstance(WorldImage, handle);
                            }
                            else
                            {
                                DisableInstance(handle);
                            }
                        }
                    }
                }
            }
            else
            {
                if (destroyOnDisable)
                {
                    RemoveFromImageAndDestroyInstance(WorldImage, prefabHandle);
                }
                else
                {
                    DisableInstance(prefabHandle);
                }
            }
        }

        public void EnableOrCreate(int prefabIndex)
        {
            EnableOrCreate(prefabIndex, destroyOnDisable: false, disableOthers: true);
        }

        public void EnableOrCreate(int prefabIndex, bool destroyOnDisable, bool disableOthers)
        {
            if (prefabIndex < 0 || prefabIndex >= Prefabs.Count)
                return;

            var prefabHandle = Prefabs[prefabIndex];
            EnableOrCreate(prefabHandle, destroyOnDisable, disableOthers);
        }

        public void EnableOrCreate(PrefabHandle prefabHandle, bool destroyOnDisable = false, bool disableOthers = true)
        {
            if (prefabHandle.Instance == null)
            {
                CreateAndAddInstanceToImage(WorldImage, prefabHandle);
            }

            if (!destroyOnDisable)
            {
                prefabHandle.Instance.SetActive(true);
            }

            if (disableOthers)
            {
                // Destroy/disable out all the others
                foreach (var handle in Prefabs)
                {
                    if (handle != prefabHandle && handle.HasInstance)
                    {
                        if (destroyOnDisable)
                        {
                            RemoveFromImageAndDestroyInstance(WorldImage, handle);
                        }
                        else
                        {
                            DisableInstance(handle);
                        }
                    }
                }
            }

            WorldImage.UpdateWorldObjectBounds();
        }

        public void DisableAllInstances()
        {
            foreach (var handle in Prefabs)
            {
                handle.DisableInstance();
            }
        }

        public void DestroyAllInstances()
        {
            foreach (var handle in Prefabs)
            {
                handle.DestroyInstance();
            }
        }

        public void DestroyInstance(PrefabHandle handle)
        {
            handle.DestroyInstance();
        }

        public void EnableInstance(PrefabHandle handle)
        {
            handle.EnableInstance();
        }

        public void DisableInstance(PrefabHandle handle)
        {
            handle.DisableInstance();
        }

        public void AddAllToImage(WorldImage image)
        {
            if (image == null)
                return;

            foreach (var handle in Prefabs)
            {
                if (handle.Instance != null)
                {
                    image.AddWorldObject(handle.Instance.transform);
                }
            }

            image.DefragWorldObjects();
            image.UpdateWorldObjectBounds();
        }

        public void AddToImage(WorldImage image, PrefabHandle handle)
        {
            if (image == null || handle == null || handle.Instance == null)
                return;

            image.AddWorldObject(handle.Instance.transform);

            image.DefragWorldObjects();
            image.UpdateWorldObjectBounds();
        }

        public void RemoveAllFromImage(WorldImage image)
        {
            if (image == null)
                return;

            foreach (var handle in Prefabs)
            {
                if (handle.Instance != null)
                {
                    image.RemoveWorldObject(handle.Instance.transform);
                }
            }

            image.DefragWorldObjects();
            image.UpdateWorldObjectBounds();
        }

        public void RemoveFromImage(WorldImage image, PrefabHandle handle)
        {
            if (image == null || handle == null || handle.Instance == null)
                return;

            image.RemoveWorldObject(handle.Instance.transform);
            
            image.DefragWorldObjects();
            image.UpdateWorldObjectBounds();
        }

        public void OnEnable()
        {
            OnWillEnable?.Invoke(this);

            onImageWillEnable(WorldImage);
        }

        public void OnDisable()
        {
            OnWillDisable?.Invoke(this);
        }

        public void OnDestroy()
        {
            if (DestroyOnDestroy)
                DestroyAllInstances();

            OnWillDestroy?.Invoke(this);
        }
    }
}