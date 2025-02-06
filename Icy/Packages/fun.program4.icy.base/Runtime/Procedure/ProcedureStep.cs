
using Cysharp.Threading.Tasks;

namespace Icy.Base
{
	/// <summary>
	/// 和Procedure一起使用的Step
	/// </summary>
	public abstract class ProcedureStep : FSMState
	{
		/// <summary>
		/// 结束当前Step
		/// </summary>
		protected void Finish()
		{
			DoFinish().Forget();
		}

		private async UniTaskVoid DoFinish()
		{
			Procedure procedure = OwnerFSM.Blackboard.ReadObject("Procedure") as Procedure;
			await UniTask.WaitUntil(()=> !procedure.IsChangingStep);
			procedure.NextStep();
		}
	}
}
