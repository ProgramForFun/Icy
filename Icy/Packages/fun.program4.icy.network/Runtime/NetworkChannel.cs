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


using Cysharp.Threading.Tasks;
using Icy.Base;
using System;

namespace Icy.Network
{
	/// <summary>
	/// 封装起来的网络通信Channel，支持多种协议
	/// </summary>
	public class NetworkChannel
	{
		/// <summary>
		/// 是否已连接到服务器
		/// </summary>
		public bool IsConnected => Session.IsConnected;
		/// <summary>
		/// 和服务器建立连接、可以通信了的事件
		/// </summary>
		public Action OnConnected
		{
			get => Session.OnConnected;
			set => Session.OnConnected = value;
		}
		/// <summary>
		/// 和服务器断开连接的事件
		/// </summary>
		public Action OnDisconnected
		{
			get => Session.OnDisconnected;
			set => Session.OnDisconnected = value;
		}
		/// <summary>
		/// 各种错误的事件
		/// </summary>
		public Action<NetworkError, Exception> OnError
		{
			get => Session.OnError;
			set => Session.OnError = value;
		}

		/// <summary>
		/// 内部的网络Session
		/// </summary>
		public NetworkSessionBase Session { get; protected set; }
		/// <summary>
		/// 负责序列化
		/// </summary>
		protected NetworkSenderBase _Sender;
		/// <summary>
		/// 负责反序列化
		/// </summary>
		protected NetworkReceiverBase _Receiver;


		public NetworkChannel(NetworkChannelArgs args)
		{
			switch (args.SessionType)
			{
				case NetworkSessionType.Tcp:
					Session = new TcpSession(args.Host, args.Port, args.BufferSize);
					break;
				case NetworkSessionType.Kcp:
					Session = new KcpSession(args.Host, args.Port, args.BufferSize);
					break;
				default:
					Log.Assert(false, $"Invalid NetworkSessionType = {args.SessionType}");
					break;
			}

			_Sender = args.Sender;
			_Sender.SetChannel(this);
			_Receiver = args.Receiver;
		}

		public NetworkChannel(NetworkSessionBase session, NetworkSenderBase sender, NetworkReceiverBase receiver)
		{
			Session = session;
			_Sender = sender;
			sender.SetChannel(this);
			_Receiver = receiver;
		}

		/// <summary>
		/// 开始通信
		/// </summary>
		/// <param name="syn">Kcp必须传，具体见KcpSession；其他协议不需要</param>
		public virtual async UniTask Start(byte[] syn = null)
		{
			Session.OnReceive = _Receiver.Decode;
			await Session.Connect(syn);
		}

		public virtual void Send<T>(T data)
		{
			_Sender.Encode(data);
		}

		public virtual void Send<T>(int arg1, T data)
		{
			_Sender.Encode(arg1, data);
		}

		public virtual void Send<T>(int arg1, int arg2, T data)
		{
			_Sender.Encode(arg1, arg2, data);
		}

		public virtual void Send<T>(int arg1, int arg2, int arg3, T data)
		{
			_Sender.Encode(arg1, arg2, arg3, data);
		}

		internal virtual void Send(byte[] encodedData, int startIdx, int length)
		{
			Session.Send(encodedData, startIdx, length);
		}

		/// <summary>
		/// 断开Channel，释放资源
		/// </summary>
		/// <param name="fin">Kcp必须传，其他协议不需要</param>
		public virtual async UniTask Dispose(byte[] fin = null)
		{
			await Session.Disconnect(fin);
			Session.OnReceive = null;
			Session.Dispose();
		}
	}
}
