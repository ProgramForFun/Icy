using UnityEngine;
using Icy.UI;
using Sirenix.OdinInspector;
using Icy.Base;
using Cysharp.Threading.Tasks;


/// <summary>
/// 
/// </summary>
public class UILogin : UIBase
{
//↓=========================== Generated code area，do NOT put your business code in this ===========================↓
	[SerializeField, ReadOnly] UnityEngine.UI.Image _Bg;
	[SerializeField, ReadOnly] TMPro.TextMeshProUGUI _Title;
	[SerializeField, ReadOnly] UnityEngine.UI.Button _Button;
	[SerializeField, ReadOnly] UnityEngine.UI.Slider _Slider;
//↑=========================== Generated code area，do NOT put your business code in this ===========================↑

	private UILoginLogic _Logic;
	private BindableData<string> BgName = new BindableData<string>("");
	private BindableData<float> SliderValue = new BindableData<float>(0);

	public override void Init()
	{
		base.Init();
		_Logic = new();
		_Logic.Init();

	}

	public override void Show(IUIParam param = null)
	{
		base.Show(param);
		TestBindAsync().Forget();
	}

	public override void Hide()
	{
		
		base.Hide();
	}

	private async UniTaskVoid TestBindAsync()
	{
		_Bg.Bind(BgName);
		await UniTask.WaitForSeconds(1);
		BgName.SetData("icon_loading");

		_Slider.Bind(SliderValue);
		await UniTask.WaitForSeconds(1);
		SliderValue.SetData(1.0f);

		_Slider.Unbind(SliderValue);
		await UniTask.WaitForSeconds(1);
		SliderValue.SetData(0.0f);
	}

	public override void Destroy()
	{
		
		base.Destroy();
	}
}
