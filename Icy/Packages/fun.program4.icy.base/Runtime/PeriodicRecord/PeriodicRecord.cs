using System;

namespace Icy.Base
{
	/// <summary>
	/// 支持记录一个key，指定其有效时间；
	/// 注意时间为本地时间，不是UTC时间
	/// </summary>
	public static class PeriodicRecord
	{
		private const string LOCAL_PREFIX = "_PR_";

		public const long MinuteSeconds = 60;
		public const long HourSeconds = MinuteSeconds * 60;
		public const long DaySeconds = HourSeconds * 24;
		public const long MonthSeconds = DaySeconds * 30;
		public const long YearSeconds = MonthSeconds * 12;


		/// <summary>
		/// 记录指定的key，有效期为从现在开始持续seconds秒
		/// </summary>
		public static void RecordDuration(string key, long durationSeconds)
		{
			long now = DateTimeOffset.Now.ToUnixTimeSeconds();
			RecordUntil(key, now + durationSeconds);
		}

		/// <summary>
		/// 记录指定的key，有效期为指定的时间戳
		/// </summary>
		public static void RecordUntil(string key, long timestamp)
		{
			LocalPrefs.SetLong(LOCAL_PREFIX + key, timestamp);
			LocalPrefs.Save();
		}

		/// <summary>
		/// 记录指定的key，有效期为今天；注意为本地日期，不是UTC日期
		/// </summary>
		public static void RecordToday(string key)
		{
			DateTime today24 = DateTime.Today.AddDays(1);
			RecordUntil(key, today24.TotalSeconds());
		}

		/// <summary>
		/// 记录指定的key，有效期为本月；注意为本地日期，不是UTC日期
		/// </summary>
		public static void RecordThisMonth(string key)
		{
			DateTime now = DateTime.Now;
			DateTime thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
			RecordUntil(key, thisMonthStart.AddMonths(1).TotalSeconds());
		}

		/// <summary>
		/// 记录指定的key，有效期为本年；注意为本地日期，不是UTC日期
		/// </summary>
		public static void RecordThisYear(string key)
		{
			DateTime now = DateTime.Now;
			DateTime thisYearStart = new DateTime(now.Year, 1, 1, 0, 0, 0);
			RecordUntil(key, thisYearStart.AddYears(1).TotalSeconds());
		}

		/// <summary>
		/// 指定key是否有效
		/// </summary>
		public static bool IsValid(string key)
		{
			long timestamp = LocalPrefs.GetLong(LOCAL_PREFIX + key, long.MinValue);
			long now = DateTimeOffset.Now.ToUnixTimeSeconds();
			return timestamp >= now;
		}

		/// <summary>
		/// 获取指定key的失效时间DateTime
		/// </summary>
		public static DateTime GetInvalidDateTime(string key)
		{
			long timestamp = LocalPrefs.GetLong(LOCAL_PREFIX + key, long.MinValue);
			if (timestamp == long.MinValue)
				return default;
			DateTime dateTime = DateTime.UnixEpoch.ToLocalTime();
			dateTime = dateTime.AddSeconds(timestamp);
			return dateTime;
		}

		/// <summary>
		/// 获取指定key的失效时间戳
		/// </summary>
		public static long GetInvalidTimestamp(string key)
		{
			return GetInvalidDateTime(key).TotalSeconds();
		}
	}
}
