/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
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
using UnityEngine;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 记录HybridCLR有没有Enable，用于运行时判断
	/// </summary>
	public class RecordHybridCLREnableStep : BuildStep
	{
		public static readonly string RECORD_FILE_NAME = "HybridCLR_Enable.txt";

		public override async UniTask Activate()
		{
			if (HybridCLR.Editor.Settings.HybridCLRSettings.Instance.enable)
			{
				string resourcesPath = Path.Combine(Application.dataPath, "Resources");

				// 确保Resources目录存在
				if (!Directory.Exists(resourcesPath))
					Directory.CreateDirectory(resourcesPath);

				string filePath = Path.Combine(resourcesPath, RECORD_FILE_NAME);
				File.WriteAllText(filePath, "1", System.Text.Encoding.UTF8);
				Log.Info("Recorded", nameof(RecordHybridCLREnableStep));
			}
			else
			{
				if (ClearHybridCLRRecordStep.Clear())
					Log.Info("Old Record cleared", nameof(RecordHybridCLREnableStep));
			}

			AssetDatabase.Refresh();

			await UniTask.CompletedTask;
			Finish();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
