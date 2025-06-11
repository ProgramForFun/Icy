
namespace Icy.Network
{
	/// <summary>
	/// 支持的Session类型
	/// </summary>
	public enum NetworkSessionType
	{
		Tcp,
		Kcp
	}

	/// <summary>
	/// 构造NetworkChannel的参数
	/// </summary>
	public class NetworkChannelArgs
	{
		/// <summary>
		/// Session类型
		/// </summary>
		public NetworkSessionType SessionType;
		/// <summary>
		/// 服务器地址
		/// </summary>
		public string Host;
		/// <summary>
		/// 服务器端口
		/// </summary>
		public int Port;
		/// <summary>
		/// Buffer大小
		/// </summary>
		public int BufferSize;
		/// <summary>
		/// 负责序列化的Sender
		/// </summary>
		public NetworkSenderBase Sender;
		/// <summary>
		/// 负责反序列化的Receiver
		/// </summary>
		public NetworkReceiverBase Receiver;
	}
}
