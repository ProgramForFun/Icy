using Cysharp.Threading.Tasks;

namespace Icy.Network
{
	/// <summary>
	/// 在TcpSession上的封装，可自定义Sender和Receiver
	/// </summary>
	public class TcpChannel : TcpSession
	{
		public TcpSender Sender { get; protected set; }
		public TcpReceiver Receiver { get; protected set; }


		public TcpChannel(string host, int port, TcpSender sender, TcpReceiver receiver, int bufferSize = 4096)
			: base(host, port, bufferSize)
		{
			Sender = sender;
			sender.SetTcpChannel(this);
			Receiver = receiver;
			InitSession().Forget();
		}

		public void Send<T>(T data)
		{
			Sender.Encode(data);
		}

		public void Send<T>(int arg1, T data)
		{
			Sender.Encode(arg1, data);
		}

		public void Send<T>(int arg1, int arg2, T data)
		{
			Sender.Encode(arg1, arg2, data);
		}

		public void Send<T>(int arg1, int arg2, int arg3, T data)
		{
			Sender.Encode(arg1, arg2, arg3, data);
		}

		internal new void Send(byte[] encodedData, int startIdx, int length)
		{
			base.Send(encodedData, startIdx, length);
		}

		protected async UniTaskVoid InitSession()
		{
			OnReceive += Receiver.Decode;
			await Connect();
			await Listen();
		}

		public override void Dispose()
		{
			OnReceive -= Receiver.Decode;
			base.Dispose();
		}
	}
}
