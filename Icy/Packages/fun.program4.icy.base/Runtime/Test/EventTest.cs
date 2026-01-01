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
using System.Threading.Tasks;
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
			Log.Info("EventCallback is added = " + EventManager.HasAlreadyListened(0, EventCallback));

			//不应该输出
			EventManager.RemoveListener(0, EventCallback);
			EventManager.Trigger(0, eventParam);
			Log.Info("EventCallback is added = " + EventManager.HasAlreadyListened(0, EventCallback));

			//应该延迟1秒输出
			EventManager.AddListener(0, EventCallback);
			eventParam.Value = "This is a delay msg";
			float delay = 1;
			Log.Info($"FireEventDelay, delay = {delay}");
			EventManager.TriggerDelay(0, eventParam, delay);

			//应该下一帧输出
			EventManager.AddListener(1, FrameCountEventCallback);
			Log.Info($"FireEventNextFrame, frameCount = {Time.frameCount}");
			EventManager.TriggerNextFrame(1, eventParam);

			//editor下支持重复注册的检查
			EventManager.AddListener(1, FrameCountEventCallback);

			//Worker线程触发事件，主线程执行
			UniTask.RunOnThreadPool(async () =>
			{
				EventManager.AddListener(2, WorkerThreadEventCallback);
				for (int i = 0; i < 10; i++)
				{
					//Log.Info("Current trigger event thread ID = " + Thread.CurrentThread.ManagedThreadId);
					Log.Info("Trigger event = " + i);
					EventParam_Int eventParam = EventManager.GetParam<EventParam_Int>();
					eventParam.Value = i;
					EventManager.Trigger(2, eventParam);
					await Task.Delay(500);
				}
			}).Forget();

			UniTask.RunOnThreadPool(async () =>
			{
				for (int i = 100; i < 110; i++)
				{
					//Log.Warn("Current trigger event thread ID = " + Thread.CurrentThread.ManagedThreadId);
					Log.Warn("Trigger event = " + i);
					EventParam_Int eventParam = EventManager.GetParam<EventParam_Int>();
					eventParam.Value = i;
					EventManager.Trigger(2, eventParam);
					await Task.Delay(500);
				}
			}).Forget();

			//输出当前EventManager里所有注册的监听，方便调试
			Log.Info(EventManager.Dump());
		}

		static void EventCallback(int eventID, IEventParam param)
		{
			if (param is EventParam_String param_String)
				Log.Info(param_String.Value, "game");
		}

		static void FrameCountEventCallback(int eventID, IEventParam param)
		{
			Log.Info($"This is a NextFrame msg, frameCount = {Time.frameCount}", "game");
		}

		static void WorkerThreadEventCallback(int eventID, IEventParam param)
		{
			if (param is EventParam_Int param_Int)
				Log.Info($"This is a msg from worker thread, value = {param_Int.Value}", "game");
		}
	}
}
#endif
