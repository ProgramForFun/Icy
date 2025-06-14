using pb = global::Google.Protobuf;
namespace TestMsg
{
	public sealed partial class TestMessageResult : pb::IMessage<TestMessageResult>
	{
		public void Reset()
		{
			errorCode_ = default;
			errorMsg_ = default;
			list_.Clear();
			obj_.Reset();
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
