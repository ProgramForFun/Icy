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
using Cysharp.Threading.Tasks;
using Icy.Base;
using NativeWebSocket;

namespace Icy.Network
{
	/// <summary>
	/// 基于NativeWebSocket的WebSocket通信类
	/// </summary>
	public class WebSocketSession : NetworkSessionBase, IUpdateable
	{
		protected WebSocket _WebSocket;

		/// <summary>
		/// 构造WebSocketSession
		/// </summary>
		/// <param name="host">ws://或wss://开头的地址</param>
		/// <param name="port">端口，如果不需指定填0</param>
		public WebSocketSession(string host, int port = 0) : base(host, port, 0) //0：WebSocket高度封装过了，不需要再提供Buffer
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			IcyFrame.Instance.AddUpdate(this);
#endif
		}

		public override async UniTask Connect(byte[] syn = null)
		{
			try
			{
				string url = Host;
				if (Port > 0)
					url = $"{Host}:{Port}";

				_WebSocket = new WebSocket(url);
				_WebSocket.OnOpen += OnWebSocketConnected;
				_WebSocket.OnMessage += OnWebSocketMessage;
				_WebSocket.OnError += OnWebSocketError;
				_WebSocket.OnClose += OnWebSocketClose;
				await _WebSocket.Connect();
			}
			catch (Exception ex)
			{
				Log.LogError($"Connect exception : {ex}", nameof(WebSocketSession));
				OnError?.Invoke(NetworkError.ConnectFailed, ex);
			}
		}

		public override async UniTask Listen()
		{
			await UniTask.CompletedTask;
		}

		private void OnWebSocketMessage(byte[] data)
		{
			HandleReceived(data, 0, data.Length);
		}

		private void OnWebSocketClose(WebSocketCloseCode closeCode)
		{
			switch (closeCode)
			{
				case WebSocketCloseCode.Normal:				//WebSocketCloseStatus.NormalClosure
					break;
				case WebSocketCloseCode.NotSet:
				case WebSocketCloseCode.Away:				//WebSocketCloseStatus.EndpointUnavailable
				case WebSocketCloseCode.ProtocolError:		//WebSocketCloseStatus.ProtocolError
				case WebSocketCloseCode.UnsupportedData:	//WebSocketCloseStatus.InvalidMessageType
				case WebSocketCloseCode.Undefined:
				case WebSocketCloseCode.NoStatus:			//WebSocketCloseStatus.Empty
				case WebSocketCloseCode.Abnormal:
				case WebSocketCloseCode.InvalidData:		//WebSocketCloseStatus.InvalidPayloadData
				case WebSocketCloseCode.PolicyViolation:	//WebSocketCloseStatus.PolicyViolation
				case WebSocketCloseCode.TooBig:				//WebSocketCloseStatus.MessageTooBig
				case WebSocketCloseCode.MandatoryExtension:	//WebSocketCloseStatus.MandatoryExtension
				case WebSocketCloseCode.ServerError:		//WebSocketCloseStatus.InternalServerError
					{
						Exception e = new Exception(closeCode.ToString());
						OnError?.Invoke(NetworkError.WebSocketError, e);
						break;
					}
				case WebSocketCloseCode.TlsHandshakeFailure:
					{
						Exception e = new Exception(WebSocketCloseCode.TlsHandshakeFailure.ToString());
						OnError?.Invoke(NetworkError.ConnectFailed, e);
						break;
					}
			}

			IsConnected = false;
			OnDisconnected?.Invoke();
		}

		private void OnWebSocketError(string errorMsg)
		{
			Exception ex = new Exception(errorMsg);
			OnError?.Invoke(NetworkError.WebSocketError, ex);
		}

		private void OnWebSocketConnected()
		{
			IsConnected = true;
			OnConnected?.Invoke();
		}

		public override async void Send(byte[] msg, int startIdx, int length)
		{
			if (!IsConnected)
			{
				Exception e = new Exception($"Call {nameof(Send)} when WebSocket is disconnected");
				Log.LogError(e.ToString(), nameof(WebSocketSession));
				OnError?.Invoke(NetworkError.SendWhenDisconnected, e);
				return;
			}

#if UNITY_WEBGL && !UNITY_EDITOR
			//TODO：GC优化
			byte[] data2Send = new byte[length];
			Buffer.BlockCopy(msg, startIdx, data2Send, 0, length);
			await _WebSocket.Send(data2Send);
#else
			ArraySegment<byte> data2Send = new ArraySegment<byte>(msg, startIdx, length);
			await _WebSocket.Send(data2Send);
#endif
		}

		protected override void HandleReceived(byte[] buffer, int startIdx, int receivedSize)
		{
			OnReceive?.Invoke(buffer, startIdx, receivedSize);
		}

		public override async UniTask Disconnect(byte[] fin = null)
		{
			if (!IsConnected)
			{
				Log.LogError("Disconnect when disconnected", nameof(WebSocketSession));
				return;
			}

			await _WebSocket.Close();
			_WebSocket = null;
		}

		public override void Dispose()
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			IcyFrame.Instance.RemoveUpdate(this);
#endif
			if (IsConnected)
				Disconnect().Forget();
			Log.LogInfo("Dispose", nameof(WebSocketSession));
		}

		public void Update(float delta)
		{
#if !UNITY_WEBGL || UNITY_EDITOR
			_WebSocket?.DispatchMessageQueue();
#endif
		}
	}
}
