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
using System;
using UnityEditor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 资源相关的设置窗口
	/// </summary>
	public class AssetSettingWindow : OdinEditorWindow
	{
		private static AssetSettingWindow _AssetSettingWindow;
		private AssetSetting _Setting;

		[Title("热更新资源Host地址（主）")]
		[DelayedProperty]
		[ValidateInput("IsValidHttpOrHttpsUrl", "Invalid Http(s) address", InfoMessageType.Error)]
		[OnValueChanged("OnAssetHostServerAddressMainChanged")]
		public string AssetHostServerAddressMain;

		[Title("热更新资源Host地址（备）")]
		[DelayedProperty]
		[ValidateInput("IsValidHttpOrHttpsUrl", "Invalid Http(s) address", InfoMessageType.Error)]
		[OnValueChanged("OnAssetHostServerAddressStandbyChanged")]
		public string AssetHostServerAddressStandby;


		[MenuItem("Icy/Asset/Setting", false, 30)]
		public static void Open()
		{
			if (_AssetSettingWindow != null)
				_AssetSettingWindow.Close();
			_AssetSettingWindow = GetWindow<AssetSettingWindow>();
		}

		protected override void Initialize()
		{
			base.Initialize();
			_Setting = GetAssetSetting();
			AssetHostServerAddressMain = _Setting.AssetHostServerAddressMain;
			AssetHostServerAddressStandby = _Setting.AssetHostServerAddressStandby;
		}

		private AssetSetting GetAssetSetting()
		{
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetSettingDir(), "AssetSetting.json");
			if (bytes == null)
				_Setting = new AssetSetting();
			else
				_Setting = AssetSetting.Parser.ParseFrom(bytes);
			return _Setting;
		}

		private void OnAssetHostServerAddressMainChanged()
		{
			OnAssetHostServerAddressChanged(true);
		}

		private void OnAssetHostServerAddressStandbyChanged()
		{
			OnAssetHostServerAddressChanged(false);
		}

		private void OnAssetHostServerAddressChanged(bool isMain)
		{
			if (isMain)
				_Setting.AssetHostServerAddressMain = AssetHostServerAddressMain;
			else
				_Setting.AssetHostServerAddressStandby = AssetHostServerAddressStandby;

			string targetDir = SettingsHelper.GetSettingDir();
			SettingsHelper.SaveSetting(targetDir, "AssetSetting.json", _Setting.ToByteArray());
		}

		private bool IsValidHttpOrHttpsUrl(string url)
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
				return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
			return false;
		}
	}
}
