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
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// 和Procedure一起使用的Step
	/// </summary>
	public abstract class ProcedureStep : FSMState
	{
		public Procedure OwnerProcedure { get; protected set; }

		public override void Init(FSM owner)
		{
			base.Init(owner);
			OwnerProcedure = OwnerFSM.Blackboard.ReadObject("Procedure", true) as Procedure;
		}

		/// <summary>
		/// 结束当前Step
		/// </summary>
		protected void Finish()
		{
			DoFinish().Forget();
		}

		/// <summary>
		/// 结束当前、并跳转到指定Step
		/// </summary>
		protected void FinishAndGoto<T>() where T : ProcedureStep
		{
			DoFinishAndGoto<T>().Forget();
		}

		private async UniTaskVoid DoFinish()
		{
			await WaitChangeStepFinish();
			await OwnerProcedure.NextStep();
		}

		private async UniTaskVoid DoFinishAndGoto<T>() where T : ProcedureStep
		{
			await WaitChangeStepFinish();
			OwnerProcedure.GotoStep<T>();
		}

		private async UniTask WaitChangeStepFinish()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
				await UniTask.WaitUntil(IsChangingStepFinish);
			else
			{
				//在editor下使用时，比如打包完成时，WaitUntil里的predicate可能会为null导致报错，这里改成Warning
				try
				{
					await UniTask.WaitUntil(IsChangingStepFinish);
				}
				catch (Exception e)
				{
					Log.Warn($"{GetType().Name} step UniTask.WaitUntil exception, {e}");
				}
			}
#else
			await UniTask.WaitUntil(IsChangingStepFinish);
#endif
		}

		private bool IsChangingStepFinish()
		{
			return !OwnerProcedure.IsChangingStep;
		}
	}
}
