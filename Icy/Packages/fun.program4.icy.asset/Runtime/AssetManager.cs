using Cysharp.Threading.Tasks;
using Icy.Base;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
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

		public void SetPackage(string packageName, bool isDefaultPackage = false)
		{
			_Package = YooAssets.GetPackage(packageName);
			Log.Assert(_Package != null, $"AssetManager SetPackage to {packageName} failed!");

			if (isDefaultPackage)
				YooAssets.SetDefaultPackage(_Package);
		}

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
		public AssetRef LoadAssetAsync(string asset)
		{
			if (_Cached.ContainsKey(asset))
				return _Cached[asset];
			else
			{
				AssetHandle handle = _Package.LoadAssetAsync(asset);
				return CreateAssetRef(handle);
			}
		}

		public AssetRef LoadAllAssetsAsync(string asset)
		{
			if (_Cached.ContainsKey(asset))
				return _Cached[asset];
			else
			{
				AllAssetsHandle handle = _Package.LoadAllAssetsAsync(asset);
				return CreateAssetRef(handle);
			}
		}

		public AssetRef LoadSubAssetsAsync(string asset)
		{
			if (_Cached.ContainsKey(asset))
				return _Cached[asset];
			else
			{
				SubAssetsHandle handle = _Package.LoadSubAssetsAsync(asset);
				return CreateAssetRef(handle);
			}
		}

		public AssetRef LoadSceneAsync(string asset)
		{
			if (_Cached.ContainsKey(asset))
				return _Cached[asset];
			else
			{
				SceneHandle handle = _Package.LoadSceneAsync(asset);
				return CreateAssetRef(handle);
			}
		}

		public AssetRef LoadRawFileAsync(string asset)
		{
			if (_Cached.ContainsKey(asset))
				return _Cached[asset];
			else
			{
				RawFileHandle handle = _Package.LoadRawFileAsync(asset);
				return CreateAssetRef(handle);
			}
		}

		internal void ReleaseAsset(HandleBase handleBase)
		{
			_Cached.Remove(handleBase.GetAssetInfo().Address);
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
	}
}
