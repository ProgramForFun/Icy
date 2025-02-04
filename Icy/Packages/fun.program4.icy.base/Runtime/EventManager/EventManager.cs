using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;


namespace Icy.Base
{
	using EventListener = Action<int, IEventParam>;

	/// <summary>
	/// �¼�������
	/// TODO��֧�ֶ��߳�
	/// </summary>
	public sealed class EventManager : Singleton<EventManager>
	{
		/// <summary>
		/// �����¼���ӳ���ϵ
		/// </summary>
		private Dictionary<int, HashSet<EventListener>> _EventListenerMap = new Dictionary<int, HashSet<EventListener>>();


		/// <summary>
		/// ���һ��ָ���¼��ļ���
		/// </summary>
		public void AddListener(int eventID, EventListener listener)
		{
			if (!_EventListenerMap.ContainsKey(eventID))
				_EventListenerMap[eventID] = new HashSet<EventListener>();
			_EventListenerMap[eventID].Add(listener);
		}

		/// <summary>
		/// �Ƴ�һ��ָ���¼��ļ���
		/// </summary>
		public void RemoveListener(int eventID, EventListener listener)
		{
			if (_EventListenerMap.ContainsKey(eventID))
				_EventListenerMap[eventID].Remove(listener);
		}

		/// <summary>
		/// ���̴���һ���¼���û�в���
		/// </summary>
		public void FireEvent(int eventID)
		{
			FireEvent(eventID, default);
		}

		/// <summary>
		/// ���̴���һ���¼�����ָ���Ĳ���
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
		/// �ӳٵ���һ֡����һ�� �� ���������¼�
		/// </summary>
		public void FireEventNextFrame(int eventID)
		{
			FireEventNextFrameAsync(eventID, default).Forget();
		}

		/// <summary>
		/// �ӳٵ���һ֡����һ�����������¼�
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
		/// �ӳٴ���һ�� �� ���������¼�����λ��
		/// </summary>
		public void FireEventDelay(int eventID, float delay)
		{
			FireEventDelay(eventID, default, delay);
		}

		/// <summary>
		/// �ӳٴ���һ�����������¼�����λ��
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