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
	/// Ugui Text的Bind扩展
	/// </summary>
	public static class UguiTextBinder
	{
		public static void BindTo<T>(this Text text, BindableData<T> bindableData)
		{
			Action<T> listener = (T newValue) => { text.text = newValue.ToString(); };
			UguiBindManager.Instance.BindTo(text, bindableData, listener);
		}

		public static void BindTo<T>(this Text text, BindableData<T> bindableData, Func<BindableData<T>, string> predicate)
		{
			Action<T> listener = (T newValue) => { text.text = predicate(bindableData); };
			UguiBindManager.Instance.BindTo(text, bindableData, listener);
		}

		public static void UnbindTo<T>(this Text text, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.UnbindTo(text, bindableData);
		}
	}
}
