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


using System;
using System.Threading.Tasks;

namespace Icy.Network
{
	/// <summary>
	/// 各个网络通信协议的Session基类
	/// </summary>
	public abstract class NetworkSessionBase : IDisposable
	{
		/// <summary>
		/// 服务器域名或IP地址
		/// </summary>
		public string Host { get; protected set; }
		/// <summary>
		/// 服务器端口
		/// </summary>
		public int Port { get; protected set; }
		/// <summary>
		/// 是否已和服务器建立连接
		/// </summary>
		public bool IsConnected { get; protected set; }

		/// <summary>
		/// 和服务器建立连接、可以通信了的事件
		/// </summary>
		public Action OnConnected;
		/// <summary>
		/// 和服务器断开连接的事件
		/// </summary>
		public Action OnDisconnected;
		/// <summary>
		/// 接收到服务器消息的事件
		/// </summary>
		public Action<byte[], int, int> OnReceive;
		/// <summary>
		/// 各种错误的事件
		/// </summary>
		public Action<NetworkError, Exception> OnError;

		/// <summary>
		/// Buffer大小
		/// </summary>
		protected int _BufferSize;
		/// <summary>
		/// 接收Buffer
		/// </summary>
		protected byte[] _ReceiveBuffer;
		/// <summary>
		/// 发送Buffer
		/// </summary>
		protected byte[] _SendBuffer;


		public NetworkSessionBase(string host, int port, int bufferSize)
		{
			Host = host;
			Port = port;
			_BufferSize = bufferSize;
			_ReceiveBuffer = new byte[bufferSize];
			_SendBuffer = new byte[bufferSize];
			IsConnected = false;
		}

		/// <summary>
		/// 连接服务器
		/// </summary>
		/// <param name="syn">Kcp必须传，其他协议不需要</param>
		public abstract Task Connect(byte[] syn = null);
		/// <summary>
		/// 发送消息
		/// </summary>
		public abstract void Send(byte[] msg, int startIdx, int length);
		/// <summary>
		/// 和服务器断开连接
		/// </summary>
		/// <param name="fin">Kcp必须传，其他协议不需要</param>
		public abstract Task Disconnect(byte[] fin = null);
		/// <summary>
		/// 处理服务器的消息
		/// </summary>
		protected abstract void HandleReceived(byte[] buffer, int startIdx, int receivedSize);
		/// <summary>
		/// 销毁Session
		/// </summary>
		public abstract void Dispose();
	}
}
