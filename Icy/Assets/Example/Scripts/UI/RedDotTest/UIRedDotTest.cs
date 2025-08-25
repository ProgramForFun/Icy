using UnityEngine;
using Icy.UI;
using Sirenix.OdinInspector;
using Icy.Base;

/// <summary>
/// 
/// </summary>
public class UIRedDotTest : UIBase
{
//↓=========================== Generated code area，do NOT put your business code in this ===========================↓
	[SerializeField, ReadOnly] Icy.UI.RedDot _RedDot_1;
	[SerializeField, ReadOnly] Icy.UI.RedDot _RedDot_2;
	[SerializeField, ReadOnly] Icy.UI.RedDot _RedDot_3;
	[SerializeField, ReadOnly] Icy.UI.RedDot _RedDot_4;
	[SerializeField, ReadOnly] Icy.UI.RedDot _RedDot_0;
//↑=========================== Generated code area，do NOT put your business code in this ===========================↑

	public override void Init()
	{
		base.Init();

	}

	public override void Show(IUIParam param = null)
	{
		base.Show(param);

		_RedDot_0.Init("RD_0", Calc0);
		_RedDot_2.Init("RD_2", null, "RD_1");//支持Child可以在Parent前Init
		_RedDot_1.Init("RD_1", Calc1, _RedDot_0.ID);
		_RedDot_3.Init("RD_3", Calc3, _RedDot_2.ID);
		_RedDot_4.Init("RD_4", Calc4, _RedDot_2.ID);
		_RedDot_3.Refresh();
		_RedDot_4.Refresh();
	}

	public override void Hide()
	{
		
		base.Hide();
	}

	protected int Calc0()
	{
		return 1;
	}

	protected int Calc1()
	{
		return 1;
	}

	protected int red3 = 0;
	protected int Calc3()
	{
		return red3;
	}

	protected int red4 = 0;
	protected int Calc4()
	{
		return red4;
	}

	protected void Update()
	{
		if (Input.GetKeyUp(KeyCode.Alpha3))
		{
			red3 = red3 == 0 ? 2 : 0;
			Log.LogInfo(red3.ToString());
			_RedDot_3.Refresh();
		}

		if (Input.GetKeyUp(KeyCode.Alpha4))
		{
			red4 = red4 == 0 ? 2 : 0;
			Log.LogInfo(red4.ToString());
			_RedDot_4.Refresh();
		}

		if (Input.GetKeyUp(KeyCode.R))
		{
			_RedDot_1.Dispose();
		}

		if (Input.GetKeyUp(KeyCode.T))
		{
			_RedDot_1.Init("RD_1", Calc1, _RedDot_0.ID);
			_RedDot_1.Refresh();
		}
	}

	public override void Destroy()
	{
		
		base.Destroy();
	}
}
