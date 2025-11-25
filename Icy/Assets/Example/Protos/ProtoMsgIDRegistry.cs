//=============================
// Generated code, do NOT edit!
//=============================
using pb = global::Google.Protobuf;
using pbr = global::Google.Protobuf.Reflection;
using System.Collections.Generic;
using System;

/// <summary>
/// Proto MsgID到Descriptor、Proto类型到MsgID 的映射；
/// 业务侧可以在这里根据Proto的ID，获取其MessageDescriptor，或者反之
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


	/// <summary>
	/// 注册所有Proto Msg
	/// </summary>
	public static void RegisterAll()
	{
		_MsgID2Descriptor = new Dictionary<int, pbr.MessageDescriptor>();
		_TypeName2MsgID = new Dictionary<Type, int>();
		Register<TestMsg.TestMessageResult>();
		Register<TestMsg.TestNest>();

	}

	/// <summary>
	/// 注册一个Proto Msg
	/// </summary>
	public static void Register<T>() where T : pb.IMessage, new()
	{
		pbr.MessageDescriptor descriptor = new T().Descriptor;
		pbr.MessageOptions options = descriptor.GetOptions();

		if (options != null && options.HasExtension(ProtoOptionsExtensions.MsgID))
		{
			int id = options.GetExtension(ProtoOptionsExtensions.MsgID);
			_MsgID2Descriptor[id] = descriptor;
			_TypeName2MsgID[typeof(T)] = id;
		}
	}

	/// <summary>
	/// 根据MsgID获取 MessageDescriptor
	/// </summary>
	public static pbr.MessageDescriptor GetMsgDescriptor(int msgID)
	{
		return _MsgID2Descriptor.TryGetValue(msgID, out pbr.MessageDescriptor descriptor) ? descriptor : null;
	}

	/// <summary>
	/// 根据Proto类型，获取MsgID
	/// </summary>
	/// <returns>如果没有找到，返回-1</returns>
	public static int GetMsgID<T>() where T : pb.IMessage, new()
	{
		Type type = typeof(T);
		if (_TypeName2MsgID.ContainsKey(type))
			return _TypeName2MsgID[type];
		else
			return -1;
	}
}
