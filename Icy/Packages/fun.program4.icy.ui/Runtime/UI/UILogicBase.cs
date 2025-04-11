using Icy.Base;

namespace Icy.UI
{
	/// <summary>
	/// UI逻辑的基类
	/// </summary>
	public abstract class UILogicBase : IUpdateable
	{
		public virtual void Init()
		{

		}

		/// <summary>
		/// 如果派生类需要Update，可以直接调用此方法
		/// </summary>
		public void EnableUpdate()
		{
			IcyFrame.Instance.AddUpdate(this);
		}

		/// <summary>
		/// 派生类如需override Update，先调用EnableUpdate
		/// </summary>
		public virtual void Update(float delta)
		{
			
		}

		public virtual void Destroy()
		{
			IcyFrame.Instance.RemoveUpdate(this);
		}
	}
}
