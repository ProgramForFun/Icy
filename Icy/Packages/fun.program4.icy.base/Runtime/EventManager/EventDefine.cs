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


namespace Icy.Base
{
	/// <summary>
	/// 框架内部的事件定义
	/// </summary>
	public static class EventDefine
	{
		#region 资源
		/// <summary>
		/// 下载资源出错
		/// </summary>
		public static readonly int AssetPatchDownloadError = -1103;
		/// <summary>
		/// 下载资源进度变化
		/// </summary>
		public static readonly int AssetPatchDownloadProgress = -1104;
		/// <summary>
		/// 资源更新完成
		/// </summary>
		public static readonly int AssetPatchFinish = -1105;
		/// <summary>
		/// HybridCLRRunner运行完成
		/// </summary>
		public static readonly int HybridCLRRunnerFinish = -1106;
		#endregion
	}
}
