//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public static class UIUtility
//{

//	/// <summary>
//	/// 屏幕空间的指定位置转换到UGUI的位置；
//	/// 参数拆分是为了Lua调用的传参效率
//	/// </summary>
//	/// <param name="screenPosX">要转换的屏幕坐标x</param>
//	/// <param name="screenPosY">要转换的屏幕坐标y</param>
//	/// <param name="parentRectTrans">转换后的UGUI父节点，如果传null的话，会以UIRoot的canvas作为父节点</param>
//	/// <param name="x">结果UGUI坐标x</param>
//	/// <param name="y">结果UGUI坐标y</param>
//	/// <returns></returns>
//	public static bool ScreenPos2UGUIPos(float screenPosX, float screenPosY, RectTransform parentRectTrans
//										, out float x, out float y)
//	{
//		if (parentRectTrans == null)
//			parentRectTrans = UGUIRoot.Canvas.transform as RectTransform;
//		Vector2 rtn;
//		bool result = RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTrans
//																			, new Vector2(screenPosX, screenPosY), UGUIRoot.UICamera, out rtn);
//		x = rtn.x;
//		y = rtn.y;
//		return result;
//	}

//	/// <summary>
//	/// 
//	/// </summary>
//	/// <param name="worldPosX">要转换的世界坐标x</param>
//	/// <param name="worldPosY">要转换的世界坐标y</param>
//	/// <param name="worldPosZ">要转换的世界坐标z</param>
//	/// <param name="parentRectTrans">转换后的UGUI父节点，如果传null的话，会以UIRoot的canvas作为父节点</param>
//	/// <param name="x">结果UGUI坐标x</param>
//	/// <param name="y">结果UGUI坐标y</param>
//	/// <returns></returns>
//	public static bool WorldPos2UGUIPos(float worldPosX, float worldPosY, float worldPosZ, RectTransform parentRectTrans
//										, out float x, out float y)
//	{
//		Vector3 worldPos = new Vector3(worldPosX, worldPosY, worldPosZ);
//		Vector3 screenPos = CameraManager.Instance.Camera3D.WorldToScreenPoint(worldPos);
//		// Tips:需要判断下世界坐标是否在相机前方，相机后方的世界坐标获得的屏幕坐标是相反的
//		Plane cameraPlane = new Plane(CameraManager.Instance.Camera3D.transform.forward, CameraManager.Instance.Camera3D.transform.position);
//		if (!cameraPlane.SameSide(CameraManager.Instance.Camera3D.transform.forward + CameraManager.Instance.Camera3D.transform.position, worldPos))
//		{
//			screenPos = -screenPos;
//		}
//		return ScreenPos2UGUIPos(screenPos.x, screenPos.y, parentRectTrans, out x, out y);
//	}

//	/// <summary>
//	/// UI坐标转UI 参考坐标系不同
//	/// </summary>
//	/// <param name="worldPosX">要转换的世界坐标x</param>
//	/// <param name="worldPosY">要转换的世界坐标y</param>
//	/// <param name="worldPosZ">要转换的世界坐标z</param>
//	/// <param name="parentRectTrans">转换后的UGUI父节点，如果传null的话，会以UIRoot的canvas作为父节点</param>
//	/// <param name="x">结果UGUI坐标x</param>
//	/// <param name="y">结果UGUI坐标y</param>
//	/// <returns></returns>
//	public static bool UIPos2UIPos(float worldPosX, float worldPosY, float worldPosZ, RectTransform parentRectTrans
//									, out float x, out float y)
//	{
//		if (parentRectTrans == null)
//			parentRectTrans = UGUIRoot.Canvas.transform as RectTransform;
//		Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(UGUIRoot.UICamera, new Vector3(worldPosX, worldPosY, worldPosZ));
//		Vector2 rtn;
//		bool result = UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTrans, screenPos, UGUIRoot.UICamera, out rtn);
//		x = rtn.x;
//		y = rtn.y;
//		return result;
//	}


//	public static float GetScreenWidthAdaption(float uiWidth)
//	{
//		if (UGUIRoot.Canvas != null && UGUIRoot.Canvas.transform != null)
//		{
//			RectTransform rectTransform = UGUIRoot.Canvas.GetComponent<RectTransform>();
//			if (rectTransform != null)
//			{
//				float width = rectTransform.rect.width;
//				float adapterRatio = Screen.width / Screen.safeArea.width;
//				float ratio = Screen.safeArea.width / (width);
//				float showWidth = ratio * (uiWidth * adapterRatio);
//				if (showWidth > Screen.width)
//				{
//					//TODO 
//					Log.LogError(string.Format("GetScreenHeightAdaption Error! showWidth:{0} uiWidth{1}  ratio:{2} adapterRatio:{3} ", showWidth, uiWidth, ratio, adapterRatio));
//				}
//				return showWidth > Screen.width ? Screen.width : showWidth;
//			}
//			return uiWidth;
//		}
//		return uiWidth;
//	}

//	public static float GetScreenHeightAdaption(float uiHeight)
//	{
//		if (UGUIRoot.Canvas != null && UGUIRoot.Canvas.transform != null)
//		{
//			RectTransform rectTransform = UGUIRoot.Canvas.GetComponent<RectTransform>();
//			if (rectTransform != null)
//			{
//				float height = rectTransform.rect.height;
//				float adapterRatio = Screen.height / Screen.safeArea.height;
//				float ratio = Screen.safeArea.height / (height);
//				float showHeight = ratio * (uiHeight * adapterRatio);
//				if (showHeight > Screen.width)
//				{
//					UnityLogUtil.LogError(string.Format("GetScreenHeightAdaption Error! showHeght:{0} uiHeight{1}  ratio:{2} adapterRatio:{3} ", showHeight, uiHeight, ratio, adapterRatio));
//				}
//				return showHeight > Screen.height ? Screen.height : showHeight;
//			}
//			return uiHeight;
//		}
//		return uiHeight;
//	}
//}
