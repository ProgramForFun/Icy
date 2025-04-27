using Icy.Base;
using System;
using TMPro;

namespace Icy.UI
{
	/// <summary>
	/// TMP的Bind扩展
	/// </summary>
	public static class UguiTmpTextBinder
	{
		#region string
		public static void Bind(this TextMeshProUGUI tmp, BindableData<string> bindableData)
		{
			Bind<string>(tmp, bindableData);
		}
		public static void Unbind(this TextMeshProUGUI tmp, BindableData<string> bindableData)
		{
			Unbind<string>(tmp, bindableData);
		}
		#endregion

		#region int
		public static void Bind(this TextMeshProUGUI tmp, BindableData<int> bindableData)
		{
			Bind<int>(tmp, bindableData);
		}
		public static void Unbind(this TextMeshProUGUI tmp, BindableData<int> bindableData)
		{
			Unbind<int>(tmp, bindableData);
		}
		#endregion

		#region long
		public static void Bind(this TextMeshProUGUI tmp, BindableData<long> bindableData)
		{
			Bind<long>(tmp, bindableData);
		}
		public static void Unbind(this TextMeshProUGUI tmp, BindableData<long> bindableData)
		{
			Unbind<long>(tmp, bindableData);
		}
		#endregion

		#region float
		public static void Bind(this TextMeshProUGUI tmp, BindableData<float> bindableData)
		{
			Bind<float>(tmp, bindableData);
		}
		public static void Unbind(this TextMeshProUGUI tmp, BindableData<float> bindableData)
		{
			Unbind<float>(tmp, bindableData);
		}
		#endregion

		#region double
		public static void Bind(this TextMeshProUGUI tmp, BindableData<double> bindableData)
		{
			Bind<double>(tmp, bindableData);
		}
		public static void Unbind(this TextMeshProUGUI tmp, BindableData<double> bindableData)
		{
			Unbind<double>(tmp, bindableData);
		}
		#endregion

		public static void Bind<T>(this TextMeshProUGUI tmp, BindableData<T> bindableData)
		{
			Action<T> listener = (T newValue) => { tmp.text = newValue.ToString(); };
			UguiBindManager.Instance.Bind(tmp, bindableData, listener);
			bindableData.Bind(listener);
		}

		public static void Unbind<T>(this TextMeshProUGUI tmp, BindableData<T> bindableData)
		{
			Action<T> listener = UguiBindManager.Instance.Unbind(tmp, bindableData) as Action<T>;
			bindableData.Unbind(listener);
		}
	}
}
