using Google.Protobuf;
using Icy.Base;
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
		private UISetting _Setting;

		[Title("UI类文件的根目录")]
		[FolderPath]
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
			byte[] bytes = IcyFrame.Instance.LoadSettingEditor(IcyFrame.Instance.GetEditorOnlySettingDir(), "UISetting.json");
			if (bytes == null)
				_Setting = new UISetting();
			else
			{
				_Setting = UISetting.Descriptor.Parser.ParseFrom(bytes) as UISetting;
				UIRootPath = _Setting.UIRootDir;
			}
		}

		private void OnUIRootPathChanged()
		{
			_Setting.UIRootDir = UIRootPath;

			string targetDir = IcyFrame.Instance.GetEditorOnlySettingDir();
			IcyFrame.Instance.SaveSetting(targetDir, "UISetting.json", _Setting.ToByteArray());
		}
	}
}
