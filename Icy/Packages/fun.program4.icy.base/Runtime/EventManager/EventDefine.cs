
namespace Icy.Base
{
	/// <summary>
	/// 框架内部的事件定义
	/// </summary>
	public static class EventDefine
	{
		#region UI代码生成
		/// <summary>
		/// UICodeGenerator的其中一个Item的Name字段发生了变化
		/// </summary>
		public static readonly int UICodeGeneratorNameChanged = -1000;
		/// <summary>
		/// 触发UI代码的生成
		/// </summary>
		public static readonly int GenerateUICode = -1001;
		/// <summary>
		/// 触发UI Logic代码的生成
		/// </summary>
		public static readonly int GenerateUILogicCode = -1002;
		/// <summary>
		/// 同时触发UI和UI Logic代码的生成
		/// </summary>
		public static readonly int GenerateUICodeBoth = -1003;
		#endregion

		#region 资源
		/// <summary>
		/// 从远端更新资源版本结束
		/// </summary>
		public static readonly int UpdateAssetVersionEnd = -1100;
		public static readonly int UpdateAssetManifestEnd = -1101;
		#endregion
	}
}
