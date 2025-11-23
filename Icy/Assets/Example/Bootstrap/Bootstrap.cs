using UnityEngine;
using Icy.Frame;
using Icy.Base;
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

		async void Start()
		{
			//基础功能的开关
			UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
			Application.runInBackground = true;
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			//避免某些地区比如南非，1.23ToString成1,23的问题
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			//框架相关初始化
			GameObject icyGo = new GameObject("Icy", typeof(IcyFrame));
			IcyFrame.Instance.Init();
			Log.Init(true);
			UIRoot.Instance.AddUICameraToCameraStack(_Camera3D);

			//资源热更
			bool assetMgrInitSucceed = await AssetManager.Instance.Init(_AssetMode, "DefaultPackage", 30);
			if (!assetMgrInitSucceed)
			{
				Log.Assert(false, "AssetManager init failed!");
				return;
			}

			//先更新资源
			await AssetManager.Instance.StartAssetPatch();
#if !UNITY_EDITOR
			//再加载热更代码
			await AssetManager.Instance.RunPatchedCSharpCode(RunPatchedCode);
#else
			//Editor下跳过HybridCLR运行时加载代码，直接调用热更代码即可
			RunPatchedCode();
#endif
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
