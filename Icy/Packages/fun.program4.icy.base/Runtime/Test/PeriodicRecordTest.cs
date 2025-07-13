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
			PeriodicRecord.RecordDuration(key, 3600);
			//记录一个指定时间戳之前有效的key
			PeriodicRecord.RecordUntil(key, 1739721600);

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
