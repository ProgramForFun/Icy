using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;


namespace Icy.Base
{
	using EventListener = Action<int, IEventParam>;

	/// <summary>
	/// 事件管理器
	/// TODO：支持多线程
	/// </summary>
	public sealed class EventManager : Singleton<EventManager>
	{
		/// <summary>
		/// 所有事件的映射关系
		/// </summary>
		private Dictionary<int, HashSet<EventListener>> _EventListenerMap = new Dictionary<int, HashSet<EventListener>>();


		/// <summary>
		/// 添加一个指定事件的监听
		/// </summary>
		public void AddListener(int eventID, EventListener listener)
		{
			if (!_EventListenerMap.ContainsKey(eventID))
				_EventListenerMap[eventID] = new HashSet<EventListener>();
			_EventListenerMap[eventID].Add(listener);
		}

		/// <summary>
		/// 移除一个指定事件的监听
		/// </summary>
		public void RemoveListener(int eventID, EventListener listener)
		{
			if (_EventListenerMap.ContainsKey(eventID))
				_EventListenerMap[eventID].Remove(listener);
		}

		/// <summary>
		/// 立刻触发一个事件，没有参数
		/// </summary>
		public void FireEvent(int eventID)
		{
			FireEvent(eventID, default);
		}

		/// <summary>
		/// 立刻触发一个事件，带指定的参数
		/// </summary>
		public void FireEvent(int eventID, IEventParam param)
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
		public void FireEventNextFrame(int eventID)
		{
			FireEventNextFrameAsync(eventID, default).Forget();
		}

		/// <summary>
		/// 延迟到下一帧触发一个带参数的事件
		/// </summary>
		public void FireEventNextFrame(int eventID, IEventParam param)
		{
			FireEventNextFrameAsync(eventID, param).Forget();
		}
		private async UniTaskVoid FireEventNextFrameAsync(int eventID, IEventParam param)
		{
			await UniTask.NextFrame();
			FireEvent(eventID, param);
		}

		/// <summary>
		/// 延迟触发一个 不 带参数的事件，单位秒
		/// </summary>
		public void FireEventDelay(int eventID, float delay)
		{
			FireEventDelay(eventID, default, delay);
		}

		/// <summary>
		/// 延迟触发一个带参数的事件，单位秒
		/// </summary>
		public void FireEventDelay(int eventID, IEventParam param, float delay)
		{
			FireEventDelayAsync(eventID, param, delay).Forget();
		}
		private async UniTaskVoid FireEventDelayAsync(int eventID, IEventParam param, float delay)
		{
			await UniTask.WaitForSeconds(delay);
			FireEvent(eventID, param);
		}
	}
}