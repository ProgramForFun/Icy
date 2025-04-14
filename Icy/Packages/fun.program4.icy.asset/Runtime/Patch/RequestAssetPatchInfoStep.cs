using Cysharp.Threading.Tasks;
using Icy.Base;
using YooAsset;

namespace Icy.Asset
{
	public class RequestAssetPatchInfoStep : ProcedureStep
	{
		private AssetPatcher _Patcher;
		private string _PackageVersion;

		public override async UniTask Activate()
		{
			Log.LogInfo($"Activate RequestAssetPatchInfoStep", "RequestAssetPatchInfoStep");
			_Patcher = _Procedure.Blackboard.ReadObject("AssetPatcher") as AssetPatcher;
			await UpdatePackageVersion();
		}

		public override UniTask Deactivate()
		{
			return UniTask.CompletedTask;
		}

		private async UniTask UpdatePackageVersion()
		{
			RequestPackageVersionOperation operation = _Patcher.Package.RequestPackageVersionAsync();
			await operation.ToUniTask();

			if (operation.Status == EOperationStatus.Succeed)
			{
				_PackageVersion = operation.PackageVersion;
				await UpdatePackageManifest();
			}
			else
				Log.LogError($"UpdatePackageVersion failed, error = {operation.Error}", "RequestAssetPatchInfoStep");

			EventManager.Trigger(EventDefine.UpdateAssetVersionEnd, new EventParam_Bool() { Value = operation.Status == EOperationStatus.Succeed });
		}

		private async UniTask UpdatePackageManifest()
		{
			UpdatePackageManifestOperation operation = _Patcher.Package.UpdatePackageManifestAsync(_PackageVersion);
			await operation.ToUniTask();

			if (operation.Status == EOperationStatus.Succeed)
				Finish();
			else
				Log.LogError($"UpdatePackageManifest failed, error = {operation.Error}", "RequestPackageManifestStep");

			EventManager.Trigger(EventDefine.UpdateAssetManifestEnd, new EventParam_Bool() { Value = operation.Status == EOperationStatus.Succeed });
		}
	}
}
