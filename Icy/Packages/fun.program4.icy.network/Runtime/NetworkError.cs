namespace Icy.Network
{
	/// <summary>
	/// 网络相关的错误
	/// </summary>
	public enum NetworkError
	{
		/// <summary>
		/// 连接服务器失败
		/// </summary>
		ConnectFailed,
		/// <summary>
		/// 在未连接的时候尝试发送
		/// </summary>
		SendWhenDisconnected,
		/// <summary>
		/// 发送失败
		/// </summary>
		SendFailed,
		/// <summary>
		/// 接收失败
		/// </summary>
		ReceiveFailed,
	}
}
