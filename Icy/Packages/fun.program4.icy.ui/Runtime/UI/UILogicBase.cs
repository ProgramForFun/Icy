using System;
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
			IcyFrame.Instance.AddUpdate(this);
		}

		public virtual void Update(float delta)
		{
			
		}

		public virtual void Destroy()
		{
			IcyFrame.Instance.RemoveUpdate(this);
		}
	}
}
