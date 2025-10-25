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
using YooAsset;

namespace Icy.Asset
{
	/// <summary>
	/// 负责资源的热更新
	/// </summary>
	internal sealed class AssetPatcher
	{
		/// <summary>
		/// 要更新的Package
		/// </summary>
		public ResourcePackage Package { get; internal set; }
		/// <summary>
		/// 是否完成
		/// </summary>
		public bool IsFinished { get; internal set; }

		internal AssetPatcher(ResourcePackage package)
		{
			Package = package;
			IsFinished = false;

			Log.LogInfo($"Start patch procedure", nameof(AssetPatcher));
		}

		internal async UniTask Start()
		{
			Procedure patchProcedure = new Procedure(nameof(AssetPatcher));
			patchProcedure.AddStep(new RequestAssetPatchInfoStep());
			patchProcedure.AddStep(new DownloadAssetPatchStep());
			patchProcedure.AddStep(new AssetPatchFinishStep());
			patchProcedure.Blackboard.WriteObject(nameof(AssetPatcher), this);
			patchProcedure.Start();

			while(!patchProcedure.IsFinished)
				await UniTask.NextFrame();

			IsFinished = true;
		}
	}
}
