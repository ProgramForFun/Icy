using Icy.Base;
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
			// Log.LogInfo("safe area = " + Screen.safeArea.ToString());
			return Screen.safeArea;
		}

		private void ApplySafeArea(Rect r)
		{
			_LastSafeArea = r;

			// Convert safe area rectangle from absolute pixels to normalised anchor coordinates
			Vector2 anchorMin = r.position;
			Vector2 anchorMax = r.position + r.size;
			anchorMin.x /= Screen.width;
			anchorMin.y /= Screen.height;
			anchorMax.x /= Screen.width;
			anchorMax.y /= Screen.height;
			_rectTrans.anchorMin = anchorMin;
			_rectTrans.anchorMax = anchorMax;

			Log.LogInfo($"New safe area applied to {name}: x={r.x}, y={r.y}, w={r.width}, h={r.height} on full extents w={Screen.width}, h={Screen.height}", "NotchScreenAdapter");
		}
	}
}
