using Icy.Base;
using System;
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// Ugui Slider的Bind扩展
	/// </summary>
	public static class UguiSliderBinder
	{
		#region float
		public static void Bind(this Slider slider, BindableData<float> bindableData)
		{
			Action<float> listener = (float newValue) => { slider.value = newValue; };
			UguiBindManager.Instance.Bind(slider, bindableData, listener);
			bindableData.Bind(listener);
		}
		public static void Unbind(this Slider slider, BindableData<float> bindableData)
		{
			Action<float> listener = UguiBindManager.Instance.Unbind(slider, bindableData) as Action<float>;
			bindableData.Unbind(listener);
		}
		#endregion

		#region double
		public static void Bind(this Slider slider, BindableData<double> bindableData)
		{
			Action<double> listener = (double newValue) => { slider.value = (float)newValue; };
			UguiBindManager.Instance.Bind(slider, bindableData, listener);
			bindableData.Bind(listener);
		}
		public static void Unbind(this Slider slider, BindableData<double> bindableData)
		{
			Action<double> listener = UguiBindManager.Instance.Unbind(slider, bindableData) as Action<double>;
			bindableData.Unbind(listener);
		}
		#endregion
	}
}
