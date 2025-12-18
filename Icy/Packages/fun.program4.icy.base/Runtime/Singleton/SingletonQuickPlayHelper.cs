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
using System.Diagnostics;
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

		/// <summary>
		/// TODO：C#代码量上来之后，此处性能有待观察
		/// </summary>
		public static void DestroyAllSingletonInstance()
		{
			if (!EditorSettings.enterPlayModeOptionsEnabled)
				return;

			if (!EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload))
				return;

			Stopwatch stopwatch = Stopwatch.StartNew();
			//带where38个程序集
			IEnumerable<Assembly> allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(asm =>
				{
					string fullName = asm.FullName;
					return !fullName.StartsWith("System") && !fullName.StartsWith("Unity")
						&& !fullName.StartsWith("YooAsset") && !fullName.StartsWith("UniTask")
						&& !fullName.StartsWith("Coffee") && !fullName.StartsWith("Sirenix")
						&& !fullName.StartsWith("nunit") && !fullName.StartsWith("dnlib")
						&& !fullName.StartsWith("Mono") && !fullName.StartsWith("mscorlib")
						&& !fullName.StartsWith("LitMotion");
				});

			Type interfaceType = typeof(ISingleton);
			//不带where大约2100
			IEnumerable<Type> allDerivedTypes = allAssemblies
				.SelectMany(asm => asm.GetTypes())
				.Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));

			Type baseType = typeof(Singleton<>);
			foreach (Type type in allDerivedTypes)
			{
				if (CommonUtility.IsSubclassOfRawGeneric(type, baseType))
				{
					MethodInfo method = type.GetMethod("DestroyInstance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
					method.Invoke(null, null);
				}
			}

			stopwatch.Stop();
			Log.Info($"{nameof(DestroyAllSingletonInstance)} cost {stopwatch.ElapsedMilliseconds} ms", nameof(SingletonQuickPlayHelper));
		}
	}
}
#endif
