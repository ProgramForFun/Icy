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
		/// 用于接收的EndPoint
		/// </summary>
		protected EndPoint _IpEndPoint = new IPEndPoint(IPAddress.Any, 0);
		/// <summary>
		/// 异步Send的参数
		/// </summary>
		protected SocketAsyncEventArgs _AsyncSendArg;
		/// <summary>
		/// 异步Receive的参数
		/// </summary>
		protected SocketAsyncEventArgs _AsyncReceiveArg;
		/// <summary>
		/// 是否正在断开连接
		/// </summary>
		protected bool _IsDisconnecting = false;

#if !USE_KCP_SHARP && !ENABLE_IL2CPP
		protected KcpOutput _KcpOutput;
#endif

		protected uint _LocalConn;
		protected uint _RemoteConn;


		public KcpSession(string host, int port, int bufferSize) : base(host, port, bufferSize)
		{
			_StartTime = ClientNow();
			_TimeNow = (uint)(ClientNow() - _StartTime);
			_RemoteEndPoint = new IPEndPoint(IPAddress.Parse(host), port);

			IcyFrame.Instance.AddUpdate(this);
		}

		public override async UniTask Connect(byte[] syn = null)
		{
			if (IsConnected)
				return;

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

			try
			{
				Buffer.BlockCopy(syn, 0, _SendBuffer, 0, syn.Length);
				Send(_SendBuffer, 0, syn.Length);
			}
			catch (Exception e)
			{
				Log.LogError($"Connect exception : {e}", nameof(KcpSession));
				OnError?.Invoke(NetworkError.ConnectFailed, e);
			}
			await UniTask.CompletedTask;
		}

		public override async UniTask Listen()
		{
			_AsyncSendArg = new SocketAsyncEventArgs();
			_AsyncSendArg.SetBuffer(_SendBuffer, 0, _ReceiveBuffer.Length);
			_AsyncSendArg.RemoteEndPoint = _RemoteEndPoint;

			_AsyncReceiveArg = new SocketAsyncEventArgs();
			_AsyncReceiveArg.SetBuffer(_ReceiveBuffer, 0, _ReceiveBuffer.Length);
			_AsyncReceiveArg.RemoteEndPoint = _IpEndPoint;
			_AsyncReceiveArg.Completed += OnReceived;
			Recv();

			await UniTask.CompletedTask;
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
			if (!IsKcpValid())
			{
				Exception e = new Exception($"Call {nameof(Send)} when Kcp is disconnected");
				Log.LogError(e.ToString(), nameof(KcpSession));
				OnError?.Invoke(NetworkError.SendWhenDisconnected, e);
				return;
			}

#if USE_KCP_SHARP
			_Kcp.Send(msg, startIdx, length);
#else
			KcpDll.KcpSend(_Kcp, msg, length);
#endif
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
				_Socket.SendToAsync(new ArraySegment<byte>(_SendBuffer, 0, len), SocketFlags.None, _RemoteEndPoint);
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
				_AsyncSendArg.SetBuffer(0, len);
				_Socket.SendToAsync(_AsyncSendArg);
			}
			catch (Exception e)
			{
				Log.LogError($"Send failed, {e}", nameof(KcpSession));
				OnError?.Invoke(NetworkError.SendFailed, e);
			}
			return len;
		}
#endif

		protected virtual void Recv()
		{
			try
			{
				bool pending = _Socket.ReceiveFromAsync(_AsyncReceiveArg);
				if (!pending)
					OnReceived(_Socket, _AsyncReceiveArg);
			}
			catch (Exception e)
			{
				Log.LogError($"Receive failed, {e}", nameof(KcpSession));
				OnError?.Invoke(NetworkError.ReceiveFailed, e);
			}
		}

		protected virtual void OnReceived(object sender, SocketAsyncEventArgs e)
		{
			if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)
			{
				if (!IsKcpValid())
					return;

				HandleReceived(e.Buffer, e.Offset, e.BytesTransferred);
				Recv();
			}
		}

		protected override void HandleReceived(byte[] buffer, int startIdx, int receivedSize)
		{
#if USE_KCP_SHARP
			_Kcp.Input(buffer, startIdx, receivedSize);
#else
			KcpDll.KcpInput(_Kcp, buffer, startIdx, receivedSize);
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
					return;

#if USE_KCP_SHARP
				int count = _Kcp.Recv(buffer, 0, ushort.MaxValue);
#else
				int count = KcpDll.KcpRecv(_Kcp, buffer, ushort.MaxValue);
#endif

				if (n != count)
					return;

				if (count <= 0)
					return;

				if (_IsDisconnecting)
					_IsDisconnecting = false;
				else
				{
					if (!IsConnected)
					{
						IsConnected = true;
						OnConnected?.Invoke();
					}
					else
						OnReceive?.Invoke(buffer, 0, n);
				}
			}
		}

		/// <summary>
		/// 客户端当前时间，单位ms
		/// </summary>
		protected long ClientNow()
		{
			return (DateTime.UtcNow.Ticks - epoch) / 10000;
		}

		public override async UniTask Disconnect(byte[] fin = null)
		{
			if (_Socket == null)
				return;

			try
			{
				_IsDisconnecting = true;
				Buffer.BlockCopy(fin, 0, _SendBuffer, 0, fin.Length);
				Send(_SendBuffer, 0, fin.Length);

				await UniTask.WaitUntil(IsDisconnectOperationFinished);
#if USE_KCP_SHARP
				if (_Kcp != null)
				{
					_Kcp.Release();
					_Kcp = null;
				}
#else
				if (_Kcp != IntPtr.Zero)
				{
					KcpDll.KcpRelease(_Kcp);
					_Kcp = IntPtr.Zero;
				}
#endif
				_AsyncReceiveArg = null;

				_Socket.Close();
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
						Disconnect().Forget();
				}
			}
			catch (Exception)
			{
				// ignored
			}

			Log.LogInfo("Dispose", nameof(KcpSession));
		}

		protected bool IsDisconnectOperationFinished()
		{
			return !_IsDisconnecting;
		}

		protected bool IsKcpValid()
		{
#if USE_KCP_SHARP
			if (_Kcp == null)
				return false;
#else
			if (_Kcp == IntPtr.Zero)
				return false;
#endif
			return true;
		}

		public void Update(float delta)
		{
			if (!IsKcpValid())
				return;

			_TimeNow = (uint)(ClientNow() - _StartTime);

#if USE_KCP_SHARP
			_Kcp.Update(_TimeNow);
			uint nextUpdateTime = _Kcp.Check(_TimeNow);
#else
			KcpDll.KcpUpdate(_Kcp, _TimeNow);
			uint nextUpdateTime = KcpDll.KcpCheck(_Kcp, _TimeNow);
#endif
		}
	}
}
