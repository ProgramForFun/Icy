#if UNITY_EDITOR
using TestMsg;
using Google.Protobuf;

//using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class TestPlayground
{
	public static void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			Test();
	}

	//[Shortcut("TestPlayground", KeyCode.Space)]
	public static void Test()
	{
		//LogTest.Test();
		//ProcedureTest.Test();
		//EventTest.Test();
		//ProtobufTest.Test();
		PoolTest.Test();
	}
}


/// <summary>
/// 因为protobuf生成的C#类在主工程这边，所以测试类不能写在Package里
/// </summary>
public static class ProtobufTest
{
	public static void Test()
	{
		TestMessageResult messageResult = new TestMessageResult();
		messageResult.ErrorCode = 0;
		messageResult.ErrorMsg = "Success";

		// proto消息对象，转换成字节数组
		byte[] dataBytes = messageResult.ToByteArray();

		// proto消息字节数组，转换成对象
		// 第一种方式：实例调用
		// IMessage message = new MessageResult();
		// MessageResult newMessageResult = (MessageResult)message.Descriptor.Parser.ParseFrom(dataBytes);
		// 第二种方式：静态直接调用
		TestMessageResult newMessageResult = TestMessageResult.Descriptor.Parser.ParseFrom(dataBytes) as TestMessageResult;

		Debug.Log(newMessageResult.ErrorCode);
		Debug.Log(newMessageResult.ErrorMsg);
	}
}
#endif
