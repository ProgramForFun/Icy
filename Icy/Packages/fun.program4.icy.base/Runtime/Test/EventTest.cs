#if UNITY_EDITOR
using UnityEngine;

namespace Icy.Base
{
	public static class EventTest
	{
		public static void Test()
		{
			//应该输出
			EventManager.Instance.AddListener(0, eventCallback);
			EventParam_String param_String = new EventParam_String();
			param_String.Value = "100";
			EventManager.Instance.FireEvent(0, param_String);

			//不应该输出
			EventManager.Instance.RemoveListener(0, eventCallback);
			EventManager.Instance.FireEvent(0, param_String);

			//应该延迟1秒输出
			EventManager.Instance.AddListener(0, eventCallback);
			param_String.Value = "This is a delay msg";
			float delay = 1;
			Log.LogInfo($"FireEventDelay, delay = {delay}");
			EventManager.Instance.FireEventDelay(0, param_String, delay);

			//应该下一帧输出
			EventManager.Instance.AddListener(1, frameCountEventCallback);
			Log.LogInfo($"FireEventNextFrame, frameCount = {Time.frameCount}");
			EventManager.Instance.FireEventNextFrame(1, param_String);
		}

		static void eventCallback(int eventID, IEventParam param)
		{
			if (param is EventParam_String param_Int)
				Log.LogInfo(param_Int.Value.ToString(), "game");
		}

		static void frameCountEventCallback(int eventID, IEventParam param)
		{
			Log.LogInfo($"This is a NextFrame msg, frameCount = {Time.frameCount}", "game");
		}
	}
}
#endif
