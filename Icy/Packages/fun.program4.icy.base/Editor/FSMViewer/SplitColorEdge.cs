using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class GradientEdgeOverlay : VisualElement
{
	private Edge edge;
	private Gradient gradient;
	private float lineWidth;

	public GradientEdgeOverlay(Edge targetEdge, Gradient edgeGradient, float width = 4f)
	{
		edge = targetEdge;
		gradient = edgeGradient;
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
		painter.strokeColor = gradient.Evaluate(0f);
		painter.MoveTo(edgePoints[0]);

		for (int i = 1; i <= midPointIndex; i++)
		{
			painter.LineTo(edgePoints[i]);
		}
		painter.Stroke();

		// 绘制后半段（第二种颜色）
		painter.BeginPath();
		painter.strokeColor = gradient.Evaluate(1f);
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
	public Gradient EdgeGradient { get; private set; }
	private GradientEdgeOverlay gradientOverlay;

	public GradientEdge()
	{
		// 创建默认渐变
		EdgeGradient = new Gradient()
		{
			colorKeys = new GradientColorKey[]
			{
				new GradientColorKey(Color.cyan, 0f),
				new GradientColorKey(Color.magenta, 1f)
			}
		};

		// 监听样式解析完成事件
		RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

		// 尝试彻底禁用原生EdgeControl
		schedule.Execute(() => {
			DisableNativeEdgeCompletely();
		}).StartingIn(1); // 延迟执行确保EdgeControl已创建
	}

	private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
	{
		// 确保EdgeControl已创建后添加渐变覆盖
		if (gradientOverlay == null && edgeControl != null)
		{
			AddGradientOverlay();
		}
	}

	private void AddGradientOverlay()
	{
		gradientOverlay = new GradientEdgeOverlay(this, EdgeGradient, 3f);
		Add(gradientOverlay);

		// 将覆盖层置于EdgeControl之上
		gradientOverlay.BringToFront();
	}

	public void SetGradient(Gradient newGradient)
	{
		EdgeGradient = newGradient;
		if (gradientOverlay != null)
		{
			// 需要重新创建或更新覆盖层
			gradientOverlay.RemoveFromHierarchy();
			gradientOverlay = null;
			AddGradientOverlay();
		}
		MarkDirtyRepaint();
	}

	public override void OnSelected()
	{
		base.OnSelected();
		// 选中时改变渐变
		SetSelectionGradient();
	}

	public override void OnUnselected()
	{
		base.OnUnselected();
		// 恢复原渐变
		RestoreOriginalGradient();
	}

	private void SetSelectionGradient()
	{
		var selectedGradient = new Gradient()
		{
			colorKeys = new GradientColorKey[]
			{
				new GradientColorKey(Color.yellow, 0f),
				new GradientColorKey(Color.red, 1f)
			}
		};
		SetGradient(selectedGradient);
	}

	private void RestoreOriginalGradient()
	{
		// 恢复原始渐变
		SetGradient(EdgeGradient);
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

public class GradientEdgeControl : EdgeControl
{
	public Gradient ColorGradient { get; set; }
	public float CustomEdgeWidth { get; set; } = 4f;

	public GradientEdgeControl()
	{
		// 设置默认渐变
		ColorGradient = new Gradient()
		{
			colorKeys = new GradientColorKey[]
			{
				new GradientColorKey(Color.blue, 0f),
				new GradientColorKey(Color.green, 0.5f),
				new GradientColorKey(Color.red, 1f)
			},
			alphaKeys = new GradientAlphaKey[]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 1f)
			}
		};
	}

	// 重写边的宽度属性
	//public override float edgeWidth
	//{
	//	get => CustomEdgeWidth;
	//	set => CustomEdgeWidth = value;
	//}

	// 通过修改控制点来影响绘制
	public override void UpdateLayout()
	{
		base.UpdateLayout();

		// 强制重绘
		MarkDirtyRepaint();
	}

	// 使用Shader或Material来实现渐变效果
	public void ApplyGradientToMesh()
	{
		// 这里需要通过修改顶点颜色或使用自定义Shader
		// 由于EdgeControl的绘制是内部的，我们需要采用其他方法
	}
}
