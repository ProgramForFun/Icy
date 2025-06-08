#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Icy.Base;
using System.Collections.Generic;

namespace Icy.Network
{
	public static class HttpTest
	{
		public static void Test()
		{
			{
				//Get成功
				HttpRequester httpRequester = new HttpRequester();
				httpRequester.Get("www.baidu.com", (HttpRequester.HttpResponse response) =>
				{
					Log.LogInfo("GET responseCode = " + response.Code);
					Log.LogInfo("GET content = " + response.Content);
				});
			}

			{
				//Get重试几次后失败
				HttpRequester httpRequester = new HttpRequester();
				httpRequester.Get("program4.fun", (HttpRequester.HttpResponse response) =>
				{
					Log.LogInfo("GET responseCode = " + response.Code);
					Log.LogInfo("GET content = " + response.Content);
				});
			}

			{
				//Post成功
				HttpRequester httpRequester = new HttpRequester();
				Dictionary<string, string> args = new Dictionary<string, string>();
				args["TestKey"] = "TestValue";
				httpRequester.Post("jsonplaceholder.typicode.com/posts", args, (HttpRequester.HttpResponse response) =>
				{
					Log.LogInfo("POST responseCode = " + response.Code);
					Log.LogInfo("POST content = " + response.Content);
				});
			}

			TestAsync().Forget();
		}

		private static async UniTask TestAsync()
		{
			//async风格Get
			HttpRequester httpRequester = new HttpRequester();
			HttpRequester.HttpResponse response = await httpRequester.Get("www.baidu.com");
			Log.LogInfo("async GET responseCode = " + response.Code);
			Log.LogInfo("async GET content = " + response.Content);
		}
	}
}
#endif
