
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
			Procedure procedure = OwnerFSM.Blackboard.ReadObject("Procedure") as Procedure;
			procedure.NextStep();
		}
	}
}
