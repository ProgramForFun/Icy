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
		}
	}
}
#endif
