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

namespace Icy.Base
{
	/// <summary>
	/// 状态机状态的基类
	/// </summary>
	public abstract class FSMState : IUpdateable, IFixedUpdateable, ILateUpdateable
	{
		/// <summary>
		/// 此状态所属的FSM
		/// </summary>
		public FSM OwnerFSM { get; protected set; }


		/// <summary>
		/// 初始化状态
		/// </summary>
		/// <param name="owner"></param>
		public virtual void Init(FSM owner)
		{
			OwnerFSM = owner;
		}

		/// <summary>
		/// 激活状态
		/// </summary>
		public abstract UniTask Activate();

		/// <summary>
		/// 解除激活状态
		/// </summary>
		public abstract UniTask Deactivate();


		public virtual void Update(float delta)
		{

		}

		public virtual void FixedUpdate(float delta)
		{

		}

		public virtual void LateUpdate(float delta)
		{

		}
	}
}
