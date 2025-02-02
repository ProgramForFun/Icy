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

		// proto��Ϣ����ת�����ֽ�����
		byte[] dataBytes = messageResult.ToByteArray();

		// proto��Ϣ�ֽ����飬ת���ɶ���
		// ��һ�ַ�ʽ��ʵ������
		// IMessage message = new MessageResult();
		// MessageResult newMessageResult = (MessageResult)message.Descriptor.Parser.ParseFrom(dataBytes);
		// �ڶ��ַ�ʽ����ֱ̬�ӵ���
		TestMessageResult newMessageResult = TestMessageResult.Descriptor.Parser.ParseFrom(dataBytes) as TestMessageResult;

		Debug.Log(newMessageResult.ErrorCode);
		Debug.Log(newMessageResult.ErrorMsg);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
