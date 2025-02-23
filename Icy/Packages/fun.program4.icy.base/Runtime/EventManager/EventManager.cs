using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;


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
		/// 立刻触发一个事件，不带参数
		/// </summary>
		public void Trigger(int eventID)
		{
			Trigger(eventID, default);
		}

		/// <summary>
		/// 立刻触发一个事件，带指定的参数
		/// </summary>
		public void Trigger(int eventID, IEventParam param)
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
		public void TriggerNextFrame(int eventID)
		{
			TriggerNextFrameAsync(eventID, default).Forget();
		}

		/// <summary>
		/// 延迟到下一帧触发一个带参数的事件
		/// </summary>
		public void TriggerNextFrame(int eventID, IEventParam param)
		{
			TriggerNextFrameAsync(eventID, param).Forget();
		}
		private async UniTaskVoid TriggerNextFrameAsync(int eventID, IEventParam param)
		{
			await UniTask.NextFrame();
			Trigger(eventID, param);
		}

		/// <summary>
		/// 延迟触发一个 不 带参数的事件，单位秒
		/// </summary>
		public void TriggerDelay(int eventID, float delay)
		{
			TriggerDelay(eventID, default, delay);
		}

		/// <summary>
		/// 延迟触发一个带参数的事件，单位秒
		/// </summary>
		public void TriggerDelay(int eventID, IEventParam param, float delay)
		{
			TriggerDelayAsync(eventID, param, delay).Forget();
		}
		private async UniTaskVoid TriggerDelayAsync(int eventID, IEventParam param, float delay)
		{
			await UniTask.WaitForSeconds(delay);
			Trigger(eventID, param);
		}

		/// <summary>
		/// 清除所有的事件注册，谨慎调用！
		/// </summary>
		public void ClearAll()
		{
			_EventListenerMap.Clear();
		}
	}
}
