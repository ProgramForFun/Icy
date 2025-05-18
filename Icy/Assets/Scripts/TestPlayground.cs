#if UNITY_EDITOR
using TestMsg;
using Google.Protobuf;

//using UnityEditor.ShortcutManagement;
using UnityEngine;
using Icy.Network;
using Icy.Base;
using System;

public static class TestPlayground
{
	public static void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			Test();

		//TcpSessionTest.Update();
		TcpChannelTest.Update();
	}



	//[Shortcut("TestPlayground", KeyCode.Space)]
	public static void Test()
	{
		//LogTest.Test();
		//ProcedureTest.Test();
		//EventTest.Test();
		//ProtobufTest.Test();
		//PoolTest.Test();
		//HttpTest.Test();
		//LocalPrefsTest.Test();
		//PeriodicRecordTest.Test();
		//RingBufferTest.Test();
		//TcpSessionTest.Test();
		TcpChannelTest.Test();
		//ConfigTest.Test();
		//TimerTest.Test();
	}
}


/// <summary>
/// ��Ϊprotobuf���ɵ�C#������������ߣ����Բ����಻��д��Package��
/// </summary>
public static class ProtobufTest
{
	public static void Test()
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
}
#endif
