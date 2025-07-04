using System;
using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 负责把 非 主线程的逻辑，dispatch到主线程来执行
	/// </summary>
	public sealed class MainThreadDispatcher : Singleton<MainThreadDispatcher>, IUpdateable
	{
		private Queue<Action> _ExecuteQueue;

		protected override void OnInitialized()
		{
			base.OnInitialized();
			_ExecuteQueue = new Queue<Action>();
			IcyFrame.Instance.AddUpdate(this);
		}

		/// <summary>
		/// Dispatch一个Action到主线程
		/// </summary>
		public void Enqueue(Action action)
		{
			//IcyFrame是框架入口，应该是最先初始化的，单例实例化中的Unity API跑不到，所以是安全的
			if (IcyFrame.Instance.IsMainThread())
				Log.LogWarning("Call MainThreadDispatcher from main thread is unnecessary", nameof(MainThreadDispatcher));

			lock (_ExecuteQueue)
			{
				_ExecuteQueue.Enqueue(action);
			}
		}

		public void Update(float delta)
		{
			lock (_ExecuteQueue)
			{
				while (_ExecuteQueue.Count > 0)
					_ExecuteQueue.Dequeue()?.Invoke();
			}
		}

		public override void ClearSingleton()
		{
			base.ClearSingleton();
			_ExecuteQueue.Clear();
			IcyFrame.Instance.RemoveUpdate(this);
		}
	}
}
