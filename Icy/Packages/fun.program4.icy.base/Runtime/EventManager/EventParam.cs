

using System;

namespace Icy.Base
{

	public interface IEventParam
	{

	}

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
}
