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
using UnityEditor;

namespace Icy.UI.Editor
{
	/// <summary>
	/// UI相关的设置窗口
	/// </summary>
	public class UISettingWindow : OdinEditorWindow
	{
		private static UISettingWindow _UISettingWindow;
		private UISetting _Setting;

		[Title("UI类文件的根目录")]
		[FolderPath]
		[Required]
		[OnValueChanged("OnUIRootPathChanged")]
		public string UIRootPath;


		[MenuItem("Icy/UI/Setting", false, 40)]
		public static void Open()
		{
			if (_UISettingWindow != null)
				_UISettingWindow.Close();
			_UISettingWindow = GetWindow<UISettingWindow>();
		}

		protected override void Initialize()
		{
			base.Initialize();
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetEditorOnlySettingDir(), "UISetting.json");
			if (bytes == null)
				_Setting = new UISetting();
			else
			{
				_Setting = UISetting.Parser.ParseFrom(bytes);
				UIRootPath = _Setting.UIRootDir;
			}
		}

		private void OnUIRootPathChanged()
		{
			_Setting.UIRootDir = UIRootPath;

			string targetDir = SettingsHelper.GetEditorOnlySettingDir();
			SettingsHelper.SaveSetting(targetDir, "UISetting.json", _Setting.ToByteArray());
		}
	}
}
