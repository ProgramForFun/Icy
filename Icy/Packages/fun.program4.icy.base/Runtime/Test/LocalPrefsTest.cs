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

using UnityEngine;

namespace Icy.Base
{
	public static class LocalPrefsTest
	{
		public static void Test()
		{
			//int
			Log.LogInfo($"Last time int = LocalPrefs.GetInt {LocalPrefs.GetInt("TestLocalSave_int")}");

			int randInt = Random.Range(0, 9);
			LocalPrefs.SetInt("TestLocalSave_int", randInt);
			Log.LogInfo($"LocalPrefs.SetInt {randInt}");

			LocalPrefs.Save();
			Log.LogInfo($"LocalPrefs.GetInt {LocalPrefs.GetInt("TestLocalSave_int")}");

			//Vector3
			Log.LogInfo($"Last time Vector3 = LocalPrefs.GetVector3 {LocalPrefs.GetVector3("TestLocalSave_Vector3")}");

			Vector3 randVector3 = new Vector3(Random.Range(0, 9), Random.Range(0, 9), Random.Range(0, 9));
			LocalPrefs.SetVector3("TestLocalSave_Vector3", randVector3);
			Log.LogInfo($"LocalPrefs.SetVector3 {randVector3}");

			LocalPrefs.Save();
			Log.LogInfo($"LocalPrefs.GetVector3 {LocalPrefs.GetVector3("TestLocalSave_Vector3")}");

			//Prefix
			int palyerID = 123456;
			LocalPrefs.SetKeyPrefix(palyerID + "_");
			LocalPrefs.SetInt("PrefixText_int", 100);
			Log.LogInfo($"LocalPrefs.GetInt {LocalPrefs.GetInt("PrefixText_int")}");
			Log.LogInfo($"Key with prefix {LocalPrefs.Data.ints.KeyByValue(100)}");
		}
	}
}
#endif
