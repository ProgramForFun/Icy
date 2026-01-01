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


using Icy.Base;
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Icy.Editor
{
	/// <summary>
	/// 在Play按钮那一行的最左侧，显示版本控制的分支名，方便区分分支
	/// </summary>
	public class BranchDisplaying
	{
		private const string GET_REPO_TYPE_KEY = "_Icy_GetRepo";
		private static RepositoryType _RepoType;
		private static string _BranchName;
		private static long _PrevUpdateBranchNameTimestamp;

		[InitializeOnLoadMethod]
		static void Init()
		{
			_PrevUpdateBranchNameTimestamp = 0;

			//每次启动Unity，只执行一次判断Repo类型
			if (SessionState.GetString(GET_REPO_TYPE_KEY, string.Empty) == string.Empty)
			{
				_RepoType = DetectRepository();
				SessionState.SetString(GET_REPO_TYPE_KEY, GET_REPO_TYPE_KEY);

				EditorLocalPrefs.SetInt(GET_REPO_TYPE_KEY, (int)_RepoType);
				EditorLocalPrefs.Save();
			}

			_RepoType = (RepositoryType)EditorLocalPrefs.GetInt(GET_REPO_TYPE_KEY, 0);

			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
		}

		static void OnToolbarGUI()
		{
			GUIStyle style = new GUIStyle();
			style.normal.textColor = Color.white;
			style.fontStyle = FontStyle.Bold;

			long totalSeconds = DateTime.Now.TotalSeconds();
			if (totalSeconds - _PrevUpdateBranchNameTimestamp > 10)
			{
				_PrevUpdateBranchNameTimestamp = totalSeconds;
				switch (_RepoType)
				{
					case RepositoryType.Git:
						_BranchName = GetGitBranch();
						break;
					case RepositoryType.SVN:
						_BranchName = GetSvnBranch();
						break;
				}
			}

			if (_RepoType != RepositoryType.NotSet && _RepoType != RepositoryType.None)
				GUILayout.Label($"{_RepoType} Branch  :  {_BranchName}", style);
		}

		/// <summary>
		/// 获取Git分支名
		/// </summary>
		static string GetGitBranch()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "git",
				Arguments = "rev-parse --abbrev-ref HEAD",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (Process process = Process.Start(startInfo))
			{
				using (StreamReader reader = process.StandardOutput)
				{
					string result = reader.ReadToEnd().Trim();
					return result;
				}
			}
		}

		/// <summary>
		/// 获取SVN分支名
		/// </summary>
		static string GetSvnBranch()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "svn",
				Arguments = "info --show-item relative-url",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (Process process = Process.Start(startInfo))
			{
				using (StreamReader reader = process.StandardOutput)
				{
					string result = reader.ReadToEnd().Trim();
					// 示例格式： ^/branches/your-branch-name
					if (result.Contains("/branches/") || result.Contains("/Branches/"))
					{
						string[] splitArray = result.Split('/');
						return splitArray[^1];
					}
					return result.Contains("/trunk") ? "trunk" : string.Empty;
				}
			}
		}

		/// <summary>
		/// 获取当前工作目录是Git仓库，还是SVN仓库
		/// </summary>
		static RepositoryType DetectRepository(string path = null)
		{
			path ??= Directory.GetCurrentDirectory();
			DirectoryInfo current = new DirectoryInfo(path);

			while (current != null)
			{
				if (Directory.Exists(Path.Combine(current.FullName, ".git")))
					return RepositoryType.Git;

				if (Directory.Exists(Path.Combine(current.FullName, ".svn")))
					return RepositoryType.SVN;

				current = current.Parent;
			}

			return RepositoryType.None;
		}

		public enum RepositoryType
		{
			NotSet,	//初始值
			None,	//不在任何版本控制目录中
			Git,		//在Git中
			SVN,		//在SVN中
		}
	}
}
