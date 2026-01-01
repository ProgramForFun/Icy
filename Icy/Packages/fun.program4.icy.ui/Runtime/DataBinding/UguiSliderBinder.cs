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
	/// Ugui Slider的Bind扩展
	/// </summary>
	public static class UguiSliderBinder
	{
		public static void BindTo(this Slider slider, BindableData<float> bindableData)
		{
			Action<float> listener = (float newValue) => { slider.value = newValue; };
			UguiBindManager.Instance.BindTo(slider, bindableData, listener);
		}

		public static void BindTo(this Slider slider, BindableData<double> bindableData)
		{
			Action<double> listener = (double newValue) => { slider.value = (float)newValue; };
			UguiBindManager.Instance.BindTo(slider, bindableData, listener);
		}

		public static void BindTo<T>(this Slider slider, BindableData<T> bindableData, Func<BindableData<T>, float> predicate)
		{
			Action<T> listener = (T newValue) => { slider.value = predicate(bindableData); };
			UguiBindManager.Instance.BindTo(slider, bindableData, listener);
		}

		public static void UnbindTo<T>(this Slider slider, BindableData<T> bindableData)
		{
			UguiBindManager.Instance.UnbindTo(slider, bindableData);
		}
	}
}
