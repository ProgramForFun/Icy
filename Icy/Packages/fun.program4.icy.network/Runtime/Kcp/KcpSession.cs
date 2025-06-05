using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Net;
using System.Net.Sockets;

namespace Icy.Network
{
	/// <summary>
	/// 基于Kcp的Reliable UDP通信类
	/// </summary>
	public class KcpSession : NetworkSessionBase, IUpdateable
	{
		private static readonly long epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

		public static class KcpProtocolType
		{
			public const short SYN = 101;
			public const short ACK = 102;
			public const short FIN = 103;
			public const short MSG = 104;
		}

		/// <summary>
		/// Session创建时间，单位ms
		/// </summary>
		protected long _StartTime;
		/// <summary>
		/// 当前时间，单位ms
		/// </summary>
		protected uint _TimeNow;
		/// <summary>
		/// 底层UDP Socket
		/// </summary>
		protected Socket _Socket;
		/// <summary>
		/// C#版本的Kcp
		/// </summary>
		protected Kcp _Kcp;
		/// <summary>
		/// 服务器地址和端口
		/// </summary>
		protected readonly IPEndPoint _RemoteEndPoint;
		/// <summary>
		/// 上次接收数据的时间，单位ms
		/// </summary>
		protected uint _LastRecvTime;
		/// <summary>
		/// 用于接收的EndPoint
		/// </summary>
		protected EndPoint _IpEndPoint = new IPEndPoint(IPAddress.Any, 0);

		protected uint _LocalConn;
		protected uint _RemoteConn;


		public KcpSession(string host, int port, int bufferSize = 4096) : base(host, port, bufferSize)
		{
			_StartTime = ClientNow();
			_TimeNow = (uint)(ClientNow() - _StartTime);
			_RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);

			IcyFrame.Instance.AddUpdate(this);
		}

		public override UniTask Connect()
		{
			_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			_Socket.Bind(new IPEndPoint(IPAddress.Any, 0));

			_LocalConn = (uint)UnityEngine.Random.Range(1000, int.MaxValue);
			_RemoteConn = _LocalConn;

			_Kcp = new Kcp(_RemoteConn, new IntPtr(_LocalConn));
			_Kcp.SetOutput(KcpOutput);

			_Kcp.NoDelay(1, 10, 1, 1);
			_Kcp.WndSize(32, 32);
			_Kcp.SetMTU(470);

			_LastRecvTime = _TimeNow;

			try
			{
				_SendBuffer.WriteTo(0, 4);
				_SendBuffer.WriteTo(4, KcpProtocolType.SYN);
				Send(_SendBuffer, 0, 6);
			}
			catch (Exception e)
			{
				OnConnectException?.Invoke(e);
			}

			return UniTask.CompletedTask;
		}

		public override void Disconnect()
		{
			if (_Socket == null)
				return;

			try
			{
				_SendBuffer.WriteTo(0, 4);
				_SendBuffer.WriteTo(4, KcpProtocolType.FIN);
				Send(_SendBuffer, 0, 6);

				if (_Kcp != null)
				{
					_Kcp.Release();
					_Kcp = null;
				}
				_Socket = null;

				IsConnected = false;
				OnDisconnected?.Invoke();
			}
			catch (Exception e)
			{
			Log.LogInfo("Dispose", nameof(KcpSession));
				Log.LogError(e.ToString(), nameof(KcpSession));
			}
		}

		public override void Dispose()
		{
			IcyFrame.Instance.RemoveUpdate(this);

			try
			{
				if (IsConnected)
				{
					for (int i = 0; i < 4; i++)
						Disconnect();
				}
			}
			catch (Exception)
			{
				// ignored
			}

			Log.LogInfo("Dispose", nameof(KcpSession));
		}

		public override UniTask Listen()
		{
			return UniTask.CompletedTask;
		}

		public override void Send(byte[] msg, int startIdx, int length)
		{
			if (_Kcp == null)
				OnListenException?.Invoke(new Exception($"Call {nameof(Send)} when Kcp is disconnected"));
			else
				_Kcp.Send(msg, startIdx, length);
		}

		protected virtual void Recv()
		{
			while (_Socket != null && _Socket.Available > 0)
			{
				int messageLength = 0;
				try
				{
					messageLength = _Socket.ReceiveFrom(_ReceiveBuffer, ref _IpEndPoint);
					if (messageLength < 1)
						continue;

					HandleReceived(_ReceiveBuffer, messageLength);
				}
				catch (Exception e)
				{
					OnListenException?.Invoke(e);
				}
			}
		}

		protected override void HandleReceived(byte[] buffer, int receivedSize)
		{
			_Kcp.Input(buffer, 0, receivedSize);

			while (true)
			{
				int n = _Kcp.PeekSize();
				if (n < 0)
					return;

				if (n == 0)
				{
					OnListenException(new Exception());
					return;
				}

				int count = _Kcp.Recv(buffer, 0, ushort.MaxValue);

				if (n != count)
					return;

				if (count <= 0)
					return;

				_LastRecvTime = _TimeNow;

				if (!IsConnected)
				{
					IsConnected = true;
					OnConnected?.Invoke();
				}
				else
					OnReceive?.Invoke(buffer, 0, n);
			}
		}

		protected void KcpOutput(byte[] bytes, int len, object user)
		{
			try
			{
				if (len == 0)
				{
					Log.LogError($"Kcp output 0", nameof(KcpSession));
					return;
				}

				Buffer.BlockCopy(bytes, 0, _SendBuffer, 0, len);
				_Socket.SendTo(_SendBuffer, 0, len, SocketFlags.None, _RemoteEndPoint);
			}
			catch (Exception e)
			{
				OnListenException(e);
			}
		}

		/// <summary>
		/// 客户端当前时间，单位ms
		/// </summary>
		protected long ClientNow()
		{
			return (DateTime.UtcNow.Ticks - epoch) / 10000;
		}

		public void Update(float delta)
		{
			if (_Kcp == null)
				return;

			_TimeNow = (uint)(ClientNow() - _StartTime);

			Recv();

			try
			{
				_Kcp.Update(_TimeNow);
			}
			catch (Exception e)
			{
				OnListenException?.Invoke(e);
				return;
			}

			if (_Kcp != null)
			{
				uint nextUpdateTime = _Kcp.Check(_TimeNow);
			}
		}
	}
}
