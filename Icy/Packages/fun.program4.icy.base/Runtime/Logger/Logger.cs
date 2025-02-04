using System.Collections.Generic;
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// Log�ȼ������س̶ȵ���
	/// </summary>
	public enum LogLevel
	{
		Info,
		Warning,
		Error,
		Assert,
	}


	/// <summary>
	/// �ṩ����ϸ��Log Level���ƣ�
	/// ʹ�ñ�Log�󣬲�Ӧ��������Unity Logger.filterLogType��
	/// </summary>
	public static class Log
	{
		/// <summary>
		/// ��СLog�ȼ�
		/// </summary>
		public static LogLevel MinLogLevel { get; set; } = LogLevel.Info;
		/// <summary>
		/// ָ��Tag������LogLevel�����ȼ�����MinLogLevel
		/// </summary>
		private static Dictionary<string, LogLevel> _OverrideTagLogLevel = new Dictionary<string, LogLevel>();

		/// <summary>
		/// ��ָ��Tag������LogLevel
		/// </summary>
		public static void OverrideTagLogLevel(string tag, LogLevel logLevel)
		{
			_OverrideTagLogLevel[tag] = logLevel;
		}

		/// <summary>
		/// ������е�OverrideTagLogLevel
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
