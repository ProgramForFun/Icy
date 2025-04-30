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
		/// <summary>
		/// Ugui组件
		/// </summary>
		private List<object> _UguiCompList;
		/// <summary>
		/// Bind的数据
		/// </summary>
		private List<object> _BindableList;
		/// <summary>
		/// 监听数据变化的listener
		/// </summary>
		private List<object> _ListenerList;

		protected override void OnInitialized()
		{
			base.OnInitialized();
			_UguiCompList = new List<object>();
			_BindableList = new List<object>();
			_ListenerList = new List<object>();
		}

		/// <summary>
		/// 指定的Ugui组件和数据，是否已经Bind过了
		/// </summary>
		internal bool AlreadyBinded(object uguiComp, object bindableData)
		{
			bool contains = _UguiCompList.Contains(uguiComp) && _BindableList.Contains(bindableData);
			return contains;
		}

		/// <summary>
		/// Bind指定的Ugui组件和数据
		/// </summary>
		internal bool Bind<T>(object uguiComp, BindableData<T> bindableData, Action<T> listener)
		{
			if (!AlreadyBinded(uguiComp, bindableData))
			{
				_UguiCompList.Add(uguiComp);
				_BindableList.Add(bindableData);
				_ListenerList.Add(listener);

				bindableData.Bind(listener);

				return true;
			}

			Log.LogError($"Duplicate binding, BindableData T = {typeof(T).Name}, listener = {listener.Target.GetType().Name}.{listener.Method.Name}", "UguiBindManager");
			return false;
		}

		/// <summary>
		/// 解除Bind指定的Ugui组件和数据
		/// </summary>
		internal bool Unbind<T>(object uguiComp, BindableData<T> bindableData)
		{
			if (AlreadyBinded(uguiComp, bindableData))
			{
				int idx = _UguiCompList.IndexOf(uguiComp);
				Action<T> listener = _ListenerList[idx] as Action<T>;
				_UguiCompList.RemoveAt(idx);
				_BindableList.RemoveAt(idx);
				_ListenerList.RemoveAt(idx);

				bindableData.Unbind(listener);
				return true;
			}
			return false;
		}
	}
}
