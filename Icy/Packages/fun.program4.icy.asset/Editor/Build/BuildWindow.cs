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


using Icy.Base;
using Sirenix.OdinInspector;
using System;
using System.IO;
using SimpleJSON;
using UnityEditor;
using Google.Protobuf;
using UnityEngine;
using Icy.Editor;
using System.Collections.Generic;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 打包窗口
	/// </summary>
	public class BuildWindow : PlatformWindowBase<BuildWindow>
	{
		[TabGroup("", "Android", SdfIconType.Robot, TextColor = "green")]
		[TabGroup("", "iOS", SdfIconType.Apple)]
		[TabGroup("", "Win64", SdfIconType.Windows, TextColor = "blue")]
		[Title("包名")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("SaveSetting")]
		public string ApplicationIdentifier;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("展示给玩家的游戏名称")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("SaveSetting")]
		public string ProductName;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("公司名")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("SaveSetting")]
		public string CompanyName;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("string版本号（PlayerSettings.bundleVersion）")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("SaveSetting")]
		public string BundleVersion;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[Title("数字版本号（PlayerSettings.Android.bundleVersionCode、PlayerSettings.iOS.buildNumber）")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("SaveSetting")]
		public int BundleNumber;

		[TabGroup("", "Android")]
		[Title("KeyStore密码")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("SaveSetting")]
		public string KeyStorePassword;

		[TabGroup("", "iOS")]
		[Title("自动签名")]
		[ShowInInspector]
		[Delayed]
		[OnValueChanged("SaveSetting")]
		public bool AutoSigning;

		[TabGroup("", "Android")]
		[TabGroup("", "iOS")]
		[TabGroup("", "Win64")]
		[Title("Build输出目录")]
		[FolderPath]
		[OnValueChanged("SaveSetting")]
		public string OutputDir;

		[Title("打包步骤")]
		[ReadOnly]
		public List<string> BuildSteps;

		[BoxGroup("AssetBundle选项")]
		[InfoBox("是否打包Bundle  ┃  是否清除缓存、打全量Bundle  ┃  是否加密Bundle", "_ShowAssetBundleOptionsTips")]
		[InlineButton("SwitchAssetBundleOptionsTips", "?")]
		[EnumToggleButtons]
		[OnValueChanged("SaveSetting")]
		public BuildOptionAssetBundle AssetBundleOptions;

		[BoxGroup("调试选项")]
		[InfoBox("是否打Dev版本  ┃  是否允许调试代码  ┃  是否启动时自动连接Profiler  ┃  是否开启Deep Profiling", "_ShowDevOptionsTips")]
		[InlineButton("SwitchDevOptionsTips", "?")]
		[EnumToggleButtons]
		[OnValueChanged("SaveSetting")]
		public BuildOptionDev DevOptions;

		protected bool _ShowAssetBundleOptionsTips = false;
		protected virtual void SwitchAssetBundleOptionsTips() => _ShowAssetBundleOptionsTips = !_ShowAssetBundleOptionsTips;

		protected bool _ShowDevOptionsTips = false;
		protected virtual void SwitchDevOptionsTips() => _ShowDevOptionsTips = !_ShowDevOptionsTips;

		protected static string BUILD_PLAYER_PROCEDURE_CFG_NAME = "BuildPlayerProcedureCfg.json";
		protected static string ICY_BUILD_PLAYER_PROCEDURE_CFG_PATH = "Packages/fun.program4.icy.asset/Editor/Build/BuildPlayerProcedure/" + BUILD_PLAYER_PROCEDURE_CFG_NAME;


		/// <summary>
		/// 当前选中平台的Setting文件
		/// </summary>
		protected BuildSetting _Setting;


		[MenuItem("Icy/Build &B", false, 1000)]
		public static void Open()
		{
			CreateWindow();
		}

		protected override void Update()
		{
			base.Update();
			if (_Setting == null)
				LoadBuildSetting(_CurrPlatformName);
		}

		protected override void OnChangePlatformTab(string tabName, BuildTarget buildTarget)
		{
			LoadBuildSetting(tabName);
		}

		protected virtual BuildSetting LoadBuildSetting(string tabName)
		{
			byte[] bytes = SettingsHelper.LoadSettingEditor(SettingsHelper.GetSettingDir(), GetSettingFileName());
			if (bytes == null)
				_Setting = new BuildSetting();
			else
				_Setting = BuildSetting.Parser.ParseFrom(bytes);


			if (_Setting != null)
			{
				ApplicationIdentifier = _Setting.ApplicationIdentifier;
				ProductName = _Setting.ProductName;
				CompanyName = _Setting.CompanyName;
				BundleVersion = _Setting.BundleVersion;
				BundleNumber = _Setting.BundleNumber;
				KeyStorePassword = _Setting.KeyStorePassword;
				AutoSigning = _Setting.AutoSigning;
				OutputDir = _Setting.OutputDir;

				DevOptions = 0;
				if (_Setting.DevelopmentBuild)
					DevOptions |= BuildOptionDev.DevelopmentBuild;
				if (_Setting.ScriptDebugging)
					DevOptions |= BuildOptionDev.ScriptDebugging;
				if (_Setting.AutoConnectProfiler)
					DevOptions |= BuildOptionDev.AutoConnectProfiler;
				if (_Setting.DeepProfiling)
					DevOptions |= BuildOptionDev.DeepProfiling;

				AssetBundleOptions = 0;
				if (_Setting.BuildAssetBundle)
					AssetBundleOptions |= BuildOptionAssetBundle.BuildAssetBundle;
				if (_Setting.ClearAssetBundleCache)
					AssetBundleOptions |= BuildOptionAssetBundle.ClearAssetBundleCache;
				if (_Setting.EncryptAssetBundle)
					AssetBundleOptions |= BuildOptionAssetBundle.EncryptAssetBundle;
			}

			BuildSteps.Clear();
			List<string> allSteps = GetAllStepNames();
			for (int i = 0; i < allSteps.Count; i++)
				BuildSteps.Add(allSteps[i]);

			return _Setting;
		}

		protected virtual void SaveSetting()
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

			string targetDir = SettingsHelper.GetSettingDir();
			SettingsHelper.SaveSetting(targetDir, GetSettingFileName(), _Setting.ToByteArray());
		}

		protected virtual string GetSettingFileName()
		{
			return string.Format($"BuildSetting_{_CurrBuildTarget}.json");
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


			Procedure procedure = new Procedure("BuildPlayer");
			List<string> allSteps = GetAllStepNames();
			for (int i = 0; i < allSteps.Count; i++)
			{
				string typeWithNameSpace = allSteps[i];
				Type type = Type.GetType(typeWithNameSpace);
				if (type == null)
				{
					Log.Assert(false, $"Can not find BuildPlayerProcedure step {typeWithNameSpace}");
					return;
				}

				ProcedureStep step = Activator.CreateInstance(type) as ProcedureStep;
				procedure.AddStep(step);
			}

			procedure.Blackboard.WriteInt("BuildTarget", (int)_CurrBuildTarget);
			procedure.Blackboard.WriteObject("BuildSetting", _Setting);
			procedure.OnChangeStep += OnChangeBuildStep;
			procedure.OnFinish += OnBuildPlayerProcedureFinish;
			procedure.Start();
		}

		/// <summary>
		/// 获取所有的打包Player的步骤类名
		/// </summary>
		protected virtual List<string> GetAllStepNames()
		{
			JSONArray jsonArray;
			if (File.Exists(BUILD_PLAYER_PROCEDURE_CFG_NAME))
				jsonArray = JSONNode.Parse(File.ReadAllText(BUILD_PLAYER_PROCEDURE_CFG_NAME)) as JSONArray;
			else
				jsonArray = JSONNode.Parse(File.ReadAllText(ICY_BUILD_PLAYER_PROCEDURE_CFG_PATH)) as JSONArray;

			List<string> rtn = new List<string>(8);
			for (int i = 0; i < jsonArray.Count; i++)
			{
				string typeWithNameSpace = jsonArray[i];
				rtn.Add(typeWithNameSpace);
			}

			return rtn;
		}

		protected virtual void OnChangeBuildStep(ProcedureStep step)
		{
			string info = $"Current build step : {step.GetType().Name}";
			BiProgress.Show("Build Player", info, step.OwnerProcedure.Progress);
		}

		protected virtual void OnBuildPlayerProcedureFinish(bool _)
		{
			BiProgress.Hide();
		}
	}
}
