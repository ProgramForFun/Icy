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
		/// 所有的FSM
		/// </summary>
		private List<FSM> _AllFSMs;


		protected override void OnInitialized()
		{
			base.OnInitialized();
			_AllFSMs = new List<FSM>();
		}

		internal void AddFSM(FSM fsm)
		{
			if (_AllFSMs.Contains(fsm))
			{
				Log.Error("Add a duplicate FSM " + fsm.Name);
				return;
			}

			_AllFSMs.Add(fsm);
			fsm.OnStateChangingStarted += OnStartChangingState;
			OnAddFSM?.Invoke(fsm);
		}

		internal void RemoveFSM(FSM fsm)
		{
			if (_AllFSMs.Remove(fsm))
			{
				fsm.OnStateChangingStarted -= OnStartChangingState;
				OnRemoveFSM?.Invoke(fsm);
			}
		}

		protected void OnStartChangingState(FSM fsm, FSMState prevState, FSMState nextState)
		{
			OnFSMStateChangingStarted?.Invoke(fsm, prevState, nextState);
		}

		public List<FSM> GetAllFSMs()
		{
			return _AllFSMs;
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
