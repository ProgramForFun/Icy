using Icy.Base;
using Cysharp.Threading.Tasks;
using UnityEditor;
using System.IO;
using UnityEngine;
using YooAsset.Editor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 打包AssetBundle，并Copy到StreamingAssets目录下
	/// </summary>
	public class CopyAssetBundle2StreamingAssetsStep : ProcedureStep
	{
		public override async UniTask Activate()
		{
			string buildPackage = OwnerProcedure.Blackboard.ReadString("BuildPackage");
			string buildOutputPath = OwnerProcedure.Blackboard.ReadString("BuildOutputPath");
			ScriptableBuildParameters buildParam = OwnerProcedure.Blackboard.ReadObject("BuildParam") as ScriptableBuildParameters;
			ClearStreamingAssetsAndCopyNew(buildPackage, buildOutputPath, buildParam);

			Finish();
			await UniTask.CompletedTask;
		}

		private static void ClearStreamingAssetsAndCopyNew(string buildPackage, string outputPath, ScriptableBuildParameters buildParam)
		{
			string assetDir = Path.Combine(Application.streamingAssetsPath, "yoo", buildPackage);
			if (Directory.Exists(assetDir))
				Directory.Delete(assetDir, true);
			CommonUtility.CopyDir(outputPath, assetDir);

			//Copy BuildInCatalog
			BuildContext buildContext = new BuildContext();
			BuildParametersContext buildParametersContext = new BuildParametersContext(buildParam);
			buildContext.SetContextObject(buildParametersContext);
			IBuildTask createCatalog = new TaskCreateCatalog_SBP();
			createCatalog.Run(buildContext);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
