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
