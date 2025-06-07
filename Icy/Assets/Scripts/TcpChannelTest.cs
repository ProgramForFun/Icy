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
	public class TcpSenderProtobuf : TcpSender
	{
		public override void Encode<T>(int msgID, T data)
		{
			//通过这种方式将泛型类型转换为实际类型
			if (data is IMessage proto)
			{
				//一个int类型消息ID + protobuf消息
				int msgIDSize = sizeof(int);
				BitConverter.TryWriteBytes(_Buffer, msgID);
				int protoSize = proto.CalculateSize();
				//用Span降低Protobuf序列化的GC
				Span<byte> outputSpan = new Span<byte>(_Buffer, msgIDSize, protoSize);
				proto.WriteTo(outputSpan);
				Send(0, msgIDSize + protoSize);
			}
		}
	}

	public class TcpReceiverProtobuf : TcpReceiver
	{
		private readonly Dictionary<int, Type> _MsgTypeMap = new Dictionary<int, Type>()
		{
			{ 1, typeof(TestMessageResult)},
		};
		private Dictionary<int, IMessage> _Cache = new Dictionary<int, IMessage>();

		public override void Decode(byte[] data, int startIdx, int length)
		{
			//解析int类型的消息ID
			int msgID = BitConverter.ToInt32(data, startIdx);
			//Log.LogInfo($"Receive msg ID = {BitConverter.ToInt32(data, startIdx)}");

			//解析protobuf消息本体
			int protoStartIdx = startIdx + sizeof(int);
			int protoLength = length - sizeof(int);
			//用Span降低Protobuf反序列化的GC
			ReadOnlySequence<byte> span = new ReadOnlySequence<byte>(data, protoStartIdx, protoLength);
			if (_Cache.ContainsKey(msgID))
			{
				IMessage newMessageResult = _Cache[msgID];
				//TODO：确认下面链接9.2提到的Clear的问题
				//https://www.cnblogs.com/wsk-0000/articles/12675826.html
				//https://zhuanlan.zhihu.com/p/588709957
				newMessageResult.MergeFrom(span);
			}
			else
			{
				Type msgType = _MsgTypeMap[msgID];
				IMessage msg = Activator.CreateInstance(msgType) as IMessage;
				msg = msg.Descriptor.Parser.ParseFrom(span);
				_Cache.Add(msgID, msg);
			}
			//Log.LogInfo(newMessageResult.ErrorMsg);
		}
	}

	public static class TcpChannelTest
	{
		static TcpChannel _TcpChannel;
		static TestMessageResult _MessageResult;
		

		public static void Test()
		{
			_MessageResult = new TestMessageResult();

			_TcpChannel = new TcpChannel("127.0.0.1", 12321, new TcpSenderProtobuf(), new TcpReceiverProtobuf());
			_TcpChannel.OnConnected += OnConnect;
			_TcpChannel.OnDisconnected += OnDisconnect;
			//由于有TcpReceiver的存在，这里没必要监听OnReceive了
			//_TcpChannel.OnReceive += OnReceiveData;
			_TcpChannel.OnError += OnError;
		}

		private static void OnConnect()
		{
			Log.LogInfo("TcpChannel Connected");
		}

		private static void OnDisconnect()
		{
			Log.LogInfo("TcpChannel Disconnected");
		}

		private static void OnError(NetworkError error, Exception ex)
		{
			Log.LogError($"TcpChannel error = {error}, exception = {ex}");
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
				_TcpChannel.Send(1, _MessageResult);
			}

			//if (Input.GetKeyUp(KeyCode.D))
			//	_TcpChannel.Disconnect();
		}
	}
}
#endif
