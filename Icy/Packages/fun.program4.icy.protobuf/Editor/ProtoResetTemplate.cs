
namespace Icy.Protobuf.Editor
{
	public static class ProtoResetTemplate
	{
		public readonly static string Code =
@"//Generated code, do NOT edit
using pb = global::Google.Protobuf;

public static class ProtoIMessageResetExtension
{{
	public static void Reset(this pb::IMessage msg)
	{{
		switch(msg)
		{{
{0}
		}}
	}}
}}

{1}
";
	}
}
