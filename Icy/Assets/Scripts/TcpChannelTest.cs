#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Icy.Base;
using System;
using System.Buffers;
using System.Collections.Generic;
using TestMsg;

namespace Icy.Network
{
	public class ProtoBufSender : NetworkSenderBase
	{
		public override void Encode<T>(int msgID, T data)
		{
			//ͨ�����ַ�ʽ����������ת��Ϊʵ������
			if (data is IMessage proto)
			{
				//һ��int������ϢID + protobuf��Ϣ
				int msgIDSize = sizeof(int);
				BitConverter.TryWriteBytes(_Buffer, msgID);
				int protoSize = proto.CalculateSize();
				//��Span����Protobuf���л���GC
				Span<byte> outputSpan = new Span<byte>(_Buffer, msgIDSize, protoSize);
				proto.WriteTo(outputSpan);
				Send(0, msgIDSize + protoSize);
			}
		}
	}

	public class ProtoBufReceiver : NetworkReceiverBase
	{
		private readonly Dictionary<int, Type> _MsgTypeMap = new Dictionary<int, Type>()
		{
			{ 1, typeof(TestMessageResult)},
		};
		private Dictionary<int, IMessage> _Cache = new Dictionary<int, IMessage>();

		public override void Decode(byte[] data, int startIdx, int length)
		{
			//����int���͵���ϢID
			int msgID = BitConverter.ToInt32(data, startIdx);
			//Log.LogInfo($"Receive msg ID = {BitConverter.ToInt32(data, startIdx)}");

			//����protobuf��Ϣ����
			int protoStartIdx = startIdx + sizeof(int);
			int protoLength = length - sizeof(int);
			//��Span����Protobuf�����л���GC
			ReadOnlySequence<byte> span = new ReadOnlySequence<byte>(data, protoStartIdx, protoLength);
			if (_Cache.ContainsKey(msgID))
			{
				IMessage newMessageResult = _Cache[msgID];
				//TODO��ȷ����������9.2�ᵽ��Clear������
				//https://www.cnblogs.com/wsk-0000/articles/12675826.html
				//https://zhuanlan.zhihu.com/p/588709957
				newMessageResult.MergeFrom(span);

				Log.LogInfo((newMessageResult as TestMessageResult).ErrorMsg);
			}
			else
			{
				Type msgType = _MsgTypeMap[msgID];
				IMessage msg = Activator.CreateInstance(msgType) as IMessage;
				msg = msg.Descriptor.Parser.ParseFrom(span);
				_Cache.Add(msgID, msg);

				Log.LogInfo((msg as TestMessageResult).ErrorMsg);
			}
		}
	}

	public static class TcpChannelTest
	{
		static NetworkChannel _TcpChannel;
		static TestMessageResult _MessageResult;
		

		public static void Test()
		{
			_MessageResult = new TestMessageResult();

			TcpSession session = new TcpSession("127.0.0.1", 12321, 4096);
			_TcpChannel = new NetworkChannel(session, new ProtoBufSender(), new ProtoBufReceiver());
			_TcpChannel.OnConnected = OnConnect;
			_TcpChannel.OnDisconnected += OnDisconnect;
			_TcpChannel.OnError += OnError;
			_TcpChannel.Start().Forget();
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

			//ÿ֡���ͣ�ģ���������IO�����
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
