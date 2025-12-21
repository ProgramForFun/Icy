/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *	 http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Icy.UI
{
	/// <summary>
	/// UI的背景模糊
	/// </summary>
	[RequireComponent(typeof(UnityEngine.UI.RawImage))]
	public class UIBlur : MonoBehaviour
	{
		[Tooltip("使用的Shader")]
		public Shader Shader;

		[Range(0, 6), Tooltip("[降采样次数] 向下采样的次数；此值越大，则采样间隔越大，需要处理的像素点越少，运行速度越快")]
		public int DownSampleNum = 2;

		[Range(0.0f, 20.0f), Tooltip("[模糊扩散度] 进行高斯模糊时，相邻像素点的间隔；此值越大相邻像素间隔越远，图像越模糊，但过大的值会导致失真")]
		public float BlurSpreadSize = 5.0f;

		[Range(0, 8), Tooltip("[迭代次数] 此值越大，则模糊操作的迭代次数越多，模糊效果越好，消耗也就越大")]
		public int BlurIterations = 1;

		[Tooltip("模糊之后叠加的颜色")]
		public Color BlurColor = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f);

		/// <summary>
		/// 挂载的Canvas
		/// </summary>
		public Canvas Canvas { get; private set; }
		/// <summary>
		/// 渲染逻辑用到的CommandBuffer
		/// </summary>
		private CommandBuffer _CmdBuffer;
		/// <summary>
		/// 渲染RT的RawImage
		/// </summary>
		private UnityEngine.UI.RawImage _RawImage;
		/// <summary>
		/// 渲染完赋给RawImage的RT
		/// </summary>
		private RenderTexture _MainTexture;
		/// <summary>
		/// RT的格式
		/// </summary>
		private RenderTextureFormat _RTFormat = RenderTextureFormat.RGB111110Float;
		/// <summary>
		/// 创建的材质
		/// </summary>
		private Material _Material;
		/// <summary>
		/// DownSampleValue Property ID
		/// </summary>
		private int _DownSampleValueID;


		protected void Awake()
		{
			Canvas = GetComponent<Canvas>();
			if (_RawImage == null)
				_RawImage = GetComponent<UnityEngine.UI.RawImage>();
			_RawImage.enabled = false;
			if (!SystemInfo.SupportsRenderTextureFormat(_RTFormat))
				_RTFormat = RenderTextureFormat.DefaultHDR;
			_DownSampleValueID = Shader.PropertyToID("_DownSampleValue");
			_CmdBuffer = new CommandBuffer() { name = "BlurCmd" };
		}

		protected void Start()
		{
			enabled = true;
			UIBlurRenderPass.OnExecute += RenderBlur;
		}

		private Material GetMat()
		{
			if (_Material == null)
			{
				_Material = new Material(Shader);
				_Material.hideFlags = HideFlags.HideAndDontSave;
			}
			return _Material;
		}

		private void RenderBlur(ScriptableRenderContext context, RenderingData renderingData)
		{
			if (!enabled)
				return;

			if (renderingData.cameraData.camera != UIRoot.Instance.UICamera)
				return;

			Material mat = GetMat();

			_CmdBuffer.Clear();

			RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height);
			_CmdBuffer.Blit(BuiltinRenderTextureType.CurrentActive, rt);

			int renderWidth = Screen.width >> DownSampleNum;
			int renderHeight = Screen.height >> DownSampleNum;
			_MainTexture = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, _RTFormat);
			_MainTexture.filterMode = FilterMode.Bilinear;

			float widthMod = 1.0f / (1.0f * (1 << DownSampleNum));
			mat.SetFloat(_DownSampleValueID, BlurSpreadSize * widthMod);
			_CmdBuffer.Blit(rt, _MainTexture, mat, 0);

			for (int i = 0; i < BlurIterations; i++)
			{
				float iterationOffs = i;
				mat.SetFloat(_DownSampleValueID, BlurSpreadSize * widthMod + iterationOffs);

				RenderTexture tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, _RTFormat);
				_CmdBuffer.Blit(_MainTexture, tempBuffer, mat, 1);
				RenderTexture.ReleaseTemporary(_MainTexture);
				_MainTexture = tempBuffer;

				tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, _RTFormat);
				_CmdBuffer.Blit(_MainTexture, tempBuffer, mat, 2);
				RenderTexture.ReleaseTemporary(_MainTexture);
				_MainTexture = tempBuffer;
			}

			context.ExecuteCommandBuffer(_CmdBuffer);
			_CmdBuffer.Clear();

			_RawImage.texture = _MainTexture;
			_RawImage.color = BlurColor;
			_RawImage.enabled = true;
			enabled = false;

			RenderTexture.ReleaseTemporary(rt);

			UIBlurRenderPass.OnExecute -= RenderBlur;
		}

		private void Cleanup()
		{
			if (_Material)
				Object.DestroyImmediate(_Material);
			_Material = null;

			if (_MainTexture)
				RenderTexture.ReleaseTemporary(_MainTexture);
			_MainTexture = null;

			_RawImage.enabled = false;
		}

		protected void OnDestroy()
		{
			Cleanup();
			_CmdBuffer.Dispose();
		}
	}
}
