using UnityEngine;
using YooAsset;
using Icy.Base;

namespace Icy.Asset
{
	/// <summary>
	/// 资源文件偏移解密类
	/// </summary>
	public class DecryptionOffset : IDecryptionServices
	{
		//避免静态分析
		private const ulong _OffsetA = 29;
		private const ulong _OffsetB = 22;

		/// <summary>
		/// 同步方式获取解密的资源包对象
		/// 注意：加载流对象在资源包对象释放的时候会自动释放
		/// </summary>
		DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
		{
			DecryptResult decryptResult = new DecryptResult();
			decryptResult.ManagedStream = null;
			decryptResult.Result = AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, GetFileOffset());
			return decryptResult;
		}

		/// <summary>
		/// 异步方式获取解密的资源包对象
		/// 注意：加载流对象在资源包对象释放的时候会自动释放
		/// </summary>
		DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
		{
			DecryptResult decryptResult = new DecryptResult();
			decryptResult.ManagedStream = null;
			decryptResult.CreateRequest = AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, GetFileOffset());
			return decryptResult;
		}

		/// <summary>
		/// 获取解密的字节数据
		/// </summary>
		byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// 获取解密的文本数据
		/// </summary>
		string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
		{
			throw new System.NotImplementedException();
		}

		public static ulong GetFileOffset()
		{
			return _OffsetA + _OffsetB;
		}

		public DecryptResult LoadAssetBundleFallback(DecryptFileInfo fileInfo)
		{
			Log.LogError("Decrypt failed, fallback");
			return default;
		}
	}
}
