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
using System.Collections.Generic;
using UnityEngine;

namespace Icy.Asset
{
	/// <summary>
	/// 加载补充元数据DLL
	/// </summary>
	public class LoadMetaDataDLLStep : ProcedureStep
	{

		public override async UniTask Activate()
		{
			await Load();
			Finish();
		}

		protected async UniTask Load()
		{
			AssetSetting assetSetting = AssetManager.Instance.AssetSetting;
			string metaDataDLLDir = assetSetting.MetaDataDLLCopyToDir;

			List<AssetRef> allMetaDataDLLRefs = new List<AssetRef>();
			List<UniTask> allMetaDataDLLRefUniTasks = new List<UniTask>();
			LoadPatchDLLStep.LoadDLLs(metaDataDLLDir, assetSetting.MetaDataDLLs, allMetaDataDLLRefs, allMetaDataDLLRefUniTasks);

			await UniTask.WhenAll(allMetaDataDLLRefUniTasks);

			//https://www.hybridclr.cn/docs/basic/aotgeneric
			for (int i = 0; i < allMetaDataDLLRefs.Count; i++)
			{
				TextAsset bytes = allMetaDataDLLRefs[i].AssetObject as TextAsset;
				//HybirdCLR支持在worker线程加载，避免卡顿
				await UniTask.RunOnThreadPool(() => { HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(bytes.bytes, HybridCLR.HomologousImageMode.SuperSet); });
			}

			//HybridCLR内部会复制一份，外部的可以直接释放掉了
			for (int i = 0; i < allMetaDataDLLRefs.Count; i++)
				allMetaDataDLLRefs[i].Release();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
