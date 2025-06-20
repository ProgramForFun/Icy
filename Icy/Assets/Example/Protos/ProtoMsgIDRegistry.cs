//Generated code, do NOT edit
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;
using System.Collections.Generic;
using System;

/// <summary>
/// Proto MsgID到Descriptor、Proto类型到MsgID 的映射
/// </summary>
public static class ProtoMsgIDRegistry
{
	/// <summary>
	/// ID → 消息描述符的映射
	/// </summary>
	private static Dictionary<int, pbr.MessageDescriptor> _MsgID2Descriptor;
	/// <summary>
	/// 消息类型 → ID 的映射
	/// </summary>
	private static Dictionary<Type, int> _TypeName2MsgID;

	public static void RegisterAll()
	{
		_MsgID2Descriptor = new Dictionary<int, pbr.MessageDescriptor>();
		_TypeName2MsgID = new Dictionary<Type, int>();
		Register<TestMsg.TestMessageResult>();
		Register<TestMsg.TestNest>();

	}

	private static void Register<T>() where T : pb.IMessage, new()
	{
		pbr.MessageDescriptor descriptor = new T().Descriptor;
		pbr.MessageOptions options = descriptor.GetOptions();

		if (options != null && options.HasExtension(IcyOptionsExtensions.MsgID))
		{
			int id = options.GetExtension(IcyOptionsExtensions.MsgID);
			_MsgID2Descriptor[id] = descriptor;
			_TypeName2MsgID[typeof(T)] = id;
		}
	}

	/// <summary>
	/// 根据MsgID获取 MessageDescriptor
	/// </summary>
	public static pbr.MessageDescriptor GetDescriptor(int msgID)
	{
		return _MsgID2Descriptor.TryGetValue(msgID, out pbr.MessageDescriptor descriptor) ? descriptor : null;
	}

	/// <summary>
	/// 根据Proto类型，获取MsgID
	/// </summary>
	public static int GetId<T>() where T : pb.IMessage, new()
	{
		return _TypeName2MsgID[typeof(T)];
	}
}
