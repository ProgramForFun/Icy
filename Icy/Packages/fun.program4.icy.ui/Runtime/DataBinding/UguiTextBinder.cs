using Icy.Base;
using System;
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// Ugui Text的Bind扩展
	/// </summary>
	public static class UguiTextBinder
	{
		public static void BindTo<T>(this Text text, BindableData<T> bindableData)
		{
			Action<T> listener = (T newValue) => { text.text = newValue.ToString(); };
			UguiBindManager.Instance.BindTo(text, bindableData, listener);
		}

		public static void BindTo<T>(this Text text, BindableData<T> bindableData, Func<BindableData<T>, string> predicate)
		{
			Action<T> listener = (T newValue) => { text.text = predicate(bindableData); };
			UguiBindManager.Instance.BindTo(text, bindableData, listener);
		}

		public static void UnbindTo<T>(this Text text, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.UnbindTo(text, bindableData);
		}
	}
}
