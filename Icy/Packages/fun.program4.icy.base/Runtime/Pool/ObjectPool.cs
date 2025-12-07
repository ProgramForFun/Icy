/*
 * Copyright 2025 @ProgramForFun. All Rights Reserved.
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


using System;
using System.Collections.Generic;
using UnityEngine;

namespace Icy.Base
{
	/// <summary>
	/// 泛型对象池
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ObjectPool<T> : IDisposable where T : new()
	{
		protected List<T> _InPool;
		protected List<T> _OutPool;

		/// <summary>
		/// 当前池里还缓存有多少个对象可用
		/// </summary>
		public int CacheCount => _InPool.Count;


		public ObjectPool(int defaultSize = 16)
		{
			_InPool = new List<T>(defaultSize);
			_OutPool = new List<T>(defaultSize);

#if UNITY_EDITOR
			if (typeof(T) == typeof(GameObject) && this.GetType() != typeof(GameObjectPool))
				Log.Error($"ObjectPool<GameObject> is unexpected, Use {nameof(GameObjectPool)} instead");
#endif
		}

		public virtual T Get()
		{
			if (_InPool.Count > 0)
			{
				int lastIdx = _InPool.Count - 1;
				T lastObj = _InPool[lastIdx];
				_InPool.RemoveAt(lastIdx);
				_OutPool.Add(lastObj);
				return lastObj;
			}
			else
			{
				T newInstance = InstantiateOne();
				_OutPool.Add(newInstance);
				return newInstance;
			}
		}

		public virtual void Put(T obj)
		{
			if (_OutPool.Remove(obj))
				_InPool.Add(obj);
			else
				Log.Error("Trying to put an invalid object to ObjectPool, object = " + obj.ToString());
		}

		protected virtual T InstantiateOne()
		{
			return new T();
		}

		public virtual void Dispose()
		{
			if (_OutPool.Count > 0)
				Log.Warn("Dispose ObjectPool when there are objects outside, first outside object = " + _OutPool[0].ToString());
		}
	}
}
