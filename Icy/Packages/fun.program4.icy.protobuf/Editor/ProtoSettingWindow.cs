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

namespace Icy.Protobuf.Editor
{
	/// <summary>
	/// Proto相关的设置窗口
	/// </summary>
	public class ProtoSettingWindow : OdinEditorWindow
	{
		private static ProtoSettingWindow _ProtoSettingWindow;
		private ProtoSetting _Setting;

		[Title("编译Proto的Bat脚本路径")]
		[Delayed]
		[Required]
		[OnValueChanged("OnDataChanged")]
		[ValidateInput("IsCompileBatPathValid", "Invalid compile bat path", InfoMessageType.Error)]
		public string CompileBatPath;

		[Title("Proto编译后的代码的输出目录")]
		[FolderPath]
		[Required]
		[OnValueChanged("OnDataChanged")]
		[ValidateInput("IsProtoOutputDirValid", "Invalid proto output dir", InfoMessageType.Error)]
		public string ProtoOutputDir;

		[Title("Proto的程序集名称，不 包括扩展名")]
		[Delayed]
		[Required]
		[OnValueChanged("OnDataChanged")]
		[ValidateInput("IsProtoAssemblyNameValid", "Invalid asmdef name", InfoMessageType.Error)]
		public string ProtoAssemblyName;


		[MenuItem("Icy/Proto/Setting", false, 50)]
		public static void Open()
		{
			if (_ProtoSettingWindow != null)
				_ProtoSettingWindow.Close();
			_ProtoSettingWindow = GetWindow<ProtoSettingWindow>();
		}

		protected override void Initialize()
		{
			base.Initialize();
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetSettingDir(), "ProtoSetting.json");
			if (bytes == null)
				_Setting = new ProtoSetting();
			else
			{
				_Setting = ProtoSetting.Parser.ParseFrom(bytes);
				CompileBatPath = _Setting.CompileBatPath;
				ProtoOutputDir = _Setting.ProtoOutputDir;
				ProtoAssemblyName = _Setting.ProtoAssemblyName;
			}
		}

		private bool IsCompileBatPathValid()
		{
			if (!string.IsNullOrEmpty(CompileBatPath))
			{
				string curDir = Directory.GetCurrentDirectory();
				if (!string.IsNullOrEmpty(CompileBatPath) && !File.Exists(Path.Combine(curDir, CompileBatPath)))
					return false;
			}
			return true;
		}

		private bool IsProtoOutputDirValid()
		{
			if (!string.IsNullOrEmpty(ProtoOutputDir))
			{
				string curDir = Directory.GetCurrentDirectory();
				if (!string.IsNullOrEmpty(ProtoOutputDir) && !Directory.Exists(Path.Combine(curDir, ProtoOutputDir)))
					return false;
			}
			return true;
		}

		private bool IsProtoAssemblyNameValid()
		{
			if (!string.IsNullOrEmpty(ProtoAssemblyName))
			{
				string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");

				string asmdefPath = null;
				foreach (string guid in guids)
				{
					string path = AssetDatabase.GUIDToAssetPath(guid);
					string fileName = Path.GetFileNameWithoutExtension(path);

					if (fileName == ProtoAssemblyName)
					{
						asmdefPath = path;
						string asmdefName = $"/{ProtoAssemblyName}.asmdef";
						string asmDefDir = asmdefPath.Remove(asmdefPath.Length - asmdefName.Length);
						if (!string.IsNullOrEmpty(ProtoOutputDir) && asmDefDir != ProtoOutputDir)
							return false;
					}
				}

				if (string.IsNullOrEmpty(asmdefPath))
					return false;
			}
			return true;
		}

		private void OnDataChanged()
		{
			if (!IsCompileBatPathValid())
			{
				EditorUtility.DisplayDialog("", $"找不到 {CompileBatPath} 文件，请检查路径", "OK");
				return;
			}

			if (!IsProtoOutputDirValid())
			{
				EditorUtility.DisplayDialog("", $"找不到 {ProtoOutputDir} 目录，请检查路径", "OK");
				return;
			}

			if (!IsProtoAssemblyNameValid())
			{
				EditorUtility.DisplayDialog("", $"找不到{ProtoAssemblyName}程序集，或程序集没有和ProtoOutputDir在一个目录", "OK");
				return;
			}

			_Setting.CompileBatPath = CompileBatPath;
			_Setting.ProtoOutputDir = ProtoOutputDir;
			_Setting.ProtoAssemblyName = ProtoAssemblyName;

			string targetDir = SettingsHelper.GetSettingDir();
			SettingsHelper.SaveSetting(targetDir, "ProtoSetting.json", _Setting.ToByteArray());
		}
	}
}
