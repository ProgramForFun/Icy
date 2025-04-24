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
		/// 是否已加载结束，不关心成功还是失败
		/// </summary>
		public bool IsFinish => _AssetRef.IsFinish;
		/// <summary>
		/// 是否已加载成功
		/// </summary>
		public bool IsSucceed => _AssetRef.IsSucceed;
		/// <summary>
		/// 加载完成的回调
		/// </summary>
		public event Action<bool> OnLoadFinish
		{
			add
			{
				_OnFinish += value;
				if (IsFinish)
					value?.Invoke(IsSucceed);
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


		public GameObjectRefPool(string address, int defaultSize = 16) : base(null, defaultSize)
		{
			_AssetRef = AssetManager.Instance.LoadAssetAsync(address);
			_AssetRef.Retain();
			_AssetRef.OnFinish += OnAssetRefFinish;
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
				_Template = GameObject.Instantiate(assetRef.AssetObject) as GameObject;
			_OnFinish?.Invoke(assetRef.IsSucceed);
		}

		public override void Dispose()
		{
			_AssetRef.Release();
			base.Dispose();
		}
	}
}
