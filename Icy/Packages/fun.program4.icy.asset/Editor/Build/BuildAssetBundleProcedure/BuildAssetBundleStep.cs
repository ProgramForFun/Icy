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
using Cysharp.Threading.Tasks;
using UnityEditor;
using YooAsset.Editor;
using YooAsset;
using System;
using UnityEngine;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 打包AssetBundle，并Copy到StreamingAssets目录下
	/// </summary>
	public class BuildAssetBundleStep : ProcedureStep
	{
		private BuildTarget _BuildTarget;
		private BuildSetting _BuildSetting;
		private static string _BuildPackage;
		private static string _BuildOutputPath;
		private static ScriptableBuildParameters _BuildParam;

		public override async UniTask Activate()
		{
			_BuildTarget = (BuildTarget)OwnerProcedure.Blackboard.ReadInt("BuildTarget");
			_BuildSetting = OwnerProcedure.Blackboard.ReadObject("BuildSetting") as BuildSetting;
			_BuildPackage = OwnerProcedure.Blackboard.ReadString("BuildPackage");

			if (_BuildSetting.BuildAssetBundle)
			{
				bool succeed = await DoBuildAssetBundle(_BuildTarget, _BuildSetting.ClearAssetBundleCache, _BuildSetting.EncryptAssetBundle);
				if (succeed)
				{
					await UniTask.Yield();
					OwnerProcedure.Blackboard.WriteString("BuildOutputPath", _BuildOutputPath);
					OwnerProcedure.Blackboard.WriteObject("BuildParam", _BuildParam);
					Finish();
				}
				else
					OwnerProcedure.Abort();
			}
			else
				Finish();
		}

		public static void BuildAssetBundle(BuildTarget buildTarget, bool clearBuildCacheFiles = false, bool encrypt = true, bool useAssetDependencyDB = true)
		{
			DoBuildAssetBundle(buildTarget, clearBuildCacheFiles, encrypt, useAssetDependencyDB).Forget();
		}

		private static async UniTask<bool> DoBuildAssetBundle(BuildTarget buildTarget, bool clearBuildCacheFiles = false, bool encrypt = true, bool useAssetDependencyDB = true)
		{
			Log.LogInfo($"Start build asset bundle, platform = {buildTarget}");

			string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
			string streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

			// 构建参数
			ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
			buildParameters.BuildOutputRoot = buildOutputRoot;
			buildParameters.BuildinFileRoot = streamingAssetsRoot;
			buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
			buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle; //必须指定资源包类型
			buildParameters.BuildTarget = buildTarget;
			buildParameters.PackageName = _BuildPackage;
			buildParameters.PackageVersion = GetDefaultPackageVersion();
			buildParameters.VerifyBuildingResult = true;
			buildParameters.EnableSharePackRule = true; //启用共享资源构建模式，兼容1.5x版本
			buildParameters.FileNameStyle = EFileNameStyle.HashName;
			buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
			buildParameters.BuildinFileCopyParams = string.Empty;
			buildParameters.CompressOption = ECompressOption.LZ4;
			buildParameters.BuiltinShadersBundleName = GetBuiltInShaderBundleName();
			buildParameters.ClearBuildCacheFiles = clearBuildCacheFiles; //不清理构建缓存，启用增量构建，可以提高打包速度！
			buildParameters.UseAssetDependencyDB = useAssetDependencyDB; //使用资源依赖关系数据库，可以提高打包速度！
			if (encrypt)
				buildParameters.EncryptionServices = new EncryptionOffset();//如果要加密，开启这里，需要提供一个加密的Service

			_BuildParam = buildParameters;

			// 执行构建
			ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
			BuildResult buildResult = pipeline.Run(buildParameters, true);
			if (buildResult.Success)
			{
				Log.LogInfo($"Build asset bundle succeed : {buildResult.OutputPackageDirectory}");
				await UniTask.Yield();
				_BuildOutputPath = buildResult.OutputPackageDirectory;
				return true;
			}
			else
			{
				Log.LogInfo($"Build asset bundle failed : {buildResult.ErrorInfo}");
				_BuildOutputPath = null;
				return false;
			}
		}

		private static string GetDefaultPackageVersion()
		{
			TimeSpan nowSpan = DateTime.Now.TimeOfDay;
			return DateTime.Now.ToString("yyyy-MM-dd") + "_" + Mathf.RoundToInt((float)nowSpan.TotalSeconds);
		}

		/// <summary>
		/// 内置着色器资源包名称
		/// 注意：和自动收集的着色器资源包名保持一致！
		/// </summary>
		private static string GetBuiltInShaderBundleName()
		{
			bool uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
			PackRuleResult packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
			return packRuleResult.GetBundleName(_BuildPackage, uniqueBundleName);
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
