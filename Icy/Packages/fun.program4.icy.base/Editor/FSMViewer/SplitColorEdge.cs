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

namespace Icy.Base.Editor
{
	public class GradientEdgeOverlay : VisualElement
	{
		private Edge edge;
		private float lineWidth;

		public GradientEdgeOverlay(Edge targetEdge, float width = 4f)
		{
			edge = targetEdge;
			lineWidth = width;

			// 设置为绝对定位，覆盖在原始边上
			style.position = Position.Absolute;
			style.left = 0;
			style.top = 0;
			style.right = 0;
			style.bottom = 0;

			generateVisualContent += OnGenerateVisualContent;

			// 监听边的变化
			edge.RegisterCallback<GeometryChangedEvent>(OnEdgeGeometryChanged);
		}

		private void OnEdgeGeometryChanged(GeometryChangedEvent evt)
		{
			MarkDirtyRepaint();
		}

		private void OnGenerateVisualContent(MeshGenerationContext context)
		{
			if (edge?.edgeControl == null) return;

			var edgePoints = CalculateEdgePoints();
			if (edgePoints.Count < 2) return;

			var painter = context.painter2D;

			painter.BeginPath();
			painter.lineWidth = lineWidth;
			painter.lineJoin = LineJoin.Round;
			painter.lineCap = LineCap.Round;

			int midPointIndex = edgePoints.Count / 2;

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

		private System.Collections.Generic.List<Vector2> CalculateEdgePoints()
		{
			var points = new System.Collections.Generic.List<Vector2>();

			if (edge.input == null || edge.output == null)
				return points;

			// 计算贝塞尔曲线点
			Vector2 startPos = edge.output.GetGlobalCenter();
			Vector2 endPos = edge.input.GetGlobalCenter();

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

	public class GradientEdge : Edge
	{
		private GradientEdgeOverlay gradientOverlay;

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
			if (gradientOverlay == null && edgeControl != null)
				AddGradientOverlay();
		}

		private void AddGradientOverlay()
		{
			gradientOverlay = new GradientEdgeOverlay(this, 3f);
			Add(gradientOverlay);

			// 将覆盖层置于EdgeControl之上
			gradientOverlay.BringToFront();
		}

		/// <summary>
		/// 彻底禁用原生EdgeControl
		/// </summary>
		private void DisableNativeEdgeCompletely()
		{
			// 通过反射获取EdgeControl并彻底禁用
			var edgeControlField = typeof(Edge).GetField("m_EdgeControl",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			if (edgeControlField != null)
			{
				var edgeControl = edgeControlField.GetValue(this) as EdgeControl;
				if (edgeControl != null)
				{
					// 彻底从视觉树中移除
					edgeControl.RemoveFromHierarchy();
				}
			}
		}
	}
}
