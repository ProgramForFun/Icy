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

namespace Icy.Base.Editor
{
	/// <summary>
	/// 自绘的两色分割连线本体
	/// </summary>
	public class GradientEdgeOverlay : VisualElement
	{
		private Edge _Edge;
		private float _LineWidth;

		public GradientEdgeOverlay(Edge targetEdge, float width)
		{
			_Edge = targetEdge;
			_LineWidth = width;

			// 设置为绝对定位，覆盖在原始边上
			style.position = Position.Absolute;
			style.left = 0;
			style.top = 0;
			style.right = 0;
			style.bottom = 0;

			generateVisualContent += OnGenerateVisualContent;

			// 监听边的变化
			_Edge.RegisterCallback<GeometryChangedEvent>(OnEdgeGeometryChanged);
		}

		private void OnEdgeGeometryChanged(GeometryChangedEvent evt)
		{
			MarkDirtyRepaint();
		}

		private void OnGenerateVisualContent(MeshGenerationContext context)
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

			//分割点取所有点中，距离两个Port距离最平均的
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

			// 绘制前半段（第一种颜色）
			painter.strokeColor = Color.red;
			painter.MoveTo(edgePoints[0]);

			for (int i = 1; i <= midPointIndex; i++)
			{
				painter.LineTo(edgePoints[i]);
			}
			painter.Stroke();

			// 绘制后半段（第二种颜色）
			painter.BeginPath();
			painter.strokeColor = Color.blue;
			painter.MoveTo(edgePoints[midPointIndex]);

			for (int i = midPointIndex + 1; i < edgePoints.Count; i++)
			{
				painter.LineTo(edgePoints[i]);
			}
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

			// 简单的线性插值（可以根据需要实现贝塞尔曲线）
			int segments = 20;
			for (int i = 0; i <= segments; i++)
			{
				float t = (float)i / segments;
				Vector2 point = Vector2.Lerp(startPos, endPos, t);

				// 添加一些曲线效果
				if (i > 0 && i < segments)
				{
					float curveStrength = 50f;
					point.y += Mathf.Sin(t * Mathf.PI) * curveStrength;
				}

				points.Add(point - parent.worldBound.position);
			}

			return points;
		}
	}

	/// <summary>
	/// 两色分割的连线
	/// </summary>
	public class GradientEdge : Edge
	{
		private GradientEdgeOverlay _GradientOverlay;

		public GradientEdge()
		{
			// 监听样式解析完成事件
			RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

			// 彻底禁用原生EdgeControl
			DisableNativeEdgeCompletely();
		}

		private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
		{
			// 确保EdgeControl已创建后添加渐变覆盖
			if (_GradientOverlay == null && edgeControl != null)
				AddGradientOverlay();
		}

		private void AddGradientOverlay()
		{
			_GradientOverlay = new GradientEdgeOverlay(this, 3f);
			Add(_GradientOverlay);

			// 将覆盖层置于EdgeControl之上
			_GradientOverlay.BringToFront();
		}

		/// <summary>
		/// 彻底禁用原生EdgeControl
		/// </summary>
		private void DisableNativeEdgeCompletely()
		{
			// 通过反射获取EdgeControl并彻底禁用
			System.Reflection.FieldInfo edgeControlField = typeof(Edge).GetField("m_EdgeControl",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

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
