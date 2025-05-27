using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// AssetTrayWindow，暂存资源列表的Item
	/// </summary>
	[Serializable]
	public sealed class AssetTrayWindowItem
	{
		/// <summary>
		/// 引用的资源
		/// </summary>
		[PreviewField(33)]
		[TableColumnWidth(38, false)]
		public UnityEngine.Object Asset;

		[ReadOnly]
		public string Name;

		[VerticalGroup("操作")]
		[Button("打开")]
		[TableColumnWidth(80, false)]
		public void Open()
		{
			if (Asset != null)
				AssetDatabase.OpenAsset(Asset);
		}

		[VerticalGroup("操作")]
		[Button("复制名字")]
		[TableColumnWidth(80, false)]
		public void CopyName()
		{
			if (Asset != null)
				GUIUtility.systemCopyBuffer = Asset.name;
		}
	}
}
