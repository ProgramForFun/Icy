/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using Cysharp.Threading.Tasks;
using Icy.Base;
using SimpleDiskUtils;
using System;
using UnityEngine;
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
			_Patcher = OwnerProcedure.Blackboard.ReadObject(nameof(AssetPatcher), true) as AssetPatcher;
			PrepareToDownload();
			await UniTask.CompletedTask;
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}

		/// <summary>
		/// 开始下载前的准备工作
		/// </summary>
		private void PrepareToDownload()
		{
			if (_Downloader == null)
				_Downloader = _Patcher.Package.CreateResourceDownloader(DOWNLOADING_MAX_NUM, FAILED_TRY_AGAIN);

			if (_Downloader.TotalDownloadCount == 0)
			{
				Log.Info($"{nameof(DownloadAssetPatchStep)} abort, no assets need to patch", nameof(AssetPatcher), true);
				AssetManager.Instance.AssetPatcher.TriggerAssetPatchEnd(false);
				OwnerProcedure.Abort();
			}
			else
			{
				int totalDownloadCount = _Downloader.TotalDownloadCount;
				long totalDownloadBytes = _Downloader.TotalDownloadBytes;
				if (HasEnoughDiskSpace(totalDownloadBytes))
				{
					Log.Info($"Ready to download patch {CommonUtility.FormatWithCommas(totalDownloadBytes)} bytes", nameof(AssetPatcher), true);

					int downloadConfirmThesholdMB = AssetManager.Instance.AssetSetting.AssetDownloadConfirmTheshold;
					if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork
						&& totalDownloadBytes > downloadConfirmThesholdMB * 1024 * 1024) //如果更新大小在设置阈值以内，即使是没有WIFI也直接下载
					{
						AssetManager.Instance.AssetPatcher.TriggerReady2DownloadAssetPatch(totalDownloadBytes, totalDownloadCount, Download);
					}
					else
						Download();
				}
				else
				{
					Log.Error($"Disk space not enough", nameof(AssetPatcher));
					AssetManager.Instance.AssetPatcher.TriggerNotEnoughDiskSpace2PatchAsset(PrepareToDownload);
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
			try
			{
				_Downloader.BeginDownload();
				await _Downloader.ToUniTask();
			}
			catch(Exception)
			{
				//Log和处理在下边OnDownloadError
			}

			// 检测下载结果
			if (_Downloader.Status != EOperationStatus.Succeed)
				return;

			Log.Info($"{nameof(DownloadAssetPatchStep)} succeed", nameof(AssetPatcher), true);
			Finish();
		}

		/// <summary>
		/// 下载出错，业务侧接收到事件后，可以调用Retry重试
		/// </summary>
		private void OnDownloadError(DownloadErrorData data)
		{
			AssetManager.Instance.AssetPatcher.TriggerDownloadError(data.PackageName, data.FileName, data.ErrorInfo, PrepareToDownload);
			Log.Error($"Asset download failed, package = {data.PackageName}, file = {data.FileName}, error = {data.ErrorInfo}", nameof(AssetPatcher));
		}

		/// <summary>
		/// 通知业务侧下载的进度
		/// </summary>
		private void OnDownloadProgressUpdate(DownloadUpdateData data)
		{
			AssetManager.Instance.AssetPatcher.TriggerDownloadProgressUpdate(data.PackageName, data.Progress, data.TotalDownloadCount
																			, data.CurrentDownloadCount, data.TotalDownloadBytes, data.CurrentDownloadBytes);
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
