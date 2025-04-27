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
			bindableData.Bind(listener);
		}
		public static void Unbind(this Image image, BindableData<string> bindableData)
		{
			Action<string> listener = UguiBindManager.Instance.Unbind(image, bindableData) as Action<string>;
			bindableData.Unbind(listener);
		}
	}
}
