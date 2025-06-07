#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Text;
using UnityEngine;

namespace Icy.Network
{
	public static class KcpSessionTest
	{
		static KcpSession _KcpSession;

		public static void Test()
		{
			_KcpSession = new KcpSession("127.0.0.1", 12333, 4096);
			_KcpSession.OnConnected += OnConnect;
			_KcpSession.OnDisconnected += OnDisconnect;
			_KcpSession.OnReceive += OnReceiveData;
			_KcpSession.OnConnectException += OnConnectException;
			_KcpSession.OnListenException += OnListenException;
		}

		private static void OnConnect()
		{
			Log.LogInfo("Kcp Connected");
		}

		private static void OnDisconnect()
		{
			Log.LogInfo("Kcp Disconnected");
		}

		private static void OnReceiveData(byte[] buffer, int start, int length)
		{
			string msg = Encoding.UTF8.GetString(buffer, start, length);
			Log.LogInfo($"HandleReceived, len = {length}, msg = {msg}");
		}

		private static void OnConnectException(Exception ex)
		{
			Log.LogError($"Kcp connect excetion {ex}");
		}

		private static void OnListenException(Exception ex)
		{
			Log.LogError($"Kcp handle received excetion {ex}");
		}

		public static async void Update()
		{
			if (Input.GetKeyUp(KeyCode.C))
			{
				byte[] syn = new byte[3] { 1, 2, 3 };
				await _KcpSession.Connect(syn);
			}

			if (Input.GetKeyUp(KeyCode.L))
				await _KcpSession.Listen();

			if (Input.GetKey(KeyCode.S))
			{
				for (int i = 0; i < 2; i++)
				{
					byte[] toSend = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz");
					_KcpSession.Send(toSend, 0, toSend.Length);
				}
			}

			if (Input.GetKeyUp(KeyCode.D))
			{
				byte[] fin = new byte[3] { 1, 2, 3 };
				_KcpSession.Disconnect(fin).Forget();
			}
		}
	}
}
#endif
