#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Icy.Base;
using System;
using System.Buffers;
using System.Collections.Generic;
using TestMsg;
using UnityEngine;

namespace Icy.Network
{
	public class ProtoSender : NetworkSenderBase<IMessage>
	{
		public ProtoSender() : base(4096)
		{

		}

		public override async UniTask Encode(int msgID, IMessage proto)
		{
			Log.Info("[ProtoSender] Is MainThread =  " + CommonUtility.IsMainThread());
			//一个int类型消息ID + protobuf消息
			int msgIDSize = sizeof(int);
			BitConverter.TryWriteBytes(_Buffer, msgID);
			int protoSize = proto.CalculateSize();
			DoEncode(msgIDSize, protoSize, proto);
			await Send(0, msgIDSize + protoSize);
		}

		/// <summary>
		/// Span不能用在async函数里，这里拿出来放在普通函数里
		/// </summary>
		private void DoEncode(int msgIDSize, int protoSize, IMessage proto)
		{
			//用Span降低Protobuf序列化的GC
			Span<byte> outputSpan = new Span<byte>(_Buffer, msgIDSize, protoSize);
			proto.WriteTo(outputSpan);
		}
	}

	public class ProtoReceiver : NetworkReceiverBase
	{
		private Dictionary<int, IMessage> _Cache = new Dictionary<int, IMessage>();

		public override void Decode(byte[] data, int startIdx, int length)
		{
			Log.Info("[ProtoReceiver] Is MainThread =  " + CommonUtility.IsMainThread());
			//解析int类型的消息ID
			int msgID = BitConverter.ToInt32(data, startIdx);
			//Log.LogInfo($"Receive msg ID = {BitConverter.ToInt32(data, startIdx)}");

			//解析protobuf消息本体
			int protoStartIdx = startIdx + sizeof(int);
			int protoLength = length - sizeof(int);
			//用Span降低Protobuf反序列化的GC
			ReadOnlySequence<byte> span = new ReadOnlySequence<byte>(data, protoStartIdx, protoLength);

			IMessage msg = null;
			if (_Cache.ContainsKey(msgID))
			{
				msg = _Cache[msgID];
				//由于下面链接9.2提到的Clear的问题，为proto扩展了一个Reset方法，来在复用前重置状态
				//https://www.cnblogs.com/wsk-0000/articles/12675826.html
				//https://zhuanlan.zhihu.com/p/588709957
				msg.Reset();
				msg.MergeFrom(span);
			}
			else
			{
				Google.Protobuf.Reflection.MessageDescriptor descriptor = ProtoMsgIDRegistry.GetMsgDescriptor(msgID);
				msg = descriptor.Parser.ParseFrom(span);
				_Cache.Add(msgID, msg);
			}

			if (msg != null)
			{
				EventParam<IMessage> evtParam = EventManager.GetParam<EventParam<IMessage>>();
				evtParam.Value = msg;
				EventManager.Trigger(123, evtParam);
			}
		}
	}

	public static class TcpChannelTest
	{
		static NetworkChannel<IMessage> _TcpChannel;
		static TestMessageResult _MessageResult;
		

		public static void Test()
		{
			_MessageResult = new TestMessageResult();

			TcpSession session = new TcpSession("127.0.0.1", 12321, 4096);
			_TcpChannel = new NetworkChannel<IMessage>(session, new ProtoSender(), new ProtoReceiver());
			_TcpChannel.OnConnected = OnConnect;
			_TcpChannel.OnDisconnected += OnDisconnect;
			_TcpChannel.OnError += OnError;
			_TcpChannel.Start().Forget();
		}

		private static void OnConnect()
		{
			Log.Info("TcpChannel Connected");
			EventManager.AddListener(123, Handle);
		}

		private static void OnDisconnect()
		{
			Log.Info("TcpChannel Disconnected");
			EventManager.RemoveListener(123, Handle);
		}

		private static void OnError(NetworkError error, Exception ex)
		{
			Log.Error($"TcpChannel error = {error}, exception = {ex}");
		}

		private static void Handle(int eventID, IEventParam param)
		{
			if (param is EventParam<IMessage> msg)
			{
				Log.Info("[TcpChannelTest.Handle] Is MainThread =  " + CommonUtility.IsMainThread());
				Log.Info((msg.Value as TestMessageResult).ErrorMsg);
			}
		}

		public static void /*async UniTaskVoid*/ Update()
		{
			//if (Input.GetKeyUp(KeyCode.C))
			//	await _TcpChannel.Connect();

			//if (Input.GetKeyUp(KeyCode.L))
			//	await _TcpChannel.Listen();

			//if (Input.GetKey(KeyCode.S))
			//{
			//	for (int i = 0; i < 2; i++)
			//	{
			//		TestMessageResult messageResult = new TestMessageResult();
			//		messageResult.ErrorCode = 0;
			//		messageResult.ErrorMsg = "Success";
			//		_TcpChannel.Send(1, messageResult);
			//	}
			//}

			//每帧发送，模拟大量网络IO的情况
			if (_TcpChannel != null && _TcpChannel.IsConnected)
			{
				_MessageResult.ErrorCode = 0;
				_MessageResult.ErrorMsg = "Success";
				_TcpChannel.Send(1001, _MessageResult);
			}

			if (Input.GetKeyUp(KeyCode.D))
				_TcpChannel.Dispose().Forget();
		}
	}
}
#endif
