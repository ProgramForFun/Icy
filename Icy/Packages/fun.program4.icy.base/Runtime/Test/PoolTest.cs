/*
 * Copyright 2025-2026 @ProgramForFun. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


#if UNITY_EDITOR
using UnityEngine;

namespace Icy.Base
{
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
				Log.Info("1、CacheCount = " + classObjectPool.CacheCount);
				Blackboard b = classObjectPool.Get();
				b.WriteFloat("Test", 1);
				classObjectPool.Put(b);
				Log.Info("2、CacheCount = " + classObjectPool.CacheCount);
				classObjectPool.Dispose();
			}

			{
				//ObjectPool：值类型
				ObjectPool<Value> valueObjectPool = new ObjectPool<Value>();
				Log.Info("3、CacheCount = " + valueObjectPool.CacheCount);
				Value v = valueObjectPool.Get();
				v.v = 0;
				valueObjectPool.Put(v);
				Log.Info("4、CacheCount = " + valueObjectPool.CacheCount);
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
				//Log.Info("float = " + b.ReadFloat("Test"));
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
				Log.Info("GameObject name = " + g.name);
				gop.Dispose();
			}
		}
	}
}
#endif
