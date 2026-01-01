/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
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
using System;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 执行HybridCLR的GenerateALl
	/// </summary>
	public class HybridCLRGenerateAllStep : BuildStep
	{
		public override async UniTask Activate()
		{
			bool succeed = GenerateAll();
			if (!succeed)
			{
				OwnerProcedure.Abort();
				return;
			}

			await UniTask.CompletedTask;
			Finish();
		}

		public static bool GenerateAll()
		{
			try
			{
				HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
			}
			catch(Exception e)
			{
				Log.Assert(false, "HybridCLR GenerateAll failed", nameof(HybridCLRGenerateAllStep));
				Log.Error(e.ToString());
				return false;
			}
			return true;
		}


		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
