using UnityEngine;
using Icy.Frame;
using Icy.UI;
using Icy.Asset;
using YooAsset;
using System.Globalization;

namespace Bootstrap
{
	/// <summary>
	/// HybridCLR概念下的AOT启动入口，是整个游戏的逻辑起点，也负责资源初始化和热更代码加载
	/// </summary>
	public class Bootstrap : MonoBehaviour
	{
		[SerializeField] private EPlayMode _AssetMode;
		[SerializeField] private Camera _Camera3D;

		public EPlayMode AssetMode => _AssetMode;

		async void Start()
		{
			//基础功能的开关
			UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
			Application.runInBackground = true;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			//避免某些地区比如南非，1.23ToString成1,23的问题
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			//框架相关初始化
			// 1、初始化框架
			GameObject icyGo = new GameObject("Icy", typeof(IcyFrame));
			await IcyFrame.Instance.Init(_AssetMode);

			// 2、热更新资源
			await AssetManager.Instance.StartAssetPatch();
#if !UNITY_EDITOR
			if (HybridCLRRunner.IsHybridCLREnabled)
			{
				// 3、加载热更代码
				// 4、运行热更代码（等HybridCLR运行时加载完成，在RunPatchedCode回调中调用热更代码）
				await AssetManager.Instance.RunPatchedCSharpCode(RunPatchedCode);
			}
			else
			{
				// 4、运行热更代码（未启用HybridCLR，直接调用热更代码）
				RunPatchedCode();
			}
#else
			// 4、运行热更代码（Editor下跳过HybridCLR运行时加载代码，直接调用热更代码）
			RunPatchedCode();
#endif

			// 在合适的时机调用此方法，把UI相机添加到业务侧的3D主相机上
			UIRoot.Instance.AddUICameraToCameraStack(_Camera3D);
		}

		/// <summary>
		/// 调用热更代码，从此处热更代码开始执行
		/// </summary>
		void RunPatchedCode()
		{
			AssetRef patchCodeEntry = AssetManager.Instance.LoadAsset("ExampleRoot");
			GameObject entryGo = GameObject.Instantiate(patchCodeEntry.AssetObject) as GameObject;
		}
	}
}
