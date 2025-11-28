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
		_Button.onClick.AddListener(OnClickBtn);
	}

	public override void Show(IUIParam param = null)
	{
		base.Show(param);
		TestBindAsync().Forget();
		//DelayByTime(() => { throw new System.Exception("ee"); }, 5);
	}

	public override void Hide()
	{
		_Bg.UnbindTo(BgName);
		_Title.UnbindTo(BgName);
		SliderValue.UnbindTo(SliderValue2);

		base.Hide();
	}

	private async UniTaskVoid TestBindAsync()
	{
		SliderValue.BindTo(SliderValue2);

		_Bg.BindTo(BgName);
		_Title.BindTo(BgName);
		await WaitForSeconds(1);
		Log.Info(1, nameof(UILogin));
		BgName.Data = "icon_loading";

		_Slider.BindTo(SliderValue, (BindableData<float> a) => { return a * 8; });
		await WaitForSeconds(1);
		Log.Info(2, nameof(UILogin));
		SliderValue2.Data = 0.1f;

		_Slider.UnbindTo(SliderValue);
		await WaitForSeconds(1);
		Log.Info(3, nameof(UILogin));
		SliderValue.Data = 0.0f;
	}

	private void OnClickBtn()
	{
		Log.Info("Click Btn");
	}

	public override void Destroy()
	{
		
		base.Destroy();
	}
}
