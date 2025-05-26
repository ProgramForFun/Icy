using System;
using Sirenix.OdinInspector;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// AssetBundleWindow，AssetBundle补丁列表的Item
	/// </summary>
	[Serializable]
	public sealed class AssetBundleWindowItem
	{
		/// <summary>
		/// 补丁包
		/// </summary>
		[ReadOnly]
		public string AssetBundleVersion;


		[Button("打开文件夹")]
		[TableColumnWidth(80, false)]
		public void OpenFolder()
		{
			System.Diagnostics.Process.Start(AssetBundleVersion);
		}
	}
}
