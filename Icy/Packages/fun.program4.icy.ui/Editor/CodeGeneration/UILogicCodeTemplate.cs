namespace Icy.UI.Editor
{
	/// <summary>
	/// UI逻辑代码的生成模板
	/// </summary>
	public static class UILogicCodeTemplate
	{
		public readonly static string Code =
@"using Icy.UI;

/// <summary>
/// 
/// </summary>
public class UI{0}Logic : UILogicBase
{{
	public override void Init()
	{{
		base.Init();
		//EnableUpdate();

	}}





	public override void Destroy()
	{{
		
		base.Destroy();
	}}
}}
";
	}
}
