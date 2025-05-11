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
		protected static BuildWindow _BuildWindow;

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
		[Title("string版本号（PlayerSettings.bundleVersion）")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("OnSettingChanged")]
		public string BundleVersion;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[Title("数字版本号（PlayerSettings.Android.bundleVersionCode、PlayerSettings.iOS.buildNumber）")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("OnSettingChanged")]
		public int BundleNumber;

		[TabGroup("", "Android")]
		[Title("KeyStore密码")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("OnSettingChanged")]
		public string KeyStorePassword;

		[TabGroup("", "iOS")]
		[Title("自动签名")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("OnSettingChanged")]
		public bool AutoSigning;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("Build输出目录")]
		[FolderPath]
		[OnValueChanged("OnSettingChanged")]
		public string OutputDir;

		//[InlineButton("A", "?")]
		[Title("AssetBundle选项")]
		[EnumToggleButtons]
		[OnValueChanged("OnSettingChanged")]
		public BuildOptionAssetBundle AssetBundleOptions;

		[Title("调试选项")]
		[EnumToggleButtons]
		[OnValueChanged("OnSettingChanged")]
		public BuildOptionDev DevOptions;

		//protected void A(){}


		/// <summary>
		/// 当前选中平台的BuildTarget
		/// </summary>
		protected BuildTarget _CurrBuildTarget;
		/// <summary>
		/// 当前选中平台的Setting文件
		/// </summary>
		protected BuildSetting _Setting;
		/// <summary>
		/// Odin Tab组件
		/// </summary>
		protected InspectorProperty _TabGroupProperty;
		/// <summary>
		/// 当前选中平台的名字
		/// </summary>
		protected string _CurrPlatformName;


		[MenuItem("Icy/Build &B", false, 1000)]
		public static void Open()
		{
			if (_BuildWindow != null)
				_BuildWindow.Close();
			_BuildWindow = GetWindow<BuildWindow>();
		}

		protected virtual void Update()
		{
#pragma warning disable CS0618
			if (_TabGroupProperty == null)
			{
				if (PropertyTree != null && PropertyTree.RootProperty != null)
					_TabGroupProperty = PropertyTree.RootProperty.Children.FirstOrDefault(p => p.Attributes.HasAttribute<TabGroupAttribute>());
			}
#pragma warning restore CS0618

			if (_TabGroupProperty != null)
			{
				string currTabName = _TabGroupProperty.State.Get<string>("CurrentTabName");
				if (_CurrPlatformName != currTabName)
				{
					_CurrPlatformName = currTabName;
					OnTabChanged(currTabName);
				}
			}
		}

		protected virtual void OnTabChanged(string tabName)
		{
			Log.LogInfo($"Switch to platform {tabName}", "BuildWindow");
			BuildSetting buildSetting = GetBuildSetting(tabName);
			if (buildSetting != null)
			{
				ApplicationIdentifier = buildSetting.ApplicationIdentifier;
				ProductName = buildSetting.ProductName;
				CompanyName = buildSetting.CompanyName;
				BundleVersion = buildSetting.BundleVersion;
				BundleNumber = buildSetting.BundleNumber;
				KeyStorePassword = buildSetting.KeyStorePassword;
				AutoSigning = buildSetting.AutoSigning;
				OutputDir = buildSetting.OutputDir;

				if (buildSetting.DevelopmentBuild)
					DevOptions |= BuildOptionDev.DevelopmentBuild;
				if (buildSetting.ScriptDebugging)
					DevOptions |= BuildOptionDev.ScriptDebugging;
				if (buildSetting.AutoConnectProfiler)
					DevOptions |= BuildOptionDev.AutoConnectProfiler;
				if (buildSetting.DeepProfiling)
					DevOptions |= BuildOptionDev.DeepProfiling;

				if (buildSetting.BuildAssetBundle)
					AssetBundleOptions |= BuildOptionAssetBundle.BuildAssetBundle;
				if (buildSetting.ClearAssetBundleCache)
					AssetBundleOptions |= BuildOptionAssetBundle.ClearAssetBundleCache;
				if (buildSetting.EncryptAssetBundle)
					AssetBundleOptions |= BuildOptionAssetBundle.EncryptAssetBundle;
			}
		}

		protected virtual BuildSetting GetBuildSetting(string platform)
		{
			switch (platform)
			{
				case "Android":
					_CurrBuildTarget = BuildTarget.Android;
					break;
				case "iOS":
					_CurrBuildTarget = BuildTarget.iOS;
					break;
				case "Win64":
					_CurrBuildTarget = BuildTarget.StandaloneWindows64;
					break;
				default:
					Log.Assert(false, $"Unsupported platform {platform}");
					break;
			}

			byte[] bytes = IcyFrame.Instance.LoadSettingEditor(IcyFrame.Instance.GetSettingDir(), $"BuildSetting{platform}.json");
			if (bytes == null)
				_Setting = new BuildSetting();
			else
				_Setting = BuildSetting.Descriptor.Parser.ParseFrom(bytes) as BuildSetting;

			return _Setting;
		}

		protected virtual void OnSettingChanged()
		{
			_Setting.ApplicationIdentifier = ApplicationIdentifier;
			_Setting.ProductName = ProductName;
			_Setting.CompanyName = CompanyName;
			_Setting.BundleVersion = BundleVersion;
			_Setting.BundleNumber = BundleNumber;
			_Setting.KeyStorePassword = KeyStorePassword.ToString();
			_Setting.AutoSigning = AutoSigning;
			_Setting.OutputDir = OutputDir;

			_Setting.DevelopmentBuild = (DevOptions & BuildOptionDev.DevelopmentBuild) == BuildOptionDev.DevelopmentBuild;
			_Setting.ScriptDebugging = (DevOptions & BuildOptionDev.ScriptDebugging) == BuildOptionDev.ScriptDebugging;
			_Setting.AutoConnectProfiler = (DevOptions & BuildOptionDev.AutoConnectProfiler) == BuildOptionDev.AutoConnectProfiler;
			_Setting.DeepProfiling = (DevOptions & BuildOptionDev.DeepProfiling) == BuildOptionDev.DeepProfiling;

			_Setting.BuildAssetBundle = (AssetBundleOptions & BuildOptionAssetBundle.BuildAssetBundle) == BuildOptionAssetBundle.BuildAssetBundle;
			_Setting.ClearAssetBundleCache = (AssetBundleOptions & BuildOptionAssetBundle.ClearAssetBundleCache) == BuildOptionAssetBundle.ClearAssetBundleCache;
			_Setting.EncryptAssetBundle = (AssetBundleOptions & BuildOptionAssetBundle.EncryptAssetBundle) == BuildOptionAssetBundle.EncryptAssetBundle;

			SaveSetting();
		}

		protected virtual void SaveSetting()
		{
			string targetDir = IcyFrame.Instance.GetSettingDir();
			IcyFrame.Instance.SaveSetting(targetDir, $"BuildSetting{_CurrPlatformName}.json", _Setting.ToByteArray());
		}

		[Title("打包")]
		[Button("Build", ButtonSizes.Large), GUIColor(0, 1, 0)]
		protected virtual void Build()
		{
			if (_CurrBuildTarget != EditorUserBuildSettings.activeBuildTarget)
			{
				Log.Assert(false, $"打包未执行；不推荐在打包时切换BuildTarget平台，请先切换完毕再打包；\n当前平台 = {EditorUserBuildSettings.activeBuildTarget}, 选择的打包平台 = {_CurrBuildTarget}");
				return;
			}

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

			procedure.Blackboard.WriteInt("BuildTarget", (int)_CurrBuildTarget);
			procedure.Blackboard.WriteObject("BuildSetting", _Setting);
			procedure.OnChangeStep += OnChangeBuildStep;
			procedure.OnFinish += OnBuildProcedureFinish;
			procedure.Start();
		}

		protected virtual void OnChangeBuildStep(ProcedureStep step)
		{
			string info = $"Current build step : {step.GetType().Name}";
			EditorUtility.DisplayProgressBar("Build Player", info, step.OwnerProcedure.Progress);
		}

		protected virtual void OnBuildProcedureFinish(bool _)
		{
			EditorUtility.ClearProgressBar();
		}
	}
}
