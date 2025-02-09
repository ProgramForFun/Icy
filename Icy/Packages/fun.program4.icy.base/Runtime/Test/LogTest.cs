#if UNITY_EDITOR

namespace Icy.Base
{
	public static class LogTest
	{
		public static void Test()
		{
			Log.MinLogLevel = LogLevel.Warning;
			Log.LogInfo("Test Msg", "game");
			Log.OverrideTagLogLevel("game", LogLevel.Info);
			Log.LogInfo("Test Msg", "game");
		}
	}
}
#endif
