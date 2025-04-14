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
				Log.LogInfo("Step A wait for 2");
				FinishAndGoto<StepC>();
			}

			public override async UniTask Deactivate()
			{
				await UniTask.CompletedTask;
			}
		}

		public class StepB : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(2);
				Log.LogInfo("Step B wait for 2");
				Finish();
			}

			public override async UniTask Deactivate()
			{
				await UniTask.CompletedTask;
			}
		}

		public class StepC : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(3);
				Log.LogInfo("Step C wait for 3");
				Finish();
			}

			public override async UniTask Deactivate()
			{
				await UniTask.CompletedTask;
			}
		}

		public class StepD : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(2);
				Log.LogInfo("Step D wait for 2");
				_Procedure.Finish();
			}

			public override async UniTask Deactivate()
			{
				await UniTask.CompletedTask;
			}
		}

		public class StepE : ProcedureStep
		{
			public override async UniTask Activate()
			{
				await UniTask.WaitForSeconds(2);
				Log.LogInfo("Step E wait for 2");
				Finish();
			}

			public override async UniTask Deactivate()
			{
				await UniTask.CompletedTask;
			}
		}
	}
}
#endif
