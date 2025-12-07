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
		/// 当前Package是否开启了Addressable
		/// </summary>
		public bool IsAddressable => _Package.GetPackageDetails().EnableAddressable;
		/// <summary>
		/// 资源相关设置
		/// </summary>
		public AssetSetting AssetSetting { get; private set; }
		/// <summary>
		/// YooAsset Package
		/// </summary>
		private ResourcePackage _Package;
		/// <summary>
		/// 资源更新脚本
		/// </summary>
		private AssetPatcher _AssetPatcher;
		/// <summary>
		/// HybridCLR运行时加载
		/// </summary>
		private HybridCLRRunner _HybridCLR;
		/// <summary>
		/// 当前正在加载或已加载的资源
		/// </summary>
		private Dictionary<string, AssetRef> _Cached;
		/// <summary>
		/// 默认Package名字
		/// </summary>
		private string _DefaultPackageName;
		/// <summary>
		/// Build相关设置
		/// </summary>
		private BuildSetting _BuildSetting;
		/// <summary>
		/// 间隔多长时间自动执行一次UnloadUnusedAssets，单位秒
		/// </summary>
		private int _AutoUnloadUnusedAssetsInterval;


		#region Init
		/// <summary>
		/// 初始化资源系统
		/// </summary>
		/// <param name="playMode">管理器的运行模式</param>
		/// <param name="defaultPackageName">默认的Package名字</param>
		/// <param name="autoUnloadUnusedAssetsInterval">间隔多长时间自动执行一次UnloadUnusedAssets，单位秒</param>
		public async UniTask<bool> Init(EPlayMode playMode, string defaultPackageName, int autoUnloadUnusedAssetsInterval)
		{
			YooAssets.Initialize();
			_DefaultPackageName = defaultPackageName;
			_AutoUnloadUnusedAssetsInterval = autoUnloadUnusedAssetsInterval;

			_Package = YooAssets.TryGetPackage(defaultPackageName);
			if (_Package == null)
			{
				_Package = YooAssets.CreatePackage(defaultPackageName);
				YooAssets.SetDefaultPackage(_Package);
			}

			byte[] assetSettingBytes = await SettingsHelper.LoadSetting(SettingsHelper.AssetSetting);
			AssetSetting = AssetSetting.Parser.ParseFrom(assetSettingBytes);

			IDecryptionServices decryptionServices = null;
			byte[] buildSettingBytes = await SettingsHelper.LoadSetting(SettingsHelper.GetBuildSettingName());
			if (buildSettingBytes != null)
			{
				_BuildSetting = BuildSetting.Parser.ParseFrom(buildSettingBytes);
				decryptionServices = _BuildSetting.EncryptAssetBundle ? new DecryptionOffset() : null;
			}
			else
				Log.Error("Can not find " + Path.Combine(SettingsHelper.GetSettingDir(), SettingsHelper.GetBuildSettingName()));

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
				createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
				initializationOperation = _Package.InitializeAsync(createParameters);
			}

			// 联机运行模式
			if (playMode == EPlayMode.HostPlayMode)
			{
				string defaultHostServer = GetHostServerURL(true);
				string fallbackHostServer = GetHostServerURL(false);
				IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
				HostPlayModeParameters createParameters = new HostPlayModeParameters();
				createParameters.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(decryptionServices);
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

			Log.Info($"{nameof(AssetManager)} init end, {initializationOperation.Status}", nameof(AssetManager));
			bool initSucceed = initializationOperation.Status == EOperationStatus.Succeed;

			return initSucceed;
		}

		/// <summary>
		/// 获取资源服务器地址
		/// </summary>
		/// <param name="isMain">是主地址还是备地址</param>
		private string GetHostServerURL(bool isMain)
		{
			string hostServerAddress = isMain ? AssetSetting.AssetHostServerAddressMain : AssetSetting.AssetHostServerAddressStandby;
			if (string.IsNullOrEmpty(hostServerAddress))
			{
				Log.Error("Asset host server address is empty, open Icy/Asset/Setting to set it");
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
		/// 切换Package到指定名字的Package
		/// </summary>
		public void SwitchPackageTo(string packageName)
		{
			_Package = YooAssets.GetPackage(packageName);
			Log.Assert(_Package != null, $"{nameof(AssetManager)} SwitchPackageTo {packageName} failed!");
		}

		/// <summary>
		/// 切换Package到默认Package
		/// </summary>
		public void SwitchPackageToDefault()
		{
			_Package = YooAssets.GetPackage(_DefaultPackageName);
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
		public async UniTask StartAssetPatch()
		{
			_AssetPatcher = new AssetPatcher(_Package);
			await _AssetPatcher.Start();

			//启动无限次的间隔执行UnloadUnusedAssets
			Timer.RepeatByTime(UnloadUnusedAssetsWrap, _AutoUnloadUnusedAssetsInterval, -1);
		}

		/// <summary>
		/// 加载运行热更代码，AssetManager负责热更DLL和补充元数据DLL的加载，以及相关的解密之类的操作；
		/// 注意，最后的AOT调用热更代码入口这个操作，需要业务侧在回调参数里具体执行；
		/// 具体参考：https://www.hybridclr.cn/docs/basic/runhotupdatecodes
		/// </summary>
		public async UniTask RunPatchedCSharpCode(Action runCode)
		{
#if UNITY_EDITOR
			Log.Warn("Run HybridCLR at runtime in editor is not expected");
#endif

			_HybridCLR = new HybridCLRRunner(runCode);
			await _HybridCLR.Run();

			Timer.DelayByTime(UnloadUnusedAssetsWrap, 1);
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

		/// <summary>
		/// 同步加载资源
		/// </summary>
		public AssetRef LoadAsset(string address)
		{
			if (_Cached.ContainsKey(address))
				return _Cached[address];
			else
			{
				AssetHandle handle = _Package.LoadAssetSync(address);
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
		public async UniTask UnloadUnusedAssets()
		{
			await _Package.UnloadUnusedAssetsAsync().ToUniTask();
		}

		/// <summary>
		/// 强制卸载所有AB，谨慎调用
		/// </summary>
		public async UniTask ForceUnloadAllAssets()
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

		private void UnloadUnusedAssetsWrap()
		{
			UnloadUnusedAssets().Forget();
		}
		#endregion

		#region Clear
		/// <summary>
		/// 以当前激活的资源清单为准，清理该资源清单内未在使用的缓存Bundle文件
		/// </summary>
		public async UniTask<bool> ClearUnusedCachedBundleFiles()
		{
			return await ClearCachedFiles(EFileClearMode.ClearUnusedBundleFiles);
		}

		/// <summary>
		/// 清理文件系统所有的缓存Bundle文件，谨慎调用
		/// </summary>
		public async UniTask<bool> ClearAllCachedBundleFiles()
		{
			return await ClearCachedFiles(EFileClearMode.ClearAllBundleFiles);
		}

		/// <summary>
		/// 清理文件系统未使用的缓存Manifest文件
		/// </summary>
		public async UniTask<bool> ClearUnusedCachedManifestFiles()
		{
			return await ClearCachedFiles(EFileClearMode.ClearUnusedManifestFiles);
		}

		/// <summary>
		/// 清理文件系统所有的缓存Manifest文件
		/// </summary>
		public async UniTask<bool> ClearAllCachedManifestFiles()
		{
			return await ClearCachedFiles(EFileClearMode.ClearAllManifestFiles);
		}

		private async UniTask<bool> ClearCachedFiles(EFileClearMode clearMode)
		{
			ClearCacheFilesOperation operation = _Package.ClearCacheFilesAsync(clearMode);
			await operation.ToUniTask();
			if (operation.Status == EOperationStatus.Succeed)
				Log.Info($"ClearCachedFiles succeed, mode = {clearMode}", nameof(AssetManager));
			else
				Log.Error($"ClearCachedFiles failed, mode = {clearMode}", nameof(AssetManager));
			return operation.Status == EOperationStatus.Succeed;
		}
		#endregion
	}
}
