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
	private BindableData<float> SliderValue2 = new BindableData<float>(0);

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
		SliderValue.BindTo(SliderValue2);

		_Bg.BindTo(BgName);
		_Title.BindTo(BgName);
		await UniTask.WaitForSeconds(1);
		BgName.Data = "icon_loading";

		_Slider.BindTo(SliderValue, (BindableData<float> a) => { return a * 8; });
		await UniTask.WaitForSeconds(1);
		SliderValue2.Data = 0.1f;

		_Slider.UnbindTo(SliderValue);
		await UniTask.WaitForSeconds(1);
		SliderValue.Data = 0.0f;

		_Title.UnbindTo(BgName);
	}

	public override void Destroy()
	{
		
		base.Destroy();
	}
}
