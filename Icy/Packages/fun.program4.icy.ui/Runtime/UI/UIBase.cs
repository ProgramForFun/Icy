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
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

namespace Icy.UI
{
	/// <summary>
	/// UI基类
	/// </summary>
	[InfoBox("Modifying component directly is disabled, re-generate ui code to achieve it")]
	[RequireComponent(typeof(UICodeGenerator))]
	public abstract class UIBase : MonoBehaviour
	{
		/// <summary>
		/// UI的名字
		/// </summary>
		public string UIName { get; internal set; }
		/// <summary>
		/// UI的类型
		/// </summary>
		public UIType UIType => _Type;
		[SerializeField] protected UIType _Type = UIType.Dialog;
		/// <summary>
		/// UI的层级
		/// </summary>
		public UILayer UILayer => _Layer;
		[SerializeField] protected UILayer _Layer = UILayer.Medium;
		/// <summary>
		/// 当前界面的隐藏类型，如果没有隐藏过，为HideType.NotSet
		/// </summary>
		public UIHideType HideType { get; set; }
		/// <summary>
		/// 当前是否正在显示
		/// </summary>
		public bool IsShowing { get; protected set; }
		/// <summary>
		/// 是否已销毁
		/// </summary>
		public bool IsDestroyed { get; protected set; }
		/// <summary>
		/// 界面根节点的RectTransform
		/// </summary>
		public RectTransform RectTransform { get; private set; }
		/// <summary>
		/// 当前界面的CanvasGroup
		/// </summary>
		public Canvas Canvas { get; private set; }
		/// <summary>
		/// 当前界面的CanvasGroup
		/// </summary>
		protected CanvasGroup _CanvasGroup;
		/// <summary>
		/// 界面的初始alpha值
		/// </summary>
		protected float _OriginalAlpha;
		/// <summary>
		/// 传给UI的参数
		/// </summary>
		protected IUIParam _Param;
		/// <summary>
		/// 是否已初始化
		/// </summary>
		protected bool _Inited = false;
		/// <summary>
		/// HideType为MoveOutScreen，移动到的位置
		/// </summary>
		protected static readonly Vector3 MOVE_OUT_POS = new Vector3(0.0f, 10240.0f, 0.0f);
		/// <summary>
		/// UI隐藏时触发的CancellationTokenSource
		/// </summary>
		protected CancellationTokenSource _CancelTokenSourceOnHide;
		/// <summary>
		/// 是否正在退出editor的play模式
		/// </summary>
		private bool _IsExitingPlayMode;


		/// <summary>
		/// 框架会自动调用，无需业务侧调用
		/// </summary>
		public virtual void Init()
		{
			if (_Inited)
			{
				Log.Error("Duplicate call Init, UIName = " + UIName, nameof(UIBase));
				return;
			}
			RectTransform = transform as RectTransform;
			RectTransform.anchorMin = Vector2.zero;
			RectTransform.anchorMax = Vector2.one;
			RectTransform.offsetMin = Vector2.zero;
			RectTransform.offsetMax = Vector2.zero;

			Canvas = gameObject.GetOrAddComponent<Canvas>();
			gameObject.GetOrAddComponent<GraphicRaycaster>();
			_CanvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

			HideType = UIHideType.Deactive;
			IsShowing = false;
			IsDestroyed = false;
			_OriginalAlpha = _CanvasGroup.alpha;
			_IsExitingPlayMode = false;

			_Inited = true;
		}

		public virtual void Show(IUIParam param = null)
		{
			if (IsShowing)
				return;
			IsShowing = true;

			_Param = param;

			switch (HideType)
			{
				case UIHideType.Deactive:
					gameObject.SetActive(true);
					break;
				case UIHideType.MoveOutScreen:
					gameObject.transform.localPosition = Vector3.zero;
					break;
				default:
					//do nothing
					break;
			}

			_CancelTokenSourceOnHide = new CancellationTokenSource();
			UIManager.Instance.Show(this, param);
		}

		public virtual void Hide()
		{
			if (!IsShowing)
				return;
			IsShowing = false;

			_CancelTokenSourceOnHide.Cancel();
			_CancelTokenSourceOnHide.Dispose();

			DoHide();
			UIManager.Instance.Hide(this);
		}
		internal void DoHide()
		{
			switch (HideType)
			{
				case UIHideType.Deactive:
					gameObject.SetActive(false);
					break;
				case UIHideType.MoveOutScreen:
					gameObject.transform.localPosition = MOVE_OUT_POS;
					break;
				default:
					Log.Error($"Invalid HideType {HideType}", nameof(UIBase));
					break;
			}
		}

		public virtual void Destroy()
		{
			if (IsDestroyed)
			{
				Log.Error($"Duplicate Destroy to {GetType().Name}", nameof(UIBase));
				return;
			}

			if (IsShowing)
				Hide();

			IsDestroyed = true;
			UIManager.Instance.Destroy(this);

			UnityEngine.Object.Destroy(this.gameObject);
		}

		protected void OnDestroy()
		{
			if (!IsDestroyed && !_IsExitingPlayMode)
				Log.Error($"UI GameObject of {GetType().Name} is unexpected destroyed", nameof(UIBase));
		}

#if UNITY_EDITOR
		protected void OnEnable()
		{
			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private void OnDisable()
		{
			UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
		{
			if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
				_IsExitingPlayMode = true;
		}
#endif

		#region Safe Delay
		/* 封装了Timer的延迟函数，使用UI销毁时触发的CancelToken控制，以保证UI销毁时延迟函数可以自动被中止
		 * UI应该优先使用这里的封装好的，而不是直接使用Timer里的
		*/

		/// <summary>
		/// 延迟指定时间后，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		/// <param name="delaySeconds">要延迟的时间，单位秒</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		protected CancellationTokenSource DelayByTime(Action action, float delaySeconds, bool ignoreTimeScale = false)
		{
			CancellationTokenSource linkedTokenSource = GetLinkedCancellationTokenSource();
			Base.Timer.DoDelayByTime(action, delaySeconds, linkedTokenSource.Token, ignoreTimeScale).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// 延迟指定帧数后，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		/// <param name="frameCount">要延迟的帧数</param>
		protected CancellationTokenSource DelayByFrame(Action action, int frameCount)
		{
			CancellationTokenSource linkedTokenSource = GetLinkedCancellationTokenSource();
			Base.Timer.DoDelayByFrame(action, frameCount, linkedTokenSource.Token).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// 延迟到下一帧，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		protected CancellationTokenSource NextFrame(Action action)
		{
			CancellationTokenSource linkedTokenSource = GetLinkedCancellationTokenSource();
			Base.Timer.DoNextFrame(action, linkedTokenSource.Token).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// 每隔指定的时间间隔，执行一次 action
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perSeconds">每几秒执行一次；下限保底为0.005秒</param>
		/// <param name="repeatCount">执行的次数；如果<0，则次数为无限</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		protected CancellationTokenSource RepeatByTime(Action action, float perSeconds, int repeatCount, bool ignoreTimeScale = false)
		{
			CancellationTokenSource linkedTokenSource = GetLinkedCancellationTokenSource();
			Base.Timer.DoRepeatByTime(action, perSeconds, repeatCount, linkedTokenSource.Token, ignoreTimeScale).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// 每隔指定的时间间隔，执行一次 action，直到predicate返回true
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perSeconds">每几秒执行一次；下限保底为0.005秒</param>
		/// <param name="predicate">返回 true时，repeat停止</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		protected CancellationTokenSource RepeatByTimeUntil(Action action, float perSeconds, Func<bool> predicate, bool ignoreTimeScale = false)
		{
			CancellationTokenSource linkedTokenSource = GetLinkedCancellationTokenSource();
			Base.Timer.DoRepeatByTimeUntil(action, perSeconds, predicate, linkedTokenSource.Token, ignoreTimeScale).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// 每隔指定的帧数，执行一次 action
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perFrames">每几帧执行一次，下限保底为1</param>
		/// <param name="repeatCount">执行的次数；如果<0，则次数为无限</param>
		protected CancellationTokenSource RepeatByFrame(Action action, int perFrames, int repeatCount)
		{
			CancellationTokenSource linkedTokenSource = GetLinkedCancellationTokenSource();
			Base.Timer.DoRepeatByFrame(action, perFrames, repeatCount, linkedTokenSource.Token).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// 每隔指定的帧数，执行一次 action，直到predicate返回true
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perFrames">每几帧执行一次，下限保底为1</param>
		/// <param name="predicate">返回 true时，repeat停止</param>
		protected CancellationTokenSource RepeatByFrameUntil(Action action, int perFrames, Func<bool> predicate)
		{
			CancellationTokenSource linkedTokenSource = GetLinkedCancellationTokenSource();
			Base.Timer.DoRepeatByFrameUntil(action, perFrames, predicate, linkedTokenSource.Token).Forget();
			return linkedTokenSource;
		}

		private CancellationTokenSource GetLinkedCancellationTokenSource()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _CancelTokenSourceOnHide.Token, destroyCancellationToken);
			return linkedTokenSource;
		}

		#endregion
	}
}
