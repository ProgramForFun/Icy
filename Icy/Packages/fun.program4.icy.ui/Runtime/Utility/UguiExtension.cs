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


namespace Icy.UI
{
	public static class UguiExtension
	{
		/// <summary>
		/// 扩展方法，以便直接用sprite名字通过资源系统切换Image
		/// </summary>
		public static void SetSprite(this UnityEngine.UI.Image image, string spriteName)
		{
			UIBase ui = UIUtility.GetUIFromParent(image.transform);
			image.sprite = UIManager.Instance.GetSprite(ui, spriteName);
		}

		/// <summary>
		/// 扩展方法，以便直接用texture名字通过资源系统切换RawImage
		/// </summary>
		public static void SetTexture(this UnityEngine.UI.RawImage rawImage, string textureName)
		{
			UIBase ui = UIUtility.GetUIFromParent(rawImage.transform);
			rawImage.texture = UIManager.Instance.GetTexture(ui, textureName);
		}
	}
}
