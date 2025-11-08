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
		private List<FSMStateNode> _CurrNodes;

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

			//创建FSM列表
			_CurrNodes = new List<FSMStateNode>();
			FSMListView flw = new FSMListView(this, OnClickFSM);
			List<string> items = new List<string>
			{
				"项目1", "项目2", "项目3", "项目4", "项目5", "项目6", "项目7", "项目8", "项目9", "项目10", "项目11", "项目12", "项目13", "项目14", "项目15", "项目16", "项目17", "项目18", "项目19", "项目20"
			};
			flw.Update(items);
		}

		private void OnClickFSM(string obj)
		{
			Log.Info(obj);

			for (int i = 0; i < _CurrNodes.Count; i++)
				RemoveElement(_CurrNodes[i]);
			_CurrNodes.Clear();

			FSMStateNode node1 = AddNode("TestNode 1");
			node1.SetPosition(new Rect(300, 200 + UnityEngine.Random.Range(0, 50), 0, 0));
			_CurrNodes.Add(node1);

			FSMStateNode node2 = AddNode("TestNode 2");
			node2.SetPosition(new Rect(800, 200 + UnityEngine.Random.Range(0, 50), 0, 0));
			_CurrNodes.Add(node2);
		}

		public FSMStateNode AddNode(string nodeName)
		{
			FSMStateNode node = new FSMStateNode(nodeName);
			AddElement(node);
			return node;
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
