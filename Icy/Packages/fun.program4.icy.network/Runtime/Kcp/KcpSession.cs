//#define USE_KCP_SHARP //定义的话，使用C#版的KCP，否则使用Native版的；这里仅做测试用，正式开启去PlayerSetting中设置Symbol

using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Net;
using System.Net.Sockets;
#if !USE_KCP_SHARP
using System.Runtime.InteropServices;
#endif

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
#if USE_KCP_SHARP
		/// <summary>
		/// C#版本的Kcp
		/// </summary>
		protected Kcp _Kcp;
#else
		/// <summary>
		/// Native版本的Kcp
		/// </summary>
		protected IntPtr _Kcp = IntPtr.Zero;
#endif
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
		/// <summary>
		/// 当前错误码
		/// </summary>
		protected int _Error;

#if !USE_KCP_SHARP && !ENABLE_IL2CPP
		protected KcpOutput _KcpOutput;
#endif

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

#if USE_KCP_SHARP
			_Kcp = new Kcp(_RemoteConn, new IntPtr(_LocalConn));
			_Kcp.SetOutput(KcpOutput);

			_Kcp.NoDelay(1, 10, 1, 1);
			_Kcp.WndSize(32, 32);
			_Kcp.SetMTU(470);
#else
			_Kcp = KcpDll.KcpCreate(_RemoteConn, new IntPtr(_LocalConn));
			SetOutput();

			KcpDll.KcpNodelay(_Kcp, 1, 10, 1, 1);
			KcpDll.KcpWndsize(_Kcp, 32, 32);
			KcpDll.KcpSetmtu(_Kcp, 470);
#endif

			_LastRecvTime = _TimeNow;

			try
			{
#if USE_KCP_SHARP
				_SendBuffer.WriteTo(0, 4);
				_SendBuffer.WriteTo(4, KcpProtocolType.SYN);
				Send(_SendBuffer, 0, 6);
#else
				_SendBuffer.WriteTo(0, KcpProtocolType.SYN);
				_SendBuffer.WriteTo(2, _LocalConn);
				_Socket.SendTo(_SendBuffer, 0, 6, SocketFlags.None, _RemoteEndPoint);
#endif
			}
			catch (Exception e)
			{
				OnConnectException?.Invoke(e);
			}

			return UniTask.CompletedTask;
		}

		public override UniTask Listen()
		{
			return UniTask.CompletedTask;
		}

#if !USE_KCP_SHARP
		public void SetOutput()
		{
#if ENABLE_IL2CPP
			KcpDll.KcpSetoutput(_Kcp, KcpOutput);
#else
			// 跟上一行一样写法，pc跟linux会出错, 保存防止被GC
			_KcpOutput = KcpOutput;
			KcpDll.KcpSetoutput(_Kcp, _KcpOutput);
#endif
		}
#endif

		public override void Send(byte[] msg, int startIdx, int length)
		{
#if USE_KCP_SHARP
			if (_Kcp == null)
#else
			if (_Kcp == IntPtr.Zero)
#endif
				OnListenException?.Invoke(new Exception($"Call {nameof(Send)} when Kcp is disconnected"));
			else
			{
#if USE_KCP_SHARP
				_Kcp.Send(msg, startIdx, length);
#else
				KcpDll.KcpSend(_Kcp, msg, length);
#endif
			}
		}

#if USE_KCP_SHARP
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
#else
#if ENABLE_IL2CPP
		[AOT.MonoPInvokeCallback(typeof(KcpOutput))]
#endif
		protected int KcpOutput(IntPtr bytes, int len, IntPtr kcp, IntPtr user)
		{
			try
			{
				if (len == 0)
				{
					Log.LogError($"Kcp output 0", nameof(KcpSession));
					return 0;
				}

				Marshal.Copy(bytes, _SendBuffer, 0, len);
				_Socket.SendTo(_SendBuffer, 0, len, SocketFlags.None, _RemoteEndPoint);
			}
			catch (Exception e)
			{
				OnListenException(e);
			}
			return len;
		}
#endif

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
#if USE_KCP_SHARP
			_Kcp.Input(buffer, 0, receivedSize);
#else
			KcpDll.KcpInput(_Kcp, buffer, 0, receivedSize);
#endif

			while (true)
			{
#if USE_KCP_SHARP
				int n = _Kcp.PeekSize();
#else
				int n = KcpDll.KcpPeeksize(_Kcp);
#endif
				if (n < 0)
					return;

				if (n == 0)
				{
					OnListenException(new Exception());
					return;
				}

#if USE_KCP_SHARP
				int count = _Kcp.Recv(buffer, 0, ushort.MaxValue);
#else
				int count = KcpDll.KcpRecv(_Kcp, buffer, ushort.MaxValue);
#endif

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

		/// <summary>
		/// 客户端当前时间，单位ms
		/// </summary>
		protected long ClientNow()
		{
			return (DateTime.UtcNow.Ticks - epoch) / 10000;
		}

		public override void Disconnect()
		{
			if (_Socket == null)
				return;

			try
			{
#if USE_KCP_SHARP
				_SendBuffer.WriteTo(0, 4);
				_SendBuffer.WriteTo(4, KcpProtocolType.FIN);
				Send(_SendBuffer, 0, 6);

				if (_Kcp != null)
				{
					_Kcp.Release();
					_Kcp = null;
				}
#else
				_SendBuffer.WriteTo(0, KcpProtocolType.FIN);
				_SendBuffer.WriteTo(2, _LocalConn);
				_SendBuffer.WriteTo(6, _RemoteConn);
				_SendBuffer.WriteTo(10, (uint)_Error);
				_Socket.SendTo(_SendBuffer, 0, 14, SocketFlags.None, _RemoteEndPoint);

				if (_Kcp != IntPtr.Zero)
				{
					KcpDll.KcpRelease(_Kcp);
					_Kcp = IntPtr.Zero;
				}
#endif
				_Socket = null;

				IsConnected = false;
				OnDisconnected?.Invoke();
			}
			catch (Exception e)
			{
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

		public void Update(float delta)
		{
#if USE_KCP_SHARP
			if (_Kcp == null)
				return;
#else
			if (_Kcp == IntPtr.Zero)
				return;
#endif

			_TimeNow = (uint)(ClientNow() - _StartTime);

			Recv();

			try
			{
#if USE_KCP_SHARP
				_Kcp.Update(_TimeNow);
#else
				KcpDll.KcpUpdate(_Kcp, _TimeNow);
#endif
			}
			catch (Exception e)
			{
				OnListenException?.Invoke(e);
				return;
			}

			if (_Kcp != null)
			{
#if USE_KCP_SHARP
				uint nextUpdateTime = _Kcp.Check(_TimeNow);
#else
				uint nextUpdateTime = KcpDll.KcpCheck(_Kcp, _TimeNow);
#endif
			}
		}
	}
}
