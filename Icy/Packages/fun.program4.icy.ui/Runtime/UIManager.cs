using Cysharp.Threading.Tasks;
using Icy.Base;
using Icy.Asset;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.UI
{
	/// <summary>
	/// UI管理器
	/// </summary>
	public sealed class UIManager : Singleton<UIManager>
	{
		private class UIData
		{
			/// <summary>
			/// UI的名字
			/// </summary>
			public string Name;
			/// <summary>
			/// UI的打开参数
			/// </summary>
			public IUIParam Param;
			/// <summary>
			/// 资源句柄引用
			/// </summary>
			public AssetRef AssetRef;
		}


		/// <summary>
		/// 当前未销毁的所有UI
		/// </summary>
		private Dictionary<UIBase, UIData> _UIMap;
		/// <summary>
		/// UI回退栈
		/// </summary>
		private Stack<UIData> _Stack;
		/// <summary>
		/// 辅助回退栈的栈
		/// </summary>
		private Stack<UIData> _StackTmp;
		/// <summary>
		/// 在回退栈中一直存在的UI，一般就是游戏主界面
		/// </summary>
		private Type _StackBottomUI;

		/// <summary>
		/// 每个Layer中，Sorting Order偏移值，每显示一个新UI会增加SORTING_ORDER_OFFSET_PER_UI
		/// </summary>
		private Dictionary<UILayer, int> _SortingOrderOffset;
		/// <summary>
		/// 每显示一个UI，Sorting Order Offset的值
		/// </summary>
		private const int SORTING_ORDER_OFFSET_PER_UI = 20;

		protected override void OnInitialized()
		{
			_UIMap = new Dictionary<UIBase, UIData>();
			_Stack = new Stack<UIData>();
			_StackTmp = new Stack<UIData>();
			_SortingOrderOffset = new Dictionary<UILayer, int>();

			Type UILayerType = typeof(UILayer);
			Array renderQueues = Enum.GetValues(UILayerType);
			for (int i = 0; i < renderQueues.Length; i++)
			{
				object value = renderQueues.GetValue(i);
				string layerName = Enum.GetName(UILayerType, value);
				_SortingOrderOffset[(UILayer)value] = 0;
			}
		}

		/// <summary>
		/// 获取一个UI（async await风格）；
		/// 如果未打开就创建，如果已打开就返回现有的
		/// </summary>
		public async UniTask<T> GetAsync<T>() where T : UIBase
		{
			string uiName = typeof(T).Name;
			foreach (KeyValuePair<UIBase, UIData> item in _UIMap)
			{
				if (item.Value.Name == uiName)
					return item.Key as T;
			}

			return await LoadUI(typeof(T).Name) as T;
		}

		/// <summary>
		/// 获取一个UI（回调函数风格）；
		/// 如果未打开就创建，如果已打开就返回现有的
		/// </summary>
		public void Get<T>(Action<UIBase> callback) where T : UIBase
		{
			Type uiType = typeof(T);
			Get(uiType.Name, callback);
		}

		private void Get(string uiName, Action<UIBase> callback)
		{
			foreach (KeyValuePair<UIBase, UIData> item in _UIMap)
			{
				if (item.Value.Name == uiName)
				{
					callback?.Invoke(item.Key);
					return;
				}
			}

			LoadUI(uiName, callback).Forget();
		}

		private async UniTask<UIBase> LoadUI(string uiName, Action<UIBase> callback = null)
		{
			AssetRef assetRef = AssetManager.Instance.LoadAssetAsync(uiName);
			await assetRef.ToUniTask();
			assetRef.Retain();

			GameObject uiGo = GameObject.Instantiate(assetRef.AssetObject as GameObject);
			UIBase uiBase = uiGo.GetComponent<UIBase>();
			if (uiBase == null)
				Log.LogError($"{uiName} is Not a UI prefab", "UIManager");
			InitUI(uiName, uiBase, assetRef);
			callback?.Invoke(uiBase);
			return uiBase;
		}

		/// <summary>
		/// 指定UI是否已经创建出来，无关显示与否
		/// </summary>
		public bool IsCreated<T>() where T : UIBase
		{
			foreach (KeyValuePair<UIBase, UIData> item in _UIMap)
			{
				if (item.Key is T)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 指定UI是否正在显示
		/// </summary>
		public bool IsShowing<T>() where T : UIBase
		{
			foreach (KeyValuePair<UIBase, UIData> item in _UIMap)
			{
				if (item.Key is T)
					return item.Key.IsShowing;
			}
			return false;
		}

		/// <summary>
		/// 设置栈底UI，既在回退栈中一直存在的UI，一般就是游戏主界面
		/// </summary>
		public void SetStackBottomUI<T>() where T: UIBase
		{
			_StackBottomUI = typeof(T);
		}

		/// <summary>
		/// 清空回退栈，除了参数里的
		/// </summary>
		public void ClearStackExcept(HashSet<Type> except)
		{
			if (except == null || except.Count == 0)
				_Stack.Clear();
			else
			{
				_StackTmp.Clear();
				while (_Stack.Count > 0)
				{
					UIData top = _Stack.Pop();
					if (except.Contains(top.GetType()))
						_StackTmp.Push(top);
				}
				while (_StackTmp.Count > 0)
					_Stack.Push(_StackTmp.Pop());
			}
		}

		/// <summary>
		/// 初始化UI
		/// </summary>
		private void InitUI(string uiName, UIBase newUI, AssetRef assetRef)
		{
			if (newUI == null)
				return;

			UIData newUIState = new UIData();
			newUIState.Name = uiName;
			newUIState.Param = null;
			newUIState.AssetRef = assetRef;
			_UIMap[newUI] = newUIState;

			GameObject layerGo = UIRoot.Instance.GetLayerGameObject(newUI.UILayer);
			CommonUtility.SetParent(layerGo, newUI.gameObject);

			newUI.UIName = uiName;
			newUI.Init();
			newUI.DoHide();
		}

		internal void Show(UIBase ui, IUIParam param)
		{
			PushStack(ui);

			UIData uiState = _UIMap[ui];
			uiState.Param = param;

			ui.Canvas.overrideSorting = true;
			ui.Canvas.sortingOrder = (int)ui.UILayer + _SortingOrderOffset[ui.UILayer];
			_SortingOrderOffset[ui.UILayer] += SORTING_ORDER_OFFSET_PER_UI;

			Log.LogInfo($"Show {ui.UIName}", "UIManager");
		}

		internal void Hide(UIBase ui)
		{
			DecreseSortingOrderOffeset(ui.UILayer);
		}

		internal void ShowPrev(UIBase ui)
		{
			if (ui.UIType == UIType.Dialog)
			{
				//先弹出栈顶，这个是刚刚Hide或者Destroy的，下一个才是前一个UI
				PopStack();

				UIData prev = PopStack();
				if (prev != null)
				{
					Get(prev.Name, (UIBase ui) =>
					{
						ui.Show(prev.Param);
					});
				}
			}
		}

		internal void Destroy(UIBase ui)
		{
			if (_UIMap.ContainsKey(ui))
			{
				UIData uiData = _UIMap[ui];
				_UIMap.Remove(ui);
				uiData.AssetRef.Release();
			}
		}

		private void PushStack(UIBase ui)
		{
			if (ui.UIType == UIType.Dialog)
			{
				_Stack.Push(_UIMap[ui]);
				RemoveDuplicateInStack();
				Log.LogInfo($"Push {ui.UIName}, stack count = {_Stack.Count}", "UIManager");
			}
		}

		private UIData PopStack()
		{
			if (_Stack.Count > 0)
			{
				UIData top = _Stack.Peek();
				if (_StackBottomUI == null || top.Name != _StackBottomUI.Name)
				{
					Log.LogInfo($"Pop {top.Name}, stack count = {_Stack.Count - 1}", "UIManager");
					return _Stack.Pop();
				}
			}
			return null;
		}

		/// <summary>
		/// 如果stack里存在重复UI了，把重复UI之间的UI都移除掉，同时重复UI只保留一个
		/// </summary>
		private void RemoveDuplicateInStack()
		{
			UIData newUIData = _Stack.Peek();

			int count = 0;
			foreach (UIData ui in _Stack)
			{
				if (ui == newUIData)
					count++;
			}

			if (count > 1)
			{
				//先把最新加进来的重复UI弹出，然后pop到第一个重复UI为止
				_Stack.Pop();
				while (_Stack.Peek() != newUIData)
					_Stack.Pop();
			}
		}

		private void DecreseSortingOrderOffeset(UILayer layer)
		{
			int maxSortingOrder = 0;
			foreach (var item in _UIMap)
			{
				if (item.Key.IsShowing && item.Key.UILayer == layer)
				{
					if (item.Key.Canvas.sortingOrder > maxSortingOrder)
						maxSortingOrder = item.Key.Canvas.sortingOrder;
				}
			}
			_SortingOrderOffset[layer] = maxSortingOrder;
		}
	}
}
