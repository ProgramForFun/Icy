using Icy.Base;
using System;
using System.Collections.Generic;

namespace Icy.UI
{
	/// <summary>
	/// 管理UI组件bind数据的映射关系
	/// </summary>
	internal sealed class UguiBindManager : Singleton<UguiBindManager>
	{
		private struct BindData
		{
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
	}
}
