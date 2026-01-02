/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
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


using Icy.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.UI
{
	[System.Serializable]
	public abstract class StatusSwitcherStatusBase : IDisposable
	{
		/// <summary>
		/// 所属StatusSwitcherTarget
		/// </summary>
		protected StatusSwitcherTarget Target;
#if UNITY_EDITOR
		/// <summary>
		/// 用于间隔Update计时
		/// </summary>
		private double t;
		/// <summary>
		/// Update间隔
		/// </summary>
		private const double UPDATE_INTERVAL = 1f;
		/// <summary>
		/// Editor启动时间
		/// </summary>
		private double _TimeSinceStartup = 0.0;
#endif

		public void Init(StatusSwitcherTarget target)
		{
			Target = target;
#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= OnEditorUpdate;
			UnityEditor.EditorApplication.update += OnEditorUpdate;
#endif
		}

#if UNITY_EDITOR
		private void OnEditorUpdate()
		{
			t += (UnityEditor.EditorApplication.timeSinceStartup - _TimeSinceStartup);
			if (t >= UPDATE_INTERVAL)
			{
				if (Target != null && Target.gameObject != null)
					SyncValue();
				t = 0.0;
			}
			_TimeSinceStartup = UnityEditor.EditorApplication.timeSinceStartup;
		}
#endif

		protected abstract void SyncValue();

		public virtual void Dispose()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= OnEditorUpdate;
#endif
		}
	}
}
