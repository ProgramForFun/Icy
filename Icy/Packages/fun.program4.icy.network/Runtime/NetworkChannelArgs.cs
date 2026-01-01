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


namespace Icy.Network
{
	/// <summary>
	/// 支持的Session类型
	/// </summary>
	public enum NetworkSessionType
	{
		Tcp,
		Kcp,
		WebSocket,
	}

	/// <summary>
	/// 构造NetworkChannel的参数
	/// </summary>
	public class NetworkChannelArgs<T>
	{
		/// <summary>
		/// Session类型
		/// </summary>
		public NetworkSessionType SessionType;
		/// <summary>
		/// 服务器地址
		/// </summary>
		public string Host;
		/// <summary>
		/// 服务器端口
		/// </summary>
		public int Port;
		/// <summary>
		/// Buffer大小
		/// </summary>
		public int BufferSize;
		/// <summary>
		/// 负责序列化的Sender
		/// </summary>
		public NetworkSenderBase<T> Sender;
		/// <summary>
		/// 负责反序列化的Receiver
		/// </summary>
		public NetworkReceiverBase Receiver;
	}
}
