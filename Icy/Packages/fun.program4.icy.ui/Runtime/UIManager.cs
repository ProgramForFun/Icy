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


using Cysharp.Threading.Tasks;
using Icy.Base;
using Icy.Asset;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Threading;

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
			/// UI的Type
			/// </summary>
			public Type Type;
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
		/// UI相机
		/// </summary>
		public Camera UICamera => UIRoot.Instance.UICamera;
		/// <summary>
		/// 在UI初始化和打开的过程中，如果有报错，会触发这个事件；
		/// 方便业务侧决定后续处理，比如说直接销毁这个UI，避免卡住整个流程；
		/// </summary>
		public event Action<UIBase, Exception> OnUIOpenException;
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
		/// <summary>
		/// 屏蔽UI输入的物体
		/// </summary>
		private GameObject _Block;
		/// <summary>
		/// 屏蔽超时关闭的CancelToken
		/// </summary>
		private CancellationTokenSource _BlockTimeoutCancelToken;

		/// <summary>
		/// 运行时为Image、RawImage加载的的Sprite和Texture和所属UI的映射关系
		/// </summary>
		private Dictionary<UIBase, Dictionary<string, AssetRef>> _SpriteTextureOfUI;


		protected override void OnInitialized()
		{
			_UIMap = new Dictionary<UIBase, UIData>();
			_Stack = new Stack<UIData>();
			_StackTmp = new Stack<UIData>();
			_SortingOrderOffset = new Dictionary<UILayer, int>();
			_SpriteTextureOfUI = new Dictionary<UIBase, Dictionary<string, AssetRef>>();

			Type UILayerType = typeof(UILayer);
			Array renderQueues = Enum.GetValues(UILayerType);
			for (int i = 0; i < renderQueues.Length; i++)
			{
				object value = renderQueues.GetValue(i);
				string layerName = Enum.GetName(UILayerType, value);
				_SortingOrderOffset[(UILayer)value] = 0;
			}

			//创建屏蔽UI输入的block
			GameObject topLayerGo = UIRoot.Instance.GetLayerGameObject(UILayer.Top);
			_Block = new GameObject("Block", typeof(Empty4RaycastTarget), typeof(GraphicRaycaster));
			CommonUtility.SetParent(topLayerGo.transform.parent, _Block.transform);
			RectTransform blockRectTrans = _Block.GetOrAddComponent<RectTransform>();
			blockRectTrans.anchorMin = Vector2.zero;
			blockRectTrans.anchorMax = Vector2.one;
			blockRectTrans.offsetMin = Vector2.zero;
			blockRectTrans.offsetMax = Vector2.zero;
			Canvas blockCanvas = _Block.GetOrAddComponent<Canvas>();
			blockCanvas.overrideSorting = true;
			blockCanvas.sortingOrder = (int)UILayer.Top + 1000;
			CancelBlockInteract();
		}

		/// <summary>
		/// 获取一个UI（async await风格）；
		/// 如果未打开就创建，如果已打开就返回现有的
		/// </summary>
		public async UniTask<T> GetAsync<T>() where T : UIBase
		{
			Type uiType = typeof(T);
			UIBase ui = GetFromUIMap(uiType);
			if (ui != null)
				return ui as T;

			return await LoadUI(uiType) as T;
		}

		/// <summary>
		/// 获取一个UI（回调函数风格）；
		/// 如果未打开就创建，如果已打开就返回现有的
		/// </summary>
		public void Get<T>(Action<UIBase> callback) where T : UIBase
		{
			Type uiType = typeof(T);
			Get(uiType, callback);
		}

		/// <summary>
		/// 显示UI（async await风格）；
		/// </summary>
		/// <param name="param">UI显示时传入的参数</param>
		/// <param name="blockInteract">是否屏蔽输入，直到UI打开</param>
		public async UniTask<T> ShowAsync<T>(IUIParam param = null, bool blockInteract = true) where T : UIBase
		{
			if (blockInteract)
				BlockInteract();
			T ui = await GetAsync<T>();

			try
			{
				ui.Show(param);
				return ui;
			}
			catch (Exception ex)
			{
				Log.Error($"Show<{typeof(T).Name}> failed, exception = {ex}", nameof(UIManager));
				OnUIOpenException?.Invoke(ui, ex);
			}
			finally
			{
				//保证一定会解除block
				if (blockInteract)
					CancelBlockInteract();
			}
			return null;
		}

		/// <summary>
		/// 显示UI（回调函数风格）；
		/// </summary>
		/// <param name="param">UI显示时传入的参数</param>
		/// <param name="blockInteract">是否屏蔽输入，直到UI打开</param>
		/// <param name="callback">UI打开后的回调函数，如果Show UI出现异常，会返回null</param>
		public void Show<T>(IUIParam param = null, bool blockInteract = true, Action<UIBase> callback = null) where T : UIBase
		{
			if (blockInteract)
				BlockInteract();

			Get(typeof(T), (UIBase ui) => 
			{
				try
				{
					ui.Show(param);
					callback?.Invoke(ui);
				}
				catch (Exception ex)
				{
					Log.Error($"Show<{typeof(T).Name}> failed, exception = {ex}", nameof(UIManager));
					OnUIOpenException?.Invoke(ui, ex);
					callback?.Invoke(null);
				}
				finally
				{
					//保证一定会解除block
					if (blockInteract)
						CancelBlockInteract();
				}
			});
		}

		/// <summary>
		/// 获取并等待predicate满足，然后显示UI
		/// </summary>
		/// <param name="param">UI显示时传入的参数</param>
		/// <param name="predicate">显示的条件</param>
		/// <param name="blockInteract">是否屏蔽输入，直到UI打开</param>
		public async UniTask<T> ShowUntil<T>(IUIParam param, Func<bool> predicate, bool blockInteract = false) where T : UIBase
		{
			if (blockInteract)
				BlockInteract();

			T ui = await GetAsync<T>();
			await UniTask.WaitUntil(predicate);

			try
			{
				ui.Show(param);
				return ui;
			}
			catch (Exception ex)
			{
				Log.Error($"ShowUntil<{typeof(T).Name}> failed, exception = {ex}", nameof(UIManager));
				OnUIOpenException?.Invoke(ui, ex);
			}
			finally
			{
				//保证一定会解除block
				if (blockInteract)
					CancelBlockInteract();
			}
			return null;
		}

		/// <summary>
		/// 隐藏UI
		/// </summary>
		public void Hide<T>() where T : UIBase
		{
			UIBase ui = GetFromUIMap(typeof(T));
			if (ui != null)
				ui.Hide();
		}

		/// <summary>
		/// 隐藏当前UI并显示前一个UI
		/// </summary>
		public void HideToPrev()
		{
			UIBase ui = GetHideableOrDestroyableUIFromStackTop();
			if (ui != null)
			{
				//Dialog才需要开前一个界面，Popup的前一个Dialog不会关闭，不需要打开前一个
				if (ui.UIType == UIType.Dialog)
					ShowPrev((UIBase prevUI) => { ui.Hide(); });
			}
		}

		/// <summary>
		/// 销毁UI
		/// </summary>
		public void Destroy<T>() where T : UIBase
		{
			UIBase ui = GetFromUIMap(typeof(T));
			if (ui != null)
				ui.Destroy();
		}

		/// <summary>
		/// 销毁当前UI并显示前一个UI
		/// </summary>
		public void DestroyToPrev()
		{
			UIBase ui = GetHideableOrDestroyableUIFromStackTop();
			if (ui != null)
			{
				//Dialog才需要开前一个界面，Popup的前一个Dialog不会关闭，不需要打开前一个
				if (ui.UIType == UIType.Dialog)
					ShowPrev((UIBase prevUI) => { ui.Destroy(); });
			}
		}

		private void Get(Type uiType, Action<UIBase> callback)
		{
			UIBase ui = GetFromUIMap(uiType);
			if (ui == null)
				LoadUI(uiType, callback).Forget();
			else
				callback?.Invoke(ui);
		}

		/// <summary>
		/// 根据UI类型从UIMap中获取UI实例
		/// </summary>
		private UIBase GetFromUIMap(Type uiType)
		{
			foreach (KeyValuePair<UIBase, UIData> item in _UIMap)
			{
				if (item.Value.Type == uiType)
					return item.Key;
			}
			return null;
		}

		private async UniTask<UIBase> LoadUI(Type uiType, Action<UIBase> callback = null)
		{
			string uiName = uiType.Name;
			AssetRef assetRef = AssetManager.Instance.LoadAssetAsync(uiName);
			await assetRef.ToUniTask();
			assetRef.Retain();

			GameObject uiGo = GameObject.Instantiate(assetRef.AssetObject as GameObject);
			string uiGoName = uiGo.name;
			if (uiGoName.EndsWith("(Clone)"))
				uiGo.name = uiGoName.Substring(0, uiGoName.Length - 7);//"(Clone)"的长度

			UIBase uiBase = uiGo.GetComponent<UIBase>();
			if (uiBase == null)
				Log.Error($"{uiName} is Not a UI prefab", nameof(UIManager));
			InitUI(uiType, uiBase, assetRef);
			callback?.Invoke(uiBase);

			EventParam_Type eventParam = EventManager.GetParam<EventParam_Type>();
			eventParam.Value = uiType;
			EventManager.Trigger(EventDefine.UILoaded, eventParam);

			return uiBase;
		}

		/// <summary>
		/// 指定UI是否已经创建出来，无关显示与否
		/// </summary>
		public bool IsCreated<T>() where T : UIBase
		{
			return GetFromUIMap(typeof(T)) != null;
		}

		/// <summary>
		/// 指定UI是否正在显示
		/// </summary>
		public bool IsShowing<T>() where T : UIBase
		{
			UIBase ui = GetFromUIMap(typeof(T));
			if (ui != null)
				return ui.IsShowing;

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
		/// <param name="except">不清除的UI集合</param>
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
		/// 屏蔽所有UI交互
		/// </summary>
		/// <param name="timeout">超时时间，单位秒，默认-1为无超时时间</param>
		public void BlockInteract(int timeout = -1)
		{
			_Block.SetActive(true);

			_BlockTimeoutCancelToken?.Cancel();
			if (timeout != -1)
				_BlockTimeoutCancelToken = Base.Timer.DelayByTime(CancelBlockInteract, timeout);
		}

		/// <summary>
		/// 关闭屏蔽
		/// </summary>
		public void CancelBlockInteract()
		{
			_Block.SetActive(false);
			_BlockTimeoutCancelToken?.Cancel();
			_BlockTimeoutCancelToken = null;
		}

		/// <summary>
		/// 初始化UI
		/// </summary>
		private void InitUI(Type uiType, UIBase newUI, AssetRef assetRef)
		{
			if (newUI == null)
				return;
			string uiName = uiType.Name;

			UIData newUIState = new UIData();
			newUIState.Name = uiName;
			newUIState.Type = uiType;
			newUIState.Param = null;
			newUIState.AssetRef = assetRef;
			_UIMap[newUI] = newUIState;

			GameObject layerGo = UIRoot.Instance.GetLayerGameObject(newUI.UILayer);
			CommonUtility.SetParent(layerGo, newUI.gameObject);

			newUI.UIName = uiName;
			try
			{
				newUI.Init();
				newUI.DoHide();
			}
			catch (Exception ex)
			{
				Log.Error($"Init {uiName} failed, Exception = {ex}", nameof(UIManager));
				OnUIOpenException?.Invoke(newUI, ex);
			}

#if UNITY_EDITOR
			ValidateUICode(uiName, newUI);
#endif
		}

		internal void Show(UIBase ui, IUIParam param)
		{
			PushStack(ui);

			UIData uiData = _UIMap[ui];
			uiData.Param = param;

			ui.Canvas.overrideSorting = true;
			ui.Canvas.sortingOrder = (int)ui.UILayer + _SortingOrderOffset[ui.UILayer];
			_SortingOrderOffset[ui.UILayer] += SORTING_ORDER_OFFSET_PER_UI;

			EventParam_Type eventParam = EventManager.GetParam<EventParam_Type>();
			eventParam.Value = uiData.Type;
			EventManager.Trigger(EventDefine.UIShown, eventParam);

			Log.Info($"Show {ui.UIName}", nameof(UIManager));
		}

		internal void Hide(UIBase ui)
		{
			DecreaseSortingOrderOffset(ui.UILayer);

			UIData uiData = _UIMap[ui];

			EventParam_Type eventParam = EventManager.GetParam<EventParam_Type>();
			eventParam.Value = uiData.Type;
			EventManager.Trigger(EventDefine.UIHid, eventParam);
		}

		internal void ShowPrev(Action<UIBase> callbackBeforePrevUIShow)
		{
			//先弹出栈顶，这个是刚刚Hide或者Destroy的，下一个才是前一个UI
			PopStack();

			UIData prev = PopStack();
			if (prev != null)
			{
				Get(prev.Type, (UIBase ui) =>
				{
					try
					{
						callbackBeforePrevUIShow?.Invoke(ui);
						ui.Show(prev.Param);
					}
					catch (Exception ex)
					{
						Log.Error($"ShowPrev {prev.Name} failed, exception = {ex}", nameof(UIManager));
						OnUIOpenException?.Invoke(ui, ex);
					}
				});
			}
		}

		internal void Destroy(UIBase ui)
		{
			if (_UIMap.ContainsKey(ui))
			{
				UIData uiData = _UIMap[ui];
				_UIMap.Remove(ui);
				uiData.AssetRef.Release();

				EventParam_Type eventParam = EventManager.GetParam<EventParam_Type>();
				eventParam.Value = uiData.Type;
				EventManager.Trigger(EventDefine.UIDestroyed, eventParam);
			}

			if (_SpriteTextureOfUI.ContainsKey(ui))
			{
				foreach (KeyValuePair<string, AssetRef> item in _SpriteTextureOfUI[ui])
					item.Value.Release();
				_SpriteTextureOfUI.Remove(ui);
			}
		}

		private void PushStack(UIBase ui)
		{
			if (ui.UIType == UIType.Dialog)
			{
				_Stack.Push(_UIMap[ui]);
				RemoveDuplicateInStack();
				Log.Info($"Push {ui.UIName}, stack count = {_Stack.Count}", nameof(UIManager));
			}
		}

		private UIData PopStack()
		{
			if (_Stack.Count > 0)
			{
				UIData top = _Stack.Peek();
				if (_StackBottomUI == null || top.Name != _StackBottomUI.Name)
				{
					Log.Info($"Pop {top.Name}, stack count = {_Stack.Count - 1}", nameof(UIManager));
					return _Stack.Pop();
				}
			}
			return null;
		}

		private UIBase GetHideableOrDestroyableUIFromStackTop()
		{
			UIData top = _Stack.Peek();
			UIBase ui = GetFromUIMap(top.Type);
			if (ui != null && (_StackBottomUI == null || top.Name != _StackBottomUI.Name))
				return ui;
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

		private void DecreaseSortingOrderOffset(UILayer layer)
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

		internal Sprite GetSprite(UIBase ui, string spriteName)
		{
			if (_SpriteTextureOfUI.ContainsKey(ui) && _SpriteTextureOfUI[ui].ContainsKey(spriteName))
				return _SpriteTextureOfUI[ui][spriteName].AssetObject as Sprite;
			else
			{
				AssetRef spriteAsset = AssetManager.Instance.LoadAsset(spriteName);
				spriteAsset.Retain();
				if (!_SpriteTextureOfUI.ContainsKey(ui))
					_SpriteTextureOfUI[ui] = new Dictionary<string, AssetRef>();
				_SpriteTextureOfUI[ui].Add(spriteName, spriteAsset);
				return spriteAsset.AssetObject as Sprite;
			}
		}

		internal Texture GetTexture(UIBase ui, string textureName)
		{
			if (_SpriteTextureOfUI.ContainsKey(ui) && _SpriteTextureOfUI[ui].ContainsKey(textureName))
				return _SpriteTextureOfUI[ui][textureName].AssetObject as Texture;
			else
			{
				AssetRef textureAsset = AssetManager.Instance.LoadAsset(textureName);
				textureAsset.Retain();
				if (!_SpriteTextureOfUI.ContainsKey(ui))
					_SpriteTextureOfUI[ui] = new Dictionary<string, AssetRef>();
				_SpriteTextureOfUI[ui].Add(textureName, textureAsset);
				return textureAsset.AssetObject as Texture;
			}
		}

#if UNITY_EDITOR
		/// <summary>
		/// 对UI代码进行一些合法性检查
		/// </summary>
		/// <param name="uiName"></param>
		private void ValidateUICode(string uiName, UIBase ui)
		{
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetEditorOnlySettingDir(), SettingsHelper.UISetting);
			if (bytes == null)
				Log.Error("Can not find UI setting");
			else
			{
				UISetting uiSetting = UISetting.Parser.ParseFrom(bytes);
				if (string.IsNullOrEmpty(uiSetting.UIRootDir))
				{
					Log.Error($"Please set UI root path first. Go to menu Icy/UI/Setting to set it");
					return;
				}
				//基于正则匹配简单检查BindableData的BindTo和UnbindTo的配对情况，避免内存泄露
				string nameWithoutPrefixUI = uiName.Substring(2);
				string uiScriptPath = Path.Combine(uiSetting.UIRootDir, nameWithoutPrefixUI, uiName + ".cs");
				ValidateBindableData(uiName, uiScriptPath);
				string uiLogicScriptPath = Path.Combine(uiSetting.UIRootDir, nameWithoutPrefixUI, uiName + "Logic.cs");
				ValidateBindableData(uiName, uiLogicScriptPath);
			}
		}

		private void ValidateBindableData(string scriptName, string scriptPath)
		{
			if (File.Exists(scriptPath))
			{
				string allCodes = File.ReadAllText(scriptPath);
				string bind = Regex.Escape(".BindTo(");
				string unbind = Regex.Escape(".UnbindTo(");
				Regex regexBind = new Regex(bind);
				Regex regexUnbind = new Regex(unbind);
				int bindCount = regexBind.Matches(allCodes).Count;
				int unbindCount = regexUnbind.Matches(allCodes).Count;
				if (bindCount > unbindCount)
					Log.Error($"{scriptName} may be leaked memory, BindTo count = {bindCount}, UnbindTo count = {unbindCount}");
			}
		}
#endif
	}
}
