/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Text;
using UnityEngine;

namespace Icy.Network
{
	public static class TcpSessionTest
	{
		static TcpSession _TcpSession;

		public static void Test()
		{
			_TcpSession = new TcpSession("127.0.0.1", 12321, 4096);
			_TcpSession.OnConnected += OnConnect;
			_TcpSession.OnDisconnected += OnDisconnect;
			_TcpSession.OnReceive += OnReceiveData;
			_TcpSession.OnError += OnError;
		}

		private static void OnConnect()
		{
			Log.LogInfo("Tcp Connected");
		}

		private static void OnDisconnect()
		{
			Log.LogInfo("Tcp Disconnected");
		}

		private static void OnReceiveData(byte[] buffer, int start, int length)
		{
			string msg = Encoding.UTF8.GetString(buffer, start, length);
			Log.LogInfo($"HandleReceived, len = {length}, msg = {msg}");
		}

		private static void OnError(NetworkError error, Exception ex)
		{
			Log.LogError($"Tcp error = {error}, exception = {ex}");
		}

		public static async void Update()
		{
			if (Input.GetKeyUp(KeyCode.C))
				await _TcpSession.Connect();

			if (Input.GetKey(KeyCode.S))
			{
				for (int i = 0; i < 2; i++)
				{
					byte[] toSend = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz");
					_TcpSession.Send(toSend, 0, toSend.Length);
				}
			}

			if (Input.GetKeyUp(KeyCode.D))
				_TcpSession.Disconnect().Forget();
		}
	}
}
#endif
