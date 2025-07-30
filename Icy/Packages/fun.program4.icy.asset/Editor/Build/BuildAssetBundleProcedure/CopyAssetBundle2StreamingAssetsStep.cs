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
using System.IO;
using UnityEngine;
using YooAsset.Editor;
using System.Collections.Generic;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 打包AssetBundle，并Copy到StreamingAssets目录下
	/// </summary>
	public class CopyAssetBundle2StreamingAssetsStep : BuildStep
	{
		public override async UniTask Activate()
		{
			string buildPackage = OwnerProcedure.Blackboard.ReadString("BuildPackage", true);
			string buildOutputPath = OwnerProcedure.Blackboard.ReadString("BuildOutputPath", true);
			ScriptableBuildParameters buildParam = OwnerProcedure.Blackboard.ReadObject("BuildParam", true) as ScriptableBuildParameters;
			ClearStreamingAssetsAndCopyNew(buildPackage, buildOutputPath, buildParam);

			Finish();
			await UniTask.CompletedTask;
		}

		/// <summary>
		/// 非bundle的文件默认都需要Copy到StreamingAssets
		/// </summary>
		protected HashSet<string> GetNonBundleAssets(string sourcePath)
		{
			HashSet<string> rtn = new HashSet<string>(1024);
			string[] allFiles = Directory.GetFiles(sourcePath);
			for (int i = 0; i < allFiles.Length; i++)
			{
				string extension = Path.GetExtension(allFiles[i]);
				if (extension != ".bundle")
					rtn.Add(Path.GetFileName(allFiles[i]));
			}

			return rtn;
		}

		private HashSet<string> GetAllBundleAssets(string sourcePath)
		{
			HashSet<string> rtn = new HashSet<string>(1024);
			string[] allFiles = Directory.GetFiles(sourcePath);
			for (int i = 0; i < allFiles.Length; i++)
			{
				string extension = Path.GetExtension(allFiles[i]);
				if (extension == ".bundle")
					rtn.Add(Path.GetFileName(allFiles[i]));
			}

			return rtn;
		}

		/// <summary>
		/// override这个函数以过滤需要Copy到StreamingAssets的资源，比如要过滤只Copy首包资源，具体参考YooAsset文档的首包部分；
		/// 返回null为全部Copy
		/// </summary>
		protected virtual HashSet<string> FilterAsset2Copy(string sourcePath)
		{
			//这些是必须Copy的
			HashSet<string> rtn = GetNonBundleAssets(sourcePath);

			//默认实现是Copy所有Bundle；派生类可以调用基类的此函数后，自己去决定Copy哪些Bundle文件
			HashSet<string> allBundles = GetAllBundleAssets(sourcePath);

			rtn.UnionWith(allBundles);
			return rtn;
		}

		protected virtual void ClearStreamingAssetsAndCopyNew(string buildPackage, string outputPath, ScriptableBuildParameters buildParam)
		{
			string assetDir = Path.Combine(Application.streamingAssetsPath, "yoo", buildPackage);
			if (Directory.Exists(assetDir))
				Directory.Delete(assetDir, true);

			HashSet<string> assets2Copy = FilterAsset2Copy(outputPath);
			if (assets2Copy == null)
				CommonUtility.CopyDir(outputPath, assetDir);
			else
				CommonUtility.CopyFilesByNames(outputPath, assetDir, assets2Copy);

			//Copy BuildInCatalog
			buildParam.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll; //改为非None，才能Copy
			BuildContext buildContext = new BuildContext();
			BuildParametersContext buildParametersContext = new BuildParametersContext(buildParam);
			buildContext.SetContextObject(buildParametersContext);
			IBuildTask createCatalog = new TaskCreateCatalog_SBP();
			createCatalog.Run(buildContext);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public override async UniTask Deactivate()
		{
			await UniTask.CompletedTask;
		}
	}
}
