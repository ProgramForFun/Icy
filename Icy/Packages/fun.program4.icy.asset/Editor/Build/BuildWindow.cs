using Icy.Base;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.IO;
using SimpleJSON;
using UnityEditor;
using Google.Protobuf;
using System.Linq;
using UnityEngine;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 打包窗口
	/// </summary>
	public class BuildWindow : OdinEditorWindow
	{
		private static BuildWindow _BuildWindow;
		private BuildSetting _Setting;

		[TabGroup("", "Android", SdfIconType.Robot, TextColor = "green")]
		[TabGroup("", "iOS", SdfIconType.Apple)]
		[TabGroup("", "Win64", SdfIconType.Windows, TextColor = "blue")]
		[Title("包名")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("OnSettingChanged")]
		public string ApplicationIdentifier;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("展示给玩家的游戏名称")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("OnSettingChanged")]
		public string ProductName;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("公司名")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("OnSettingChanged")]
		public string CompanyName;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("版本号")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("OnSettingChanged")]
		public string Version;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[ShowInInspector]
		[OnValueChanged("OnSettingChanged")]
		public bool DevelopmentBuild = false;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("Build输出目录")]
		[FolderPath]
		[OnValueChanged("OnSettingChanged")]
		public string OutputDir;

		private InspectorProperty _TabGroupProperty;
		private string _CurrPlatform;


		[MenuItem("Icy/Build")]
		public static void Open()
		{
			if (_BuildWindow != null)
				_BuildWindow.Close();
			_BuildWindow = GetWindow<BuildWindow>();
		}

		private void Update()
		{
			if (_TabGroupProperty == null)
			{
				if (PropertyTree != null && PropertyTree.RootProperty != null)
					_TabGroupProperty = PropertyTree.RootProperty.Children.FirstOrDefault(p => p.Attributes.HasAttribute<TabGroupAttribute>());
			}

			if (_TabGroupProperty != null)
			{
				string currTabName = _TabGroupProperty.State.Get<string>("CurrentTabName");
				if (_CurrPlatform != currTabName)
				{
					_CurrPlatform = currTabName;
					OnTabChanged(currTabName);
				}
			}
		}

		private void OnTabChanged(string tabName)
		{
			Log.LogInfo($"Switch to platform {tabName}");
			BuildSetting buildSetting = GetBuildSetting(tabName);
			if (buildSetting != null)
			{
				ApplicationIdentifier = buildSetting.ApplicationIdentifier;
				ProductName = buildSetting.ProductName;
				CompanyName = buildSetting.CompanyName;
				Version = buildSetting.Version;
				OutputDir = buildSetting.OutputDir;
			}
		}

		private BuildSetting GetBuildSetting(string platform)
		{
			//switch (platform)
			//{
			//	case "Android":
			//		break;
			//	case "iOS":
			//		break;
			//	case "Win64":
			//		break;
			//	default:
			//		Log.Assert(false, $"Unsupported platform {platform}");
			//		break;
			//}

			string fullPath = Path.Combine(IcyFrame.Instance.GetEditorOnlySettingDir(), $"BuildSetting{platform}.bin");
			if (File.Exists(fullPath))
			{
				byte[] bytes = File.ReadAllBytes(fullPath);
				_Setting = BuildSetting.Descriptor.Parser.ParseFrom(bytes) as BuildSetting;
				return _Setting;
			}
			else
				_Setting = new BuildSetting();
			return _Setting;
		}

		private void OnSettingChanged()
		{
			_Setting.ApplicationIdentifier = ApplicationIdentifier;
			_Setting.ProductName = ProductName;
			_Setting.CompanyName = CompanyName;
			_Setting.Version = Version;
			_Setting.OutputDir = OutputDir;
			SaveSetting();
		}

		private void SaveSetting()
		{
			string targetDir = IcyFrame.Instance.GetEditorOnlySettingDir();
			if (!Directory.Exists(targetDir))
				Directory.CreateDirectory(targetDir);
			string targetPath = Path.Combine(targetDir, $"BuildSetting{_CurrPlatform}.bin");
			File.WriteAllBytes(targetPath, _Setting.ToByteArray());
		}

		[Title("打包")]
		[Button("Build", ButtonSizes.Large), GUIColor(0, 1, 0)]
		private void Build()
		{
			SaveSetting();

			JSONArray jsonArray;
			if (Directory.Exists("BuildProcedure"))
				jsonArray = JSONArray.Parse(File.ReadAllText("BuildProcedure")) as JSONArray;
			else
				jsonArray = JSONArray.Parse(File.ReadAllText("Packages/fun.program4.icy.asset/Editor/Build/BuildProcedure.json")) as JSONArray;


			Procedure procedure = new Procedure("Build");
			for (int i = 0; i < jsonArray.Count; i++)
			{
				string typeWithNameSpace = jsonArray[i];
				Type type = Type.GetType(typeWithNameSpace);
				if (type == null)
				{
					Log.Assert(false, $"Can not find BuildProcedure step {typeWithNameSpace}");
					return;
				}

				ProcedureStep step = Activator.CreateInstance(type) as ProcedureStep;
				procedure.AddStep(step);
			}
			procedure.Blackboard.WriteObject("BuildSetting", _Setting);
			procedure.Start();
		}
	}
}
