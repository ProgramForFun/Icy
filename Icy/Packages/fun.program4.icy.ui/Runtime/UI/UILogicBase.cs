using System;
using Icy.Base;

namespace Icy.UI
{
	/// <summary>
	/// UI逻辑的基类
	/// </summary>
	public abstract class UILogicBase : IUpdateable, IDisposable
	{
		public virtual void Init()
		{
			IcyFrame.Instance.AddUpdate(this);
		}

		public virtual void Update(float delta)
		{
			
		}

		public virtual void Dispose()
		{
			IcyFrame.Instance.RemoveUpdate(this);
		}
	}
}
