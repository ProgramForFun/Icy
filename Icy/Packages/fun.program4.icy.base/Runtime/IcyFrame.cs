using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// 框架入口
	/// </summary>
	public sealed class IcyFrame : PersistentMonoSingleton<IcyFrame>
	{
		private List<IUpdateable> _Updateables = new List<IUpdateable>();
		private List<IFixedUpdateable> _FixedUpdateables = new List<IFixedUpdateable>();
		private List<ILateUpdateable> _LateUpdateables = new List<ILateUpdateable>();

		public void Init()
		{
			Log.ClearOverrideTagLogLevel();
			EventManager.ClearAll();
			LocalPrefs.ClearKeyPrefix();
		}

		#region Setting
		/// <summary>
		/// 获取框架Setting的根目录；
		/// Editor下是相对于项目根目录的相对路径，其他情况下是相对于SteamingAssets的相对路径
		/// </summary>
		internal string GetSettingDir()
		{
			return "IcySettings";
		}

		/// <summary>
		/// 获取框架EditorOnly Setting的根目录
		/// </summary>
		internal string GetEditorOnlySettingDir()
		{
			return Path.Combine(GetSettingDir(), "EditorOnly");
		}

		/// <summary>
		/// 加载框架Setting
		/// </summary>
		/// <param name="fileNameWithExtension">setting文件名</param>
		internal async UniTask<byte[]> LoadSetting(string fileNameWithExtension)
		{
			string path = Path.Combine(GetSettingDir(), fileNameWithExtension);
#if UNITY_EDITOR
			byte[] bytes = File.ReadAllBytes(path);
			await UniTask.CompletedTask;
#else
			byte[] bytes = await CommonUtility.LoadStreamingAsset(path);
#endif
			CommonUtility.xor(bytes);
			return bytes;
		}

#if UNITY_EDITOR
		/// <summary>
		/// 直接同步加载框架Setting，editor专用
		/// </summary>
		/// <param name="dir">Setting文件所在的目录</param>
		/// <param name="fileNameWithExtension">setting文件名</param>
		internal byte[] LoadSettingEditor(string dir, string fileNameWithExtension)
		{
			string path = Path.Combine(dir, fileNameWithExtension);
			if (File.Exists(path))
			{
				byte[] bytes = File.ReadAllBytes(path);
				CommonUtility.xor(bytes);
				return bytes;
			}
			return null;
		}
#endif

		/// <summary>
		/// 保存框架Setting
		/// </summary>
		/// <param name="dir">要保存到的目录</param>
		/// <param name="fileNameWithExtension">setting文件名</param>
		/// <param name="bytes">setting的byte数组数据</param>
		internal void SaveSetting(string dir, string fileNameWithExtension, byte[] bytes)
		{
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			string targetPath = Path.Combine(dir, fileNameWithExtension);
			CommonUtility.xor(bytes);
			File.WriteAllBytes(targetPath, bytes);
		}

		public string GetBuildSettingName()
		{
#if UNITY_EDITOR
			switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
			{
				case UnityEditor.BuildTarget.Android:
					return "BuildSetting_Android.json";
				case UnityEditor.BuildTarget.iOS:
					return "BuildSetting_iOS.json";
				case UnityEditor.BuildTarget.StandaloneWindows64:
					return "BuildSetting_Win64";
				default:
					Log.Assert(false, $"Unsupported platform {UnityEditor.EditorUserBuildSettings.activeBuildTarget}");
					return "";
			}
#else
			switch (Application.platform)
			{
				case RuntimePlatform.Android:
					return "BuildSetting_Android.json";
				case RuntimePlatform.IPhonePlayer:
					return "BuildSetting_iOS.json";
				case RuntimePlatform.WindowsPlayer:
					return "BuildSetting_Win64";
				default:
					Log.Assert(false, $"Unsupported platform {Application.platform}");
					return "";
			}
#endif
		}
		#endregion

		#region Update
		public void AddUpdate(IUpdateable updateable)
		{
			_Updateables.Add(updateable);
		}

		public void RemoveUpdate(IUpdateable updateable)
		{
			_Updateables.Remove(updateable);
		}

		public void AddFixedUpdate(IFixedUpdateable updateable)
		{
			_FixedUpdateables.Add(updateable);
		}

		public void RemoveFixedUpdate(IFixedUpdateable updateable)
		{
			_FixedUpdateables.Remove(updateable);
		}

		public void AddLateUpdate(ILateUpdateable updateable)
		{
			_LateUpdateables.Add(updateable);
		}

		public void RemoveLateUpdate(ILateUpdateable updateable)
		{
			_LateUpdateables.Remove(updateable);
		}

		private void Update()
		{
			float delta = Time.deltaTime;
			for (int i = 0; i < _Updateables.Count; i++)
				_Updateables[i]?.Update(delta);
		}

		private void FixedUpdate()
		{
			float delta = Time.fixedDeltaTime;
			for (int i = 0; i < _FixedUpdateables.Count; i++)
				_FixedUpdateables[i]?.FixedUpdate(delta);
		}

		private void LateUpdate()
		{
			float delta = Time.deltaTime;
			for (int i = 0; i < _LateUpdateables.Count; i++)
				_LateUpdateables[i]?.LateUpdate(delta);
		}
		#endregion

		private void OnApplicationQuit()
		{
			Log.StopLog2FileThread();
		}
	}
}
