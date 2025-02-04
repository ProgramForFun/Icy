
namespace Icy.Base
{
	/// <summary>
	/// ��Procedureһ��ʹ�õ�Step
	/// </summary>
	public abstract class ProcedureStep : FSMState
	{
		/// <summary>
		/// ������ǰStep
		/// </summary>
		protected void Finish()
		{
			Procedure procedure = OwnerFSM.Blackboard.ReadObject("Procedure") as Procedure;
			procedure.NextStep();
		}
	}
}