using Cysharp.Threading.Tasks;
using Icy.Base;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace Icy.Asset
{
	/// <summary>
	/// 运行时资源管理器，基于YooAsset；
	/// 业务侧运行时使用此管理器来管理资源，不应该直接访问YooAsset;
	/// </summary>
	public sealed class AssetManager : Singleton<AssetManager>
	{
		/// <summary>
		/// YooAsset Package
		/// </summary>
		private ResourcePackage _Package;
		/// <summary>
		/// 资源更新脚本
		/// </summary>
		private AssetPatcher _Patcher;
		/// <summary>
		/// 当前正在加载或已加载的资源
		/// </summary>
		private Dictionary<string, AssetRef> _Cached;

		#region Init
		/// <summary>
		/// 初始化资源系统
		/// </summary>
		public async UniTask<bool> Init(EPlayMode playMode, string defaultPackageName)
		{
			YooAssets.Initialize();

			_Package = YooAssets.TryGetPackage(defaultPackageName);
			if (_Package == null)
				_Package = YooAssets.CreatePackage(defaultPackageName);

			// 编辑器下的模拟模式
			InitializationOperation initializationOperation = null;
			if (playMode == EPlayMode.EditorSimulateMode)
			{
				PackageInvokeBuildResult buildResult = EditorSimulateModeHelper.SimulateBuild(defaultPackageName);
				string packageRoot = buildResult.PackageRootDirectory;
				EditorSimulateModeParameters createParameters = new EditorSimulateModeParameters();
				createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
				initializationOperation = _Package.InitializeAsync(createParameters);
			}

			// 单机运行模式
			if (playMode == EPlayMode.OfflinePlayMode)
			{
				OfflinePlayModeParameters createParameters = new OfflinePlayModeParameters();
				createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
				initializationOperation = _Package.InitializeAsync(createParameters);
			}

			// 联机运行模式
			if (playMode == EPlayMode.HostPlayMode)
			{
				string defaultHostServer = GetHostServerURL(true);
				string fallbackHostServer = GetHostServerURL(false);
				IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
				HostPlayModeParameters createParameters = new HostPlayModeParameters();
				createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
				createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
				initializationOperation = _Package.InitializeAsync(createParameters);
			}

			// WebGL运行模式
			if (playMode == EPlayMode.WebPlayMode)
			{
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
				WebPlayModeParameters createParameters = new WebPlayModeParameters();
				string defaultHostServer = GetHostServerURL(true);
				string fallbackHostServer = GetHostServerURL(false);
				string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE"; //注意：如果有子目录，请修改此处！
				IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
				createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices);
				initializationOperation = package.InitializeAsync(createParameters);
#else
				WebPlayModeParameters createParameters = new WebPlayModeParameters();
				createParameters.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
				initializationOperation = _Package.InitializeAsync(createParameters);
#endif
			}
			await initializationOperation;

			_Cached = new Dictionary<string, AssetRef>();

			Log.LogInfo($"AssetManager init end, {initializationOperation.Status}", "AssetManager");
			return initializationOperation.Status == EOperationStatus.Succeed;
			//SetPackage(defaultPackageName, true);
		}

		/// <summary>
		/// 获取资源服务器地址
		/// </summary>
		private string GetHostServerURL(bool isMain)
		{
			string hostServerAddress = GetAssetHostServerAddressFromSetting(isMain);
			if (string.IsNullOrEmpty(hostServerAddress))
			{
				Log.LogError("Asset host server address is empty, open Icy/Asset/Setting to set it");
				return null;
			}
			string appVersion = "v1.0";

#if UNITY_EDITOR
			if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
				return $"{hostServerAddress}/CDN/Android/{appVersion}";
			else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
				return $"{hostServerAddress}/CDN/IPhone/{appVersion}";
			else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
				return $"{hostServerAddress}/CDN/WebGL/{appVersion}";
			else
				return $"{hostServerAddress}/CDN/PC/{appVersion}";
#else
        if (Application.platform == RuntimePlatform.Android)
            return $"{hostServerAddress}/CDN/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerAddress}/CDN/IPhone/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerAddress}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerAddress}/CDN/PC/{appVersion}";
#endif
		}

		private string GetAssetHostServerAddressFromSetting(bool isMain)
		{
			string fullPath = Path.Combine(Application.streamingAssetsPath, "IcySettings", "AssetSetting.bin");
			if (File.Exists(fullPath))
			{
				byte[] bytes = File.ReadAllBytes(fullPath);
				AssetSetting assetSetting = AssetSetting.Descriptor.Parser.ParseFrom(bytes) as AssetSetting;
				return isMain ? assetSetting.AssetHostServerAddressMain : assetSetting.AssetHostServerAddressStandby;
			}
			else
				return null;
		}

		/// <summary>
		/// 远端资源地址查询服务类
		/// </summary>
		private class RemoteServices : IRemoteServices
		{
			private readonly string _defaultHostServer;
			private readonly string _fallbackHostServer;

			public RemoteServices(string defaultHostServer, string fallbackHostServer)
			{
				_defaultHostServer = defaultHostServer;
				_fallbackHostServer = fallbackHostServer;
			}
			string IRemoteServices.GetRemoteMainURL(string fileName)
			{
				return $"{_defaultHostServer}/{fileName}";
			}
			string IRemoteServices.GetRemoteFallbackURL(string fileName)
			{
				return $"{_fallbackHostServer}/{fileName}";
			}
		}
		#endregion

		/// <summary>
		/// 切换Package
		/// </summary>
		public void SetPackage(string packageName, bool isDefaultPackage = false)
		{
			_Package = YooAssets.GetPackage(packageName);
			Log.Assert(_Package != null, $"AssetManager SetPackage to {packageName} failed!");

			if (isDefaultPackage)
				YooAssets.SetDefaultPackage(_Package);
		}

		/// <summary>
		/// 获取当前Package的名字
		/// </summary>
		public string GetCurrentPackageName()
		{
			return _Package == null ? string.Empty : _Package.PackageName;
		}

		#region Patch
		/// <summary>
		/// 开始热更新资源
		/// </summary>
		public async UniTask StartPatch()
		{
			_Patcher = new AssetPatcher(_Package);
			while (!_Patcher.IsFinished)
				await UniTask.NextFrame();
		}
		#endregion

		#region Load
		/// <summary>
		/// 异步加载资源
		/// </summary>
		public AssetRef LoadAssetAsync(string address, uint priority = 0)
		{
			if (_Cached.ContainsKey(address))
				return _Cached[address];
			else
			{
				AssetHandle handle = _Package.LoadAssetAsync(address, priority);
				return CreateAssetRef(handle);
			}
		}

		/// <summary>
		/// 异步加载资源包内所有资源
		/// </summary>
		public AssetRef LoadAllAssetsAsync(string anyAssetAddressInBundle, uint priority = 0)
		{
			if (_Cached.ContainsKey(anyAssetAddressInBundle))
				return _Cached[anyAssetAddressInBundle];
			else
			{
				AllAssetsHandle handle = _Package.LoadAllAssetsAsync(anyAssetAddressInBundle, priority);
				return CreateAssetRef(handle);
			}
		}

		/// <summary>
		/// 异步加载子资源
		/// </summary>
		public AssetRef LoadSubAssetsAsync(string address, uint priority = 0)
		{
			if (_Cached.ContainsKey(address))
				return _Cached[address];
			else
			{
				SubAssetsHandle handle = _Package.LoadSubAssetsAsync(address, priority);
				return CreateAssetRef(handle);
			}
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		public AssetRef LoadSceneAsync(string address, LoadSceneMode sceneMode = LoadSceneMode.Single, LocalPhysicsMode physicsMode = LocalPhysicsMode.None, bool suspendLoad = false, uint priority = 0)
		{
			if (_Cached.ContainsKey(address))
				return _Cached[address];
			else
			{
				SceneHandle handle = _Package.LoadSceneAsync(address, sceneMode, physicsMode, suspendLoad, priority);
				return CreateAssetRef(handle);
			}
		}

		/// <summary>
		/// 异步加载原生文件
		/// </summary>
		public AssetRef LoadRawFileAsync(string address, uint priority = 0)
		{
			if (_Cached.ContainsKey(address))
				return _Cached[address];
			else
			{
				RawFileHandle handle = _Package.LoadRawFileAsync(address, priority);
				return CreateAssetRef(handle);
			}
		}

		internal void ReleaseAsset(HandleBase handleBase)
		{
			_Cached.Remove(handleBase.GetAssetInfo().Address);
			//场景的卸载，YooAsset有自己特有的方法
			if (handleBase is SceneHandle sceneHandle)
				sceneHandle.UnloadAsync();
			else
				handleBase.Release();
		}

		private AssetRef CreateAssetRef<T>(T handle) where T : HandleBase
		{
			AssetRef assetRef = new AssetRef(handle);
			string assetAddress = handle.GetAssetInfo().Address;
			_Cached.Add(assetAddress, assetRef);
			return assetRef;
		}
		#endregion

		#region Unload
		/// <summary>
		/// 卸载所有没有引用的AB
		/// </summary>
		public async UniTaskVoid UnloadUnusedAssets()
		{
			await _Package.UnloadUnusedAssetsAsync().ToUniTask();
		}

		/// <summary>
		/// 强制卸载所有AB，谨慎调用
		/// </summary>
		public async UniTaskVoid ForceUnloadAllAssets()
		{
			await _Package.UnloadAllAssetsAsync().ToUniTask();
		}

		/// <summary>
		/// 尝试卸载指定的资源，如果该资源还有引用，什么都不做
		/// </summary>
		public void TryUnloadUnusedAsset(string address)
		{
			_Package.TryUnloadUnusedAsset(address);
		}
		#endregion

		internal Sprite GetSprite(string spriteName)
		{
			AssetHandle loadHandle = _Package.LoadAssetSync<Sprite>(spriteName);
			return loadHandle.AssetObject as Sprite;
		}

		internal Texture GetTexture(string textureName)
		{
			AssetHandle loadHandle = _Package.LoadAssetSync<Texture>(textureName);
			return loadHandle.AssetObject as Texture;
		}
	}
}
