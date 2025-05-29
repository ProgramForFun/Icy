using Icy.Base;
using Cysharp.Threading.Tasks;
using UnityEditor;
using System.IO;
using UnityEngine;

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
			ClearStreamingAssetsAndCopyNew(buildPackage, buildOutputPath);

			Finish();
			await UniTask.CompletedTask;
		}

		private static void ClearStreamingAssetsAndCopyNew(string buildPackage, string outputPath)
		{
			string assetDir = Path.Combine(Application.streamingAssetsPath, "yoo", buildPackage);
			if (Directory.Exists(assetDir))
				Directory.Delete(assetDir, true);
			CommonUtility.CopyDir(outputPath, assetDir);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
