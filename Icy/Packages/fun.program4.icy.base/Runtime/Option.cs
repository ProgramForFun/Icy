/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
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


using System;

namespace Icy.Base
{
	/// <summary>
	/// 提供两个回调选项的工具类，泛型版本
	/// </summary>
	public class Option<T>
	{
		/// <summary>
		/// Option的附带数据，可能为null
		/// </summary>
		public object Payload { get; private set; }
		private Action<T> _YesCallback;
		private Action<T> _NoCallback;

		public Option(Action<T> yesCallback, Action<T> noCallback, object payload = null)
		{
			_YesCallback = yesCallback;
			_NoCallback = noCallback;
			Payload = payload;
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
		/// Option的附带数据，可能为null
		/// </summary>
		public object Payload { get; private set; }
		private Action _YesCallback;
		private Action _NoCallback;

		public Option(Action yesCallback, Action noCallback, object payload = null)
		{
			_YesCallback = yesCallback;
			_NoCallback = noCallback;
			Payload = payload;
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
