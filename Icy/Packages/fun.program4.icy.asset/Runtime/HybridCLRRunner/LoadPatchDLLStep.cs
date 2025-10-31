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
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Icy.Asset
{
	/// <summary>
	/// 加载热更DLL
	/// </summary>
	public class LoadPatchDLLStep : ProcedureStep
	{
		public override async UniTask Activate()
		{
			AssetSetting assetSetting = AssetManager.Instance.AssetSetting;
			string patchDLLDir = assetSetting.PatchDLLCopyToDir;

			List<AssetRef> allPatchDLLRefs = new List<AssetRef>();
			List<UniTask> allPatchDLLRefUniTasks = new List<UniTask>();
			for (int i = 0; i < assetSetting.PatchDLLs.Count; i++)
			{
				string patchDLLPath;
				if (AssetManager.Instance.IsAddressable)
					patchDLLPath = assetSetting.PatchDLLs[i];
				else
					patchDLLPath = Path.Combine(patchDLLDir, assetSetting.PatchDLLs[i]);

				AssetRef assetRef = AssetManager.Instance.LoadAssetAsync(patchDLLPath);
				assetRef.Retain();
				allPatchDLLRefs.Add(assetRef);
				allPatchDLLRefUniTasks.Add(assetRef.ToUniTask());
			}

			await UniTask.WhenAll(allPatchDLLRefUniTasks);

			//按顺序加载，由用户在Icy/Asset/Setting中根据热更DLL的依赖关系，编辑的顺序
			//https://www.hybridclr.cn/docs/basic/runhotupdatecodes
			for (int i = 0; i < allPatchDLLRefs.Count; i++)
			{
				//TextAsset bytes = allPatchDLLRefs[i].AssetObject as TextAsset;
				////HybirdCLR支持在worker线程加载，避免卡顿
				//await UniTask.RunOnThreadPool(() => { Assembly.Load(bytes.bytes); });
			}

			//HybridCLR内部会复制一份，外部的可以直接释放掉了
			for (int i = 0; i < allPatchDLLRefs.Count; i++)
				allPatchDLLRefs[i].Release();

			Finish();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
