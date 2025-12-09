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


using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Reflection;

namespace Icy.Base.Editor
{
	/// <summary>
	/// 自绘的两色分割连线本体
	/// </summary>
	public class SplitColorEdgeOverlay : VisualElement
	{
		private SplitColorEdge _Edge;
		private float _LineWidth;
		private Color _FirstColor;
		private Color _SecondColor;

		public SplitColorEdgeOverlay(SplitColorEdge targetEdge, Color first, Color second)
		{
			_Edge = targetEdge;
			_LineWidth = 3.0f;
			_FirstColor = first;
			_SecondColor = second;

			// 设置为绝对定位，覆盖在原始边上
			style.position = Position.Absolute;
			style.left = 0;
			style.top = 0;
			style.right = 0;
			style.bottom = 0;

			generateVisualContent += DrawOverlay;

			// 监听边的变化
			_Edge.RegisterCallback<GeometryChangedEvent>(OnEdgeGeometryChanged);
		}

		public void DrawOverlay(MeshGenerationContext context)
		{
			if (_Edge?.edgeControl == null)
				return;

			List<Vector2> edgePoints = CalculateEdgePoints();
			if (edgePoints.Count < 2)
				return;

			Painter2D painter = context.painter2D;

			painter.BeginPath();
			painter.lineWidth = _LineWidth;
			painter.lineJoin = LineJoin.Round;
			painter.lineCap = LineCap.Round;

			// 分割点取所有点中，距离两个Port距离最平均的
			Vector2 startPos = _Edge.output.GetGlobalCenter();
			Vector2 endPos = _Edge.input.GetGlobalCenter();
			int midPointIndex = 0;
			float minDistDiff = float.MaxValue;
			for (int i = 0; i < edgePoints.Count; i++)
			{
				float a = Vector2.Distance(startPos, edgePoints[i]);
				float b = Vector2.Distance(endPos, edgePoints[i]);
				float diff = Mathf.Abs(a - b);
				if (diff < minDistDiff)
				{
					minDistDiff = diff;
					midPointIndex = i;
				}
			}

			float scale = _Edge.GetGraphViewScale();
			// 绘制前半段
			painter.strokeColor = _FirstColor;
			painter.MoveTo(edgePoints[0] / scale);

			for (int i = 1; i <= midPointIndex; i++)
				painter.LineTo(edgePoints[i] / scale);
			painter.Stroke();

			// 绘制后半段
			painter.BeginPath();
			painter.strokeColor = _SecondColor;
			painter.MoveTo(edgePoints[midPointIndex] / scale);

			for (int i = midPointIndex + 1; i < edgePoints.Count; i++)
				painter.LineTo(edgePoints[i] / scale);
			painter.Stroke();
		}

		private List<Vector2> CalculateEdgePoints()
		{
			List<Vector2> points = new List<Vector2>();

			if (_Edge.input == null || _Edge.output == null)
				return points;

			// 计算贝塞尔曲线点
			Vector2 startPos = _Edge.output.GetGlobalCenter();
			Vector2 endPos = _Edge.input.GetGlobalCenter();
			float xDiff = Mathf.Abs(startPos.x - endPos.x) * 0.7f;
			Vector2 startCtrl = startPos + new Vector2(xDiff, 0);
			Vector2 endCtrl = endPos + new Vector2(-xDiff, 0);

			int segments = 100;
			for (int i = 0; i <= segments; i++)
			{
				float t = (float)i / segments;
				Vector3 point = CommonUtility.CalculateBezierPoint2(startPos.x0y(), startCtrl.x0y(), endCtrl.x0y(), endPos.x0y(), t);
				points.Add(point.xz() - parent.worldBound.position);
			}

			return points;
		}

		private void OnEdgeGeometryChanged(GeometryChangedEvent evt)
		{
			MarkDirtyRepaint();
		}
	}

	/// <summary>
	/// 两色分割的连线
	/// </summary>
	public class SplitColorEdge : Edge
	{
		private SplitColorEdgeOverlay _SplitColorEdgeOverlay;
		private Color _FirstColor;
		private Color _SecondColor;
		private GraphView _GraphView;

		public SplitColorEdge()
		{
			// 监听样式解析完成事件
			RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

			// 彻底禁用原生EdgeControl
			DisableNativeEdgeCompletely();
		}

		/// <summary>
		/// 设置两段的颜色
		/// </summary>
		public void SetSplitColor(Color first, Color second)
		{
			_FirstColor = first;
			_SecondColor = second;
			if (_SplitColorEdgeOverlay != null)
				Remove(_SplitColorEdgeOverlay);
			AddSplitColorEdgeOverlay();
		}

		public void UpdateEdge()
		{
			_SplitColorEdgeOverlay?.MarkDirtyRepaint();
		}

		public void SetGraphView(GraphView owner)
		{
			_GraphView = owner;
		}

		public float GetGraphViewScale()
		{
			return _GraphView.viewTransform.scale.x;
		}

		private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
		{
			// 确保EdgeControl已创建后添加覆盖
			if (_SplitColorEdgeOverlay == null && edgeControl != null)
				AddSplitColorEdgeOverlay();
		}

		private void AddSplitColorEdgeOverlay()
		{
			_SplitColorEdgeOverlay = new SplitColorEdgeOverlay(this, _FirstColor, _SecondColor);
			Add(_SplitColorEdgeOverlay);

			// 将覆盖层置于EdgeControl之上
			_SplitColorEdgeOverlay.BringToFront();
		}

		/// <summary>
		/// 彻底禁用原生EdgeControl
		/// </summary>
		private void DisableNativeEdgeCompletely()
		{
			// 通过反射获取EdgeControl并彻底禁用
			FieldInfo edgeControlField = typeof(Edge).GetField("m_EdgeControl", BindingFlags.NonPublic | BindingFlags.Instance);

			if (edgeControlField != null)
			{
				EdgeControl edgeControl = edgeControlField.GetValue(this) as EdgeControl;
				// 彻底从视觉树中移除
				if (edgeControl != null)
					edgeControl.RemoveFromHierarchy();
			}
		}
	}
}
