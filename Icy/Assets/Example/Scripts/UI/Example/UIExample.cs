using UnityEngine;
using Icy.UI;
using Sirenix.OdinInspector;
using SuperScrollView;
using System;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class UIExample : UIBase
{
//↓=========================== Generated code area，do NOT put your business code in this ===========================↓
	[TitleGroup("Components")]
	[SerializeField, ReadOnly] SuperScrollView.LoopListView2 _ScrollView;
//↑=========================== Generated code area，do NOT put your business code in this ===========================↑

	public override void Init()
	{
		base.Init();
		_ScrollView.InitListView(128, OnItem);
	}

	public override void Show(IUIParam param = null)
	{
		base.Show(param);
	}

	private LoopListViewItem2 OnItem(LoopListView2 view, int arg2)
	{
		LoopListViewItem2 item = _ScrollView.NewListViewItem("Item");
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
