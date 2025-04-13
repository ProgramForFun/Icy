using Cysharp.Threading.Tasks;
using Icy.Base;
using YooAsset;

namespace Icy.Asset
{
	public class RequestPackageManifestStep : ProcedureStep
	{
		public override async UniTask Activate()
		{
			Log.LogInfo($"Activate RequestPackageManifestStep", "RequestPackageManifestStep");
			await UpdatePackageManifest();
		}

		public override UniTask Deactivate()
		{
			return UniTask.CompletedTask;
		}

		private async UniTask UpdatePackageManifest()
		{
			AssetPatcher patcher = _Procedure.Blackboard.ReadObject("AssetPatcher") as AssetPatcher;
			string packageVersion = _Procedure.Blackboard.ReadString("PackageVersion");
			UpdatePackageManifestOperation operation = patcher.Package.UpdatePackageManifestAsync(packageVersion);
			await operation.ToUniTask();

			if (operation.Status == EOperationStatus.Succeed)
				Finish();
			else
				Log.LogError($"UpdatePackageManifest failed, error = {operation.Error}", "RequestPackageManifestStep");

			EventManager.Trigger(EventDefine.UpdateAssetManifestEnd, new EventParam_Bool() { Value = operation.Status == EOperationStatus.Succeed });
		}
	}
}
