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