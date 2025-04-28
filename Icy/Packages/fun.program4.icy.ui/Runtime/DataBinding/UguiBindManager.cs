using Icy.Base;
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
		internal void Bind(object uguiComp, object bindableData, object listener)
		{
			if (!AlreadyBinded(uguiComp, bindableData))
			{
				_UguiCompList.Add(uguiComp);
				_BindableList.Add(bindableData);
				_ListenerList.Add(listener);
			}
		}

		/// <summary>
		/// 解除Bind指定的Ugui组件和数据
		/// </summary>
		internal object Unbind(object uguiComp, object bindableData)
		{
			if (AlreadyBinded(uguiComp, bindableData))
			{
				int idx = _UguiCompList.IndexOf(uguiComp);
				object rtn = _ListenerList[idx];
				_UguiCompList.RemoveAt(idx);
				_BindableList.RemoveAt(idx);
				_ListenerList.RemoveAt(idx);
				return rtn;
			}
			return null;
		}
	}
}
