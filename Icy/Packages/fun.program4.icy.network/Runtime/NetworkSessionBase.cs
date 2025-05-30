using Cysharp.Threading.Tasks;
using System;

public abstract class NetworkSessionBase : IDisposable
{
	/// <summary>
	/// 服务器域名或IP地址
	/// </summary>
	public string Host { get; protected set; }
	/// <summary>
	/// 服务器端口
	/// </summary>
	public int Port { get; protected set; }
	/// <summary>
	/// 是否已和服务器建立连接
	/// </summary>
	public bool IsConnected { get; protected set; }

	/// <summary>
	/// 和服务器建立连接、可以通信了的事件
	/// </summary>
	public Action OnConnected;
	/// <summary>
	/// 和服务器断开连接的事件
	/// </summary>
	public Action OnDisconnected;
	/// <summary>
	/// 接收到服务器消息的事件
	/// </summary>
	public Action<byte[], int, int> OnReceive;
	/// <summary>
	/// 连接服务器的异常
	/// </summary>
	public Action<Exception> OnConnectException;
	/// <summary>
	/// 接收处理服务器数据的异常
	/// </summary>
	public Action<Exception> OnListenException;

	/// <summary>
	/// Buffer大小
	/// </summary>
	protected int _BufferSize;
	/// <summary>
	/// 接收Buffer
	/// </summary>
	protected byte[] _ReceiveBuffer;
	/// <summary>
	/// 发送Buffer
	/// </summary>
	protected byte[] _SendBuffer;


	public NetworkSessionBase(string host, int port, int bufferSize)
	{
		Host = host;
		Port = port;
		_BufferSize = bufferSize;
		_ReceiveBuffer = new byte[bufferSize];
		_SendBuffer = new byte[bufferSize];
	}

	/// <summary>
	/// 连接服务器
	/// </summary>
	public abstract UniTask Connect();
	/// <summary>
	/// 监听服务器消息
	/// </summary>
	public abstract UniTask Listen();
	/// <summary>
	/// 发送消息
	/// </summary>
	public abstract void Send(byte[] msg, int startIdx, int length);
	/// <summary>
	/// 和服务器断开连接
	/// </summary>
	public abstract void Disconnect();
	/// <summary>
	/// 处理服务器的消息
	/// </summary>
	protected abstract void HandleReceived(byte[] buffer, int receivedSize);
	/// <summary>
	/// 销毁Session
	/// </summary>
	public abstract void Dispose();
}
