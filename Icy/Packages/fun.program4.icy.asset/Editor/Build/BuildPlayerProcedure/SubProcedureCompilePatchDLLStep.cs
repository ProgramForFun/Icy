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
using System.IO;
using SimpleJSON;
using Icy.Editor;
using System.Collections.Generic;
using Icy.Base.Editor;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// HybridCLR编译热更DLL的子Procedure
	/// </summary>
	public class SubProcedureCompilePatchDLLStep : BuildStep
	{
		protected static Action<bool> _CompileCallback;

		protected static string HYBRID_CLR_COMPILE_PATCH_DLL_PROCEDURE_CFG_NAME = "HybridCLRCompilePatchDLLProcedureCfg.json";
		protected static string ICY_HYBRID_CLR_COMPILE_PATCH_DLL_PROCEDURE_CFG_NAME = "Packages/fun.program4.icy.asset/Editor/Build/HybridCLRCompilePatchDLL/" + HYBRID_CLR_COMPILE_PATCH_DLL_PROCEDURE_CFG_NAME;

		public override async UniTask Activate()
		{
			BuildTarget buildTarget = (BuildTarget)OwnerProcedure.Blackboard.ReadInt("BuildTarget", true);
			Compile(buildTarget, DoOnCompilePatchDLLProcedureFinish);
			await UniTask.CompletedTask;
		}

		public override bool IsSubProcedure()
		{
			return true;
		}

		public override List<string> GetAllStepNames()
		{
			return GetAllStepNamesImpl();
		}

		/// <summary>
		/// 编译热更DLL
		/// </summary>
		/// <param name="buildTarget">打包的平台</param>
		/// <param name="callback">回调</param>
		public static void Compile(BuildTarget buildTarget, Action<bool> callback)
		{
			if (buildTarget != EditorUserBuildSettings.activeBuildTarget)
			{
				Log.Assert(false, $"Compile Patch DLL未执行；BuildTarget平台不一致，请先切换完毕再编译；\n当前平台 = {EditorUserBuildSettings.activeBuildTarget}, 选择的编译平台 = {buildTarget}");
				return;
			}

			_CompileCallback = callback;

			List<string> allSteps = GetAllStepNamesImpl();
			Procedure procedure = new Procedure("CompilePatchDLL");
			for (int i = 0; i < allSteps.Count; i++)
			{
				string typeWithNameSpace = allSteps[i];
				Type type = TypeResolver.GetType(typeWithNameSpace);
				if (type == null)
				{
					Log.Assert(false, $"Can not find CompilePatchDLLProcedure step {typeWithNameSpace}");
					return;
				}

				ProcedureStep step = Activator.CreateInstance(type) as ProcedureStep;
				procedure.AddStep(step);
			}

			procedure.Blackboard.WriteInt("BuildTarget", (int)buildTarget);
			procedure.OnChangeStep += OnChangeBuildStep;
			procedure.OnFinish += OnCompilePatchDLLProcedureFinish;
			procedure.Start();
		}

		/// <summary>
		/// 获取所有的编译热更DLL的步骤类名
		/// </summary>
		public static List<string> GetAllStepNamesImpl()
		{
			JSONArray jsonArray;
			if (File.Exists(HYBRID_CLR_COMPILE_PATCH_DLL_PROCEDURE_CFG_NAME))
				jsonArray = JSONNode.Parse(File.ReadAllText(HYBRID_CLR_COMPILE_PATCH_DLL_PROCEDURE_CFG_NAME)) as JSONArray;
			else
				jsonArray = JSONNode.Parse(File.ReadAllText(ICY_HYBRID_CLR_COMPILE_PATCH_DLL_PROCEDURE_CFG_NAME)) as JSONArray;

			List<string> rtn = new List<string>(8);
			for (int i = 0; i < jsonArray.Count; i++)
			{
				string typeWithNameSpace = jsonArray[i];
				rtn.Add(typeWithNameSpace);
			}

			return rtn;
		}

		protected static void OnChangeBuildStep(ProcedureStep step)
		{
			string info = $"Current compile patch dll step : {step.GetType().Name}";
			BiProgress.Show("Compile Patch DLL", info, step.OwnerProcedure.Progress);
		}

		protected static void OnCompilePatchDLLProcedureFinish(bool succeed)
		{
			BiProgress.Hide();
			_CompileCallback?.Invoke(succeed);
		}

		protected void DoOnCompilePatchDLLProcedureFinish(bool succeed)
		{
			if (succeed)
				Finish();
			else
				OwnerProcedure.Abort();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
