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


using Icy.Base;
using Cysharp.Threading.Tasks;
using UnityEditor;
using YooAsset.Editor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 将HybirdCLR编译出来的补充元数据DLL，Copy到StreamingAssets目录下
	/// </summary>
	public class CopyMetaDataDLL2StreamingAssetsStep : BuildStep
	{
		public override async UniTask Activate()
		{
			string buildPackage = OwnerProcedure.Blackboard.ReadString("BuildPackage", true);
			string buildOutputPath = OwnerProcedure.Blackboard.ReadString("BuildOutputPath", true);
			ScriptableBuildParameters buildParam = OwnerProcedure.Blackboard.ReadObject("BuildParam", true) as ScriptableBuildParameters;
			CopyMetaDataDLL(buildPackage, buildOutputPath, buildParam);

			Finish();
			await UniTask.CompletedTask;
		}

		protected virtual void CopyMetaDataDLL(string buildPackage, string outputPath, ScriptableBuildParameters buildParam)
		{

			//AssetDatabase.SaveAssets();
			//AssetDatabase.Refresh();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
