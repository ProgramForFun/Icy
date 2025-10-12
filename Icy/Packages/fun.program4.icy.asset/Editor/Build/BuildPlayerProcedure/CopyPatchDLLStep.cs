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


using UnityEngine;
using Icy.Base;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEditor;
using System.Collections.Generic;
using Google.Protobuf;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 将HybirdCLR编译出来的热更DLL，Copy到AssetSetting中的指定目录下
	/// </summary>
	public class CopyPatchDLLStep : BuildStep
	{
		protected AssetSetting _Setting;
		protected List<string> _PatchDLLs;

		[System.Serializable]
		public class AssemblyDefinitionData
		{
			public string name;
		}

		public override async UniTask Activate()
		{
			if (!HybridCLR.Editor.Settings.HybridCLRSettings.Instance.enable)
			{
				Log.LogWarning("HybridCLR enable = false，跳过CopyPatchDLLStep");
				Finish();
				return;
			}

			//确保热更DLL生成完成
			await UniTask.WaitForSeconds(5);
			GetAssetSetting();
			GetPatchDLLList();

			string patchDllListPath = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir;
			CopyPatchDLLs(patchDllListPath);
			await UniTask.CompletedTask;
			Finish();
		}

		protected virtual void GetPatchDLLList()
		{
			_PatchDLLs = new List<string>(4);

			string[] patchAssembleNames = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblies;
			if (patchAssembleNames != null)
			{
				for (int i = 0; i < patchAssembleNames.Length; i++)
					_PatchDLLs.Add(patchAssembleNames[i] + ".dll");
			}

			UnityEditorInternal.AssemblyDefinitionAsset[] patchAsmDefs = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions;
			if (patchAsmDefs != null)
			{
				for (int i = 0; i < patchAsmDefs.Length; i++)
				{
					AssemblyDefinitionData data = JsonUtility.FromJson<AssemblyDefinitionData>(patchAsmDefs[i].text);
					_PatchDLLs.Add(data.name + ".dll");
				}
			}
		}

		protected virtual bool CopyPatchDLLs(string patchDLLOutputPath)
		{
			string srcDir = Path.Combine(patchDLLOutputPath, EditorUserBuildSettings.activeBuildTarget.ToString());
			string copy2Dir = _Setting.PatchDLLCopyToDir;

			if (string.IsNullOrEmpty(copy2Dir))
			{
				Log.LogError($"CopyPatchDLLs 失败，请先去Icy/Asset/Setting设置{nameof(AssetSetting.PatchDLLCopyToDir)}", nameof(CopyPatchDLLStep));
				return false;
			}

			for (int i = 0; i < _PatchDLLs.Count; i++)
			{
				string dllPath = Path.Combine(srcDir, _PatchDLLs[i]);
				if (File.Exists(dllPath))
				{
					string copy2Path = Path.Combine(copy2Dir, _PatchDLLs[i]);
					File.Copy(dllPath, copy2Path, true);
				}
				else
				{
					Log.LogError($"CopyPatchDLLs 失败，没有找到{dllPath}", nameof(CopyPatchDLLStep));
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
			if (_PatchDLLs != null)
			{
				_Setting.PatchDLLs.Clear();
				for (int i = 0; i < _PatchDLLs.Count; i++)
					_Setting.PatchDLLs.Add(_PatchDLLs[i]);

				string targetDir = SettingsHelper.GetSettingDir();
				SettingsHelper.SaveSetting(targetDir, SettingsHelper.AssetSetting, _Setting.ToByteArray());
			}

			await UniTask.CompletedTask;
		}
	}
}
