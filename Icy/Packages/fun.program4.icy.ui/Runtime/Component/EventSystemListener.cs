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


using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Icy.UI
{
	/// <summary>
	/// 物体的EventSystem事件监听
	/// </summary>
	public class EventSystemListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
									, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerEnterHandler
	{
		public Action<PointerEventData> OnClick;
		public Action<PointerEventData> OnClickDown;
		public Action<PointerEventData> OnClickUp;
		public Action<PointerEventData> OnSlideIn;
		public Action<PointerEventData> OnSlideOut;
		public Action<PointerEventData> OnBeginDragObj;
		public Action<PointerEventData> OnEndDragObj;
		public Action<PointerEventData> OnDragObj;
		public Action<PointerEventData> OnDropObj;

		private bool _isSlideOut;
		//private bool _isSlideIn;
		private bool _isDragging = false;

		private int _FingerId = 0;

		[Header("是否启用点击CD")]
		public bool ClickCD = false;
		public float ClickCDTime = 1.2f;
		private float _ClickCDTimer = 0;
		private bool _CanClick = true;

		public void OnPointerClick(PointerEventData eventData)
		{
			//双指以上同时操作时屏蔽点击事件
#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
		if (Input.touchCount > 1)
		{
			return;
		}
#endif

			//PC平台只响应鼠标左键
#if UNITY_STANDALONE
			if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
				return;
#endif

			_isSlideOut = false;
			if (!_isSlideOut && _CanClick)
			{
				OnClick?.Invoke(eventData);
				// 开启了点击CD
				if (ClickCD)
				{
					_ClickCDTimer = 0;
					_CanClick = false;
				}
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			// 不是当前的手指 不响应
			if (_FingerId != eventData.pointerId)
				return;
			//_isSlideIn = true;
			OnSlideIn?.Invoke(eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			//PC平台只响应鼠标左键
#if UNITY_STANDALONE
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
#endif
			_FingerId = eventData.pointerId;
			_isSlideOut = false;
			OnClickDown?.Invoke(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			//PC平台只响应鼠标左键
#if UNITY_STANDALONE
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
#endif
			// 不是当前的手指 不响应
			if (_FingerId != eventData.pointerId)
				return;

			// 拖拽的时候 结束时刻 先响应Up再响应DragEnd，所以拖拽下 留给DragEnd把ID设置默认
			if (!_isDragging)
				_FingerId = int.MinValue;

			OnClickUp?.Invoke(eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			// 不是当前的手指 不响应
			if (_FingerId != eventData.pointerId)
				return;
			_isSlideOut = true;
			OnSlideOut?.Invoke(eventData);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			//PC平台只响应鼠标左键
#if UNITY_STANDALONE
			if (eventData.button != PointerEventData.InputButton.Left)
				return;
#endif

			// 不是当前的手指 不响应
			if (_FingerId != eventData.pointerId)
				return;

			_isDragging = true;
			OnBeginDragObj?.Invoke(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			// 不是当前的手指 不响应
			if (_FingerId != eventData.pointerId)
				return;

			_FingerId = int.MinValue;
			_isDragging = false;
			OnEndDragObj?.Invoke(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			// 不是当前的手指 不响应
			if (_FingerId != eventData.pointerId)
				return;
			OnDragObj?.Invoke(eventData);
		}

		public void OnDrop(PointerEventData eventData)
		{
			OnDropObj?.Invoke(eventData);
		}

		void OnEnable()
		{
			if (ClickCD)
			{
				_CanClick = true;
				_ClickCDTimer = 0;
			}
		}

		void Update()
		{
			if (ClickCD && !_CanClick)
			{
				_ClickCDTimer += Time.deltaTime;
				if (_ClickCDTimer > ClickCDTime)
					_CanClick = true;
			}
		}
	}
}
