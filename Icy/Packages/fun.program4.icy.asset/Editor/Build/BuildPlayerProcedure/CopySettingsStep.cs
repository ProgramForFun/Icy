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


using UnityEngine;
using Icy.Base;
using Cysharp.Threading.Tasks;
using System.IO;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 把框架Setting文件copy到streamingAssetsPath
	/// </summary>
	public class CopySettingsStep : ProcedureStep
	{
		public override async UniTask Activate()
		{
			string dest = Path.Combine(Application.streamingAssetsPath, SettingsHelper.GetSettingDir());
			bool succeed = CommonUtility.CopyDir(SettingsHelper.GetSettingDir(), dest, false);
			if (!succeed)
			{
				Log.Assert(false, "Copy setting files failed", nameof(CopySettingsStep));
				OwnerProcedure.Abort();
				return;
			}

			await UniTask.CompletedTask;
			Finish();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
