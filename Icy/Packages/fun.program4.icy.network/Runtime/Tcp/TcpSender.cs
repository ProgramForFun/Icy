
namespace Icy.Network
{
	/// <summary>
	/// 和TcpChannel一起使用的，负责发送前的序列化、转换成byte[]的工作
	/// </summary>
	public abstract class TcpSender
	{
		protected TcpChannel _Channel;
		protected byte[] _Buffer;

		public TcpSender(int bufferSize = 4096)
		{
			_Buffer = new byte[bufferSize];
		}

		/// <summary>
		/// 把TcpChannel传进来
		/// </summary>
		public void SetTcpChannel(TcpChannel tcpChannel)
		{
			_Channel = tcpChannel;
		}

		public virtual void Encode<T0>(T0 data) { }
		public virtual void Encode<T0, T1>(T0 data, T1 data1) { }
		public virtual void Encode<T0, T1, T2>(T0 data, T1 data1, T2 data2) { }
		public virtual void Encode<T0, T1, T2, T3>(T0 data, T1 data1, T2 data2, T3 data3) { }
		public virtual void Encode<T0, T1, T2, T3, T4>(T0 data, T1 data1, T2 data2, T3 data3, T4 data4) { }

		/// <summary>
		/// 转换成byte[]后，调用这个方法
		/// </summary>
		protected void Send(byte[] encodedData, int startIdx, int length)
		{
			_Channel.Send(encodedData, startIdx, length);
		}
	}
}
