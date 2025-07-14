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

			Log.LogInfo($"AssetPatchFinish, patches patched", nameof(AssetPatcher));
			Finish();
		}
	}
}
