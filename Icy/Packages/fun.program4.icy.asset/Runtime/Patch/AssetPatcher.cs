using Cysharp.Threading.Tasks;
using Icy.Base;
using YooAsset;

namespace Icy.Asset
{
	/// <summary>
	/// 负责资源的热更新
	/// </summary>
	internal sealed class AssetPatcher
	{
		/// <summary>
		/// 要更新的Package
		/// </summary>
		public ResourcePackage Package { get; internal set; }
		/// <summary>
		/// 是否完成
		/// </summary>
		public bool IsFinished { get; internal set; }

		internal AssetPatcher(ResourcePackage package)
		{
			Package = package;
			IsFinished = false;

			Log.LogInfo($"Start patch procedure", "AssetPatcher");
			Start().Forget();
		}

		private async UniTaskVoid Start()
		{
			Procedure patchProcedure = new Procedure("AssetPatcher");
			patchProcedure.AddStep(new RequestAssetPatchInfoStep());
			patchProcedure.AddStep(new DownloadAssetPatchStep());

			patchProcedure.Blackboard.WriteObject("AssetPatcher", this);
			patchProcedure.Start();

			while(!patchProcedure.IsFinished)
				await UniTask.NextFrame();

			IsFinished = true;
		}
	}
}
