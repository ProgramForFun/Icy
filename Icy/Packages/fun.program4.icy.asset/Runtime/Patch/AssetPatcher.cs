using Icy.Base;
using YooAsset;

namespace Icy.Asset
{
	internal sealed class AssetPatcher
	{
		/// <summary>
		/// 要更新的Package
		/// </summary>
		public ResourcePackage Package { get; internal set; }
		/// <summary>
		/// 是否完成
		/// </summary>
		public bool IsDone { get; internal set; }

		internal AssetPatcher(ResourcePackage package)
		{
			Package = package;
			IsDone = false;

			Log.LogInfo($"Start patch procedure", "AssetPatcher");
			Procedure patchProcedure = new Procedure("AssetPatcher");
			patchProcedure.AddStep(new RequestAssetPatchInfoStep());

			patchProcedure.Blackboard.WriteObject("AssetPatcher", this);
			patchProcedure.Start();
		}
	}
}
