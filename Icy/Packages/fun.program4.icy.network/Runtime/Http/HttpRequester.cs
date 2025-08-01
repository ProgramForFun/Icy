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


using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Icy.Network
{
	/// <summary>
	/// 带有重试的Http封装
	/// </summary>
	public sealed class HttpRequester : IDisposable
	{
		/// <summary>
		/// Http请求结果
		/// </summary>
		public struct HttpResponse
		{
			/// <summary>
			/// HTTP response code
			/// </summary>
			public int Code;
			/// <summary>
			/// 成功时是返回内容，失败时是错误信息
			/// </summary>
			public string Content;
		}

		/// <summary>
		/// 当前支持的Http Method
		/// </summary>
		enum SupportMethod
		{
			GET,
			POST,
		}

		/// <summary>
		/// 当前正在执行的请求
		/// </summary>
		private HashSet<UnityWebRequest> _CurRequests;
		/// <summary>
		/// 一个请求发送失败的重试次数
		/// </summary>
		private int _RetryTimes;
		/// <summary>
		/// 单次请求的超时时间，单位秒；如果重试3次的话，总体超时时间就是timeout x 3
		/// </summary>
		private int _Timeout;


		public HttpRequester(int timeoutPerRequest = 5, int retryTimes = 3)
		{
			_Timeout = timeoutPerRequest;
			_RetryTimes = retryTimes;
			_CurRequests = new HashSet<UnityWebRequest>();
		}

		/// <summary>
		/// 发送GET请求；回调风格
		/// </summary>
		/// <param name="url">请求的url</param>
		/// <param name="callback"></param>
		public void Get(string url, Action<HttpResponse> callback)
		{
			RequestAsync(SupportMethod.GET, url, null, callback).Forget();
		}

		/// <summary>
		/// 发送GET请求；async风格
		/// </summary>
		/// <param name="url">请求的url</param>
		/// <returns></returns>
		public async UniTask<HttpResponse> GetAsync(string url)
		{
			return await RequestAsync(SupportMethod.GET, url, null, null);
		}

		/// <summary>
		/// 发送POST请求
		/// </summary>
		/// <param name="url">请求的url</param>
		/// <param name="dict">要发送的内容</param>
		/// <param name="callback"></param>
		/// <returns>如果当前正在发送其他请求，返回false；否则返回true</returns>
		public void Post(string url, Dictionary<string, string> dict, Action<HttpResponse> callback)
		{
			RequestAsync(SupportMethod.POST, url, dict, callback).Forget();
		}

		/// <summary>
		/// 发送POST请求
		/// </summary>
		/// <param name="url">请求的url</param>
		/// <param name="dict">要发送的内容</param>
		public async UniTask<HttpResponse> PostAsync(string url, Dictionary<string, string> dict)
		{
			return await RequestAsync(SupportMethod.POST, url, dict, null);
		}

		private async UniTask<HttpResponse> RequestAsync(SupportMethod method, string url, Dictionary<string, string> dict, Action<HttpResponse> callback)
		{
			UnityWebRequest request = null;
			int retry = 0;
			int lastResponseCode;
			string lastError;
			do
			{
				try
				{
					if (method == SupportMethod.GET)
						request = UnityWebRequest.Get(url);
					else if (method == SupportMethod.POST)
						request = UnityWebRequest.Post(url, dict);
					else
					{
						string error = $"{nameof(HttpRequester)} does NOT support method {method} yet";
						Log.LogError(error, nameof(HttpRequester));
						return new HttpResponse() { Code = -1, Content = error };
					}
					request.timeout = _Timeout;
					_CurRequests.Add(request);
					await request.SendWebRequest();

					lastResponseCode = (int)request.responseCode;
					if (request.result != UnityWebRequest.Result.Success)
					{
						lastError = request.error;
						Log.LogWarning($"{nameof(HttpRequester)} failed, url = {url}, result = {request.result}" +
										$", responseCode = {lastResponseCode}, error = {lastError}", nameof(HttpRequester));
					}
					else
					{
						string content = DownloadHandlerBuffer.GetContent(request);

						_CurRequests.Remove(request);
						request.Dispose();
						request = null;
						HttpResponse rtnSucceed = new HttpResponse() { Code = lastResponseCode, Content = content };
						callback?.Invoke(rtnSucceed);

						return rtnSucceed;
					}
				}
				catch (Exception ex)
				{
					lastResponseCode = -1;
					lastError = ex.Message;
				}

				retry++;
				Log.LogInfo($"{nameof(HttpRequester)} {method} retry {retry}", nameof(HttpRequester));
			} while (retry < _RetryTimes);

			Log.LogError($"{nameof(HttpRequester)} failed, url = {url}, result = {request.result}" +
							$", responseCode = {lastResponseCode}, error = {lastError}", nameof(HttpRequester));

			_CurRequests.Remove(request);
			request.Dispose();
			request = null;
			HttpResponse rtnFailed = new HttpResponse() { Code = lastResponseCode, Content = lastError };
			callback?.Invoke(rtnFailed);
			return rtnFailed;
		}

		/// <summary>
		/// 设置重试次数
		/// </summary>
		/// <param name="count"></param>
		public void SetRetryTimes(int count)
		{
			_RetryTimes = count;
		}

		/// <summary>
		/// 设置单次请求的超时时间，单位秒；如果重试3次的话，总体超时时间就是timeout x 3
		/// </summary>
		/// <param name="timeout"></param>
		public void SetTimeout(int timeout)
		{
			_Timeout = timeout;
		}

		/// <summary>
		/// 获取当前有多少个请求正在执行
		/// </summary>
		public int GetCurrentRequestCount()
		{
			return _CurRequests.Count;
		}

		public void Dispose()
		{
			foreach (var item in _CurRequests)
				item?.Dispose();
		}
	}
}
