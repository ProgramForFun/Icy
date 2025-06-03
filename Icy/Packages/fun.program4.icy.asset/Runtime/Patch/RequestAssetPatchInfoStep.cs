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
			Log.LogInfo($"Activate {nameof(RequestAssetPatchInfoStep)}", nameof(AssetPatcher));
			_Patcher = OwnerProcedure.Blackboard.ReadObject(nameof(AssetPatcher)) as AssetPatcher;
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
				Log.LogInfo($"{nameof(UpdatePackageVersion)} succeed", nameof(AssetPatcher));
				_PackageVersion = operation.PackageVersion;
				await UpdatePackageManifest();
			}
			else
				Log.LogError($"{nameof(UpdatePackageVersion)} failed, error = {operation.Error}", nameof(AssetPatcher));

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
				Log.LogInfo($"{nameof(UpdatePackageManifest)} succeed", nameof(AssetPatcher));
				Finish();
			}
			else
				Log.LogError($"{nameof(UpdatePackageManifest)} failed, error = {operation.Error}", nameof(AssetPatcher));

			EventParam_Bool eventParam = EventManager.GetParam<EventParam_Bool>();
			eventParam.Value = operation.Status == EOperationStatus.Succeed;
			EventManager.Trigger(EventDefine.RequestAssetPatchInfoEnd, eventParam);
		}
	}
}
