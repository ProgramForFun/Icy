#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using Icy.Base;

public static class ProcedureTest
{
	public static void Test()
	{
		Procedure procedure = new Procedure("TestProcedure");
		procedure.AddStep(new StepA());
		procedure.AddStep(new StepB());
		procedure.Start();
	}

	public class StepA : ProcedureStep
	{
		public override async UniTask Activate()
		{
			await UniTask.WaitForSeconds(3);
			Log.LogInfo("Step A wait for 3");
			Finish();
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
			await UniTask.WaitForSeconds(3);
			Log.LogInfo("Step B wait for 3");
			Finish();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
#endif
