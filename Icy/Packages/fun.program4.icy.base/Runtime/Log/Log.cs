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


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using Cysharp.Text;

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
	/// 提供Log到文件、多线程Log、以及更精细的Log Level控制；
	/// 使用本Log后，不应该再设置Unity Logger.filterLogType；
	/// </summary>
	public static class Log
	{
		/// <summary>
		/// 最小Log等级
		/// </summary>
		public static LogLevel MinLogLevel { get; set; } = LogLevel.Info;
		/// <summary>
		/// 指定Tag独立的最小LogLevel，优先级高于MinLogLevel
		/// </summary>
		private static Dictionary<string, LogLevel> _OverrideMinLogLevelForTag = new Dictionary<string, LogLevel>();
		/// <summary>
		/// Log Level相关数据的锁
		/// </summary>
		private static object _LevelLock = new object();
#if UNITY_EDITOR
		/// <summary>
		/// Log颜色，只在editor下生效
		/// </summary>
		private static string _ColorOnce = null;
#endif
		/// <summary>
		/// 是否忽略Log等级，强制Log一条日志，单次生效；
		/// 对某些情况下要在release下log一条info日志时有用；
		/// </summary>
		private static bool _ForceOnce = false;

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
		private static object _QueueLock;
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
				_QueueLock = new object();
				_LogQueue = new Queue<string>();
				_CancellationTokenSource = new CancellationTokenSource();
				_Write2FileThread = new Thread(ConsumeLog);
				_Write2FileThread.Start();
				Application.logMessageReceivedThreaded += OnLog;
			}
			#endregion
		}

		/// <summary>
		/// 给指定Tag独立的最小LogLevel
		/// </summary>
		public static void OverrideMinLogLevelForTag(string tag, LogLevel logLevel)
		{
			lock (_LevelLock)
				_OverrideMinLogLevelForTag[tag] = logLevel;
		}

		/// <summary>
		/// 清除所有的OverrideTagLogLevel
		/// </summary>
		public static void Reset()
		{
			MinLogLevel = LogLevel.Info;
			lock (_LevelLock)
			{
				_OverrideMinLogLevelForTag.Clear();
				_ForceOnce = false;
			}

#if UNITY_EDITOR
			_ColorOnce = null;
#endif
		}

		public static void LogInfo(string msg, string tag = null)
		{
			if (!IsMatchLogLevel(tag, LogLevel.Info))
				return;

			Debug.Log(FormatByTag(tag,msg));
		}

		public static void LogWarning(string msg, string tag = null)
		{
			if (!IsMatchLogLevel(tag, LogLevel.Warning))
				return;
			Debug.LogWarning(FormatByTag(tag, msg));
		}

		public static void LogError(string msg, string tag = null)
		{
			if (!IsMatchLogLevel(tag, LogLevel.Error))
				return;
			Debug.LogError(FormatByTag(tag, msg));
		}

		public static void Assert(bool condition, string msg, string tag = null)
		{
			if (!IsMatchLogLevel(tag, LogLevel.Assert))
				return;

			if (!condition)
			{
				LogError("[ASSERT] " + msg, tag);
#if UNITY_EDITOR
				if (!Application.isBatchMode)
				{
					if (UnityEditor.EditorApplication.isPlaying)
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

		/// <summary>
		/// 忽略Log等级，强制Log一条日志，单次生效；
		/// 比如在某些情况下要在release下保留一条info日志时有用；
		/// </summary>
		public static void ForceLogOnce()
		{
			lock (_LevelLock)
				_ForceOnce = true;
		}

		private static bool IsMatchLogLevel(string tag, LogLevel logLevel)
		{
			lock (_LevelLock)
			{
				if (_ForceOnce)
				{
					_ForceOnce = false;
					return true;
				}
				else if (tag != null && _OverrideMinLogLevelForTag.ContainsKey(tag))
				{
					if (logLevel < _OverrideMinLogLevelForTag[tag])
						return false;
				}
				else if (logLevel < MinLogLevel)
					return false;

				return true;
			}
		}

		private static string FormatByTag(string tag, string msg)
		{
			string now = DateTime.Now.ToString("HH:mm:ss.fff");
			string logFormatted;
			if (tag != null)
				logFormatted = ZString.Format("{0} : [{1}] {2}", now, tag, msg);
			else
				logFormatted = ZString.Format("{0} : {1}", now, msg);

#if UNITY_EDITOR
			//颜色
			if (string.IsNullOrEmpty(_ColorOnce))
				return logFormatted;
			else
			{
				string color = _ColorOnce;
				_ColorOnce = null;
				return ZString.Format("<color=#{0}>{1}</color>", color, logFormatted);
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

			string newFilePath = ZString.Format("{0}{1}.txt", LOG_ROOT_DIR, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
			File.Create(newFilePath).Dispose();
			return newFilePath;
		}

		/// <summary>
		/// 监听Unity的log
		/// </summary>
		private static void OnLog(string condition, string stackTrace, LogType type)
		{
			lock (_QueueLock)
			{
				_LogQueue.Enqueue(condition);
				if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
					_LogQueue.Enqueue(stackTrace);
				Monitor.Pulse(_QueueLock);
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
				while (!_CancellationTokenSource.IsCancellationRequested)
				{
					string log = null;
					lock (_QueueLock)
					{
						while (_LogQueue.Count == 0)
							Monitor.Wait(_QueueLock);
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
				lock (_QueueLock)
				{
					//令牌取消后，线程函数实际还Wait在锁上，这里要写入一行log、调用Pulse，以唤起线程函数来继续向下执行，才能停止
					_LogQueue.Enqueue("EOF");
					Monitor.Pulse(_QueueLock);
				}
				_Write2FileThread.Join(1000);
			}
		}
		#endregion
	}
}
