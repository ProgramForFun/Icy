using UnityEngine;
using Icy.Base;

namespace Icy.UI
{
	/// <summary>
	/// UI基类
	/// </summary>
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
				Log.LogError("Duplicate call Init, UIName = " + UIName, "UIBase");
				return;
			}
			RectTransform = transform as RectTransform;
			RectTransform.anchorMin = Vector2.zero;
			RectTransform.anchorMax = Vector2.one;
			RectTransform.offsetMin = Vector2.zero;
			RectTransform.offsetMax = Vector2.zero;

			Canvas = gameObject.GetOrAddComponent<Canvas>();
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
				case UIHideType.Transparent:
					_CanvasGroup.alpha = _OriginalAlpha;
					_CanvasGroup.interactable = true;
					break;
				default:
					//do nothing
					break;
			}

			UIManager.Instance.Show(this, param);
		}

		public virtual void Hide()
		{
			if (!IsShowing)
				return;
			IsShowing = false;

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
				case UIHideType.Transparent:
					_CanvasGroup.alpha = 0.0f;
					_CanvasGroup.interactable = false;
					break;
				default:
					Log.LogError($"Invalid HideType {HideType}", "UIBase");
					break;
			}
		}

		public void HideToPrev()
		{
			Hide();
			UIManager.Instance.ShowPrev(this);
		}

		public virtual void Destroy()
		{
			if (IsDestroyed)
			{
				Log.LogError($"Duplicate Destroy to {GetType().Name}", "UIBase");
				return;
			}

			if (IsShowing)
				Hide();

			IsDestroyed = true;
			UIManager.Instance.Destroy(this);

			UnityEngine.Object.Destroy(this.gameObject);
		}

		public void DestroyToPrev()
		{
			Destroy();
			UIManager.Instance.ShowPrev(this);
		}

		protected void OnDestroy()
		{
			if (!IsDestroyed && !_IsExitingPlayMode)
				Log.LogError($"UI GameObject of {GetType().Name} is unexpected destroyed", "UIBase");
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
	}
}
