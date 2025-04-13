using Cysharp.Threading.Tasks;
using Icy.Base;
using YooAsset;

namespace Icy.Asset
{
	public class RequestPackageVersionStep : ProcedureStep
	{
		public override async UniTask Activate()
		{
			Log.LogInfo($"Activate RequestPackageVersionStep", "RequestPackageVersionStep");
			await UpdatePackageVersion();
		}

		public override UniTask Deactivate()
		{
			return UniTask.CompletedTask;
		}

		private async UniTask UpdatePackageVersion()
		{
			AssetPatcher patcher = _Procedure.Blackboard.ReadObject("AssetPatcher") as AssetPatcher;
			RequestPackageVersionOperation operation = patcher.Package.RequestPackageVersionAsync();
			await operation.ToUniTask();

			if (operation.Status == EOperationStatus.Succeed)
			{
				_Procedure.Blackboard.WriteString("PackageVersion", operation.PackageVersion);
				Finish();
			}
			else
				Log.LogError($"UpdatePackageVersion failed, error = {operation.Error}", "RequestPackageVersionStep");

			EventManager.Trigger(EventDefine.UpdateAssetVersionEnd, new EventParam_Bool() { Value = operation.Status == EOperationStatus.Succeed });
		}
	}
}
