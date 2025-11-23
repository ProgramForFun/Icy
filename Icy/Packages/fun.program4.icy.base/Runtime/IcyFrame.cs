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


		public void Init()
		{
			MainThreadID = Thread.CurrentThread.ManagedThreadId;

			Log.Reset();
			EventManager.ClearAll();
			LocalPrefs.ClearKeyPrefix();

			//监听UniTask中未处理的异常
			UniTaskScheduler.UnobservedTaskException += OnUniTaskUnobservedTaskException;

			InitProto();

#if ICY_PRESERVE_UNITY_CLASS
			int dummy = UnityEngine.Random.Range(1, 2);
			//只保证代码有引用、不被裁剪，实际不会执行到
			if (dummy == 0)
				UnityClassReferencer.Preserve();
#endif
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
		/// </summary>
		private void InitProto()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				//TODO：Icy.Protobuf可能因为没有引用，被裁剪掉了
				if (assembly.GetName().Name == "Icy.Protobuf")
				{
					Type type = assembly.GetType("Icy.Protobuf.InitProto");
					MethodInfo method = type.GetMethod("InitProtoMsgIDRegistry", BindingFlags.Public | BindingFlags.Instance);
					object instance = Activator.CreateInstance(type);
					method.Invoke(instance, null);
					return;
				}
			}
			Log.Error("InitProto failed, could not find Icy.Protobuf assembly", nameof(IcyFrame));
		}

		/// <summary>
		/// 所有不等待的UniTask方法，都应该把这个方法传入Forget函数中，或者业务侧自己定义一个类似方法也可以
		/// </summary>
		public static void OnUniTaskForgetException(Exception ex)
		{
			Log.Error(ex.ToString(), "UniTask Forget Exception");
		}

		private void OnUniTaskUnobservedTaskException(Exception ex)
		{
			Log.Error(ex.ToString(), "UniTask Unobserved Exception");
		}

		private void Update()
		{
			Updater.Instance.Update(Time.deltaTime);
			EventManager.Update();
		}

		private void FixedUpdate()
		{
			Updater.Instance.Update(Time.fixedDeltaTime);
		}

		private void LateUpdate()
		{
			Updater.Instance.LateUpdate(Time.deltaTime);
		}

		private void OnApplicationQuit()
		{
			UniTaskScheduler.UnobservedTaskException -= OnUniTaskUnobservedTaskException;
			Log.StopLog2FileThread();
		}
	}
}
