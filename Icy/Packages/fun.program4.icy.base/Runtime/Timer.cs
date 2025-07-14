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
using System.Threading;
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// 提供延迟执行、重复执行等常见计时器功能
	/// </summary>
	public static class Timer
	{
		private const float MIN_REPEAT_TIME_INTERVAL = 0.005f;
		private const int MIN_REPEAT_FRAME_INTERVAL = 1;

		/// <summary>
		/// 延迟指定时间后，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		/// <param name="delaySeconds">要延迟的时间，单位秒</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource DelayByTime(Action action, float delaySeconds, bool ignoreTimeScale = false)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoDelayByTime(action, delaySeconds, cts, ignoreTimeScale).Forget();
			return cts;
		}

		/// <summary>
		/// 延迟指定帧数后，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		/// <param name="frameCount">要延迟的帧数</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource DelayByFrame(Action action, int frameCount)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoDelayByFrame(action, frameCount, cts).Forget();
			return cts;
		}

		/// <summary>
		/// 延迟到下一帧，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource NextFrame(Action action)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoNextFrame(action, cts).Forget();
			return cts;
		}

		/// <summary>
		/// 每隔指定的时间间隔，执行一次 action
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perSeconds">每几秒执行一次；下限保底为0.005秒</param>
		/// <param name="repeatCount">执行的次数；如果<=0，则次数为无限</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource RepeatByTime(Action action, float perSeconds, int repeatCount, bool ignoreTimeScale = false)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoRepeatByTime(action, perSeconds, repeatCount, cts, ignoreTimeScale).Forget();
			return cts;
		}

		/// <summary>
		/// 每隔指定的时间间隔，执行一次 action
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perSeconds">每几秒执行一次；下限保底为0.005秒</param>
		/// <param name="predicate">返回 true时，repeat停止</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource RepeatByTimeUntil(Action action, float perSeconds, Func<bool> predicate, bool ignoreTimeScale = false)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoRepeatByTimeUntil(action, perSeconds, predicate, cts, ignoreTimeScale).Forget();
			return cts;
		}

		/// <summary>
		/// 每隔指定的帧数，执行一次 action
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perFrames">每几帧执行一次，下限保底为1</param>
		/// <param name="repeatCount">执行的次数；如果<=0，则次数为无限</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource RepeatByFrame(Action action, int perFrames, int repeatCount)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoRepeatByFrame(action, perFrames, repeatCount, cts).Forget();
			return cts;
		}

		/// <summary>
		/// 每隔指定的帧数，执行一次 action
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perFrames">每几帧执行一次，下限保底为1</param>
		/// <param name="predicate">返回 true时，repeat停止</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource RepeatByFrameUntil(Action action, int perFrames, Func<bool> predicate)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoRepeatByFrameUntil(action, perFrames, predicate, cts).Forget();
			return cts;
		}

		#region Implementation
		private static async UniTaskVoid DoDelayByTime(Action action, float delaySeconds, CancellationTokenSource cts, bool ignoreTimeScale = false)
		{
			await UniTask.Delay(Mathf.RoundToInt(delaySeconds * 1000), ignoreTimeScale, PlayerLoopTiming.Update, cts.Token);
			action?.Invoke();
		}

		private static async UniTaskVoid DoDelayByFrame(Action action, int frameCount, CancellationTokenSource cts)
		{
			await UniTask.DelayFrame(frameCount, PlayerLoopTiming.Update, cts.Token);
			action?.Invoke();
		}

		private static async UniTaskVoid DoNextFrame(Action action, CancellationTokenSource cts)
		{
			await UniTask.NextFrame(cts.Token);
			action?.Invoke();
		}

		private static async UniTaskVoid DoRepeatByTime(Action action, float perSeconds, int repeatCount, CancellationTokenSource cts, bool ignoreTimeScale = false)
		{
			perSeconds = ValidateRepeatTimeInterval(perSeconds);

			int intervalMs = Mathf.RoundToInt(perSeconds * 1000);
			int count = 0;
			while ((repeatCount <= 0 || count < repeatCount) && !cts.IsCancellationRequested)
			{
				action?.Invoke();
				await UniTask.Delay(intervalMs, ignoreTimeScale, PlayerLoopTiming.Update, cts.Token);
				count++;
			}
		}

		private static async UniTaskVoid DoRepeatByTimeUntil(Action action, float perSeconds, Func<bool> predicate, CancellationTokenSource cts, bool ignoreTimeScale = false)
		{
			perSeconds = ValidateRepeatTimeInterval(perSeconds);

			int intervalMs = Mathf.RoundToInt(perSeconds * 1000);
			int count = 0;
			while (!predicate() && !cts.IsCancellationRequested)
			{
				action?.Invoke();
				await UniTask.Delay(intervalMs, ignoreTimeScale, PlayerLoopTiming.Update, cts.Token);
				count++;
			}
		}

		private static async UniTaskVoid DoRepeatByFrame(Action action, int perFrames, int repeatCount, CancellationTokenSource cts)
		{
			perFrames = ValidateRepeatFrameInterval(perFrames);

			int count = 0;
			while ((repeatCount <= 0 || count < repeatCount) && !cts.IsCancellationRequested)
			{
				action?.Invoke();
				await UniTask.DelayFrame(perFrames, PlayerLoopTiming.Update, cts.Token);
				count++;
			}
		}

		private static async UniTaskVoid DoRepeatByFrameUntil(Action action, int perFrames, Func<bool> predicate, CancellationTokenSource cts)
		{
			perFrames = ValidateRepeatFrameInterval(perFrames);

			int count = 0;
			while (!predicate() && !cts.IsCancellationRequested)
			{
				action?.Invoke();
				await UniTask.DelayFrame(perFrames, PlayerLoopTiming.Update, cts.Token);
				count++;
			}
		}

		private static float ValidateRepeatTimeInterval(float interval)
		{
			if (interval < MIN_REPEAT_TIME_INTERVAL)
				return MIN_REPEAT_TIME_INTERVAL;
			else
				return interval;
		}

		private static int ValidateRepeatFrameInterval(int interval)
		{
			if (interval < MIN_REPEAT_FRAME_INTERVAL)
				return MIN_REPEAT_FRAME_INTERVAL;
			else
				return interval;
		}
		#endregion
	}
}
