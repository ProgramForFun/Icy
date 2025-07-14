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


using Cysharp.Text;
using System;

namespace Icy.Base
{
	/// <summary>
	/// 泛型RingBuffer，支持满了自动扩容
	/// </summary>
	public sealed class RingBuffer<T>
	{
		/// <summary>
		/// 当前Buffer里元素的数量
		/// </summary>
		public int Count { get; private set; }
		/// <summary>
		/// Buffer的总容量
		/// </summary>
		public int Capacity => _RingArray.Length;
		/// <summary>
		/// Buffer数量满了（即Count >= Capacity）时自动扩容
		/// </summary>
		public bool AutoExpansion { get; set; }

		/// <summary>
		/// 内部数组
		/// </summary>
		private T[] _RingArray;
		/// <summary>
		/// 第一个元素所在的下标
		/// </summary>
		private int _HeadIdx;
		/// <summary>
		/// 最后一个元素下标 + 1，可以理解为C++的end迭代器/哨兵
		/// </summary>
		private int _TailIdx;


		public RingBuffer(int capacity, bool autoExpansion = true)
		{
			_RingArray = new T[capacity];
			Init(autoExpansion);
		}

		public RingBuffer(T[] buffer, bool autoExpansion = true)
		{
			_RingArray = buffer;
			Init(autoExpansion);
		}

		private void Init(bool autoExpansion)
		{
			AutoExpansion = autoExpansion;

			Count = 0;
			_HeadIdx = 0;
			_TailIdx = 0;
		}

		/// <summary>
		/// 放入一个元素
		/// </summary>
		public void Put(T value)
		{
			Count++;

			if (Count >= Capacity)
			{
				if (AutoExpansion)
				{
					//扩容x2
					T[] newArray = new T[Capacity * 2];
					int newArrayIdx = 0;
					for (int i = _HeadIdx; i < _TailIdx; i++)
					{
						newArray[newArrayIdx] = _RingArray[i];
						newArrayIdx++;
					}
					_RingArray = newArray;
				}
				else
					throw new Exception($"RingBuffer overflow, T = {typeof(T).Name}");
			}

			_RingArray[_TailIdx] = value;
			if (_TailIdx + 1 < _RingArray.Length)
				_TailIdx++;
			else
				_TailIdx = 0;
		}

		/// <summary>
		/// 把数组里的一段放入RingBuffer
		/// </summary>
		public void Put(T[] src, int startIdx, int length)
		{
			for (int i = startIdx; i < length; ++i)
				Put(src[i]);
		}

		/// <summary>
		/// 从RingBuffer的 前面 获取并移除一个元素
		/// </summary>
		public T Get()
		{
			T rtn = _RingArray[_HeadIdx];
			_RingArray[_HeadIdx] = default;
			if (_HeadIdx + 1 < _RingArray.Length)
				_HeadIdx++;
			else
				_HeadIdx = 0;

			Count--;
			return rtn;
		}

		/// <summary>
		/// 从RingBuffer的 前面 开始，获取length个元素，填入dst数组startIdx开始的位置
		/// </summary>
		public void Get(T[] dst, int startIdx, int length)
		{
			for (int i = startIdx; i < length; i++)
				dst[i] = Get();
		}

		public override string ToString()
		{
			Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder();
			for (int i = 0; i < _RingArray.Length; ++i)
			{
				if (i == _HeadIdx)
					stringBuilder.Append("<");

				stringBuilder.Append(_RingArray[i].ToString());

				if (i + 1 == _TailIdx)
					stringBuilder.Append(">");

				if (i < _RingArray.Length - 1)
					stringBuilder.Append(", ");
			}
			return stringBuilder.ToString();
		}
	}
}
