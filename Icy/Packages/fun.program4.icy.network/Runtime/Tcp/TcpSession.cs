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


using System.Net.Sockets;
using System;
using Icy.Base;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace Icy.Network
{
	/// <summary>
	/// 一个简单的TCP通信类，基于System.Net.Sockets.TcpClient封装；
	/// 处理了粘包，基于包头的一个int大小的消息长度实现
	/// </summary>
	public class TcpSession : NetworkSessionBase
	{
		protected TcpClient _TcpClient;
		protected NetworkStream _Stream;

		/// <summary>
		/// 当前正在接收的消息的长度，包括长度本身的4字节
		/// </summary>
		protected int _CurMsgLen = 0;
		/// <summary>
		/// 当前正在接收的消息（包括消息长度），在buffer中的起始index
		/// </summary>
		protected int _CurMsgStartIdx = 0;
		/// <summary>
		/// 当前Buffer中剩余多少数据
		/// </summary>
		protected int _BufferRemain = 0;
		/// <summary>
		/// 消息长度本身，的长度
		/// </summary>
		protected const int MSG_LENGTH_SIZE = sizeof(int);


		public TcpSession(string host, int port, int bufferSize) : base(host, port, bufferSize)
		{

		}

		/// <summary>
		/// 和服务器建立连接
		/// </summary>
		public override async Task Connect(byte[] _ = null)
		{
			if (IsConnected)
			{
				Log.LogError("Duplicate Connect", nameof(TcpSession));
				return;
			}

			try
			{
				_TcpClient = new TcpClient();
				await _TcpClient.ConnectAsync(Host, Port);
				_TcpClient.NoDelay = true;  //关闭Nagle算法
				_Stream = _TcpClient.GetStream();
				IsConnected = true;
				OnConnected?.Invoke();
				Log.LogInfo("Connected", nameof(TcpSession));
			}
			catch (Exception ex)
			{
				Log.LogError($"Connect exception : {ex}", nameof(TcpSession));
				OnError?.Invoke(NetworkError.ConnectFailed, ex);
			}

			while (IsConnected)
			{
				int receivedSize = 0;
				try
				{
					Memory<byte> memBytes = new Memory<byte>(_ReceiveBuffer, _BufferRemain, _BufferSize - _BufferRemain);
					receivedSize = await _Stream.ReadAsync(memBytes);
					HandleReceived(_ReceiveBuffer, -1, receivedSize);
				}
				catch (Exception ex)
				{
					Log.LogError($"Receive failed, {ex}", nameof(TcpSession));
					OnError?.Invoke(NetworkError.ReceiveFailed, ex);
					await Disconnect();
				}
			}
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		public override void Send(byte[] msg, int startIdx, int length)
		{
			if (!IsConnected)
			{
				Exception e = new Exception($"Call {nameof(Send)} when Tcp is disconnected");
				Log.LogError(e.ToString(), nameof(TcpSession));
				OnError?.Invoke(NetworkError.SendWhenDisconnected, e);
				return;
			}

			try
			{
				//消息结构前4个byte为本数据包的长度，以解决粘包问题
				//  消息长度 消息本体
				// |- - - -| - - -...
				//     4      n

				//1、长度 = 消息ID + 消息本体的长度
				int len = length + MSG_LENGTH_SIZE;
				BitConverter.TryWriteBytes(_SendBuffer, len);
				//2、消息本体
				Buffer.BlockCopy(msg, 0, _SendBuffer, MSG_LENGTH_SIZE, length);
				_Stream.WriteAsync(_SendBuffer, 0, len);
			}
			catch (Exception e)
			{
				Log.LogError($"Send failed, {e}", nameof(TcpSession));
				OnError?.Invoke(NetworkError.SendFailed, e);
			}
		}

		/// <summary>
		/// 断开连接
		/// </summary>
		public override async Task Disconnect(byte[] _ = null)
		{
			if (!IsConnected)
			{
				Log.LogError("Disconnect when disconnected", nameof(TcpSession));
				return;
			}
			IsConnected = false;
			_TcpClient.Close();
			_TcpClient = null;
			OnDisconnected?.Invoke();
			Log.LogInfo("Disconnect", nameof(TcpSession));
			await UniTask.CompletedTask;
		}

		/// <summary>
		/// 处理TCP接收到的消息
		/// </summary>
		protected override void HandleReceived(byte[] buffer, int _, int receivedSize)
		{
			_BufferRemain += receivedSize;
			while (true)
			{
				if (_CurMsgLen == 0)
				{
					//处理消息长度都没能完整发过来的情况
					if (_BufferRemain < MSG_LENGTH_SIZE)
						break;
					//获取消息长度
					else
						_CurMsgLen = BitConverter.ToInt32(buffer, _CurMsgStartIdx);
				}

				//当前buffer里的数据，足够一条完整消息，正常处理
				if (_BufferRemain >= _CurMsgLen)
				{
					OnReceive?.Invoke(buffer, _CurMsgStartIdx + MSG_LENGTH_SIZE, _CurMsgLen - MSG_LENGTH_SIZE);
					_BufferRemain -= _CurMsgLen;
					if (_BufferRemain > 0)
						_CurMsgStartIdx += _CurMsgLen;
					_CurMsgLen = 0;

					//如果当前buffer有剩余 且 不是从下标0开始的，挪到下标0开始，方便逻辑处理
					//TODO：其实应该搞一个环形buffer，避免Copy，更高效
					if (_BufferRemain > 0 && _CurMsgStartIdx > 0)
					{
						//Log.LogInfo("Move " + _BufferRemain);
						Buffer.BlockCopy(_ReceiveBuffer, _CurMsgStartIdx, _ReceiveBuffer, 0, _BufferRemain);
						_CurMsgStartIdx = 0;
					}
				}
				//等待下一轮数据到来，凑成完整消息再处理
				else
					break;
			}
		}

		public override void Dispose()
		{
			if (IsConnected)
				Disconnect().AsUniTask().Forget();
			Log.LogInfo("Dispose", nameof(TcpSession));
		}
	}
}
