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
				Log.Warn("HybridCLR enable = false，跳过CopyPatchDLLStep");
				Finish();
				return;
			}

			//确保热更DLL生成完成
			await UniTask.WaitForSeconds(1);
			GetAssetSetting();
			GetPatchDLLList();

			string patchDllListPath = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir;
			if (!CopyPatchDLLs(patchDllListPath))
			{
				OwnerProcedure.Abort();
				return;
			}

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
				Log.Error($"CopyPatchDLLs 失败，请先去Icy/Asset/Setting设置{nameof(AssetSetting.PatchDLLCopyToDir)}", nameof(CopyPatchDLLStep));
				return false;
			}

			for (int i = 0; i < _PatchDLLs.Count; i++)
			{
				string dllPath = Path.Combine(srcDir, _PatchDLLs[i]);
				if (File.Exists(dllPath))
				{
					string copy2Path = Path.Combine(copy2Dir, _PatchDLLs[i] + ".bytes");
					File.Copy(dllPath, copy2Path, true);
					Log.Info($"Copy {dllPath}  to  {copy2Path}", nameof(CopyPatchDLLStep));
				}
				else
				{
					Log.Error($"CopyPatchDLLs 失败，没有找到{dllPath}", nameof(CopyPatchDLLStep));
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
				var oldPatchDLLs = _Setting.PatchDLLs.Clone();
				_Setting.PatchDLLs.Clear();

				//先加之前就有的，尽量保持现有的顺序
				for (int i = 0; i < oldPatchDLLs.Count; i++)
				{
					if (_PatchDLLs.Contains(oldPatchDLLs[i]))
						_Setting.PatchDLLs.Add(oldPatchDLLs[i]);
				}

				bool changed = _Setting.PatchDLLs.Count != oldPatchDLLs.Count;
				//本次编译如果有新增，新增的放在最后
				for (int i = 0; i < _PatchDLLs.Count; i++)
				{
					if (!_Setting.PatchDLLs.Contains(_PatchDLLs[i]))
						_Setting.PatchDLLs.Add(oldPatchDLLs[i]);
				}

				string targetDir = SettingsHelper.GetSettingDir();
				SettingsHelper.SaveSetting(targetDir, SettingsHelper.AssetSetting, _Setting.ToByteArray());

				if (changed)
				{
					string msg = "热更DLL列表有变化，在当前的打包/编译操作结束后，请记得前往Icy/Asset/Setting，重新调整热更DLL列表的顺序";
					CommonUtility.SafeDisplayDialog("", msg, "OK", LogLevel.Error);
				}
			}

			await UniTask.CompletedTask;
		}
	}
}
