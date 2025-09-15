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


using Icy.Base;
using Cysharp.Threading.Tasks;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 将HybirdCLR编译出来的补充元数据DLL，Copy到StreamingAssets目录下
	/// </summary>
	public class CopyMetaDataDLL2StreamingAssetsStep : BuildStep
	{
		public override async UniTask Activate()
		{
			await UniTask.WaitForSeconds(5);

			string metaDataDllListPath = Path.Combine("Assets", HybridCLR.Editor.Settings.HybridCLRSettings.Instance.outputAOTGenericReferenceFile);
			List<string> metaDataDLLList = ParseMetaDLLList(metaDataDllListPath);
			if (metaDataDLLList == null || metaDataDLLList.Count == 0)
			{
				Log.LogError($"解析HybridCLR补充元数据DLL列表失败，请检查路径 {metaDataDllListPath}是否存在，以及其中的PatchedAOTAssemblyList这个字段是否有内容");
				OwnerProcedure.Abort();
				return;
			}


			CopyMetaDataDLL();

			Finish();
		}

		protected List<string> ParseMetaDLLList(string path)
		{
			if (File.Exists(path))
			{
				string all = File.ReadAllText(path);

				// 匹配 PatchedAOTAssemblyList 后的大括号内容
				Match blockMatch = Regex.Match(all,
					@"PatchedAOTAssemblyList\s*=\s*new\s+List<string>\s*{\s*([\s\S]*?)\s*};",
					RegexOptions.Singleline);

				if (!blockMatch.Success)
					return null;

				string innerContent = blockMatch.Groups[1].Value;

				// 提取所有引号内的字符串
				MatchCollection stringMatches = Regex.Matches(innerContent, @"\""([^\""]+)\""");

				List<string> rtn = new List<string>();
				foreach (Match match in stringMatches)
					rtn.Add(match.Groups[1].Value);
				return rtn;
			}
			return null;
		}

		protected virtual void CopyMetaDataDLL()
		{

			//AssetDatabase.SaveAssets();
			//AssetDatabase.Refresh();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
