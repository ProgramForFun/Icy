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

			buildSetting = BuildSetting.Descriptor.Parser.ParseFrom(bytes) as BuildSetting;
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
