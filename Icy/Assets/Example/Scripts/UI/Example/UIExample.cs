using UnityEngine;
using Icy.UI;
using Sirenix.OdinInspector;
using SuperScrollView;
using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// 
/// </summary>
public class UIExample : UIBase
{
//↓=========================== Generated code area，do NOT put your business code in this ===========================↓
	[TitleGroup("Components")]
	[SerializeField, ReadOnly] SuperScrollView.LoopListView2 _ScrollView;
	[SerializeField, ReadOnly] UnityEngine.UI.Image _Item1;
	[SerializeField, ReadOnly] UnityEngine.UI.Image _Item2;
	[SerializeField, ReadOnly] Icy.UI.StatusSwitcher _StatusRoot;
//↑=========================== Generated code area，do NOT put your business code in this ===========================↑

	public override void Init()
	{
		base.Init();
		_ScrollView.InitListView(128, OnItem);
	}

	public override void Show(IUIParam param = null)
	{
		base.Show(param);

		TestAsync().Forget();
	}

	private async UniTaskVoid TestAsync()
	{
		await WaitForSeconds(1);

		_StatusRoot.SwitchTo("666");

		await WaitForSeconds(1);

		_StatusRoot.SwitchTo("123");

		await WaitForSeconds(1);

		_StatusRoot.SwitchTo("666");
	}

	private LoopListViewItem2 OnItem(LoopListView2 view, int arg2)
	{
		LoopListViewItem2 item = _ScrollView.NewListViewItem("Template");
		Text text = item.transform.GetChild(0).GetComponent<Text>();
		text.text = arg2.ToString();
		return item;
	}

	public override void Hide()
	{
		
		base.Hide();
	}





	public override void Destroy()
	{
		
		base.Destroy();
	}
}
