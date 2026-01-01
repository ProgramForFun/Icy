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


using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Icy.Base.Editor
{
	/// <summary>
	/// FSM可视化Editor窗口
	/// </summary>
	public class FSMViewerWindow : EditorWindow
	{
		/// <summary>
		/// 持有的GraphView
		/// </summary>
		FSMViewerGraphView _GraphView;
		/// <summary>
		/// 当前界面上点选的FSM
		/// </summary>
		FSM _CurrSelectedFSM;


		[MenuItem("Icy/Tools/FSM Viewer")]
		public static void Open()
		{
			GetWindow<FSMViewerWindow>("FSM Viewer");
		}

		private void OnEnable()
		{
			Init();
		}

		private void OnDisable()
		{
			FSMManager.Instance.OnAddFSM -= OnAddFSM;
			FSMManager.Instance.OnRemoveFSM -= OnRemoveFSM;
			FSMManager.Instance.OnFSMStateChangingStarted -= OnFSMStateChangingStarted;
			FSMManager.Instance.OnFSMPrevStateDeactivated -= OnFSMPrevStateDeactivated;
			FSMManager.Instance.OnFSMChangingStateEnd -= OnFSMChangingStateEnd;
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
		}

		private void Init()
		{
			rootVisualElement.Clear();

			_GraphView = new FSMViewerGraphView(this);
			_GraphView.style.flexGrow = 1;
			_GraphView.StretchToParentSize();

			List<FSM> allFSMs = FSMManager.Instance.GetAllFSMs();
			//复制一下，避免后续FSMManager的变化，继续影响FSMViewer，出现重复FSM的情况
			allFSMs = new List<FSM>(allFSMs);
			_GraphView.SetFSMData(allFSMs);
			_GraphView.AddClickFSMListener(OnClickFSM);

			rootVisualElement.Add(_GraphView);
			rootVisualElement.MarkDirtyRepaint();

			FSMManager.Instance.OnAddFSM -= OnAddFSM;
			FSMManager.Instance.OnAddFSM += OnAddFSM;
			FSMManager.Instance.OnRemoveFSM -= OnRemoveFSM;
			FSMManager.Instance.OnRemoveFSM += OnRemoveFSM;
			FSMManager.Instance.OnFSMStateChangingStarted -= OnFSMStateChangingStarted;
			FSMManager.Instance.OnFSMStateChangingStarted += OnFSMStateChangingStarted;
			FSMManager.Instance.OnFSMPrevStateDeactivated -= OnFSMPrevStateDeactivated;
			FSMManager.Instance.OnFSMPrevStateDeactivated += OnFSMPrevStateDeactivated;
			FSMManager.Instance.OnFSMChangingStateEnd -= OnFSMChangingStateEnd;
			FSMManager.Instance.OnFSMChangingStateEnd += OnFSMChangingStateEnd;
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
		}

		private void OnAddFSM(FSM fsm)
		{
			_GraphView.AddSingleFSM(fsm);
		}

		private void OnRemoveFSM(FSM fsm)
		{
			_GraphView.RemoveSingleFSM(fsm);
			if (fsm == _CurrSelectedFSM)
				_GraphView.ClearNodes();
		}

		private void OnFSMStateChangingStarted(FSM fsm, FSMState prevState, FSMState nextState)
		{
			if (fsm != _CurrSelectedFSM)
				return;

			string prevStateName = "Null";
			if (prevState != null)
				prevStateName = prevState.GetType().Name;
			string nextStateName = nextState.GetType().Name;

			//Log.Error($"{prevStateName} --> {nextStateName}");

			_GraphView.SetLineStateChangingFinished();
			_GraphView.RemovePrevConnectLine();
			_GraphView.UnHighlightNode(prevStateName);
			_GraphView.ConnectNodes(prevStateName, nextStateName);
			long startTimestamp = FSMManager.Instance.GetFSMStateStartTimestamp(fsm, nextState);
			_GraphView.HighlightNode(nextStateName, startTimestamp);

			_GraphView.SetLineWaitPrevStateDeactivate();
		}

		private void OnFSMPrevStateDeactivated(FSM fsm, FSMState prevState, FSMState nextState)
		{
			if (fsm != _CurrSelectedFSM)
				return;
			_GraphView.SetLineWaitNextStateActivate();
		}

		private void OnFSMChangingStateEnd(FSM fsm, FSMState prevState, FSMState nextState)
		{
			if (fsm != _CurrSelectedFSM)
				return;
			_GraphView.SetLineStateChangingFinished();
		}

		private void OnClickFSM(FSM fsm)
		{
			if (_CurrSelectedFSM != null && _CurrSelectedFSM == fsm)
				return;

			_CurrSelectedFSM = fsm;
			_GraphView.ClearNodes();
			_GraphView.AddNodesOfFSM(fsm);

			//切换到某FSM后，立刻显示当前切换
			if (fsm.IsChangingState)
			{
				FSMState prev = fsm.PrevState;
				FSMState next = fsm.IsChangingState ? fsm.NextState : fsm.CurrState;
				OnFSMStateChangingStarted(fsm, prev, next);
				if (fsm.IsPrevStateDeactivated)
					OnFSMPrevStateDeactivated(fsm, prev, next);
			}
			else
			{
				string currStateName = fsm.CurrState.GetType().Name;
				if (fsm.PrevState != null)
				{
					string prevStateName = fsm.PrevState.GetType().Name;
					_GraphView.ConnectNodes(prevStateName, currStateName);
					_GraphView.SetLineStateChangingFinished();
				}
				long startTimestamp = FSMManager.Instance.GetFSMStateStartTimestamp(fsm, fsm.CurrState);
				_GraphView.HighlightNode(currStateName, startTimestamp);
			}
		}

		private void OnPlayModeChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredPlayMode)
				Init();

			if (state == PlayModeStateChange.ExitingPlayMode)
			{
				_CurrSelectedFSM = null;
				_GraphView.SetFSMData(null);
			}
		}
	}
}
