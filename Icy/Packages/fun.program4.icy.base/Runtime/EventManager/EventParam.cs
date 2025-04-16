namespace Icy.Base
{
	/// <summary>
	/// 所有事件参数都要实现这个接口
	/// </summary>
	public interface IEventParam
	{

	}

	//以下是默认提供的事件参数
	public struct EventParam_Int : IEventParam
	{
		public int Value;
	}

	public struct EventParam_Long : IEventParam
	{
		public long Value;
	}

	public struct EventParam_Float : IEventParam
	{
		public float Value;
	}

	public struct EventParam_Double : IEventParam
	{
		public double Value;
	}

	public struct EventParam_Bool : IEventParam
	{
		public bool Value;
	}

	public struct EventParam_String : IEventParam
	{
		public string Value;
	}

	public struct EventParam<T> : IEventParam
	{
		public T Value;
	}
}
