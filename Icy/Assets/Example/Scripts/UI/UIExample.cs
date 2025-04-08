using System.Collections.Generic;
using UnityEngine;
using Icy.UI;
using UnityEngine.UI;

/// <summary>
/// Ê¾ÀýUI
/// </summary>
public class UIExample : UIBase
{
	[SerializeField] private Image _Icon;

	public override void Init()
	{
		base.Init();

	}

	public override void Show(IUIParam param = null)
	{
		base.Show();

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
