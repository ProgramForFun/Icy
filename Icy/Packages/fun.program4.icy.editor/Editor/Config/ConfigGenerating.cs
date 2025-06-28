using Icy.Base;
using Icy.Editor;
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

/// <summary>
/// 打表
/// </summary>
public static class ConfigGenerating
{
	/// <summary>
	/// 打表
	/// </summary>
	[MenuItem("Icy/Generate Config", false, 950)]
	[MenuItem("Icy/Config/Generate Config", false)]
	static void GenerateConfig()
	{
		try
		{
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetEditorOnlySettingDir(), "ConfigSetting.json");
			if (bytes != null)
			{
				string batFilePath = null;
				ConfigSetting setting = ConfigSetting.Parser.ParseFrom(bytes);
				if (setting != null)
					batFilePath = setting.GenerateBatPath;
				if (string.IsNullOrEmpty(batFilePath))
				{
					EditorUtility.DisplayDialog("", $"打表未执行，请先去Icy/Config/Setting菜单中，设置 生成Config的Bat脚本路径", "OK");
					return;
				}

				ClearConsole.Clear();

				//string outputDirFullPath = Path.GetFullPath(setting.ProtoOutputDir);
				ProcessStartInfo processInfo = new ProcessStartInfo()
				{
					FileName = batFilePath,									// 批处理文件名
					WorkingDirectory = Path.GetDirectoryName(batFilePath),	// 工作目录
					CreateNoWindow = true,									// 不创建新窗口（后台运行）
					UseShellExecute = false,								// 不使用系统Shell（用于重定向输出）
					//Arguments = $"\"{outputDirFullPath}\"",				//Config编译后的代码的输出目录，传入bat

					// 重定向输入/输出
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				};

				using (Process process = new Process())
				{
					process.StartInfo = processInfo;

					//注册输出/错误事件处理程序
					process.OutputDataReceived += OnGenerateConfigLog;
					process.ErrorDataReceived += OnGenerateConfigError;

					process.Start();

					// 如果重定向输出，需要开始异步读取
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();

					// 等待批处理执行完成
					process.WaitForExit();

					int exitCode = process.ExitCode;
					UnityEngine.Debug.Log("Generate config exit code = " + exitCode);
					if (exitCode != 0)
						return;




					AssetDatabase.Refresh();
				}
			}
		}
		catch(Exception e)
		{
			UnityEngine.Debug.LogException(e);
		}
	}

	private static void OnGenerateConfigLog(object sender, DataReceivedEventArgs e)
	{
		if (e != null && e.Data != null)
			UnityEngine.Debug.Log(e.Data);
	}

	private static void OnGenerateConfigError(object sender, DataReceivedEventArgs e)
	{
		if (e != null && e.Data != null)
			UnityEngine.Debug.LogError(e.Data);
	}
}
