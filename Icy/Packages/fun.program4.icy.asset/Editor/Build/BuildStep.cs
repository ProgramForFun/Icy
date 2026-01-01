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
using System.Collections.Generic;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 打包Step的基类
	/// </summary>
	public abstract class BuildStep : ProcedureStep
	{
		/// <summary>
		/// 是否是子Procedure
		/// </summary>
		public virtual bool IsSubProcedure()
		{
			return false;
		}

		/// <summary>
		/// 获取子Procedure所有的Step名字，也是类的名字
		/// </summary>
		public virtual List<string> GetAllStepNames()
		{
			return null;
		}
	}
}
