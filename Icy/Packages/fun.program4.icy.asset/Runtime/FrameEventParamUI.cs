using System;
using Icy.Base;

namespace Icy.Asset
{
	/// <summary>
	/// 框架发出的即将开始下载热更资源的事件参数
	/// </summary>
	public class Ready2DownloadAssetPatchParam : IEventParam
	{
		/// <summary>
		/// 即将要下载的资源大小
		/// </summary>
		public long About2DownloadBytes;
		/// <summary>
		/// 即将要下载的资源数量
		/// </summary>
		public int About2DownloadCount;
		/// <summary>
		/// 开始下载的回调
		/// </summary>
		public Action StartDownload;

		public void Reset()
		{
			About2DownloadBytes = 0L;
			About2DownloadCount = 0;
			StartDownload = null;
		}
	}

	/// <summary>
	/// 资源下载出错的事件参数
	/// </summary>
	public class AssetPatchDownloadErrorParam : IEventParam
	{
		/// <summary>
		/// 所属包裹名称
		/// </summary>
		public string PackageName;
		/// <summary>
		/// 下载失败的文件名称
		/// </summary>
		public string FileName;
		/// <summary>
		/// 错误信息
		/// </summary>
		public string ErrorInfo;
		/// <summary>
		/// 重拾的回调
		/// </summary>
		public Action Retry;

		public void Reset()
		{
			PackageName = null;
			FileName = null;
			ErrorInfo = null;
			Retry = null;
		}
	}

	/// <summary>
	/// 资源下载进度变化的事件参数
	/// </summary>
	public class AssetPatchDownloadProgressParam : IEventParam
	{
		/// <summary>
		/// 所属包裹名称
		/// </summary>
		public string PackageName;
		/// <summary>
		/// 下载进度 (0-1f)
		/// </summary>
		public float Progress;
		/// <summary>
		/// 下载文件总数
		/// </summary>
		public int TotalDownloadCount;
		/// <summary>
		/// 当前完成的下载文件数量
		/// </summary>
		public int CurrentDownloadCount;
		/// <summary>
		/// 下载数据总大小（单位：字节）
		/// </summary>
		public long TotalDownloadBytes;
		/// <summary>
		/// 当前完成的下载数据大小（单位：字节）
		/// </summary>
		public long CurrentDownloadBytes;

		public void Reset()
		{
			PackageName = null;
			Progress = 0.0f;
			TotalDownloadCount = 0;
			CurrentDownloadBytes = 0;
			TotalDownloadBytes = 0L;
			CurrentDownloadBytes = 0L;
		}
	}
}
