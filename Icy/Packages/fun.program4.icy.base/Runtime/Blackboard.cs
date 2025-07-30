/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


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

		public bool HasInt(string key)
		{
			return _Int != null && _Int.ContainsKey(key);
		}

		public int ReadInt(string key, bool keep = false)
		{
			int rtn = _Int[key];
			if (!keep)
				_Int.Remove(key);
			return rtn;
		}

		public void WriteFloat(string key, float value)
		{
			TryToLazyAllocate<float>();
			_Float[key] = value;
		}

		public bool HasFloat(string key)
		{
			return _Float != null && _Float.ContainsKey(key);
		}

		public float ReadFloat(string key, bool keep = false)
		{
			float rtn = _Float[key];
			if (!keep)
				_Float.Remove(key);
			return rtn;
		}

		public void WriteString(string key, string value)
		{
			TryToLazyAllocate<string>();
			_String[key] = value;
		}

		public bool HasString(string key)
		{
			return _String != null && _String.ContainsKey(key);
		}

		public string ReadString(string key, bool keep = false)
		{
			string rtn = _String[key];
			if (!keep)
				_String.Remove(key);
			return rtn;
		}

		public void WriteObject(string key, object value)
		{
			TryToLazyAllocate<object>();
			_Object[key] = value;
		}

		public bool HasObject(string key)
		{
			return _Object != null && _Object.ContainsKey(key);
		}

		public object ReadObject(string key, bool keep = false)
		{
			object rtn = _Object[key];
			if (!keep)
				_Object.Remove(key);
			return rtn;
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
