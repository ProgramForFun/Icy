
namespace Icy.Network
{
	/// <summary>
	/// 和TcpChannel一起使用的，负责接收、解析数据
	/// </summary>
	public abstract class TcpReceiver
	{
		protected byte[] _Buffer;

		public TcpReceiver(int bufferSize = 4096)
		{
			_Buffer = new byte[bufferSize];
		}

		public abstract void Decode(byte[] data, int startIdx, int length);
	}
}
