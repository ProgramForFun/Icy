using System;
using System.Reflection;
using UnityEditor;

namespace Icy.Editor
{
	/// <summary>
	/// 用反射调用内部类，清空Unity editor的Console窗口
	/// </summary>
	public static class ClearConsole
	{
		[MenuItem("Icy/Tools/Clear Console", false, 20)]
		public static void Clear()
		{
			Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
			MethodInfo clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
			clearMethod?.Invoke(null, null);
		}
	}
}
