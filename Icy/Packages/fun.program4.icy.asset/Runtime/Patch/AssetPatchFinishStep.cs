using Cysharp.Threading.Tasks;
using Icy.Asset;
using Icy.Base;
using YooAsset;

/// <summary>
/// 资源更新完成，做一些清理工作
/// </summary>
public class AssetPatchFinishStep : ProcedureStep
{
	private AssetPatcher _Patcher;

	public override async UniTask Activate()
	{
		Log.LogInfo($"Activate {nameof(AssetPatchFinishStep)}", nameof(AssetPatcher));
		_Patcher = OwnerProcedure.Blackboard.ReadObject(nameof(AssetPatcher)) as AssetPatcher;
		await Clear();
	}

	public override async UniTask Deactivate()
	{
		await UniTask.CompletedTask;
	}

	private async UniTask Clear()
	{
		ClearCacheFilesOperation operation = _Patcher.Package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
		await operation.ToUniTask();

		EventParam_Bool eventParam = EventManager.GetParam<EventParam_Bool>();
		eventParam.Value = true;
		EventManager.Trigger(EventDefine.AssetPatchFinish, eventParam);

		Log.LogInfo($"AssetPatchFinish, patchs patched", nameof(AssetPatcher));
		Finish();
	}
}
