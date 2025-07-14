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

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Icy.Editor
{
	/// <summary>
	/// 封装UnityEditor的EditorUtility.DisplayProgressBar，和右下角的Progress；
	/// 
	/// 因为EditorUtility.DisplayProgressBar Unity内部也在用，导致我们代码里设置的进度会被覆盖掉，基本没有了看进度的功能；
	/// 所以封装了这个类，EditorUtility.DisplayProgressBar只用做屏蔽交互，右下角的Progress来负责显示进度；
	/// </summary>
	public static class BiProgress
	{
		private const string PROGRESS_ID_KEY = "_Icy_ProgressID";
		private const string PROGRESS_NAME_KEY = "_Icy_ProgressName";


		/// <summary>
		/// 显示BiProgress
		/// </summary>
		/// <param name="title">抬头/名字</param>
		/// <param name="desc">当前的描述，可以每次更新进度时更新</param>
		/// <param name="progress">当前进度，取值0~1</param>
		public static void Show(string title, string desc, float progress)
		{
			//检测移除无效的ProgressID
			int currProgressID = EditorLocalPrefs.GetInt(PROGRESS_ID_KEY, int.MinValue);
			if (currProgressID != int.MinValue && !Progress.Exists(currProgressID))
			{
				currProgressID = int.MinValue;
				RemoveKey();
			}

			desc = ColorString(desc);
			if (currProgressID == int.MinValue)
			{
				//新创建
				int newProgressID = StartProgress(title, desc);
				SetKey(newProgressID, title);

				Progress.Report(newProgressID, progress, desc);
			}
			else
			{
				//更新进度
				string currProgressName = EditorLocalPrefs.GetString(PROGRESS_NAME_KEY);
				if (title == currProgressName)
					Progress.Report(currProgressID, progress, desc);
				else
				{
					//右下角显示的Progress Name一旦创建就不能修改了，如果要更新这个显示，需要重新创建一个Progress
					Progress.Remove(currProgressID);

					int newProgressID = StartProgress(title, desc);
					SetKey(newProgressID, title);
					Progress.Report(newProgressID, progress, desc);
				}
			}

			EditorUtility.DisplayProgressBar(title, desc, progress);
		}

		/// <summary>
		/// 关闭BiProgress；
		/// BiProgress不会自动关闭，需要手动调用Hide；
		/// </summary>
		public static void Hide()
		{
			int currProgressID = EditorLocalPrefs.GetInt(PROGRESS_ID_KEY, int.MinValue);
			if (currProgressID != int.MinValue && Progress.Exists(currProgressID))
			{
				Progress.Remove(currProgressID);
				RemoveKey();

				//关闭Progress Detail窗口
				ReflectCloseMethod();
			}

			EditorUtility.ClearProgressBar();
		}

		private static int StartProgress(string name, string desc)
		{
			int newProgressID = Progress.Start(name, ColorString(desc), Progress.Options.Unmanaged | Progress.Options.Synchronous);
			Progress.SetPriority(newProgressID, Progress.Priority.High);
			Progress.SetTimeDisplayMode(newProgressID, Progress.TimeDisplayMode.ShowRunningTime);
			//打开Progress Detail窗口
			Progress.ShowDetails();
			return newProgressID;
		}

		private static void ReflectCloseMethod()
		{
			try
			{
				Type progressWindowType = Type.GetType("UnityEditor.ProgressWindow, UnityEditor");
				MethodInfo closeMethod = progressWindowType.GetMethod("HideDetails",
										BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				closeMethod.Invoke(null, null);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		private static string ColorString(string str)
		{
			return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(Color.green), str);
		}

		private static void SetKey(int progressID, string name)
		{
			EditorLocalPrefs.SetInt(PROGRESS_ID_KEY, progressID);
			EditorLocalPrefs.SetString(PROGRESS_NAME_KEY, name);
			EditorLocalPrefs.Save();
		}

		private static void RemoveKey()
		{
			EditorLocalPrefs.RemoveKey(PROGRESS_ID_KEY);
			EditorLocalPrefs.RemoveKey(PROGRESS_NAME_KEY);
			EditorLocalPrefs.Save();
		}
	}
}
