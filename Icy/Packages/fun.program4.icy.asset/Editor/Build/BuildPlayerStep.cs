using Icy.Base;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 执行Unity的打包
	/// </summary>
	public class BuildPlayerStep : ProcedureStep
	{
		private BuildSetting _BuildSetting;

		public override async UniTask Activate()
		{
			_BuildSetting = OwnerProcedure.Blackboard.ReadObject("BuildSetting") as BuildSetting;

			await Build();

			Finish();
		}

		private async UniTask Build()
		{
			BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

			string outputDir = _BuildSetting.OutputDir + "/" + buildTarget.ToString();
			if (Directory.Exists(outputDir))
				Directory.Delete(outputDir, true);


			// 场景列表（根据项目实际场景修改）
			List<string> scenes = new List<string>();
			foreach (var scene in EditorBuildSettings.scenes)
			{
				if (scene.enabled)
					scenes.Add(scene.path);
			}

			// 构建选项配置
			BuildOptions options = BuildOptions.None;
			if (_BuildSetting.DevelopmentBuild)
				options |= BuildOptions.Development;
			if (_BuildSetting.ScriptDebugging)
				options |= BuildOptions.AllowDebugging;
			if (_BuildSetting.DeepProfiling)
				options |= BuildOptions.EnableDeepProfilingSupport;
			if (_BuildSetting.AutoConnectProfiler)
				options |= BuildOptions.ConnectWithProfiler;

			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
			buildPlayerOptions.target = buildTarget;
			buildPlayerOptions.options = options;
			buildPlayerOptions.scenes = scenes.ToArray();
			buildPlayerOptions.locationPathName = GetLocationPathName(buildTarget, outputDir);
			BuildPipeline.BuildPlayer(buildPlayerOptions);

			while (BuildPipeline.isBuildingPlayer)
				await UniTask.Yield();
		}

		private string GetExecutableFileName()
		{
			string[] split = _BuildSetting.ApplicationIdentifier.Split('.');
			return split[split.Length - 1];
		}

		/// <summary>
		/// 获取包含文件名的完整输出路径
		/// </summary>
		private string GetLocationPathName(BuildTarget buildTarget, string outputPath)
		{
			string executableFileName = GetExecutableFileName();

			switch (buildTarget)
			{
				case BuildTarget.Android:
					return Path.Combine(outputPath, executableFileName + ".apk");
				case BuildTarget.iOS:
				case BuildTarget.WebGL:
					return Path.Combine(outputPath, executableFileName);
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return Path.Combine(outputPath, executableFileName + ".exe");
				case BuildTarget.StandaloneOSX:
					return Path.Combine(outputPath, executableFileName);
				default:
					throw new Exception(string.Format("Platform Error! Can't Build {0}!", buildTarget));
			}
		}

		private static BuildTarget ParsePlatform(string platform)
		{
			switch (platform.ToLower())
			{
				case "win": 
					return BuildTarget.StandaloneWindows;
				case "android": 
					return BuildTarget.Android;
				case "ios": 
					return BuildTarget.iOS;
				case "mac": 
					return BuildTarget.StandaloneOSX;
				default: 
					return BuildTarget.StandaloneWindows;
			}
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
