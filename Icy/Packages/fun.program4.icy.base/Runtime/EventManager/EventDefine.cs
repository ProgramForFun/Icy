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
		#region UI代码生成
		/// <summary>
		/// UICodeGenerator的其中一个Item的Name字段发生了变化
		/// </summary>
		public static readonly int UICodeGeneratorNameChanged = -1000;
		/// <summary>
		/// 触发UI代码的生成
		/// </summary>
		public static readonly int GenerateUICode = -1001;
		/// <summary>
		/// 触发UI Logic代码的生成
		/// </summary>
		public static readonly int GenerateUILogicCode = -1002;
		/// <summary>
		/// 同时触发UI和UI Logic代码的生成
		/// </summary>
		public static readonly int GenerateUICodeBoth = -1003;
		#endregion

		#region 资源
		/// <summary>
		/// 从远端更新资源版本信息结束
		/// </summary>
		public static readonly int RequestAssetPatchInfoEnd = -1100;
		/// <summary>
		/// 磁盘空间不足以更新资源
		/// </summary>
		public static readonly int NotEnoughDiskSpace2PatchAsset = -1101;
		/// <summary>
		/// 下载资源前的条件都已经准备好，可以开始下载
		/// </summary>
		public static readonly int Ready2DownloadAssetPatch = -1102;
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
		#endregion

		#region UI
		/// <summary>
		/// 一个UI加载完成
		/// </summary>
		public static readonly int UILoaded = -1200;
		/// <summary>
		/// 一个UI调用Show显示
		/// </summary>
		public static readonly int UIShown = -1201;
		/// <summary>
		/// 一个UI调用Hide隐藏
		/// </summary>
		public static readonly int UIHid = -1202;
		/// <summary>
		/// 一个UI调用Destroy销毁
		/// </summary>
		public static readonly int UIDestroyed = -1203;
		#endregion
	}
}
