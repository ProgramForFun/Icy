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
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Icy.Base.Editor
{
	/// <summary>
	/// FSM可视化GraphView窗口
	/// </summary>
	public class FSMViewerGraphView : GraphView
	{
		private EditorWindow _EditorWindow;
		private FSMListView _ListView;
		private List<FSMStateNode> _CurrNodes;
		private readonly Vector2 _NodesCenter = new Vector2(600, 300);
		private readonly float _NodesCircleRadius = 200.0f;

		public FSMViewerGraphView(EditorWindow editorWindow)
		{
			_EditorWindow = editorWindow;

			//缩放范围
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			//创建背景网格
			//TODO：暂时有未知问题，无法创建
			GridBackground gridBackground = new GridBackground();
			gridBackground.name = nameof(GridBackground);
			Insert(0, gridBackground); // 插入到最底层
			gridBackground.StretchToParentSize();

			//允许拖动
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			//允许框选
			this.AddManipulator(new RectangleSelector());

			//监听节点创建事件
			nodeCreationRequest += OnNodeCreationRequest;

			_CurrNodes = new List<FSMStateNode>();
		}

		public void SetFSMData(List<FSM> fsmList)
		{
			//清除已有Node
			for (int i = 0; i < _CurrNodes.Count; i++)
				RemoveElement(_CurrNodes[i]);
			_CurrNodes.Clear();

			//初始化左侧的列表
			if (_ListView != null)
				_ListView.RemoveFromHierarchy();

			_ListView = new FSMListView();
			_ListView.SetData(fsmList);
			Add(_ListView);

			//默认创建列表第一个FSM的Node
			if (fsmList != null && fsmList.Count > 0)
				AddNodesOfFSM(fsmList[0]);
		}

		public void AddSingleFSM(FSM fsm)
		{
			_ListView.AddListItem(fsm);
		}

		public void RemoveSingleFSM(FSM fsm)
		{
			_ListView.RemoveListItem(fsm);
		}

		public void AddClickFSMListener(Action<FSM> onClickFSM)
		{
			_ListView.AddClickListener(onClickFSM);
		}

		/// <summary>
		/// 把一个FSM的所有状态，按逆时针方向圆形排列，生成Node
		/// </summary>
		public void AddNodesOfFSM(FSM fsm)
		{
			Vector2 startDir = new Vector2(_NodesCircleRadius, 0);
			int count = fsm.AllStates.Count;
			for (int i = 0; i < count; i++)
			{
				FSMStateNode newNode = AddNode(fsm.AllStates[i].GetType().Name);
				Vector2 pos = CommonUtility.RotateVector2(startDir, -360.0f / count * i) + _NodesCenter;
				newNode.SetPosition(new Rect(pos.x, pos.y, 0, 0));
				_CurrNodes.Add(newNode);
			}
		}

		public FSMStateNode AddNode(string nodeName)
		{
			FSMStateNode node = new FSMStateNode(nodeName);
			AddElement(node);
			return node;
		}

		public void ClearNodes()
		{
			List<Edge> edgesToRemove = new List<Edge>();
			for (int i = 0; i < _CurrNodes.Count; i++)
			{
				// 删除所有节点的连线
				edgesToRemove.Clear();
				foreach (Port inputPort in _CurrNodes[i].inputContainer.Children().OfType<Port>())
					edgesToRemove.AddRange(inputPort.connections);
				foreach (Port output in _CurrNodes[i].outputContainer.Children().OfType<Port>())
					edgesToRemove.AddRange(output.connections);


				foreach (Edge edge in edgesToRemove)
				{
					edge.input.Disconnect(edge);
					edge.output.Disconnect(edge);
					RemoveElement(edge);
				}

				//删除节点本身
				RemoveElement(_CurrNodes[i]);
			}
			_CurrNodes.Clear();
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			//允许连接结点
			return ports.ToList();
		}

		/// <summary>
		/// 由菜单创建节点
		/// </summary>
		private void OnNodeCreationRequest(NodeCreationContext context)
		{
			FSMStateNode node = AddNode("Request add Node");
			Vector2 localPos = ScreenToGraphView(context.screenMousePosition);
			node.SetPosition(new Rect(localPos, Vector2.zero));
		}

		/// <summary>
		/// 把屏幕坐标转换到GraphView坐标下；
		/// TODO：缩放状态下的转换有问题
		/// </summary>
		private Vector2 ScreenToGraphView(Vector2 screenPosition)
		{
			// 处理非全屏的EditorWindow的位置偏移
			Vector2 relativeToGraphView = screenPosition - _EditorWindow.position.position;

			// 考虑当前缩放级别
			relativeToGraphView /= scale;

			// 考虑内容容器的滚动偏移
			Vector2 scrollOffset = contentViewContainer.transform.position;
			Vector2 finalPosition = relativeToGraphView - scrollOffset;

			return finalPosition;
		}
	}
}
