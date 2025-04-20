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

		[InfoBox("热更新资源Host地址")]
		[DelayedProperty]
		[ValidateInput("IsValidHttpOrHttpsUrl", "Invalid Http(s) address", InfoMessageType.Error)]
		[OnValueChanged("OnAssetHostServerAddressChanged")]
		public string AssetHostServerAddress;


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
			string fullPath = Path.Combine(Application.streamingAssetsPath, "IcySettings", "AssetSetting.bin");
			if (File.Exists(fullPath))
			{
				byte[] bytes = File.ReadAllBytes(fullPath);
				AssetSetting uiSetting = AssetSetting.Descriptor.Parser.ParseFrom(bytes) as AssetSetting;
				AssetHostServerAddress = uiSetting.AssetHostServerAddress;
			}
		}

		private void OnAssetHostServerAddressChanged()
		{
			string targetDir = Path.Combine(Application.streamingAssetsPath, "IcySettings");
			if (!Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);

			AssetSetting assetSetting = new AssetSetting();
			assetSetting.AssetHostServerAddress = AssetHostServerAddress;
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
