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


using Icy.Base;

namespace Icy.UI
{
	/// <summary>
	/// UI逻辑的基类
	/// </summary>
	public abstract class UILogicBase : IUpdateable
	{
		public virtual void Init()
		{

		}

		/// <summary>
		/// 如果派生类需要Update，可以直接调用此方法
		/// </summary>
		public void EnableUpdate()
		{
			IcyFrame.Instance.AddUpdate(this);
		}

		/// <summary>
		/// 派生类如需override Update，先调用EnableUpdate
		/// </summary>
		public virtual void Update(float delta)
		{
			
		}

		public virtual void Destroy()
		{
			IcyFrame.Instance.RemoveUpdate(this);
		}
	}
}
