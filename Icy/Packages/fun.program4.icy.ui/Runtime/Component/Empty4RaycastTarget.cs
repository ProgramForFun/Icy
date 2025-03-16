using UnityEngine;
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// 使用alpha为0的Image实现不可见的可交互区域，会导致Overdraw；
	/// 因为UGUI仍会去渲染alpha为0的控件；
	/// 因此实现一个只在逻辑上响应Raycast但是不参与绘制的组件，降低Fill Rate
	/// </summary>
	[RequireComponent(typeof(CanvasRenderer))]
	public class Empty4RaycastTarget : MaskableGraphic
	{
		protected Empty4RaycastTarget()
		{
			useLegacyMeshGeneration = false;
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			toFill.Clear();
		}
	}
}
