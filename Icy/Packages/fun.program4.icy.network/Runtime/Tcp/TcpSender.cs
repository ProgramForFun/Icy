
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

		public virtual void Encode<T>(T data) { }
		public virtual void Encode<T>(int arg1, T data) { }
		public virtual void Encode<T>(int arg1, int arg2, T data) { }
		public virtual void Encode<T>(int arg1, int arg2, int arg3, T data) { }

		/// <summary>
		/// 转换成byte[]后，调用这个方法
		/// </summary>
		protected void Send(byte[] encodedData, int startIdx, int length)
		{
			_Channel.Send(encodedData, startIdx, length);
		}
	}
}
