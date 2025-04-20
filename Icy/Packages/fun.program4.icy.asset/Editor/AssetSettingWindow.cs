using Google.Protobuf;
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
			AssetSetting assetSetting = GetAssetSetting();
			if (assetSetting != null)
			{
				AssetHostServerAddressMain = assetSetting.AssetHostServerAddressMain;
				AssetHostServerAddressStandby = assetSetting.AssetHostServerAddressStandby;
			}
		}

		private AssetSetting GetAssetSetting()
		{
			string fullPath = Path.Combine(Application.streamingAssetsPath, "IcySettings", "AssetSetting.bin");
			if (File.Exists(fullPath))
			{
				byte[] bytes = File.ReadAllBytes(fullPath);
				AssetSetting assetSetting = AssetSetting.Descriptor.Parser.ParseFrom(bytes) as AssetSetting;
				return assetSetting;
			}
			return null;
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
			string targetDir = Path.Combine(Application.streamingAssetsPath, "IcySettings");
			if (!Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);

			AssetSetting assetSetting = GetAssetSetting();
			if (assetSetting == null)
				assetSetting = new AssetSetting();
			else
			{
				if (isMain)
					assetSetting.AssetHostServerAddressMain = AssetHostServerAddressMain;
				else
					assetSetting.AssetHostServerAddressStandby = AssetHostServerAddressStandby;
			}
			string targetPath = Path.Combine(targetDir, "AssetSetting.bin");
			File.WriteAllBytes(targetPath, assetSetting.ToByteArray());
		}

		private bool IsValidHttpOrHttpsUrl(string url)
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
				return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
			return false;
		}
	}
}
