using Icy.Base;
using System;
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// Ugui Image的Bind扩展
	/// </summary>
	public static class UguiImageBinder
	{
		public static void Bind(this Image image, BindableData<string> bindableData)
		{
			Action<string> listener = (string newValue) => { image.SetSprite(newValue); };
			UguiBindManager.Instance.Bind(image, bindableData, listener);
		}

		public static void Bind<T>(this Image rawImage, BindableData<T> bindableData, Func<BindableData<T>, string> predicate)
		{
			Action<T> listener = (T newValue) => { rawImage.SetSprite(predicate(bindableData)); };
			UguiBindManager.Instance.Bind(rawImage, bindableData, listener);
		}

		public static void Unbind<T>(this Image image, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.Unbind(image, bindableData);
		}
	}
}
