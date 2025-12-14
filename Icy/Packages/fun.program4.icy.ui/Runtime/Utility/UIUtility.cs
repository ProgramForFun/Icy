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


using Icy.Base;
using UnityEngine;

namespace Icy.UI
{
	public static class UIUtility
	{
		/// <summary>
		/// 屏幕空间的指定坐标转换到UGUI的坐标
		/// </summary>
		/// <param name="screenPos">要转换的屏幕坐标</param>
		/// <param name="parentRectTrans">转换后的UGUI父节点，如果传null的话，会以UIRoot的canvas作为父节点</param>
		/// <param name="uguiPos">结果UGUI坐标</param>
		/// <returns></returns>
		public static bool ScreenPos2UGUIPos(Vector2 screenPos, RectTransform parentRectTrans, out Vector2 uguiPos)
		{
			if (parentRectTrans == null)
				parentRectTrans = UIRoot.Instance.RootCanvas.transform as RectTransform;
			bool result = RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTrans, screenPos, UIRoot.Instance.UICamera, out uguiPos);
			return result;
		}

		/// <summary>
		/// 世界坐标转UGUI坐标
		/// </summary>
		/// <param name="worldPos">要转换的世界坐标</param>
		/// <param name="parentRectTrans">转换后的UGUI父节点，如果传null的话，会以UIRoot的canvas作为父节点</param>
		/// <param name="uguiPos">结果UGUI坐标</param>
		/// <returns></returns>
		public static bool WorldPos2UGUIPos(Vector3 worldPos, Camera camera3D, RectTransform parentRectTrans, out Vector2 uguiPos)
		{
			Vector3 screenPos = camera3D.WorldToScreenPoint(worldPos);
			// Tips:需要判断下世界坐标是否在相机前方，相机后方的世界坐标获得的屏幕坐标是相反的
			Plane cameraPlane = new Plane(camera3D.transform.forward, camera3D.transform.position);
			if (!cameraPlane.SameSide(camera3D.transform.forward + camera3D.transform.position, worldPos))
				screenPos = -screenPos;
			return ScreenPos2UGUIPos(screenPos, parentRectTrans, out uguiPos);
		}

		/// <summary>
		/// UI坐标转UI坐标，参考坐标系不同
		/// </summary>
		/// <param name="worldPosOfUI">要转换的UI组件的世界坐标</param>
		/// <param name="parentRectTrans">转换后的UGUI父节点，如果传null的话，会以UIRoot的canvas作为父节点</param>
		/// <param name="uguiPos">结果UGUI坐标</param>
		/// <returns></returns>
		public static bool UIPos2UIPos(Vector3 worldPosOfUI, RectTransform parentRectTrans, out Vector2 uguiPos)
		{
			if (parentRectTrans == null)
				parentRectTrans = UIRoot.Instance.RootCanvas.transform as RectTransform;
			Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(UIRoot.Instance.UICamera, worldPosOfUI);
			return ScreenPos2UGUIPos(screenPos, parentRectTrans, out uguiPos);
		}

		/// <summary>
		/// 从一个UI的子节点开始向上找，直到找到UI脚本或层级顶
		/// </summary>
		public static UIBase GetUIFromParent(Transform childOfUI)
		{
			Transform parent = childOfUI;
			while (parent != null)
			{
				UIBase ui = parent.GetComponent<UIBase>();
				if (ui != null)
					return ui;
				parent = parent.parent;
			}

			string childName = childOfUI == null ? "null" : childOfUI.name;
			Log.Assert(false, $"Can NOT find UIBase of {childName}");
			return null;
		}
	}
}
