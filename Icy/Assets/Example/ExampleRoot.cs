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
		//�����س�ʼ��
		GameObject icyGo = new GameObject("Icy", typeof(IcyFrame));
		IcyFrame.Instance.Init();
		Log.Init(true);
		UIRoot.Instance.AddUICameraToCameraStack(_Camera3D);

		//��Դ�ȸ�
		bool assetMgrInitSucceed = await AssetManager.Instance.Init(_AssetMode, "DefaultPackage", 30);
		if (!assetMgrInitSucceed)
		{
			Log.Assert(false, "AssetManager init failed!");
			return;
		}
		await AssetManager.Instance.StartPatch();

		//=================��ʼҵ���߼�=================

		//��ʾUI
		UILogin uiLogin = null;
		//�ص�������
		UIManager.Instance.Get<UILogin>((UIBase ui) =>
		{
			uiLogin = ui as UILogin;
			uiLogin.Show();
			Log.LogInfo($"UILogin is showing = {UIManager.Instance.IsShowing<UILogin>()}");
		});


		await UniTask.WaitForSeconds(1);
		//uiLogin.Hide();
		//uiLogin.Destroy();

		//UniTask������
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
