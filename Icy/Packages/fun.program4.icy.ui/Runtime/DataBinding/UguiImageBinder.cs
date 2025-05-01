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
		public static void BindTo(this Image image, BindableData<string> bindableData)
		{
			Action<string> listener = (string newValue) => { image.SetSprite(newValue); };
			UguiBindManager.Instance.BindTo(image, bindableData, listener);
		}

		public static void BindTo<T>(this Image rawImage, BindableData<T> bindableData, Func<BindableData<T>, string> predicate)
		{
			Action<T> listener = (T newValue) => { rawImage.SetSprite(predicate(bindableData)); };
			UguiBindManager.Instance.BindTo(rawImage, bindableData, listener);
		}

		public static void UnbindTo<T>(this Image image, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.UnbindTo(image, bindableData);
		}
	}
}
