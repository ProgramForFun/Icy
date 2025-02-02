using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using TestMsg;

public class ExampleRoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		TestProtobuf();
	}

	void TestProtobuf()
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
