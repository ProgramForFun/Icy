using Cysharp.Threading.Tasks;
using Icy.Base;
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
		//一个加载请求没完成时，其他重复请求的处理：不去真的重复请求，而是记下来等唯一的一个加载完成后，分发给所有的重复请求
		//一个加载请求没完成时，取消操作的处理：Unity没有真的取消的接口，记下来加载完成后不增加引用计数，直接丢弃即可
		//HostServerURL改为EditorWindow配置

		private ResourcePackage _Package;
		private AssetPatcher _Patcher;

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
				string defaultHostServer = GetHostServerURL();
				string fallbackHostServer = GetHostServerURL();
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
				string defaultHostServer = GetHostServerURL();
				string fallbackHostServer = GetHostServerURL();
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

			Log.LogInfo($"AssetManager init end, {initializationOperation.Status}", "AssetManager");
			return initializationOperation.Status == EOperationStatus.Succeed;
			//SetPackage(defaultPackageName, true);
		}

		/// <summary>
		/// 获取资源服务器地址
		/// </summary>
		private string GetHostServerURL()
		{
			//string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
			string hostServerIP = "http://127.0.0.1";
			string appVersion = "v1.0";

#if UNITY_EDITOR
			if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
				return $"{hostServerIP}/CDN/Android/{appVersion}";
			else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
				return $"{hostServerIP}/CDN/IPhone/{appVersion}";
			else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
				return $"{hostServerIP}/CDN/WebGL/{appVersion}";
			else
				return $"{hostServerIP}/CDN/PC/{appVersion}";
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
			AssetHandle handle = _Package.LoadAssetAsync(asset);
			return new AssetRef(handle);
		}

		public AssetRef LoadAllAssetsAsync(string asset)
		{
			AllAssetsHandle handle = _Package.LoadAllAssetsAsync(asset);
			return new AssetRef(handle);
		}

		public AssetRef LoadSubAssetsAsync(string asset)
		{
			SubAssetsHandle handle = _Package.LoadSubAssetsAsync(asset);
			return new AssetRef(handle);
		}

		public AssetRef LoadSceneAsync(string asset)
		{
			SceneHandle handle = _Package.LoadSceneAsync(asset);
			return new AssetRef(handle);
		}

		public AssetRef LoadRawFileAsync(string asset)
		{
			RawFileHandle handle = _Package.LoadRawFileAsync(asset);
			return new AssetRef(handle);
		}
		#endregion
	}
}
