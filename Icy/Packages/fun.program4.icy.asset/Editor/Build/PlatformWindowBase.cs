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
		protected BuildTarget _CurrBuildTarget;
		/// <summary>
		/// Odin Tab组件
		/// </summary>
		protected InspectorProperty _TabGroupProperty;
		/// <summary>
		/// 当前选中平台的名字
		/// </summary>
		protected string _CurrPlatformName;

		protected static void CreateWindow()
		{
			if (_Window != null)
				_Window.Close();
			_Window = GetWindow<T>();
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

					Log.LogInfo($"Switch to platform {_CurrPlatformName}", "BuildWindow");
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
							Log.Assert(false, $"Unsupported platform {_CurrPlatformName}");
							break;
					}

					OnChangePlatformTab(_CurrPlatformName, _CurrBuildTarget);
				}
			}
		}

		protected abstract void OnChangePlatformTab(string tabName, BuildTarget buildTarget);
	}
}
