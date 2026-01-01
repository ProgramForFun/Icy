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

using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Reflection;

namespace Icy.Asset
{
	/// <summary>
	/// 负责HybridCLR相关的运行时处理
	/// </summary>
	public sealed class HybridCLRRunner
	{
		/// <summary>
		/// HybridCLR是否已启用
		/// </summary>
		public static bool IsHybridCLREnabled { get; internal set; }
		/// <summary>
		/// 是否完成（不包括_RunPatchCode回调的执行）
		/// </summary>
		public bool IsFinished { get; internal set; }
		/// <summary>
		/// HybridCLRRunner运行完成的事件（不包括_RunPatchCode回调的执行）
		/// </summary>
		public event Action OnFinish;
		/// <summary>
		/// 强制加载、解密等完成，调用业务侧传入的热更代码入口的执行
		/// </summary>
		private Action _RunPatchCode;


		internal HybridCLRRunner(Action runPatchCode)
		{
			if (runPatchCode == null)
				Log.Assert(false, "Argument of the constructor is null", nameof(HybridCLRRunner));
			_RunPatchCode = runPatchCode;
			IsFinished = false;
		}

		/// <summary>
		/// 获取HybridCLR是否已启用；
		/// Editor下直接读取HybridCLR的设置，运行时根据编译时存储的文件来确定
		/// </summary>
		internal static void DetermineWhetherHybridCLRIsEnabled()
		{
			IsHybridCLREnabled = false;

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
#if UNITY_EDITOR
				if (assembly.GetName().Name == "HybridCLR.Editor")
				{
					try
					{
						Type type = assembly.GetType("HybridCLR.Editor.Settings.HybridCLRSettings");
						PropertyInfo instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
						object instance = instanceProperty.GetValue(null);
						FieldInfo enableField = type.GetField("enable", BindingFlags.Public | BindingFlags.Instance);
						IsHybridCLREnabled = (bool)enableField.GetValue(instance);
					}
					catch
					{
						IsHybridCLREnabled = false;
					}
					break;
				}
#else
				UnityEngine.TextAsset record = UnityEngine.Resources.Load<UnityEngine.TextAsset>("HybridCLR_Enable");
				IsHybridCLREnabled = record != null;
#endif
			}
			Log.Info(IsHybridCLREnabled ? "enabled" : "disabled", nameof(HybridCLR), true);
		}

		internal async UniTask Run()
		{
			Procedure patchProcedure = new Procedure(nameof(HybridCLRRunner));
			patchProcedure.AddStep(new LoadPatchDLLStep());
			patchProcedure.AddStep(new LoadMetaDataDLLStep());
			patchProcedure.Start();
			Log.Info($"Start HybridCLR patch procedure", nameof(HybridCLRRunner), true);

			while(!patchProcedure.IsFinished)
				await UniTask.NextFrame();

			Log.Info($"HybridCLR patch procedure finished", nameof(HybridCLRRunner), true);
			EventManager.Trigger(EventDefine.HybridCLRRunnerFinish);
			IsFinished = true;
			OnFinish?.Invoke();

			_RunPatchCode();
		}
	}
}
