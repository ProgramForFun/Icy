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


using System;

namespace Icy.UI
{
	/// <summary>
	/// StatusSwitcher状态的基类
	/// </summary>
	[Serializable]
	public abstract class StatusSwitcherStatusBase : IDisposable
	{
		/// <summary>
		/// 所属StatusSwitcherTarget
		/// </summary>
		protected StatusSwitcherTarget Target;

		/// <summary>
		/// 初始化状态
		/// </summary>
		public void Init(StatusSwitcherTarget target)
		{
			Target = target;
		}

		/// <summary>
		/// 记录Target当前的值到本状态
		/// </summary>
		public abstract void Record();

		/// <summary>
		/// 应用本状态的值到Target
		/// </summary>
		public abstract void Apply();

		/// <summary>
		/// 从另一个状态copy数据
		/// </summary>
		/// <param name="other"></param>
		public abstract void CopyFrom(StatusSwitcherStatusBase other);

		public virtual void Dispose()
		{

		}
	}
}
