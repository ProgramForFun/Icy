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


using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;

namespace Icy.Base
{
	/// <summary>
	/// 有限状态机
	/// </summary>
	public class FSM : IUpdateable, IFixedUpdateable, ILateUpdateable, IDisposable
	{
		/// <summary>
		/// 此状态机的名字
		/// </summary>
		public string Name { get; protected set; }
		/// <summary>
		/// 当前状态
		/// </summary>
		public FSMState CurrState { get; protected set; }
		/// <summary>
		/// 前一个状态
		/// </summary>
		public FSMState PrevState { get; protected set; }
		/// <summary>
		/// 下一个状态
		/// </summary>
		public FSMState NextState { get; protected set; }
		/// <summary>
		/// 是否正在切换状态
		/// </summary>
		public bool IsChangingState { get; protected set; }
		/// <summary>
		/// 是否已被销毁
		/// </summary>
		public bool IsDisposed { get; protected set; }
		/// <summary>
		/// 黑板数据
		/// </summary>
		public Blackboard Blackboard { get; protected set; }

		/// <summary>
		/// 属于此状态机的所有状态
		/// </summary>
		public List<FSMState> AllStates { get; }
		/// <summary>
		/// 默认状态
		/// </summary>
		protected FSMState _DefaultState;


		/// <summary>
		/// 建议每个状态机都有一个自己独有的名字
		/// </summary>
		public FSM(string name)
		{
			Name = name;
			IsChangingState = false;
			IsDisposed = false;

			Blackboard = new Blackboard();
			AllStates = new List<FSMState>();

			IcyFrame.Instance.AddUpdate(this);
			IcyFrame.Instance.AddFixedUpdate(this);
			IcyFrame.Instance.AddLateUpdate(this);

			FSMManager.Instance.AddFSM(this);
		}

		/// <summary>
		/// 添加一个状态
		/// </summary>
		/// <param name="state"></param>
		/// <param name="isDefaultState">是否是默认状态；FSM已启动会自动切到DefaultState；DefaultState只能有一个</param>
		public void AddState(FSMState state, bool isDefaultState = false)
		{
			if (AllStates.Contains(state))
			{
				Log.Error($"{state.GetType().Name} has already been add to FSM {Name}");
				return;
			}
			AllStates.Add(state);


			if (isDefaultState)
			{
				if (_DefaultState != null)
				{
					Log.Error($"{state.GetType().Name} added as default FSMState, but FSM {Name} already got one, it's {_DefaultState.GetType().Name}");
					return;
				}
				_DefaultState = state;
			}
		}

		public void Start()
		{
			for (int i = 0; i < AllStates.Count; ++i)
				AllStates[i].Init(this);

			if (_DefaultState != null)
				ChangeState(_DefaultState);
		}

		/// <summary>
		/// 切换状态
		/// </summary>
		public void ChangeState(FSMState newState)
		{
			Log.Assert(!IsChangingState, $"Attempt to change to {newState.GetType().Name} when FSM {Name} is changing");
			if (newState == CurrState)
			{
				Log.Error("New state is same as current state, ignore state changing");
				return;
			}

			string logMsg = string.Format("Change FSMState from {0} to {1}"
							, CurrState == null ? "Null" : CurrState.GetType().Name
							, newState == null ? "Null" : newState.GetType().Name);
			Log.Info(logMsg);

			IsChangingState = true;
			PrevState = CurrState;
			NextState = newState;

			DoChangeState(newState).Forget();
		}

		/// <summary>
		/// FSM是否包含指定FSMState对象
		/// </summary>
		public bool ContainsState(FSMState state)
		{
			return AllStates.Contains(state);
		}

		/// <summary>
		/// FSM是否包含指定类型的FSMState
		/// </summary>
		public bool ContainsState<T>() where T : FSMState
		{
			for (int i = 0; i < AllStates.Count; i++)
			{
				if (AllStates[i] is T)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 真正负责切换状态的协程
		/// </summary>
		private async UniTaskVoid DoChangeState(FSMState newState)
		{
			if (CurrState != null)
			{
				await CurrState.Deactivate();
				CurrState = null;
			}

			if (newState == null)
				CurrState = null;
			else
			{
				if (newState.OwnerFSM != this)
				{
					Log.Assert(false, $"FSM is changing a unexpected FSMState which is not belong to it, state = {newState.GetType().Name}");
					return;
				}
				CurrState = newState;

				await CurrState.Activate();
			}
			ChangeStateEnd();
		}

		/// <summary>
		/// 状态切换结束
		/// </summary>
		private void ChangeStateEnd()
		{
			IsChangingState = false;
			PrevState = null;
			NextState = null;
		}

		public virtual void Update(float delta)
		{
			for (int i = 0; i < AllStates.Count; i++)
				AllStates[i]?.Update(delta);
		}

		public virtual void FixedUpdate(float delta)
		{
			for (int i = 0; i < AllStates.Count; i++)
				AllStates[i]?.FixedUpdate(delta);
		}

		public virtual void LateUpdate(float delta)
		{
			for (int i = 0; i < AllStates.Count; i++)
				AllStates[i]?.LateUpdate(delta);
		}

		public void Dispose()
		{
			IcyFrame.Instance.RemoveUpdate(this);
			IcyFrame.Instance.RemoveFixedUpdate(this);
			IcyFrame.Instance.RemoveLateUpdate(this);
			IsDisposed = true;

			FSMManager.Instance.RemoveFSM(this);
		}
	}
}
