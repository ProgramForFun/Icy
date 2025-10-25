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
	/// 从远端获取资源热更信息
	/// </summary>
	public class RequestAssetPatchInfoStep : ProcedureStep
	{
		private AssetPatcher _Patcher;
		private string _PackageVersion;

		public override async UniTask Activate()
		{
			Log.Info($"Activate {nameof(RequestAssetPatchInfoStep)}", nameof(AssetPatcher));
			_Patcher = OwnerProcedure.Blackboard.ReadObject(nameof(AssetPatcher), true) as AssetPatcher;
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
				Log.Info($"{nameof(UpdatePackageVersion)} succeed", nameof(AssetPatcher));
				_PackageVersion = operation.PackageVersion;
				await UpdatePackageManifest();
			}
			else
				Log.Error($"{nameof(UpdatePackageVersion)} failed, error = {operation.Error}", nameof(AssetPatcher));

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
				Log.Info($"{nameof(UpdatePackageManifest)} succeed", nameof(AssetPatcher));
				Finish();
			}
			else
				Log.Error($"{nameof(UpdatePackageManifest)} failed, error = {operation.Error}", nameof(AssetPatcher));

			EventParam_Bool eventParam = EventManager.GetParam<EventParam_Bool>();
			eventParam.Value = operation.Status == EOperationStatus.Succeed;
			EventManager.Trigger(EventDefine.RequestAssetPatchInfoEnd, eventParam);
		}
	}
}
