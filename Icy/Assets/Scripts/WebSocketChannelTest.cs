#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Icy.Base;
using System;
using TestMsg;
using UnityEngine;

namespace Icy.Network
{
	public static class WebSocketChannelTest
	{
		static NetworkChannel<IMessage> _WebSocketChannel;
		static TestMessageResult _MessageResult;
		

		public static void Test()
		{
			_MessageResult = new TestMessageResult();

			WebSocketSession session = new WebSocketSession("ws://localhost", 12888);
			_WebSocketChannel = new NetworkChannel<IMessage>(session, new ProtoSender(), new ProtoReceiver());
			_WebSocketChannel.OnConnected = OnConnect;
			_WebSocketChannel.OnDisconnected += OnDisconnect;
			_WebSocketChannel.OnError += OnError;

			byte[] syn = new byte[4];
			syn.WriteTo(0, 1u);
			_WebSocketChannel.Start(syn).Forget();
		}

		private static void OnConnect()
		{
			Log.Info("WebSocketChannel Connected");
			EventManager.AddListener(123, Handle);
		}

		private static void OnDisconnect()
		{
			Log.Info("WebSocketChannel Disconnected");
			EventManager.RemoveListener(123, Handle);
		}

		private static void OnError(NetworkError error, Exception ex)
		{
			Log.Error($"WebSocketChannel error = {error}, exception = {ex}");
		}

		private static void Handle(int eventID, IEventParam param)
		{
			if (param is EventParam<IMessage> msg)
			{
				Log.Info("[WebSocketChannel.Handle] Is MainThread =  " + IcyFrame.Instance.IsMainThread());
				Log.Info((msg.Value as TestMessageResult).ErrorMsg);
			}
		}

		public static void /*async UniTaskVoid*/ Update()
		{
			//if (Input.GetKeyUp(KeyCode.C))
			//	await _WebSocketChannel.Connect();

			//if (Input.GetKeyUp(KeyCode.L))
			//	await _WebSocketChannel.Listen();

			//if (Input.GetKey(KeyCode.S))
			//{
			//	for (int i = 0; i < 2; i++)
			//	{
			//		TestMessageResult messageResult = new TestMessageResult();
			//		messageResult.ErrorCode = 0;
			//		messageResult.ErrorMsg = "Success";
			//		_WebSocketChannel.Send(1, messageResult);
			//	} 
			//}

			//每帧发送，模拟大量网络IO的情况
			if (_WebSocketChannel != null && _WebSocketChannel.IsConnected)
			{
				_MessageResult.ErrorCode = 0;
				_MessageResult.ErrorMsg = "Success";
				_WebSocketChannel.Send(1001, _MessageResult);
			}

			if (Input.GetKeyUp(KeyCode.D))
				_WebSocketChannel.Dispose(new byte[3] { 4, 5, 6}).Forget();
		}
	}
}
#endif
