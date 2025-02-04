using System.Collections.Generic;
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

		public static void Assert(bool condition, string msg)
		{
#if UNITY_EDITOR
			Debug.Assert(condition, msg);
#else
			if (!condition)
				Debug.LogError("[ASSERT] " + msg);
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
			if (tag != null)
				return string.Format("[{0}] {1}", tag, msg);
			return msg;
		}
	}
}
