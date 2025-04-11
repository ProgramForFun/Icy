namespace Icy.UI.Editor
{
	/// <summary>
	/// UI代码的生成模板
	/// </summary>
	public static class UICodeTemplate
	{
		public readonly static string Code =
@"using UnityEngine;
using Icy.UI;
using Sirenix.OdinInspector;

/// <summary>
/// 
/// </summary>
public class UI{0} : UIBase
{{
//↓=========================== Generated code area，do NOT put your business code in this ===========================↓
{1}
//↑=========================== Generated code area，do NOT put your business code in this ===========================↑
{2}
	public override void Init()
	{{
		base.Init();
{3}
	}}

	public override void Show(IUIParam param = null)
	{{
		base.Show(param);
		
	}}

	public override void Hide()
	{{
		
		base.Hide();
	}}





	public override void Destroy()
	{{
		
		base.Destroy();
	}}
}}
";
	}
}
