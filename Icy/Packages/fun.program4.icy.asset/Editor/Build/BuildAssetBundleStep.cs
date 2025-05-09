using Icy.Base;
using Cysharp.Threading.Tasks;
using UnityEditor;
using YooAsset.Editor;
using YooAsset;
using System;
using System.IO;
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
		private static string _BuildPackage = "DefaultPackage";
		private static string _BuildOutputPath;

		public override async UniTask Activate()
		{
			_BuildTarget = (BuildTarget)OwnerProcedure.Blackboard.ReadInt("BuildTarget");
			_BuildSetting = OwnerProcedure.Blackboard.ReadObject("BuildSetting") as BuildSetting;

			bool succeed = BuildAssetBundle(_BuildTarget);
			if (succeed)
			{
				await UniTask.Yield();
				ClearStreamingAssetsAndCopyNew();
				await UniTask.Yield();
				Finish();
			}
			else
				OwnerProcedure.Abort();
		}

		private static bool BuildAssetBundle(BuildTarget buildTarget, bool useAssetDependencyDB = true, bool clearBuildCacheFiles = false)
		{
			Log.LogInfo($"Start build asset bundle, platform = {buildTarget}");

			string buildoutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
			string streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();

			// 构建参数
			ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
			buildParameters.BuildOutputRoot = buildoutputRoot;
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
			//buildParameters.EncryptionServices = CreateEncryptionInstance();//如果要加密，开启这里，需要提供一个加密的Service
			buildParameters.CompressOption = ECompressOption.LZ4;
			buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName();
			buildParameters.ClearBuildCacheFiles = clearBuildCacheFiles; //不清理构建缓存，启用增量构建，可以提高打包速度！
			buildParameters.UseAssetDependencyDB = useAssetDependencyDB; //使用资源依赖关系数据库，可以提高打包速度！

			// 执行构建
			ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
			BuildResult buildResult = pipeline.Run(buildParameters, true);
			if (buildResult.Success)
			{
				Log.LogInfo($"Build asset bundle succeed : {buildResult.OutputPackageDirectory}");
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
			int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
			return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
		}

		/// <summary>
		/// 内置着色器资源包名称
		/// 注意：和自动收集的着色器资源包名保持一致！
		/// </summary>
		private static string GetBuiltinShaderBundleName()
		{
			bool uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
			PackRuleResult packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
			return packRuleResult.GetBundleName(_BuildPackage, uniqueBundleName);
		}

		private void ClearStreamingAssetsAndCopyNew()
		{
			string assetDir = Path.Combine(Application.streamingAssetsPath, "yoo", _BuildPackage);
			if (Directory.Exists(assetDir))
				Directory.Delete(assetDir, true);
			CommonUtility.CopyDir(_BuildOutputPath, assetDir);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
