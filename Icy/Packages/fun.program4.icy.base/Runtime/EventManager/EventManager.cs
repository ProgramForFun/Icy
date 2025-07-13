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
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace Icy.Base
{
	using EventListener = Action<int, IEventParam>;

	/// <summary>
	/// 事件管理器
	/// TODO：支持多线程
	/// </summary>
	public static class EventManager
	{
		/// <summary>
		/// 所有事件的映射关系
		/// </summary>
		private static Dictionary<int, HashSet<EventListener>> _EventListenerMap = new Dictionary<int, HashSet<EventListener>>();
		/// <summary>
		/// EventParam缓存
		/// </summary>
		private static Dictionary<Type, IEventParam> _EventParamCache = new Dictionary<Type, IEventParam>();


		/// <summary>
		/// 获取缓存的Param，避免每次都new一个新的，降低GC分配；
		/// 注意，业务逻辑只应该从Param中获取数据，不应该持有EventParam
		/// </summary>
		public static T GetParam<T>() where T : class, IEventParam, new()
		{
			IEventParam param;

			Type type = typeof(T);
			if (_EventParamCache.ContainsKey(type))
				param = _EventParamCache[type];
			else
			{
				param = new T();
				_EventParamCache[type] = param;
			}

			param.Reset();
			return param as T;
		}

		/// <summary>
		/// 指定listener是否已经在监听指定event了
		/// </summary>
		public static bool HasAlreadyListened(int eventID, EventListener listener)
		{
			if (!_EventListenerMap.ContainsKey(eventID))
				return false;

			foreach (var item in _EventListenerMap[eventID])
			{
				if (item == listener)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 添加一个指定事件的监听
		/// </summary>
		public static void AddListener(int eventID, EventListener listener)
		{
			if (HasAlreadyListened(eventID, listener))
			{
				MethodInfo listenerMethod = listener.Method;
				Log.LogError($"Duplicate listener register, eventID = {eventID}, listener = {listenerMethod.DeclaringType?.FullName}.{listenerMethod.Name}", nameof(EventManager));
				return;
			}

			if (!_EventListenerMap.ContainsKey(eventID))
				_EventListenerMap[eventID] = new HashSet<EventListener>();
			_EventListenerMap[eventID].Add(listener);
		}

		/// <summary>
		/// 移除一个指定事件的监听
		/// </summary>
		public static void RemoveListener(int eventID, EventListener listener)
		{
			if (_EventListenerMap.ContainsKey(eventID))
				_EventListenerMap[eventID].Remove(listener);
		}

		/// <summary>
		/// 立刻触发一个事件，不带参数
		/// </summary>
		public static void Trigger(int eventID)
		{
			Trigger(eventID, default);
		}

		/// <summary>
		/// 立刻触发一个事件，带指定的参数
		/// </summary>
		public static void Trigger(int eventID, IEventParam param)
		{
			if (_EventListenerMap.ContainsKey(eventID))
			{
				foreach (EventListener listener in _EventListenerMap[eventID])
					listener(eventID, param);
			}
		}

		/// <summary>
		/// 延迟到下一帧触发一个 不 带参数的事件
		/// </summary>
		public static void TriggerNextFrame(int eventID)
		{
			TriggerNextFrameAsync(eventID, default).Forget();
		}

		/// <summary>
		/// 延迟到下一帧触发一个带参数的事件
		/// </summary>
		public static void TriggerNextFrame(int eventID, IEventParam param)
		{
			TriggerNextFrameAsync(eventID, param).Forget();
		}
		private static async UniTaskVoid TriggerNextFrameAsync(int eventID, IEventParam param)
		{
			await UniTask.NextFrame();
			Trigger(eventID, param);
		}

		/// <summary>
		/// 延迟触发一个 不 带参数的事件，单位秒
		/// </summary>
		public static void TriggerDelay(int eventID, float delaySec)
		{
			TriggerDelay(eventID, default, delaySec);
		}

		/// <summary>
		/// 延迟触发一个带参数的事件，单位秒
		/// </summary>
		public static void TriggerDelay(int eventID, IEventParam param, float delaySec)
		{
			TriggerDelayAsync(eventID, param, delaySec).Forget();
		}
		private static async UniTaskVoid TriggerDelayAsync(int eventID, IEventParam param, float delaySec)
		{
			await UniTask.WaitForSeconds(delaySec);
			Trigger(eventID, param);
		}

		/// <summary>
		/// 清除所有的事件注册，谨慎调用！
		/// </summary>
		public static void ClearAll()
		{
			Log.LogInfo("Clear EventManager");
			_EventListenerMap.Clear();
			_EventParamCache.Clear();
		}

		/// <summary>
		/// 序列化输出当前EventManager里所有注册的监听，方便调试；
		/// 内部实现有反射，注意谨慎在性能敏感的场景使用
		/// </summary>
		public static string Dump()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var kvp in _EventListenerMap)
			{
				stringBuilder.AppendLine($"EventKey : {kvp.Key} ");
				foreach (var callback in kvp.Value)
				{
					MethodInfo methodInfo = callback.Method;
					stringBuilder.AppendLine($"{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}");
				}
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}
	}
}
