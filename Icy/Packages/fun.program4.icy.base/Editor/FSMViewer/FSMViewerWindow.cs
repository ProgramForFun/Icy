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


using System;
using System.Collections.Generic;
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
		FSMViewerGraphView _GraphView;
		FSM _CurrSelectedFSM;

		[MenuItem("Icy/FSM")]
		public static void Open()
		{
			GetWindow<FSMViewerWindow>(nameof(FSMViewerWindow));
		}

		private void OnEnable()
		{
			rootVisualElement.Clear();

			_GraphView = new FSMViewerGraphView(this);
			_GraphView.style.flexGrow = 1;
			_GraphView.StretchToParentSize();

			List<FSM> allFSMs = FSMManager.Instance.GetAllFSMs();
			_GraphView.SetFSMData(allFSMs);
			_GraphView.AddClickFSMListener(OnClickFSM);

			if (allFSMs.Count > 0)
				_CurrSelectedFSM = allFSMs[0];

			rootVisualElement.Add(_GraphView);
			rootVisualElement.MarkDirtyRepaint();
		}

		private void OnClickFSM(FSM obj)
		{
			if (_CurrSelectedFSM != null && _CurrSelectedFSM == obj)
				return;

			Log.Info(obj.Name);
			_GraphView.ClearNodes();
		}
	}
}
