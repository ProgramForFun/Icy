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
using Icy.Asset;
using System;
using System.Threading;
using UnityEngine;

namespace Icy.Frame
{
	/// <summary>
	/// 框架入口
	/// </summary>
	public sealed class IcyFrame : PersistentMonoSingleton<IcyFrame>
	{
		public void Init()
		{
			CommonUtility.MainThreadID = Thread.CurrentThread.ManagedThreadId;

			Log.Reset();
			EventManager.ClearAll();
			LocalPrefs.ClearKeyPrefix();
			HybridCLRRunner.GetHybridCLREnabled();

			//监听UniTask中未处理的异常
			UniTaskScheduler.UnobservedTaskException += OnUniTaskUnobservedTaskException;

#if !UNITY_EDITOR
			if (HybridCLRRunner.IsHybridCLREnabled)
				EventManager.AddListener(EventDefine.HybridCLRRunnerFinish, OnHybridCLRRunnerFinish);
			else
				OnHybridCLRRunnerFinish(0, null);
#else
			OnHybridCLRRunnerFinish(0, null);
#endif

#if ICY_PRESERVE_UNITY_CLASS
			int dummy = UnityEngine.Random.Range(1, 2);
			//只保证代码有引用、不被裁剪，实际不会执行到
			if (dummy == 0)
				UnityClassReferencer.Preserve();
#endif
		}

		private void OnHybridCLRRunnerFinish(int arg1, IEventParam param)
		{
			Protobuf.InitProto.InitProtoMsgIDRegistry().Forget();
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
