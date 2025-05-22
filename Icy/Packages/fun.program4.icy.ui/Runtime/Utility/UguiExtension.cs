
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
