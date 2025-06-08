using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

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
	private UnityWebRequest _CurRequest;
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
	}

	/// <summary>
	/// 发送GET请求；回调风格
	/// </summary>
	/// <param name="url">请求的url</param>
	/// <param name="callback"></param>
	/// <returns>如果当前正在发送其他请求，返回false；否则返回true</returns>
	public bool Get(string url, Action<HttpResponse> callback)
	{
		if (_CurRequest != null)
			return false;

		RequestAsync(SupportMethod.GET, url, null, callback).Forget();
		return true;
	}

	/// <summary>
	/// 发送GET请求；async风格
	/// </summary>
	/// <param name="url">请求的url</param>
	/// <returns></returns>
	public async UniTask<HttpResponse> Get(string url)
	{
		if (_CurRequest != null)
			return new HttpResponse();

		return await RequestAsync(SupportMethod.GET, url, null, null);
	}

	/// <summary>
	/// 发送POST请求
	/// </summary>
	/// <param name="url">请求的url</param>
	/// <param name="dict">要发送的内容</param>
	/// <param name="callback"></param>
	/// <returns>如果当前正在发送其他请求，返回false；否则返回true</returns>
	public bool Post(string url, Dictionary<string, string> dict, Action<HttpResponse> callback)
	{
		if (_CurRequest != null)
			return false;

		RequestAsync(SupportMethod.POST, url, dict, callback).Forget();
		return true;
	}

	/// <summary>
	/// 发送POST请求
	/// </summary>
	/// <param name="url">请求的url</param>
	/// <param name="dict">要发送的内容</param>
	public async UniTask<HttpResponse> Post(string url, Dictionary<string, string> dict)
	{
		if (_CurRequest != null)
			return new HttpResponse();

		return await	RequestAsync(SupportMethod.POST, url, dict, null);
	}

	private async UniTask<HttpResponse> RequestAsync(SupportMethod method, string url, Dictionary<string, string> dict, Action<HttpResponse> callback)
	{
		int retry = 0;
		int lastResponseCode;
		string lastError;
		do
		{
			try
			{
				if (method == SupportMethod.GET)
					_CurRequest = UnityWebRequest.Get(url);
				else if (method == SupportMethod.POST)
					_CurRequest = UnityWebRequest.Post(url, dict);
				else
					Log.LogError($"{nameof(HttpRequester)} does NOT support method {method} yet");
				_CurRequest.timeout = _Timeout;
				await _CurRequest.SendWebRequest();

				lastResponseCode = (int)_CurRequest.responseCode;
				if (_CurRequest.result != UnityWebRequest.Result.Success)
				{
					lastError = _CurRequest.error;
					Log.LogWarning($"{nameof(HttpRequester)} failed, url = {url}, result = {_CurRequest.result}" +
									$", responseCode = {lastResponseCode}, error = {lastError}");
				}
				else
				{
					string content = DownloadHandlerBuffer.GetContent(_CurRequest);

					_CurRequest.Dispose();
					_CurRequest = null;
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
			Log.LogInfo($"{nameof(HttpRequester)} {method} retry {retry}");
		} while (retry < _RetryTimes);

		Log.LogError($"{nameof(HttpRequester)} failed, url = {url}, result = {_CurRequest.result}" +
						$", responseCode = {lastResponseCode}, error = {lastError}");

		_CurRequest.Dispose();
		_CurRequest = null;
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

	public void Dispose()
	{
		_CurRequest?.Dispose();
	}
}
