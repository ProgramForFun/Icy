
namespace Icy.UI
{
	/// <summary>
	/// UI的类型
	/// </summary>
	public enum UIType
	{
		/// <summary>
		/// 一般是比较大一点的窗口，参与回退栈
		/// </summary>
		Dialog,
		/// <summary>
		/// 一般是比较小的弹窗之类的，不参与回退栈
		/// </summary>
		Popup,
	}

	/// <summary>
	/// 如何隐藏UI
	/// </summary>
	public enum UIHideType
	{
		/// <summary>
		/// SetActive(false);
		/// </summary>
		Deactive,
		/// <summary>
		/// 移动到屏幕外面
		/// </summary>
		MoveOutScreen,
		/// <summary>
		/// 变透明
		/// </summary>
		Transparent,
	}

	/// <summary>
	/// UI的层级划分，取值是Canvas上的Order In Layer
	/// </summary>
	public enum UILayer
	{
		Bottom = 1000,
		Low = 1500,
		Medium = 2000,
		High = 2500,
		Top = 3000,
	}
}
