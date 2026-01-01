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

namespace Icy.UI
{
	/// <summary>
	/// ImageEx的Bind扩展
	/// </summary>
	public static class ImageExBinder
	{
		public static void BindTo(this ImageEx imageEx, BindableData<string> bindableData)
		{
			Action<string> listener = (string newValue) => { imageEx.SetSprite(newValue); };
			UguiBindManager.Instance.BindTo(imageEx, bindableData, listener);
		}

		public static void BindTo<T>(this ImageEx imageEx, BindableData<T> bindableData, Func<BindableData<T>, string> predicate)
		{
			Action<T> listener = (T newValue) => { imageEx.SetSprite(predicate(bindableData)); };
			UguiBindManager.Instance.BindTo(imageEx, bindableData, listener);
		}

		public static void UnbindTo<T>(this ImageEx imageEx, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.UnbindTo(imageEx, bindableData);
		}
	}
}
