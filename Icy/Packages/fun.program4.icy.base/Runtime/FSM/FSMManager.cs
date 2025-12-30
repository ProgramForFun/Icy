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


using Cysharp.Text;
using System;
using System.Collections.Generic;

namespace Icy.Base
{
	/// <summary>
	/// 统一管理FSM
	/// </summary>
	public class FSMManager : Singleton<FSMManager>
	{
		/// <summary>
		/// 一个FSM被创建出来的事件
		/// </summary>
		public event Action<FSM> OnAddFSM;
		/// <summary>
		/// 一个FSM被Dispose的事件
		/// </summary>
		public event Action<FSM> OnRemoveFSM;
		/// <summary>
		/// 开始切换状态的事件，<所属FSM，旧State，新State>
		/// </summary>
		public event Action<FSM, FSMState, FSMState> OnFSMStateChangingStarted;
		/// <summary>
		/// 前一个状态Deactivate完成的事件，<所属FSM，旧State，新State>
		/// </summary>
		public event Action<FSM, FSMState, FSMState> OnFSMPrevStateDeactivated;
		/// <summary>
		/// 整个状态切换完成的事件，<所属FSM，旧State，新State>
		/// </summary>
		public event Action<FSM, FSMState, FSMState> OnFSMChangingStateEnd;
		/// <summary>
		/// 所有的FSM
		/// </summary>
		private List<FSM> _AllFSMs;
		/// <summary>
		/// 切换到每个FSM状态的时间戳
		/// </summary>
		private Dictionary<FSM, Dictionary<FSMState, long>> _FSMStateStartTimestamps;


		protected override void OnInitialized()
		{
			base.OnInitialized();
			_AllFSMs = new List<FSM>();
			_FSMStateStartTimestamps = new Dictionary<FSM, Dictionary<FSMState, long>>();
		}

		internal void AddFSM(FSM fsm)
		{
			if (_AllFSMs.Contains(fsm))
			{
				Log.Error("Add a duplicate FSM " + fsm.Name, nameof(FSMManager));
				return;
			}

			_AllFSMs.Add(fsm);
			_FSMStateStartTimestamps.Add(fsm, new Dictionary<FSMState, long>());
			fsm.OnStateChangingStarted += OnStateChangingStarted;
			fsm.OnPrevStateDeactivated += OnPrevStateDeactivated;
			fsm.OnChangingStateEnd += OnChangingStateEnd;
			OnAddFSM?.Invoke(fsm);
		}

		internal void RemoveFSM(FSM fsm)
		{
			if (_AllFSMs.Remove(fsm))
			{
				fsm.OnStateChangingStarted -= OnStateChangingStarted;
				fsm.OnPrevStateDeactivated -= OnPrevStateDeactivated;
				fsm.OnChangingStateEnd -= OnChangingStateEnd;
				OnRemoveFSM?.Invoke(fsm);
				_FSMStateStartTimestamps.Remove(fsm);
			}
		}

		protected void OnStateChangingStarted(FSM fsm, FSMState prevState, FSMState nextState)
		{
			_FSMStateStartTimestamps[fsm][nextState] = DateTime.Now.TotalSeconds();
			OnFSMStateChangingStarted?.Invoke(fsm, prevState, nextState);
		}

		protected void OnPrevStateDeactivated(FSM fsm, FSMState prevState, FSMState nextState)
		{
			OnFSMPrevStateDeactivated?.Invoke(fsm, prevState, nextState);
		}

		protected void OnChangingStateEnd(FSM fsm, FSMState prevState, FSMState nextState)
		{
			OnFSMChangingStateEnd?.Invoke(fsm, prevState, nextState);
		}

		/// <summary>
		/// 获取所有FSM列表
		/// </summary>
		public List<FSM> GetAllFSMs()
		{
			return _AllFSMs;
		}

		/// <summary>
		/// 获取指定FSM的指定State的开始时间戳
		/// </summary>
		public long GetFSMStateStartTimestamp(FSM fsm, FSMState state)
		{
			if (_FSMStateStartTimestamps.TryGetValue(fsm, out Dictionary<FSMState, long> fsmDict))
			{
				if (fsmDict.TryGetValue(state, out long timestamp))
					return timestamp;
				else
				{
					Log.Error($"Invalid FSMState {state}", nameof(FSMManager));
					return 0;
				}
			}

			Log.Error($"Invalid FSM {fsm}", nameof(FSMManager));
			return 0;
		}

		public string Dump()
		{
			Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder();
			stringBuilder.AppendLine("---FSM : ");
			for (int i = 0; i < _AllFSMs.Count; i++)
				stringBuilder.AppendLine(_AllFSMs[i].Name);
			return stringBuilder.ToString();
		}
	}
}
