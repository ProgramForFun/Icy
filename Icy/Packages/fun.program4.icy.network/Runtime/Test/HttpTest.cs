#if UNITY_EDITOR
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
				httpRequester.Get("www.baidu.com", (int code, string content) =>
				{
					Log.LogInfo("GET responseCode = " + code);
					Log.LogInfo("GET content = " + content);
				});
			}

			{
				//Get重试几次后失败
				HttpRequester httpRequester = new HttpRequester();
				httpRequester.Get("program4.fun", (int code, string content) =>
				{
					Log.LogInfo("GET responseCode = " + code);
					Log.LogInfo("GET content = " + content);
				});
			}

			{
				//Post成功
				HttpRequester httpRequester = new HttpRequester();
				Dictionary<string, string> args = new Dictionary<string, string>();
				args["TestKey"] = "TestValue";
				httpRequester.Post("jsonplaceholder.typicode.com/posts", args, (int code, string content) =>
				{
					Log.LogInfo("POST responseCode = " + code);
					Log.LogInfo("POST content = " + content);
				});
			}
		}
	}
}
#endif
