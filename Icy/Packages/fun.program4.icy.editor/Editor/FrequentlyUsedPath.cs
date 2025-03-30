using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 把常用的路径，加入到菜单里
/// </summary>
public static class FrequentlyUsedPath
{
	[MenuItem("Icy/Path/Persistent")]
	static void OpenPersistentDataPath()
	{
		System.Diagnostics.Process.Start(Application.persistentDataPath);
	}

	[MenuItem("Icy/Path/StreamingAssets")]
	static void OpenStreamingAssetsPath()
	{
		if (Directory.Exists(Application.streamingAssetsPath))
			System.Diagnostics.Process.Start(Application.streamingAssetsPath);
		else
			Debug.LogError($"Can not find {Application.streamingAssetsPath}");
	}

	[MenuItem("Icy/Path/EditorLog")]
	static void OpenEditorLogPath()
	{
		string editorLogPath;
#if UNITY_EDITOR_WIN
		string userDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
		editorLogPath = Path.Combine(userDir, "Unity", "Editor").Replace("\\", "/");
#else
		Debug.LogError("OpenEditorLogPath can only available on Windows as present");
#endif
		if (!string.IsNullOrEmpty(editorLogPath))
			System.Diagnostics.Process.Start(editorLogPath);
	}
}
