using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// 框架入口
	/// </summary>
	public sealed class IcyFrame : PersistentMonoSingleton<IcyFrame>
	{
		/// <summary>
		/// 主线程ID
		/// </summary>
		public int MainThreadID { get; private set; }

		private List<IUpdateable> _Updateables = new List<IUpdateable>();
		private List<IFixedUpdateable> _FixedUpdateables = new List<IFixedUpdateable>();
		private List<ILateUpdateable> _LateUpdateables = new List<ILateUpdateable>();


		public void Init()
		{
			MainThreadID = Thread.CurrentThread.ManagedThreadId;

			Log.Reset();
			EventManager.ClearAll();
			LocalPrefs.ClearKeyPrefix();

			//反射调用注册所有proto id，牺牲一点点性能，换取用户不需要关心这个调用了
			//TODO：接入HybridCLR后，这里的调用时机要改
			Assembly assembly = Assembly.Load("Protos");
			if (assembly != null)
			{
				Type type = assembly.GetType("ProtoMsgIDRegistry");
				if (type != null)
				{
					MethodInfo method = type.GetMethod("RegisterAll");
					method?.Invoke(null, null);
				}
			}
		}

		/// <summary>
		/// 当前是否是主线程？
		/// </summary>
		public bool IsMainThread()
		{
			return Thread.CurrentThread.ManagedThreadId == MainThreadID;
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
