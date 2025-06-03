using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// Log等级，严重程度递增
	/// </summary>
	public enum LogLevel
	{
		Info,
		Warning,
		Error,
		Assert,
	}


	/// <summary>
	/// 提供更精细的Log Level控制；
	/// 使用本Log后，不应该再设置Unity Logger.filterLogType；
	/// </summary>
	public static class Log
	{
		/// <summary>
		/// 最小Log等级
		/// </summary>
		public static LogLevel MinLogLevel { get; set; } = LogLevel.Info;
		/// <summary>
		/// 指定Tag独立的LogLevel，优先级高于MinLogLevel
		/// </summary>
		private static Dictionary<string, LogLevel> _OverrideTagLogLevel = new Dictionary<string, LogLevel>();
#if UNITY_EDITOR
		/// <summary>
		/// Log颜色，只在editor下生效
		/// </summary>
		private static string _ColorOnce;
#endif

		#region WriteLog2File
		/// <summary>
		/// Log文件的目录
		/// </summary>
		private static string LOG_ROOT_DIR = Application.persistentDataPath + "/Log/";
		/// <summary>
		/// Log队列
		/// </summary>
		private static Queue<string> _LogQueue;
		/// <summary>
		/// Log队列的锁
		/// </summary>
		private static object _Lock;
		/// <summary>
		/// 停止线程令牌
		/// </summary>
		private static CancellationTokenSource _CancellationTokenSource;
		/// <summary>
		/// 写入文件的线程
		/// </summary>
		private static Thread _Write2FileThread;
		#endregion

		public static void Init(bool write2File)
		{
			#region WriteLog2File
			if (write2File)
			{
				_Lock = new object();
				_LogQueue = new Queue<string>();
				_CancellationTokenSource = new CancellationTokenSource();
				_Write2FileThread = new Thread(ConsumeLog);
				_Write2FileThread.Start();
				Application.logMessageReceivedThreaded += OnLog;
			}
			#endregion
		}

		/// <summary>
		/// 给指定Tag独立的LogLevel
		/// </summary>
		public static void OverrideTagLogLevel(string tag, LogLevel logLevel)
		{
			_OverrideTagLogLevel[tag] = logLevel;
		}

		/// <summary>
		/// 清除所有的OverrideTagLogLevel
		/// </summary>
		public static void ClearOverrideTagLogLevel()
		{
			_OverrideTagLogLevel.Clear();
		}

		public static void LogInfo(string msg, string tag = null)
		{
			if (!IsMatchLogLevel(tag))
				return;

			Debug.Log(FormatByTag(tag,msg));
		}

		public static void LogWarning(string msg, string tag = null)
		{
			if (!IsMatchLogLevel(tag))
				return;
			Debug.LogWarning(FormatByTag(tag, msg));
		}

		public static void LogError(string msg, string tag = null)
		{
			if (!IsMatchLogLevel(tag))
				return;
			Debug.LogError(FormatByTag(tag, msg));
		}

		public static void Assert(bool condition, string msg, string tag = null)
		{
			if (!condition)
			{
				LogError("[ASSERT] " + msg, tag);
#if UNITY_EDITOR
				if (!Application.isBatchMode)
				{
					Debug.Break();
					UnityEditor.EditorUtility.DisplayDialog("ASSERT FAILED!", msg, "Oh  No");
				}
#endif
			}
		}

		/// <summary>
		/// 设置Editor下的Log颜色，只作用于下一条Log
		/// </summary>
		public static void SetColorOnce(Color color)
		{
#if UNITY_EDITOR
			_ColorOnce = ColorUtility.ToHtmlStringRGB(color);
#endif
		}

		private static bool IsMatchLogLevel(string tag)
		{
			if (tag != null && _OverrideTagLogLevel.ContainsKey(tag))
			{
				if (_OverrideTagLogLevel[tag] > LogLevel.Info)
					return false;
			}
			else if (MinLogLevel > LogLevel.Info)
				return false;

			return true;
		}

		private static string FormatByTag(string tag, string msg)
		{
			string now = DateTime.Now.ToString("HH:mm:ss.fff");
			string logFormatted;
			if (tag != null)
				logFormatted = string.Format("{0} : [{1}] {2}", now, tag, msg);
			else
				logFormatted = string.Format("{0} : {1}", now, msg);

#if UNITY_EDITOR
			//颜色
			if (string.IsNullOrEmpty(_ColorOnce))
				return logFormatted;
			else
			{
				string color = _ColorOnce;
				_ColorOnce = null;
				return string.Format("<color=#{0}>{1}</color>", color, logFormatted);
			}
#else
			return logFormatted;
#endif
		}

		#region WriteLog2File
		/// <summary>
		/// 每次启动，删除最旧的一个Log文件，再新建一个文件作为本次启动要写入的
		/// </summary>
		private static string DeleteAndNewLogFile()
		{
			if (!Directory.Exists(LOG_ROOT_DIR))
				Directory.CreateDirectory(LOG_ROOT_DIR);

			string[] allFiles = Directory.GetFiles(LOG_ROOT_DIR);
			Array.Sort(allFiles);
			Array.Reverse(allFiles);

			for (int i = allFiles.Length - 1; i >= 0; i--)
			{
				//算上马上要新创建的，最多存在3个，所以这里留2个
				if (i >= 2)
					File.Delete(allFiles[i]);
			}

			string newFilePath = string.Format("{0}{1}.txt", LOG_ROOT_DIR, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
			File.Create(newFilePath).Dispose();
			return newFilePath;
		}

		/// <summary>
		/// 监听Unity的log
		/// </summary>
		private static void OnLog(string condition, string stackTrace, LogType type)
		{
			lock (_Lock)
			{
				_LogQueue.Enqueue(condition);
				if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
					_LogQueue.Enqueue(stackTrace);
				Monitor.Pulse(_Lock);
			}

		}

		/// <summary>
		/// 消费者函数，从队列里读取log，写入文件
		/// </summary>
		private static void ConsumeLog()
		{
			string newLogFilePath = DeleteAndNewLogFile();
			StreamWriter sw = new StreamWriter(newLogFilePath, true, Encoding.UTF8);
			try
			{
				while (!_CancellationTokenSource.Token.IsCancellationRequested)
				{
					string log = null;
					lock (_Lock)
					{
						while (_LogQueue.Count == 0)
							Monitor.Wait(_Lock);
						log = _LogQueue.Dequeue();
					}

					if (!string.IsNullOrEmpty(log))
					{
						sw.WriteLine(log);
						sw.Flush();
					}
				}
				Log.LogInfo("Write to file thread stopped", nameof(Log));
			}
			catch (Exception ex)
			{
				LogError($"Write log to file exception : {ex}", nameof(Log));
			}
			finally
			{
				sw.Close();
			}
		}

		/// <summary>
		/// 停止写入文件线程
		/// </summary>
		public static void StopLog2FileThread()
		{
			if (_Write2FileThread != null && _CancellationTokenSource != null)
			{
				_CancellationTokenSource.Cancel();
				lock (_Lock)
				{
					//令牌取消后，线程函数实际还Wait在锁上，这里要写入一行log、调用Pulse，以唤起线程函数来继续向下执行，才能停止
					_LogQueue.Enqueue("EOF");
					Monitor.Pulse(_Lock);
				}
				_Write2FileThread.Join(1000);
			}
		}
		#endregion
	}
}
