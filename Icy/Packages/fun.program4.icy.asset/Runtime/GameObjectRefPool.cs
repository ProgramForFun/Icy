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


using Cysharp.Threading.Tasks;
using Icy.Base;
using System;
using UnityEngine;

namespace Icy.Asset
{
	/// <summary>
	/// 基于GameObjectPool，接受GameObject的资源地址做为模板
	/// </summary>
	public class GameObjectRefPool : GameObjectPool
	{
		/// <summary>
		/// 是否已准备好、可以开始使用了
		/// </summary>
		public bool IsReady => _AssetRef.IsSucceed;
		/// <summary>
		/// 资源加载的最近错误信息，如果有的话
		/// </summary>
		public string LastError => _AssetRef.LastError;
		/// <summary>
		/// 资源加载完成的回调
		/// </summary>
		public event Action<bool> OnLoadFinish
		{
			add
			{
				_OnFinish += value;
				if (_AssetRef.IsFinish)
					value?.Invoke(IsReady);
			}
			remove => _OnFinish -= value;
		}

		/// <summary>
		/// 内部持有的AssetRef
		/// </summary>
		protected AssetRef _AssetRef;
		/// <summary>
		/// 加载完成的回调
		/// </summary>
		protected Action<bool> _OnFinish;


		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="address">资源路径</param>
		/// <param name="syncLoad">同步加载，默认为false</param>
		/// <param name="defaultSize">同步加载，默认为false</param>
		public GameObjectRefPool(string address, bool syncLoad = false, int defaultSize = 16) : base(null, defaultSize)
		{
			if (syncLoad)
			{
				_AssetRef = AssetManager.Instance.LoadAsset(address);
				_AssetRef.Retain();
				OnAssetRefFinish(_AssetRef);
			}
			else
			{
				_AssetRef = AssetManager.Instance.LoadAssetAsync(address);
				_AssetRef.Retain();
				_AssetRef.OnFinish += OnAssetRefFinish;
			}
		}

		/// <summary>
		/// UniTask支持
		/// </summary>
		public UniTask ToUniTask(IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
		{
			return _AssetRef.ToUniTask(progress, timing);
		}

		protected virtual void OnAssetRefFinish(AssetRef assetRef)
		{
			if (assetRef.IsSucceed)
			{
				_Template = GameObject.Instantiate(assetRef.AssetObject) as GameObject;
				_Template.RemoveCloneSuffix();
			}
			_OnFinish?.Invoke(assetRef.IsSucceed);
		}

		public override void Dispose()
		{
			base.Dispose();
			_AssetRef.Release();
		}
	}
}
