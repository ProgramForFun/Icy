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
		public bool IsConnected => _Session.IsConnected;
		/// <summary>
		/// 和服务器建立连接、可以通信了的事件
		/// </summary>
		public Action OnConnected
		{
			get => _Session.OnConnected;
			set => _Session.OnConnected = value;
		}
		/// <summary>
		/// 和服务器断开连接的事件
		/// </summary>
		public Action OnDisconnected
		{
			get => _Session.OnDisconnected;
			set => _Session.OnDisconnected = value;
		}
		/// <summary>
		/// 各种错误的事件
		/// </summary>
		public Action<NetworkError, Exception> OnError
		{
			get => _Session.OnError;
			set => _Session.OnError = value;
		}

		/// <summary>
		/// 内部的网络Session
		/// </summary>
		protected NetworkSessionBase _Session;
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
					_Session = new TcpSession(args.Host, args.Port, args.BufferSize);
					break;
				case NetworkSessionType.Kcp:
					_Session = new KcpSession(args.Host, args.Port, args.BufferSize);
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
			_Session = session;
			_Sender = sender;
			sender.SetChannel(this);
			_Receiver = receiver;
		}

		/// <summary>
		/// 开始通信
		/// </summary>
		/// <param name="syn">Kcp必须传，其他协议不需要</param>
		public virtual async UniTask Start(byte[] syn = null)
		{
			_Session.OnReceive = _Receiver.Decode;
			await _Session.Connect(syn);
			await _Session.Listen();
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
			_Session.Send(encodedData, startIdx, length);
		}

		/// <summary>
		/// 断开Channel，释放资源
		/// </summary>
		/// <param name="fin">Kcp必须传，其他协议不需要</param>
		public virtual async UniTask Dispose(byte[] fin = null)
		{
			await _Session.Disconnect(fin);
			_Session.OnReceive = null;
			_Session.Dispose();
		}
	}
}
