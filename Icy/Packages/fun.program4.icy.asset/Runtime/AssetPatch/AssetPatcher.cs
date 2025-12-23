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
using System;
using YooAsset;

namespace Icy.Asset
{
	/// <summary>
	/// 负责资源的热更新
	/// </summary>
	public sealed class AssetPatcher
	{
		/// <summary>
		/// 要更新的Package
		/// </summary>
		public ResourcePackage Package { get; internal set; }
		/// <summary>
		/// 是否完成
		/// </summary>
		public bool IsFinished { get; internal set; }
		/// <summary>
		/// 从远端更新资源版本信息结束
		/// </summary>
		public event Action<EventParam_Reuslt> OnRequestAssetPatchInfoEnd;
		/// <summary>
		/// 磁盘空间不足以更新资源
		/// </summary>
		public event Action<EventParam<Action>> OnNotEnoughDiskSpace2PatchAsset;
		/// <summary>
		/// 
		/// </summary>
		public event Action<Ready2DownloadAssetPatchParam> OnReady2DownloadAssetPatch;

		internal AssetPatcher(ResourcePackage package)
		{
			Package = package;
			IsFinished = false;
		}

		internal async UniTask Start()
		{
			Procedure patchProcedure = new Procedure(nameof(AssetPatcher));
			patchProcedure.AddStep(new RequestAssetPatchInfoStep());
			patchProcedure.AddStep(new DownloadAssetPatchStep());
			patchProcedure.AddStep(new AssetPatchFinishStep());
			patchProcedure.Blackboard.WriteObject(nameof(AssetPatcher), this);
			patchProcedure.Start();
			Log.Info($"Start asset patch procedure", nameof(AssetPatcher), true);

			while (!patchProcedure.IsFinished)
				await UniTask.NextFrame();

			IsFinished = true;
		}

		internal void TriggerRequestAssetPatchInfoEnd(bool succeed, string error)
		{
			EventParam_Reuslt eventParam = EventManager.GetParam<EventParam_Reuslt>();
			eventParam.Succeed = succeed;
			eventParam.Error = error;
			OnRequestAssetPatchInfoEnd?.Invoke(eventParam);
		}

		internal void TriggerNotEnoughDiskSpace2PatchAsset(Action retry)
		{
			EventParam<Action> eventParam = EventManager.GetParam<EventParam<Action>>();
			eventParam.Value = retry;
			OnNotEnoughDiskSpace2PatchAsset?.Invoke(eventParam);
		}

		internal void TriggerReady2DownloadAssetPatch(long totalBytes, int totalCount, Action startDownload)
		{
			Ready2DownloadAssetPatchParam eventParam = EventManager.GetParam<Ready2DownloadAssetPatchParam>();
			eventParam.About2DownloadBytes = totalBytes;
			eventParam.About2DownloadCount = totalCount;
			eventParam.StartDownload = startDownload;
			OnReady2DownloadAssetPatch?.Invoke(eventParam);
		}
	}
}
