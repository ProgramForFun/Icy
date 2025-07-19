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

#if ICY_USE_SRDEBUGGER

using Icy.Base;
using SRDebugger.Services;
using SRF.Service;

namespace Icy.GM
{
	/// <summary>
	/// GM 和 运行时Console
	/// </summary>
	public static class GM
	{
		/// <summary>
		/// 初始化GM
		/// </summary>
		public static void Init(IGMOptions gmOptions)
		{
			if (IsInited())
			{
				Log.LogError("Duplicate Init to GM is invalid", nameof(GM));
				return;
			}

			SRDebug.Init();
			//注册SRDebugger的Options
			IOptionsService srService = SRServiceManager.GetService<IOptionsService>();
			srService.AddContainer(gmOptions);
		}

		/// <summary>
		/// 是否已经初始化了
		/// </summary>
		public static bool IsInited()
		{
			return SRServiceManager.HasService<IConsoleService>();
		}
	}
}

#endif
