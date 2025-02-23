#if UNITY_EDITOR
using Icy.Base;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Icy.Network
{
	public static class TcpTest
	{
		static TcpSession _TcpSession;

		public static void Test()
		{
			_TcpSession = new TcpSession("127.0.0.1", 12321);
			_TcpSession.OnConnected += OnConnect;
			_TcpSession.OnDisconnected += OnDisconnect;
			_TcpSession.OnReceive += OnReceiveData;
			_TcpSession.OnConnectException += OnConnectException;
			_TcpSession.OnHandleReceivedException += OnHandleReceivedException;
		}

		private static void OnConnect()
		{
			Log.LogInfo("Tcp Connected");
		}

		private static void OnDisconnect()
		{
			Log.LogInfo("Tcp Disconnected");
		}

		private static void OnReceiveData(byte[] buffer, int start, int count)
		{
			string msg = Encoding.UTF8.GetString(buffer, start, count);
			Log.LogInfo($"HandleReceived, len = {count}, msg = {msg}");
		}

		private static void OnConnectException(Exception ex)
		{
			Log.LogError($"Tcp connect excetion {ex}");
		}

		private static void OnHandleReceivedException(Exception ex)
		{
			Log.LogError($"Tcp handle received excetion {ex}");
		}

		public static async void Update()
		{
			if (Input.GetKeyUp(KeyCode.C))
				await _TcpSession.Connect();

			if (Input.GetKeyUp(KeyCode.L))
				await _TcpSession.Listen();

			if (Input.GetKey(KeyCode.S))
			{
				for (int i = 0; i < 2; i++)
					_TcpSession.Send("abcdefghijklmnopqrstuvwxyz");
			}

			if (Input.GetKeyUp(KeyCode.D))
				_TcpSession.Close();
		}
	}
}
#endif
