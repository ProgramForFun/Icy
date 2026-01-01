/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

namespace Icy.Editor
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
