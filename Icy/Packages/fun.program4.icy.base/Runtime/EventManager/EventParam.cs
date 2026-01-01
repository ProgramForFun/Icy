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

	public class EventParam_Reuslt : IEventParam
	{
		/// <summary>
		/// 结果是成功还是失败
		/// </summary>
		public bool Succeed;
		/// <summary>
		/// 如果失败了，这里是报错
		/// </summary>
		public string Error;

		public virtual void Reset()
		{
			Succeed = default;
			Error = null;
		}
	}

	public class EventParam_Reuslt<PayloadT> : EventParam_Reuslt
	{
		/// <summary>
		/// 其他附带的数据
		/// </summary>
		public PayloadT Payload;

		public override void Reset()
		{
			base.Reset();
			Payload	= default;
		}
	}
}
