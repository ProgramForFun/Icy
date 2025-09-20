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
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace Icy.Editor
{
	/// <summary>
	/// 打表
	/// </summary>
	public static class ConfigGenerating
	{
		private static Process _Process;

		/// <summary>
		/// 打表
		/// </summary>
		[MenuItem("Icy/Generate Config", false, 950)]
		[MenuItem("Icy/Config/Generate Config", false)]
		static void GenerateConfig()
		{
			try
			{
				BiProgress.Show("Generate Config", "Generating config...", 0.5f);

				byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetEditorOnlySettingDir(), SettingsHelper.ConfigSetting);
				if (bytes == null)
				{
					EditorUtility.DisplayDialog("", $"打表未执行，未找到{SettingsHelper.GetEditorOnlySettingDir()}/{SettingsHelper.ConfigSetting}", "OK");
					Clear();
				}
				else
				{
					string batFilePath = null;
					ConfigSetting setting = ConfigSetting.Parser.ParseFrom(bytes);
					if (setting != null)
						batFilePath = setting.GenerateBatPath;
					if (string.IsNullOrEmpty(batFilePath) || string.IsNullOrEmpty(setting.CodeOutputDir) 
						|| string.IsNullOrEmpty(setting.BinOutputDir) || string.IsNullOrEmpty(setting.JsonOutputDir))
					{
						EditorUtility.DisplayDialog("", $"打表未执行，请先去Icy/Config/Setting菜单中，完成所有的设置", "OK");
						Clear();
						return;
					}

					ClearConsole.Clear();

					string batDir = Path.GetDirectoryName(batFilePath);
					ProcessStartInfo processInfo = new ProcessStartInfo()
					{
						FileName = batFilePath,         // 批处理文件名
						WorkingDirectory = batDir,      // 工作目录
						CreateNoWindow = true,          // 不创建新窗口（后台运行）
						UseShellExecute = false,        // 不使用系统Shell（用于重定向输出）

						// 重定向输入/输出
						RedirectStandardOutput = true,
						RedirectStandardError = true,
					};

					string relativeCodeOutputDir = Path.GetRelativePath(Path.GetFullPath(batDir), Path.GetFullPath(setting.CodeOutputDir));
					string relativeBinOutputDir = Path.GetRelativePath(Path.GetFullPath(batDir), Path.GetFullPath(setting.BinOutputDir));
					string relativeJsonOutputDir = Path.GetRelativePath(Path.GetFullPath(batDir), Path.GetFullPath(setting.JsonOutputDir));
					processInfo.ArgumentList.Add(relativeCodeOutputDir);
					processInfo.ArgumentList.Add(relativeBinOutputDir);
					processInfo.ArgumentList.Add(relativeJsonOutputDir);

					_Process = new Process();
					_Process.StartInfo = processInfo;
					_Process.EnableRaisingEvents = true;
					_Process.Exited += OnGenerateEnd;

					//注册输出/错误事件处理程序
					_Process.OutputDataReceived += OnGenerateConfigLog;
					_Process.ErrorDataReceived += OnGenerateConfigError;

					_Process.Start();

					// 如果重定向输出，需要开始异步读取
					_Process.BeginOutputReadLine();
					_Process.BeginErrorReadLine();
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				Clear();
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

		private static void OnGenerateEnd(object sender, EventArgs e)
		{
			int exitCode = _Process.ExitCode;
			UnityEngine.Debug.Log("Generate config exit code = " + exitCode);

			EditorApplication.delayCall += () =>
			{
				Clear();
			};

			if (exitCode == 0)
				AssetDatabase.Refresh();
		}

		private static void Clear()
		{
			BiProgress.Hide();
			_Process?.Dispose();
		}
	}
}
