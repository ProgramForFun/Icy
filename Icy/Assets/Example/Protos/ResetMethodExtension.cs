//Generated code, do NOT edit
using pb = global::Google.Protobuf;

public static class ProtoIMessageResetExtension
{
	public static void Reset(this pb::IMessage msg)
	{
		switch(msg)
		{
			case TestMsg.TestMessageResult testmessageresult:
				testmessageresult.Reset();
				break;
			case TestMsg.TestNest testnest:
				testnest.Reset();
				break;

		}
	}
}

namespace TestMsg
{
	public sealed partial class TestMessageResult : pb::IMessage<TestMessageResult>
	{
		public void Reset()
		{
			errorCode_ = default;
			errorMsg_ = default;
			list_?.Clear();
			obj_?.Reset();
		}
	}
	public sealed partial class TestNest : pb::IMessage<TestNest>
	{
		public void Reset()
		{
			nestInt_ = default;
		}
	}
}

