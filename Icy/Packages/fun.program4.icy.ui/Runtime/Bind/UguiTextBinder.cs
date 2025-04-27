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
		#region string
		public static void Bind(this Text text, BindableData<string> bindableData)
		{
			Bind<string>(text, bindableData);
		}
		public static void Unbind(this Text text, BindableData<string> bindableData)
		{
			Unbind<string>(text, bindableData);
		}
		#endregion

		#region int
		public static void Bind(this Text text, BindableData<int> bindableData)
		{
			Bind<int>(text, bindableData);
		}
		public static void Unbind(this Text text, BindableData<int> bindableData)
		{
			Unbind<int>(text, bindableData);
		}
		#endregion

		#region long
		public static void Bind(this Text text, BindableData<long> bindableData)
		{
			Bind<long>(text, bindableData);
		}
		public static void Unbind(this Text text, BindableData<long> bindableData)
		{
			Unbind<long>(text, bindableData);
		}
		#endregion

		#region float
		public static void Bind(this Text text, BindableData<float> bindableData)
		{
			Bind<float>(text, bindableData);
		}
		public static void Unbind(this Text text, BindableData<float> bindableData)
		{
			Unbind<float>(text, bindableData);
		}
		#endregion

		#region double
		public static void Bind(this Text text, BindableData<double> bindableData)
		{
			Bind<double>(text, bindableData);
		}
		public static void Unbind(this Text text, BindableData<double> bindableData)
		{
			Unbind<double>(text, bindableData);
		}
		#endregion

		public static void Bind<T>(this Text text, BindableData<T> bindableData)
		{
			Action<T> listener = (T newValue) => { text.text = newValue.ToString(); };
			UguiBindManager.Instance.Bind(text, bindableData, listener);
			bindableData.Bind(listener);
		}

		public static void Unbind<T>(this Text text, BindableData<T> bindableData)
		{
			Action<T> listener = UguiBindManager.Instance.Unbind(text, bindableData) as Action<T>;
			bindableData.Unbind(listener);
		}
	}
}
