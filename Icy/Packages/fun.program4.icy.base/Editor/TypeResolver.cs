/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Icy.Base.Editor
{
	/// <summary>
	/// 根据类名字符串，通过反射，获取其Type
	/// </summary>
	public static class TypeResolver
	{
		private static readonly Dictionary<string, Type> _TypeCache = new Dictionary<string, Type>();

		// Unity程序集优先级列表
		private static readonly string[] _PriorityAssemblies =
		{
			"Assembly-CSharp",
			"Assembly-CSharp-Editor",
			"Assembly-CSharp-firstpass",
		};

		[InitializeOnLoadMethod]
		static void Init()
		{
			AssemblyReloadEvents.afterAssemblyReload -= ClearCache;
			AssemblyReloadEvents.afterAssemblyReload += ClearCache;
		}

		/// <summary>
		/// 获取指定，带namespace或者只有类名都支持
		/// </summary>
		public static Type GetType(string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
			{
				Debug.LogError("Type name cannot be null or empty");
				return null;
			}

			if (_TypeCache.TryGetValue(typeName, out Type cachedType))
			{
				return cachedType;
			}

			Type type = ResolveType(typeName);

			_TypeCache[typeName] = type;

			if (type == null)
			{
				Debug.LogError($"Type not found: {typeName}");
			}

			return type;
		}

		private static Type ResolveType(string typeName)
		{
			// 1. 尝试直接通过Type.GetType()获取
			Type type = Type.GetType(typeName);
			if (type != null)
				return type;

			// 2. 尝试在已加载的程序集中查找
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			// 2.1 优先搜索完整类型名（带命名空间）
			type = SearchInAssemblies(typeName, assemblies, true);
			if (type != null)
				return type;

			// 2.2 尝试处理嵌套类型语法（将.替换为+）
			if (typeName.Contains('.'))
			{
				string nestedTypeName = typeName.Replace('.', '+');
				type = SearchInAssemblies(nestedTypeName, assemblies, true);
				if (type != null)
					return type;
			}

			// 3. 尝试仅通过类名搜索（无命名空间）
			type = SearchInAssemblies(typeName, assemblies, false);
			if (type != null)
				return type;

			// 4. 尝试处理泛型类型（如List`1）
			if (typeName.Contains('`'))
			{
				type = SearchGenericType(typeName, assemblies);
				if (type != null)
					return type;
			}

			return null;
		}

		private static Type SearchInAssemblies(string typeName, Assembly[] assemblies, bool fullNameMatch)
		{
			// 按优先级排序程序集
			IOrderedEnumerable<Assembly> orderedAssemblies = assemblies.OrderBy(a =>
			{
				string name = a.GetName().Name;
				int index = Array.IndexOf(_PriorityAssemblies, name);
				return index >= 0 ? index : int.MaxValue;
			});

			foreach (Assembly assembly in orderedAssemblies)
			{
				try
				{
					// 完整名称匹配
					if (fullNameMatch)
					{
						Type type = assembly.GetType(typeName);
						if (type != null) return type;
					}
					// 仅类名匹配（无命名空间）
					else
					{
						Type type = assembly.GetTypes()
							.FirstOrDefault(t => t.Name == typeName);

						if (type != null) return type;
					}
				}
				catch (ReflectionTypeLoadException)
				{
					// 忽略加载异常的程序集
				}
			}

			return null;
		}

		private static Type SearchGenericType(string typeName, Assembly[] assemblies)
		{
			// 分离泛型参数计数（如List`1）
			int backtickIndex = typeName.IndexOf('`');
			if (backtickIndex < 0)
				return null;

			string baseName = typeName.Substring(0, backtickIndex);
			string argCountStr = typeName.Substring(backtickIndex + 1);

			if (!int.TryParse(argCountStr, out int argCount))
				return null;

			foreach (Assembly assembly in assemblies)
			{
				try
				{
					Type type = assembly.GetTypes()
						.FirstOrDefault(t =>
							t.Name.StartsWith(baseName) &&
							t.GetGenericArguments().Length == argCount);

					if (type != null)
						return type;
				}
				catch (ReflectionTypeLoadException)
				{
					// 忽略加载异常的程序集
				}
			}

			return null;
		}

		public static void ClearCache()
		{
			_TypeCache.Clear();
		}
	}
}
