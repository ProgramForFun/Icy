#if UNITY_EDITOR
using Icy.Base;
using UnityEngine;

public static class PoolTest
{
	public struct Value
	{
		public int v;
	}

	public static void Test()
	{
		{
			//ObjectPool：对象类型
			ObjectPool<Blackboard> classObjectPool = new ObjectPool<Blackboard>();
			Log.LogInfo("1、CacheCount = " + classObjectPool.CacheCount);
			Blackboard b = classObjectPool.Get();
			b.WriteFloat("Test", 1);
			classObjectPool.Put(b);
			Log.LogInfo("2、CacheCount = " + classObjectPool.CacheCount);
			classObjectPool.Dispose();
		}

		{
			//ObjectPool：值类型
			ObjectPool<Value> valueObjectPool = new ObjectPool<Value>();
			Log.LogInfo("3、CacheCount = " + valueObjectPool.CacheCount);
			Value v = valueObjectPool.Get();
			v.v = 0;
			valueObjectPool.Put(v);
			Log.LogInfo("4、CacheCount = " + valueObjectPool.CacheCount);
		}

		{
			//错误示例，应使用GameObjectPool
			ObjectPool<GameObject> wrong = new ObjectPool<GameObject>();
		}

		{
			//自定义实例化
			//ObjectPool<Blackboard> classObjectPool = new ObjectPool<Blackboard>();
			//classObjectPool.CustomInstantiate = () =>
			//{
			//	Blackboard b = new Blackboard();
			//	b.WriteFloat("Test", 100);
			//	return b;
			//};
			//Blackboard b = classObjectPool.Get();
			//Log.LogInfo("float = " + b.ReadFloat("Test"));
			//classObjectPool.Put(b);
		}

		{
			//GameObjectPool
			GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
			GameObjectPool gop = new GameObjectPool(template);
			GameObject g = gop.Get();
			g.name = "TestGameObjectName";
			gop.Put(g);
			g = gop.Get();
			Log.LogInfo("GameObject name = " + g.name);
			gop.Dispose();
		}
	}
}
#endif
