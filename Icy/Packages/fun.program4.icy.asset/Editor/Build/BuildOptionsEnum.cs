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

	/// <summary>
	/// AssetBundle相关选项
	/// </summary>
	[Flags]
	public enum BuildOptionAssetBundle
	{
		/// <summary>
		/// 是否打包AssetBundle
		/// </summary>
		BuildAssetBundle = 1 << 0,
		/// <summary>
		/// 是否清除缓存、打全量AssetBundle
		/// </summary>
		ClearAssetBundleCache = 1 << 1,
		/// <summary>
		/// 是否启动AssetBundle加密
		/// </summary>
		EncryptAssetBundle  = 1 << 2,
	}
}
