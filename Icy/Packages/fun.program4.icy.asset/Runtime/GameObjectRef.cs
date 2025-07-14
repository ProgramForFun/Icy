/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Icy.Asset
{
	/// <summary>
	/// 基于AssetRef封装的GameObejctRef，方便使用
	/// </summary>
	public class GameObjectRef
	{
		/// <summary>
		/// 持有的GameObject，命名遵循Unity
		/// </summary>
		public GameObject gameObject { get; protected set; }
		#region 便利转发，命名遵循Unity
		// 属性
		public Transform transform => gameObject.transform;
		public int layer { get => gameObject.layer; set => gameObject.layer = value; }
		public string tag { get => gameObject.tag; set => gameObject.tag = value; }
		public bool activeSelf => gameObject.activeSelf;
		public bool activeInHierarchy => gameObject.activeInHierarchy;
		public bool isStatic { get => gameObject.isStatic; set => gameObject.isStatic = value; }
		public Scene scene => gameObject.scene;
		public string name { get => gameObject.name; set => gameObject.name = value; }
		public HideFlags hideFlags { get => gameObject.hideFlags; set => gameObject.hideFlags = value; }

		// 实例方法
		public Component AddComponent(Type type) => gameObject.AddComponent(type);
		public T AddComponent<T>() where T : Component => gameObject.AddComponent<T>();
		public void BroadcastMessage(string methodName, object parameter = null, SendMessageOptions options = SendMessageOptions.RequireReceiver) => gameObject.BroadcastMessage(methodName, parameter, options);
		public bool CompareTag(string tag) => gameObject.CompareTag(tag);
		public Component GetComponent(string type) => gameObject.GetComponent(type);
		public T GetComponent<T>() => gameObject.GetComponent<T>();
		public Component GetComponentInChildren(Type type, bool includeInactive = false) => gameObject.GetComponentInChildren(type, includeInactive);
		public T GetComponentInChildren<T>(bool includeInactive = false) => gameObject.GetComponentInChildren<T>(includeInactive);
		public Component GetComponentInParent(Type type) => gameObject.GetComponentInParent(type);
		public T GetComponentInParent<T>() => gameObject.GetComponentInParent<T>();
		public Component[] GetComponents(Type type) => gameObject.GetComponents(type);
		public void GetComponents(Type type, List<Component> results) => gameObject.GetComponents(type, results);
		public T[] GetComponents<T>() => gameObject.GetComponents<T>();
		public void GetComponents<T>(List<T> results) => gameObject.GetComponents<T>(results);
		public Component[] GetComponentsInChildren(Type type, bool includeInactive = false) => gameObject.GetComponentsInChildren(type, includeInactive);
		public T[] GetComponentsInChildren<T>(bool includeInactive = false) => gameObject.GetComponentsInChildren<T>(includeInactive);
		public Component[] GetComponentsInParent(Type type, bool includeInactive = false) => gameObject.GetComponentsInParent(type, includeInactive);
		public T[] GetComponentsInParent<T>(bool includeInactive = false) => gameObject.GetComponentsInParent<T>(includeInactive);
		public void SendMessage(string methodName, object value = null, SendMessageOptions options = SendMessageOptions.RequireReceiver) => gameObject.SendMessage(methodName, value, options);
		public void SendMessageUpwards(string methodName, object value = null, SendMessageOptions options = SendMessageOptions.RequireReceiver) => gameObject.SendMessageUpwards(methodName, value, options);
		public void SetActive(bool value) => gameObject.SetActive(value);

		// 静态方法
		public static GameObject Find(string name) => GameObject.Find(name);
		public static GameObject[] FindGameObjectsWithTag(string tag) => GameObject.FindGameObjectsWithTag(tag);
		public static GameObject FindWithTag(string tag) => GameObject.FindGameObjectWithTag(tag);
		public static GameObject CreatePrimitive(PrimitiveType type) => GameObject.CreatePrimitive(type);
		#endregion
		/// <summary>
		/// 是否已加载结束，不关心成功还是失败
		/// </summary>
		public bool IsFinish => _AssetRef.IsFinish;
		/// <summary>
		/// 是否已加载成功
		/// </summary>
		public bool IsSucceed => _AssetRef.IsSucceed;
		/// <summary>
		/// 加载完成的回调
		/// </summary>
		public event Action<bool> OnLoadFinish
		{
			add
			{
				_OnFinish += value;
				if (IsFinish)
					value?.Invoke(IsSucceed);
			}
			remove => _OnFinish -= value;
		}

		/// <summary>
		/// 内部持有的AssetRef
		/// </summary>
		protected AssetRef _AssetRef;
		/// <summary>
		/// 加载完成的回调
		/// </summary>
		protected Action<bool> _OnFinish;


		public GameObjectRef(string address)
		{
			_AssetRef = AssetManager.Instance.LoadAssetAsync(address);
			_AssetRef.Retain();
			_AssetRef.OnFinish += OnAssetRefFinish;
		}

		/// <summary>
		/// UniTask支持
		/// </summary>
		public UniTask ToUniTask(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return _AssetRef.ToUniTask(progress, timing);
		}

		protected void OnAssetRefFinish(AssetRef assetRef)
		{
			if (assetRef.IsSucceed)
				gameObject = GameObject.Instantiate(assetRef.AssetObject) as GameObject;
			_OnFinish?.Invoke(assetRef.IsSucceed);
		}

		public void Destroy()
		{
			GameObject.Destroy(gameObject);
			_AssetRef.Release();
		}
	}
}
