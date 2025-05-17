using Cysharp.Threading.Tasks;
using System.IO;

namespace Icy.Base
{
	public static class SettingsHelper
	{
		/// <summary>
		/// 获取框架Setting的根目录；
		/// Editor下是相对于项目根目录的相对路径，其他情况下是相对于SteamingAssets的相对路径
		/// </summary>
		internal static string GetSettingDir()
		{
			return "IcySettings";
		}

		/// <summary>
		/// 获取框架EditorOnly Setting的根目录
		/// </summary>
		internal static string GetEditorOnlySettingDir()
		{
			return Path.Combine(GetSettingDir(), "EditorOnly");
		}

		/// <summary>
		/// 加载框架Setting
		/// </summary>
		/// <param name="fileNameWithExtension">setting文件名</param>
		internal static async UniTask<byte[]> LoadSetting(string fileNameWithExtension)
		{
			string path = Path.Combine(GetSettingDir(), fileNameWithExtension);
#if UNITY_EDITOR
			byte[] bytes = File.ReadAllBytes(path);
			await UniTask.CompletedTask;
#else
			byte[] bytes = await CommonUtility.LoadStreamingAsset(path);
#endif
			CommonUtility.xor(bytes);
			return bytes;
		}

#if UNITY_EDITOR
		/// <summary>
		/// 直接同步加载框架Setting，editor专用
		/// </summary>
		/// <param name="dir">Setting文件所在的目录</param>
		/// <param name="fileNameWithExtension">setting文件名</param>
		internal static byte[] LoadSettingEditor(string dir, string fileNameWithExtension)
		{
			string path = Path.Combine(dir, fileNameWithExtension);
			if (File.Exists(path))
			{
				byte[] bytes = File.ReadAllBytes(path);
				CommonUtility.xor(bytes);
				return bytes;
			}
			return null;
		}
#endif

		/// <summary>
		/// 保存框架Setting
		/// </summary>
		/// <param name="dir">要保存到的目录</param>
		/// <param name="fileNameWithExtension">setting文件名</param>
		/// <param name="bytes">setting的byte数组数据</param>
		internal static void SaveSetting(string dir, string fileNameWithExtension, byte[] bytes)
		{
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			string targetPath = Path.Combine(dir, fileNameWithExtension);
			CommonUtility.xor(bytes);
			File.WriteAllBytes(targetPath, bytes);
		}

		internal static string GetBuildSettingName()
		{
#if UNITY_EDITOR
			switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
			{
				case UnityEditor.BuildTarget.Android:
					return "BuildSetting_Android.json";
				case UnityEditor.BuildTarget.iOS:
					return "BuildSetting_iOS.json";
				case UnityEditor.BuildTarget.StandaloneWindows64:
					return "BuildSetting_Win64";
				default:
					Log.Assert(false, $"Unsupported platform {UnityEditor.EditorUserBuildSettings.activeBuildTarget}");
					return "";
			}
#else
			switch (Application.platform)
			{
				case RuntimePlatform.Android:
					return "BuildSetting_Android.json";
				case RuntimePlatform.IPhonePlayer:
					return "BuildSetting_iOS.json";
				case RuntimePlatform.WindowsPlayer:
					return "BuildSetting_Win64";
				default:
					Log.Assert(false, $"Unsupported platform {Application.platform}");
					return "";
			}
#endif
		}
	}
}
