#if UNITY_EDITOR

namespace Icy.Base
{
	public static class RingBufferTest
	{
		public static void Test()
		{
			RingBuffer<byte> rb = new RingBuffer<byte>(10);
			for (int c = 0; c < 3; c++)
			{
				byte[] src = { 1, 2, 3 };
				for (int i = 0; i < 3; i++)
				{
					rb.Put(src, 0, 3);
					Log.LogInfo(rb.ToString());
				}

				Log.LogInfo("-----------------------");

				byte[] dst = new byte[3];
				for (int i = 0; i < 3; i++)
				{
					rb.Get(dst, 0, 3);
					Log.LogInfo(rb.ToString());
				}

				Log.LogInfo("=====================");
			}
		}
	}
}
#endif
