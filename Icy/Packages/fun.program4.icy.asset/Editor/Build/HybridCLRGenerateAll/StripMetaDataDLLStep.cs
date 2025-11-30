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
using HybridCLR.Editor.AOT;
using System.IO;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 优化补充元数据dll大小；
	/// 具体见 https://www.hybridclr.cn/docs/basic/aotgeneric#%E4%BC%98%E5%8C%96%E8%A1%A5%E5%85%85%E5%85%83%E6%95%B0%E6%8D%AEdll%E5%A4%A7%E5%B0%8F
	/// </summary>
	public class StripMetaDataDLLStep : BuildStep
	{
		public override async UniTask Activate()
		{
			AssetSetting assetSetting = OwnerProcedure.Blackboard.ReadObject(nameof(AssetSetting), true) as AssetSetting;
			Strip(assetSetting);

			await UniTask.CompletedTask;
			Finish();
		}

		protected void Strip(AssetSetting assetSetting)
		{
			string copy2Dir = assetSetting.MetaDataDLLCopyToDir;
			for (int i = 0; i < assetSetting.MetaDataDLLs.Count; i++)
			{
				string path = Path.Combine(copy2Dir, assetSetting.MetaDataDLLs[i] + ".bytes");
				string tmpPath = Path.Combine(copy2Dir, assetSetting.MetaDataDLLs[i] + "_.bytes");
				File.Move(path, tmpPath);
				AOTAssemblyMetadataStripper.Strip(tmpPath, path);
				File.Delete(tmpPath);
			}
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
