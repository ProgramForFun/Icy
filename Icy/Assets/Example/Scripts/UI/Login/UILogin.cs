using UnityEngine;
using Icy.UI;
using Sirenix.OdinInspector;


/// <summary>
/// 
/// </summary>
public class UILogin : UIBase
{
//↓=========================== Generated code area，do NOT put your business code in this ===========================↓
	[SerializeField, ReadOnly] UnityEngine.UI.Image _Bg;
	[SerializeField, ReadOnly] TMPro.TextMeshProUGUI _Title;
	[SerializeField, ReadOnly] UnityEngine.UI.Button _Button;
//↑=========================== Generated code area，do NOT put your business code in this ===========================↑

	private UILoginLogic _Logic;

	public override void Init()
	{
		base.Init();
		_Logic = new();
		_Logic.Init();

	}

	public override void Show(IUIParam param = null)
	{
		base.Show(param);
		
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
