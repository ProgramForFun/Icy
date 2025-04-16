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
			_Patcher = _Procedure.Blackboard.ReadObject("AssetPatcher") as AssetPatcher;
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
				EventManager.Trigger(EventDefine.AssetPatchFinish, new EventParam_Bool() { Value = false });
				_Procedure.Finish();
			}
			else
			{
				int totalDownloadCount = _Downloader.TotalDownloadCount;
				long totalDownloadBytes = _Downloader.TotalDownloadBytes;
				if (HasEnoughDiskSpace(totalDownloadBytes))
				{
					Ready2DownloadAssetPatchParam param = new Ready2DownloadAssetPatchParam();
					param.About2DownloadBytes = _Downloader.TotalDownloadBytes;
					param.About2DownloadCount = _Downloader.TotalDownloadCount;
					param.StartDownload = Download;
					EventManager.Trigger(EventDefine.Ready2DownloadAssetPatch, param);
				}
				else
					EventManager.Trigger(EventDefine.NotEnoughDiskSpace2PatchAsset, new EventParam<Action>() { Value = PreDownload });
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
			AssetPatchDownloadErrorParam param = new AssetPatchDownloadErrorParam();
			param.PackageName = data.PackageName;
			param.FileName = data.FileName;
			param.ErrorInfo = data.ErrorInfo;
			param.Retry = PreDownload;
			EventManager.Trigger(EventDefine.AssetPatchDownloadError, param);
		}

		/// <summary>
		/// 通知业务侧下载的进度
		/// </summary>
		private void OnDownloadProgressUpdate(DownloadUpdateData data)
		{
			AssetPatchDownloadProgressParam param = new AssetPatchDownloadProgressParam();
			param.PackageName = data.PackageName;
			param.Progress = data.Progress;
			param.TotalDownloadCount = data.TotalDownloadCount;
			param.CurrentDownloadCount = data.CurrentDownloadCount;
			param.TotalDownloadBytes = data.TotalDownloadBytes;
			param.CurrentDownloadBytes = data.CurrentDownloadBytes;
			EventManager.Trigger(EventDefine.AssetPatchDownloadProgress, param);
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
