/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
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

namespace Icy.UI
{
	/// <summary>
	/// 适配刘海屏
	/// </summary>
	public class NotchScreenAdapter : MonoBehaviour
	{
		private RectTransform _rectTrans;
		private Rect _LastSafeArea;

		protected void Start()
		{
			_rectTrans = GetComponent<RectTransform>();
			Refresh();
		}

#if UNITY_EDITOR
		//为了可以工作在editor下的Device Simulator
		protected void Update()
		{
			Refresh();
		}
#endif

		private void Refresh()
		{
			Rect safeArea = GetSafeArea();

			if (safeArea != _LastSafeArea)
				ApplySafeArea(safeArea);
		}

		private Rect GetSafeArea()
		{
			// Log.Info("safe area = " + Screen.safeArea.ToString());
			return Screen.safeArea;
		}

		private void ApplySafeArea(Rect r)
		{
			_LastSafeArea = r;

			// Convert safe area rectangle from absolute pixels to normalize anchor coordinates
			Vector2 anchorMin = r.position;
			Vector2 anchorMax = r.position + r.size;
			anchorMin.x /= Screen.width;
			anchorMin.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;
			_rectTrans.anchorMin = anchorMin;
			_rectTrans.anchorMax = anchorMax;

			//Base.Log.Info($"New safe area applied to {name}: x={r.x}, y={r.y}, w={r.width}, h={r.height} on full extents w={Screen.width}, h={Screen.height}", "NotchScreenAdapter");
		}
	}
}
