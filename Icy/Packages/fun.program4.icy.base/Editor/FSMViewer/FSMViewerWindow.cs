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
using UnityEngine.UIElements;

namespace Icy.Base.Editor
{
	/// <summary>
	/// FSM可视化Editor窗口
	/// </summary>
	public class FSMViewerWindow : EditorWindow
	{
		FSMViewerGraphView graphView;

		[MenuItem("Icy/FSM")]
		public static void Open()
		{
			GetWindow<FSMViewerWindow>(nameof(FSMViewerWindow));
		}

		private void OnEnable()
		{
			rootVisualElement.Clear();

			graphView = new FSMViewerGraphView(this);
			graphView.style.flexGrow = 1;
			graphView.StretchToParentSize();

			rootVisualElement.Add(graphView);
			rootVisualElement.MarkDirtyRepaint();
		}
	}
}
