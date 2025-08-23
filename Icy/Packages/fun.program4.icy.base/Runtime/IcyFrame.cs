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

			//监听UniTask中未处理的异常
			UniTaskScheduler.UnobservedTaskException += OnUniTaskUnobservedTaskException;

			InitProto();
		}

		/// <summary>
		/// 当前是否是主线程？
		/// </summary>
		public bool IsMainThread()
		{
			return Thread.CurrentThread.ManagedThreadId == MainThreadID;
		}

		/// <summary>
		/// 反射调用初始化Proto，牺牲一点点性能，换取用户不需要关心这个调用了
		/// TODO：接入HybridCLR后，这里的调用时机要改
		/// </summary>
		private void InitProto()
		{
			Assembly assembly = Assembly.Load("Icy.Protobuf");
			Type type = assembly.GetType("Icy.Protobuf.InitProto");
			MethodInfo method = type.GetMethod("InitProtoMsgIDRegistry", BindingFlags.Public | BindingFlags.Instance);
			object instance = Activator.CreateInstance(type);
			method.Invoke(instance, null);
		}

		/// <summary>
		/// 所有不等待的UniTask方法，都应该把这个方法传入Forget函数中，或者业务侧自己定义一个类似方法也可以
		/// </summary>
		public static void OnUniTaskForgetException(Exception ex)
		{
			Log.LogError(ex.ToString(), "UniTask Forget Exception");
		}

		private void OnUniTaskUnobservedTaskException(Exception ex)
		{
			Log.LogError(ex.ToString(), "UniTask Unobserved Exception");
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

			EventManager.Update();
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
			UniTaskScheduler.UnobservedTaskException -= OnUniTaskUnobservedTaskException;
			Log.StopLog2FileThread();
		}
	}
}
