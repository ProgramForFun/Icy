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


using Sirenix.OdinInspector;
using System.IO;
using UnityEditor;
using System.Linq;
using Icy.Base;
using Google.Protobuf;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// Asset Bundle窗口
	/// </summary>
	public class AssetBundleWindow : PlatformWindowBase<AssetBundleWindow>
	{
		[TabGroup("", "Android", SdfIconType.Robot, TextColor = "green")]
		[TabGroup("", "iOS", SdfIconType.Apple)]
		[TabGroup("", "Win64", SdfIconType.Windows, TextColor = "blue")]
		[Title("AssetBundle补丁包列表")]
		[HideLabel]
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		[OnCollectionChanged("OnTableListChanged")]
		public List<AssetBundleWindowItem> _BundleVersionList;

		[FoldoutGroup("打包步骤", Expanded = false)]
		[ReadOnly]
		public List<string> BuildBundleSteps;

		[BoxGroup("AssetBundle选项")]
		[InfoBox("是否打包Bundle  ┃  是否清除缓存、打全量Bundle  ┃  是否加密Bundle", "_ShowAssetBundleOptionsTips")]
		[InlineButton("SwitchAssetBundleOptionsTips", "?")]
		[EnumToggleButtons]
		[OnValueChanged("SaveSetting")]
		public BuildOptionAssetBundle AssetBundleOptions;

		protected bool _ShowAssetBundleOptionsTips = false;
		protected virtual void SwitchAssetBundleOptionsTips() => _ShowAssetBundleOptionsTips = !_ShowAssetBundleOptionsTips;


		/// <summary>
		/// 当前选中平台的Setting文件
		/// </summary>
		protected BuildSetting _Setting;
		/// <summary>
		/// 所有AssetBundle补丁包路径List
		/// </summary>
		protected List<string> _AllBuiltAssetBundleVersion;


		[MenuItem("Icy/Asset/Build AssetBundle", false)]
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
			UpdateBuiltAssetBundleVersion(buildTarget);
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
				//Asset Bundle的打包窗口里，BuildAssetBundle选项不能被取消才合理
				_Setting.BuildAssetBundle = true;

				AssetBundleOptions = 0;
				if (_Setting.BuildAssetBundle)
					AssetBundleOptions |= BuildOptionAssetBundle.BuildAssetBundle;
				if (_Setting.ClearAssetBundleCache)
					AssetBundleOptions |= BuildOptionAssetBundle.ClearAssetBundleCache;
				if (_Setting.EncryptAssetBundle)
					AssetBundleOptions |= BuildOptionAssetBundle.EncryptAssetBundle;
			}

			BuildBundleSteps = new List<string>();
			List<string> allSteps = SubProcedureBuildAssetBundleStep.GetAllStepNamesImpl();
			BuildWindow.SetBuildSteps(BuildBundleSteps, allSteps, 0);

			return _Setting;
		}

		protected virtual void SaveSetting()
		{
			//Asset Bundle的打包窗口里，BuildAssetBundle选项不能被取消才合理
			AssetBundleOptions |= BuildOptionAssetBundle.BuildAssetBundle;

			_Setting.BuildAssetBundle = true;
			_Setting.ClearAssetBundleCache = (AssetBundleOptions & BuildOptionAssetBundle.ClearAssetBundleCache) == BuildOptionAssetBundle.ClearAssetBundleCache;
			_Setting.EncryptAssetBundle = (AssetBundleOptions & BuildOptionAssetBundle.EncryptAssetBundle) == BuildOptionAssetBundle.EncryptAssetBundle;

			string targetDir = SettingsHelper.GetSettingDir();
			SettingsHelper.SaveSetting(targetDir, GetSettingFileName(), _Setting.ToByteArray());
		}

		protected virtual string GetSettingFileName()
		{
			return string.Format($"BuildSetting_{_CurrBuildTarget}.json");
		}

		protected void UpdateBuiltAssetBundleVersion(BuildTarget buildTarget)
		{
			string dir = $"Bundles/{buildTarget}/DefaultPackage";
			if (Directory.Exists(dir))
			{
				_BundleVersionList.Clear();
				_AllBuiltAssetBundleVersion = Directory.GetDirectories(dir).Where(dir => Path.GetFileName(dir) != "OutputCache").ToList();
				for (int i = 0; i < _AllBuiltAssetBundleVersion.Count; i++)
				{
					AssetBundleWindowItem item = new AssetBundleWindowItem();
					item.AssetBundleVersion = _AllBuiltAssetBundleVersion[i];
					_BundleVersionList.Add(item);
				}
			}
			else
				_BundleVersionList = new List<AssetBundleWindowItem>();
		}

		protected void OnTableListChanged(CollectionChangeInfo info, object value)
		{
			if (info.ChangeType == CollectionChangeType.RemoveValue || info.ChangeType == CollectionChangeType.RemoveKey || info.ChangeType == CollectionChangeType.RemoveIndex)
			{
				string dir = _AllBuiltAssetBundleVersion[info.Index];

				if (EditorUtility.DisplayDialog("", $"确定要删除 {dir} 吗？", "是", "否"))
				{
					Directory.Delete(dir, true);
					_AllBuiltAssetBundleVersion.RemoveAt(info.Index);
					UpdateBuiltAssetBundleVersion(_CurrBuildTarget);
				}
				else
					UpdateBuiltAssetBundleVersion(_CurrBuildTarget);
			}
		}

		[Title("打包Asset Bundle")]
		[Button("Build Asset Bundle", ButtonSizes.Large), GUIColor(0, 1, 0)]
		protected virtual void BuildAssetBundle()
		{
			if (_CurrBuildTarget != EditorUserBuildSettings.activeBuildTarget)
			{
				Log.Assert(false, $"打包Bundle未执行；不推荐在打包Bundle时切换BuildTarget平台，请先切换完毕再打包Bundle；\n当前平台 = {EditorUserBuildSettings.activeBuildTarget}, 选择的打包Bundle平台 = {_CurrBuildTarget}");
				return;
			}

			SaveSetting();

			SubProcedureBuildAssetBundleStep.Build(_CurrBuildTarget, _Setting, OnBuildFinish);
		}

		protected void OnBuildFinish(bool succeed)
		{
			UpdateBuiltAssetBundleVersion(_CurrBuildTarget);
		}
	}
}
