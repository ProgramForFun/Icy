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
			bindableData.Bind(listener);
		}
		public static void Unbind(this Toggle toggle, BindableData<bool> bindableData)
		{
			Action<bool> listener = UguiBindManager.Instance.Unbind(toggle, bindableData) as Action<bool>;
			bindableData.Unbind(listener);
		}
	}
}
