using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Icy.Base;
using Cysharp.Threading.Tasks;
using Icy.UI;
using Icy.Asset;
using YooAsset;

public class ExampleRoot : MonoBehaviour
{
	[SerializeField] private EPlayMode _AssetMode;
	[SerializeField] private Camera _Camera3D;

	// Start is called before the first frame update
	async void Start()
	{
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
		await AssetManager.Instance.StartPatch();

		//=================开始业务逻辑=================

		//显示UI
		UILogin uiLogin = null;
		//回调风格加载
		UIManager.Instance.Get<UILogin>((UIBase ui) =>
		{
			uiLogin = ui as UILogin;
			uiLogin.Show();
			Log.LogInfo($"UILogin is showing = {UIManager.Instance.IsShowing<UILogin>()}");
		});


		await UniTask.WaitForSeconds(1);
		//uiLogin.Hide();
		//uiLogin.Destroy();

		//UniTask风格加载
		//UIExample uiExample = await UIManager.Instance.GetAsync<UIExample>();
		//uiExample.Show();

		//await UniTask.WaitForSeconds(1);

		////uiExample.HideToPrev();
		//uiExample.DestroyToPrev();
	}

	// Update is called once per frame
	void Update()
    {
#if UNITY_EDITOR
		TestPlayground.Update();
#endif
	}
}
