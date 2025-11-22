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
using System;

namespace Icy.Asset
{
	/// <summary>
	/// 负责HybridCLR相关的运行时处理
	/// </summary>
	internal sealed class HybridCLRRunner
	{
		/// <summary>
		/// 是否完成
		/// </summary>
		public bool IsFinished { get; internal set; }
		/// <summary>
		/// 强制加载、解密等完成，调用业务侧传入的热更代码入口的执行
		/// </summary>
		private Action _RunCode;

		internal HybridCLRRunner(Action runCode)
		{
			if (runCode == null)
				Log.Assert(false, "Argument of the constructor is null", nameof(HybridCLRRunner));
			_RunCode = runCode;
			IsFinished = false;
		}

		internal async UniTask Run()
		{
			Procedure patchProcedure = new Procedure(nameof(HybridCLRRunner));
			patchProcedure.AddStep(new LoadPatchDLLStep());
			patchProcedure.AddStep(new LoadMetaDataDLLStep());
			patchProcedure.Start();
			Log.Info($"Start HybridCLR patch procedure", nameof(HybridCLRRunner));

			while(!patchProcedure.IsFinished)
				await UniTask.NextFrame();

			_RunCode();
			IsFinished = true;
		}
	}
}
