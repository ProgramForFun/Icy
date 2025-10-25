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

using System.Threading;
using UnityEngine;

namespace Icy.Base
{
	public static class TimerTest
	{
		public static void Test()
		{
			//TestDealy();
			TestRepeat();
		}
		#region Dealy
		private static void TestDealy()
		{
			Log.Info("Timer.DelayByTime(DelayByTimeAction, 1);   This will be cancelled");
			CancellationTokenSource cancel = Timer.DelayByTime(NeverReached, 1);
			cancel.Cancel();

			Log.Info("Timer.DelayByTime(DelayByTimeAction, 1);");
			Timer.DelayByTime(DelayByTimeAction, 1);

			Log.Info("Timer.NextFrame(NextFrameAction); Current frame = " + Time.frameCount);
			Timer.NextFrame(NextFrameAction);

			Log.Info("Timer.DelayByFrame(DelayByTimeAction, 3); Current frame = " + Time.frameCount);
			Timer.DelayByFrame(DelayByFrameAction, 3);
		}

		private static void NeverReached()
		{
			Log.Assert(false, "You should never got this");
		}

		private static void DelayByTimeAction()
		{
			Log.Info("DelayByTimeAction, delay 1 second");
		}

		private static void NextFrameAction()
		{
			Log.Info("NextFrameAction,  Current frame = " + Time.frameCount);
		}

		private static void DelayByFrameAction()
		{
			Log.Info("DelayByFrameAction,  Current frame = " + Time.frameCount);
		}
		#endregion

		#region Repeat
		private static void TestRepeat()
		{
			Log.Info("Timer.RepeatByTime(RepeatByTimeAction, 1, 5);");
			CancellationTokenSource token = Timer.RepeatByTime(RepeatByTimeAction, 1, 5);
			token.CancelAfter(3000);

			Log.Info("Timer.RepeatByFrame(RepeatByFrameAction, 2, 5);  Current frame = " + Time.frameCount);
			Timer.RepeatByFrame(RepeatByFrameAction, 2, 5);
		}

		private static void RepeatByTimeAction()
		{
			Log.Info("RepeatByTimeAction");
		}

		private static void RepeatByFrameAction()
		{
			Log.Info("RepeatByFrameAction;  Current frame = " + Time.frameCount);
		}
		#endregion
	}
}
#endif
