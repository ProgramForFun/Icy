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
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 执行Unity的打包
	/// </summary>
	public class BuildPlayerStep : BuildStep
	{
		public override async UniTask Activate()
		{
			BuildSetting buildSetting = OwnerProcedure.Blackboard.ReadObject("BuildSetting") as BuildSetting;
			Build(buildSetting);
			Finish();
			await UniTask.CompletedTask;
		}

		public static void Build(BuildSetting buildSetting)
		{
			BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

			string outputDir = buildSetting.OutputDir + "/" + buildTarget.ToString();
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
			if (buildSetting.DevelopmentBuild)
				options |= BuildOptions.Development;
			if (buildSetting.ScriptDebugging)
				options |= BuildOptions.AllowDebugging;
			if (buildSetting.DeepProfiling)
				options |= BuildOptions.EnableDeepProfilingSupport;
			if (buildSetting.AutoConnectProfiler)
				options |= BuildOptions.ConnectWithProfiler;

			BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
			buildPlayerOptions.target = buildTarget;
			buildPlayerOptions.options = options;
			buildPlayerOptions.scenes = scenes.ToArray();
			buildPlayerOptions.locationPathName = GetLocationPathName(buildSetting, buildTarget, outputDir);
			BuildPipeline.BuildPlayer(buildPlayerOptions);
		}

		private static string GetExecutableFileName(BuildSetting buildSetting)
		{
			string[] split = buildSetting.ApplicationIdentifier.Split('.');
			return split[split.Length - 1];
		}

		/// <summary>
		/// 获取包含文件名的完整输出路径
		/// </summary>
		private static string GetLocationPathName(BuildSetting buildSetting, BuildTarget buildTarget, string outputPath)
		{
			string executableFileName = GetExecutableFileName(buildSetting);

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
