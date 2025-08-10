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


using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Icy.Editor
{
	/// <summary>
	/// 在Play按钮那一行的最左侧，显示Git分支名，方便区分分支
	/// </summary>
	public class BranchDisplaying
	{
		[InitializeOnLoadMethod]
		static void Init()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
		}

		static void OnToolbarGUI()
		{
			GUIStyle style = new GUIStyle();
			style.normal.textColor = Color.white;
			style.fontStyle = FontStyle.Bold;

			string branch = GetGitBranch();
			GUILayout.Label($"Git Branch  :  {branch}", style);
			Debug.Log(branch);
		}

		static string GetGitBranch()
		{
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
			{
				FileName = "git",
				Arguments = "rev-parse --abbrev-ref HEAD",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
			{
				using (System.IO.StreamReader reader = process.StandardOutput)
				{
					string result = reader.ReadToEnd().Trim();
					return result;
				}
			}
		}
	}
}
