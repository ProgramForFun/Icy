using Icy.Base;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// 管理UI组件bind数据的映射关系
	/// </summary>
	internal sealed class UguiBindManager : Singleton<UguiBindManager>
	{
		private struct BindData
		{
			public string UIName;
			public object UguiComp;
			public object BindableData;
			public object Listener;
		}

		private List<BindData> _BindDataList;

		/// <summary>
		/// 预分配List大小
		/// </summary>
		private const int DEFAULT_LIST_SIZE = 32;

		protected override void OnInitialized()
		{
			base.OnInitialized();
			_BindDataList = new List<BindData>(DEFAULT_LIST_SIZE);
			EventManager.AddListener(EventDefine.UIHided, OnUIHided);
		}

		/// <summary>
		/// 指定的Ugui组件和数据的Index
		/// </summary>
		internal int GetIndex(object uguiComp, object bindableData)
		{
			for (int i = 0; i < _BindDataList.Count; i++)
			{
				if (_BindDataList[i].UguiComp == uguiComp && _BindDataList[i].BindableData == bindableData)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Bind指定的Ugui组件和数据
		/// </summary>
		internal bool BindTo<T>(object uguiComp, BindableData<T> bindableData, Action<T> listener)
		{
			if (GetIndex(uguiComp, bindableData) < 0)
			{
				BindData data = new BindData();

				//如果新增了可Bind的UI组件，可能需要扩展这里
				Transform trans;
				if (uguiComp is Selectable selectable)
					trans = selectable.transform;
				else if (uguiComp is Graphic graphic)
					trans = graphic.transform;
				else
				{
					Log.Assert(false, $"Not supported ugui component type {uguiComp.GetType()}");
					return false;
				}

				UIBase ui = UIUtility.GetUIFromParent(trans);
				data.UIName = ui.UIName;
				data.UguiComp = uguiComp;
				data.BindableData = bindableData;
				data.Listener = listener;
				_BindDataList.Add(data);

				bindableData.BindTo(listener);

				return true;
			}

			Log.LogError($"Duplicate binding, BindableData T = {typeof(T).Name}, listener = {listener.Target.GetType().Name}.{listener.Method.Name}", "UguiBindManager");
			return false;
		}

		/// <summary>
		/// 解除Bind指定的Ugui组件和数据
		/// </summary>
		internal bool UnbindTo<T>(object uguiComp, BindableData<T> bindableData)
		{
			int dataIdx = GetIndex(uguiComp, bindableData);
			if (dataIdx >= 0)
			{
				BindData data = _BindDataList[dataIdx];
				Action<T> listener = data.Listener as Action<T>;
				_BindDataList.RemoveAt(dataIdx);

				bindableData.UnbindTo(listener);
				return true;
			}
			return false;
		}

		/// <summary>
		/// 序列化输出当前所有Bind，方便调试；
		/// 内部实现有反射，注意谨慎在性能敏感的场景使用
		/// </summary>
		internal string Dump()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < _BindDataList.Count; i++)
			{
				stringBuilder.AppendLine();
				BindData data = _BindDataList[i];
				dynamic uiComp = data.UguiComp;
				dynamic bindableData = data.BindableData;
				stringBuilder.AppendLine($"{data.UIName}.{uiComp.gameObject.name} : ");
				stringBuilder.Append(bindableData.Dump());
			}
			return stringBuilder.ToString();
		}

		/// <summary>
		/// UI隐藏时，自动Unbind该UI所属UI组件
		/// </summary>
		private void OnUIHided(int eventID, IEventParam param)
		{
			if (param is EventParam_Type paramType)
			{
				string uiName = paramType.Value.Name;
				int i = _BindDataList.Count - 1;
				for (; i >= 0; i--)
				{
					if (_BindDataList[i].UIName == uiName)
					{
						BindData bindData = _BindDataList[i];
						dynamic bindableData = bindData.BindableData;
						bindableData.UnbindTo((dynamic)bindData.Listener);
						_BindDataList.RemoveAt(i);
					}
				}
			}
		}
	}
}
