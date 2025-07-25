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


using Cysharp.Text;
using System.Text;

namespace Icy.Network
{
	/// <summary>
	/// byte相关的扩展方法，便于网络部分使用
	/// </summary>
	public static class ByteExtension
	{
		/// <summary>
		/// byte转16进制
		/// </summary>
		public static string ToHex(this byte b)
		{
			return b.ToString("X2");
		}

		/// <summary>
		/// byte[]转16进制
		/// </summary>
		public static string ToHex(this byte[] bytes)
		{
			Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder();
			for (int i = 0; i < bytes.Length; i++)
				stringBuilder.Append(bytes[i].ToString("X2"));
			return stringBuilder.ToString();
		}

		/// <summary>
		/// byte[]转16进制，自定义format
		/// </summary>
		public static string ToHex(this byte[] bytes, string format)
		{
			Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder();
			for (int i = 0; i < bytes.Length; i++)
				stringBuilder.Append(bytes[i].ToString(format));
			return stringBuilder.ToString();
		}

		/// <summary>
		/// byte[]中指定部分转16进制
		/// </summary>
		public static string ToHex(this byte[] bytes, int offset, int count)
		{
			Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder();
			for (int i = offset; i < offset + count; i++)
				stringBuilder.Append(bytes[i].ToString("X2"));
			return stringBuilder.ToString();
		}

		/// <summary>
		/// 用Encoding.UTF8把byte[]转成string
		/// </summary>
		public static string ToUtf8Str(this byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}

		/// <summary>
		/// 用Encoding.UTF8把byte[]指定部分转成string
		/// </summary>
		public static string ToUtf8Str(this byte[] bytes, int index, int count)
		{
			return Encoding.UTF8.GetString(bytes, index, count);
		}

		/// <summary>
		/// 把uint写入byte[]的指定位置
		/// </summary>
		public static void WriteTo(this byte[] bytes, int offset, uint num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
			bytes[offset + 2] = (byte)((num & 0xff0000) >> 16);
			bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
		}

		/// <summary>
		/// 把int写入byte[]的指定位置
		/// </summary>
		public static void WriteTo(this byte[] bytes, int offset, int num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
			bytes[offset + 2] = (byte)((num & 0xff0000) >> 16);
			bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
		}

		/// <summary>
		/// 把byte写入byte[]的指定位置
		/// </summary>
		public static void WriteTo(this byte[] bytes, int offset, byte num)
		{
			bytes[offset] = num;
		}

		/// <summary>
		/// 把short写入byte[]的指定位置
		/// </summary>
		public static void WriteTo(this byte[] bytes, int offset, short num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
		}

		/// <summary>
		/// 把ushort写入byte[]的指定位置
		/// </summary>
		public static void WriteTo(this byte[] bytes, int offset, ushort num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
		}
	}
}
