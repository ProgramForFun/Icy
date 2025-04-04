#if UNITY_EDITOR
using Google.Protobuf;
using Icy.Base;
using System;
using TestMsg;
using UnityEngine;

namespace Icy.Network
{
	public class TcpSenderProtobuf : TcpSender
	{
		public override void Encode<T0, T1>(T0 data, T1 data1)
		{
			//通过这种方式将泛型类型转换为实际类型
			if (data is int msgID && data1 is IMessage proto)
			{
				//一个int类型消息ID + protobuf消息
				byte[] buf = BitConverter.GetBytes(msgID);
				Array.Copy(buf, 0, _Buffer, 0, buf.Length);
				byte[] encodedData = proto.ToByteArray();
				Array.Copy(encodedData, 0, _Buffer, buf.Length, encodedData.Length);
				Send(_Buffer, 0, buf.Length + encodedData.Length);
			}
		}
	}

	public class TcpReceiverProtobuf : TcpRecevier
	{
		public override void Decode(byte[] data, int startIdx, int length)
		{
			//解析int类型的消息ID
			Log.LogInfo($"Receive msg ID = {BitConverter.ToInt32(data, startIdx)}");
			//解析protobuf消息本体
			int protoStartIdx = startIdx + sizeof(int);
			int protoLength = length - sizeof(int);
			TestMessageResult newMessageResult = TestMessageResult.Descriptor.Parser.ParseFrom(data, protoStartIdx, protoLength) as TestMessageResult;
			Log.LogInfo(newMessageResult.ErrorMsg);
		}
	}

	public static class TcpChannelTest
	{
		static TcpChannel _TcpChannel;

		public static void Test()
		{
			_TcpChannel = new TcpChannel("127.0.0.1", 12321, new TcpSenderProtobuf(), new TcpReceiverProtobuf());
			_TcpChannel.OnConnected += OnConnect;
			_TcpChannel.OnDisconnected += OnDisconnect;
			//由于有TcpReceiver的存在，这里没必要监听OnReceive了
			//_TcpChannel.OnReceive += OnReceiveData;
			_TcpChannel.OnConnectException += OnConnectException;
			_TcpChannel.OnListenException += OnListenException;
		}

		private static void OnConnect()
		{
			Log.LogInfo("TcpChannel Connected");
		}

		private static void OnDisconnect()
		{
			Log.LogInfo("TcpChannel Disconnected");
		}

		private static void OnConnectException(Exception ex)
		{
			Log.LogError($"TcpChannel connect excetion {ex}");
		}

		private static void OnListenException(Exception ex)
		{
			Log.LogError($"TcpChannel handle received excetion {ex}");
		}

		public static async void Update()
		{
			if (Input.GetKeyUp(KeyCode.C))
				await _TcpChannel.Connect();

			if (Input.GetKeyUp(KeyCode.L))
				await _TcpChannel.Listen();

			if (Input.GetKey(KeyCode.S))
			{
				for (int i = 0; i < 2; i++)
				{
					TestMessageResult messageResult = new TestMessageResult();
					messageResult.ErrorCode = 0;
					messageResult.ErrorMsg = "Success";
					_TcpChannel.Send(1, messageResult);
				}
			}

			if (Input.GetKeyUp(KeyCode.D))
				_TcpChannel.Disconnect();
		}
	}
}
#endif
