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
using System.Threading;
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// 包装常用UniTask包含CancellationToken参数的API，将其中的CancellationToken参数设置为
	/// MonoBehaviour.destroyCancellationToken，以达到GameObject销毁时，UniTask相关的操作可以自动中断；
	/// </summary>
	public class UniTaskMonoBehaviour : MonoBehaviour
	{
		/// <summary>
		/// 此UniTaskMonoBehaviour创建的所有CancelToken
		/// </summary>
		protected List<CancellationTokenSource> _AllCancelTokens = new List<CancellationTokenSource>();


		#region 基础等待方法

		/// <summary>
		/// 延迟等待（毫秒）
		/// </summary>
		protected UniTask Delay(int millisecondsDelay, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, bool cancelImmediately = false)
		{
			return UniTask.Delay(millisecondsDelay, ignoreTimeScale, delayTiming, destroyCancellationToken);
		}

		/// <summary>
		/// 延迟等待（TimeSpan）
		/// </summary>
		protected UniTask Delay(TimeSpan timeSpan, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, bool cancelImmediately = false)
		{
			return UniTask.Delay(timeSpan, ignoreTimeScale, delayTiming, destroyCancellationToken);
		}

		/// <summary>
		/// 延迟等待指定帧数
		/// </summary>
		protected UniTask DelayFrame(int delayFrameCount, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, bool cancelImmediately = false)
		{
			return UniTask.DelayFrame(delayFrameCount, delayTiming, destroyCancellationToken, cancelImmediately);
		}

		/// <summary>
		/// 等待指定秒数
		/// </summary>
		protected UniTask WaitForSeconds(float duration, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, bool cancelImmediately = false)
		{
			return UniTask.WaitForSeconds(duration, ignoreTimeScale, delayTiming, destroyCancellationToken, cancelImmediately);
		}

		/// <summary>
		/// 等待到下一帧
		/// </summary>
		protected UniTask NextFrame(PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return UniTask.NextFrame(timing, destroyCancellationToken);
		}

		/// <summary>
		/// 等待FixedUpdate
		/// </summary>
		protected UniTask WaitForFixedUpdate()
		{
			return UniTask.WaitForFixedUpdate(destroyCancellationToken);
		}

		/// <summary>
		/// 等待本帧结束
		/// </summary>
		protected UniTask WaitForEndOfFrame()
		{
			return UniTask.WaitForEndOfFrame(this, destroyCancellationToken);
		}

		#endregion

		#region 条件等待方法

		/// <summary>
		/// 等待直到条件满足
		/// </summary>
		protected UniTask WaitUntil(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return UniTask.WaitUntil(predicate, timing, destroyCancellationToken);
		}

		/// <summary>
		/// 等待当条件满足
		/// </summary>
		protected UniTask WaitWhile(Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return UniTask.WaitWhile(predicate, timing, destroyCancellationToken);
		}

		/// <summary>
		/// 等待直到值改变
		/// </summary>
		protected UniTask WaitUntilValueChanged<T, U>(T target, Func<T, U> valueCheck, PlayerLoopTiming timing = PlayerLoopTiming.Update, IEqualityComparer<U> equalityComparer = null) where T : class
		{
			return UniTask.WaitUntilValueChanged(target, valueCheck, timing, equalityComparer, destroyCancellationToken);
		}
		#endregion


		#region 实用扩展方法

		/// <summary>
		/// 链接取消令牌
		/// </summary>
		protected CancellationToken LinkToken(CancellationToken externalToken)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, externalToken).Token;
		}

		/// <summary>
		/// 链接多个取消令牌
		/// </summary>
		protected CancellationToken LinkTokens(params CancellationToken[] tokens)
		{
			List<CancellationToken> allTokens = new List<CancellationToken> { destroyCancellationToken };
			allTokens.AddRange(tokens);
			return CancellationTokenSource.CreateLinkedTokenSource(allTokens.ToArray()).Token;
		}

		/// <summary>
		/// 安全的异步方法执行（自动捕获异常）
		/// </summary>
		protected async UniTaskVoid SafeFireAndForget(Func<UniTask> asyncAction, Action<Exception> onError = null)
		{
			try
			{
				await asyncAction();
			}
			catch (OperationCanceledException)
			{
				// 对象被销毁，正常取消
			}
			catch (Exception ex)
			{
				onError?.Invoke(ex);
				Log.Error($"UniTask async operation failed: {ex.Message}");
			}
		}

		/// <summary>
		/// 带超时的异步操作
		/// </summary>
		protected async UniTask<T> WithTimeout<T>(UniTask<T> task, int timeoutMilliseconds, string timeoutMessage = "UniTask operation timed out")
		{
			UniTask timeoutTask = Delay(timeoutMilliseconds);
			(bool hasResultLeft, T result) completedTask = await UniTask.WhenAny(task, timeoutTask);

			if (!completedTask.hasResultLeft)
				throw new TimeoutException(timeoutMessage);

			return await task;
		}

		/// <summary>
		/// 重试异步操作
		/// </summary>
		protected async UniTask<T> Retry<T>(Func<UniTask<T>> operation, int maxRetries, int delayBetweenRetries = 1000)
		{
			for (int i = 0; i < maxRetries; i++)
			{
				try
				{
					return await operation();
				}
				catch (Exception ex) when (i < maxRetries - 1)
				{
					Log.Warn($"Attempt {i + 1} failed: {ex.Message}. Retrying...");
					await Delay(delayBetweenRetries);
				}
			}

			// 最后一次尝试让异常正常抛出
			return await operation();
		}

		#endregion

		#region Icy.Base.Timer相关封装
		/// <summary>
		/// Timer：延迟指定时间后，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		/// <param name="delaySeconds">要延迟的时间，单位秒</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		protected CancellationTokenSource DelayByTime(Action action, float delaySeconds, bool ignoreTimeScale = false)
		{
			CancellationTokenSource linkedTokenSource = GenerateLinkedCancellationTokenSource();
			Base.Timer.DoDelayByTime(action, delaySeconds, linkedTokenSource.Token, ignoreTimeScale).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// Timer：延迟指定帧数后，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		/// <param name="frameCount">要延迟的帧数</param>
		protected CancellationTokenSource DelayByFrame(Action action, int frameCount)
		{
			CancellationTokenSource linkedTokenSource = GenerateLinkedCancellationTokenSource();
			Base.Timer.DoDelayByFrame(action, frameCount, linkedTokenSource.Token).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// Timer：延迟到下一帧，执行 action
		/// </summary>
		/// <param name="action">要延迟执行的回调</param>
		protected CancellationTokenSource NextFrame(Action action)
		{
			CancellationTokenSource linkedTokenSource = GenerateLinkedCancellationTokenSource();
			Base.Timer.DoNextFrame(action, linkedTokenSource.Token).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// Timer：每隔指定的时间间隔，执行一次 action
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perSeconds">每几秒执行一次；下限保底为0.005秒</param>
		/// <param name="repeatCount">执行的次数；如果<0，则次数为无限</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		protected CancellationTokenSource RepeatByTime(Action action, float perSeconds, int repeatCount, bool ignoreTimeScale = false)
		{
			CancellationTokenSource linkedTokenSource = GenerateLinkedCancellationTokenSource();
			Base.Timer.DoRepeatByTime(action, perSeconds, repeatCount, linkedTokenSource.Token, ignoreTimeScale).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// Timer：每隔指定的时间间隔，执行一次 action，直到predicate返回true
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perSeconds">每几秒执行一次；下限保底为0.005秒</param>
		/// <param name="predicate">返回 true时，repeat停止</param>
		/// <param name="ignoreTimeScale">是否忽略TimeScale</param>
		protected CancellationTokenSource RepeatByTimeUntil(Action action, float perSeconds, Func<bool> predicate, bool ignoreTimeScale = false)
		{
			CancellationTokenSource linkedTokenSource = GenerateLinkedCancellationTokenSource();
			Base.Timer.DoRepeatByTimeUntil(action, perSeconds, predicate, linkedTokenSource.Token, ignoreTimeScale).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// Timer：每隔指定的帧数，执行一次 action
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perFrames">每几帧执行一次，下限保底为1</param>
		/// <param name="repeatCount">执行的次数；如果<0，则次数为无限</param>
		protected CancellationTokenSource RepeatByFrame(Action action, int perFrames, int repeatCount)
		{
			CancellationTokenSource linkedTokenSource = GenerateLinkedCancellationTokenSource();
			Base.Timer.DoRepeatByFrame(action, perFrames, repeatCount, linkedTokenSource.Token).Forget();
			return linkedTokenSource;
		}

		/// <summary>
		/// Timer：每隔指定的帧数，执行一次 action，直到predicate返回true
		/// </summary>
		/// <param name="action">要间隔执行的 action</param>
		/// <param name="perFrames">每几帧执行一次，下限保底为1</param>
		/// <param name="predicate">返回 true时，repeat停止</param>
		protected CancellationTokenSource RepeatByFrameUntil(Action action, int perFrames, Func<bool> predicate)
		{
			CancellationTokenSource linkedTokenSource = GenerateLinkedCancellationTokenSource();
			Base.Timer.DoRepeatByFrameUntil(action, perFrames, predicate, linkedTokenSource.Token).Forget();
			return linkedTokenSource;
		}
		#endregion

		protected virtual CancellationTokenSource GenerateLinkedCancellationTokenSource()
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, destroyCancellationToken);
			_AllCancelTokens.Add(linkedTokenSource);
			return linkedTokenSource;
		}

		private void OnDestroy()
		{
			for (int i = 0; i < _AllCancelTokens.Count; i++)
				_AllCancelTokens[i].Dispose();
		}
	}
}
