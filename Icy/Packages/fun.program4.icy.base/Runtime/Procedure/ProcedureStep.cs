
using Cysharp.Threading.Tasks;

namespace Icy.Base
{
	/// <summary>
	/// 和Procedure一起使用的Step
	/// </summary>
	public abstract class ProcedureStep : FSMState
	{
		protected Procedure _Procedure;

		public override void Init(FSM owner)
		{
			base.Init(owner);
			_Procedure = OwnerFSM.Blackboard.ReadObject("Procedure") as Procedure;
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
			await UniTask.WaitUntil(()=> !_Procedure.IsChangingStep);
			_Procedure.NextStep();
		}

		private async UniTaskVoid DoFinishAndGoto<T>() where T : ProcedureStep
		{
			await UniTask.WaitUntil(() => !_Procedure.IsChangingStep);
			_Procedure.GotoStep<T>();
		}
	}
}
