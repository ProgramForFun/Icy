using System;

namespace Icy.Base
{
	/// <summary>
	/// 所有事件参数都要实现这个接口
	/// </summary>
	public interface IEventParam
	{
		/// <summary>
		/// 重置各个字段
		/// </summary>
		void Reset();
	}

	//以下是默认提供的事件参数
	public class EventParam_Int : IEventParam
	{
		public int Value;

		public void Reset()
		{
			Value = 0;
		}
	}

	public class EventParam_Long : IEventParam
	{
		public long Value;

		public void Reset()
		{
			Value = 0L;
		}
	}

	public class EventParam_Float : IEventParam
	{
		public float Value;

		public void Reset()
		{
			Value = 0.0f;
		}
	}

	public class EventParam_Double : IEventParam
	{
		public double Value;

		public void Reset()
		{
			Value = 0.0;
		}
	}

	public class EventParam_Bool : IEventParam
	{
		public bool Value;

		public void Reset()
		{
			Value = false;
		}
	}

	public class EventParam_String : IEventParam
	{
		public string Value;

		public void Reset()
		{
			Value = null;
		}
	}

	public class EventParam_Type : IEventParam
	{
		public Type Value;

		public void Reset()
		{
			Value = null;
		}
	}

	public class EventParam<T> : IEventParam
	{
		public T Value;

		public void Reset()
		{
			Value = default;
		}
	}
}
