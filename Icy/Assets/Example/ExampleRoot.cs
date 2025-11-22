using UnityEngine;
using Icy.Base;
using Cysharp.Threading.Tasks;
using Icy.UI;
using System.ComponentModel;
#if ICY_USE_SRDEBUGGER
using Icy.GM;
using SRDebugger;
#endif

/// <summary>
/// HybridCLR概念下的热更代码入口
/// </summary>
public class ExampleRoot : MonoBehaviour
{
	async void Start()
	{
		Log.Info("Hello from patch code", "Patch", true);

#if ICY_USE_SRDEBUGGER
		GM.Init(new TestGM());//可以根据服务器，决定是否要开启GM
#endif
		//=================开始业务逻辑=================

		//显示UI
		UILogin uiLogin = await UIManager.Instance.ShowAsync<UILogin>();
		Log.Info($"UILogin is showing = {UIManager.Instance.IsShowing<UILogin>()}");

		await UniTask.WaitForSeconds(1);
		UIManager.Instance.Hide<UILogin>();

		UIExample uiExample = await UIManager.Instance.ShowAsync<UIExample>();

		await UniTask.WaitForSeconds(1);
		UIManager.Instance.DestroyToPrev();
	}

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
