
namespace Icy.UI
{
	public static class UguiExtension
	{
		/// <summary>
		/// 扩展方法，以便直接用sprite名字通过资源系统切换Image
		/// </summary>
		public static void SetSprite(this UnityEngine.UI.Image image, string spriteName)
		{
			image.sprite = UIManager.Instance.GetSprite(spriteName);
		}

		/// <summary>
		/// 扩展方法，以便直接用texture名字通过资源系统切换RawImage
		/// </summary>
		public static void SetTexture(this UnityEngine.UI.RawImage image, string textureName)
		{
			image.texture = UIManager.Instance.GetTexture(textureName);
		}
	}
}
