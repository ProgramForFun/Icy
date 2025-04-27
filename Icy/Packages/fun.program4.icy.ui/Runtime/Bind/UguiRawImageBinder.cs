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
			bindableData.Bind(listener);
		}
		public static void Unbind(this RawImage rawImage, BindableData<string> bindableData)
		{
			Action<string> listener = UguiBindManager.Instance.Unbind(rawImage, bindableData) as Action<string>;
			bindableData.Unbind(listener);
		}
	}
}
