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
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Icy.Base.Editor
{
	/// <summary>
	/// FSM状态节点
	/// </summary>
	public class FSMStateNode : Node
	{
		/// <summary>
		/// 对应的FSM状态
		/// </summary>
		private FSMState _FSMState;
		/// <summary>
		/// 是不是Procedure内部的FSM的状态
		/// </summary>
		private bool _IsInProcedure;
		/// <summary>
		/// 时间的横向布局
		/// </summary>
		private VisualElement _HorizontalContainer;
		/// <summary>
		/// 持续时间的标题
		/// </summary>
		private Label _DurationTitle;
		/// <summary>
		/// 持续时间
		/// </summary>
		private Label _Duration;
		/// <summary>
		/// Node的默认背景颜色
		/// </summary>
		private StyleColor _OriginalColor;
		/// <summary>
		/// 更新时间的Schedule
		/// </summary>
		private IVisualElementScheduledItem scheduledUpdate;
		/// <summary>
		/// 本状态开始运行的时间戳
		/// </summary>
		private long _StartTimestamp;


		public FSMStateNode(FSMState state, bool isInProcedure)
		{
			_FSMState = state;
			_IsInProcedure = isInProcedure;
			title = state.GetType().Name;

			//高亮边框相关
			_OriginalColor = style.borderTopColor;
			style.borderLeftWidth = 2;
			style.borderRightWidth = 2;
			style.borderTopWidth = 2;
			style.borderBottomWidth = 2;
			style.borderTopLeftRadius = 5;
			style.borderTopRightRadius = 5;
			style.borderBottomLeftRadius = 5;
			style.borderBottomRightRadius = 5;

			//两个Label，显示描述和时间
			_DurationTitle = new Label();
			_DurationTitle.style.fontSize = 13;
			_DurationTitle.text = "持续时间：";
			_Duration = new Label();
			_Duration.style.fontSize = 13;

			//横向布局，带边框
			_HorizontalContainer = new VisualElement();
			_HorizontalContainer.style.flexDirection = FlexDirection.Row;
			_HorizontalContainer.style.marginTop = 2;
			_HorizontalContainer.style.marginBottom = 2;
			_HorizontalContainer.style.marginLeft = 2;
			_HorizontalContainer.style.marginRight = 2;
			_HorizontalContainer.style.borderTopWidth = 1;
			_HorizontalContainer.style.borderBottomWidth = 1;
			_HorizontalContainer.style.borderLeftWidth = 1;
			_HorizontalContainer.style.borderRightWidth = 1;
			_HorizontalContainer.style.borderTopColor = new Color(0.4f, 0.4f, 0.5f);
			_HorizontalContainer.style.borderBottomColor = new Color(0.4f, 0.4f, 0.5f);
			_HorizontalContainer.style.borderLeftColor = new Color(0.4f, 0.4f, 0.5f);
			_HorizontalContainer.style.borderRightColor = new Color(0.4f, 0.4f, 0.5f);
			_HorizontalContainer.Add(_DurationTitle);
			_HorizontalContainer.Add(_Duration);

			AddInput(Orientation.Horizontal);
			AddOutput(Orientation.Horizontal);

			RegisterCallback<DetachFromPanelEvent>(OnRemoved);
			RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
			//RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.TrickleDown);
		}

		public void SetStartTime(long timestamp)
		{
			//插入到标题和输入输出Port之间
			mainContainer.Insert(1, _HorizontalContainer);

			if (scheduledUpdate != null && scheduledUpdate.isActive)
				ClearStartTime();
			scheduledUpdate = schedule.Execute(OnScheduledUpdate).Every(500);
			_StartTimestamp = timestamp;
		}

		public void ClearStartTime()
		{
			if (mainContainer.Contains(_HorizontalContainer))
				mainContainer.Remove(_HorizontalContainer);
			scheduledUpdate?.Pause();
		}

		public void AddInput(Orientation orientation)
		{
			Port input = Port.Create<Edge>(orientation, Direction.Input, Port.Capacity.Multi, typeof(Port));
			input.portName = string.Empty;
			inputContainer.Add(input);
		}

		public void AddOutput(Orientation orientation)
		{
			Port output = Port.Create<Edge>(orientation, Direction.Output, Port.Capacity.Multi, typeof(Port));
			output.portName = string.Empty;
			outputContainer.Add(output);
		}

		public void SetColor(Color color)
		{
			StyleColor newColor = new StyleColor(color);
			style.borderTopColor = newColor;
			style.borderBottomColor = newColor;
			style.borderLeftColor = newColor;
			style.borderRightColor = newColor;
		}

		public void ResetColor()
		{
			style.borderTopColor = _OriginalColor;
			style.borderBottomColor = _OriginalColor;
			style.borderLeftColor = _OriginalColor;
			style.borderRightColor = _OriginalColor;
		}

		private void OnScheduledUpdate()
		{
			long diff = DateTime.Now.TotalSeconds() - _StartTimestamp;
			TimeSpan span = TimeSpan.FromSeconds(diff);
			_Duration.text = span.ToString(@"mm\:ss");
		}

		private void OnRemoved(DetachFromPanelEvent evt)
		{
			ClearStartTime();
			UnregisterCallback<DetachFromPanelEvent>(OnRemoved);
		}

		//private void OnMouseMove(MouseMoveEvent evt)
		//{
		//	if (resolvedStyle != null)
		//		NotifyConnectedEdges();
		//}

		private void OnGeometryChanged(GeometryChangedEvent evt)
		{
			if (resolvedStyle != null)
				NotifyConnectedEdges();
		}

		/// <summary>
		/// 节点发生变化时，通知连线重绘
		/// </summary>
		private void NotifyConnectedEdges()
		{
			foreach (Port port in inputContainer.Children().OfType<Port>())
			{
				foreach (Edge edge in port.connections)
				{
					if (edge is SplitColorEdge splitColorEdge)
						splitColorEdge.UpdateEdge();
				}
			}

			foreach (Port port in outputContainer.Children().OfType<Port>())
			{
				foreach (Edge edge in port.connections)
				{
					if (edge is SplitColorEdge splitColorEdge)
						splitColorEdge.UpdateEdge();
				}
			}
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			if (_IsInProcedure)
			{
				evt.menu.AppendAction("Force Go To", OnForceGoToProcedureStep, DropdownMenuAction.Status.Normal);
				evt.menu.AppendSeparator();
			}
			else
			{
				evt.menu.AppendAction("Force Change To", OnForceChangeToFSMState, DropdownMenuAction.Status.Normal);
				evt.menu.AppendSeparator();
			}
		}

		private void OnForceChangeToFSMState(DropdownMenuAction action)
		{
			if (_FSMState.OwnerFSM.IsChangingState)
				Log.Error($"Changing to FSMState {title} failed, previous state changing is still running", nameof(FSMStateNode));
			else
				_FSMState.OwnerFSM.ChangeState(_FSMState);
		}

		private void OnForceGoToProcedureStep(DropdownMenuAction action)
		{
			ProcedureStep step = _FSMState as ProcedureStep;
			if (step.OwnerProcedure.IsChangingStep)
				Log.Error($"Go to ProcedureStep {title} failed, previous step changing is still running", nameof(FSMStateNode));
			else
				step.OwnerProcedure.CurrStep.FinishAndGoto(step);
		}
	}
}
