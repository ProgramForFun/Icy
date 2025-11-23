#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Icy.Base;
using System;
using TestMsg;
using UnityEngine;

namespace Icy.Network
{
	public static class KcpChannelTest
	{
		static NetworkChannel<IMessage> _KcpChannel;
		static TestMessageResult _MessageResult;
		

		public static void Test()
		{
			_MessageResult = new TestMessageResult();

			KcpSession session = new KcpSession("127.0.0.1", 12333, 4096);
			_KcpChannel = new NetworkChannel<IMessage>(session, new ProtoSender(), new ProtoReceiver());
			_KcpChannel.OnConnected = OnConnect;
			_KcpChannel.OnDisconnected += OnDisconnect;
			_KcpChannel.OnError += OnError;

			byte[] syn = new byte[4];
			syn.WriteTo(0, 1u);
			_KcpChannel.Start(syn).Forget();
		}

		private static void OnConnect()
		{
			Log.Info("KcpChannel Connected");
			EventManager.AddListener(123, Handle);
		}

		private static void OnDisconnect()
		{
			Log.Info("KcpChannel Disconnected");
			EventManager.RemoveListener(123, Handle);
		}

		private static void OnError(NetworkError error, Exception ex)
		{
			Log.Error($"KcpChannel error = {error}, exception = {ex}");
		}

		private static void Handle(int eventID, IEventParam param)
		{
			if (param is EventParam<IMessage> msg)
			{
				Log.Info("[KcpChannelTest.Handle] Is MainThread =  " + CommonUtility.IsMainThread());
				Log.Info((msg.Value as TestMessageResult).ErrorMsg);
			}
		}

		public static void /*async UniTaskVoid*/ Update()
		{
			//if (Input.GetKeyUp(KeyCode.C))
			//	await KcpChannel.Connect();

			//if (Input.GetKeyUp(KeyCode.L))
			//	await KcpChannel.Listen();

			//if (Input.GetKey(KeyCode.S))
			//{
			//	for (int i = 0; i < 2; i++)
			//	{
			//		TestMessageResult messageResult = new TestMessageResult();
			//		messageResult.ErrorCode = 0;
			//		messageResult.ErrorMsg = "Success";
			//		KcpChannel.Send(1, messageResult);
			//	}
			//}

			//每帧发送，模拟大量网络IO的情况
			if (_KcpChannel != null && _KcpChannel.IsConnected)
			{
				_MessageResult.ErrorCode = 0;
				_MessageResult.ErrorMsg = "Success";
				_KcpChannel.Send(1001, _MessageResult);
			}

			if (Input.GetKeyUp(KeyCode.D))
				_KcpChannel.Dispose(new byte[3] { 4, 5, 6}).Forget();
		}
	}
}
#endif
