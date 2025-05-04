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

		/// <summary>
		/// 获取框架Setting的根目录；
		/// Editor下是相对于项目根目录的相对路径，其他情况下是相对于SteamingAssets的相对路径
		/// </summary>
		public string GetSettingDir()
		{
			return "IcySettings";
		}

		/// <summary>
		/// 获取框架EditorOnly Setting的根目录
		/// </summary>
		public string GetEditorOnlySettingDir()
		{
			return Path.Combine(GetSettingDir(), "EditorOnly");
		}

		/// <summary>
		/// 加载框架设置
		/// </summary>
		/// <param name="fileNameWithExtension"></param>
		public async UniTask<byte[]> LoadSetting(string fileNameWithExtension)
		{
			string path = Path.Combine(GetSettingDir(), fileNameWithExtension);
#if UNITY_EDITOR
			byte[] bytes = File.ReadAllBytes(path);
			await UniTask.CompletedTask;
#else
			byte[] bytes = await CommonUtility.LoadStreamingAsset(path);
#endif
			return bytes;
		}

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
