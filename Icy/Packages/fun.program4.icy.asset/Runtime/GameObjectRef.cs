using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Icy.Asset
{
	/// <summary>
	/// 基于AssetRef封装的GameObejctRef，方便使用
	/// </summary>
	public class GameObjectRef
	{
		/// <summary>
		/// 持有的GameObject，命名遵循Unity
		/// </summary>
		public GameObject gameObject { get; protected set; }
		/// <summary>
		/// 便利转发，命名遵循Unity
		/// </summary>
		public Transform transform => gameObject.transform;

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
		public event Action<bool> OnLoadFinish;

		/// <summary>
		/// 内部持有的AssetRef
		/// </summary>
		protected AssetRef _AssetRef;

		public GameObjectRef(string address)
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

		protected void OnAssetRefFinish(AssetRef assetRef)
		{
			if (assetRef.IsSucceed)
				gameObject = GameObject.Instantiate(assetRef.AssetObject) as GameObject;
			OnLoadFinish?.Invoke(assetRef.IsSucceed);
		}

		public void Destroy()
		{
			GameObject.Destroy(gameObject);
			_AssetRef.Release();
		}
	}
}
