using Icy.Base;
using System;
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// Ugui RawImage的Bind扩展
	/// </summary>
	public static class UguiRawImageBinder
	{
		public static void BindTo(this RawImage rawImage, BindableData<string> bindableData)
		{
			Action<string> listener = (string newValue) => { rawImage.SetTexture(newValue); };
			UguiBindManager.Instance.BindTo(rawImage, bindableData, listener);
		}

		public static void BindTo<T>(this RawImage rawImage, BindableData<T> bindableData, Func<BindableData<T>, string> predicate)
		{
			Action<T> listener = (T newValue) => { rawImage.SetTexture(predicate(bindableData)); };
			UguiBindManager.Instance.BindTo(rawImage, bindableData, listener);
		}

		public static void UnbindTo<T>(this RawImage rawImage, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.UnbindTo(rawImage, bindableData);
		}
	}
}
