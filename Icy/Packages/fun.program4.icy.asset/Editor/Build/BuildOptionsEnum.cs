using System;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 调试相关选项
	/// </summary>
	[Flags]
	public enum BuildOptionDev
	{
		/// <summary>
		/// 打Dev版本
		/// </summary>
		DevelopmentBuild = 1 << 0,
		/// <summary>
		/// 允许调试代码
		/// </summary>
		ScriptDebugging = 1 << 1,
		/// <summary>
		/// 启动时自动连接Profiler
		/// </summary>
		AutoConnectProfiler = 1 << 2,
		/// <summary>
		/// DeepProfiling
		/// </summary>
		DeepProfiling = 1 << 3,
	}
}
