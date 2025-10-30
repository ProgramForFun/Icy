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


using Cysharp.Text;
using Icy.Base;
using System;
using System.Collections.Generic;
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
			EventManager.AddListener(EventDefine.UIHid, OnUIHid);
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

				bindableData.Bind(listener);

				return true;
			}

			Log.Error($"Duplicate binding, BindableData T = {typeof(T).Name}, listener = {listener.Target.GetType().Name}.{listener.Method.Name}", "UguiBindManager");
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

				bindableData.Unbind(listener);
				return true;
			}
			return false;
		}

		/// <summary>
		/// 序列化输出当前所有Bind，方便调试；
		/// 有大量文本操作，在性能敏感的场景慎用
		/// </summary>
		internal string Dump()
		{
			Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder();
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
		private void OnUIHid(int eventID, IEventParam param)
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
						bindableData.Unbind((dynamic)bindData.Listener);
						_BindDataList.RemoveAt(i);
					}
				}
			}
		}
	}
}
