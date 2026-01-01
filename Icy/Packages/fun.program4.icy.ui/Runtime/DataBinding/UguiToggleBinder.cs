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
using UnityEngine.UI;

namespace Icy.UI
{
	/// <summary>
	/// Ugui Toggle的Bind扩展
	/// </summary>
	public static class UguiToggleBinder
	{
		public static void BindTo(this Toggle toggle, BindableData<bool> bindableData)
		{
			Action<bool> listener = (bool newValue) => { toggle.isOn = newValue; };
			UguiBindManager.Instance.BindTo(toggle, bindableData, listener);
		}

		public static void BindTo<T>(this Toggle toggle, BindableData<T> bindableData, Func<BindableData<T>, bool> predicate)
		{
			Action<T> listener = (T newValue) => { toggle.isOn = predicate(bindableData); };
			UguiBindManager.Instance.BindTo(toggle, bindableData, listener);
		}

		public static void UnbindTo<T>(this Toggle toggle, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.UnbindTo(toggle, bindableData);
		}
	}
}
