using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using TestMsg;
using Icy.Base;
using Cysharp.Threading.Tasks;

public class ExampleRoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		
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

	void TestLog()
	{
		Log.MinLogLevel = LogLevel.Warning;
		Log.OverrideTagLogLevel("game", LogLevel.Info);
		Log.LogInfo("11111", "game");
	}

	void TestProcedure()
	{
		Procedure procedure = new Procedure("TestProcedure");
		procedure.AddStep(new StepA());
		procedure.AddStep(new StepB());
		procedure.Start();
	}

	void TestEvent()
	{
		EventManager.Instance.AddListener(0, eventCallback);
		EventParam_Int param_Int = new EventParam_Int();
		param_Int.Value = 1;
		EventManager.Instance.FireEvent(0, param_Int);

		//EventManager.Instance.RemoveListener(0, eventCallback);

		EventManager.Instance.FireEventDelay(0, param_Int, 1);
	}
	void eventCallback(int eventID, IEventParam param)
	{
		if (param is EventParam_Int param_Int)
			Log.LogInfo(param_Int.Value.ToString(), "game");
	}

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
		TestPlayground.Update();
#endif
	}
}

public class StepA : ProcedureStep
{
	public override async UniTask Activate()
	{
		await UniTask.WaitForSeconds(3);
		Log.LogInfo("StepA wait for 3");
		Finish();
	}

	public override async UniTask Deactivate()
	{
		await UniTask.CompletedTask;
	}
}

public class StepB : ProcedureStep
{
	public override async UniTask Activate()
	{
		await UniTask.WaitForSeconds(3);
		Log.LogInfo("StepB wait for 3");
		Finish();
	}

	public override async UniTask Deactivate()
	{
		await UniTask.CompletedTask;
	}
}
