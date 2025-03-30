using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Icy.Base;
using Cysharp.Threading.Tasks;
using Icy.UI;

public class ExampleRoot : MonoBehaviour
{
	[SerializeField] private Camera Camera3D;

	// Start is called before the first frame update
	async void Start()
	{
		//框架相关初始化
		GameObject icyGo = new GameObject("Icy", typeof(IcyFrame));
		IcyFrame.Instance.Init();
		Log.Init(true);
		UIRoot.Instance.AddUICameraToCameraStack(Camera3D);


		UILogin uiLogin = null;
		UIManager.Instance.Get<UILogin>((UIBase ui) =>
		{
			uiLogin = ui as UILogin;
			uiLogin.Show();
			Log.LogInfo($"UILogin is showing = {UIManager.Instance.IsShowing<UILogin>()}");
		});


		await UniTask.WaitForSeconds(1);
		//uiLogin.Hide();
		uiLogin.Destroy();


		UIExample uIExample = null;
		UIManager.Instance.Get<UIExample>((UIBase ui) =>
		{
			uIExample = ui as UIExample;
			uIExample.Show();
		});

		await UniTask.WaitForSeconds(1);

		//uIExample.HideToPrev();
		uIExample.DestroyToPrev();
	}

	// Update is called once per frame
	void Update()
    {
#if UNITY_EDITOR
		TestPlayground.Update();
#endif
	}
}
