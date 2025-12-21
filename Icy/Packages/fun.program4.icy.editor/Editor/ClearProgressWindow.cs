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


using Icy.Base;
using System;
using System.Reflection;
using UnityEditor;

namespace Icy.Editor
{
	/// <summary>
	/// 清空并关闭Unity editor右下角的ProgressWindow窗口
	/// </summary>
	public static class ClearProgressWindow
	{
		public static void Close()
		{
			//反射关闭窗口
			Type progressWindowType = Type.GetType("UnityEditor.ProgressWindow, UnityEditor");
			MethodInfo closeMethod = progressWindowType.GetMethod("HideDetails",
									BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			closeMethod.Invoke(null, null);
		}

		[MenuItem("Icy/Tools/Clear ProgressWindow", false, 21)]
		public static void Clear()
		{
			try
			{
				//清空所有显示的Progress
				int count = Progress.GetCount();
				for (int i = 0; i < count; i++)
				{
					int id = Progress.GetId(i);
					Progress.Remove(id);
				}
			}
			catch (Exception e)
			{
				//报错也无所谓
				Log.Info(e, nameof(ClearProgressWindow));
			}

			Close();
		}
	}
}
