using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using UnityEditor;

namespace Icy.UI.Editor
{
	/// <summary>
	/// UI相关的设置窗口
	/// </summary>
	public class UISettingWindow : OdinEditorWindow
	{
		private static UISettingWindow _UISettingWindow;

		[InfoBox("UI类文件的根目录")]
		[FolderPath(ParentFolder = "Assets/")]
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
			UIRootPath = LocalPrefs.GetString("_Icy_UIRootPath", "");
		}

		private void OnUIRootPathChanged()
		{
			LocalPrefs.SetString("_Icy_UIRootPath", UIRootPath);
			LocalPrefs.Save();
		}
	}
}
