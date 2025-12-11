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


#define AssetRef_Log

using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace Icy.Asset
{
	/// <summary>
	/// Bundle资源的引用
	/// </summary>
	public class AssetRef
	{
		/// <summary>
		/// 可寻址地址
		/// </summary>
		public string Address => _AssetHandle.GetAssetInfo().Address;
		/// <summary>
		/// 资源完整路径
		/// </summary>
		public string AssetPath => _AssetHandle.GetAssetInfo().AssetPath;
		/// <summary>
		/// 当前加载的进度
		/// </summary>
		public float Progress => _AssetHandle.Progress;
		/// <summary>
		/// 是否已加载结束，不关心成功还是失败
		/// </summary>
		public bool IsFinish => _AssetHandle.IsDone;
		/// <summary>
		/// 是否已加载成功
		/// </summary>
		public bool IsSucceed => _AssetHandle.Status == EOperationStatus.Succeed;
		/// <summary>
		/// 当前AssetRef是否还有效
		/// </summary>
		public bool IsValid => _AssetHandle.IsValid;
		/// <summary>
		/// 加载完成的回调
		/// </summary>
		public event Action<AssetRef> OnFinish
		{
			add
			{
				_OnFinish += value;
				if (IsFinish)
					value?.Invoke(this);
			}
			remove => _OnFinish -= value;
		}

		/// <summary>
		/// 内部持有的YooAsset资源句柄
		/// </summary>
		protected HandleBase _AssetHandle;
		/// <summary>
		/// 引用计数
		/// </summary>
		protected int _RefCount;
		/// <summary>
		/// 加载完成的回调
		/// </summary>
		protected Action<AssetRef> _OnFinish;

		internal AssetRef(HandleBase handleBase)
		{
			_AssetHandle = handleBase;
			_RefCount = 0;

			if (handleBase is AssetHandle assetHandle)
				assetHandle.Completed += OnAnyAssetLoadCompleted;
			else if (handleBase is AllAssetsHandle allAssetsHandle)
				allAssetsHandle.Completed += OnAnyAssetLoadCompleted;
			else if (handleBase is SubAssetsHandle subAssetsHandle)
				subAssetsHandle.Completed += OnAnyAssetLoadCompleted;
			else if (handleBase is SceneHandle sceneHandle)
				sceneHandle.Completed += OnAnyAssetLoadCompleted;
			else if (handleBase is RawFileHandle rawFileHandle)
				rawFileHandle.Completed += OnAnyAssetLoadCompleted;
			else
				Log.Assert(false, $"Unsupported HandleBase derived class {handleBase.GetType().Name}");
		}

		/// <summary>
		/// 使用AssetManager.LoadAssetAsync加载的资源，使用此属性获取资源
		/// </summary>
		public UnityEngine.Object AssetObject
		{
			get
			{
				if (_AssetHandle is AssetHandle assetHandle)
					return assetHandle.AssetObject;

				Log.Error($"Unexpected method to get asset, handle = {_AssetHandle.GetType().Name}", nameof(AssetRef));
				return null;
			}
		}

		/// <summary>
		/// 使用AssetManager.LoadAllAssetsAsync 和 AssetManager.LoadSubAssetsAsync加载的资源，使用此属性获取资源
		/// </summary>
		public IReadOnlyList<UnityEngine.Object> AllAssetObjects
		{
			get
			{
				if (_AssetHandle is AllAssetsHandle allAssetHandle)
					return allAssetHandle.AllAssetObjects;
				else if (_AssetHandle is SubAssetsHandle subAssetHandle)
					return subAssetHandle.SubAssetObjects;

				Log.Error($"Unexpected method to get asset, handle = {_AssetHandle.GetType().Name}", nameof(AssetRef));
				return null;
			}
		}

		/// <summary>
		/// 使用AssetManager.LoadSceneAsync加载的场景资源，使用此属性获取场景
		/// </summary>
		public Scene SceneObject
		{
			get
			{
				if (_AssetHandle is SceneHandle sceneHandle)
					return sceneHandle.SceneObject;

				Log.Error($"Unexpected method to get asset, handle = {_AssetHandle.GetType().Name}", nameof(AssetRef));
				return default;
			}
		}

		/// <summary>
		/// 使用AssetManager.LoadRawFileAsync加载的原生资源，使用此属性获取原生数据
		/// </summary>
		public byte[] RawData
		{
			get
			{
				if (_AssetHandle is RawFileHandle rawFileHandle)
					return rawFileHandle.GetRawFileData();

				Log.Error($"Unexpected method to get asset, handle = {_AssetHandle.GetType().Name}", nameof(AssetRef));
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
		/// 引用计数-1，计数<=0释放资源
		/// </summary>
		public void Release()
		{
			_RefCount--;
			if (_RefCount <= 0)
			{
#if AssetRef_Log
				Log.SetColorOnce(Color.yellow);
				Log.Info($"Asset {_AssetHandle.GetAssetInfo().Address} RefCount <= 0, released", nameof(AssetRef), true);
#endif
				AssetManager.Instance.ReleaseAsset(_AssetHandle);
			}
		}

		/// <summary>
		/// UniTask支持
		/// </summary>
		public UniTask ToUniTask(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return _AssetHandle.ToUniTask(progress, timing);
		}

		private void OnAnyAssetLoadCompleted(HandleBase handle)
		{
			_OnFinish?.Invoke(this);
#if AssetRef_Log
			Log.SetColorOnce(Color.yellow);
			Log.Info($"Asset {handle.GetAssetInfo().Address} loaded", nameof(AssetRef), true);
#endif
		}
	}
}
