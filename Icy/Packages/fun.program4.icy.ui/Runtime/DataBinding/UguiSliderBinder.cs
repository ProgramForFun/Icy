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
		public static void Bind(this Slider slider, BindableData<float> bindableData)
		{
			Action<float> listener = (float newValue) => { slider.value = newValue; };
			UguiBindManager.Instance.Bind(slider, bindableData, listener);
		}

		public static void Bind(this Slider slider, BindableData<double> bindableData)
		{
			Action<double> listener = (double newValue) => { slider.value = (float)newValue; };
			UguiBindManager.Instance.Bind(slider, bindableData, listener);
		}

		public static void Bind<T>(this Slider slider, BindableData<T> bindableData, Func<BindableData<T>, float> predicate)
		{
			Action<T> listener = (T newValue) => { slider.value = predicate(bindableData); };
			UguiBindManager.Instance.Bind(slider, bindableData, listener);
		}

		public static void Unbind<T>(this Slider slider, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.Unbind(slider, bindableData);
		}
	}
}
