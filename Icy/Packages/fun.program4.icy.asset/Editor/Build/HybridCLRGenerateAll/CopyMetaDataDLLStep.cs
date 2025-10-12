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
using Google.Protobuf;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 将HybirdCLR编译出来的补充元数据DLL，Copy到AssetSetting中的指定目录下
	/// </summary>
	public class CopyMetaDataDLLStep : BuildStep
	{
		protected AssetSetting _Setting;
		protected List<string> _MetaDataDLLs;

		public override async UniTask Activate()
		{
			//确保HybridCLRGenerate/AOTGenericReferences.cs生成完成
			await UniTask.WaitForSeconds(1);
			GetAssetSetting();

			string metaDataDllListPath = Path.Combine("Assets", HybridCLR.Editor.Settings.HybridCLRSettings.Instance.outputAOTGenericReferenceFile);
			_MetaDataDLLs = ParseMetaDLLList(metaDataDllListPath);
			if (_MetaDataDLLs == null)
			{
				Log.LogError($"解析HybridCLR补充元数据DLL列表失败，请检查路径 {metaDataDllListPath}是否存在", nameof(CopyMetaDataDLLStep));
				OwnerProcedure.Abort();
				return;
			}

			if (_MetaDataDLLs.Count == 0)
			{
				Log.LogError($"解析HybridCLR补充元数据DLL列表失败，请检查其中的PatchedAOTAssemblyList这个字段是否有内容", nameof(CopyMetaDataDLLStep));
				OwnerProcedure.Abort();
				return;
			}

			if (!CopyMetaDataDLLs())
			{
				Log.LogError($"复制补充元数据DLL失败", nameof(CopyMetaDataDLLStep));
				OwnerProcedure.Abort();
				return;
			}

			Finish();
		}

		protected virtual List<string> ParseMetaDLLList(string path)
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

		protected virtual bool CopyMetaDataDLLs()
		{
			string settingDir = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.strippedAOTDllOutputRootDir;
			string srcDir = Path.Combine(settingDir, EditorUserBuildSettings.activeBuildTarget.ToString());
			string copy2Dir = _Setting.MetaDataDLLCopyToDir;

			if (string.IsNullOrEmpty(copy2Dir))
			{
				Log.LogError($"CopyMetaDataDLL 失败，请先去Icy/Asset/Setting设置{nameof(AssetSetting.MetaDataDLLCopyToDir)}", nameof(CopyMetaDataDLLStep));
				return false;
			}

			for (int i = 0; i < _MetaDataDLLs.Count; i++)
			{
				string dllPath = Path.Combine(srcDir, _MetaDataDLLs[i]);
				if (File.Exists(dllPath))
				{
					string copy2Path = Path.Combine(copy2Dir, _MetaDataDLLs[i]);
					File.Copy(dllPath, copy2Path, true);
					Log.LogInfo($"Copy {dllPath}  to  {copy2Path}", nameof(CopyMetaDataDLLStep));
				}
				else
				{
					Log.LogError($"CopyMetaDataDLL 失败，没有找到{dllPath}", nameof(CopyMetaDataDLLStep));
					return false;
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			return true;
		}

		protected AssetSetting GetAssetSetting()
		{
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetSettingDir(), SettingsHelper.AssetSetting);
			if (bytes == null)
				_Setting = new AssetSetting();
			else
				_Setting = AssetSetting.Parser.ParseFrom(bytes);
			return _Setting;
		}

		public override async UniTask Deactivate()
		{
			if (_MetaDataDLLs != null)
			{
				_Setting.MetaDataDLLs.Clear();
				for (int i = 0; i < _MetaDataDLLs.Count; i++)
					_Setting.MetaDataDLLs.Add(_MetaDataDLLs[i]);

				string targetDir = SettingsHelper.GetSettingDir();
				SettingsHelper.SaveSetting(targetDir, SettingsHelper.AssetSetting, _Setting.ToByteArray());
			}

			await UniTask.CompletedTask;
		}
	}
}
