using Icy.Base;
using System;
using TMPro;

namespace Icy.UI
{
	/// <summary>
	/// TMP的Bind扩展
	/// </summary>
	public static class UguiTmpTextBinder
	{
		public static void Bind<T>(this TextMeshProUGUI tmp, BindableData<T> bindableData)
		{
			Action<T> listener = (T newValue) => { tmp.text = newValue.ToString(); };
			UguiBindManager.Instance.Bind(tmp, bindableData, listener);
		}

		public static void Bind<T>(this TextMeshProUGUI tmp, BindableData<T> bindableData, Func<BindableData<T>, string> predicate)
		{
			Action<T> listener = (T newValue) => { tmp.text = predicate(bindableData); };
			UguiBindManager.Instance.Bind(tmp, bindableData, listener);
		}

		public static void Unbind<T>(this TextMeshProUGUI tmp, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.Unbind(tmp, bindableData);
		}
	}
}
