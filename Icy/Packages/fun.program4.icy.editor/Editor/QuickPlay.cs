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
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

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
		/// 不 进行Domain Reload，直接进入or退出Play状态；
		/// 使用这个功能需要知道其原理和限制，否则请不要使用这个功能
		/// </summary>
		[MenuItem("Help/QuickPlay _F5")] //主要从Toolbar的按钮或快捷键触发，菜单藏到Help里了
		static void QuickPlay()
		{
			if (EditorApplication.isPlaying)
				EditorApplication.ExitPlaymode();
			else
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

		static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingPlayMode)
			{
				if (EditorSettings.enterPlayModeOptionsEnabled)
				{
					EditorSettings.enterPlayModeOptionsEnabled = false;
					EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
				}
			}
			else if (state == PlayModeStateChange.EnteredEditMode)
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
