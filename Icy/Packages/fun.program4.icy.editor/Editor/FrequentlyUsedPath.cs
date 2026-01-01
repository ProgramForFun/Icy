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


using Icy.Asset;
using Icy.Base;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Icy.Editor
{
	/// <summary>
	/// 把常用的路径，加入到菜单里
	/// </summary>
	public static class FrequentlyUsedPath
	{
		/// <summary>
		/// Persistent目录
		/// </summary>
		[MenuItem("Icy/Path/Persistent", false, 10)]
		static void OpenPersistentDataPath()
		{
			System.Diagnostics.Process.Start(Application.persistentDataPath);
		}

		/// <summary>
		/// 项目根目录
		/// </summary>
		[MenuItem("Icy/Path/Project Root")]
		static void OpenProjectRootPath()
		{
			string parentDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
			System.Diagnostics.Process.Start(parentDirectory);
		}

		/// <summary>
		/// StreamingAssets目录
		/// </summary>
		[MenuItem("Icy/Path/StreamingAssets")]
		static void OpenStreamingAssetsPath()
		{
			if (Directory.Exists(Application.streamingAssetsPath))
				System.Diagnostics.Process.Start(Application.streamingAssetsPath);
			else
				Debug.LogError($"Can not find {Application.streamingAssetsPath}");
		}

		/// <summary>
		/// Editor log 目录
		/// </summary>
		[MenuItem("Icy/Path/Editor Log")]
		static void OpenEditorLogPath()
		{
			string editorLogPath;
#if UNITY_EDITOR_WIN
			string userDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
			editorLogPath = Path.Combine(userDir, "Unity", "Editor").Replace("\\", "/");
#else
		Debug.LogError("OpenEditorLogPath can only available on Windows as present");
#endif
			if (!string.IsNullOrEmpty(editorLogPath))
				System.Diagnostics.Process.Start(editorLogPath);
		}

		/// <summary>
		/// 打包输出目录
		/// </summary>
		[MenuItem("Icy/Path/Build Output")]
		static void OpenBuildOutputPath()
		{
			string buildSettingFileName = SettingsHelper.GetBuildSettingName();
			BuildSetting buildSetting;
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetSettingDir(), buildSettingFileName);
			if (bytes == null)
			{
				Debug.LogError($"Can not find {buildSettingFileName}");
				return;
			}

			buildSetting = BuildSetting.Parser.ParseFrom(bytes);
			if (string.IsNullOrEmpty(buildSetting.OutputDir))
			{
				Debug.LogError($"BuildSetting.OutputDir is null or empty");
				return;
			}

			if (Directory.Exists(buildSetting.OutputDir))
				System.Diagnostics.Process.Start(buildSetting.OutputDir);
			else
				Debug.LogError($"Can not find {buildSetting.OutputDir}");
		}

		/// <summary>
		/// AssetBundle输出目录
		/// </summary>
		[MenuItem("Icy/Path/AssetBundle Output")]
		static void OpenAssetBundleOutputPath()
		{
			string dir = "Bundles";
			if (Directory.Exists(dir))
				System.Diagnostics.Process.Start(dir);
			else
				Debug.LogError($"Can not find {dir}");
		}
	}
}
