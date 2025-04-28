using Icy.Base;
using System;
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// Ugui Toggle的Bind扩展
	/// </summary>
	public static class UguiToggleBinder
	{
		public static void Bind(this Toggle toggle, BindableData<bool> bindableData)
		{
			Action<bool> listener = (bool newValue) => { toggle.isOn = newValue; };
			UguiBindManager.Instance.Bind(toggle, bindableData, listener);
		}

		public static void Bind<T>(this Toggle toggle, BindableData<T> bindableData, Func<BindableData<T>, bool> predicate)
		{
			Action<T> listener = (T newValue) => { toggle.isOn = predicate(bindableData); };
			UguiBindManager.Instance.Bind(toggle, bindableData, listener);
		}

		public static void Unbind<T>(this Toggle toggle, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.Unbind(toggle, bindableData);
		}
	}
}
