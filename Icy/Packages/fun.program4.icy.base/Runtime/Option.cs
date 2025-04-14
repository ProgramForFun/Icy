using System;

namespace Icy.Base
{
	/// <summary>
	/// 提供两个回调选项的工具类，泛型版本
	/// </summary>
	public class Option<T>
	{
		/// <summary>
		/// Option的描述，可能为null
		/// </summary>
		public string Description { get; private set; }
		private Action<T> _YesCallback;
		private Action<T> _NoCallback;

		public Option(Action<T> yesCallback, Action<T> noCallback, string desc = null)
		{
			_YesCallback = yesCallback;
			_NoCallback = noCallback;
			Description = desc;
		}

		public void Yes(T arg = default)
		{
			_YesCallback(arg);
		}

		public void No(T arg = default)
		{
			_NoCallback(arg);
		}
	}

	/// <summary>
	/// 提供两个回调选项的工具类，非泛型版本
	/// </summary>
	public class Option
	{
		/// <summary>
		/// Option的描述，可能为null
		/// </summary>
		public string Description { get; private set; }
		private Action _YesCallback;
		private Action _NoCallback;

		public Option(Action yesCallback, Action noCallback, string desc = null)
		{
			_YesCallback = yesCallback;
			_NoCallback = noCallback;
			Description = desc;
		}

		public void Yes()
		{
			_YesCallback();
		}

		public void No()
		{
			_NoCallback();
		}
	}
}
