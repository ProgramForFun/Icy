using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace Icy.Editor
{
	/// <summary>
	/// 在原生Play按钮右侧，添加快速Play、RePlay按钮，加快迭代速度
	/// </summary>
	public class QuickPlayViewer
	{
		private static Texture _QuickPlayIcon;
		private static Texture _RePlayIcon;
		private static bool _IsRequestingRePlay;

		[InitializeOnLoadMethod]
		static void Init()
		{
			ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
			EditorApplication.playModeStateChanged += ClearPlayMode;

			if (_QuickPlayIcon == null)
				_QuickPlayIcon = AssetDatabase.LoadAssetAtPath<Texture>("Packages/fun.program4.icy.editor/Editor/Res/QuickIcon.png");
			if (_RePlayIcon == null)
				_RePlayIcon = AssetDatabase.LoadAssetAtPath<Texture>("Packages/fun.program4.icy.editor/Editor/Res/ReplayIcon.png");

			_IsRequestingRePlay = false;
		}

		static void OnToolbarGUI()
		{
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

			if (!EditorApplication.isPlaying)
			{
				if (GUILayout.Button(new GUIContent("QuickPlay", _QuickPlayIcon, "不 进行Domain Reload，直接进入Play状态；\n使用这个功能需要知道其原理和限制，否则请不要使用这个功能"), style))
					QuickPlay();
			}

			if (EditorApplication.isPlaying)
			{
				style.fixedWidth = 70;
				if (GUILayout.Button(new GUIContent(" RePlay", _RePlayIcon, "不 进行Domain Reload，直接进入Play状态；\n使用这个功能需要知道其原理和限制，否则请不要使用这个功能"), style))
					RePlay();
			}

			GUI.backgroundColor = originalBackgroundColor;
		}

		/// <summary>
		/// 不 进行Domain Reload，直接进入Play状态；
		/// 使用这个功能需要知道其原理和限制，否则请不要使用这个功能
		/// </summary>
		static void QuickPlay()
		{
			if (EditorApplication.isPlaying == false)
			{
				EditorSettings.enterPlayModeOptionsEnabled = true;
				EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
				EditorApplication.EnterPlaymode();
			}
		}

		/// <summary>
		/// 退出Play状态，然后使用QuickPlay重新进入Play状态
		/// </summary>
		static void RePlay()
		{
			if (EditorApplication.isPlaying)
			{
				EditorApplication.ExitPlaymode();
				_IsRequestingRePlay = true;
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

			if (state == PlayModeStateChange.EnteredEditMode)
			{
				if (_IsRequestingRePlay)
				{
					_IsRequestingRePlay = false;
					QuickPlay();
				}
			}
		}
	}
}
