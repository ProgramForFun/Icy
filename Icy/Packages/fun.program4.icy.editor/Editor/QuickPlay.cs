using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Icy.Editor
{
	/// <summary>
	/// 不 进行Domain Reload，直接进入Play状态；
	/// 使用这个功能需要知道其原理和限制，否则请不要使用这个功能
	/// </summary>
	[InitializeOnLoad]
	public class QuickPlayViewer
	{
		static QuickPlayViewer()
		{
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
			EditorApplication.playModeStateChanged += ClearPlayMode;
		}

		static void OnToolbarGUI()
		{
			if (EditorApplication.isPlaying)
				return;

			Color originalBackgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f);

			GUIStyle style = new GUIStyle("Button")
			{
				fontSize = 13,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				fontStyle = FontStyle.Normal,
				fixedWidth = 70
			};

			if (GUILayout.Button(new GUIContent("QuickPlay", "不 进行Domain Reload，直接进入Play状态；\n使用这个功能需要知道其原理和限制，否则请不要使用这个功能"), style))
				QuickPlay();

			GUI.backgroundColor = originalBackgroundColor;
		}

		static void QuickPlay()
		{
			if (EditorApplication.isPlaying == false)
			{
				EditorSettings.enterPlayModeOptionsEnabled = true;
				EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
				EditorApplication.EnterPlaymode();
			}
		}

		static void ClearPlayMode(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingPlayMode)
			{
				if (EditorSettings.enterPlayModeOptionsEnabled)
					EditorSettings.enterPlayModeOptionsEnabled = false;
			}
		}
	}
}
