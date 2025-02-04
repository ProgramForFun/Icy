using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Icy.Base
{
	/// <summary>
	/// 状态机状态的基类
	/// </summary>
	public class FSMState : IUpdateable, IFixedUpdateable, ILateUpdateable
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
		public virtual async UniTask Activate()
		{
			await UniTask.Yield();
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// 解除激活状态
		/// </summary>
		public virtual async UniTask Deactivate()
		{
			await UniTask.Yield();
			throw new System.NotImplementedException();
		}


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