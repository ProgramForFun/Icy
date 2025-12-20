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


using UnityEngine;
using Icy.Base;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using System;
using System.Collections.Generic;

namespace Icy.UI
{
	/// <summary>
	/// 所有UI的根节点
	/// </summary>
	public sealed class UIRoot : PersistentMonoSingleton<UIRoot>
	{
		/// <summary>
		/// 所有UI的根Canvas
		/// </summary>
		public Canvas RootCanvas => _Canvas;
		[SerializeField] private Canvas _Canvas;

		/// <summary>
		/// UI相机
		/// </summary>
		public Camera UICamera => _Camera;
		[SerializeField] private Camera _Camera;

		/// <summary>
		/// UGUI的EventSystem
		/// </summary>
		public EventSystem EventSystem => _EventSystem;
		[SerializeField] private EventSystem _EventSystem;

		/// <summary>
		/// UI的背景模糊
		/// </summary>
		public UIBlur Blur => _Blur;
		[SerializeField] private UIBlur _Blur;

		private Dictionary<UILayer, GameObject> _LayerGameObjMap;


		protected override void OnInitialized()
		{
			_LayerGameObjMap = new Dictionary<UILayer, GameObject>();

			//创建UI层级的根节点
			Type UILayerType = typeof(UILayer);
			Array renderQueues = Enum.GetValues(UILayerType);
			for (int i = 0; i < renderQueues.Length; i++)
			{
				object value = renderQueues.GetValue(i);
				string layerName = Enum.GetName(UILayerType, value);
				GameObject layerGo = new GameObject(layerName);
				CommonUtility.SetParent(_Canvas.gameObject, layerGo);
				_LayerGameObjMap[(UILayer)value] = layerGo;

				//全屏
				RectTransform rectTrans = layerGo.AddComponent<RectTransform>();
				rectTrans.anchorMin = Vector2.zero;
				rectTrans.anchorMax = Vector2.one;
				rectTrans.offsetMin = Vector2.zero;
				rectTrans.offsetMax = Vector2.zero;
			}
		}

		/// <summary>
		/// 获取指定UILayer对应的根物体
		/// </summary>
		public GameObject GetLayerGameObject(UILayer layer)
		{
			if (_LayerGameObjMap.ContainsKey(layer))
				return _LayerGameObjMap[layer];
			else
			{
				Log.Error($"Unexpected UI layer = {layer}", nameof(UIRoot));
				return _LayerGameObjMap[UILayer.Medium];
			}
		}

		/// <summary>
		/// 把UI相机添加到URP Base Camera的Camera Stack里
		/// </summary>
		public void AddUICameraToCameraStack(Camera baseCamera)
		{
			if (baseCamera == null)
			{
				Log.Error("Trying to add UICamera to a null baseCamera", nameof(UIRoot));
				return;
			}

			UniversalAdditionalCameraData baseCameraData = baseCamera.GetUniversalAdditionalCameraData();
			if (baseCameraData.renderType != CameraRenderType.Base)
			{
				Log.Error($"Trying to add UICamera to a overlay camera, camera gameObject name = {baseCamera.gameObject.name}", nameof(UIRoot));
				return;
			}

			UniversalAdditionalCameraData urpCameraData = UICamera.GetUniversalAdditionalCameraData();
			urpCameraData.renderType = CameraRenderType.Overlay;
			baseCameraData.cameraStack.Add(UICamera);
		}

		/// <summary>
		/// 把UI相机从URP Base Camera的Camera Stack里移除
		/// </summary>
		public void RemoveUICameraFromCameraStack(Camera baseCamera)
		{
			if (baseCamera == null)
			{
				Log.Error("Trying to remove UICamera from a null baseCamera", nameof(UIRoot));
				return;
			}

			UniversalAdditionalCameraData baseCameraData = baseCamera.GetUniversalAdditionalCameraData();
			if (!baseCameraData.cameraStack.Contains(UICamera))
			{
				Log.Error("Trying to remove UICamera from a baseCamera which UICamera doesn't add to", nameof(UIRoot));
				return;
			}
			baseCameraData.cameraStack.Remove(UICamera);
		}

		/// <summary>
		/// 为指定UI添加背景模糊
		/// </summary>
		public void SetBlurToUI(UIBase ui)
		{
			Blur.gameObject.SetActive(true);

			Blur.transform.SetParent(ui.transform.parent);
			Blur.transform.SetSiblingIndex(ui.transform.GetSiblingIndex());

			int order = ui.Canvas.sortingOrder;
			Blur.Canvas.sortingOrder = order - 1;
		}

		/// <summary>
		/// 关闭背景模糊
		/// </summary>
		public void CloseBlur()
		{
			Blur.gameObject.SetActive(false);
			Blur.transform.SetParent(RootCanvas.transform);
			Blur.transform.SetAsFirstSibling();
		}
	}
}
