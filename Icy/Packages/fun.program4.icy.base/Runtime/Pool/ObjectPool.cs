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
		public int CacheCount { get { return _InPool.Count; } }


		public ObjectPool(int defaultSize = 16)
		{
			_InPool = new List<T>(defaultSize);
			_OutPool = new List<T>(defaultSize);

#if UNITY_EDITOR
			if (typeof(T) == typeof(GameObject) && this.GetType() != typeof(GameObjectPool))
				Log.LogError($"ObjectPool<GameObject> is unexpected, Use {nameof(GameObjectPool)} instead");
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
				T newIntance = InstantiateOne();
				_OutPool.Add(newIntance);
				return newIntance;
			}
		}

		public virtual void Put(T obj)
		{
			if (_OutPool.Remove(obj))
				_InPool.Add(obj);
			else
				Log.LogError("Trying to put an invalid object to ObjectPool, object = " + obj.ToString());
		}

		protected virtual T InstantiateOne()
		{
			return new T();
		}

		public virtual void Dispose()
		{
			if (_OutPool.Count > 0)
				Log.LogWarning("Dispose ObjectPool when there are objects outside, first outside object = " + _OutPool[0].ToString());
		}
	}
}
