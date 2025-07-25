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

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 打包AssetBundle的子Procedure
	/// </summary>
	public class SubProcedureBuildAssetBundleStep : ProcedureStep
	{
		protected static BuildTarget _BuildTarget;
		protected static BuildSetting _BuildSetting;
		protected static Action<bool> _BuildCallback;

		protected static string BUILD_ASSET_BUNDLE_PROCEDURE_CFG_NAME = "BuildAssetBundleProcedureCfg.json";
		protected static string ICY_BUILD_ASSET_BUNDLE_PROCEDURE_CFG_PATH = "Packages/fun.program4.icy.asset/Editor/Build/BuildAssetBundleProcedure/" + BUILD_ASSET_BUNDLE_PROCEDURE_CFG_NAME;

		public override async UniTask Activate()
		{
			_BuildTarget = (BuildTarget)OwnerProcedure.Blackboard.ReadInt("BuildTarget");
			_BuildSetting = OwnerProcedure.Blackboard.ReadObject("BuildSetting") as BuildSetting;

			Build(_BuildTarget, _BuildSetting, DoOnBuildAssetBundleProcedureFinish);
			await UniTask.CompletedTask;
		}

		public static void Build(BuildTarget buildTarget, BuildSetting buildSetting, Action<bool> callback)
		{
			_BuildCallback = callback;

			JSONArray jsonArray;
			if (File.Exists(BUILD_ASSET_BUNDLE_PROCEDURE_CFG_NAME))
				jsonArray = JSONNode.Parse(File.ReadAllText(BUILD_ASSET_BUNDLE_PROCEDURE_CFG_NAME)) as JSONArray;
			else
				jsonArray = JSONNode.Parse(File.ReadAllText(ICY_BUILD_ASSET_BUNDLE_PROCEDURE_CFG_PATH)) as JSONArray;


			Procedure procedure = new Procedure("BuildAssetBundle");
			for (int i = 0; i < jsonArray.Count; i++)
			{
				string typeWithNameSpace = jsonArray[i];
				Type type = Type.GetType(typeWithNameSpace);
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
			procedure.OnChangeStep += OnChangeBuildStep;
			procedure.OnFinish += OnBuildAssetBundleProcedureFinish;
			procedure.Start();
		}

		protected static void OnChangeBuildStep(ProcedureStep step)
		{
			string info = $"Current build asset bundle step : {step.GetType().Name}";
			BiProgress.Show("Build AssetBundle", info, step.OwnerProcedure.Progress);
		}

		protected static void OnBuildAssetBundleProcedureFinish(bool succeed)
		{
			BiProgress.Hide();
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
