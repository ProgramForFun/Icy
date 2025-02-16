#if UNITY_EDITOR

using System;

namespace Icy.Base
{
	public static class PeriodicRecordTest
	{
		public static void Test()
		{
			string key = "PeriodicRecordTest";
			//记录一个今天、本月、今年内有效的key
			PeriodicRecord.RecordToday(key);
			PeriodicRecord.RecordThisMonth(key);
			PeriodicRecord.RecordThisYear(key);
			//记录一个从现在开始3600秒内有效的key
			PeriodicRecord.RecordSeconds(key, 3600);
			//记录一个指定时间戳之前有效的key
			PeriodicRecord.RecordTimestamp(key, 1739721600);

			//查询一个key是否还有效
			bool isValid = PeriodicRecord.IsValid(key);
			//获取一个key的失效时间
			DateTime invalidDateTime = PeriodicRecord.GetInvalidDateTime(key);

			Log.LogInfo($"Is Valid {PeriodicRecord.IsValid(key).ToString()}");
			Log.LogInfo($"Invalid time {PeriodicRecord.GetInvalidDateTime(key)}");
		}
	}
}
#endif
