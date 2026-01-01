/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
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
	public static class KcpSessionTest
	{
		static KcpSession _KcpSession;

		public static void Test()
		{
			_KcpSession = new KcpSession("127.0.0.1", 12333, 4096);
			_KcpSession.OnConnected += OnConnect;
			_KcpSession.OnDisconnected += OnDisconnect;
			_KcpSession.OnReceive += OnReceiveData;
			_KcpSession.OnError += OnError;
		}

		private static void OnConnect()
		{
			Log.Info("Kcp Connected");
		}

		private static void OnDisconnect()
		{
			Log.Info("Kcp Disconnected");
		}

		private static void OnReceiveData(byte[] buffer, int start, int length)
		{
			string msg = Encoding.UTF8.GetString(buffer, start, length);
			Log.Info($"HandleReceived, len = {length}, msg = {msg}");
		}

		private static void OnError(NetworkError error, Exception ex)
		{
			Log.Error($"Kcp error = {error}, exception = {ex}");
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
					await _KcpSession.Send(toSend, 0, toSend.Length);
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
