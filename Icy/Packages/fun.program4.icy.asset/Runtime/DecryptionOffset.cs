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
			Log.Error("Decrypt failed, fallback");
			return default;
		}
	}
}
