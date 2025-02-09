using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Icy.Editor
{
	/// <summary>
	/// 在Play按钮那一行的最左侧，显示Git分支名，方便区分分支
	/// </summary>
	[InitializeOnLoad]
	public class ProjectPathViewer
	{
		static ProjectPathViewer()
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
