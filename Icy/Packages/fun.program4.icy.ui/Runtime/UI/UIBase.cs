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

namespace Icy.UI
{
	/// <summary>
	/// UI基类；
	/// 派生自UniTaskMonoBehaviour，因此Timer和UniTask相关接口应该优先使用UniTaskMonoBehaviour中封装好的
	/// </summary>
	[InfoBox("Modifying component directly is disabled, re-generate ui code to achieve it")]
	[RequireComponent(typeof(UICodeGenerator))]
	public abstract class UIBase : UniTaskMonoBehaviour
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
		/// 是否是全屏界面
		/// </summary>
		public bool IsFullScreen => _IsFullScreen;
		[SerializeField] protected bool _IsFullScreen = false;
		/// <summary>
		/// 背景模糊
		/// </summary>
		public bool BlurBackground => _BlurBackground;
		[HideIf(nameof(_IsFullScreen))]
		[SerializeField] protected bool _BlurBackground = false;
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

			HideType = UIHideType.Deactivate;
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
				case UIHideType.Deactivate:
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
			SetExternalToken(_CancelTokenSourceOnHide.Token);

			UIManager.Instance.Show(this, param);
			Log.Info($"{UIName} shown", "UI", true);
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
				case UIHideType.Deactivate:
					gameObject.SetActive(false);
					break;
				case UIHideType.MoveOutScreen:
					gameObject.transform.localPosition = MOVE_OUT_POS;
					break;
				default:
					Log.Error($"Invalid HideType {HideType}", nameof(UIBase));
					break;
			}
			Log.Info($"{UIName} hid", "UI", true);
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
			Log.Info($"{UIName} destroyed", "UI", true);
		}

		protected override void OnDestroy()
		{
			if (!IsDestroyed && !_IsExitingPlayMode)
				Log.Error($"UI GameObject of {GetType().Name} is unexpected destroyed", nameof(UIBase));

			for (int i = 0; i < _AllCancelTokens.Count; i++)
				_AllCancelTokens[i].Dispose();

			base.OnDestroy();
		}

#if UNITY_EDITOR
		protected override void OnEnable()
		{
			base.OnEnable();
			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		protected override void OnDisable()
		{
			UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			base.OnDisable();
		}

		private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
		{
			if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
				_IsExitingPlayMode = true;
		}
#endif
	}
}
