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
}
