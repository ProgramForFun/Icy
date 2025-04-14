using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 用于存储一些短生命周期的易变数据
	/// </summary>
	public class Blackboard
	{
		protected Dictionary<string, float> _FloatBlackboard = new Dictionary<string, float>();
		protected Dictionary<string, string> _StringBlackboard = new Dictionary<string, string>();
		protected Dictionary<string, object> _ObjectBlackboard = new Dictionary<string, object>();

		public void WriteFloat(string key, float value)
		{
			_FloatBlackboard[key] = value;
		}

		public float ReadFloat(string key)
		{
			return _FloatBlackboard[key];
		}

		public void WriteString(string key, string value)
		{
			_StringBlackboard[key] = value;
		}

		public string ReadString(string key)
		{
			return _StringBlackboard[key];
		}

		public void WriteObject(string key, object value)
		{
			_ObjectBlackboard[key] = value;
		}

		public object ReadObject(string key)
		{
			return _ObjectBlackboard[key];
		}
	}
}
