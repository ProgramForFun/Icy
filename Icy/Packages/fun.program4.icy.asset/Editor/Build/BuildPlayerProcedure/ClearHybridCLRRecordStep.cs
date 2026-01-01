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


using Cysharp.Threading.Tasks;
using Icy.Base;
using System.IO;
using UnityEditor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 清除HybridCLR有没有Enable的记录文件
	/// </summary>
	public class ClearHybridCLRRecordStep : BuildStep
	{
		public override async UniTask Activate()
		{
			if (Clear())
				Log.Info("Record cleared", nameof(ClearHybridCLRRecordStep));

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			await UniTask.CompletedTask;
			Finish();
		}

		public static bool Clear()
		{
			string relativePath = Path.Combine("Assets/Resources", RecordHybridCLREnableStep.RECORD_FILE_NAME);
			return AssetDatabase.DeleteAsset(relativePath);
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
