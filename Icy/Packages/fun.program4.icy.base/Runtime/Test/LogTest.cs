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

using Cysharp.Threading.Tasks;
using System.Threading;

namespace Icy.Base
{
	public static class LogTest
	{
		public static void Test()
		{
			Log.MinLogLevel = LogLevel.Warning;
			Log.Info("Test Msg", "game");
			Log.OverrideMinLogLevelForTag("game", LogLevel.Info);
			Log.Info("Test Msg", "game");

			UniTask.RunOnThreadPool(() => 
			{
				Log.OverrideMinLogLevelForTag("worker", LogLevel.Info);
				Log.Error($"Log from worker thread {Thread.CurrentThread.ManagedThreadId}", "worker");
				Log.Error($"Log from worker thread {Thread.CurrentThread.ManagedThreadId}", "worker");
				Log.Error($"Log from worker thread {Thread.CurrentThread.ManagedThreadId}", "worker");
			}).Forget();

			UniTask.RunOnThreadPool(() =>
			{
				Log.OverrideMinLogLevelForTag("worker 2", LogLevel.Info);
				Log.Warn($"Log from worker thread {Thread.CurrentThread.ManagedThreadId}", "worker 2");
				Log.Warn($"Log from worker thread {Thread.CurrentThread.ManagedThreadId}", "worker 2");
				Log.Warn($"Log from worker thread {Thread.CurrentThread.ManagedThreadId}", "worker 2");
			}).Forget();
		}
	}
}
#endif
