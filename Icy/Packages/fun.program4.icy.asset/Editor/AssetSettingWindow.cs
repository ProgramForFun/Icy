using Google.Protobuf;
using Icy.Base;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

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


		[MenuItem("Icy/Asset/Setting")]
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
			string fullPath = Path.Combine(IcyFrame.Instance.GetSettingDir(), "AssetSetting.json");
			if (File.Exists(fullPath))
			{
				byte[] bytes = File.ReadAllBytes(fullPath);
				AssetSetting assetSetting = AssetSetting.Descriptor.Parser.ParseFrom(bytes) as AssetSetting;
				return assetSetting;
			}
			return new AssetSetting();
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

			string targetDir = IcyFrame.Instance.GetSettingDir();
			if (!Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);
			string targetPath = Path.Combine(targetDir, "AssetSetting.json");
			File.WriteAllBytes(targetPath, _Setting.ToByteArray());
		}

		private bool IsValidHttpOrHttpsUrl(string url)
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
				return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
			return false;
		}
	}
}
