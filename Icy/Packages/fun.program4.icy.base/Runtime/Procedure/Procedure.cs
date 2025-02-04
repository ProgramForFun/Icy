using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 基于FSM实现的Procedure，用于用按任意顺序组合Step，然后执行
	/// </summary>
	public sealed class Procedure
	{
		/// <summary>
		/// Procedure的名字
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// 获取归一化的执行进度
		/// </summary>
		public float Progress { get { return (_CurrStepIdx + 1) / _Steps.Count; } }
		/// <summary>
		/// 内嵌的状态机
		/// </summary>
		private FSM _FSM;
		/// <summary>
		/// 所有步骤
		/// </summary>
		private List<ProcedureStep> _Steps;
		/// <summary>
		/// 当前执行的Step下标
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
		/// 添加一个指定步骤
		/// </summary>
		public void AddStep(ProcedureStep step)
		{
			_FSM.AddState(step, _Steps.Count == 0);
			_Steps.Add(step);
		}

		/// <summary>
		/// 启动Procedure，开始执行
		/// </summary>
		public void Start()
		{
			_CurrStepIdx = 0;
			_FSM.Start();
		}

		/// <summary>
		/// 执行下一步
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
