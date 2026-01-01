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
		/// <summary>
		/// 外部的EditorWindow
		/// </summary>
		private EditorWindow _EditorWindow;
		/// <summary>
		/// 显示所有FSM的GraphView层面的列表组件
		/// </summary>
		private FSMListView _ListView;
		/// <summary>
		/// 圆形布局半径的Slider
		/// </summary>
		private Slider _RadiusSlider;
		/// <summary>
		/// 当前点选的FSM所属的所有状态节点
		/// </summary>
		private Dictionary<string, FSMStateNode> _CurrNodes;
		/// <summary>
		/// 前一个FSM状态转换的连线
		/// </summary>
		private SplitColorEdge _ConnectLine;
		/// <summary>
		/// 创建圆形排列的状态节点时，创建位置的圆中心点坐标
		/// </summary>
		private readonly Vector2 NODES_CENTER = new Vector2(600, 300);
		/// <summary>
		/// 创建圆形排列的状态节点时，圆的半径
		/// </summary>
		private float _NodesCircleRadius;
		/// <summary>
		/// 创建圆形排列的状态节点时，圆的默认半径
		/// </summary>
		private const float NODES_CIRCLE_RADIUS = 200.0f;
		/// <summary>
		/// 创建直线排列的状态节点时，的左侧起点x坐标
		/// </summary>
		private const float NODES_LINE_START_POS_X = 300.0f;
		/// <summary>
		/// 创建直线排列的状态节点时，每个节点之间间隔
		/// </summary>
		private const float NODES_LINE_INTERVAL_X = 100.0f;
		/// <summary>
		/// 创建直线排列的状态节点时，y轴的位置
		/// </summary>
		private const float NODES_LINE_POS_Y = 300.0f;
		/// <summary>
		/// Port默认的颜色
		/// </summary>
		private readonly Color DEFAULT_PORT_COLOR = new Color(0.384f, 0.384f, 0.384f, 1.0f);
		/// <summary>
		/// 当前选中的FSM，是不是Procedure内部的FSM
		/// </summary>
		private bool _IsFSMInProcedure = false;


		public FSMViewerGraphView(EditorWindow editorWindow)
		{
			_EditorWindow = editorWindow;

			StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/fun.program4.icy.base/Editor/FSMViewer/FSMViewerStyles.uss");
			if (styleSheet != null)
			{
				styleSheets.Clear();
				styleSheets.Add(styleSheet);
			}

			//缩放范围
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			//创建背景网格
			GridBackground gridBackground = new GridBackground();
			gridBackground.name = nameof(GridBackground);
			Insert(0, gridBackground); // 插入到最底层
			gridBackground.StretchToParentSize();

			//允许拖动
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			//允许框选
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new ContentZoomer());

			//监听节点创建事件
			nodeCreationRequest += OnNodeCreationRequest;
			//监听右键菜单事件
			RegisterCallback<ContextualMenuPopulateEvent>(OnGraphViewContextMenu);

			_CurrNodes = new Dictionary<string, FSMStateNode>();
		}

		/// <summary>
		/// 给FSM列表设置数据，同时清除View上现有的节点
		/// </summary>
		public void SetFSMData(List<FSM> fsmList)
		{
			ClearNodes();

			//初始化左侧的列表
			if (_ListView != null)
				_ListView.RemoveFromHierarchy();

			_ListView = new FSMListView();
			_ListView.SetData(fsmList);
			Add(_ListView);
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
		/// 将一个FSM的所有状态，生成出Node
		/// </summary>
		public void AddNodesOfFSM(FSM fsm)
		{
			_IsFSMInProcedure = fsm.Name.EndsWith($"({nameof(Procedure)})");

			if (!_IsFSMInProcedure)
			{
				_NodesCircleRadius = NODES_CIRCLE_RADIUS;
				AddRadiusSlider();
			}

			for (int i = 0; i < fsm.AllStates.Count; i++)
			{
				string stateName = fsm.AllStates[i].GetType().Name;
				FSMStateNode newNode = AddNode(fsm.AllStates[i], _IsFSMInProcedure);

				_CurrNodes.Add(stateName, newNode);
			}

			if (_IsFSMInProcedure)
				schedule.Execute(() => { LineUpNodes(); }).ExecuteLater(18);
			else
				CircleNodes();
		}

		/// <summary>
		/// 把一个FSM的所有状态，按逆时针方向圆形排列，生成Node
		/// </summary>
		private void CircleNodes()
		{
			Vector2 startDir = new Vector2(_NodesCircleRadius, 0);
			int i = 0;
			foreach (KeyValuePair<string, FSMStateNode> item in _CurrNodes)
			{
				Vector2 pos = CommonUtility.RotateVector2(startDir, -360.0f / _CurrNodes.Count * i) + NODES_CENTER;
				item.Value.SetPosition(new Rect(pos.x, pos.y, 0, 0));
				i++;
			}
		}

		/// <summary>
		/// 把Procedure内FSM的所有状态，按从左到右的顺序，生成Node
		/// </summary>
		private void LineUpNodes()
		{
			int i = 0;
			float preNodePosX = NODES_LINE_START_POS_X;
			foreach (KeyValuePair<string, FSMStateNode> item in _CurrNodes)
			{
				float x = preNodePosX + NODES_LINE_INTERVAL_X;
				Vector2 pos = new Vector2(x, NODES_LINE_POS_Y);
				item.Value.SetPosition(new Rect(pos.x, pos.y, 0, 0));
				preNodePosX = x + item.Value.resolvedStyle.width;
				i++;
			}
		}

		public FSMStateNode AddNode(FSMState state, bool isInProcedure)
		{
			FSMStateNode node = new FSMStateNode(state, isInProcedure);
			AddElement(node);
			return node;
		}

		/// <summary>
		/// 清除所有节点及其连线
		/// </summary>
		public void ClearNodes()
		{
			List<Edge> edgesToRemove = new List<Edge>();
			foreach (KeyValuePair<string, FSMStateNode> node in _CurrNodes)
			{
				// 删除所有节点的连线
				edgesToRemove.Clear();
				foreach (Port inputPort in node.Value.inputContainer.Children().OfType<Port>())
					edgesToRemove.AddRange(inputPort.connections);
				foreach (Port output in node.Value.outputContainer.Children().OfType<Port>())
					edgesToRemove.AddRange(output.connections);

				foreach (Edge edge in edgesToRemove)
				{
					edge.input.Disconnect(edge);
					edge.output.Disconnect(edge);
					RemoveElement(edge);
				}

				//删除节点本身
				RemoveElement(node.Value);
			}
			_ConnectLine = null;

			_CurrNodes.Clear();

			if (_RadiusSlider != null)
			{
				Remove(_RadiusSlider);
				_RadiusSlider = null;
			}
		}

		/// <summary>
		/// 连接两个节点
		/// </summary>
		public void ConnectNodes(string fromStateName, string toStateName)
		{
			if (fromStateName != "Null" && _CurrNodes.TryGetValue(fromStateName, out FSMStateNode fromNode) 
				&& _CurrNodes.TryGetValue(toStateName, out FSMStateNode toNode))
			{
				Port fromNodeOutput = fromNode.outputContainer[0] as Port;
				Port toNodeInput = toNode.inputContainer[0] as Port;

				_ConnectLine = fromNodeOutput.ConnectTo<SplitColorEdge>(toNodeInput);
				_ConnectLine.SetGraphView(this);
				AddElement(_ConnectLine);
			}
		}

		/// <summary>
		/// 删除前一个状态切换的连线
		/// </summary>
		public void RemovePrevConnectLine()
		{
			if (_ConnectLine != null)
			{
				_ConnectLine.input.Disconnect(_ConnectLine);
				_ConnectLine.output.Disconnect(_ConnectLine);

				RemoveElement(_ConnectLine);
				_ConnectLine = null;
			}
		}

		/// <summary>
		/// 高亮一个节点，以示当前状态在此节点
		/// </summary>
		public void HighlightNode(string stateName, long startTimestamp)
		{
			if (_CurrNodes.TryGetValue(stateName, out FSMStateNode node))
			{
				node.SetStartTimestamp(startTimestamp);
				node.SetColor(Color.cyan);
			}
		}

		/// <summary>
		/// 把高亮的节点恢复为默认状态
		/// </summary>
		public void UnHighlightNode(string stateName)
		{
			if (_CurrNodes.TryGetValue(stateName, out FSMStateNode node))
			{
				node.ClearStartTime();
				node.ResetColor();
			}
		}

		/// <summary>
		/// 等待前一个状态Deactivate完成的连线颜色
		/// </summary>
		public void SetLineWaitPrevStateDeactivate()
		{
			if (_ConnectLine != null)
				_ConnectLine.SetSplitColor(Color.cyan, DEFAULT_PORT_COLOR);
		}

		/// <summary>
		/// 等待下一个状态Activate完成的连线颜色
		/// </summary>
		public void SetLineWaitNextStateActivate()
		{
			if (_ConnectLine != null)
				_ConnectLine.SetSplitColor(DEFAULT_PORT_COLOR, Color.cyan);
		}

		/// <summary>
		/// 整个状态切换完成的连线颜色
		/// </summary>
		public void SetLineStateChangingFinished()
		{
			if (_ConnectLine != null)
				_ConnectLine.SetSplitColor(DEFAULT_PORT_COLOR, DEFAULT_PORT_COLOR);
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			//允许连接结点
			return ports.ToList();
		}

		/// <summary>
		/// 添加调整圆形布局半径的Slider
		/// </summary>
		private void AddRadiusSlider()
		{
			_RadiusSlider = new Slider("布局半径：", NODES_CIRCLE_RADIUS, 500);
			_RadiusSlider.style.fontSize = 14;
			_RadiusSlider.labelElement.style.paddingLeft = 40;
			_RadiusSlider.style.position = Position.Absolute;
			_RadiusSlider.style.width = 300;
			_RadiusSlider.style.marginTop = 10;
			_RadiusSlider.style.marginLeft = 200;
			_RadiusSlider.showInputField = true;
			_RadiusSlider.RegisterValueChangedCallback(OnRadiusSliderValueChanged);
			Add(_RadiusSlider);
			_RadiusSlider.BringToFront();
		}

		/// <summary>
		/// 由菜单创建节点
		/// </summary>
		private void OnNodeCreationRequest(NodeCreationContext context)
		{
			//FSMStateNode node = AddNode("New Node");
			//Vector2 localPos = ScreenToGraphView(context.screenMousePosition);
			//node.SetPosition(new Rect(localPos, Vector2.zero));
		}

		/// <summary>
		/// 清除节点右键菜单里的默认选项
		/// </summary>
		private void OnGraphViewContextMenu(ContextualMenuPopulateEvent evt)
		{
			VisualElement target = evt.target as VisualElement;

			if (target is Node || (target != null && target.GetFirstAncestorOfType<Node>() != null))
			{
				// 完全阻止 GraphView 添加默认菜单项
				evt.StopPropagation();

				// 清除所有已存在的菜单项
				evt.menu.ClearItems();

				if (target is Node node)
					node.BuildContextualMenu(evt);
			}
		}

		/// <summary>
		/// 拖动半径Slider时，按新半径重新布局节点
		/// </summary>
		private void OnRadiusSliderValueChanged(ChangeEvent<float> evt)
		{
			_NodesCircleRadius = evt.newValue;
			CircleNodes();
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
