using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Icy.Base;
using Cysharp.Threading.Tasks;
using Icy.UI;
using Icy.Asset;
using YooAsset;
using System.Globalization;
using System.ComponentModel;
#if ICY_USE_SRDEBUGGER
using Icy.GM;
using SRDebugger;
#endif


public class ExampleRoot : MonoBehaviour
{
	[SerializeField] private EPlayMode _AssetMode;
	[SerializeField] private Camera _Camera3D;

	// Start is called before the first frame update
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
#if ICY_USE_SRDEBUGGER
		GM.Init(new TestGM());//可以根据服务器，决定是否要开启GM
#endif


		//资源热更
		bool assetMgrInitSucceed = await AssetManager.Instance.Init(_AssetMode, "DefaultPackage", 30);
		if (!assetMgrInitSucceed)
		{
			Log.Assert(false, "AssetManager init failed!");
			return;
		}
		await AssetManager.Instance.StartPatch();

		//=================开始业务逻辑=================

		//显示UI
		UILogin uiLogin = await UIManager.Instance.Show<UILogin>();
		Log.LogInfo($"UILogin is showing = {UIManager.Instance.IsShowing<UILogin>()}");

		await UniTask.WaitForSeconds(1);
		UIManager.Instance.Hide<UILogin>();

		UIExample uiExample = await UIManager.Instance.Show<UIExample>();

		await UniTask.WaitForSeconds(1);
		UIManager.Instance.HideToPrev<UIExample>();
	}

	// Update is called once per frame
	void Update()
    {
#if UNITY_EDITOR
		TestPlayground.Update();
#endif
	}
}

#if ICY_USE_SRDEBUGGER
public class TestGM : IGMOptions
{
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add { }
		remove { }
	}

	[Category("AAA"), DisplayName("Clear LocalPrefs"), Sort(1)]
	public void DeleteSave()
	{

	}
}
#endif
