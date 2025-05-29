using Icy.Base;
using Cysharp.Threading.Tasks;
using UnityEditor;
using System;
using System.IO;
using SimpleJSON;

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
			if (File.Exists("BuildAssetBundleProcedureCfg.json"))
				jsonArray = JSONNode.Parse(File.ReadAllText("BuildAssetBundleProcedureCfg.json")) as JSONArray;
			else
				jsonArray = JSONNode.Parse(File.ReadAllText("Packages/fun.program4.icy.asset/Editor/Build/BuildAssetBundleProcedure/BuildAssetBundleProcedureCfg.json")) as JSONArray;


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
			EditorUtility.DisplayProgressBar("Build AssetBundle", info, step.OwnerProcedure.Progress);
		}

		protected static void OnBuildAssetBundleProcedureFinish(bool succeed)
		{
			EditorUtility.ClearProgressBar();
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
