using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.Network
{
	/// <summary>
	/// 在TcpSession上的封装，可自定义Sender和Receiver
	/// </summary>
	public class TcpChannel : TcpSession
	{
		public TcpSender Sender { get; protected set; }
		public TcpRecevier Recevier { get; protected set; }


		public TcpChannel(string host, int port, TcpSender sender, TcpRecevier recevier, int bufferSize = 4096)
			: base(host, port, bufferSize)
		{
			Sender = sender;
			sender.SetTcpChannel(this);
			Recevier = recevier;
			InitSession().Forget();
		}

		public void Send<T0>(T0 data)
		{
			Sender.Encode(data);
		}

		public void Send<T0, T1>(T0 data, T1 data1)
		{
			Sender.Encode(data, data1);
		}

		public void Send<T0, T1, T2>(T0 data, T1 data1, T2 data2)
		{
			Sender.Encode(data, data1, data2);
		}

		public void Send<T0, T1, T2, T3>(T0 data, T1 data1, T2 data2, T3 data3)
		{
			Sender.Encode(data, data1, data2, data3);
		}

		public void Send<T0, T1, T2, T3, T4>(T0 data, T1 data1, T2 data2, T3 data3, T4 data4)
		{
			Sender.Encode(data, data1, data2, data3, data4);
		}

		internal new void Send(byte[] encodedData, int startIdx, int length)
		{
			base.Send(encodedData, startIdx, length);
		}

		protected async UniTaskVoid InitSession()
		{
			OnReceive += Recevier.Decode;
			await Connect();
			await Listen();
		}

		public override void Dispose()
		{
			OnReceive -= Recevier.Decode;
			base.Dispose();
		}
	}
}
