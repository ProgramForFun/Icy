using Cysharp.Threading.Tasks;
using Icy.Base;
using SimpleDiskUtils;
using System;
using YooAsset;

namespace Icy.Asset
{
	/// <summary>
	/// 下载热更资源
	/// </summary>
	public class DownloadAssetPatchStep : ProcedureStep
	{
		private const int DOWNLOADING_MAX_NUM = 10;
		private const int FAILED_TRY_AGAIN = 3;
		private AssetPatcher _Patcher;
		private ResourceDownloaderOperation _Downloader;

		public override async UniTask Activate()
		{
			Log.LogInfo($"Activate RequestAssetPatchInfoStep", "AssetPatcher");
			_Patcher = OwnerProcedure.Blackboard.ReadObject("AssetPatcher") as AssetPatcher;
			PreDownload();
			await UniTask.CompletedTask;
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}

		/// <summary>
		/// 开始下载前的准备工作
		/// </summary>
		private void PreDownload()
		{
			if (_Downloader == null)
				_Downloader = _Patcher.Package.CreateResourceDownloader(DOWNLOADING_MAX_NUM, FAILED_TRY_AGAIN);

			if (_Downloader.TotalDownloadCount == 0)
			{
				Log.LogInfo($"AssetPatchFinish, no assets needs to patch", "AssetPatcher");

				EventParam_Bool eventParam = EventManager.GetParam<EventParam_Bool>();
				eventParam.Value = false;
				EventManager.Trigger(EventDefine.AssetPatchFinish, eventParam);

				OwnerProcedure.Abort();
			}
			else
			{
				int totalDownloadCount = _Downloader.TotalDownloadCount;
				long totalDownloadBytes = _Downloader.TotalDownloadBytes;
				if (HasEnoughDiskSpace(totalDownloadBytes))
				{
					Ready2DownloadAssetPatchParam eventParam = EventManager.GetParam<Ready2DownloadAssetPatchParam>();
					eventParam.About2DownloadBytes = _Downloader.TotalDownloadBytes;
					eventParam.About2DownloadCount = _Downloader.TotalDownloadCount;
					eventParam.StartDownload = Download;
					EventManager.Trigger(EventDefine.Ready2DownloadAssetPatch, eventParam);
				}
				else
				{
					EventParam<Action> eventParam = EventManager.GetParam<EventParam<Action>>();
					eventParam.Value = PreDownload;
					EventManager.Trigger(EventDefine.NotEnoughDiskSpace2PatchAsset, eventParam);
				}
			}
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		private void Download()
		{
			DoDownload().Forget();
		}

		private async UniTaskVoid DoDownload()
		{
			_Downloader.DownloadErrorCallback = OnDownloadError;
			_Downloader.DownloadUpdateCallback = OnDownloadProgressUpdate;
			_Downloader.BeginDownload();
			await _Downloader.ToUniTask();

			// 检测下载结果
			if (_Downloader.Status != EOperationStatus.Succeed)
				return;

			Finish();
		}

		/// <summary>
		/// 下载出错，业务侧接收到事件后，可以调用Retry重试
		/// </summary>
		private void OnDownloadError(DownloadErrorData data)
		{
			AssetPatchDownloadErrorParam eventParam = EventManager.GetParam<AssetPatchDownloadErrorParam>();
			eventParam.PackageName = data.PackageName;
			eventParam.FileName = data.FileName;
			eventParam.ErrorInfo = data.ErrorInfo;
			eventParam.Retry = PreDownload;
			EventManager.Trigger(EventDefine.AssetPatchDownloadError, eventParam);
		}

		/// <summary>
		/// 通知业务侧下载的进度
		/// </summary>
		private void OnDownloadProgressUpdate(DownloadUpdateData data)
		{
			AssetPatchDownloadProgressParam eventParam = EventManager.GetParam<AssetPatchDownloadProgressParam>();
			eventParam.PackageName = data.PackageName;
			eventParam.Progress = data.Progress;
			eventParam.TotalDownloadCount = data.TotalDownloadCount;
			eventParam.CurrentDownloadCount = data.CurrentDownloadCount;
			eventParam.TotalDownloadBytes = data.TotalDownloadBytes;
			eventParam.CurrentDownloadBytes = data.CurrentDownloadBytes;
			EventManager.Trigger(EventDefine.AssetPatchDownloadProgress, eventParam);
		}

		/// <summary>
		/// 是否足够磁盘空间存放更新
		/// </summary>
		private bool HasEnoughDiskSpace(long targetBytes)
		{
			long availableSpaceInMB = DiskUtils.CheckAvailableSpace();
			long targetMB = targetBytes / 1024 / 1024;
			return availableSpaceInMB > targetMB + 50; //再额外冗余50M，以防万一
		}
	}
}
