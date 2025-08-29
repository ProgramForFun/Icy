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
using UnityEngine;

namespace Icy.Network
{
	public static class HttpTest
	{
		private static HttpRequester _HttpRequester;

		public static void Test()
		{
			_HttpRequester = new HttpRequester("application/json");

			//Get成功
			_HttpRequester.Get("https://www.baidu.com", (HttpRequester.HttpResponse response) =>
			{
				Log.LogInfo("GET responseCode = " + response.Code);
				Log.LogInfo("GET content = " + response.Content);
			});

			//Get重试几次后失败
			_HttpRequester.Get("https://program4.fun", (HttpRequester.HttpResponse response) =>
			{
				Log.LogInfo("GET responseCode = " + response.Code);
				Log.LogInfo("GET content = " + response.Content);
			});

			//Post成功
			string json = @"{""TestKey1"":""TestValue2""}";
			_HttpRequester.Post("https://jsonplaceholder.typicode.com/posts", json, (HttpRequester.HttpResponse response) =>
			{
				Log.LogInfo("POST responseCode = " + response.Code);
				Log.LogInfo("POST content = " + response.Content);
			});

			TestAsync().Forget();
		}

		private static async UniTask TestAsync()
		{
			//async风格Get
			HttpRequester.HttpResponse response = await _HttpRequester.GetAsync("https://www.baidu.com");
			Log.LogInfo("async GET responseCode = " + response.Code);
			Log.LogInfo("async GET content = " + response.Content);
		}

		public static void Update()
		{
			if (Input.GetKeyUp(KeyCode.D))
				_HttpRequester.Dispose();
		}
	}
}
#endif
