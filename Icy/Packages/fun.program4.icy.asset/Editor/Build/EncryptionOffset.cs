using System;
using System.IO;
using YooAsset;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 文件偏移加密
	/// </summary>
	public class EncryptionOffset : IEncryptionServices
	{
		public EncryptResult Encrypt(EncryptFileInfo fileInfo)
		{
			int offset = 32;
			byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
			byte[] encryptedData = new byte[fileData.Length + offset];
			Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

			EncryptResult result = new EncryptResult();
			result.Encrypted = true;
			result.EncryptedData = encryptedData;
			return result;
		}
	}
}
