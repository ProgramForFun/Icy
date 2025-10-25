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
					Log.Info(rb.ToString());
				}

				Log.Info("-----------------------");

				byte[] dst = new byte[3];
				for (int i = 0; i < 3; i++)
				{
					rb.Get(dst, 0, 3);
					Log.Info(rb.ToString());
				}

				Log.Info("=====================");
			}
		}
	}
}
#endif
