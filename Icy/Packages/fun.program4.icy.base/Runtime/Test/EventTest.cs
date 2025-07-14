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
using UnityEngine;

namespace Icy.Base
{
	public static class EventTest
	{
		public static void Test()
		{
			//应该输出
			EventManager.AddListener(0, EventCallback);
			EventParam_String eventParam = EventManager.GetParam<EventParam_String>();
			eventParam.Value = "100";
			EventManager.Trigger(0, eventParam);
			Log.LogInfo("EventCallback is added = " + EventManager.HasAlreadyListened(0, EventCallback));

			//不应该输出
			EventManager.RemoveListener(0, EventCallback);
			EventManager.Trigger(0, eventParam);
			Log.LogInfo("EventCallback is added = " + EventManager.HasAlreadyListened(0, EventCallback));

			//应该延迟1秒输出
			EventManager.AddListener(0, EventCallback);
			eventParam.Value = "This is a delay msg";
			float delay = 1;
			Log.LogInfo($"FireEventDelay, delay = {delay}");
			EventManager.TriggerDelay(0, eventParam, delay);

			//应该下一帧输出
			EventManager.AddListener(1, FrameCountEventCallback);
			Log.LogInfo($"FireEventNextFrame, frameCount = {Time.frameCount}");
			EventManager.TriggerNextFrame(1, eventParam);

			//editor下支持重复注册的检查
			EventManager.AddListener(1, FrameCountEventCallback);

			//输出当前EventManager里所有注册的监听，方便调试
			Log.LogInfo(EventManager.Dump());
		}

		static void EventCallback(int eventID, IEventParam param)
		{
			if (param is EventParam_String param_Int)
				Log.LogInfo(param_Int.Value.ToString(), "game");
		}

		static void FrameCountEventCallback(int eventID, IEventParam param)
		{
			Log.LogInfo($"This is a NextFrame msg, frameCount = {Time.frameCount}", "game");
		}
	}
}
#endif
