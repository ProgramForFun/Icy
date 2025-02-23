using System.Net.Sockets;
using System.Text;
using System;
using Icy.Base;
using Cysharp.Threading.Tasks;

/// <summary>
/// 一个简单的TCP通信类，基于System.Net.Sockets.TcpClient封装；
/// 处理了粘包，基于包头的一个int大小的消息长度实现
/// </summary>
public class TcpSession : IDisposable
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
	public bool IsConnected { get; private set; }

	/// <summary>
	/// 和服务器建立连接、可以通信了的事件
	/// </summary>
	public event Action OnConnected;
	/// <summary>
	/// 和服务器断开连接的事件
	/// </summary>
	public event Action OnDisconnected;
	/// <summary>
	/// 接收到服务器消息的事件
	/// </summary>
	public event Action<byte[], int, int> OnReceive;
	/// <summary>
	/// 连接服务器的异常
	/// </summary>
	public event Action<Exception> OnConnectException;
	/// <summary>
	/// 接收处理服务器数据的异常
	/// </summary>
	public event Action<Exception> OnHandleReceivedException;


	protected TcpClient _TcpClient;
	protected NetworkStream _Stream;
	protected byte[] _ReceiveBuffer;
	protected byte[] _SendBuffer;

	/// <summary>
	/// 当前正在接收的消息的长度，包括长度本身的4字节
	/// </summary>
	protected int _CurMsgLen = 0;
	/// <summary>
	/// 当前正在接收的消息（包括消息长度），在buffer中的起始index
	/// </summary>
	protected int _CurMsgStartIdx = 0;
	/// <summary>
	/// 当前Buffer中剩余多少数据
	/// </summary>
	protected int _BufferRemain = 0;
	/// <summary>
	/// Buffer大小
	/// </summary>
	protected int _BufferSize;
	/// <summary>
	/// 消息长度本身，的长度
	/// </summary>
	protected const int MSG_LENGTH_SIZE = sizeof(int);


	public TcpSession(string host, int port, int bufferSize = 4096)
	{
		Host = host;
		Port = port;
		_BufferSize = bufferSize;
		_ReceiveBuffer = new byte[bufferSize];
		_SendBuffer = new byte[bufferSize];
	}

	/// <summary>
	/// 和服务器建立连接
	/// </summary>
	public async UniTask Connect()
	{
		if (IsConnected)
		{
			Log.LogError("[TcpSession] Duplicate Connect");
			return;
		}

		try
		{
			_TcpClient = new TcpClient();
			await _TcpClient.ConnectAsync(Host, Port);
			_TcpClient.NoDelay = false;  //关闭Nagle算法
			_Stream = _TcpClient.GetStream();
			IsConnected = true;
			OnConnected?.Invoke();
			Log.LogInfo("[TcpSession] Connected");
		}
		catch (Exception ex)
		{
			Log.LogError($"[TcpSession] Connect exception : {ex}");
			OnConnectException?.Invoke(ex);
		}
	}

	/// <summary>
	/// 监听消息
	/// </summary>
	public async UniTask Listen()
	{
		Log.LogInfo("[TcpSession] Start Listen");
		try
		{
			while (IsConnected)
			{
				int receivedSize = await _Stream.ReadAsync(_ReceiveBuffer, _BufferRemain, _BufferSize - _BufferRemain);
				HandleReceived(_ReceiveBuffer, receivedSize);
			}
		}
		catch (Exception ex)
		{
			Log.LogError($"[TcpSession] HandleReceived exception : {ex}");
			OnHandleReceivedException?.Invoke(ex);
		}
	}

	/// <summary>
	/// 发送消息
	/// </summary>
	public void Send(string msg)
	{
		Send(Encoding.UTF8.GetBytes(msg));
	}
	public void Send(byte[] msg)
	{
		if (!IsConnected)
		{
			Log.LogError("[TcpSession] Trying to send when TCP disconnected");
			return;
		}

		//消息结构前4个byte为本数据包的长度，以解决粘包问题
		//  消息长度 消息本体
		// |- - - -| - - -...
		//     4      n

		//1、长度 = 消息ID + 消息本体的长度
		int len = msg.Length + MSG_LENGTH_SIZE;
		byte[] buf = BitConverter.GetBytes(len);
		Array.Copy(buf, 0, _SendBuffer, 0, buf.Length);
		//2、消息本体
		Array.Copy(msg, 0, _SendBuffer, MSG_LENGTH_SIZE, msg.Length);
		_Stream.Write(_SendBuffer, 0, len);
	}

	/// <summary>
	/// 关闭连接
	/// </summary>
	public void Close()
	{
		if (!IsConnected)
		{
			Log.LogError("[TcpSession] Close when disconnected");
			return;
		}
		IsConnected = false;
		_TcpClient.Close();
		_TcpClient = null;
		OnDisconnected?.Invoke();
		Log.LogInfo("[TcpSession] Close");
	}

	/// <summary>
	/// 处理TCP接收到的消息
	/// </summary>
	private void HandleReceived(byte[] buffer, int receivedSize)
	{
		_BufferRemain += receivedSize;
		while (true)
		{
			if (_CurMsgLen == 0)
			{
				//处理消息长度都没能完整发过来的情况
				if (_BufferRemain < MSG_LENGTH_SIZE)
					break;
				//获取消息长度
				else
					_CurMsgLen = BitConverter.ToInt32(buffer, _CurMsgStartIdx);
			}

			//当前buffer里的数据，足够一条完整消息，正常处理
			if (_BufferRemain >= _CurMsgLen)
			{
				OnReceive?.Invoke(buffer, _CurMsgStartIdx + MSG_LENGTH_SIZE, _CurMsgLen - MSG_LENGTH_SIZE);
				_BufferRemain -= _CurMsgLen;
				if (_BufferRemain > 0)
					_CurMsgStartIdx += _CurMsgLen;
				_CurMsgLen = 0;

				//如果当前buffer有剩余 且 不是从下标0开始的，挪到下标0开始，方便逻辑处理
				//TODO：其实应该搞一个环形buffer，避免Copy，更高效
				if (_BufferRemain > 0 && _CurMsgStartIdx > 0)
				{
					//Log.LogInfo("Move " + _BufferRemain);
					Array.Copy(_ReceiveBuffer, _CurMsgStartIdx, _ReceiveBuffer, 0, _BufferRemain);
					_CurMsgStartIdx = 0;
				}
			}
			//等待下一轮数据到来，凑成完整消息再处理
			else
				break;
		}
	}

	public void Dispose()
	{
		if (IsConnected)
			Close();
		Log.LogInfo("[TcpSession] Dispose");
	}
}
