using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// ����FSMʵ�ֵ�Procedure������������˳�����Step������ĳ��
	/// </summary>
	public sealed class Procedure
	{
		public string Name { get; private set; }
		private FSM _FSM;
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

		public void AddStep(ProcedureStep step)
		{
			_FSM.AddState(step, _Steps.Count == 0);
			_Steps.Add(step);
		}

		public void Start()
		{
			_CurrStepIdx = 0;
			_FSM.Start();
		}

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
