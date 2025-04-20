using Google.Protobuf;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Icy.UI.Editor
{
	/// <summary>
	/// UI相关的设置窗口
	/// </summary>
	public class UISettingWindow : OdinEditorWindow
	{
		private static UISettingWindow _UISettingWindow;

		[InfoBox("UI类文件的根目录")]
		[FolderPath]
		[OnValueChanged("OnUIRootPathChanged")]
		public string UIRootPath;


		[MenuItem("Icy/UI/Setting")]
		public static void Open()
		{
			if (_UISettingWindow != null)
				_UISettingWindow.Close();
			_UISettingWindow = GetWindow<UISettingWindow>();
		}

		protected override void Initialize()
		{
			base.Initialize();
			string fullPath = Path.Combine(Application.streamingAssetsPath, "IcySettings", "UISetting.bin");
			if (File.Exists(fullPath))
			{
				byte[] bytes = File.ReadAllBytes(fullPath);
				UISetting uiSetting = UISetting.Descriptor.Parser.ParseFrom(bytes) as UISetting;
				UIRootPath = uiSetting.UIRootDir;
			}
		}

		private void OnUIRootPathChanged()
		{
			string targetDir = Path.Combine(Application.streamingAssetsPath, "IcySettings");
			if (!Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);

			UISetting uiSetting = new UISetting();
			uiSetting.UIRootDir = UIRootPath;
			string targetPath = Path.Combine(targetDir, "UISetting.bin");
			File.WriteAllBytes(targetPath, uiSetting.ToByteArray());
		}
	}
}
