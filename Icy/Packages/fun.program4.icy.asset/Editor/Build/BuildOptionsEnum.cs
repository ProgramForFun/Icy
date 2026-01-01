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

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 调试相关选项
	/// </summary>
	[Flags]
	public enum BuildOptionDev
	{
		/// <summary>
		/// 打Dev版本
		/// </summary>
		DevelopmentBuild = 1 << 0,
		/// <summary>
		/// 允许调试代码
		/// </summary>
		ScriptDebugging = 1 << 1,
		/// <summary>
		/// 启动时自动连接Profiler
		/// </summary>
		AutoConnectProfiler = 1 << 2,
		/// <summary>
		/// DeepProfiling
		/// </summary>
		DeepProfiling = 1 << 3,
	}

	/// <summary>
	/// AssetBundle相关选项
	/// </summary>
	[Flags]
	public enum BuildOptionAssetBundle
	{
		/// <summary>
		/// 是否打包AssetBundle
		/// </summary>
		BuildAssetBundle = 1 << 0,
		/// <summary>
		/// 是否清除缓存、打全量AssetBundle
		/// </summary>
		ClearAssetBundleCache = 1 << 1,
		/// <summary>
		/// 是否启动AssetBundle加密
		/// </summary>
		EncryptAssetBundle  = 1 << 2,
	}
}
