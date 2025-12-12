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


using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 把框架Setting文件PlayerSetting部分，应用到PlayerSetting
	/// </summary>
	public class ApplyPlayerSettingsStep : BuildStep
	{
		private BuildTarget _BuildTarget;
		private BuildSetting _BuildSetting;

		public override async UniTask Activate()
		{
			_BuildTarget = (BuildTarget)OwnerProcedure.Blackboard.ReadInt("BuildTarget", true);
			_BuildSetting = OwnerProcedure.Blackboard.ReadObject("BuildSetting", true) as BuildSetting;

			if (_BuildSetting != null)
			{
				if (!string.IsNullOrEmpty(_BuildSetting.ApplicationIdentifier))
					PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, _BuildSetting.ApplicationIdentifier);

				if (!string.IsNullOrEmpty(_BuildSetting.ProductName))
					PlayerSettings.productName = _BuildSetting.ProductName;

				if (!string.IsNullOrEmpty(_BuildSetting.CompanyName))
					PlayerSettings.companyName = _BuildSetting.CompanyName;

				if (!string.IsNullOrEmpty(_BuildSetting.BundleVersion))
					PlayerSettings.bundleVersion = _BuildSetting.BundleVersion;

				switch (_BuildTarget)
				{
					case BuildTarget.Android:
						PlayerSettings.Android.bundleVersionCode = _BuildSetting.BundleNumber;
						PlayerSettings.Android.keystorePass = _BuildSetting.KeyStorePassword;
						break;
					case BuildTarget.iOS:
						PlayerSettings.iOS.buildNumber = _BuildSetting.BundleNumber.ToString();
						PlayerSettings.iOS.appleEnableAutomaticSigning = _BuildSetting.AutoSigning;
						break;
					case BuildTarget.StandaloneWindows64:
						break;
					default:
						break;
				}
			}

			EditorUserBuildSettings.exportAsGoogleAndroidProject = _BuildSetting.ExportAndroidProject;

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			await UniTask.CompletedTask;
			Finish();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
