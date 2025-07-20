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


namespace Icy.UI
{
	/// <summary>
	/// UI的类型
	/// </summary>
	public enum UIType
	{
		/// <summary>
		/// 一般是比较大一点的窗口，参与回退栈
		/// </summary>
		Dialog,
		/// <summary>
		/// 一般是比较小的弹窗之类的，不参与回退栈
		/// </summary>
		Popup,
	}

	/// <summary>
	/// 如何隐藏UI
	/// </summary>
	public enum UIHideType
	{
		/// <summary>
		/// SetActive(false);
		/// </summary>
		Deactive,
		/// <summary>
		/// 移动到屏幕外面
		/// </summary>
		MoveOutScreen,
	}

	/// <summary>
	/// UI的层级划分，取值是Canvas上的Order In Layer
	/// </summary>
	public enum UILayer
	{
		Bottom = 1000,
		Low = 1500,
		Medium = 2000,
		High = 2500,
		Top = 3000,
	}
}
