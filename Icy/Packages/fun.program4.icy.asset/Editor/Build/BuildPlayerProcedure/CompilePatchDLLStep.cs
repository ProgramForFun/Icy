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
using System;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 用HybridCLR编译热更DLL
	/// </summary>
	public class CompilePatchDLLStep : BuildStep
	{
		public override async UniTask Activate()
		{
			BuildTarget buildTarget = (BuildTarget)OwnerProcedure.Blackboard.ReadInt("BuildTarget", true);

			bool succeed = Compile(buildTarget);
			if (!succeed)
			{
				OwnerProcedure.Abort();
				return;
			}

			await UniTask.CompletedTask;
			Finish();
		}

		public static bool Compile(BuildTarget buildTarget)
		{
			string[] patchAssembleNames = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblies;
			UnityEditorInternal.AssemblyDefinitionAsset[] patchAsmDefs = HybridCLR.Editor.Settings.HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions;
			if (patchAssembleNames.Length == 0 && patchAsmDefs.Length == 0)
			{
				Log.LogError("CompilePatchDLLStep 未执行，请先在HybridCLR的Setting界面配置热更程序集");
				return false;
			}

			try
			{
				switch (buildTarget)
				{
					case BuildTarget.Android:
						HybridCLR.Editor.Commands.CompileDllCommand.CompileDllAndroid();
						break;
					case BuildTarget.iOS:
						HybridCLR.Editor.Commands.CompileDllCommand.CompileDllIOS();
						break;
					case BuildTarget.StandaloneWindows64:
						HybridCLR.Editor.Commands.CompileDllCommand.CompileDllWin64();
						break;
					default:
						Log.Assert(false, $"CompilePatchDLLStep 未执行，暂不支持的平台 = {buildTarget}");
						return false;
				}
			}
			catch (Exception e)
			{
				Log.Assert(false, "Compile patch DLL failed", nameof(CompilePatchDLLStep));
				Log.LogError(e.ToString());
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
