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
