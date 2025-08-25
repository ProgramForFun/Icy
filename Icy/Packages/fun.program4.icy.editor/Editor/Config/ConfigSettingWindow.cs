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


using Google.Protobuf;
using Icy.Base;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Icy.Editor
{
	/// <summary>
	/// Proto相关的设置窗口
	/// </summary>
	public class ConfigSettingWindow : OdinEditorWindow
	{
		private static ConfigSettingWindow _ConfigSettingWindow;
		private ConfigSetting _Setting;

		[Title("生成Config的Bat脚本路径")]
		[Delayed]
		[Required]
		[OnValueChanged("OnDataChanged")]
		[ValidateInput("IsGenerateBatPathValid", "Invalid path", InfoMessageType.Error)]
		public string GenerateBatPath;

		[Title("Config代码的导出输出目录")]
		[FolderPath]
		[Required]
		[OnValueChanged("OnDataChanged")]
		[ValidateInput("IsCodeOutputDirValid", "Invalid dir", InfoMessageType.Error)]
		public string CodeOutputDir;

		[Title("Bin格式的Config的导出输出目录")]
		[FolderPath]
		[Required]
		[OnValueChanged("OnDataChanged")]
		[ValidateInput("IsBinOutputDirValid", "Invalid dir", InfoMessageType.Error)]
		public string BinOutputDir;

		[Title("Json格式的Config的导出输出目录")]
		[FolderPath]
		[Required]
		[OnValueChanged("OnDataChanged")]
		[ValidateInput("IsJsonOutputDirValid", "Invalid dir", InfoMessageType.Error)]
		public string JsonOutputDir;


		[MenuItem("Icy/Config/Setting", false, 60)]
		public static void Open()
		{
			if (_ConfigSettingWindow != null)
				_ConfigSettingWindow.Close();
			_ConfigSettingWindow = GetWindow<ConfigSettingWindow>();
		}

		protected override void Initialize()
		{
			base.Initialize();
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetEditorOnlySettingDir(), "ConfigSetting.json");
			if (bytes == null)
				_Setting = new ConfigSetting();
			else
			{
				_Setting = ConfigSetting.Parser.ParseFrom(bytes);
				GenerateBatPath = _Setting.GenerateBatPath;
				CodeOutputDir = _Setting.CodeOutputDir;
				BinOutputDir = _Setting.BinOutputDir;
				JsonOutputDir = _Setting.JsonOutputDir;
			}
		}

		private bool IsGenerateBatPathValid()
		{
			string curDir = Directory.GetCurrentDirectory();
			if (string.IsNullOrEmpty(GenerateBatPath) || !File.Exists(Path.Combine(curDir, GenerateBatPath)))
				return false;
			return true;
		}

		private bool IsCodeOutputDirValid()
		{
			string curDir = Directory.GetCurrentDirectory();
			if (string.IsNullOrEmpty(CodeOutputDir) || !Directory.Exists(Path.Combine(curDir, CodeOutputDir)))
				return false;
			return true;
		}

		private bool IsBinOutputDirValid()
		{
			string curDir = Directory.GetCurrentDirectory();
			if (string.IsNullOrEmpty(BinOutputDir) || !Directory.Exists(Path.Combine(curDir, BinOutputDir)))
				return false;
			return true;
		}

		private bool IsJsonOutputDirValid()
		{
			string curDir = Directory.GetCurrentDirectory();
			if (string.IsNullOrEmpty(JsonOutputDir) || !Directory.Exists(Path.Combine(curDir, JsonOutputDir)))
				return false;
			return true;
		}

		private void OnDataChanged()
		{
			if (!IsGenerateBatPathValid())
			{
				EditorUtility.DisplayDialog("", $"找不到 {GenerateBatPath} 文件，请检查路径", "OK");
				return;
			}

			if (!IsCodeOutputDirValid())
			{
				EditorUtility.DisplayDialog("", $"找不到 {CodeOutputDir} 目录，请检查路径", "OK");
				return;
			}

			if (!IsBinOutputDirValid())
			{
				EditorUtility.DisplayDialog("", $"找不到 {BinOutputDir} 目录，请检查路径", "OK");
				return;
			}

			if (!IsJsonOutputDirValid())
			{
				EditorUtility.DisplayDialog("", $"找不到 {JsonOutputDir} 目录，请检查路径", "OK");
				return;
			}

			_Setting.GenerateBatPath = GenerateBatPath;
			_Setting.CodeOutputDir = CodeOutputDir;
			_Setting.BinOutputDir = BinOutputDir;
			_Setting.JsonOutputDir = JsonOutputDir;

			string targetDir = SettingsHelper.GetEditorOnlySettingDir();
			SettingsHelper.SaveSetting(targetDir, "ConfigSetting.json", _Setting.ToByteArray());
		}
	}
}
