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
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Linq;

namespace Icy.Asset.Editor
{
	/// <summary>
	/// 使用了Odin TabGroup组件来区分打包平台的EditorWindow，抽出来的公共逻辑
	/// </summary>
	public abstract class PlatformWindowBase<T> : OdinEditorWindow where T : OdinEditorWindow
	{
		protected static T _Window;

		/// <summary>
		/// 当前选中平台的BuildTarget
		/// </summary>
		protected BuildTarget _CurrBuildTarget = BuildTarget.Android;
		/// <summary>
		/// Odin Tab组件
		/// </summary>
		protected InspectorProperty _TabGroupProperty;
		/// <summary>
		/// 当前选中平台的名字
		/// </summary>
		protected string _CurrPlatformName;

		protected static T CreateWindow()
		{
			if (_Window != null)
				_Window.Close();
			_Window = GetWindow<T>();
			return _Window;
		}

		protected virtual void Update() 
		{
#pragma warning disable CS0618
			if (_TabGroupProperty == null)
			{
				if (PropertyTree != null && PropertyTree.RootProperty != null)
					_TabGroupProperty = PropertyTree.RootProperty.Children.FirstOrDefault(p => p.Attributes.HasAttribute<TabGroupAttribute>());
			}
#pragma warning restore CS0618

			if (_TabGroupProperty != null)
			{
				string currTabName = _TabGroupProperty.State.Get<string>("CurrentTabName");
				if (_CurrPlatformName != currTabName)
				{
					_CurrPlatformName = currTabName;

					Log.Info($"Switch to platform {_CurrPlatformName}", nameof(BuildWindow));
					switch (_CurrPlatformName)
					{
						case "Android":
							_CurrBuildTarget = BuildTarget.Android;
							break;
						case "iOS":
							_CurrBuildTarget = BuildTarget.iOS;
							break;
						case "Win64":
							_CurrBuildTarget = BuildTarget.StandaloneWindows64;
							break;
						default:
							Log.Error(false, $"Unsupported platform {_CurrPlatformName}");
							_CurrBuildTarget = BuildTarget.Android;
							break;
					}

					OnChangePlatformTab(_CurrPlatformName, _CurrBuildTarget);
				}
			}
		}

		protected abstract void OnChangePlatformTab(string tabName, BuildTarget buildTarget);

		protected bool IsPlayMode()
		{
			return EditorApplication.isPlaying;
		}
	}
}
