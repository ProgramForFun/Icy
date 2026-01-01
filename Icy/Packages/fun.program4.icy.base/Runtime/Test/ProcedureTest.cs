/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
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

namespace Icy.Base
{
	public static class ProcedureTest
	{
		public static void Test()
		{
			Procedure procedure = new Procedure("TestProcedure");
			procedure.AddStep(new StepA());
			procedure.AddStep(new StepB());
			procedure.AddStep(new StepC());
			procedure.AddStep(new StepD());
			procedure.AddStep(new StepE());
			procedure.Start();
		}

		public class StepA : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(2);
				Log.Info("Step A wait for 2");
				FinishAndGoto<StepC>();
			}

			public override async UniTask Deactivate()
			{
				Log.Info("Step A Deactivate");
				await UniTask.CompletedTask;
			}
		}

		public class StepB : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(2);
				Log.Info("Step B wait for 2");
				Finish();
			}

			public override async UniTask Deactivate()
			{
				Log.Info("Step B Deactivate");
				await UniTask.CompletedTask;
			}
		}

		public class StepC : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(3);
				Log.Info("Step C wait for 3");
				Finish();
			}

			public override async UniTask Deactivate()
			{
				Log.Info("Step C Deactivate");
				await UniTask.CompletedTask;
			}
		}

		public class StepD : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(2);
				Log.Info("Step D wait for 2");
				OwnerProcedure.Abort();
			}

			public override async UniTask Deactivate()
			{
				//注意：Abort后，Deactivate不会执行
				Log.Info("Step D Deactivate");
				await UniTask.CompletedTask;
			}
		}

		public class StepE : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(2);
				Log.Info("Step E wait for 2");
				Finish();
			}

			public override async UniTask Deactivate()
			{
				Log.Info("Step E Deactivate");
				await UniTask.CompletedTask;
			}
		}
	}
}
#endif
