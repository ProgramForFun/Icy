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
		private static Texture _Icon;

		static QuickPlayViewer()
		{
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
			EditorApplication.playModeStateChanged += ClearPlayMode;

			if (_Icon == null)
				_Icon = AssetDatabase.LoadAssetAtPath<Texture>("Packages/fun.program4.icy.editor/Editor/Res/QuickIcon.png");
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
				imagePosition = ImagePosition.ImageLeft,
				fontStyle = FontStyle.Normal,
				fixedWidth = 83,
				fixedHeight = 20,
				padding = new RectOffset(0, 0, 3, 3)
			};

			if (GUILayout.Button(new GUIContent("QuickPlay", _Icon, "不 进行Domain Reload，直接进入Play状态；\n使用这个功能需要知道其原理和限制，否则请不要使用这个功能"), style))
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
				{
					EditorSettings.enterPlayModeOptionsEnabled = false;
					EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
				}
			}
		}
	}
}
