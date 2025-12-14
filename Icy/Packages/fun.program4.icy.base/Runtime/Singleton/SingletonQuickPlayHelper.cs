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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Icy.Base
{
	/// <summary>
	/// 因为Singleton是泛型类，所以UnityEditor.InitializeOnEnterPlayMode之类的特性无法生效；
	/// 所以用这个非泛型类，来保证QuickPlay时static instance变量的置空
	/// </summary>
	public static class SingletonQuickPlayHelper
	{
		[InitializeOnEnterPlayMode]
		private static void OnEnterPlayMode()
		{
			DestroyAllSingletonInstance();
		}

		public static void DestroyAllSingletonInstance()
		{
			if (!EditorSettings.enterPlayModeOptionsEnabled)
				return;

			if (!EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload))
				return;

			// TODO：C#代码量上来之后，此处性能有待观察
			Type interfaceType = typeof(ISingleton);
			List<Type> allDerivedTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
				.ToList();

			Type baseType = typeof(Singleton<>);
			for (int i = 0; i < allDerivedTypes.Count; i++)
			{
				if (CommonUtility.IsSubclassOfRawGeneric(allDerivedTypes[i], baseType))
				{
					MethodInfo method = allDerivedTypes[i].GetMethod("DestroyInstance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
					method.Invoke(null, null);
				}
			}
		}
	}
}
#endif
