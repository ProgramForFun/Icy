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

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 将HybirdCLR编译出来的热更DLL，Copy到AssetSetting中的指定目录下
	/// </summary>
	public class CopyPatchDLLStep : BuildStep
	{
		protected AssetSetting _Setting;
		protected List<string> _PatchDLLs;

		public override async UniTask Activate()
		{
			string patchDllListPath = Path.Combine("Assets", HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir);
			CopyPatchDLLs(patchDllListPath);
			await UniTask.CompletedTask;
			Finish();
		}

		protected virtual void GetPatchDLLList()
		{
			_PatchDLLs = new List<string>(4);

		}

		protected virtual bool CopyPatchDLLs(string patchDLLOutputPath)
		{
			string srcDir = Path.Combine(patchDLLOutputPath, EditorUserBuildSettings.activeBuildTarget.ToString());
			string copy2Dir = _Setting.PatchDLLCopyToDir;

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
			await UniTask.CompletedTask;
		}
	}
}
