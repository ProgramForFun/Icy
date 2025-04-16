using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using YooAsset;

namespace Icy.Asset
{
	/// <summary>
	/// 
	/// </summary>
	public class AssetRef
	{
		public float Progress => _AssetHandle.Progress;
		public bool IsDone => _AssetHandle.IsDone;
		public bool IsValid => _AssetHandle.IsValid;

		protected HandleBase _AssetHandle;
		protected int _RefCount;

		internal AssetRef(HandleBase handle)
		{
			_AssetHandle = handle;
			_RefCount = 1;
		}

		public UnityEngine.Object AssetObject
		{
			get
			{
				if (_AssetHandle is AssetHandle assetHandle)
					return assetHandle.AssetObject;

				Log.LogError($"Unexpected method to get asset, handle = {_AssetHandle.GetType().Name}", "AssetRef");
				return null;
			}
		}

		public IReadOnlyList<UnityEngine.Object> AssetObjects
		{
			get
			{
				if (_AssetHandle is AllAssetsHandle allAssetHandle)
					return allAssetHandle.AllAssetObjects;
				else if (_AssetHandle is SubAssetsHandle subAssetHandle)
					return subAssetHandle.SubAssetObjects;

				Log.LogError($"Unexpected method to get asset, handle = {_AssetHandle.GetType().Name}", "AssetRef");
				return null;
			}
		}

		public Scene SceneObject
		{
			get
			{
				if (_AssetHandle is SceneHandle sceneHandle)
					return sceneHandle.SceneObject;

				Log.LogError($"Unexpected method to get asset, handle = {_AssetHandle.GetType().Name}", "AssetRef");
				return default;
			}
		}

		public byte[] RawData
		{
			get
			{
				if (_AssetHandle is RawFileHandle rawFileHandle)
					return rawFileHandle.GetRawFileData();

				Log.LogError($"Unexpected method to get asset, handle = {_AssetHandle.GetType().Name}", "AssetRef");
				return null;
			}
		}

		/// <summary>
		/// 引用计数+1
		/// </summary>
		public void Retain()
		{
			_RefCount++;
		}

		/// <summary>
		/// 引用计数-1，如果<=0会释放资源
		/// </summary>
		public void Release()
		{
			_RefCount--;
			if (_RefCount <= 0)
				_AssetHandle.Release();
		}

		/// <summary>
		/// UniTask支持
		/// </summary>
		public UniTask ToUniTask(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return _AssetHandle.ToUniTask(progress, timing);
		}
	}
}
