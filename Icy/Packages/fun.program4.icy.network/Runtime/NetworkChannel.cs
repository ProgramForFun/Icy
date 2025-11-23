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
using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Icy.Network
{
	/// <summary>
	/// 封装的网络通信Channel，支持多种协议；
	/// 业务侧通过几个Send方法发送消息，
	/// 接收消息、在NetworkReceiver中解析后，可以通过EventManager发送到主线程的业务侧；
	/// </summary>
	public class NetworkChannel<T>
	{
		/// <summary>
		/// 是否已连接到服务器
		/// </summary>
		public bool IsConnected => Session.IsConnected && _CancellationTokenSource != null && !_CancellationTokenSource.IsCancellationRequested;
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
		protected NetworkSenderBase<T> _Sender;
		/// <summary>
		/// 负责反序列化
		/// </summary>
		protected NetworkReceiverBase _Receiver;

		/// <summary>
		/// Syn参数
		/// </summary>
		protected byte[] _Syn;
		/// <summary>
		/// 发送队列
		/// </summary>
		protected Queue<T> _SendQueue1;
		protected Queue<ValueTuple<int, T>> _SendQueue2;
		protected Queue<ValueTuple<int, int, T>> _SendQueue3;
		protected Queue<ValueTuple<int, int, int, T>> _SendQueue4;
		/// <summary>
		/// 待发送消息数量
		/// </summary>
		protected int _ToSendCount;
		/// <summary>
		/// 发送线程锁
		/// </summary>
		protected object _SendLock;
		/// <summary>
		/// 发送线程取消令牌
		/// </summary>
		protected CancellationTokenSource _CancellationTokenSource;
		/// <summary>
		/// 待发送的消息超过这个数量，会报错预警
		/// </summary>
		protected const int SEND_QUEUE_COUNT_ALERT = 32;


		public NetworkChannel(NetworkChannelArgs<T> args)
		{
			NetworkSessionBase session = null;
			switch (args.SessionType)
			{
				case NetworkSessionType.Tcp:
					session = new TcpSession(args.Host, args.Port, args.BufferSize);
					break;
				case NetworkSessionType.Kcp:
					session = new KcpSession(args.Host, args.Port, args.BufferSize);
					break;
				case NetworkSessionType.WebSocket:
					session = new WebSocketSession(args.Host, args.Port);
					break;
				default:
					Log.Assert(false, $"Invalid NetworkSessionType = {args.SessionType}");
					break;
			}

			if (session != null)
				Init(session, args.Sender, args.Receiver);
		}

		public NetworkChannel(NetworkSessionBase session, NetworkSenderBase<T> sender, NetworkReceiverBase receiver)
		{
			Init(session, sender, receiver);
		}

		protected void Init(NetworkSessionBase session, NetworkSenderBase<T> sender, NetworkReceiverBase receiver)
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
			_Syn = syn;

			Session.OnReceive = _Receiver.Decode;
			//await Session.Connect(syn);

			_SendQueue1 = new Queue<T>();
			_SendQueue2 = new Queue<ValueTuple<int, T>>();
			_SendQueue3 = new Queue<ValueTuple<int, int, T>>();
			_SendQueue4 = new Queue<ValueTuple<int, int, int, T>>();

			_ToSendCount = 0;
			_SendLock = new object();
			_CancellationTokenSource = new CancellationTokenSource();
			UniTask.RunOnThreadPool(ReceiveLoop).Forget(CommonUtility.OnUniTaskForgetException);
			UniTask.RunOnThreadPool(SendLoop).Forget(CommonUtility.OnUniTaskForgetException);

			await UniTask.CompletedTask;
		}

		public virtual void Send(T data)
		{
			if (CheckSendable())
			{
				lock (_SendLock)
				{
					_SendQueue1.Enqueue(data);
					_ToSendCount++;
					MonitorSendQueueBacklog();
				}
			}
		}

		public virtual void Send(int arg1, T data)
		{
			if (CheckSendable())
			{
				lock (_SendLock)
				{
					_SendQueue2.Enqueue(new ValueTuple<int, T>(arg1, data));
					_ToSendCount++;
					MonitorSendQueueBacklog();
				}
			}
		}

		public virtual void Send(int arg1, int arg2, T data)
		{
			if (CheckSendable())
			{
				lock (_SendLock)
				{
					_SendQueue3.Enqueue(new ValueTuple<int, int, T>(arg1, arg2, data));
					_ToSendCount++;
					MonitorSendQueueBacklog();
				}
			}
		}

		public virtual void Send(int arg1, int arg2, int arg3, T data)
		{
			if (CheckSendable())
			{
				lock (_SendLock)
				{
					_SendQueue4.Enqueue(new ValueTuple<int, int, int, T>(arg1, arg2, arg3, data));
					_ToSendCount++;
					MonitorSendQueueBacklog();
				}
			}
		}

		internal virtual async UniTask Send(byte[] encodedData, int startIdx, int length)
		{
			await Session.Send(encodedData, startIdx, length);
		}

		/// <summary>
		/// 发送循环
		/// </summary>
		protected async UniTaskVoid SendLoop()
		{
			while (!IsConnected && !_CancellationTokenSource.IsCancellationRequested)
				await Task.Delay(16).ConfigureAwait(false);	//帧率60的每帧时间，这个等待总体不会太长

			while (!_CancellationTokenSource.IsCancellationRequested)
			{
				//TODO：想办法避免Task.Delay的GC Alloc
				while (_ToSendCount == 0 && !_CancellationTokenSource.IsCancellationRequested)
					await Task.Delay(16).ConfigureAwait(false);

				if (_SendQueue1.Count > 0)
				{
					T toSend;
					lock (_SendLock)
					{
						toSend = _SendQueue1.Dequeue();
						_ToSendCount--;
					}
					await _Sender.Encode(toSend);
					continue;
				}

				if (_SendQueue2.Count > 0)
				{
					ValueTuple<int, T> toSend;
					lock (_SendLock)
					{
						toSend = _SendQueue2.Dequeue();
						_ToSendCount--;
					}
					await _Sender.Encode(toSend.Item1, toSend.Item2);
					continue;
				}

				if (_SendQueue3.Count > 0)
				{
					ValueTuple<int, int, T> toSend;
					lock (_SendLock)
					{
						toSend = _SendQueue3.Dequeue();
						_ToSendCount--;
					}
					await _Sender.Encode(toSend.Item1, toSend.Item2, toSend.Item3);
					_ToSendCount--;
					continue;
				}

				if (_SendQueue4.Count > 0)
				{
					ValueTuple<int, int, int, T> toSend;
					lock (_SendLock)
					{
						toSend = _SendQueue4.Dequeue();
						_ToSendCount--;
					}
					await _Sender.Encode(toSend.Item1, toSend.Item2, toSend.Item3, toSend.Item4);
					continue;
				}
			}
		}

		/// <summary>
		/// 接收循环
		/// </summary>
		protected async UniTaskVoid ReceiveLoop()
		{
			await Session.Connect(_Syn);

			if (!_CancellationTokenSource.IsCancellationRequested)
				await Session.Listen();
		}

		protected bool CheckSendable()
		{
			if (!IsConnected || _CancellationTokenSource == null || _CancellationTokenSource.IsCancellationRequested)
			{
				Exception e = new Exception($"Call {nameof(Send)} when {nameof(NetworkChannel<T>)} is disconnected");
				Log.Error(e.ToString(), nameof(NetworkChannel<T>));
				OnError?.Invoke(NetworkError.SendWhenDisconnected, e);
				return false;
			}

			return true;
		}

		/// <summary>
		/// 监视发送队列是否有积压
		/// </summary>
		protected bool MonitorSendQueueBacklog()
		{
			int totalCount = _SendQueue1.Count + _SendQueue2.Count + _SendQueue3.Count + _SendQueue4.Count;
			if (totalCount >= SEND_QUEUE_COUNT_ALERT)
			{
				string content = ZString.Format("To send msg is backlogging, {0} - {1} - {2} - {3}", _SendQueue1.Count, _SendQueue2.Count, _SendQueue3.Count, _SendQueue4.Count);
				Exception e = new Exception(content);
				Log.Error(content, nameof(NetworkChannel<T>));
				OnError?.Invoke(NetworkError.SendQueueBacklog, e);
				return false;
			}
			return true;
		}

		/// <summary>
		/// 断开Channel，释放资源
		/// </summary>
		/// <param name="fin">Kcp必须传，其他协议不需要</param>
		public virtual async UniTask Dispose(byte[] fin = null)
		{
			_CancellationTokenSource.Cancel();
			await UniTask.Delay(1000);
			if (Session.IsConnected)
				await Session.Disconnect(fin);
			Session.OnReceive = null;
			Session.Dispose();
		}
	}
}
