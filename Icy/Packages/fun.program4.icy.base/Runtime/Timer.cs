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
		/// <summary>
		/// 延迟指定时间后，执行action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		/// <param name="delaySeconds">要延迟的时间，单位秒</param>
		/// <param name="ignorTimeScale">是否忽略TimeScale</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource DelayByTime(Action action, float delaySeconds, bool ignorTimeScale = false)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoDelayByTime(action, delaySeconds, cts, ignorTimeScale).Forget();
			return cts;
		}

		/// <summary>
		/// 延迟指定帧数后，执行action
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
		/// 延迟到下一帧，执行action
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
		/// 每隔指定的时间间隔，执行一次action
		/// </summary>
		/// <param name="action">要间隔执行的action</param>
		/// <param name="perSeconds">每几秒执行一次；下限保底为0.005秒</param>
		/// <param name="repeatCount">执行的次数；如果<=0，则次数为无限</param>
		/// <param name="ignorTimeScale">是否忽略TimeScale</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource RepeatByTime(Action action, float perSeconds, int repeatCount, bool ignorTimeScale = false)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoRepeatByTime(action, perSeconds, repeatCount, cts, ignorTimeScale).Forget();
			return cts;
		}

		/// <summary>
		/// 每隔指定的帧数，执行一次action
		/// </summary>
		/// <param name="action">要间隔执行的action</param>
		/// <param name="perFrames">每几帧执行一次，下限保底为1</param>
		/// <param name="repeatCount">执行的次数；如果<=0，则次数为无限</param>
		/// <returns>取消令牌</returns>
		public static CancellationTokenSource RepeatByFrame(Action action, int perFrames, int repeatCount)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			DoRepeatByFrame(action, perFrames, repeatCount, cts).Forget();
			return cts;
		}

		#region Implementation
		private static async UniTaskVoid DoDelayByTime(Action action, float delaySeconds, CancellationTokenSource cts, bool ignorTimeScale = false)
		{
			await UniTask.Delay(Mathf.RoundToInt(delaySeconds * 1000), ignorTimeScale, PlayerLoopTiming.Update, cts.Token);
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

		private static async UniTaskVoid DoRepeatByTime(Action action, float perSeconds, int repeatCount, CancellationTokenSource cts, bool ignorTimeScale = false)
		{
			if (perSeconds <= 0)
				perSeconds = 0.005f;

			int intervalMs = Mathf.RoundToInt(perSeconds * 1000);
			int count = 0;
			while ((repeatCount <= 0 || count < repeatCount) && !cts.IsCancellationRequested)
			{
				action?.Invoke();
				await UniTask.Delay(intervalMs, ignorTimeScale, PlayerLoopTiming.Update, cts.Token);
				count++;
			}
		}

		private static async UniTaskVoid DoRepeatByFrame(Action action, int perFrames, int repeatCount, CancellationTokenSource cts)
		{
			if (perFrames <= 0)
				perFrames = 1;

			int count = 0;
			while ((repeatCount <= 0 || count < repeatCount) && !cts.IsCancellationRequested)
			{
				action?.Invoke();
				await UniTask.DelayFrame(perFrames, PlayerLoopTiming.Update, cts.Token);
				count++;
			}
		}
		#endregion
	}
}
