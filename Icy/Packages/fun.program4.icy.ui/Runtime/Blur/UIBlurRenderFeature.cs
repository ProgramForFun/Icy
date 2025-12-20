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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Icy.UI
{
	/// <summary>
	/// 配合UIBlur使用的RendererFeature
	/// </summary>
	public class UIBlurRenderFeature : ScriptableRendererFeature
	{
		public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRendering;

		private UIBlurRenderPass _BlurPass;

		public override void Create()
		{
			_BlurPass = new UIBlurRenderPass();
			_BlurPass.renderPassEvent = RenderPassEvent;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			renderer.EnqueuePass(_BlurPass);
		}
	}

	public class UIBlurRenderPass : ScriptableRenderPass
	{
		static public event Action<ScriptableRenderContext, RenderingData> OnExecute;

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			OnExecute?.Invoke(context, renderingData);
		}

		public static void ClearEvent()
		{
			OnExecute = null;
		}
	}
}
