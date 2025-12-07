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
	/// 打包AssetBundle的子Procedure
	/// </summary>
	public class SubProcedureBuildAssetBundleStep : BuildStep
	{
		protected static BuildTarget _BuildTarget;
		protected static BuildSetting _BuildSetting;
		protected static Action<bool> _BuildCallback;

		protected static string BUILD_ASSET_BUNDLE_PROCEDURE_CFG_NAME = "BuildAssetBundleProcedureCfg.json";
		protected static string ICY_BUILD_ASSET_BUNDLE_PROCEDURE_CFG_PATH = "Packages/fun.program4.icy.asset/Editor/Build/BuildAssetBundleProcedure/" + BUILD_ASSET_BUNDLE_PROCEDURE_CFG_NAME;

		public override async UniTask Activate()
		{
			_BuildSetting = OwnerProcedure.Blackboard.ReadObject("BuildSetting", true) as BuildSetting;
			if (!_BuildSetting.BuildAssetBundle)
			{
				Finish();
				return;
			}

			_BuildTarget = (BuildTarget)OwnerProcedure.Blackboard.ReadInt("BuildTarget", true);

			Build(_BuildTarget, _BuildSetting, DoOnBuildAssetBundleProcedureFinish);
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
		/// 打包AssetBundle
		/// </summary>
		/// <param name="buildTarget">打包的平台</param>
		/// <param name="buildSetting">打包的设置</param>
		/// <param name="callback">回调</param>
		public static void Build(BuildTarget buildTarget, BuildSetting buildSetting, Action<bool> callback)
		{
			if (buildTarget != EditorUserBuildSettings.activeBuildTarget)
			{
				Log.Assert(false, $"AssetBunle打包未执行；BuildTarget平台不一致，请先切换完毕再打包；\n当前平台 = {EditorUserBuildSettings.activeBuildTarget}, 选择的打包平台 = {buildTarget}");
				return;
			}

			_BuildCallback = callback;

			List<string> allSteps = GetAllStepNamesImpl();
			Procedure procedure = new Procedure("BuildAssetBundle");
			BiProgress.MonitorProcedure(procedure);
			for (int i = 0; i < allSteps.Count; i++)
			{
				string typeWithNameSpace = allSteps[i];
				Type type = TypeResolver.GetType(typeWithNameSpace);
				if (type == null)
				{
					Log.Assert(false, $"Can not find BuildAssetBundleProcedure step {typeWithNameSpace}");
					return;
				}

				ProcedureStep step = Activator.CreateInstance(type) as ProcedureStep;
				procedure.AddStep(step);
			}

			procedure.Blackboard.WriteInt("BuildTarget", (int)buildTarget);
			procedure.Blackboard.WriteObject("BuildSetting", buildSetting);
			procedure.Blackboard.WriteString("BuildPackage", "DefaultPackage");
			procedure.OnFinish += OnBuildAssetBundleProcedureFinish;
			procedure.Start();
		}

		/// <summary>
		/// 获取所有的打包Bundle的步骤类名
		/// </summary>
		public static List<string> GetAllStepNamesImpl()
		{
			JSONArray jsonArray;
			if (File.Exists(BUILD_ASSET_BUNDLE_PROCEDURE_CFG_NAME))
				jsonArray = JSONNode.Parse(File.ReadAllText(BUILD_ASSET_BUNDLE_PROCEDURE_CFG_NAME)) as JSONArray;
			else
				jsonArray = JSONNode.Parse(File.ReadAllText(ICY_BUILD_ASSET_BUNDLE_PROCEDURE_CFG_PATH)) as JSONArray;

			List<string> rtn = new List<string>(8);
			for (int i = 0; i < jsonArray.Count; i++)
			{
				string typeWithNameSpace = jsonArray[i];
				rtn.Add(typeWithNameSpace);
			}

			return rtn;
		}

		protected static void OnBuildAssetBundleProcedureFinish(bool succeed)
		{
			_BuildCallback?.Invoke(succeed);
		}

		protected void DoOnBuildAssetBundleProcedureFinish(bool succeed)
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
