using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 基于FSM实现的Procedure，用于用按任意顺序组合Step，然后执行
	/// </summary>
	public sealed class Procedure
	{
		public enum StateType
		{
			NotStart,
			Running,
			Finishing,
			Finished,
		}

		/// <summary>
		/// Procedure的名字
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// 黑板数据
		/// </summary>
		public Blackboard Blackboard { get { return _FSM.Blackboard; } }
		/// <summary>
		/// Procedure的当前状态
		/// </summary>
		public StateType State { get; private set; }
		/// <summary>
		/// Procedure结束的回调
		/// </summary>
		public event Action OnFinish;
		/// <summary>
		/// Procedure是否已经执行完
		/// </summary>
		public bool IsFinished { get { return State == StateType.Finished; } }
		/// <summary>
		/// 获取归一化的执行进度，跳转Step也包括
		/// </summary>
		public float Progress { get { return (_CurrStepIdx + 1) / _Steps.Count; } }
		/// <summary>
		/// 是否正在切换Step
		/// </summary>
		public bool IsChangingStep { get { return _FSM.IsChangingState; } }
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
			State = StateType.NotStart;
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
			State = StateType.Running;
			_CurrStepIdx = 0;
			_FSM.Start();
		}

		/// <summary>
		/// 执行下一步
		/// </summary>
		public void NextStep()
		{
			if (State == StateType.Finishing || State == StateType.Finished)
				return;

			_CurrStepIdx++;
			if (_CurrStepIdx < _Steps.Count)
				_FSM.ChangeState(_Steps[_CurrStepIdx]);
			else
			{
				State = StateType.Finished;
				OnFinish?.Invoke();
				Log.LogInfo($"Procedure {_FSM.Name} finished");
			}
		}

		/// <summary>
		/// 跳转到指定Step
		/// </summary>
		public void GotoStep<T>() where T : ProcedureStep
		{
			if (State == StateType.Finishing || State == StateType.Finished)
				return;

			int gotoIdx = -1;
			for (int i = 0; i < _Steps.Count; i++)
			{
				if (_Steps[i] is T)
				{
					gotoIdx = i;
					break;
				}
			}

			if (gotoIdx == -1)
			{
				Log.LogError($"{typeof(T).Name} is not belonged to Procedure {Name}", "Procedure");
				return;
			}

			_CurrStepIdx = gotoIdx;
			_FSM.ChangeState(_Steps[_CurrStepIdx]);
		}

		/// <summary>
		/// 等当前状态切换完成后，直接结束本Procedure
		/// </summary>
		public void Finish()
		{
			State = StateType.Finishing;
			DoFinish().Forget();
		}

		private async UniTaskVoid DoFinish()
		{
			await UniTask.WaitUntil(() => !IsChangingStep);
			State = StateType.Finished;
			OnFinish?.Invoke();
			Log.LogInfo($"Procedure {_FSM.Name} finished by Finish()");
		}
	}
}
