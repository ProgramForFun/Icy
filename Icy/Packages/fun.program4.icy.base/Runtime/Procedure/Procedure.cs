using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// ����FSMʵ�ֵ�Procedure�������ð�����˳�����Step��Ȼ��ִ��
	/// </summary>
	public sealed class Procedure
	{
		/// <summary>
		/// Procedure������
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// ��ȡ��һ����ִ�н���
		/// </summary>
		public float Progress { get { return (_CurrStepIdx + 1) / _Steps.Count; } }
		/// <summary>
		/// ��Ƕ��״̬��
		/// </summary>
		private FSM _FSM;
		/// <summary>
		/// ���в���
		/// </summary>
		private List<ProcedureStep> _Steps;
		/// <summary>
		/// ��ǰִ�е�Step�±�
		/// </summary>
		private int _CurrStepIdx;

		public Procedure(string name)
		{
			Name = name;
			_FSM = new FSM(name);
			_FSM.Blackboard.WriteObject("Procedure", this);
			_Steps = new List<ProcedureStep>();
		}

		/// <summary>
		/// ���һ��ָ������
		/// </summary>
		public void AddStep(ProcedureStep step)
		{
			_FSM.AddState(step, _Steps.Count == 0);
			_Steps.Add(step);
		}

		/// <summary>
		/// ����Procedure����ʼִ��
		/// </summary>
		public void Start()
		{
			_CurrStepIdx = 0;
			_FSM.Start();
		}

		/// <summary>
		/// ִ����һ��
		/// </summary>
		public void NextStep()
		{
			_CurrStepIdx++;
			if (_CurrStepIdx < _Steps.Count)
				_FSM.ChangeState(_Steps[_CurrStepIdx]);
			else
				Log.LogInfo($"Procedure {_FSM.Name} finished");
		}
	}
}
