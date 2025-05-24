using Cysharp.Threading.Tasks;
using Icy.Base;
using YooAsset;

namespace Icy.Asset
{
	/// <summary>
	/// 从远端获取资源热更信息
	/// </summary>
	public class RequestAssetPatchInfoStep : ProcedureStep
	{
		private AssetPatcher _Patcher;
		private string _PackageVersion;

		public override async UniTask Activate()
		{
			Log.LogInfo($"Activate RequestAssetPatchInfoStep", "AssetPatcher");
			_Patcher = OwnerProcedure.Blackboard.ReadObject("AssetPatcher") as AssetPatcher;
			await UpdatePackageVersion();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		private async UniTask UpdatePackageVersion()
		{
			RequestPackageVersionOperation operation = _Patcher.Package.RequestPackageVersionAsync();
			await operation.ToUniTask();

			if (operation.Status == EOperationStatus.Succeed)
			{
				Log.LogInfo($"UpdatePackageVersion succeed", "AssetPatcher");
				_PackageVersion = operation.PackageVersion;
				await UpdatePackageManifest();
			}
			else
				Log.LogError($"UpdatePackageVersion failed, error = {operation.Error}", "AssetPatcher");

			EventParam_Bool eventParam = EventManager.GetParam<EventParam_Bool>();
			eventParam.Value = operation.Status == EOperationStatus.Succeed;
			EventManager.Trigger(EventDefine.RequestAssetPatchInfoEnd, eventParam);
		}

		/// <summary>
		/// 获取资源Manifest
		/// </summary>
		private async UniTask UpdatePackageManifest()
		{
			UpdatePackageManifestOperation operation = _Patcher.Package.UpdatePackageManifestAsync(_PackageVersion);
			await operation.ToUniTask();

			if (operation.Status == EOperationStatus.Succeed)
			{
				Log.LogInfo($"UpdatePackageManifest succeed", "AssetPatcher");
				Finish();
			}
			else
				Log.LogError($"UpdatePackageManifest failed, error = {operation.Error}", "AssetPatcher");

			EventParam_Bool eventParam = EventManager.GetParam<EventParam_Bool>();
			eventParam.Value = operation.Status == EOperationStatus.Succeed;
			EventManager.Trigger(EventDefine.RequestAssetPatchInfoEnd, eventParam);
		}
	}
}
