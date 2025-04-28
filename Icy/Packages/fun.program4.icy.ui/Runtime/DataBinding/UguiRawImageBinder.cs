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
		public static void Bind(this RawImage rawImage, BindableData<string> bindableData)
		{
			Action<string> listener = (string newValue) => { rawImage.SetTexture(newValue); };
			UguiBindManager.Instance.Bind(rawImage, bindableData, listener);
		}

		public static void Bind<T>(this RawImage rawImage, BindableData<T> bindableData, Func<BindableData<T>, string> predicate)
		{
			Action<T> listener = (T newValue) => { rawImage.SetTexture(predicate(bindableData)); };
			UguiBindManager.Instance.Bind(rawImage, bindableData, listener);
		}

		public static void Unbind<T>(this RawImage rawImage, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.Unbind(rawImage, bindableData);
		}
	}
}
