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


namespace Icy.Network
{
	/// <summary>
	/// 和NetworkChannel一起使用的，负责发送前的序列化、转换成byte[]的工作；
	/// 注意，这个类跑在Worker线程里；
	/// </summary>
	public abstract class NetworkSenderBase<T>
	{
		protected NetworkChannel<T> _Channel;
		protected byte[] _Buffer;

		public NetworkSenderBase(int bufferSize = 4096)
		{
			_Buffer = new byte[bufferSize];
		}

		/// <summary>
		/// 把NetworkChannel传进来
		/// </summary>
		public void SetChannel(NetworkChannel<T> channel)
		{
			_Channel = channel;
		}

		public virtual void Encode(T data) { }
		public virtual void Encode(int arg1, T data) { }
		public virtual void Encode(int arg1, int arg2, T data) { }
		public virtual void Encode(int arg1, int arg2, int arg3, T data) { }

		/// <summary>
		/// 转换成byte[]后，调用这个方法
		/// </summary>
		protected void Send(int startIdx, int length)
		{
			_Channel.Send(_Buffer, startIdx, length);
		}
	}
}
