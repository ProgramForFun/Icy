/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


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
		/// Procedure结束的回调，true是正常执行完，false是调用Abort退出了
		/// </summary>
		public event Action<bool> OnFinish;
		/// <summary>
		/// 切换一个新Step的回调，参数是新的Step，但是可能还没切换完成
		/// </summary>
		public event Action<ProcedureStep> OnChangeStep;
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
		public async UniTask NextStep()
		{
			if (State == StateType.Finishing || State == StateType.Finished)
				return;

			_CurrStepIdx++;
			if (_CurrStepIdx < _Steps.Count)
			{
				OnChangeStep?.Invoke(_Steps[_CurrStepIdx]);
				_FSM.ChangeState(_Steps[_CurrStepIdx]);
			}
			else
			{
				//执行最后一步的Deactivate
				await _Steps[_CurrStepIdx - 1].Deactivate();

				End(false);
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
				Log.LogError($"{typeof(T).Name} is not belonged to Procedure {Name}", nameof(Procedure));
				return;
			}

			_CurrStepIdx = gotoIdx;
			OnChangeStep?.Invoke(_Steps[_CurrStepIdx]);
			_FSM.ChangeState(_Steps[_CurrStepIdx]);
		}

		/// <summary>
		/// 等当前状态切换完成后，直接结束本Procedure
		/// </summary>
		public void Abort()
		{
			DoAbort().Forget();
		}

		private async UniTaskVoid DoAbort()
		{
			await UniTask.WaitUntil(IsNotChangingStep);
			End(true);
		}

		private bool IsNotChangingStep()
		{
			return !IsChangingStep;
		}

		private void End(bool isAbort)
		{
			State = StateType.Finished;
			OnFinish?.Invoke(!isAbort);
			_FSM.Dispose();

			if (isAbort)
				Log.LogInfo($"Procedure {_FSM.Name} aborted", nameof(Procedure));
			else
				Log.LogInfo($"Procedure {_FSM.Name} finished", nameof(Procedure));
		}
	}
}
