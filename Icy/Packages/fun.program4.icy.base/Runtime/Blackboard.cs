using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 用于存储一些短生命周期的易变数据
	/// </summary>
	public class Blackboard
	{
		protected Dictionary<string, int> _Int;
		protected Dictionary<string, float> _Float;
		protected Dictionary<string, string> _String;
		protected Dictionary<string, object> _Object;

		public void WriteInt(string key, int value)
		{
			TryToLazyAllocate<int>();
			_Int[key] = value;
		}

		public int ReadInt(string key)
		{
			return _Int[key];
		}

		public void WriteFloat(string key, float value)
		{
			TryToLazyAllocate<float>();
			_Float[key] = value;
		}

		public float ReadFloat(string key)
		{
			return _Float[key];
		}

		public void WriteString(string key, string value)
		{
			TryToLazyAllocate<string>();
			_String[key] = value;
		}

		public string ReadString(string key)
		{
			return _String[key];
		}

		public void WriteObject(string key, object value)
		{
			TryToLazyAllocate<object>();
			_Object[key] = value;
		}

		public object ReadObject(string key)
		{
			return _Object[key];
		}

		private void TryToLazyAllocate<T>()
		{
			if (typeof(T) == typeof(int))
			{
				if (_Int == null)
					_Int = new Dictionary<string, int>();
			}
			else if (typeof(T) == typeof(float))
			{
				if (_Float == null)
					_Float = new Dictionary<string, float>();
			}
			else if (typeof(T) == typeof(string))
			{
				if (_String == null)
					_String = new Dictionary<string, string>();
			}
			else if (typeof(object) == typeof(object))
			{
				if (_Object == null)
					_Object = new Dictionary<string, object>();
			}
		}
	}
}
